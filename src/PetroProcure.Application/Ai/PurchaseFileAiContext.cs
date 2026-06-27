using Microsoft.Extensions.Options;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Application.Ai;

public sealed class PurchaseFileAiContextOptions
{
    public const string SectionName = "PetroProcure:AI:Context";
    public int MaxContextItems { get; set; } = 50;
    public int MaxNotes { get; set; } = 10;
    public string[] RequiredDocumentTypes { get; set; } = ["Indent", "TechnicalSpecification"];
}

public interface IPurchaseFileAiContextBuilder
{
    Task<PurchaseFileAiContextDto> BuildAsync(Guid purchaseFileId, AiAnalysisType analysisType, CancellationToken ct);
}

public interface IPurchaseFileAiContextDataSource
{
    Task<PurchaseFileAiContextSnapshot?> LoadAsync(Guid purchaseFileId, AiAnalysisType analysisType, int maxItems,
        int maxNotes, CancellationToken ct);
}

public sealed class PurchaseFileAiContextBuilder(
    IPurchaseFileAiContextDataSource dataSource,
    IOptions<PurchaseFileAiContextOptions> options) : IPurchaseFileAiContextBuilder
{
    public async Task<PurchaseFileAiContextDto> BuildAsync(Guid purchaseFileId, AiAnalysisType analysisType, CancellationToken ct)
    {
        var maxItems = Math.Max(1, options.Value.MaxContextItems);
        var maxNotes = Math.Max(0, options.Value.MaxNotes);
        var snapshot = await dataSource.LoadAsync(purchaseFileId, analysisType, maxItems, maxNotes, ct)
            ?? throw new InvalidOperationException("Purchase file was not found.");
        var requiredDocuments = options.Value.RequiredDocumentTypes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var documentTypes = snapshot.Documents
            .Select(x => x.DocumentType)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = requiredDocuments
            .Where(x => !documentTypes.Contains(x))
            .Select(x => $"Missing required document type: {x}")
            .ToArray();

        return new PurchaseFileAiContextDto(
            ContextVersion: "purchase-file-ai-context.v1",
            PurchaseFileId: snapshot.PurchaseFileId,
            AnalysisType: analysisType.ToString(),
            UserQuestion: null,
            FileNumber: snapshot.FileNumber,
            Title: snapshot.Title,
            Status: snapshot.Status,
            CurrentDepartment: snapshot.CurrentDepartment,
            Priority: snapshot.Priority,
            SourceIndentNumber: snapshot.SourceIndentNumber,
            ItemGroups: snapshot.Items
                .OrderBy(x => x.MescGeneralGroupCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.MescCode, StringComparer.OrdinalIgnoreCase)
                .GroupBy(x => new { x.MescGeneralGroupCode, x.GeneralDescription })
                .Select(g => new AiContextItemGroupDto(
                    g.Key.MescGeneralGroupCode,
                    g.Key.GeneralDescription,
                    g.Select(i => new AiContextItemDto(i.MescCode, i.SpecificDescription,
                        i.Unit, i.RequestedQuantity, i.ApprovedQuantity, i.TechnicalDescription)).ToArray()))
                .ToArray(),
            Documents: snapshot.Documents
                .OrderBy(x => x.DocumentType, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            MissingDocumentHints: missing,
            WorkflowTimeline: snapshot.WorkflowTimeline
                .OrderBy(x => x.CreatedAtUtc)
                .ThenBy(x => x.Action)
                .ToArray(),
            NotesSummary: snapshot.Notes
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(maxNotes)
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x => x.NoteText)
                .ToArray(),
            TenderInformation: snapshot.TenderInformation,
            ContractInformation: snapshot.ContractInformation,
            RuleClauses: snapshot.RuleClauses
                .OrderBy(x => x.RuleCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.ClauseCode, StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }
}

public sealed record PurchaseFileAiContextDto(
    string ContextVersion,
    Guid PurchaseFileId,
    string AnalysisType,
    string? UserQuestion,
    string FileNumber,
    string Title,
    string Status,
    string CurrentDepartment,
    string Priority,
    string? SourceIndentNumber,
    IReadOnlyList<AiContextItemGroupDto> ItemGroups,
    IReadOnlyList<AiContextDocumentDto> Documents,
    IReadOnlyList<string> MissingDocumentHints,
    IReadOnlyList<AiContextWorkflowStepDto> WorkflowTimeline,
    IReadOnlyList<string> NotesSummary,
    AiContextTenderDto? TenderInformation,
    AiContextContractDto? ContractInformation,
    IReadOnlyList<AiContextRuleClauseDto> RuleClauses);

public sealed record AiContextItemGroupDto(
    string GeneralGroupCode,
    string GeneralDescription,
    IReadOnlyList<AiContextItemDto> Items);

public sealed record AiContextItemDto(
    string MescCode,
    string SpecificDescription,
    string Unit,
    decimal RequestedQuantity,
    decimal ApprovedQuantity,
    string? TechnicalDescription);

public sealed record AiContextDocumentDto(
    Guid DocumentId,
    string DocumentType,
    string FileName,
    string Extension,
    string MimeType,
    long Size,
    int VersionNo,
    DateTime UploadedAtUtc,
    string? Description);

public sealed record AiContextWorkflowStepDto(
    string Action,
    string FromDepartment,
    string ToDepartment,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    string? Comment);

public sealed record AiContextRuleClauseDto(
    string RuleCode,
    string RuleTitle,
    string ClauseCode,
    string? ClauseText,
    string LegalReference,
    string Severity,
    string? AppliesTo);

public sealed record AiContextTenderDto(
    Guid TenderId,
    string TenderNumber,
    string Title,
    string Status,
    string TenderType,
    DateTime IssueDate,
    DateTime? SubmissionDeadline,
    int ItemCount,
    int BidCount);

public sealed record AiContextContractDto(
    Guid ContractId,
    string ContractNumber,
    string Title,
    string Status,
    string ContractType,
    string Currency,
    decimal? FinalAmount,
    DateTime? StartDate,
    DateTime? EndDate);

public sealed record PurchaseFileAiContextSnapshot(
    Guid PurchaseFileId,
    string FileNumber,
    string Title,
    string Status,
    string CurrentDepartment,
    string Priority,
    string? SourceIndentNumber,
    IReadOnlyList<PurchaseFileAiContextItemSnapshot> Items,
    IReadOnlyList<AiContextDocumentDto> Documents,
    IReadOnlyList<AiContextWorkflowStepDto> WorkflowTimeline,
    IReadOnlyList<PurchaseFileAiNoteSnapshot> Notes,
    AiContextTenderDto? TenderInformation,
    AiContextContractDto? ContractInformation,
    IReadOnlyList<AiContextRuleClauseDto> RuleClauses);

public sealed record PurchaseFileAiContextItemSnapshot(
    string MescGeneralGroupCode,
    string GeneralDescription,
    string MescCode,
    string SpecificDescription,
    string Unit,
    decimal RequestedQuantity,
    decimal ApprovedQuantity,
    string? TechnicalDescription);

public sealed record PurchaseFileAiNoteSnapshot(
    DateTime CreatedAtUtc,
    string NoteText);

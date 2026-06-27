using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Ai;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Infrastructure.Persistence;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class PurchaseFileAiContextDataSource(PetroProcureDbContext dbContext) : IPurchaseFileAiContextDataSource
{
    public async Task<PurchaseFileAiContextSnapshot?> LoadAsync(
        Guid purchaseFileId,
        AiAnalysisType analysisType,
        int maxItems,
        int maxNotes,
        CancellationToken ct)
    {
        var file = await dbContext.PurchaseFiles
            .AsNoTracking()
            .Where(x => x.Id == purchaseFileId)
            .Select(x => new
            {
                x.Id,
                x.FileNumber,
                x.Title,
                x.Status,
                x.Priority,
                x.SourceIndentId,
                x.CurrentDepartmentId
            })
            .SingleOrDefaultAsync(ct);

        if (file is null)
            return null;

        var currentDepartment = await dbContext.Departments
            .AsNoTracking()
            .Where(x => x.Id == file.CurrentDepartmentId)
            .Select(x => x.Name)
            .SingleOrDefaultAsync(ct) ?? file.CurrentDepartmentId.ToString();

        var sourceIndentNumber = file.SourceIndentId.HasValue
            ? await dbContext.Indents
                .AsNoTracking()
                .Where(x => x.Id == file.SourceIndentId.Value)
                .Select(x => x.IndentNumber)
                .SingleOrDefaultAsync(ct)
            : null;

        var items = await LoadItemsAsync(purchaseFileId, maxItems, ct);
        var documents = await LoadDocumentsAsync(purchaseFileId, ct);
        var workflow = await LoadWorkflowAsync(purchaseFileId, ct);
        var notes = await LoadNotesAsync(purchaseFileId, maxNotes, ct);
        var tender = await LoadTenderAsync(purchaseFileId, ct);
        var contract = await LoadContractAsync(purchaseFileId, ct);
        var rules = await LoadRuleClausesAsync(analysisType, ct);

        return new PurchaseFileAiContextSnapshot(
            file.Id,
            file.FileNumber,
            file.Title,
            file.Status.ToString(),
            currentDepartment,
            file.Priority.ToString(),
            sourceIndentNumber,
            items,
            documents,
            workflow,
            notes,
            tender,
            contract,
            rules);
    }

    private async Task<IReadOnlyList<PurchaseFileAiContextItemSnapshot>> LoadItemsAsync(
        Guid purchaseFileId,
        int maxItems,
        CancellationToken ct)
    {
        var rows = await dbContext.PurchaseFileItems
            .AsNoTracking()
            .Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderBy(x => x.MescGeneralGroupCode)
            .ThenBy(x => x.MescCode)
            .Take(maxItems)
            .Select(x => new
            {
                x.MescGeneralGroupCode,
                x.GeneralDescription,
                x.MescCode,
                x.SpecificDescription,
                x.UnitOfMeasureId,
                x.RequestedQuantity,
                x.ApprovedQuantity,
                x.TechnicalDescription
            })
            .ToListAsync(ct);

        var unitIds = rows.Select(x => x.UnitOfMeasureId).Distinct().ToArray();
        var units = await dbContext.UnitOfMeasures
            .AsNoTracking()
            .Where(x => unitIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => string.IsNullOrWhiteSpace(x.Code) ? x.Name : x.Code, ct);

        return rows
            .Select(x => new PurchaseFileAiContextItemSnapshot(
                x.MescGeneralGroupCode,
                x.GeneralDescription,
                x.MescCode,
                x.SpecificDescription,
                units.GetValueOrDefault(x.UnitOfMeasureId, x.UnitOfMeasureId.ToString()),
                x.RequestedQuantity,
                x.ApprovedQuantity,
                x.TechnicalDescription))
            .ToArray();
    }

    private async Task<IReadOnlyList<AiContextDocumentDto>> LoadDocumentsAsync(Guid purchaseFileId, CancellationToken ct)
    {
        var documents = await dbContext.FileDocuments
            .AsNoTracking()
            .Where(x => x.PurchaseFileId == purchaseFileId && !x.IsDeleted)
            .OrderBy(x => x.DocumentType)
            .ThenBy(x => x.OriginalFileName)
            .Select(x => new
            {
                x.Id,
                x.DocumentType,
                x.OriginalFileName,
                x.Extension,
                x.MimeType,
                x.Size,
                x.VersionNo,
                x.UploadedAt,
                x.Description
            })
            .ToListAsync(ct);

        return documents
            .Select(x => new AiContextDocumentDto(
                x.Id,
                x.DocumentType.ToString(),
                SafeFileName(x.OriginalFileName),
                x.Extension,
                x.MimeType,
                x.Size,
                x.VersionNo,
                x.UploadedAt,
                x.Description))
            .ToArray();
    }

    private async Task<IReadOnlyList<AiContextWorkflowStepDto>> LoadWorkflowAsync(Guid purchaseFileId, CancellationToken ct)
    {
        var instanceIds = await dbContext.WorkflowInstances
            .AsNoTracking()
            .Where(x => x.EntityType == "PurchaseFile" && x.EntityId == purchaseFileId)
            .Select(x => x.Id)
            .ToArrayAsync(ct);

        var steps = await dbContext.WorkflowSteps
            .AsNoTracking()
            .Where(x => instanceIds.Contains(x.WorkflowInstanceId))
            .OrderBy(x => x.CreatedAt)
            .Select(x => new
            {
                x.ActionName,
                x.FromDepartmentId,
                x.ToDepartmentId,
                x.CreatedAt,
                x.CompletedAt,
                x.Comment
            })
            .ToListAsync(ct);

        var departmentIds = steps.SelectMany(x => new[] { x.FromDepartmentId, x.ToDepartmentId }).Distinct().ToArray();
        var departments = await dbContext.Departments
            .AsNoTracking()
            .Where(x => departmentIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        return steps
            .Select(x => new AiContextWorkflowStepDto(
                x.ActionName,
                departments.GetValueOrDefault(x.FromDepartmentId, x.FromDepartmentId.ToString()),
                departments.GetValueOrDefault(x.ToDepartmentId, x.ToDepartmentId.ToString()),
                x.CreatedAt,
                x.CompletedAt,
                x.Comment))
            .ToArray();
    }

    private async Task<IReadOnlyList<PurchaseFileAiNoteSnapshot>> LoadNotesAsync(
        Guid purchaseFileId,
        int maxNotes,
        CancellationToken ct)
    {
        return await dbContext.PurchaseFileNotes
            .AsNoTracking()
            .Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(maxNotes)
            .Select(x => new PurchaseFileAiNoteSnapshot(x.CreatedAt, x.NoteText))
            .ToArrayAsync(ct);
    }

    private async Task<AiContextTenderDto?> LoadTenderAsync(Guid purchaseFileId, CancellationToken ct)
    {
        var tender = await dbContext.Tenders
            .AsNoTracking()
            .Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.TenderNumber,
                x.Title,
                x.Status,
                x.TenderType,
                x.IssueDate,
                x.SubmissionDeadline
            })
            .FirstOrDefaultAsync(ct);

        if (tender is null)
            return null;

        var itemCount = await dbContext.TenderItems.CountAsync(x => x.TenderId == tender.Id, ct);
        var bidCount = await dbContext.TenderBids.CountAsync(x => x.TenderId == tender.Id, ct);

        return new AiContextTenderDto(
            tender.Id,
            tender.TenderNumber,
            tender.Title,
            tender.Status.ToString(),
            tender.TenderType.ToString(),
            tender.IssueDate,
            tender.SubmissionDeadline,
            itemCount,
            bidCount);
    }

    private async Task<AiContextContractDto?> LoadContractAsync(Guid purchaseFileId, CancellationToken ct)
    {
        return await dbContext.PurchaseContracts
            .AsNoTracking()
            .Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AiContextContractDto(
                x.Id,
                x.ContractNumber,
                x.Title,
                x.Status.ToString(),
                x.ContractType.ToString(),
                x.Currency,
                x.FinalAmount,
                x.StartDate,
                x.EndDate))
            .FirstOrDefaultAsync(ct);
    }

    private async Task<IReadOnlyList<AiContextRuleClauseDto>> LoadRuleClausesAsync(
        AiAnalysisType analysisType,
        CancellationToken ct)
    {
        var versions = await dbContext.LegalProcurementRuleVersions
            .AsNoTracking()
            .Where(x => x.Status == RuleStatus.Active)
            .OrderBy(x => x.ProcurementRuleId)
            .ThenBy(x => x.VersionNo)
            .Select(x => new
            {
                x.ProcurementRuleId,
                x.Title,
                x.LegalClauseId,
                x.LegalReference,
                x.ConditionType,
                x.ConditionDescription,
                x.Severity
            })
            .ToListAsync(ct);

        if (versions.Count == 0)
            return [];

        var ruleIds = versions.Select(x => x.ProcurementRuleId).Distinct().ToArray();
        var rules = await dbContext.LegalProcurementRules
            .AsNoTracking()
            .Where(x => ruleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => new RuleRow(x.Code, x.Title), ct);

        var clauseIds = versions.Where(x => x.LegalClauseId.HasValue).Select(x => x.LegalClauseId!.Value).Distinct().ToArray();
        var clauses = await dbContext.LegalClauses
            .AsNoTracking()
            .Where(x => clauseIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => new ClauseRow(x.ClauseNumber, x.Body, x.AppliesTo), ct);

        return versions
            .Where(x => analysisType == AiAnalysisType.LegalCompliance || IsPurchaseFileClause(x.LegalClauseId, clauses))
            .Select(x =>
            {
                rules.TryGetValue(x.ProcurementRuleId, out var rule);
                var clause = x.LegalClauseId.HasValue && clauses.TryGetValue(x.LegalClauseId.Value, out var legalClause)
                    ? legalClause
                    : null;

                return new AiContextRuleClauseDto(
                    rule?.Code ?? x.ProcurementRuleId.ToString(),
                    string.IsNullOrWhiteSpace(x.Title) ? rule?.Title ?? string.Empty : x.Title,
                    clause?.ClauseNumber ?? x.ConditionType,
                    clause?.Body ?? x.ConditionDescription,
                    x.LegalReference,
                    x.Severity.ToString(),
                    clause?.AppliesTo);
            })
            .OrderBy(x => x.RuleCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.ClauseCode, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsPurchaseFileClause(Guid? clauseId, IReadOnlyDictionary<Guid, ClauseRow> clauses)
    {
        if (!clauseId.HasValue || !clauses.TryGetValue(clauseId.Value, out var clause))
            return true;

        return string.IsNullOrWhiteSpace(clause.AppliesTo) ||
               clause.AppliesTo.Contains("PurchaseFile", StringComparison.OrdinalIgnoreCase);
    }

    private static string SafeFileName(string fileName)
    {
        var normalized = fileName.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFileName(normalized);
    }

    private sealed record RuleRow(string Code, string Title);

    private sealed record ClauseRow(string ClauseNumber, string Body, string? AppliesTo);
}

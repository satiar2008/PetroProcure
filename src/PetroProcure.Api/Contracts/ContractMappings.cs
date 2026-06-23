using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Contracts.V1.Documents;
using PetroProcure.Contracts.V1.Indents;
using PetroProcure.Contracts.V1.Mesc;
using PetroProcure.Contracts.V1.PurchaseFiles;
using PetroProcure.Contracts.V1.Workflow;

namespace PetroProcure.Api.Contracts;

public static class ContractMappings
{
    public static MescGeneralGroupDto ToContract(this Application.Mesc.MescGeneralGroupDto dto) =>
        new(dto.Id, dto.Code, dto.GeneralDescription, dto.IsActive);
    public static MescItemDto ToContract(this Application.Mesc.MescItemDto dto) =>
        new(dto.Id, dto.Code, dto.GeneralGroupCode, dto.GeneralDescription, dto.SpecificDescription, dto.UnitOfMeasure, dto.IsActive);
    public static MescItemGroupedDto ToContract(this Application.Mesc.MescItemGroupedDto dto) =>
        new(dto.GeneralGroupCode, dto.GeneralDescription, dto.Items.Select(ToContract).ToArray());

    public static IndentItemDto ToContract(this Application.Indents.IndentItemDto dto) =>
        new(dto.Id, dto.MescItemId, dto.MescCode, dto.MescGeneralGroupCode, dto.GeneralDescription,
            dto.SpecificDescription, dto.UnitOfMeasureId, dto.RequestedQuantity, dto.TechnicalDescription, dto.RequiredDate);
    public static IndentDto ToContract(this Application.Indents.IndentDto dto) =>
        new(dto.Id, dto.IndentNumber, dto.YearPart, dto.TypeDigit, dto.Sequence, dto.IndentType, dto.Title,
            dto.RequestingDepartmentId, dto.ApplicantDepartmentId, dto.CreatedByUserId, dto.CreatedAt,
            dto.Status, dto.Description, dto.Items.Select(ToContract).ToArray());
    public static IndentSummaryDto ToContract(this Application.Indents.IndentListDto dto) =>
        new(dto.Id, dto.IndentNumber, dto.IndentType, dto.Title, dto.RequestingDepartmentId, dto.Status, dto.CreatedAt, dto.ItemCount);
    public static IndentGroupedItemsDto ToContract(this Application.Indents.IndentItemsGroupedDto dto) =>
        new(dto.MescGeneralGroupCode, dto.GeneralDescription, dto.Items.Select(ToContract).ToArray());

    public static PurchaseFileItemDto ToContract(this Application.PurchaseFiles.PurchaseFileItemDto dto) =>
        new(dto.Id, dto.MescItemId, dto.MescCode, dto.MescGeneralGroupCode, dto.GeneralDescription,
            dto.SpecificDescription, dto.UnitOfMeasureId, dto.RequestedQuantity, dto.ApprovedQuantity,
            dto.TechnicalDescription, dto.SourceIndentItemId);
    public static PurchaseFileStatusHistoryDto ToContract(this Application.PurchaseFiles.PurchaseFileStatusHistoryDto dto) =>
        new(dto.Id, dto.FromStatus, dto.ToStatus, dto.ChangedByUserId, dto.ChangedAt, dto.Reason, dto.DepartmentId);
    public static PurchaseFileNoteDto ToContract(this Application.PurchaseFiles.PurchaseFileNoteDto dto) =>
        new(dto.Id, dto.DepartmentId, dto.UserId, dto.NoteText, dto.CreatedAt, dto.IsInternal);
    public static PurchaseFileDto ToContract(this Application.PurchaseFiles.PurchaseFileDto dto) =>
        new(dto.Id, dto.FileNumber, dto.Title, dto.Description, dto.Status, dto.Priority, dto.SourceIndentId,
            dto.PurchaseDepartmentId, dto.CurrentDepartmentId, dto.ResponsibleUserId, dto.CreatedByUserId,
            dto.CreatedAt, dto.CompletedAt, dto.ArchivedAt, dto.Items.Select(ToContract).ToArray(),
            dto.StatusHistory.Select(ToContract).ToArray(), dto.Notes.Select(ToContract).ToArray());
    public static PurchaseFileSummaryDto ToContract(this Application.PurchaseFiles.PurchaseFileListDto dto) =>
        new(dto.Id, dto.FileNumber, dto.Title, dto.Status, dto.Priority, dto.CurrentDepartmentId,
            dto.ResponsibleUserId, dto.CreatedAt, dto.ItemCount, dto.SourceIndentNumber);
    public static PurchaseFileGroupedItemsDto ToContract(this Application.PurchaseFiles.PurchaseFileItemsGroupedDto dto) =>
        new(dto.MescGeneralGroupCode, dto.GeneralDescription, dto.Items.Select(ToContract).ToArray());
    public static PurchaseFileTimelineDto ToContract(this Application.PurchaseFiles.PurchaseFileTimelineDto dto) =>
        new(dto.StatusChanges.Select(ToContract).ToArray(), dto.Notes.Select(ToContract).ToArray());

    public static FileDocumentDto ToContract(this Application.Documents.FileDocumentDto dto) =>
        new(dto.Id, dto.PurchaseFileId, dto.DepartmentId, dto.DocumentType, dto.OriginalFileName,
            dto.StoredFileName, dto.RelativePath, dto.Extension, dto.MimeType, dto.Size, dto.Hash,
            dto.VersionNo, dto.UploadedByUserId, dto.UploadedAt, dto.IsDeleted, dto.Description);

    public static InboxTaskDto ToContract(this Application.Workflow.InboxTaskDto dto) =>
        new(dto.Id, dto.WorkflowInstanceId, dto.PurchaseFileId, dto.IndentId, dto.AssignedDepartmentId,
            dto.AssignedUserId, dto.Title, dto.Description, dto.Status, dto.DueDate, dto.CreatedAt, dto.CompletedAt);
    public static WorkflowStepDto ToContract(this Application.Workflow.WorkflowStepDto dto) =>
        new(dto.Id, dto.FromDepartmentId, dto.ToDepartmentId, dto.ActionName, dto.Comment,
            dto.CreatedByUserId, dto.CreatedAt, dto.CompletedByUserId, dto.CompletedAt);

    public static AiEvaluationResultDto ToContract(this PetroProcure.AI.AiEvaluationDto dto) =>
        new(dto.Id, dto.PurchaseFileId, dto.EvaluationType, dto.Summary, dto.CreatedAt,
            dto.Findings.Select(f => new AiFindingDto(f.Id, f.Title, f.Description, (AiSeverity)(int)f.Severity, f.Code)).ToArray(),
            dto.Recommendations.Select(r => new AiRecommendationDto(r.Id, r.Title, r.Description, (AiSeverity)(int)r.Severity)).ToArray());
}

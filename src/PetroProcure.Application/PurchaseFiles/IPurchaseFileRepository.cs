using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.Domain.Enums;
using PetroProcure.Contracts.V1.Common;
using PurchaseFileListRequest = PetroProcure.Contracts.V1.PurchaseFiles.PurchaseFileListRequest;

namespace PetroProcure.Application.PurchaseFiles;

public interface IPurchaseFileRepository
{
    Task<string> GenerateNextFileNumberAsync(int year, CancellationToken cancellationToken);
    Task<bool> FileNumberExistsAsync(string fileNumber, CancellationToken cancellationToken);
    Task<bool> SourceIndentAlreadyUsedAsync(Guid indentId, CancellationToken cancellationToken);
    Task<PurchaseFile?> FindAsync(Guid id, bool includeDetails, CancellationToken cancellationToken);
    Task<PurchaseFile?> FindByNumberAsync(string fileNumber, CancellationToken cancellationToken);
    Task<PurchaseFile?> FindBySourceIndentAsync(Guid indentId, bool includeDetails, CancellationToken cancellationToken);
    Task<Indent?> FindApprovedIndentAsync(Guid id, CancellationToken cancellationToken);
    Task<string?> GetDepartmentNameAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid?> GetDepartmentIdByTypeAsync(DepartmentType type, CancellationToken cancellationToken);
    Task<WorkflowInstance?> FindPurchaseFileWorkflowAsync(Guid purchaseFileId, CancellationToken cancellationToken);
    Task AddWorkflowAsync(WorkflowInstance workflow, CancellationToken cancellationToken);
    Task AddInboxTaskAsync(InboxTask task, CancellationToken cancellationToken);
    Task<InboxTask?> FindInboxTaskAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> HasActiveTechnicalReviewAsync(Guid purchaseFileId, Guid departmentId, CancellationToken cancellationToken);
    Task AddTechnicalReviewAsync(PurchaseFileTechnicalReview review, CancellationToken cancellationToken);
    Task<PurchaseFileTechnicalReview?> FindTechnicalReviewAsync(Guid id, bool includeFile, CancellationToken cancellationToken);
    Task<IReadOnlyList<PurchaseFileTechnicalReviewDto>> GetTechnicalReviewsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PurchaseFileTechnicalReviewDto>> GetTechnicalReviewsByDepartmentsAsync(IReadOnlyCollection<Guid> departmentIds, CancellationToken cancellationToken);
    Task<PurchaseFileTechnicalReviewDto?> GetTechnicalReviewDtoAsync(Guid id, CancellationToken cancellationToken);
    Task<ApplicantDashboardDto> GetApplicantDashboardAsync(IReadOnlyCollection<Guid> departmentIds, CancellationToken cancellationToken);
    Task<DepartmentDashboardDto> GetDepartmentDashboardAsync(string departmentKey, IReadOnlyCollection<Guid> departmentIds, CancellationToken cancellationToken);
    Task<PurchaseFileMescSnapshot?> GetMescSnapshotAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> UnitOfMeasureExistsAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(PurchaseFile purchaseFile, CancellationToken cancellationToken);
    Task<IReadOnlyList<PurchaseFileListDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<PagedResult<PurchaseFileListDto>> GetPagedAsync(PurchaseFileListRequest request, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

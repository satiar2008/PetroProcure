using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.PurchaseFiles;

namespace PetroProcure.Application.PurchaseFiles;

public interface IPurchaseFileRepository
{
    Task<string> GenerateNextFileNumberAsync(int year, CancellationToken cancellationToken);
    Task<bool> FileNumberExistsAsync(string fileNumber, CancellationToken cancellationToken);
    Task<bool> SourceIndentAlreadyUsedAsync(Guid indentId, CancellationToken cancellationToken);
    Task<PurchaseFile?> FindAsync(Guid id, bool includeDetails, CancellationToken cancellationToken);
    Task<PurchaseFile?> FindByNumberAsync(string fileNumber, CancellationToken cancellationToken);
    Task<Indent?> FindApprovedIndentAsync(Guid id, CancellationToken cancellationToken);
    Task<PurchaseFileMescSnapshot?> GetMescSnapshotAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> UnitOfMeasureExistsAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(PurchaseFile purchaseFile, CancellationToken cancellationToken);
    Task<IReadOnlyList<PurchaseFileListDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<PagedResult<PurchaseFileListDto>> GetPagedAsync(PurchaseFileListRequest request, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

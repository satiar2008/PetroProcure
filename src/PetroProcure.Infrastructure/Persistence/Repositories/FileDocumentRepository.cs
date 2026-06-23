using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Documents;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.PurchaseFiles;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class FileDocumentRepository(PetroProcureDbContext dbContext) : IFileDocumentRepository
{
    public Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.PurchaseFiles.AsNoTracking().SingleOrDefaultAsync(file => file.Id == id, cancellationToken);

    public Task<FileDocument?> FindDocumentAsync(Guid id, bool includeVersions, CancellationToken cancellationToken)
    {
        IQueryable<FileDocument> query = dbContext.FileDocuments;
        if (includeVersions) query = query.Include(document => document.Versions);
        return query.SingleOrDefaultAsync(document => document.Id == id, cancellationToken);
    }

    public async Task AddDocumentAsync(FileDocument document, CancellationToken cancellationToken) =>
        await dbContext.FileDocuments.AddAsync(document, cancellationToken);

    public async Task<IReadOnlyList<FileDocumentDto>> GetDocumentsAsync(
        Guid purchaseFileId, bool includeDeleted, CancellationToken cancellationToken) =>
        await dbContext.FileDocuments.AsNoTracking()
            .Where(document => document.PurchaseFileId == purchaseFileId && (includeDeleted || !document.IsDeleted))
            .OrderBy(document => document.DocumentType).ThenBy(document => document.OriginalFileName)
            .Select(document => new FileDocumentDto(
                document.Id, document.PurchaseFileId, document.DepartmentId, document.DocumentType,
                document.OriginalFileName, document.StoredFileName, document.RelativePath, document.Extension,
                document.MimeType, document.Size, document.Hash, document.VersionNo, document.UploadedByUserId,
                document.UploadedAt, document.IsDeleted, document.Description))
            .ToListAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}

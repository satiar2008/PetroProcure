using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Documents;
using PetroProcure.Application.Rag;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class CorpusSourceTextProvider(
    PetroProcureDbContext db,
    IFileStorageService fileStorage,
    ITextExtractionService textExtraction) : ICorpusSourceTextProvider
{
    public Task<CorpusSourceText?> GetTextAsync(AiDocumentSourceType sourceType, Guid sourceId, CancellationToken ct = default) =>
        sourceType switch
        {
            AiDocumentSourceType.LegalClause => GetLegalClauseTextAsync(sourceId, ct),
            AiDocumentSourceType.PurchaseFileDocument => GetFileDocumentTextAsync(sourceId, sourceType, ct),
            AiDocumentSourceType.ReportDocument => GetFileDocumentTextAsync(sourceId, sourceType, ct),
            AiDocumentSourceType.TenderDocument => GetTenderDocumentTextAsync(sourceId, ct),
            AiDocumentSourceType.ContractDocument => GetContractDocumentTextAsync(sourceId, ct),
            _ => Task.FromResult<CorpusSourceText?>(null)
        };

    private async Task<CorpusSourceText?> GetLegalClauseTextAsync(Guid clauseId, CancellationToken ct)
    {
        var clause = await db.LegalClauses.AsNoTracking()
            .Where(x => x.Id == clauseId)
            .Select(x => new { x.Id, x.ClauseNumber, x.Body, x.SearchText, x.AppliesTo, x.Tags })
            .SingleOrDefaultAsync(ct);

        if (clause is null) return null;

        return new CorpusSourceText(
            string.IsNullOrWhiteSpace(clause.SearchText) ? clause.Body : clause.SearchText,
            new ChunkMetadata(
                LegalClauseId: clause.Id,
                Values: new Dictionary<string, object?>
                {
                    ["clauseNumber"] = clause.ClauseNumber,
                    ["appliesTo"] = clause.AppliesTo,
                    ["tags"] = clause.Tags
                }));
    }

    private async Task<CorpusSourceText?> GetFileDocumentTextAsync(Guid documentId, AiDocumentSourceType sourceType,
        CancellationToken ct)
    {
        var document = await db.FileDocuments.AsNoTracking()
            .Where(x => x.Id == documentId && !x.IsDeleted)
            .Select(x => new { x.Id, x.PurchaseFileId, x.DocumentType, x.OriginalFileName, x.MimeType, x.Description })
            .SingleOrDefaultAsync(ct);

        if (document is null) return null;

        var content = await fileStorage.OpenFileAsync(documentId, ct);
        await using var stream = content.Stream;
        var text = await textExtraction.ExtractTextAsync(stream, content.OriginalFileName, content.MimeType, ct);
        return new CorpusSourceText(text, new ChunkMetadata(
            PurchaseFileId: document.PurchaseFileId,
            DocumentId: document.Id,
            Values: new Dictionary<string, object?>
            {
                ["sourceType"] = sourceType.ToString(),
                ["documentType"] = document.DocumentType.ToString(),
                ["originalFileName"] = document.OriginalFileName,
                ["description"] = document.Description
            }));
    }

    private async Task<CorpusSourceText?> GetTenderDocumentTextAsync(Guid tenderDocumentId, CancellationToken ct)
    {
        var tenderDocument = await db.TenderDocuments.AsNoTracking()
            .Where(x => x.Id == tenderDocumentId)
            .Select(x => new { x.Id, x.TenderId, x.FileDocumentId, x.DocumentType, x.OriginalFileName, x.Description })
            .SingleOrDefaultAsync(ct);

        if (tenderDocument?.FileDocumentId is null) return null;
        return await GetFileDocumentTextAsync(tenderDocument.FileDocumentId.Value, AiDocumentSourceType.TenderDocument, ct);
    }

    private async Task<CorpusSourceText?> GetContractDocumentTextAsync(Guid contractDocumentId, CancellationToken ct)
    {
        var contractDocument = await db.ContractDocuments.AsNoTracking()
            .Where(x => x.Id == contractDocumentId)
            .Select(x => new { x.Id, x.ContractId, x.FileDocumentId, x.DocumentType, x.OriginalFileName, x.Description })
            .SingleOrDefaultAsync(ct);

        if (contractDocument?.FileDocumentId is null) return null;
        return await GetFileDocumentTextAsync(contractDocument.FileDocumentId.Value, AiDocumentSourceType.ContractDocument, ct);
    }
}

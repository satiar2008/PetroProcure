using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Rag;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class AiDocumentChunkRepository(PetroProcureDbContext db) : IAiDocumentChunkRepository
{
    public async Task<IReadOnlyList<AiDocumentChunk>> GetChunksAsync(AiDocumentSourceType sourceType, Guid sourceId,
        bool includeDeleted, CancellationToken ct)
    {
        var query = db.AiDocumentChunks
            .Where(x => x.SourceType == sourceType && x.SourceId == sourceId);

        if (!includeDeleted)
            query = query.Where(x => !x.IsDeleted);

        return await query.OrderBy(x => x.Ordinal).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AiDocumentChunk>> GetActiveChunksAsync(CancellationToken ct) =>
        await db.AiDocumentChunks
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.SourceType)
            .ThenBy(x => x.SourceId)
            .ThenBy(x => x.Ordinal)
            .ToListAsync(ct);

    public async Task AddAsync(AiDocumentChunk chunk, CancellationToken ct) =>
        await db.AiDocumentChunks.AddAsync(chunk, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

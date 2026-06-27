using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Ai;

// AI-RAG-04: a vector embedding for a chunk, produced by a pinned model.
// VectorJson keeps the implementation portable (brute-force cosine now; SQL Server Vector / Qdrant later).
public sealed class AiEmbedding : Entity<Guid>
{
    public AiEmbedding(Guid id, Guid chunkId, string model, string vectorJson, int dimensions,
        string contentHash) : base(id)
    {
        ChunkId = chunkId == Guid.Empty ? throw new ArgumentException("Chunk is required.", nameof(chunkId)) : chunkId;
        Model = Required(model, nameof(model));
        VectorJson = Required(vectorJson, nameof(vectorJson));
        Dimensions = dimensions <= 0 ? throw new ArgumentOutOfRangeException(nameof(dimensions)) : dimensions;
        ContentHash = Required(contentHash, nameof(contentHash));
        CreatedAt = DateTime.UtcNow;
    }

    private AiEmbedding(Guid id) : base(id)
    {
        Model = string.Empty;
        VectorJson = string.Empty;
        ContentHash = string.Empty;
    }

    public Guid ChunkId { get; private set; }
    public string Model { get; private set; }
    public string VectorJson { get; private set; }
    public int Dimensions { get; private set; }
    public string ContentHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void Update(string model, string vectorJson, int dimensions, string contentHash)
    {
        Model = Required(model, nameof(model));
        VectorJson = Required(vectorJson, nameof(vectorJson));
        Dimensions = dimensions <= 0 ? throw new ArgumentOutOfRangeException(nameof(dimensions)) : dimensions;
        ContentHash = Required(contentHash, nameof(contentHash));
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}

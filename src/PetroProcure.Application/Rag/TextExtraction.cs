namespace PetroProcure.Application.Rag;

public interface ITextExtractionService
{
    Task<string> ExtractTextAsync(Stream stream, string fileName, string mimeType, CancellationToken ct = default);
}

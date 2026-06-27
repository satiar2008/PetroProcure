using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PetroProcure.Application.Rag;

namespace PetroProcure.Infrastructure.Storage;

internal sealed partial class TextExtractionService : ITextExtractionService
{
    public async Task<string> ExtractTextAsync(Stream stream, string fileName, string mimeType, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".txt" => await ReadTextAsync(stream, ct),
            ".html" or ".htm" => StripHtml(await ReadTextAsync(stream, ct)),
            ".docx" => ExtractDocxText(stream),
            ".pdf" => string.Empty,
            _ when mimeType.Equals("text/plain", StringComparison.OrdinalIgnoreCase) => await ReadTextAsync(stream, ct),
            _ when mimeType.Equals("text/html", StringComparison.OrdinalIgnoreCase) => StripHtml(await ReadTextAsync(stream, ct)),
            _ => string.Empty
        };
    }

    private static async Task<string> ReadTextAsync(Stream stream, CancellationToken ct)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await reader.ReadToEndAsync(ct);
    }

    private static string ExtractDocxText(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var entry = archive.GetEntry("word/document.xml");
        if (entry is null) return string.Empty;

        using var entryStream = entry.Open();
        var document = XDocument.Load(entryStream);
        XNamespace word = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        return string.Join(" ", document.Descendants(word + "t").Select(x => x.Value)).Trim();
    }

    private static string StripHtml(string html)
    {
        var withoutTags = HtmlTagRegex().Replace(html, " ");
        return WebUtility.HtmlDecode(WhitespaceRegex().Replace(withoutTags, " ")).Trim();
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("\\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}

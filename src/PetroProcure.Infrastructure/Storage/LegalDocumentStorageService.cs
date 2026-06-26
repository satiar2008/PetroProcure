using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using PetroProcure.Application.Documents;
using PetroProcure.Application.Legal;
using PetroProcure.Domain.Modules.Legal;

namespace PetroProcure.Infrastructure.Storage;

public sealed class LegalDocumentStorageService(
    IOptions<FileStorageOptions> options,
    IFileScanner scanner) : ILegalDocumentStorageService
{
    private readonly FileStorageOptions _options = options.Value;
    private readonly string _rootPath = ResolveRootPath(options.Value.RootPath);
    private readonly string _quarantinePath = ResolveQuarantinePath(options.Value);

    public async Task<StoredLegalDocument> SaveAsync(Guid legalDocumentId, string originalFileName, Stream stream,
        string? mimeType, CancellationToken ct = default)
    {
        var safeOriginalName = ValidateAndSanitizeName(originalFileName);
        var extension = Path.GetExtension(safeOriginalName).ToLowerInvariant();
        ValidateMetadata(stream, extension, mimeType);
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine("Legal", "Documents", DateTime.UtcNow.Year.ToString(),
            legalDocumentId.ToString("N"), storedName).Replace('\\', '/');
        var physicalPath = ToPhysicalPath(relativePath);
        var quarantineFile = Path.Combine(_quarantinePath, $"{Guid.NewGuid():N}{extension}");
        string? createdPhysicalPath = null;
        var (size, hash) = await WriteAndHashAsync(stream, quarantineFile, MaxBytes, ct);
        try
        {
            await EnsureCleanAsync(quarantineFile, ct);
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
            File.Move(quarantineFile, physicalPath);
            createdPhysicalPath = physicalPath;
            return new StoredLegalDocument(safeOriginalName, storedName, relativePath, extension,
                string.IsNullOrWhiteSpace(mimeType) ? GetMimeType(extension) : mimeType, size, hash);
        }
        catch
        {
            TryDelete(quarantineFile);
            if (createdPhysicalPath is not null) TryDelete(createdPhysicalPath);
            throw;
        }
    }

    public Task<StoredFileContent> OpenAsync(LegalDocument document, CancellationToken ct = default)
    {
        if (document.IsDeleted) throw new FileStorageNotFoundException("Legal document was deleted.");
        if (string.IsNullOrWhiteSpace(document.RelativePath))
            throw new FileStorageNotFoundException("Legal document does not have a stored file.");
        var path = ToPhysicalPath(document.RelativePath);
        if (!File.Exists(path)) throw new FileStorageNotFoundException("Physical legal document was not found.");
        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        return Task.FromResult(new StoredFileContent(stream, document.MimeType, document.OriginalFileName));
    }

    public Task DeletePhysicalAsync(string relativePath, CancellationToken ct = default)
    {
        TryDelete(ToPhysicalPath(relativePath));
        return Task.CompletedTask;
    }

    private long MaxBytes => Math.Max(1, _options.MaxFileSizeMb) * 1024L * 1024L;

    private string ValidateAndSanitizeName(string originalFileName)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new FileStorageValidationException("Original file name is required.");
        if (originalFileName.Contains("..", StringComparison.Ordinal)
            || originalFileName.Contains('/') || originalFileName.Contains('\\')
            || Path.GetFileName(originalFileName) != originalFileName)
            throw new FileStorageValidationException("File name contains an invalid path.");
        var sanitized = string.Concat(originalFileName.Where(character => !Path.GetInvalidFileNameChars().Contains(character))).Trim();
        if (string.IsNullOrWhiteSpace(sanitized))
            throw new FileStorageValidationException("File name is invalid.");
        return sanitized.Length <= 255 ? sanitized : sanitized[..255];
    }

    private void ValidateMetadata(Stream stream, string extension, string? mimeType)
    {
        if (stream.CanSeek && stream.Length == 0)
            throw new FileStorageValidationException("Empty files are not allowed.");
        if (stream.CanSeek && stream.Length > MaxBytes)
            throw new FileStorageValidationException("File exceeds the configured maximum size.");
        if (!_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new FileStorageValidationException($"Extension '{extension}' is not allowed.");
        var effectiveMime = string.IsNullOrWhiteSpace(mimeType) ? GetMimeType(extension) : mimeType;
        if (!_options.AllowedMimeTypes.Contains(effectiveMime, StringComparer.OrdinalIgnoreCase))
            throw new FileStorageValidationException($"MIME type '{effectiveMime}' is not allowed.");
    }

    private async Task EnsureCleanAsync(string path, CancellationToken ct)
    {
        if (!_options.EnableAntivirusScan) return;
        var result = await scanner.ScanAsync(path, ct);
        if (result.Status != FileScanStatus.Clean)
            throw new FileStorageValidationException($"File scan did not pass: {result.Status}. {result.Details}".Trim());
    }

    private string ToPhysicalPath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath) || relativePath.Contains("..", StringComparison.Ordinal))
            throw new FileStorageValidationException("Relative path is invalid.");
        var full = Path.GetFullPath(Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new FileStorageValidationException("Resolved path escapes the configured root.");
        return full;
    }

    private static async Task<(long Size, string Hash)> WriteAndHashAsync(Stream source, string path, long maxBytes, CancellationToken ct)
    {
        if (source.CanSeek) source.Position = 0;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var target = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        long size = 0;
        int read;
        while ((read = await source.ReadAsync(buffer, ct)) > 0)
        {
            await target.WriteAsync(buffer.AsMemory(0, read), ct);
            hash.AppendData(buffer, 0, read);
            size += read;
            if (size > maxBytes) throw new FileStorageValidationException("File exceeds the configured maximum size.");
        }
        if (size == 0) throw new FileStorageValidationException("Empty files are not allowed.");
        if (source.CanSeek) source.Position = 0;
        return (size, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    private static string GetMimeType(string extension) => extension switch
    {
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xls" => "application/vnd.ms-excel",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".txt" => "text/plain",
        _ => "application/octet-stream"
    };

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static string ResolveRootPath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            throw new InvalidOperationException("PetroProcure:FileStorage:RootPath is required.");
        return Path.GetFullPath(configuredPath.Trim()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string ResolveQuarantinePath(FileStorageOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.QuarantinePath))
            return Path.GetFullPath(options.QuarantinePath.Trim());
        return Path.Combine(Path.GetFullPath(options.RootPath.Trim()), "_quarantine");
    }
}

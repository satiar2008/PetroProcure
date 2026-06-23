using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using PetroProcure.Application.Documents;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.PurchaseFiles;

namespace PetroProcure.Infrastructure.Storage;

public sealed class FileStorageService(
    IFileDocumentRepository repository,
    IOptions<FileStorageOptions> options,
    IFileScanner scanner) : IFileStorageService
{
    public static readonly string[] StandardFolders =
    [
        "01-Indent", "02-Technical", "03-Suppliers", "04-Inquiry", "05-Tender",
        "06-Commission", "07-Contract", "08-PurchaseOrder", "09-Warehouse", "10-Final"
    ];

    private readonly string _rootPath = ResolveRootPath(options.Value.RootPath);
    private readonly FileStorageOptions _options = options.Value;
    private readonly string _quarantinePath = ResolveQuarantinePath(options.Value);

    public async Task EnsurePurchaseFileFoldersAsync(PurchaseFile purchaseFile, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var basePath = GetPurchaseFileDirectory(purchaseFile);
        foreach (var folder in StandardFolders)
            Directory.CreateDirectory(Path.Combine(basePath, folder));
        await Task.CompletedTask;
    }

    public async Task<FileDocumentDto> SaveFileAsync(
        Guid purchaseFileId, DocumentType documentType, string originalFileName, Stream stream,
        Guid uploadedByUserId, Guid? departmentId = null, string? mimeType = null,
        string? description = null, CancellationToken cancellationToken = default)
    {
        var purchaseFile = await repository.FindPurchaseFileAsync(purchaseFileId, cancellationToken)
            ?? throw new FileStorageNotFoundException("Purchase file was not found.");
        await EnsurePurchaseFileFoldersAsync(purchaseFile, cancellationToken);
        var safeOriginalName = ValidateAndSanitizeName(originalFileName);
        var extension = Path.GetExtension(safeOriginalName).ToLowerInvariant();
        ValidateMetadata(stream, extension, mimeType);
        var storedName = $"{Guid.NewGuid():N}-v1{extension}";
        var relativePath = GetRelativePath(purchaseFile, documentType, storedName);
        var physicalPath = ToPhysicalPath(relativePath);
        var quarantineFile = Path.Combine(_quarantinePath, $"{Guid.NewGuid():N}{extension}");
        string? createdPhysicalPath = null;
        var (size, hash) = await WriteAndHashAsync(stream, quarantineFile, MaxBytes, cancellationToken);
        try
        {
            await EnsureCleanAsync(quarantineFile, cancellationToken);
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
            File.Move(quarantineFile, physicalPath);
            createdPhysicalPath = physicalPath;
        var document = new FileDocument(
            Guid.NewGuid(), purchaseFileId, departmentId, documentType, safeOriginalName, storedName,
            relativePath, extension, mimeType ?? GetMimeType(extension), size, hash, 1,
            uploadedByUserId, description);
        await repository.AddDocumentAsync(document, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(document);
        }
        catch
        {
            TryDelete(quarantineFile);
            if (createdPhysicalPath is not null) TryDelete(createdPhysicalPath);
            throw;
        }
    }

    public async Task<StoredFileContent> OpenFileAsync(Guid fileDocumentId, CancellationToken cancellationToken = default)
    {
        var document = await RequiredDocument(fileDocumentId, false, cancellationToken);
        if (document.IsDeleted) throw new FileStorageNotFoundException("Document was deleted.");
        var path = ToPhysicalPath(document.RelativePath);
        if (!File.Exists(path)) throw new FileStorageNotFoundException("Physical file was not found.");
        return new StoredFileContent(
            new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true),
            document.MimeType, document.OriginalFileName);
    }

    public async Task DeleteFileAsync(Guid fileDocumentId, CancellationToken cancellationToken = default)
    {
        var document = await RequiredDocument(fileDocumentId, false, cancellationToken);
        document.SoftDelete();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<FileDocumentDto> CreateNewVersionAsync(
        Guid fileDocumentId, Stream stream, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var document = await RequiredDocument(fileDocumentId, true, cancellationToken);
        if (document.IsDeleted) throw new FileStorageValidationException("Deleted documents cannot be versioned.");
        ValidateMetadata(stream, document.Extension, document.MimeType);
        var nextVersion = document.VersionNo + 1;
        var storedName = $"{Guid.NewGuid():N}-v{nextVersion}{document.Extension}";
        var relativePath = ReplaceFileName(document.RelativePath, storedName);
        var physicalPath = ToPhysicalPath(relativePath);
        var quarantineFile = Path.Combine(_quarantinePath, $"{Guid.NewGuid():N}{document.Extension}");
        string? createdPhysicalPath = null;
        var (size, hash) = await WriteAndHashAsync(stream, quarantineFile, MaxBytes, cancellationToken);
        try
        {
            await EnsureCleanAsync(quarantineFile, cancellationToken);
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
            File.Move(quarantineFile, physicalPath);
            createdPhysicalPath = physicalPath;
        document.AddVersion(new DocumentVersion(
            Guid.NewGuid(), document.Id, nextVersion, storedName, relativePath, size, hash, createdByUserId));
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(document);
        }
        catch
        {
            TryDelete(quarantineFile);
            if (createdPhysicalPath is not null) TryDelete(createdPhysicalPath);
            throw;
        }
    }

    public string GetRelativePath(PurchaseFile purchaseFile, DocumentType documentType, string storedFileName)
    {
        var year = GetFileYear(purchaseFile.FileNumber);
        var folder = GetDocumentFolder(documentType);
        return Path.Combine("PurchaseFiles", year.ToString(), purchaseFile.FileNumber, folder, storedFileName)
            .Replace('\\', '/');
    }

    public Task<IReadOnlyList<FileDocumentDto>> GetPurchaseFileDocumentsAsync(
        Guid purchaseFileId, bool includeDeleted = false, CancellationToken cancellationToken = default) =>
        repository.GetDocumentsAsync(purchaseFileId, includeDeleted, cancellationToken);

    private async Task<FileDocument> RequiredDocument(Guid id, bool versions, CancellationToken cancellationToken) =>
        await repository.FindDocumentAsync(id, versions, cancellationToken)
        ?? throw new FileStorageNotFoundException("Document was not found.");

    private string GetPurchaseFileDirectory(PurchaseFile purchaseFile)
    {
        var year = GetFileYear(purchaseFile.FileNumber);
        return Path.Combine(_rootPath, "PurchaseFiles", year.ToString(), purchaseFile.FileNumber);
    }

    private string ToPhysicalPath(string relativePath)
    {
        var safe = FileDocument.ValidateRelativePath(relativePath);
        var full = Path.GetFullPath(Path.Combine(_rootPath, safe.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new FileStorageValidationException("Resolved path escapes the configured root.");
        return full;
    }

    private static async Task<(long Size, string Hash)> WriteAndHashAsync(
        Stream source, string path, long maxBytes, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var target = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        long size = 0;
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            hash.AppendData(buffer, 0, read);
            size += read;
            if (size > maxBytes)
                throw new FileStorageValidationException("File exceeds the configured maximum size.");
        }
        if (size == 0) throw new FileStorageValidationException("Empty files are not allowed.");
        return (size, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
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
        var sanitized = string.Concat(originalFileName
            .Where(character => !Path.GetInvalidFileNameChars().Contains(character)))
            .Trim();
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
    private async Task EnsureCleanAsync(string path, CancellationToken cancellationToken)
    {
        if (!_options.EnableAntivirusScan) return;
        var result = await scanner.ScanAsync(path, cancellationToken);
        if (result.Status != FileScanStatus.Clean)
            throw new FileStorageValidationException($"File scan did not pass: {result.Status}. {result.Details}".Trim());
    }
    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static string ReplaceFileName(string relativePath, string storedName) =>
        $"{relativePath[..(relativePath.LastIndexOf('/') + 1)]}{storedName}";

    private static int GetFileYear(string fileNumber)
    {
        if (fileNumber.Length < 7 || !int.TryParse(fileNumber.AsSpan(3, 4), out var year))
            throw new FileStorageValidationException("Purchase file number does not contain a valid year.");
        return year;
    }

    private static string GetDocumentFolder(DocumentType type) => type switch
    {
        DocumentType.Indent => "01-Indent",
        DocumentType.TechnicalSpecification => "02-Technical",
        DocumentType.CommercialOffer => "03-Suppliers",
        DocumentType.Correspondence => "04-Inquiry",
        DocumentType.TenderDocument => "05-Tender",
        DocumentType.TenderCommissionMinutes => "06-Commission",
        DocumentType.Contract => "07-Contract",
        DocumentType.PurchaseOrder => "08-PurchaseOrder",
        DocumentType.WarehouseReceipt => "09-Warehouse",
        DocumentType.FinalReport => "10-Final",
        _ => "10-Final"
    };

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

    private static string ResolveRootPath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            throw new InvalidOperationException("PetroProcure:FileStorage:RootPath is required.");
        return Path.GetFullPath(configuredPath.Trim()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
    private static string ResolveQuarantinePath(FileStorageOptions options)
    {
        var configured = string.IsNullOrWhiteSpace(options.QuarantinePath)
            ? Path.Combine(options.RootPath, ".quarantine")
            : options.QuarantinePath;
        var path = Path.GetFullPath(configured);
        Directory.CreateDirectory(path);
        return path;
    }

    private static FileDocumentDto ToDto(FileDocument document) => new(
        document.Id, document.PurchaseFileId, document.DepartmentId, document.DocumentType,
        document.OriginalFileName, document.StoredFileName, document.RelativePath, document.Extension,
        document.MimeType, document.Size, document.Hash, document.VersionNo, document.UploadedByUserId,
        document.UploadedAt, document.IsDeleted, document.Description);
}

public sealed class FileStorageNotFoundException(string message) : Exception(message);
public sealed class FileStorageValidationException(string message) : Exception(message);

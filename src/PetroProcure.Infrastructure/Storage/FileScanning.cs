using PetroProcure.Application.Documents;

namespace PetroProcure.Infrastructure.Storage;

public sealed class NoOpFileScanner : IFileScanner
{
    public Task<FileScanResult> ScanAsync(string filePath, CancellationToken cancellationToken = default) =>
        Task.FromResult(new FileScanResult(FileScanStatus.Clean));
}

public sealed class OrphanFileCleanupService : IOrphanFileCleanupService
{
    public Task<int> CleanupAsync(CancellationToken cancellationToken = default)
    {
        // Placeholder: a future implementation will reconcile physical files with document metadata.
        return Task.FromResult(0);
    }
}

namespace PetroProcure.Application.PurchaseFiles;

public interface IPurchaseFileNumberService
{
    Task<string> GenerateNextFileNumber(int year, CancellationToken cancellationToken = default);
}

public sealed class PurchaseFileNumberService(IPurchaseFileRepository repository) : IPurchaseFileNumberService
{
    public Task<string> GenerateNextFileNumber(int year, CancellationToken cancellationToken = default)
    {
        if (year is < 2000 or > 9999) throw new ArgumentOutOfRangeException(nameof(year));
        return repository.GenerateNextFileNumberAsync(year, cancellationToken);
    }
}

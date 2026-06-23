namespace PetroProcure.Contracts.V1.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items, int PageNumber, int PageSize, long TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

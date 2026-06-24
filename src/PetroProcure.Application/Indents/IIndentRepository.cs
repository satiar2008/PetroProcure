using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Application.Indents;

public interface IIndentRepository
{
    Task<string> GenerateNextIndentNumberAsync(int yearPart, int typeDigit, CancellationToken cancellationToken);
    Task<bool> IndentNumberExistsAsync(string indentNumber, CancellationToken cancellationToken);
    Task<Indent?> FindAsync(Guid id, bool includeItems, CancellationToken cancellationToken);
    Task<Indent?> FindByNumberAsync(string indentNumber, CancellationToken cancellationToken);
    Task<MescItemSnapshot?> GetMescItemSnapshotAsync(Guid mescItemId, CancellationToken cancellationToken);
    Task<bool> UnitOfMeasureExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid?> GetDepartmentIdByTypeAsync(DepartmentType type, CancellationToken cancellationToken);
    Task AddAsync(Indent indent, CancellationToken cancellationToken);
    Task<IReadOnlyList<IndentListDto>> GetAllAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

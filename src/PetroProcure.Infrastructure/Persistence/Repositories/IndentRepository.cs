using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Indents;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class IndentRepository(PetroProcureDbContext dbContext) : IIndentRepository
{
    public async Task<string> GenerateNextIndentNumberAsync(int yearPart, int typeDigit, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var sequence = await dbContext.IndentSequences
                .SingleOrDefaultAsync(value => value.YearPart == yearPart && value.TypeDigit == typeDigit, cancellationToken);
            int next;
            if (sequence is null)
            {
                next = 1;
                dbContext.IndentSequences.Add(new IndentSequence(Guid.NewGuid(), yearPart, typeDigit, next));
            }
            else
            {
                next = sequence.Next();
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Indent.BuildIndentNumber(yearPart, typeDigit, next);
        });
    }

    public Task<bool> IndentNumberExistsAsync(string indentNumber, CancellationToken cancellationToken) =>
        dbContext.Indents.AnyAsync(indent => indent.IndentNumber == indentNumber, cancellationToken);

    public Task<Indent?> FindAsync(Guid id, bool includeItems, CancellationToken cancellationToken)
    {
        IQueryable<Indent> query = dbContext.Indents;
        if (includeItems) query = query.Include(indent => indent.Items);
        return query.SingleOrDefaultAsync(indent => indent.Id == id, cancellationToken);
    }

    public Task<Indent?> FindByNumberAsync(string indentNumber, CancellationToken cancellationToken) =>
        dbContext.Indents.Include(indent => indent.Items)
            .SingleOrDefaultAsync(indent => indent.IndentNumber == indentNumber, cancellationToken);

    public Task<MescItemSnapshot?> GetMescItemSnapshotAsync(Guid mescItemId, CancellationToken cancellationToken) =>
        dbContext.MescItems.AsNoTracking()
            .Where(item => item.Id == mescItemId)
            .Select(item => new MescItemSnapshot(
                item.Id, item.Code, item.GeneralGroupCode, item.GeneralGroup!.Description,
                item.Description, item.UnitOfMeasureId, item.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<bool> UnitOfMeasureExistsAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.UnitOfMeasures.AnyAsync(unit => unit.Id == id && unit.IsActive, cancellationToken);

    public async Task<Guid?> GetDepartmentIdByTypeAsync(DepartmentType type, CancellationToken cancellationToken) =>
        await dbContext.Departments.AsNoTracking()
            .Where(department => department.Type == type && department.IsActive)
            .Select(department => (Guid?)department.Id)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task AddAsync(Indent indent, CancellationToken cancellationToken) =>
        await dbContext.Indents.AddAsync(indent, cancellationToken);

    public async Task<IReadOnlyList<IndentListDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Indents.AsNoTracking()
            .OrderByDescending(indent => indent.CreatedAt)
            .Select(indent => new IndentListDto(
                indent.Id, indent.IndentNumber, indent.IndentType, indent.Title,
                indent.RequestingDepartmentId, indent.Status, indent.CreatedAt, indent.Items.Count,
                indent.SourceType, indent.SourceDescription, indent.SourceReferenceId, indent.SourceDisplayText))
            .ToListAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try { await dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (exception.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new IndentConflictException("An indent with the same number or sequence already exists.");
        }
    }
}

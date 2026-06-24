using Microsoft.EntityFrameworkCore;
using PetroProcure.Reporting;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class ReportDataProvider(PetroProcureDbContext db) : IReportDataProvider
{
    public async Task<PurchaseFileReportData?> GetPurchaseFileAsync(Guid id, CancellationToken ct)
    {
        var file = await db.PurchaseFiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (file is null) return null;
        var department = await db.Departments.Where(x => x.Id == file.CurrentDepartmentId).Select(x => x.Name).SingleOrDefaultAsync(ct) ?? "—";
        var indent = file.SourceIndentId.HasValue ? await db.Indents.Where(x => x.Id == file.SourceIndentId).Select(x => x.IndentNumber).SingleOrDefaultAsync(ct) : null;
        var items = await db.PurchaseFileItems.AsNoTracking().Where(x => x.PurchaseFileId == id).OrderBy(x => x.MescCode).ToListAsync(ct);
        return new(file.Id, file.FileNumber, file.Title, file.Status.ToString(), department, file.CreatedAt, indent, Group(items.Select(x =>
            new ReportItemData(x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, Unit(x.UnitOfMeasureId), x.RequestedQuantity))));
    }

    public async Task<IndentReportData?> GetIndentAsync(Guid id, CancellationToken ct)
    {
        var indent = await db.Indents.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (indent is null) return null;
        var department = await db.Departments.Where(x => x.Id == indent.RequestingDepartmentId).Select(x => x.Name).SingleOrDefaultAsync(ct) ?? "—";
        var items = await db.IndentItems.AsNoTracking().Where(x => x.IndentId == id).OrderBy(x => x.MescCode).ToListAsync(ct);
        return new(indent.Id, indent.IndentNumber, indent.IndentType.ToString(), department, Group(items.Select(x =>
            new ReportItemData(x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, Unit(x.UnitOfMeasureId), x.RequestedQuantity))));
    }

    private static IReadOnlyList<ReportItemGroupData> Group(IEnumerable<ReportItemData> items) =>
        items.GroupBy(x => new { x.GeneralGroupCode, x.GeneralDescription }).OrderBy(x => x.Key.GeneralGroupCode)
            .Select(x => new ReportItemGroupData(x.Key.GeneralGroupCode, x.Key.GeneralDescription, x.ToArray())).ToArray();
    private static string Unit(Guid id) => id.ToString()[^1] switch { '1' => "عدد", '2' => "متر", '3' => "کیلوگرم", '4' => "لیتر", '5' => "بسته", '6' => "دستگاه", _ => "واحد" };
}

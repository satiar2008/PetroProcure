using System.Data;
using PetroProcure.Application.Indents;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Orders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Orders;

namespace PetroProcure.Application.Orders;

public sealed record UpdateInventoryControlItemCommand(Guid Id, decimal MinimumStockLevel, decimal ReorderPoint,
    decimal? MaximumStockLevel, decimal? SafetyStock, bool IsStockControlled, bool IsActive, string? Notes);
public sealed record CreateInventoryControlItemCommand(Guid MescItemId, Guid WarehouseId, decimal InitialQuantity,
    decimal MinimumStockLevel, decimal ReorderPoint, decimal? MaximumStockLevel, decimal? SafetyStock,
    bool IsStockControlled, string? Notes);
public sealed record CreateStockAdjustmentCommand(Guid InventoryControlItemId, Guid WarehouseId, decimal Quantity, string? Description);
public sealed record CreateMaterialNeedCommand(Guid MescItemId, decimal RequestedQuantity, DateOnly? NeededByDate,
    Guid SourceDepartmentId, Guid? ApplicantDepartmentId, MaterialNeedPriority Priority, string? Description);
public sealed record SubmitMaterialNeedCommand(Guid Id);
public sealed record ReviewMaterialNeedCommand(Guid Id, string? Comment);
public sealed record ApproveMaterialNeedCommand(Guid Id, string? Comment);
public sealed record RejectMaterialNeedCommand(Guid Id, string Reason);
public sealed record ConvertMaterialNeedToIndentCommand(Guid Id, int YearPart, int TypeDigit, string? Title);
public sealed record ConvertMaterialNeedsToIndentCommand(IReadOnlyList<Guid> MaterialNeedIds, int YearPart, int TypeDigit, string? Title);
public sealed record DetectShortageAlertsCommand(bool IncludeExistingOpen);
public sealed record ConvertShortageAlertToIndentCommand(Guid Id, int YearPart, int TypeDigit, Guid RequestingDepartmentId, string? Title);
public sealed record ResolveShortageAlertCommand(Guid Id, string? ResolutionNote);

public sealed record GetOrdersDashboardQuery;
public sealed record GetInventoryControlItemsQuery(InventoryControlListRequest Request);
public sealed record GetStockBalancesQuery(StockBalanceListRequest Request);
public sealed record GetMaterialNeedsQuery(MaterialNeedListRequest Request);
public sealed record GetMaterialNeedByIdQuery(Guid Id);
public sealed record GetShortageAlertsQuery(ShortageAlertListRequest Request);
public sealed record GetMaterialNeedsGroupedByMescQuery(MaterialNeedListRequest Request);

public interface IOrdersRepository
{
    Task<string> GenerateNextNeedNumberAsync(int year, CancellationToken ct);
    Task<MescOrderSnapshot?> GetMescSnapshotAsync(Guid mescItemId, CancellationToken ct);
    Task<Guid> ResolveUnitOfMeasureIdAsync(string unitOfMeasure, CancellationToken ct);
    Task<InventoryControlItem?> FindInventoryControlItemAsync(Guid id, CancellationToken ct);
    Task<InventoryControlItem?> FindInventoryControlItemByMescItemAsync(Guid mescItemId, CancellationToken ct);
    Task<MaterialNeed?> FindMaterialNeedAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<MaterialNeed>> FindMaterialNeedsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
    Task<ShortageAlert?> FindShortageAlertAsync(Guid id, CancellationToken ct);
    Task AddMaterialNeedAsync(MaterialNeed need, CancellationToken ct);
    Task AddInventoryControlItemAsync(InventoryControlItem item, CancellationToken ct);
    Task AddShortageAlertAsync(ShortageAlert alert, CancellationToken ct);
    Task AddIndentAsync(Indent indent, CancellationToken ct);
    Task ApplyStockAdjustmentAsync(InventoryControlItem item, Guid warehouseId, decimal quantity,
        Func<Task<string>> transactionNumberFactory, Guid userId, string? description, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<PagedResult<InventoryControlItemDto>> GetInventoryControlItemsAsync(InventoryControlListRequest request, CancellationToken ct);
    Task<PagedResult<StockBalanceDto>> GetStockBalancesAsync(StockBalanceListRequest request, CancellationToken ct);
    Task<PagedResult<MaterialNeedDto>> GetMaterialNeedsAsync(MaterialNeedListRequest request, CancellationToken ct);
    Task<MaterialNeedDto?> GetMaterialNeedDtoAsync(Guid id, CancellationToken ct);
    Task<PagedResult<ShortageAlertDto>> GetShortageAlertsAsync(ShortageAlertListRequest request, CancellationToken ct);
    Task<IReadOnlyList<MaterialNeedsGroupedByMescDto>> GetMaterialNeedsGroupedByMescAsync(MaterialNeedListRequest request, CancellationToken ct);
    Task<OrdersDashboardDto> GetDashboardAsync(CancellationToken ct);
    Task<IReadOnlyList<ShortageAlert>> DetectShortagesAsync(bool includeExistingOpen, CancellationToken ct);
}

public sealed record MescOrderSnapshot(Guid Id, string Code, string GeneralGroupCode, string GeneralDescription,
    string SpecificDescription, string UnitOfMeasure, Guid UnitOfMeasureId, bool IsActive);

public sealed class OrdersCommandHandler(
    IOrdersRepository repository,
    IIndentNumberService indentNumberService,
    ICurrentUserService currentUser,
    PetroProcure.Application.Warehouse.IInventoryTransactionNumberService? transactionNumberService = null)
{
    public async Task<InventoryControlItemDto> Handle(UpdateInventoryControlItemCommand command, CancellationToken ct = default)
    {
        var item = await repository.FindInventoryControlItemAsync(command.Id, ct)
            ?? throw new OrdersNotFoundException("Inventory control item was not found.");
        item.Update(command.MinimumStockLevel, command.ReorderPoint, command.MaximumStockLevel,
            command.SafetyStock, command.IsStockControlled, command.IsActive, command.Notes);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetInventoryControlItemsAsync(new InventoryControlListRequest(item.MescCode, true, 1, 1), ct)).Items[0];
    }

    public async Task<InventoryControlItemDto> Handle(CreateInventoryControlItemCommand command, CancellationToken ct = default)
    {
        if (command.InitialQuantity <= 0) throw new OrdersValidationException("مقدار اولیه موجودی باید بزرگ‌تر از صفر باشد.");
        if (transactionNumberService is null)
            throw new OrdersValidationException("Inventory transaction number service is not configured.");
        var snapshot = await ActiveMesc(command.MescItemId, ct);
        var item = await repository.FindInventoryControlItemByMescItemAsync(command.MescItemId, ct);
        if (item is null)
        {
            item = new InventoryControlItem(Guid.NewGuid(), snapshot.Id, snapshot.Code, snapshot.GeneralGroupCode,
                snapshot.GeneralDescription, snapshot.SpecificDescription, snapshot.UnitOfMeasureId,
                command.MinimumStockLevel, command.ReorderPoint, command.MaximumStockLevel, command.SafetyStock,
                command.IsStockControlled, command.Notes);
            await repository.AddInventoryControlItemAsync(item, ct);
        }

        await repository.ApplyStockAdjustmentAsync(item, command.WarehouseId, command.InitialQuantity,
            () => transactionNumberService.GenerateNextTransactionNumber(DateTime.UtcNow.Year, ct),
            currentUser.UserId, command.Notes, ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetInventoryControlItemsAsync(new InventoryControlListRequest(snapshot.Code, true, 1, 1), ct)).Items[0];
    }

    public async Task<InventoryControlItemDto> Handle(CreateStockAdjustmentCommand command, CancellationToken ct = default)
    {
        if (command.Quantity <= 0) throw new OrdersValidationException("مقدار موجودی باید بزرگ‌تر از صفر باشد.");
        if (transactionNumberService is null)
            throw new OrdersValidationException("Inventory transaction number service is not configured.");
        var item = await repository.FindInventoryControlItemAsync(command.InventoryControlItemId, ct)
            ?? throw new OrdersNotFoundException("Inventory control item was not found.");
        await repository.ApplyStockAdjustmentAsync(item, command.WarehouseId, command.Quantity,
            () => transactionNumberService.GenerateNextTransactionNumber(DateTime.UtcNow.Year, ct),
            currentUser.UserId, command.Description, ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetInventoryControlItemsAsync(new InventoryControlListRequest(item.MescCode, true, 1, 1), ct)).Items[0];
    }

    public async Task<MaterialNeedDto> Handle(CreateMaterialNeedCommand command, CancellationToken ct = default)
    {
        RequireDepartment(command.SourceDepartmentId);
        var snapshot = await ActiveMesc(command.MescItemId, ct);
        var unitId = snapshot.UnitOfMeasureId;
        var needNumber = await repository.GenerateNextNeedNumberAsync(DateTime.UtcNow.Year, ct);
        var need = new MaterialNeed(Guid.NewGuid(), needNumber, snapshot.Id, snapshot.Code, snapshot.GeneralGroupCode,
            snapshot.GeneralDescription, snapshot.SpecificDescription, unitId, command.RequestedQuantity,
            command.NeededByDate, command.SourceDepartmentId, command.ApplicantDepartmentId, currentUser.UserId,
            command.Priority, command.Description);
        await repository.AddMaterialNeedAsync(need, ct);
        await repository.SaveChangesAsync(ct);
        return ToDto(need);
    }

    public Task Handle(SubmitMaterialNeedCommand command, CancellationToken ct = default) =>
        ChangeNeed(command.Id, need => need.Submit(), ct);
    public Task Handle(ReviewMaterialNeedCommand command, CancellationToken ct = default) =>
        ChangeNeed(command.Id, need => need.Review(currentUser.UserId), ct);
    public Task Handle(ApproveMaterialNeedCommand command, CancellationToken ct = default) =>
        ChangeNeed(command.Id, need => need.Approve(currentUser.UserId), ct);
    public Task Handle(RejectMaterialNeedCommand command, CancellationToken ct = default) =>
        ChangeNeed(command.Id, need => need.Reject(currentUser.UserId, command.Reason), ct);

    public async Task<Guid> Handle(ConvertMaterialNeedToIndentCommand command, CancellationToken ct = default)
    {
        var need = await repository.FindMaterialNeedAsync(command.Id, ct)
            ?? throw new OrdersNotFoundException("Material need was not found.");
        if (need.Status != MaterialNeedStatus.Approved)
            throw new OrdersValidationException("Only approved material needs can be converted to indent.");
        var indent = await CreateIndent(command.YearPart, command.TypeDigit, command.Title ?? $"تقاضا از نیاز {need.NeedNumber}",
            need.SourceDepartmentId, need.ApplicantDepartmentId, need.Description, need.MescItemId, need.MescCode,
            need.MescGeneralGroupCode, need.GeneralDescription, need.SpecificDescription, need.UnitOfMeasureId,
            need.RequestedQuantity, need.Description, need.NeededByDate, IndentSourceType.MaterialNeed, need.Id, null, need.NeedNumber, ct);
        need.MarkConvertedToIndent(indent.Id);
        await repository.SaveChangesAsync(ct);
        return indent.Id;
    }

    public async Task<Guid> Handle(ConvertMaterialNeedsToIndentCommand command, CancellationToken ct = default)
    {
        var ids = command.MaterialNeedIds.Distinct().ToArray();
        if (ids.Length == 0) throw new OrdersValidationException("حداقل یک نیاز کالا باید انتخاب شود.");
        var needs = await repository.FindMaterialNeedsAsync(ids, ct);
        if (needs.Count != ids.Length) throw new OrdersValidationException("برخی از نیازهای انتخاب‌شده یافت نشدند.");
        if (needs.Any(x => x.Status != MaterialNeedStatus.Approved))
            throw new OrdersValidationException("فقط نیازهای تأییدشده قابل تبدیل به Indent هستند.");
        if (needs.Select(x => x.MescGeneralGroupCode).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            throw new OrdersValidationException("برای تجمیع، همه نیازها باید از یک گروه کالا باشند.");
        if (needs.Select(x => x.SourceDepartmentId).Distinct().Count() > 1)
            throw new OrdersValidationException("برای تجمیع، واحد درخواست‌کننده نیازها باید یکسان باشد.");

        var ordered = needs.OrderBy(x => x.NeedNumber).ToArray();
        var first = ordered[0];
        var number = await indentNumberService.GenerateNextIndentNumber(command.YearPart, command.TypeDigit, ct);
        var parts = indentNumberService.ParseIndentNumber(number);
        var sourceNumbers = string.Join("، ", ordered.Select(x => x.NeedNumber));
        var indent = new Indent(Guid.NewGuid(), number, parts.YearPart, parts.TypeDigit, parts.Sequence,
            command.Title ?? $"تقاضای تجمیعی گروه {first.MescGeneralGroupCode}",
            first.SourceDepartmentId,
            first.ApplicantDepartmentId,
            currentUser.UserId,
            $"تجمیع نیازهای کالا: {sourceNumbers}",
            IndentSourceType.MaterialNeed,
            first.Id,
            null,
            sourceNumbers);
        foreach (var need in ordered)
        {
            indent.AddItem(new IndentItem(Guid.NewGuid(), indent.Id, need.MescItemId, need.MescCode,
                need.MescGeneralGroupCode, need.GeneralDescription, need.SpecificDescription, need.UnitOfMeasureId,
                need.RequestedQuantity, need.Description, need.NeededByDate));
            need.MarkConvertedToIndent(indent.Id);
        }
        await repository.AddIndentAsync(indent, ct);
        await repository.SaveChangesAsync(ct);
        return indent.Id;
    }

    public async Task<IReadOnlyList<ShortageAlertDto>> Handle(DetectShortageAlertsCommand command, CancellationToken ct = default)
    {
        var alerts = await repository.DetectShortagesAsync(command.IncludeExistingOpen, ct);
        await repository.SaveChangesAsync(ct);
        return alerts.Select(ToDto).ToArray();
    }

    public async Task<Guid> Handle(ConvertShortageAlertToIndentCommand command, CancellationToken ct = default)
    {
        RequireDepartment(command.RequestingDepartmentId);
        var alert = await repository.FindShortageAlertAsync(command.Id, ct)
            ?? throw new OrdersNotFoundException("Shortage alert was not found.");
        if (alert.Status is ShortageAlertStatus.Resolved or ShortageAlertStatus.Cancelled)
            throw new OrdersValidationException("Resolved or cancelled shortage alerts cannot be converted.");
        var indent = await CreateIndent(command.YearPart, command.TypeDigit, command.Title ?? $"تقاضا از کمبود {alert.MescCode}",
            command.RequestingDepartmentId, null, alert.ResolutionNote, alert.MescItemId, alert.MescCode,
            alert.MescGeneralGroupCode, alert.GeneralDescription, alert.SpecificDescription, alert.UnitOfMeasureId,
            alert.ShortageQuantity, "Created from shortage alert.", null, IndentSourceType.ShortageAlert, null, alert.Id, alert.MescCode, ct);
        alert.MarkConvertedToIndent(indent.Id);
        await repository.SaveChangesAsync(ct);
        return indent.Id;
    }

    public Task Handle(ResolveShortageAlertCommand command, CancellationToken ct = default) =>
        ChangeAlert(command.Id, alert => alert.Resolve(command.ResolutionNote), ct);

    private async Task<Indent> CreateIndent(int yearPart, int typeDigit, string title, Guid requestingDepartmentId,
        Guid? applicantDepartmentId, string? description, Guid mescItemId, string mescCode, string groupCode,
        string generalDescription, string specificDescription, Guid unitOfMeasureId, decimal quantity,
        string? technicalDescription, DateOnly? requiredDate, IndentSourceType sourceType, Guid? sourceMaterialNeedId,
        Guid? sourceShortageAlertId, string? sourceDescription, CancellationToken ct)
    {
        var number = await indentNumberService.GenerateNextIndentNumber(yearPart, typeDigit, ct);
        var parts = indentNumberService.ParseIndentNumber(number);
        var indent = new Indent(Guid.NewGuid(), number, parts.YearPart, parts.TypeDigit, parts.Sequence,
            title, requestingDepartmentId, applicantDepartmentId, currentUser.UserId, description,
            sourceType, sourceMaterialNeedId, sourceShortageAlertId, sourceDescription);
        indent.AddItem(new IndentItem(Guid.NewGuid(), indent.Id, mescItemId, mescCode, groupCode, generalDescription,
            specificDescription, unitOfMeasureId, quantity, technicalDescription, requiredDate));
        await repository.AddIndentAsync(indent, ct);
        return indent;
    }

    private async Task ChangeNeed(Guid id, Action<MaterialNeed> change, CancellationToken ct)
    {
        var need = await repository.FindMaterialNeedAsync(id, ct) ?? throw new OrdersNotFoundException("Material need was not found.");
        try { change(need); } catch (InvalidOperationException e) { throw new OrdersValidationException(e.Message); }
        await repository.SaveChangesAsync(ct);
    }

    private async Task ChangeAlert(Guid id, Action<ShortageAlert> change, CancellationToken ct)
    {
        var alert = await repository.FindShortageAlertAsync(id, ct) ?? throw new OrdersNotFoundException("Shortage alert was not found.");
        try { change(alert); } catch (InvalidOperationException e) { throw new OrdersValidationException(e.Message); }
        await repository.SaveChangesAsync(ct);
    }

    private async Task<MescOrderSnapshot> ActiveMesc(Guid id, CancellationToken ct)
    {
        var snapshot = await repository.GetMescSnapshotAsync(id, ct) ?? throw new OrdersValidationException("MESC item was not found.");
        if (!snapshot.IsActive) throw new OrdersValidationException("Inactive MESC items cannot be used.");
        return snapshot;
    }

    private void RequireDepartment(Guid departmentId)
    {
        if (!currentUser.IsSystemAdmin && !currentUser.DepartmentIds.Contains(departmentId))
            throw new CurrentUserForbiddenException("User does not have access to the selected department.");
    }

    internal static MaterialNeedDto ToDto(MaterialNeed x) => new(x.Id, x.NeedNumber, x.MescItemId, x.MescCode,
        x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId, x.RequestedQuantity,
        x.NeededByDate, x.SourceDepartmentId, x.ApplicantDepartmentId, x.RequestedByUserId, x.Status, x.Priority,
        x.Description, x.CreatedAt, x.SubmittedAt, x.ReviewedAt, x.ReviewedByUserId, x.RelatedIndentId, x.RejectionReason);
    internal static ShortageAlertDto ToDto(ShortageAlert x) => new(x.Id, x.MescItemId, x.MescCode, x.MescGeneralGroupCode,
        x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId, x.CurrentStock, x.ReorderPoint,
        x.ShortageQuantity, x.Status, x.DetectedAt, x.ResolvedAt, x.RelatedIndentId, x.ResolutionNote);
}

public sealed class OrdersQueryHandler(IOrdersRepository repository)
{
    public Task<OrdersDashboardDto> Handle(GetOrdersDashboardQuery _, CancellationToken ct = default) => repository.GetDashboardAsync(ct);
    public Task<PagedResult<InventoryControlItemDto>> Handle(GetInventoryControlItemsQuery q, CancellationToken ct = default) => repository.GetInventoryControlItemsAsync(q.Request, ct);
    public Task<PagedResult<StockBalanceDto>> Handle(GetStockBalancesQuery q, CancellationToken ct = default) => repository.GetStockBalancesAsync(q.Request, ct);
    public Task<PagedResult<MaterialNeedDto>> Handle(GetMaterialNeedsQuery q, CancellationToken ct = default) => repository.GetMaterialNeedsAsync(q.Request, ct);
    public async Task<MaterialNeedDetailDto?> Handle(GetMaterialNeedByIdQuery q, CancellationToken ct = default) =>
        await repository.GetMaterialNeedDtoAsync(q.Id, ct) is { } dto ? new MaterialNeedDetailDto(dto) : null;
    public Task<PagedResult<ShortageAlertDto>> Handle(GetShortageAlertsQuery q, CancellationToken ct = default) => repository.GetShortageAlertsAsync(q.Request, ct);
    public Task<IReadOnlyList<MaterialNeedsGroupedByMescDto>> Handle(GetMaterialNeedsGroupedByMescQuery q, CancellationToken ct = default) => repository.GetMaterialNeedsGroupedByMescAsync(q.Request, ct);
}

public sealed class OrdersValidationException(string message) : Exception(message);
public sealed class OrdersNotFoundException(string message) : Exception(message);

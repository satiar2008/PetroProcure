using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.PurchaseOrders;
using PetroProcure.Contracts.V1.Warehouse;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Warehouse;

namespace PetroProcure.Application.Warehouse;

public sealed record CreateWarehouseCommand(CreateWarehouseRequest Request);
public sealed record UpdateWarehouseCommand(Guid Id, UpdateWarehouseRequest Request);
public sealed record CreateWarehouseReceiptCommand(CreateWarehouseReceiptRequest Request);
public sealed record CreateWarehouseReceiptFromPurchaseOrderCommand(Guid PurchaseOrderId, CreateWarehouseReceiptFromPurchaseOrderRequest Request);
public sealed record UpdateWarehouseReceiptCommand(Guid Id, UpdateWarehouseReceiptRequest Request);
public sealed record AddWarehouseReceiptItemCommand(Guid ReceiptId, AddWarehouseReceiptItemRequest Request);
public sealed record RemoveWarehouseReceiptItemCommand(Guid ReceiptId, Guid ItemId);
public sealed record SubmitWarehouseReceiptCommand(Guid Id, string? Comment);
public sealed record ApproveWarehouseReceiptCommand(Guid Id, string? Comment);
public sealed record CancelWarehouseReceiptCommand(Guid Id, string Reason);

public sealed record GetWarehousesQuery(WarehouseListRequest Request);
public sealed record GetWarehouseByIdQuery(Guid Id);
public sealed record GetWarehouseReceiptsQuery(WarehouseReceiptListRequest Request);
public sealed record GetWarehouseReceiptByIdQuery(Guid Id);
public sealed record GetWarehouseReceiptByNumberQuery(string ReceiptNumber);
public sealed record GetWarehouseReceiptsByPurchaseOrderQuery(Guid PurchaseOrderId);
public sealed record GetWarehouseReceiptsByPurchaseFileQuery(Guid PurchaseFileId);
public sealed record GetInventoryTransactionsQuery(InventoryTransactionListRequest Request);
public sealed record GetStockBalancesQuery(StockBalanceListRequest Request);
public sealed record GetIssuedPurchaseOrdersWaitingForReceiptQuery;

public sealed record PurchaseOrderReceiptSnapshot(Guid Id, string PurchaseOrderNumber, Guid PurchaseFileId,
    string PurchaseFileNumber, Guid SupplierId, string SupplierName, PurchaseOrderStatus Status,
    IReadOnlyList<PurchaseOrderItemReceiptSnapshot> Items);
public sealed record PurchaseOrderItemReceiptSnapshot(Guid Id, Guid MescItemId, string MescCode,
    string MescGeneralGroupCode, string GeneralDescription, string SpecificDescription, Guid UnitOfMeasureId,
    decimal OrderedQuantity, decimal ReceivedQuantity, decimal RemainingQuantity);

public interface IWarehouseReceiptNumberService { Task<string> GenerateNextReceiptNumber(int year, CancellationToken ct = default); }
public interface IInventoryTransactionNumberService { Task<string> GenerateNextTransactionNumber(int year, CancellationToken ct = default); }
public interface IWarehouseReceiptEligibilityService { Task<PurchaseOrderReceiptSnapshot> EnsureCanReceiveAsync(Guid purchaseOrderId, CancellationToken ct = default); }
public interface IInventoryBalanceService
{
    Task ApplyReceiptAsync(WarehouseReceipt receipt, IReadOnlyList<WarehouseReceiptItem> items, Guid userId, CancellationToken ct = default);
}

public interface IWarehouseRepository
{
    Task<string> GenerateNextReceiptNumberAsync(int year, CancellationToken ct);
    Task<string> GenerateNextInventoryTransactionNumberAsync(int year, CancellationToken ct);
    Task AddWarehouseAsync(Domain.Modules.Warehouse.Warehouse warehouse, CancellationToken ct);
    Task<Domain.Modules.Warehouse.Warehouse?> FindWarehouseAsync(Guid id, CancellationToken ct);
    Task<bool> WarehouseCodeExistsAsync(string code, Guid? excludingId, CancellationToken ct);
    Task<PurchaseOrderReceiptSnapshot?> GetPurchaseOrderReceiptSnapshotAsync(Guid purchaseOrderId, CancellationToken ct);
    Task AddReceiptAsync(WarehouseReceipt receipt, CancellationToken ct);
    Task<WarehouseReceipt?> FindReceiptAsync(Guid id, bool includeDetails, CancellationToken ct);
    Task AddReceiptDocumentAsync(WarehouseReceiptDocument document, CancellationToken ct);
    Task ApplyReceiptToPurchaseOrderAndStockAsync(WarehouseReceipt receipt, IReadOnlyList<WarehouseReceiptItem> items,
        Func<Task<string>> transactionNumberFactory, Guid userId, CancellationToken ct);
    Task<PagedResult<WarehouseDto>> GetWarehousesAsync(WarehouseListRequest request, CancellationToken ct);
    Task<WarehouseDto?> GetWarehouseAsync(Guid id, CancellationToken ct);
    Task<PagedResult<WarehouseReceiptSummaryDto>> GetReceiptsAsync(WarehouseReceiptListRequest request, CancellationToken ct);
    Task<WarehouseReceiptDetailDto?> GetReceiptDetailAsync(Guid id, CancellationToken ct);
    Task<WarehouseReceiptDetailDto?> GetReceiptDetailByNumberAsync(string receiptNumber, CancellationToken ct);
    Task<IReadOnlyList<WarehouseReceiptSummaryDto>> GetReceiptsByPurchaseOrderAsync(Guid purchaseOrderId, CancellationToken ct);
    Task<IReadOnlyList<WarehouseReceiptSummaryDto>> GetReceiptsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct);
    Task<IReadOnlyList<WarehouseReceiptDocumentDto>> GetDocumentsAsync(Guid receiptId, CancellationToken ct);
    Task<PagedResult<InventoryTransactionDto>> GetInventoryTransactionsAsync(InventoryTransactionListRequest request, CancellationToken ct);
    Task<PagedResult<StockBalanceDto>> GetStockBalancesAsync(StockBalanceListRequest request, CancellationToken ct);
    Task<IReadOnlyList<PurchaseOrderSummaryDto>> GetIssuedPurchaseOrdersWaitingForReceiptAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class WarehouseReceiptNumberService(IWarehouseRepository repository) : IWarehouseReceiptNumberService
{
    public async Task<string> GenerateNextReceiptNumber(int year, CancellationToken ct = default)
    {
        var number = await repository.GenerateNextReceiptNumberAsync(year, ct);
        if (!number.StartsWith($"WR-{year:0000}-", StringComparison.Ordinal) || number.Length != 14)
            throw new WarehouseReceiptValidationException("Generated receipt number has invalid format.");
        return number;
    }
}

public sealed class InventoryTransactionNumberService(IWarehouseRepository repository) : IInventoryTransactionNumberService
{
    public async Task<string> GenerateNextTransactionNumber(int year, CancellationToken ct = default)
    {
        var number = await repository.GenerateNextInventoryTransactionNumberAsync(year, ct);
        if (!number.StartsWith($"ITX-{year:0000}-", StringComparison.Ordinal) || number.Length != 15)
            throw new WarehouseReceiptValidationException("Generated inventory transaction number has invalid format.");
        return number;
    }
}

public sealed class WarehouseReceiptEligibilityService(IWarehouseRepository repository) : IWarehouseReceiptEligibilityService
{
    public async Task<PurchaseOrderReceiptSnapshot> EnsureCanReceiveAsync(Guid purchaseOrderId, CancellationToken ct = default)
    {
        var po = await repository.GetPurchaseOrderReceiptSnapshotAsync(purchaseOrderId, ct)
            ?? throw new WarehouseReceiptValidationException("Purchase order was not found.");
        if (po.Status is not PurchaseOrderStatus.Issued and not PurchaseOrderStatus.PartiallyReceived)
            throw new WarehouseReceiptValidationException("Warehouse receipt can be created only for issued or partially received purchase orders.");
        return po;
    }
}

public sealed class InventoryBalanceService(
    IWarehouseRepository repository,
    IInventoryTransactionNumberService transactionNumberService) : IInventoryBalanceService
{
    public Task ApplyReceiptAsync(WarehouseReceipt receipt, IReadOnlyList<WarehouseReceiptItem> items, Guid userId, CancellationToken ct = default) =>
        repository.ApplyReceiptToPurchaseOrderAndStockAsync(receipt, items,
            () => transactionNumberService.GenerateNextTransactionNumber(DateTime.UtcNow.Year, ct), userId, ct);
}

public sealed class WarehouseCommandHandler(
    IWarehouseRepository repository,
    IWarehouseReceiptNumberService numberService,
    IWarehouseReceiptEligibilityService eligibility,
    IInventoryBalanceService inventory,
    ICurrentUserService currentUser)
{
    public async Task<WarehouseDto> Handle(CreateWarehouseCommand command, CancellationToken ct = default)
    {
        var r = command.Request;
        if (await repository.WarehouseCodeExistsAsync(r.Code.Trim().ToUpperInvariant(), null, ct))
            throw new WarehouseReceiptValidationException("Warehouse code already exists.");
        var warehouse = new Domain.Modules.Warehouse.Warehouse(Guid.NewGuid(), r.Code, r.Name, r.Location, r.Description);
        await repository.AddWarehouseAsync(warehouse, ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetWarehouseAsync(warehouse.Id, ct))!;
    }

    public async Task<WarehouseDto> Handle(UpdateWarehouseCommand command, CancellationToken ct = default)
    {
        var warehouse = await repository.FindWarehouseAsync(command.Id, ct)
            ?? throw new WarehouseReceiptNotFoundException("Warehouse was not found.");
        warehouse.Update(command.Request.Name, command.Request.Location, command.Request.IsActive, command.Request.Description);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetWarehouseAsync(warehouse.Id, ct))!;
    }

    public async Task<WarehouseReceiptDetailDto> Handle(CreateWarehouseReceiptCommand command, CancellationToken ct = default)
    {
        var r = command.Request;
        await eligibility.EnsureCanReceiveAsync(r.PurchaseOrderId, ct);
        var po = await repository.GetPurchaseOrderReceiptSnapshotAsync(r.PurchaseOrderId, ct)
            ?? throw new WarehouseReceiptValidationException("Purchase order was not found.");
        var receipt = await NewReceipt(po, r.WarehouseId, r.ReceiptDate, r.DeliveryNoteNumber,
            r.CarrierName, r.VehicleNumber, r.Description, ct);
        await repository.AddReceiptAsync(receipt, ct);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(receipt.Id, ct);
    }

    public async Task<WarehouseReceiptDetailDto> Handle(CreateWarehouseReceiptFromPurchaseOrderCommand command, CancellationToken ct = default)
    {
        var po = await eligibility.EnsureCanReceiveAsync(command.PurchaseOrderId, ct);
        var r = command.Request;
        var receipt = await NewReceipt(po, r.WarehouseId, r.ReceiptDate, r.DeliveryNoteNumber,
            r.CarrierName, r.VehicleNumber, r.Description, ct);
        foreach (var itemRequest in r.Items ?? [])
            receipt.AddItem(ToReceiptItem(receipt.Id, po, itemRequest));
        await repository.AddReceiptAsync(receipt, ct);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(receipt.Id, ct);
    }

    public async Task<WarehouseReceiptDetailDto> Handle(UpdateWarehouseReceiptCommand command, CancellationToken ct = default)
    {
        var receipt = await RequiredReceipt(command.Id, true, ct);
        var r = command.Request;
        receipt.Update(r.ReceiptDate, r.DeliveryNoteNumber, r.CarrierName, r.VehicleNumber, r.Description);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(receipt.Id, ct);
    }

    public async Task<WarehouseReceiptItemDto> Handle(AddWarehouseReceiptItemCommand command, CancellationToken ct = default)
    {
        var receipt = await RequiredReceipt(command.ReceiptId, true, ct);
        var po = await repository.GetPurchaseOrderReceiptSnapshotAsync(receipt.PurchaseOrderId, ct)
            ?? throw new WarehouseReceiptValidationException("Purchase order was not found.");
        var item = ToReceiptItem(receipt.Id, po, command.Request);
        receipt.AddItem(item);
        await repository.SaveChangesAsync(ct);
        return (await RequiredDetail(receipt.Id, ct)).Items.OrderByDescending(x => x.Id).First();
    }

    public async Task Handle(RemoveWarehouseReceiptItemCommand command, CancellationToken ct = default)
    {
        var receipt = await RequiredReceipt(command.ReceiptId, true, ct);
        receipt.RemoveItem(command.ItemId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(SubmitWarehouseReceiptCommand command, CancellationToken ct = default)
    {
        var receipt = await RequiredReceipt(command.Id, true, ct);
        receipt.Submit();
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(ApproveWarehouseReceiptCommand command, CancellationToken ct = default)
    {
        var receipt = await RequiredReceipt(command.Id, true, ct);
        var items = receipt.Items.ToArray();
        receipt.Approve(currentUser.UserId);
        await inventory.ApplyReceiptAsync(receipt, items, currentUser.UserId, ct);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(CancelWarehouseReceiptCommand command, CancellationToken ct = default)
    {
        var receipt = await RequiredReceipt(command.Id, true, ct);
        receipt.Cancel(command.Reason, currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    private async Task<WarehouseReceipt> NewReceipt(PurchaseOrderReceiptSnapshot po, Guid warehouseId,
        DateTime receiptDate, string? deliveryNoteNumber, string? carrierName, string? vehicleNumber,
        string? description, CancellationToken ct)
    {
        if (await repository.FindWarehouseAsync(warehouseId, ct) is null)
            throw new WarehouseReceiptValidationException("Warehouse was not found.");
        var number = await numberService.GenerateNextReceiptNumber(DateTime.UtcNow.Year, ct);
        return new WarehouseReceipt(Guid.NewGuid(), number, po.Id, po.PurchaseFileId, warehouseId, po.SupplierId,
            receiptDate, currentUser.UserId, currentUser.UserId, deliveryNoteNumber, carrierName, vehicleNumber, description);
    }

    private static WarehouseReceiptItem ToReceiptItem(Guid receiptId, PurchaseOrderReceiptSnapshot po, AddWarehouseReceiptItemRequest r)
    {
        var item = po.Items.SingleOrDefault(x => x.Id == r.PurchaseOrderItemId)
            ?? throw new WarehouseReceiptValidationException("Purchase order item was not found.");
        if (r.ReceivedQuantity > item.RemainingQuantity)
            throw new WarehouseReceiptValidationException("Received quantity cannot exceed remaining quantity.");
        return new WarehouseReceiptItem(Guid.NewGuid(), receiptId, item.Id, item.MescItemId, item.MescCode,
            item.MescGeneralGroupCode, item.GeneralDescription, item.SpecificDescription, item.UnitOfMeasureId,
            item.OrderedQuantity, item.ReceivedQuantity, r.ReceivedQuantity, item.RemainingQuantity - r.ReceivedQuantity,
            r.QualityStatus, r.AcceptedQuantity, r.RejectedQuantity, r.BatchNumber, r.SerialNumber, r.ExpiryDate, r.Notes);
    }

    private async Task<WarehouseReceipt> RequiredReceipt(Guid id, bool includeDetails, CancellationToken ct) =>
        await repository.FindReceiptAsync(id, includeDetails, ct)
        ?? throw new WarehouseReceiptNotFoundException("Warehouse receipt was not found.");
    private async Task<WarehouseReceiptDetailDto> RequiredDetail(Guid id, CancellationToken ct) =>
        await repository.GetReceiptDetailAsync(id, ct)
        ?? throw new WarehouseReceiptNotFoundException("Warehouse receipt was not found.");
}

public sealed class WarehouseQueryHandler(IWarehouseRepository repository)
{
    public Task<PagedResult<WarehouseDto>> Handle(GetWarehousesQuery query, CancellationToken ct = default) => repository.GetWarehousesAsync(query.Request, ct);
    public Task<WarehouseDto?> Handle(GetWarehouseByIdQuery query, CancellationToken ct = default) => repository.GetWarehouseAsync(query.Id, ct);
    public Task<PagedResult<WarehouseReceiptSummaryDto>> Handle(GetWarehouseReceiptsQuery query, CancellationToken ct = default) => repository.GetReceiptsAsync(query.Request, ct);
    public Task<WarehouseReceiptDetailDto?> Handle(GetWarehouseReceiptByIdQuery query, CancellationToken ct = default) => repository.GetReceiptDetailAsync(query.Id, ct);
    public Task<WarehouseReceiptDetailDto?> Handle(GetWarehouseReceiptByNumberQuery query, CancellationToken ct = default) => repository.GetReceiptDetailByNumberAsync(query.ReceiptNumber, ct);
    public Task<IReadOnlyList<WarehouseReceiptSummaryDto>> Handle(GetWarehouseReceiptsByPurchaseOrderQuery query, CancellationToken ct = default) => repository.GetReceiptsByPurchaseOrderAsync(query.PurchaseOrderId, ct);
    public Task<IReadOnlyList<WarehouseReceiptSummaryDto>> Handle(GetWarehouseReceiptsByPurchaseFileQuery query, CancellationToken ct = default) => repository.GetReceiptsByPurchaseFileAsync(query.PurchaseFileId, ct);
    public Task<PagedResult<InventoryTransactionDto>> Handle(GetInventoryTransactionsQuery query, CancellationToken ct = default) => repository.GetInventoryTransactionsAsync(query.Request, ct);
    public Task<PagedResult<StockBalanceDto>> Handle(GetStockBalancesQuery query, CancellationToken ct = default) => repository.GetStockBalancesAsync(query.Request, ct);
    public Task<IReadOnlyList<PurchaseOrderSummaryDto>> Handle(GetIssuedPurchaseOrdersWaitingForReceiptQuery query, CancellationToken ct = default) => repository.GetIssuedPurchaseOrdersWaitingForReceiptAsync(ct);
}

public sealed class WarehouseReceiptValidationException(string message) : InvalidOperationException(message);
public sealed class WarehouseReceiptNotFoundException(string message) : InvalidOperationException(message);

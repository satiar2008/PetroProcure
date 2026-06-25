using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.PurchaseOrders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.PurchaseOrders;

namespace PetroProcure.Application.PurchaseOrders;

public sealed record CreatePurchaseOrderCommand(CreatePurchaseOrderRequest Request);
public sealed record CreatePurchaseOrderFromContractCommand(Guid ContractId, CreatePurchaseOrderFromContractRequest Request);
public sealed record CreatePurchaseOrderFromPurchaseFileCommand(Guid PurchaseFileId, CreatePurchaseOrderFromPurchaseFileRequest Request);
public sealed record UpdatePurchaseOrderCommand(Guid Id, UpdatePurchaseOrderRequest Request);
public sealed record AddPurchaseOrderItemCommand(Guid PurchaseOrderId, AddPurchaseOrderItemRequest Request);
public sealed record RemovePurchaseOrderItemCommand(Guid PurchaseOrderId, Guid ItemId);
public sealed record SubmitPurchaseOrderCommand(Guid Id, string? Comment);
public sealed record ApprovePurchaseOrderCommand(Guid Id, string? Comment);
public sealed record RejectPurchaseOrderCommand(Guid Id, string Comment);
public sealed record IssuePurchaseOrderCommand(Guid Id, string? Comment);
public sealed record CancelPurchaseOrderCommand(Guid Id, string Reason);

public sealed record GetPurchaseOrdersQuery(PurchaseOrderListRequest Request);
public sealed record GetPurchaseOrderByIdQuery(Guid Id);
public sealed record GetPurchaseOrderByNumberQuery(string PurchaseOrderNumber);
public sealed record GetPurchaseOrdersByPurchaseFileQuery(Guid PurchaseFileId);
public sealed record GetPurchaseOrdersBySupplierQuery(Guid SupplierId);
public sealed record GetPurchaseOrdersByContractQuery(Guid ContractId);
public sealed record GetPurchaseOrderDocumentsQuery(Guid PurchaseOrderId);

public sealed record PurchaseOrderItemSnapshot(Guid? PurchaseFileItemId, Guid? ContractItemId, Guid? TenderBidItemId,
    Guid MescItemId, string MescCode, string MescGeneralGroupCode, string GeneralDescription,
    string SpecificDescription, Guid UnitOfMeasureId, decimal OrderedQuantity, decimal? UnitPrice,
    DateTime? ExpectedDeliveryDate, string? TechnicalDescription, string? Notes);

public sealed record ContractPurchaseOrderSnapshot(Guid ContractId, string ContractNumber, Guid PurchaseFileId,
    Guid SupplierId, Guid? TenderId, Guid? TenderBidId, string Title, ContractStatus Status, string Currency,
    decimal? TotalAmount, decimal? TaxAmount, decimal? FinalAmount, DateTime? DeliveryDeadline,
    string? DeliveryTerms, string? PaymentTerms, string? WarrantyTerms, IReadOnlyList<PurchaseOrderItemSnapshot> Items);

public interface IPurchaseOrderNumberService
{
    Task<string> GenerateNextPurchaseOrderNumber(int year, CancellationToken ct = default);
}

public interface IPurchaseOrderEligibilityService
{
    Task EnsurePurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task EnsureSupplierExistsAsync(Guid supplierId, CancellationToken ct = default);
}

public interface IPurchaseOrderReportDataSourceBuilder
{
    Task<PurchaseOrderDetailDto?> BuildAsync(Guid purchaseOrderId, CancellationToken ct = default);
}

public interface IPurchaseOrderRepository
{
    Task<string> GenerateNextPurchaseOrderNumberAsync(int year, CancellationToken ct);
    Task<bool> PurchaseOrderNumberExistsAsync(string purchaseOrderNumber, CancellationToken ct);
    Task<bool> PurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken ct);
    Task<bool> SupplierExistsAsync(Guid supplierId, CancellationToken ct);
    Task<PurchaseOrder?> FindPurchaseOrderAsync(Guid id, bool includeDetails, CancellationToken ct);
    Task<ContractPurchaseOrderSnapshot?> GetContractSnapshotAsync(Guid contractId, CancellationToken ct);
    Task<IReadOnlyList<PurchaseOrderItemSnapshot>> GetPurchaseFileItemSnapshotsAsync(Guid purchaseFileId, CancellationToken ct);
    Task<PurchaseOrderItemSnapshot?> GetPurchaseFileItemSnapshotAsync(Guid purchaseFileItemId, CancellationToken ct);
    Task<PurchaseOrderItemSnapshot?> GetContractItemSnapshotAsync(Guid contractItemId, CancellationToken ct);
    Task AddPurchaseOrderAsync(PurchaseOrder purchaseOrder, CancellationToken ct);
    Task AddPurchaseOrderDocumentAsync(PurchaseOrderDocument document, CancellationToken ct);
    Task<PagedResult<PurchaseOrderSummaryDto>> GetPurchaseOrdersAsync(PurchaseOrderListRequest request, CancellationToken ct);
    Task<PurchaseOrderDetailDto?> GetPurchaseOrderDetailAsync(Guid id, CancellationToken ct);
    Task<PurchaseOrderDetailDto?> GetPurchaseOrderDetailByNumberAsync(string purchaseOrderNumber, CancellationToken ct);
    Task<IReadOnlyList<PurchaseOrderSummaryDto>> GetPurchaseOrdersByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct);
    Task<IReadOnlyList<PurchaseOrderSummaryDto>> GetPurchaseOrdersBySupplierAsync(Guid supplierId, CancellationToken ct);
    Task<IReadOnlyList<PurchaseOrderSummaryDto>> GetPurchaseOrdersByContractAsync(Guid contractId, CancellationToken ct);
    Task<IReadOnlyList<PurchaseOrderDocumentDto>> GetDocumentsAsync(Guid purchaseOrderId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class PurchaseOrderNumberService(IPurchaseOrderRepository repository) : IPurchaseOrderNumberService
{
    public async Task<string> GenerateNextPurchaseOrderNumber(int year, CancellationToken ct = default)
    {
        if (year < 2000 || year > 9999) throw new ArgumentOutOfRangeException(nameof(year));
        var number = await repository.GenerateNextPurchaseOrderNumberAsync(year, ct);
        if (!number.StartsWith($"PO-{year:0000}-", StringComparison.Ordinal) || number.Length != 14)
            throw new PurchaseOrderValidationException("Generated purchase order number has invalid format.");
        return number;
    }
}

public sealed class PurchaseOrderEligibilityService(IPurchaseOrderRepository repository) : IPurchaseOrderEligibilityService
{
    public async Task EnsurePurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken ct = default)
    {
        if (!await repository.PurchaseFileExistsAsync(purchaseFileId, ct))
            throw new PurchaseOrderValidationException("Purchase file was not found.");
    }

    public async Task EnsureSupplierExistsAsync(Guid supplierId, CancellationToken ct = default)
    {
        if (supplierId == Guid.Empty || !await repository.SupplierExistsAsync(supplierId, ct))
            throw new PurchaseOrderValidationException("Supplier was not found.");
    }
}

public sealed class PurchaseOrderReportDataSourceBuilder(IPurchaseOrderRepository repository) : IPurchaseOrderReportDataSourceBuilder
{
    public Task<PurchaseOrderDetailDto?> BuildAsync(Guid purchaseOrderId, CancellationToken ct = default) =>
        repository.GetPurchaseOrderDetailAsync(purchaseOrderId, ct);
}

public sealed class PurchaseOrderCommandHandler(
    IPurchaseOrderRepository repository,
    IPurchaseOrderNumberService numberService,
    IPurchaseOrderEligibilityService eligibility,
    ICurrentUserService currentUser)
{
    public async Task<PurchaseOrderDetailDto> Handle(CreatePurchaseOrderCommand command, CancellationToken ct = default)
    {
        var request = command.Request;
        await eligibility.EnsurePurchaseFileExistsAsync(request.PurchaseFileId, ct);
        await eligibility.EnsureSupplierExistsAsync(request.SupplierId, ct);
        var po = await NewPurchaseOrder(request.PurchaseFileId, request.SupplierId, request.ContractId,
            request.TenderId, request.TenderBidId, request.Title, request.PurchaseOrderType, request.Currency,
            request.Description, request.TotalAmount, request.TaxAmount, request.DiscountAmount, request.FinalAmount,
            request.OrderDate, request.ExpectedDeliveryDate, request.DeliveryLocation, request.DeliveryTerms,
            request.PaymentTerms, request.WarrantyTerms, request.Notes, ct);
        await repository.AddPurchaseOrderAsync(po, ct);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(po.Id, ct);
    }

    public async Task<PurchaseOrderDetailDto> Handle(CreatePurchaseOrderFromContractCommand command, CancellationToken ct = default)
    {
        var snapshot = await repository.GetContractSnapshotAsync(command.ContractId, ct)
            ?? throw new PurchaseOrderValidationException("Contract was not found.");
        if (snapshot.Status is not ContractStatus.Approved and not ContractStatus.Signed and not ContractStatus.Active)
            throw new PurchaseOrderValidationException("Purchase order can be created only from approved or signed contracts.");
        var po = await NewPurchaseOrder(snapshot.PurchaseFileId, snapshot.SupplierId, snapshot.ContractId,
            snapshot.TenderId, snapshot.TenderBidId, command.Request.Title ?? $"سفارش خرید قرارداد {snapshot.ContractNumber}",
            snapshot.TenderId.HasValue ? PurchaseOrderType.TenderBased : PurchaseOrderType.ContractBased,
            snapshot.Currency, "سفارش خرید ایجادشده از قرارداد", snapshot.TotalAmount, snapshot.TaxAmount,
            null, snapshot.FinalAmount, command.Request.OrderDate, command.Request.ExpectedDeliveryDate ?? snapshot.DeliveryDeadline,
            command.Request.DeliveryLocation, snapshot.DeliveryTerms, snapshot.PaymentTerms, snapshot.WarrantyTerms,
            command.Request.Notes, ct);
        foreach (var item in snapshot.Items) po.AddItem(ToItem(po.Id, item));
        await repository.AddPurchaseOrderAsync(po, ct);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(po.Id, ct);
    }

    public async Task<PurchaseOrderDetailDto> Handle(CreatePurchaseOrderFromPurchaseFileCommand command, CancellationToken ct = default)
    {
        await eligibility.EnsurePurchaseFileExistsAsync(command.PurchaseFileId, ct);
        await eligibility.EnsureSupplierExistsAsync(command.Request.SupplierId, ct);
        var po = await NewPurchaseOrder(command.PurchaseFileId, command.Request.SupplierId, null, null, null,
            command.Request.Title, command.Request.PurchaseOrderType, command.Request.Currency,
            "سفارش خرید ایجادشده از پرونده خرید", null, null, null, null, command.Request.OrderDate,
            command.Request.ExpectedDeliveryDate, command.Request.DeliveryLocation, command.Request.DeliveryTerms,
            command.Request.PaymentTerms, command.Request.WarrantyTerms, command.Request.Notes, ct);
        var requestedItems = command.Request.Items;
        if (requestedItems is { Count: > 0 })
        {
            foreach (var request in requestedItems)
                po.AddItem(ToItem(po.Id, await SnapshotFromRequest(request, ct)));
        }
        else
        {
            foreach (var item in await repository.GetPurchaseFileItemSnapshotsAsync(command.PurchaseFileId, ct))
                po.AddItem(ToItem(po.Id, item));
        }
        await repository.AddPurchaseOrderAsync(po, ct);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(po.Id, ct);
    }

    public async Task<PurchaseOrderDetailDto> Handle(UpdatePurchaseOrderCommand command, CancellationToken ct = default)
    {
        var po = await RequiredPurchaseOrder(command.Id, true, ct);
        var r = command.Request;
        po.Update(r.Title, r.Description, r.PurchaseOrderType, r.Currency, r.TotalAmount, r.TaxAmount,
            r.DiscountAmount, r.FinalAmount, r.OrderDate, r.ExpectedDeliveryDate, r.DeliveryLocation,
            r.DeliveryTerms, r.PaymentTerms, r.WarrantyTerms, r.Notes,
            currentUser.IsSystemAdmin);
        await repository.SaveChangesAsync(ct);
        return await RequiredDetail(po.Id, ct);
    }

    public async Task<PurchaseOrderItemDto> Handle(AddPurchaseOrderItemCommand command, CancellationToken ct = default)
    {
        var po = await RequiredPurchaseOrder(command.PurchaseOrderId, true, ct);
        po.AddItem(ToItem(po.Id, await SnapshotFromRequest(command.Request, ct)),
            currentUser.IsSystemAdmin);
        await repository.SaveChangesAsync(ct);
        return (await RequiredDetail(po.Id, ct)).Items.OrderByDescending(x => x.Id).First();
    }

    public async Task Handle(RemovePurchaseOrderItemCommand command, CancellationToken ct = default)
    {
        var po = await RequiredPurchaseOrder(command.PurchaseOrderId, true, ct);
        po.RemoveItem(command.ItemId, currentUser.IsSystemAdmin);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(SubmitPurchaseOrderCommand command, CancellationToken ct = default)
    {
        var po = await RequiredPurchaseOrder(command.Id, true, ct);
        po.Submit(currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(ApprovePurchaseOrderCommand command, CancellationToken ct = default)
    {
        var po = await RequiredPurchaseOrder(command.Id, true, ct);
        po.Approve(currentUser.UserId, command.Comment);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(RejectPurchaseOrderCommand command, CancellationToken ct = default)
    {
        var po = await RequiredPurchaseOrder(command.Id, true, ct);
        po.Reject(currentUser.UserId, command.Comment);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(IssuePurchaseOrderCommand command, CancellationToken ct = default)
    {
        var po = await RequiredPurchaseOrder(command.Id, true, ct);
        po.Issue(currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(CancelPurchaseOrderCommand command, CancellationToken ct = default)
    {
        var po = await RequiredPurchaseOrder(command.Id, true, ct);
        po.Cancel(command.Reason, currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    private async Task<PurchaseOrder> NewPurchaseOrder(Guid purchaseFileId, Guid supplierId, Guid? contractId,
        Guid? tenderId, Guid? tenderBidId, string title, PurchaseOrderType type, string currency,
        string? description, decimal? totalAmount, decimal? taxAmount, decimal? discountAmount,
        decimal? finalAmount, DateTime? orderDate, DateTime? expectedDeliveryDate, string? deliveryLocation,
        string? deliveryTerms, string? paymentTerms, string? warrantyTerms, string? notes, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var number = await numberService.GenerateNextPurchaseOrderNumber(year, ct);
        return new PurchaseOrder(Guid.NewGuid(), number, purchaseFileId, supplierId, contractId, tenderId,
            tenderBidId, title, type, currency, currentUser.UserId, description, totalAmount, taxAmount,
            discountAmount, finalAmount, orderDate, expectedDeliveryDate, deliveryLocation, deliveryTerms,
            paymentTerms, warrantyTerms, notes);
    }

    private async Task<PurchaseOrder> RequiredPurchaseOrder(Guid id, bool includeDetails, CancellationToken ct) =>
        await repository.FindPurchaseOrderAsync(id, includeDetails, ct)
        ?? throw new PurchaseOrderNotFoundException("Purchase order was not found.");

    private async Task<PurchaseOrderDetailDto> RequiredDetail(Guid id, CancellationToken ct) =>
        await repository.GetPurchaseOrderDetailAsync(id, ct)
        ?? throw new PurchaseOrderNotFoundException("Purchase order was not found.");

    private async Task<PurchaseOrderItemSnapshot> SnapshotFromRequest(AddPurchaseOrderItemRequest request, CancellationToken ct)
    {
        if (request.ContractItemId.HasValue)
            return await repository.GetContractItemSnapshotAsync(request.ContractItemId.Value, ct) ?? FromRequest(request);
        if (request.PurchaseFileItemId.HasValue)
            return await repository.GetPurchaseFileItemSnapshotAsync(request.PurchaseFileItemId.Value, ct) ?? FromRequest(request);
        return FromRequest(request);
    }

    private static PurchaseOrderItemSnapshot FromRequest(AddPurchaseOrderItemRequest r) =>
        new(r.PurchaseFileItemId, r.ContractItemId, r.TenderBidItemId, r.MescItemId, r.MescCode,
            r.MescGeneralGroupCode, r.GeneralDescription, r.SpecificDescription, r.UnitOfMeasureId,
            r.OrderedQuantity, r.UnitPrice, r.ExpectedDeliveryDate, r.TechnicalDescription, r.Notes);

    private static PurchaseOrderItem ToItem(Guid purchaseOrderId, PurchaseOrderItemSnapshot s) =>
        new(Guid.NewGuid(), purchaseOrderId, s.PurchaseFileItemId, s.ContractItemId, s.TenderBidItemId,
            s.MescItemId, s.MescCode, s.MescGeneralGroupCode, s.GeneralDescription, s.SpecificDescription,
            s.UnitOfMeasureId, s.OrderedQuantity, s.UnitPrice, s.ExpectedDeliveryDate, s.TechnicalDescription, s.Notes);
}

public sealed class PurchaseOrderQueryHandler(IPurchaseOrderRepository repository)
{
    public Task<PagedResult<PurchaseOrderSummaryDto>> Handle(GetPurchaseOrdersQuery query, CancellationToken ct = default) =>
        repository.GetPurchaseOrdersAsync(query.Request, ct);
    public Task<PurchaseOrderDetailDto?> Handle(GetPurchaseOrderByIdQuery query, CancellationToken ct = default) =>
        repository.GetPurchaseOrderDetailAsync(query.Id, ct);
    public Task<PurchaseOrderDetailDto?> Handle(GetPurchaseOrderByNumberQuery query, CancellationToken ct = default) =>
        repository.GetPurchaseOrderDetailByNumberAsync(query.PurchaseOrderNumber, ct);
    public Task<IReadOnlyList<PurchaseOrderSummaryDto>> Handle(GetPurchaseOrdersByPurchaseFileQuery query, CancellationToken ct = default) =>
        repository.GetPurchaseOrdersByPurchaseFileAsync(query.PurchaseFileId, ct);
    public Task<IReadOnlyList<PurchaseOrderSummaryDto>> Handle(GetPurchaseOrdersBySupplierQuery query, CancellationToken ct = default) =>
        repository.GetPurchaseOrdersBySupplierAsync(query.SupplierId, ct);
    public Task<IReadOnlyList<PurchaseOrderSummaryDto>> Handle(GetPurchaseOrdersByContractQuery query, CancellationToken ct = default) =>
        repository.GetPurchaseOrdersByContractAsync(query.ContractId, ct);
    public Task<IReadOnlyList<PurchaseOrderDocumentDto>> Handle(GetPurchaseOrderDocumentsQuery query, CancellationToken ct = default) =>
        repository.GetDocumentsAsync(query.PurchaseOrderId, ct);
}

public sealed class PurchaseOrderValidationException(string message) : InvalidOperationException(message);
public sealed class PurchaseOrderNotFoundException(string message) : InvalidOperationException(message);

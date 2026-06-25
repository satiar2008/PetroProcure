using PetroProcure.Api.Security;
using PetroProcure.Application.Documents;
using PetroProcure.Application.Security;
using PetroProcure.Application.Warehouse;
using PetroProcure.Contracts.V1.Reports;
using PetroProcure.Contracts.V1.Warehouse;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Warehouse;
using PetroProcure.Reporting;

namespace PetroProcure.Api.Endpoints;

public static class WarehouseEndpoints
{
    public static IEndpointRouteBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        var warehouses = app.MapGroup("/api/warehouses").WithTags("Warehouses");
        warehouses.MapGet("/", async (string? searchTerm, bool? includeInactive, int? pageNumber, int? pageSize,
            WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetWarehousesQuery(new WarehouseListRequest(
                searchTerm, includeInactive ?? false, pageNumber ?? 1, pageSize ?? 50)), ct))
            .RequirePermission(ApplicationPermissions.WarehouseView);

        warehouses.MapGet("/{id:guid}", async (Guid id, WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetWarehouseByIdQuery(id), ct) is { } warehouse ? Results.Ok(warehouse) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.WarehouseView);

        warehouses.MapPost("/", async (CreateWarehouseRequest request, WarehouseCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateWarehouseCommand(request), ct);
            return Results.Created($"/api/warehouses/{result.Id}", result);
        }).RequirePermission(ApplicationPermissions.WarehouseManageWarehouses);

        warehouses.MapPut("/{id:guid}", async (Guid id, UpdateWarehouseRequest request, WarehouseCommandHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new UpdateWarehouseCommand(id, request), ct)))
            .RequirePermission(ApplicationPermissions.WarehouseManageWarehouses);

        var receipts = app.MapGroup("/api/warehouse-receipts").WithTags("Warehouse Receipts");
        receipts.MapGet("/", async (string? searchTerm, WarehouseReceiptStatus? status, string? receiptNumber,
            string? purchaseOrderNumber, string? purchaseFileNumber, Guid? supplierId, Guid? warehouseId,
            DateTime? receiptDateFrom, DateTime? receiptDateTo, string? sortBy, bool? sortDescending,
            int? pageNumber, int? pageSize, WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetWarehouseReceiptsQuery(new WarehouseReceiptListRequest(
                searchTerm, status, receiptNumber, purchaseOrderNumber, purchaseFileNumber, supplierId,
                warehouseId, receiptDateFrom, receiptDateTo, sortBy ?? "CreatedAt", sortDescending ?? true,
                pageNumber ?? 1, pageSize ?? 20)), ct))
            .RequirePermission(ApplicationPermissions.WarehouseReceiptView);

        receipts.MapGet("/{id:guid}", async (Guid id, WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetWarehouseReceiptByIdQuery(id), ct) is { } receipt ? Results.Ok(receipt) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.WarehouseReceiptView);

        receipts.MapGet("/by-number/{receiptNumber}", async (string receiptNumber, WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetWarehouseReceiptByNumberQuery(receiptNumber), ct) is { } receipt ? Results.Ok(receipt) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.WarehouseReceiptView);

        app.MapGet("/api/purchase-orders/{purchaseOrderId:guid}/warehouse-receipts", async (
            Guid purchaseOrderId, WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetWarehouseReceiptsByPurchaseOrderQuery(purchaseOrderId), ct))
            .RequirePermission(ApplicationPermissions.WarehouseReceiptView);

        app.MapGet("/api/purchase-files/{purchaseFileId:guid}/warehouse-receipts", async (
            Guid purchaseFileId, WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetWarehouseReceiptsByPurchaseFileQuery(purchaseFileId), ct))
            .RequirePermission(ApplicationPermissions.WarehouseReceiptView);

        receipts.MapPost("/", async (CreateWarehouseReceiptRequest request, WarehouseCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateWarehouseReceiptCommand(request), ct);
            return Results.Created($"/api/warehouse-receipts/{result.Receipt.Id}", result);
        }).RequirePermission(ApplicationPermissions.WarehouseReceiptCreate);

        receipts.MapPost("/from-purchase-order/{purchaseOrderId:guid}", async (
            Guid purchaseOrderId, CreateWarehouseReceiptFromPurchaseOrderRequest request,
            WarehouseCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateWarehouseReceiptFromPurchaseOrderCommand(purchaseOrderId, request), ct);
            return Results.Created($"/api/warehouse-receipts/{result.Receipt.Id}", result);
        }).RequirePermission(ApplicationPermissions.WarehouseReceiptCreate);

        receipts.MapPut("/{id:guid}", async (Guid id, UpdateWarehouseReceiptRequest request, WarehouseCommandHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new UpdateWarehouseReceiptCommand(id, request), ct)))
            .RequirePermission(ApplicationPermissions.WarehouseReceiptEdit);

        receipts.MapGet("/{id:guid}/items", async (Guid id, WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetWarehouseReceiptByIdQuery(id), ct) is { } receipt ? Results.Ok(receipt.Items) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.WarehouseReceiptView);

        receipts.MapPost("/{id:guid}/items", async (Guid id, AddWarehouseReceiptItemRequest request, WarehouseCommandHandler handler, CancellationToken ct) =>
            Results.Created($"/api/warehouse-receipts/{id}/items", await handler.Handle(new AddWarehouseReceiptItemCommand(id, request), ct)))
            .RequirePermission(ApplicationPermissions.WarehouseReceiptEdit);

        receipts.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, WarehouseCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new RemoveWarehouseReceiptItemCommand(id, itemId), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.WarehouseReceiptEdit);

        receipts.MapPost("/{id:guid}/submit", async (Guid id, SubmitWarehouseReceiptRequest request, WarehouseCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new SubmitWarehouseReceiptCommand(id, request.Comment), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.WarehouseReceiptSubmit);

        receipts.MapPost("/{id:guid}/approve", async (Guid id, ApproveWarehouseReceiptRequest request, WarehouseCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new ApproveWarehouseReceiptCommand(id, request.Comment), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.WarehouseReceiptApprove);

        receipts.MapPost("/{id:guid}/cancel", async (Guid id, CancelWarehouseReceiptRequest request, WarehouseCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new CancelWarehouseReceiptCommand(id, request.Reason), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.WarehouseReceiptCancel);

        receipts.MapPost("/{id:guid}/documents/upload", async (
            Guid id, IFormFile file, string? documentType, string? description,
            WarehouseQueryHandler query, IWarehouseRepository repository, IFileStorageService storage,
            ICurrentUserService currentUser, CancellationToken ct) =>
        {
            var detail = await query.Handle(new GetWarehouseReceiptByIdQuery(id), ct);
            if (detail is null) return Results.NotFound();
            await using var stream = file.OpenReadStream();
            var document = await storage.SaveFileAsync(detail.Receipt.PurchaseFileId, DocumentType.WarehouseReceipt,
                file.FileName, stream, currentUser.UserId, mimeType: file.ContentType, description: description,
                cancellationToken: ct);
            var receiptDocument = new WarehouseReceiptDocument(Guid.NewGuid(), id, document.Id,
                documentType ?? "WarehouseReceiptDocument", file.FileName, description, currentUser.UserId);
            await repository.AddReceiptDocumentAsync(receiptDocument, ct);
            await repository.SaveChangesAsync(ct);
            return Results.Created($"/api/warehouse-receipts/{id}/documents", new WarehouseReceiptDocumentDto(
                receiptDocument.Id, id, document.Id, receiptDocument.DocumentType, receiptDocument.OriginalFileName,
                receiptDocument.Description, receiptDocument.UploadedAt, receiptDocument.UploadedByUserId));
        }).DisableAntiforgery().RequirePermission(ApplicationPermissions.WarehouseReceiptManageDocuments);

        receipts.MapGet("/{id:guid}/documents", async (Guid id, WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetWarehouseReceiptByIdQuery(id), ct) is { } receipt ? Results.Ok(receipt.Documents) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.WarehouseReceiptView);

        receipts.MapGet("/{id:guid}/documents/{documentId:guid}/download", async (
            Guid id, Guid documentId, WarehouseQueryHandler query, IFileStorageService storage, CancellationToken ct) =>
        {
            var document = (await query.Handle(new GetWarehouseReceiptByIdQuery(id), ct))?.Documents.SingleOrDefault(x => x.Id == documentId);
            if (document?.FileDocumentId is not Guid fileDocumentId) return Results.NotFound();
            var content = await storage.OpenFileAsync(fileDocumentId, ct);
            return Results.File(content.Stream, content.MimeType, content.OriginalFileName);
        }).RequirePermission(ApplicationPermissions.WarehouseReceiptView);

        receipts.MapDelete("/{id:guid}/documents/{documentId:guid}", async (
            Guid id, Guid documentId, WarehouseQueryHandler query, IFileStorageService storage, CancellationToken ct) =>
        {
            var document = (await query.Handle(new GetWarehouseReceiptByIdQuery(id), ct))?.Documents.SingleOrDefault(x => x.Id == documentId);
            if (document?.FileDocumentId is Guid fileDocumentId) await storage.DeleteFileAsync(fileDocumentId, ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.WarehouseReceiptManageDocuments);

        receipts.MapGet("/{id:guid}/reports/receipt/pdf", async (Guid id, IReportGenerator reports, CancellationToken ct) =>
        {
            var bytes = await reports.GeneratePdfAsync(ReportNames.WarehouseReceipt,
                new Dictionary<string, object?> { ["WarehouseReceiptId"] = id }, ct);
            return Results.File(bytes, "application/pdf", $"WarehouseReceipt-{id:N}.pdf");
        }).RequirePermission(ApplicationPermissions.WarehouseReceiptReportView);

        receipts.MapPost("/{id:guid}/reports/receipt/save-to-file", async (
            Guid id, WarehouseQueryHandler query, IReportGenerator reports, CancellationToken ct) =>
        {
            var receipt = await query.Handle(new GetWarehouseReceiptByIdQuery(id), ct);
            if (receipt is null) return Results.NotFound();
            var document = await reports.SaveGeneratedReportToPurchaseFileAsync(receipt.Receipt.PurchaseFileId,
                ReportNames.WarehouseReceipt, new Dictionary<string, object?>
                {
                    ["WarehouseReceiptId"] = id,
                    ["ReceiptNumber"] = receipt.Receipt.ReceiptNumber
                }, ct);
            return Results.Ok(new GeneratedReportResultDto(document.Id, document.PurchaseFileId,
                document.OriginalFileName, document.RelativePath, ReportNames.WarehouseReceipt, document.UploadedAt));
        }).RequirePermission(ApplicationPermissions.WarehouseReceiptReportExport);

        var inventory = app.MapGroup("/api/inventory").WithTags("Inventory");
        inventory.MapGet("/stock-balances", async (string? searchTerm, Guid? warehouseId, bool? lowStockOnly,
            string? mescGeneralGroupCode, int? pageNumber, int? pageSize, WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetStockBalancesQuery(new StockBalanceListRequest(
                searchTerm, warehouseId, lowStockOnly ?? false, mescGeneralGroupCode, pageNumber ?? 1, pageSize ?? 20)), ct))
            .RequirePermission(ApplicationPermissions.InventoryViewStockBalance);

        inventory.MapGet("/transactions", async (string? searchTerm, Guid? warehouseId,
            InventoryTransactionType? transactionType, DateTime? dateFrom, DateTime? dateTo,
            int? pageNumber, int? pageSize, WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetInventoryTransactionsQuery(new InventoryTransactionListRequest(
                searchTerm, warehouseId, transactionType, dateFrom, dateTo, pageNumber ?? 1, pageSize ?? 20)), ct))
            .RequirePermission(ApplicationPermissions.InventoryViewTransactions);

        app.MapGet("/api/purchase-orders/waiting-for-receipt", async (WarehouseQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetIssuedPurchaseOrdersWaitingForReceiptQuery(), ct))
            .RequirePermission(ApplicationPermissions.WarehouseReceiptCreate);

        return app;
    }
}

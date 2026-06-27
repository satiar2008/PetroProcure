using PetroProcure.Api.Security;
using PetroProcure.Application.Documents;
using PetroProcure.Application.PurchaseOrders;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.PurchaseOrders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.PurchaseOrders;
using PetroProcure.Reporting;

namespace PetroProcure.Api.Endpoints;

public static class PurchaseOrderEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/purchase-orders").WithTags("Purchase Orders");

        orders.MapGet("/", async (
            string? searchTerm, PurchaseOrderStatus? status, PurchaseOrderType? purchaseOrderType,
            Guid? supplierId, string? contractNumber, string? purchaseFileNumber, DateTime? dateFrom,
            DateTime? dateTo, string? sortBy, bool? sortDescending, int? pageNumber, int? pageSize,
            PurchaseOrderQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseOrdersQuery(new PurchaseOrderListRequest(
                searchTerm, status, purchaseOrderType, supplierId, contractNumber, purchaseFileNumber,
                dateFrom, dateTo, sortBy ?? "CreatedAt", sortDescending ?? true, pageNumber ?? 1, pageSize ?? 20)), ct))
            .RequirePermission(ApplicationPermissions.PurchaseOrderView);

        orders.MapGet("/{id:guid}", async (Guid id, PurchaseOrderQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseOrderByIdQuery(id), ct) is { } po ? Results.Ok(po) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.PurchaseOrderView);

        orders.MapGet("/by-number/{purchaseOrderNumber}", async (string purchaseOrderNumber, PurchaseOrderQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseOrderByNumberQuery(purchaseOrderNumber), ct) is { } po ? Results.Ok(po) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.PurchaseOrderView);

        app.MapGet("/api/purchase-files/{purchaseFileId:guid}/purchase-orders", async (
            Guid purchaseFileId, PurchaseOrderQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseOrdersByPurchaseFileQuery(purchaseFileId), ct))
            .RequirePermission(ApplicationPermissions.PurchaseOrderView);

        app.MapGet("/api/suppliers/{supplierId:guid}/purchase-orders", async (
            Guid supplierId, PurchaseOrderQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseOrdersBySupplierQuery(supplierId), ct))
            .RequirePermission(ApplicationPermissions.PurchaseOrderView);

        app.MapGet("/api/contracts/{contractId:guid}/purchase-orders", async (
            Guid contractId, PurchaseOrderQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseOrdersByContractQuery(contractId), ct))
            .RequirePermission(ApplicationPermissions.PurchaseOrderView);

        orders.MapPost("/", async (CreatePurchaseOrderRequest request, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () =>
            {
                var result = await handler.Handle(new CreatePurchaseOrderCommand(request), ct);
                return Results.Created($"/api/purchase-orders/{result.PurchaseOrder.Id}", result);
            })).RequirePermission(ApplicationPermissions.PurchaseOrderCreate);

        orders.MapPost("/from-contract/{contractId:guid}", async (
            Guid contractId, CreatePurchaseOrderFromContractRequest request, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () =>
            {
                var result = await handler.Handle(new CreatePurchaseOrderFromContractCommand(contractId, request), ct);
                return Results.Created($"/api/purchase-orders/{result.PurchaseOrder.Id}", result);
            })).RequirePermission(ApplicationPermissions.PurchaseOrderCreate);

        orders.MapPost("/from-purchase-file/{purchaseFileId:guid}", async (
            Guid purchaseFileId, CreatePurchaseOrderFromPurchaseFileRequest request, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () =>
            {
                var result = await handler.Handle(new CreatePurchaseOrderFromPurchaseFileCommand(purchaseFileId, request), ct);
                return Results.Created($"/api/purchase-orders/{result.PurchaseOrder.Id}", result);
            })).RequirePermission(ApplicationPermissions.PurchaseOrderCreate);

        orders.MapPut("/{id:guid}", async (Guid id, UpdatePurchaseOrderRequest request, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () => Results.Ok(await handler.Handle(new UpdatePurchaseOrderCommand(id, request), ct))))
            .RequirePermission(ApplicationPermissions.PurchaseOrderEdit);

        orders.MapPost("/{id:guid}/submit", async (Guid id, SubmitPurchaseOrderRequest request, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () => { await handler.Handle(new SubmitPurchaseOrderCommand(id, request.Comment), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.PurchaseOrderSubmit);
        orders.MapPost("/{id:guid}/approve", async (Guid id, ApprovePurchaseOrderRequest request, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () => { await handler.Handle(new ApprovePurchaseOrderCommand(id, request.Comment), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.PurchaseOrderApprove);
        orders.MapPost("/{id:guid}/reject", async (Guid id, RejectPurchaseOrderRequest request, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () => { await handler.Handle(new RejectPurchaseOrderCommand(id, request.Comment), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.PurchaseOrderReject);
        orders.MapPost("/{id:guid}/issue", async (Guid id, IssuePurchaseOrderRequest request, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () => { await handler.Handle(new IssuePurchaseOrderCommand(id, request.Comment), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.PurchaseOrderIssue);
        orders.MapPost("/{id:guid}/cancel", async (Guid id, CancelPurchaseOrderRequest request, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () => { await handler.Handle(new CancelPurchaseOrderCommand(id, request.Reason), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.PurchaseOrderCancel);

        orders.MapGet("/{id:guid}/items", async (Guid id, PurchaseOrderQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseOrderByIdQuery(id), ct) is { } po ? Results.Ok(po.Items) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.PurchaseOrderView);
        orders.MapPost("/{id:guid}/items", async (Guid id, AddPurchaseOrderItemRequest request, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () => Results.Created($"/api/purchase-orders/{id}/items", await handler.Handle(new AddPurchaseOrderItemCommand(id, request), ct))))
            .RequirePermission(ApplicationPermissions.PurchaseOrderManageItems);
        orders.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, PurchaseOrderCommandHandler handler, CancellationToken ct) =>
            await Execute(async () => { await handler.Handle(new RemovePurchaseOrderItemCommand(id, itemId), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.PurchaseOrderManageItems);

        orders.MapPost("/{id:guid}/documents/upload", async (
            Guid id, IFormFile file, string? documentType, string? description, PurchaseOrderQueryHandler query,
            IPurchaseOrderRepository repository, IFileStorageService storage, ICurrentUserService currentUser, CancellationToken ct) =>
        {
            var detail = await query.Handle(new GetPurchaseOrderByIdQuery(id), ct);
            if (detail is null) return Results.NotFound();
            await using var stream = file.OpenReadStream();
            var document = await storage.SaveFileAsync(detail.PurchaseOrder.PurchaseFileId, DocumentType.PurchaseOrder,
                file.FileName, stream, currentUser.UserId, mimeType: file.ContentType, description: description,
                cancellationToken: ct);
            var poDocument = new PurchaseOrderDocument(Guid.NewGuid(), id, document.Id,
                documentType ?? "PurchaseOrderDocument", file.FileName, description, currentUser.UserId);
            await repository.AddPurchaseOrderDocumentAsync(poDocument, ct);
            await repository.SaveChangesAsync(ct);
            return Results.Created($"/api/purchase-orders/{id}/documents", new PurchaseOrderDocumentDto(
                poDocument.Id, id, document.Id, poDocument.DocumentType, poDocument.OriginalFileName,
                poDocument.Description, poDocument.UploadedAt, poDocument.UploadedByUserId));
        }).DisableAntiforgery().RequirePermission(ApplicationPermissions.PurchaseOrderManageDocuments);

        orders.MapGet("/{id:guid}/documents", async (Guid id, PurchaseOrderQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseOrderDocumentsQuery(id), ct))
            .RequirePermission(ApplicationPermissions.PurchaseOrderView);

        orders.MapGet("/{id:guid}/documents/{documentId:guid}/download", async (
            Guid id, Guid documentId, PurchaseOrderQueryHandler query, IFileStorageService storage, CancellationToken ct) =>
        {
            var document = (await query.Handle(new GetPurchaseOrderDocumentsQuery(id), ct)).SingleOrDefault(x => x.Id == documentId);
            if (document?.FileDocumentId is not Guid fileDocumentId) return Results.NotFound();
            var content = await storage.OpenFileAsync(fileDocumentId, ct);
            return Results.File(content.Stream, content.MimeType, content.OriginalFileName);
        }).RequirePermission(ApplicationPermissions.PurchaseOrderView);

        orders.MapDelete("/{id:guid}/documents/{documentId:guid}", async (
            Guid id, Guid documentId, PurchaseOrderQueryHandler query, IFileStorageService storage, CancellationToken ct) =>
        {
            var document = (await query.Handle(new GetPurchaseOrderDocumentsQuery(id), ct)).SingleOrDefault(x => x.Id == documentId);
            if (document?.FileDocumentId is Guid fileDocumentId) await storage.DeleteFileAsync(fileDocumentId, ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.PurchaseOrderManageDocuments);

        orders.MapGet("/{id:guid}/reports/purchase-order/pdf", async (
            Guid id, IReportGenerator reports, CancellationToken ct) =>
        {
            var bytes = await reports.GeneratePdfAsync(ReportNames.PurchaseOrder,
                new Dictionary<string, object?> { ["PurchaseOrderId"] = id }, ct);
            return Results.File(bytes, "application/pdf", $"PurchaseOrder-{id:N}.pdf");
        }).RequirePermission(ApplicationPermissions.PurchaseOrderReportView);

        orders.MapPost("/{id:guid}/reports/purchase-order/save-to-file", async (
            Guid id, PurchaseOrderQueryHandler query, IReportGenerator reports, CancellationToken ct) =>
        {
            var po = await query.Handle(new GetPurchaseOrderByIdQuery(id), ct);
            if (po is null) return Results.NotFound();
            var result = await reports.SaveGeneratedReportToPurchaseFileAsync(po.PurchaseOrder.PurchaseFileId,
                ReportNames.PurchaseOrder, new Dictionary<string, object?>
                {
                    ["PurchaseOrderId"] = id,
                    ["PurchaseOrderNumber"] = po.PurchaseOrder.PurchaseOrderNumber
                }, ct);
            return Results.Ok(new { PurchaseOrderId = id, po.PurchaseOrder.PurchaseOrderNumber, result.OriginalFileName });
        }).RequirePermission(ApplicationPermissions.PurchaseOrderReportExport);

        return app;
    }

    private static async Task<IResult> Execute(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}

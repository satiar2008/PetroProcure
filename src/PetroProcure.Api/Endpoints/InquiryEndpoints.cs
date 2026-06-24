using PetroProcure.Api.Security;
using PetroProcure.Application.Inquiries;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Inquiry;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Api.Endpoints;

public static class InquiryEndpoints
{
    public static IEndpointRouteBuilder MapInquiryEndpoints(this IEndpointRouteBuilder app)
    {
        var inquiries = app.MapGroup("/api/inquiries").WithTags("Inquiry / RFQ");
        inquiries.MapGet("/", async (string? searchTerm, string? inquiryNumber, string? purchaseFileNumber,
            InquiryStatus? status, InquiryType? inquiryType, Guid? supplierId, DateTime? createdDateFrom,
            DateTime? createdDateTo, DateTime? deadlineDateFrom, DateTime? deadlineDateTo, string? sortBy,
            bool? sortDescending, int? pageNumber, int? pageSize, InquiryQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetInquiriesQuery(new InquiryListRequest(searchTerm, inquiryNumber, purchaseFileNumber,
                status, inquiryType, supplierId, createdDateFrom, createdDateTo, deadlineDateFrom, deadlineDateTo,
                sortBy ?? "CreatedAt", sortDescending ?? true, pageNumber ?? 1, pageSize ?? 20)), ct))
            .RequirePermission(ApplicationPermissions.InquiryView);
        inquiries.MapGet("/{id:guid}", async (Guid id, InquiryQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetInquiryByIdQuery(id), ct) is { } dto ? Results.Ok(dto) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.InquiryView);
        inquiries.MapGet("/by-number/{inquiryNumber}", async (string inquiryNumber, InquiryQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetInquiryByNumberQuery(inquiryNumber), ct) is { } dto ? Results.Ok(dto) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.InquiryView);
        app.MapGet("/api/purchase-files/{purchaseFileId:guid}/inquiries", async (Guid purchaseFileId, InquiryQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetInquiriesByPurchaseFileQuery(purchaseFileId), ct))
            .RequirePermission(ApplicationPermissions.InquiryView);

        inquiries.MapPost("/", async (CreateInquiryRequest r, InquiryCommandHandler h, CancellationToken ct) =>
            Results.Created("/api/inquiries", await h.Handle(new CreateInquiryCommand(r.PurchaseFileId, r.Title, r.InquiryType, r.DeadlineDate, r.Description), ct)))
            .RequirePermission(ApplicationPermissions.InquiryCreate);
        inquiries.MapPost("/from-purchase-file/{purchaseFileId:guid}", async (Guid purchaseFileId, CreateInquiryFromPurchaseFileRequest r, InquiryCommandHandler h, CancellationToken ct) =>
            Results.Created("/api/inquiries", await h.Handle(new CreateInquiryFromPurchaseFileCommand(purchaseFileId, r.Title, r.InquiryType, r.DeadlineDate, r.Description, r.PurchaseFileItemIds, r.SupplierIds), ct)))
            .RequirePermission(ApplicationPermissions.InquiryCreate);
        inquiries.MapPut("/{id:guid}", async (Guid id, UpdateInquiryRequest r, InquiryCommandHandler h, CancellationToken ct) =>
            Results.Ok(await h.Handle(new UpdateInquiryCommand(id, r.Title, r.InquiryType, r.DeadlineDate, r.Description), ct)))
            .RequirePermission(ApplicationPermissions.InquiryEdit);
        inquiries.MapPost("/{id:guid}/send", async (Guid id, InquiryCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new SendInquiryCommand(id), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.InquirySend);
        inquiries.MapPost("/{id:guid}/cancel", async (Guid id, CancelInquiryRequest r, InquiryCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new CancelInquiryCommand(id, r.Reason), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.InquiryCancel);
        inquiries.MapPost("/{id:guid}/close", async (Guid id, InquiryCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new CloseInquiryCommand(id), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.InquiryEdit);

        inquiries.MapGet("/{id:guid}/items", async (Guid id, InquiryQueryHandler h, CancellationToken ct) =>
            (await h.Handle(new GetInquiryByIdQuery(id), ct))?.Items ?? []).RequirePermission(ApplicationPermissions.InquiryView);
        inquiries.MapGet("/{id:guid}/items/grouped", async (Guid id, InquiryQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetInquiryItemsGroupedByMescQuery(id), ct)).RequirePermission(ApplicationPermissions.InquiryView);
        inquiries.MapPost("/{id:guid}/items", async (Guid id, AddInquiryItemRequest r, InquiryCommandHandler h, CancellationToken ct) =>
            Results.Created($"/api/inquiries/{id}/items", await h.Handle(new AddInquiryItemCommand(id, r.PurchaseFileItemId), ct)))
            .RequirePermission(ApplicationPermissions.InquiryEdit);
        inquiries.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, InquiryCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new RemoveInquiryItemCommand(id, itemId), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.InquiryEdit);

        inquiries.MapGet("/{id:guid}/suppliers", async (Guid id, InquiryQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetInquirySuppliersQuery(id), ct)).RequirePermission(ApplicationPermissions.InquiryView);
        inquiries.MapPost("/{id:guid}/suppliers", async (Guid id, AddInquirySupplierRequest r, InquiryCommandHandler h, CancellationToken ct) =>
            Results.Created($"/api/inquiries/{id}/suppliers", await h.Handle(new AddInquirySupplierCommand(id, r.SupplierId, r.ContactId), ct)))
            .RequirePermission(ApplicationPermissions.InquiryManageSuppliers);
        inquiries.MapDelete("/{id:guid}/suppliers/{inquirySupplierId:guid}", async (Guid id, Guid inquirySupplierId, InquiryCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new RemoveInquirySupplierCommand(id, inquirySupplierId), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.InquiryManageSuppliers);

        inquiries.MapGet("/{id:guid}/quotes", async (Guid id, InquiryQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetSupplierQuotesQuery(id), ct)).RequirePermission(ApplicationPermissions.InquiryView);
        inquiries.MapGet("/{id:guid}/comparison", async (Guid id, InquiryQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetInquiryComparisonQuery(id), ct) is { } dto ? Results.Ok(dto) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.InquiryCompareQuotes);
        inquiries.MapPost("/{id:guid}/quotes", async (Guid id, AddSupplierQuoteRequest r, InquiryCommandHandler h, CancellationToken ct) =>
            Results.Created($"/api/inquiries/{id}/quotes", await h.Handle(new AddSupplierQuoteCommand(id, r.InquirySupplierId, r.QuoteNumber, r.QuoteDate, r.ValidUntil, r.Currency, r.DeliveryTerms, r.PaymentTerms, r.DeliveryDate, r.TotalAmount, r.TaxAmount, r.DiscountAmount, r.TechnicalNote, r.CommercialNote), ct)))
            .RequirePermission(ApplicationPermissions.InquiryReceiveQuote);
        inquiries.MapPost("/{id:guid}/quotes/{quoteId:guid}/items", async (Guid id, Guid quoteId, AddSupplierQuoteItemRequest r, InquiryCommandHandler h, CancellationToken ct) =>
            Results.Created($"/api/inquiries/{id}/quotes/{quoteId}/items", await h.Handle(new AddSupplierQuoteItemCommand(id, quoteId, r.InquiryItemId, r.Quantity, r.UnitPrice, r.DeliveryDate, r.TechnicalComplianceStatus, r.TechnicalNote), ct)))
            .RequirePermission(ApplicationPermissions.InquiryReceiveQuote);
        inquiries.MapPost("/{id:guid}/quotes/{quoteId:guid}/select", async (Guid id, Guid quoteId, SelectSupplierQuoteRequest r, InquiryCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new SelectSupplierQuoteCommand(id, quoteId, r.Reason), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.InquirySelectSupplier);
        inquiries.MapPost("/{id:guid}/quotes/{quoteId:guid}/reject", async (Guid id, Guid quoteId, RejectSupplierQuoteRequest r, InquiryCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new RejectSupplierQuoteCommand(id, quoteId, r.Reason), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.InquiryReceiveQuote);
        return app;
    }
}

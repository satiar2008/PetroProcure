using PetroProcure.Api.Security;
using PetroProcure.Application.Security;
using PetroProcure.Application.Tenders;
using PetroProcure.Contracts.V1.Tenders;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Api.Endpoints;

public static class TenderEndpoints
{
    public static IEndpointRouteBuilder MapTenderEndpoints(this IEndpointRouteBuilder app)
    {
        var tenders = app.MapGroup("/api/tenders").WithTags("Tenders");

        tenders.MapGet("/", async (string? searchTerm, string? tenderNumber, string? purchaseFileNumber,
            TenderStatus? status, TenderType? tenderType, Guid? supplierId, DateTime? createdDateFrom,
            DateTime? createdDateTo, DateTime? submissionDeadlineFrom, DateTime? submissionDeadlineTo,
            string? sortBy, bool? sortDescending, int? pageNumber, int? pageSize, TenderQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetTendersQuery(new TenderListRequest(searchTerm, tenderNumber, purchaseFileNumber,
                status, tenderType, supplierId, createdDateFrom, createdDateTo, submissionDeadlineFrom,
                submissionDeadlineTo, sortBy ?? "CreatedAt", sortDescending ?? true, pageNumber ?? 1, pageSize ?? 20)), ct))
            .RequirePermission(ApplicationPermissions.TenderView);

        tenders.MapGet("/{id:guid}", async (Guid id, TenderQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetTenderByIdQuery(id), ct) is { } dto ? Results.Ok(dto) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.TenderView);
        tenders.MapGet("/by-number/{tenderNumber}", async (string tenderNumber, TenderQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetTenderByNumberQuery(tenderNumber), ct) is { } dto ? Results.Ok(dto) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.TenderView);
        app.MapGet("/api/purchase-files/{purchaseFileId:guid}/tenders", async (Guid purchaseFileId, TenderQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetTendersByPurchaseFileQuery(purchaseFileId), ct)).RequirePermission(ApplicationPermissions.TenderView);

        tenders.MapPost("/", async (CreateTenderRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created("/api/tenders", await h.Handle(new CreateTenderCommand(r.PurchaseFileId, r.Title, r.TenderType, r.SubmissionDeadline, r.OpeningDate, r.Description), ct))))
            .RequirePermission(ApplicationPermissions.TenderCreate);
        tenders.MapPost("/from-purchase-file/{purchaseFileId:guid}", async (Guid purchaseFileId, CreateTenderFromPurchaseFileRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created("/api/tenders", await h.Handle(new CreateTenderFromPurchaseFileCommand(purchaseFileId, r.Title, r.TenderType, r.SubmissionDeadline, r.OpeningDate, r.Description, r.PurchaseFileItemIds, r.SupplierIds), ct))))
            .RequirePermission(ApplicationPermissions.TenderCreate);
        tenders.MapPost("/from-inquiry/{inquiryId:guid}", async (Guid inquiryId, CreateTenderFromInquiryRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created("/api/tenders", await h.Handle(new CreateTenderFromInquiryCommand(inquiryId, r.Title, r.TenderType, r.SubmissionDeadline, r.OpeningDate, r.Description, r.InquirySupplierIds), ct))))
            .RequirePermission(ApplicationPermissions.TenderCreate);
        tenders.MapPut("/{id:guid}", async (Guid id, UpdateTenderRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Ok(await h.Handle(new UpdateTenderCommand(id, r.Title, r.TenderType, r.SubmissionDeadline, r.OpeningDate, r.Description), ct))))
            .RequirePermission(ApplicationPermissions.TenderEdit);
        tenders.MapPost("/{id:guid}/publish", async (Guid id, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new PublishTenderCommand(id), ct); return Results.NoContent(); })).RequirePermission(ApplicationPermissions.TenderPublish);
        tenders.MapPost("/{id:guid}/cancel", async (Guid id, CancelTenderRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new CancelTenderCommand(id, r.Reason), ct); return Results.NoContent(); })).RequirePermission(ApplicationPermissions.TenderCancel);
        tenders.MapPost("/{id:guid}/close", async (Guid id, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new CloseTenderCommand(id), ct); return Results.NoContent(); })).RequirePermission(ApplicationPermissions.TenderClose);

        tenders.MapGet("/{id:guid}/items", async (Guid id, TenderQueryHandler h, CancellationToken ct) =>
            (await h.Handle(new GetTenderByIdQuery(id), ct))?.Items ?? []).RequirePermission(ApplicationPermissions.TenderView);
        tenders.MapGet("/{id:guid}/items/grouped", async (Guid id, TenderQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetTenderItemsGroupedByMescQuery(id), ct)).RequirePermission(ApplicationPermissions.TenderView);
        tenders.MapPost("/{id:guid}/items", async (Guid id, AddTenderItemRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created($"/api/tenders/{id}/items", await h.Handle(new AddTenderItemCommand(id, r.PurchaseFileItemId), ct))))
            .RequirePermission(ApplicationPermissions.TenderManageItems);
        tenders.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new RemoveTenderItemCommand(id, itemId), ct); return Results.NoContent(); })).RequirePermission(ApplicationPermissions.TenderManageItems);

        tenders.MapGet("/{id:guid}/participants", async (Guid id, TenderQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetTenderParticipantsQuery(id), ct)).RequirePermission(ApplicationPermissions.TenderView);
        tenders.MapPost("/{id:guid}/participants", async (Guid id, AddTenderParticipantRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created($"/api/tenders/{id}/participants", await h.Handle(new AddTenderParticipantCommand(id, r.SupplierId, r.ContactId), ct))))
            .RequirePermission(ApplicationPermissions.TenderManageParticipants);
        tenders.MapDelete("/{id:guid}/participants/{participantId:guid}", async (Guid id, Guid participantId, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new RemoveTenderParticipantCommand(id, participantId), ct); return Results.NoContent(); })).RequirePermission(ApplicationPermissions.TenderManageParticipants);

        tenders.MapGet("/{id:guid}/bids", async (Guid id, TenderQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetTenderBidsQuery(id), ct)).RequirePermission(ApplicationPermissions.TenderView);
        tenders.MapPost("/{id:guid}/bids", async (Guid id, AddTenderBidRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created($"/api/tenders/{id}/bids", await h.Handle(new AddTenderBidCommand(id, r.TenderParticipantId, r.BidNumber, r.Currency, r.TotalAmount, r.FinalAmount, r.DeliveryTerms, r.PaymentTerms, r.ValidUntil, r.Notes), ct))))
            .RequirePermission(ApplicationPermissions.TenderReceiveBid);
        tenders.MapPut("/{id:guid}/bids/{bidId:guid}", async (Guid id, Guid bidId, UpdateTenderBidRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Ok(await h.Handle(new UpdateTenderBidCommand(id, bidId, r.BidNumber, r.Currency, r.TotalAmount, r.FinalAmount, r.DeliveryTerms, r.PaymentTerms, r.ValidUntil, r.Notes), ct))))
            .RequirePermission(ApplicationPermissions.TenderReceiveBid);
        tenders.MapPost("/{id:guid}/bids/{bidId:guid}/items", async (Guid id, Guid bidId, AddTenderBidItemRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created($"/api/tenders/{id}/bids/{bidId}/items", await h.Handle(new AddTenderBidItemCommand(id, bidId, r.TenderItemId, r.Quantity, r.UnitPrice, r.TechnicalComplianceStatus, r.TechnicalNote), ct))))
            .RequirePermission(ApplicationPermissions.TenderReceiveBid);
        tenders.MapGet("/{id:guid}/comparison", async (Guid id, TenderQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetTenderComparisonQuery(id), ct) is { } dto ? Results.Ok(dto) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.TenderCompareBids);
        tenders.MapPost("/{id:guid}/evaluations", async (Guid id, AddTenderEvaluationRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created($"/api/tenders/{id}/evaluations", await h.Handle(new AddTenderEvaluationCommand(id, r.TenderBidId, r.EvaluationType, r.Score, r.Result, r.Notes), ct))))
            .RequirePermission(ApplicationPermissions.TenderEvaluate);
        tenders.MapPost("/{id:guid}/select-winner", async (Guid id, SelectTenderWinnerRequest r, TenderCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new SelectTenderWinnerCommand(id, r.TenderBidId, r.Reason, r.Notes), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.TenderSelectWinner);
        return app;
    }

    private static async Task<IResult> Execute(Func<Task<IResult>> action)
    {
        try { return await action(); }
        catch (TenderValidationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    }
}

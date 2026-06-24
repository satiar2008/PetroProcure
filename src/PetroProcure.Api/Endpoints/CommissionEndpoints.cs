using PetroProcure.Api.Security;
using PetroProcure.Application.Commission;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Commission;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Api.Endpoints;

public static class CommissionEndpoints
{
    public static IEndpointRouteBuilder MapCommissionEndpoints(this IEndpointRouteBuilder app)
    {
        var sessions = app.MapGroup("/api/commission/sessions").WithTags("Tender Commission Sessions");

        sessions.MapGet("/", async (string? searchTerm, string? sessionNumber, string? tenderNumber,
            string? purchaseFileNumber, string? status, DateTime? sessionDateFrom, DateTime? sessionDateTo,
            Guid? memberUserId, string? sortBy, bool? sortDescending, int? pageNumber, int? pageSize,
            CommissionQueryHandler h, CancellationToken ct) =>
        {
            if (!TryParseNullableEnum<TenderCommissionSessionStatus>(status, out var parsedStatus))
                return Results.BadRequest(new { error = "وضعیت جلسه کمیسیون نامعتبر است." });
            return Results.Ok(await h.Handle(new GetCommissionSessionsQuery(new CommissionSessionListRequest(
                searchTerm, sessionNumber, tenderNumber, purchaseFileNumber, parsedStatus, sessionDateFrom,
                sessionDateTo, memberUserId, sortBy ?? "SessionDate", sortDescending ?? true,
                pageNumber ?? 1, pageSize ?? 20)), ct));
        }).RequirePermission(ApplicationPermissions.CommissionView);

        sessions.MapGet("/{id:guid}", async (Guid id, CommissionQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetCommissionSessionByIdQuery(id), ct) is { } dto ? Results.Ok(dto) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.CommissionView);
        sessions.MapGet("/by-number/{sessionNumber}", async (string sessionNumber, CommissionQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetCommissionSessionByNumberQuery(sessionNumber), ct) is { } dto ? Results.Ok(dto) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.CommissionView);
        app.MapGet("/api/tenders/{tenderId:guid}/commission-sessions", async (Guid tenderId, CommissionQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetCommissionSessionsByTenderQuery(tenderId), ct)).RequirePermission(ApplicationPermissions.CommissionView);
        app.MapGet("/api/purchase-files/{purchaseFileId:guid}/commission-sessions", async (Guid purchaseFileId, CommissionQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetCommissionSessionsByPurchaseFileQuery(purchaseFileId), ct)).RequirePermission(ApplicationPermissions.CommissionView);

        sessions.MapPost("/", async (CreateCommissionSessionRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created("/api/commission/sessions", await h.Handle(new CreateCommissionSessionCommand(r.TenderId, r.Title, r.SessionDate, r.Location, r.Description), ct))))
            .RequirePermission(ApplicationPermissions.CommissionCreate);
        sessions.MapPost("/from-tender/{tenderId:guid}", async (Guid tenderId, CreateCommissionSessionFromTenderRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created("/api/commission/sessions", await h.Handle(new CreateCommissionSessionFromTenderCommand(tenderId, r.Title, r.SessionDate, r.Location, r.Description, r.Members, r.AgendaItems), ct))))
            .RequirePermission(ApplicationPermissions.CommissionCreate);
        sessions.MapPut("/{id:guid}", async (Guid id, UpdateCommissionSessionRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Ok(await h.Handle(new UpdateCommissionSessionCommand(id, r.Title, r.SessionDate, r.Location, r.Description), ct))))
            .RequirePermission(ApplicationPermissions.CommissionEdit);
        sessions.MapPost("/{id:guid}/schedule", async (Guid id, ScheduleCommissionSessionRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new ScheduleCommissionSessionCommand(id, r.SessionDate, r.Location), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.CommissionSchedule);
        sessions.MapPost("/{id:guid}/start", async (Guid id, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new StartCommissionSessionCommand(id), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.CommissionStart);
        sessions.MapPost("/{id:guid}/complete", async (Guid id, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new CompleteCommissionSessionCommand(id), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.CommissionComplete);
        sessions.MapPost("/{id:guid}/approve", async (Guid id, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new ApproveCommissionSessionCommand(id), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.CommissionApprove);
        sessions.MapPost("/{id:guid}/cancel", async (Guid id, CancelCommissionSessionRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new CancelCommissionSessionCommand(id, r.Reason), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.CommissionCancel);

        sessions.MapGet("/{id:guid}/members", async (Guid id, CommissionQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetCommissionSessionMembersQuery(id), ct)).RequirePermission(ApplicationPermissions.CommissionView);
        sessions.MapPost("/{id:guid}/members", async (Guid id, AddCommissionMemberRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created($"/api/commission/sessions/{id}/members", await h.Handle(new AddCommissionMemberCommand(id, r.UserId, r.Role), ct))))
            .RequirePermission(ApplicationPermissions.CommissionManageMembers);
        sessions.MapPut("/{id:guid}/members/{memberId:guid}", async (Guid id, Guid memberId, UpdateCommissionMemberRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Ok(await h.Handle(new UpdateCommissionMemberCommand(id, memberId, r.Role, r.AttendanceStatus, r.VoteStatus, r.VoteNote), ct))))
            .RequirePermission(ApplicationPermissions.CommissionManageMembers);
        sessions.MapDelete("/{id:guid}/members/{memberId:guid}", async (Guid id, Guid memberId, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => { await h.Handle(new RemoveCommissionMemberCommand(id, memberId), ct); return Results.NoContent(); }))
            .RequirePermission(ApplicationPermissions.CommissionManageMembers);

        sessions.MapGet("/{id:guid}/agenda", async (Guid id, CommissionQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetCommissionSessionAgendaQuery(id), ct)).RequirePermission(ApplicationPermissions.CommissionView);
        sessions.MapPost("/{id:guid}/agenda", async (Guid id, AddAgendaItemRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created($"/api/commission/sessions/{id}/agenda", await h.Handle(new AddAgendaItemCommand(id, r.OrderNo, r.Title, r.Description, r.RelatedTenderBidId, r.RelatedSupplierId), ct))))
            .RequirePermission(ApplicationPermissions.CommissionManageAgenda);
        sessions.MapPut("/{id:guid}/agenda/{agendaItemId:guid}", async (Guid id, Guid agendaItemId, UpdateAgendaItemRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Ok(await h.Handle(new UpdateAgendaItemCommand(id, agendaItemId, r.OrderNo, r.Title, r.Description, r.Status, r.Notes), ct))))
            .RequirePermission(ApplicationPermissions.CommissionManageAgenda);

        sessions.MapGet("/{id:guid}/minutes", async (Guid id, CommissionQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetCommissionSessionMinutesQuery(id), ct)).RequirePermission(ApplicationPermissions.CommissionView);
        sessions.MapPost("/{id:guid}/minutes", async (Guid id, AddCommissionMinuteRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created($"/api/commission/sessions/{id}/minutes", await h.Handle(new AddCommissionMinuteCommand(id, r.AgendaItemId, r.Text), ct))))
            .RequirePermission(ApplicationPermissions.CommissionManageMinutes);
        sessions.MapPut("/{id:guid}/minutes/{minuteId:guid}", async (Guid id, Guid minuteId, UpdateCommissionMinuteRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Ok(await h.Handle(new UpdateCommissionMinuteCommand(id, minuteId, r.Text), ct))))
            .RequirePermission(ApplicationPermissions.CommissionManageMinutes);

        sessions.MapGet("/{id:guid}/decisions", async (Guid id, CommissionQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetCommissionSessionDecisionsQuery(id), ct)).RequirePermission(ApplicationPermissions.CommissionView);
        sessions.MapPost("/{id:guid}/decisions", async (Guid id, AddCommissionDecisionRequest r, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Created($"/api/commission/sessions/{id}/decisions", await h.Handle(new AddCommissionDecisionCommand(id, r.DecisionType, r.SelectedTenderBidId, r.SelectedSupplierId, r.DecisionText, r.Reason), ct))))
            .RequirePermission(ApplicationPermissions.CommissionManageDecisions);
        sessions.MapPost("/{id:guid}/decisions/{decisionId:guid}/approve", async (Guid id, Guid decisionId, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Ok(await h.Handle(new ApproveCommissionDecisionCommand(id, decisionId), ct))))
            .RequirePermission(ApplicationPermissions.CommissionManageDecisions);
        sessions.MapPost("/{id:guid}/decisions/{decisionId:guid}/reject", async (Guid id, Guid decisionId, CommissionCommandHandler h, CancellationToken ct) =>
            await Execute(async () => Results.Ok(await h.Handle(new RejectCommissionDecisionCommand(id, decisionId), ct))))
            .RequirePermission(ApplicationPermissions.CommissionManageDecisions);

        return app;
    }

    private static bool TryParseNullableEnum<TEnum>(string? value, out TEnum? result)
        where TEnum : struct
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (!Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)) return false;
        result = parsed;
        return true;
    }

    private static async Task<IResult> Execute(Func<Task<IResult>> action)
    {
        try { return await action(); }
        catch (CommissionValidationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    }
}

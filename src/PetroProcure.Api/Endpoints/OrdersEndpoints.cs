using PetroProcure.Api.Security;
using PetroProcure.Application.Orders;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Orders;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Api.Endpoints;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/orders").RequireAuthorization();

        orders.MapGet("/dashboard", async (OrdersQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetOrdersDashboardQuery(), ct)).RequirePermission(ApplicationPermissions.OrdersViewDashboard);

        orders.MapGet("/inventory-control", async ([AsParameters] InventoryControlListRequest r, OrdersQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetInventoryControlItemsQuery(r), ct)).RequirePermission(ApplicationPermissions.OrdersViewInventory);

        orders.MapPut("/inventory-control/{id:guid}", async (Guid id, UpdateInventoryControlItemRequest r, OrdersCommandHandler h, CancellationToken ct) =>
            Results.Ok(await h.Handle(new UpdateInventoryControlItemCommand(id, r.MinimumStockLevel, r.ReorderPoint, r.MaximumStockLevel, r.SafetyStock, r.IsStockControlled, r.IsActive, r.Notes), ct)))
            .RequirePermission(ApplicationPermissions.OrdersManageInventoryControl);

        orders.MapGet("/stock-balances", async ([AsParameters] StockBalanceListRequest r, OrdersQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetStockBalancesQuery(r), ct)).RequirePermission(ApplicationPermissions.OrdersViewInventory);

        orders.MapGet("/material-needs", async ([AsParameters] MaterialNeedListRequest r, OrdersQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetMaterialNeedsQuery(r), ct)).RequirePermission(ApplicationPermissions.OrdersCreateMaterialNeed);

        orders.MapGet("/material-needs/grouped", async ([AsParameters] MaterialNeedListRequest r, OrdersQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetMaterialNeedsGroupedByMescQuery(r), ct)).RequirePermission(ApplicationPermissions.OrdersCreateMaterialNeed);

        orders.MapGet("/material-needs/{id:guid}", async (Guid id, OrdersQueryHandler h, CancellationToken ct) =>
            await h.Handle(new GetMaterialNeedByIdQuery(id), ct) is { } dto ? Results.Ok(dto) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.OrdersCreateMaterialNeed);

        orders.MapPost("/material-needs", async (CreateMaterialNeedRequest r, OrdersCommandHandler h, CancellationToken ct) =>
            Results.Created("/api/orders/material-needs", await h.Handle(new CreateMaterialNeedCommand(r.MescItemId, r.RequestedQuantity, r.NeededByDate, r.SourceDepartmentId, r.ApplicantDepartmentId, r.Priority, r.Description), ct)))
            .RequirePermission(ApplicationPermissions.OrdersCreateMaterialNeed);

        orders.MapPost("/material-needs/{id:guid}/submit", async (Guid id, OrdersCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new SubmitMaterialNeedCommand(id), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.OrdersCreateMaterialNeed);

        orders.MapPost("/material-needs/{id:guid}/review", async (Guid id, ReviewMaterialNeedRequest r, OrdersCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new ReviewMaterialNeedCommand(id, r.Comment), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.OrdersReviewMaterialNeed);

        orders.MapPost("/material-needs/{id:guid}/approve", async (Guid id, ApproveMaterialNeedRequest r, OrdersCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new ApproveMaterialNeedCommand(id, r.Comment), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.OrdersApproveMaterialNeed);

        orders.MapPost("/material-needs/{id:guid}/reject", async (Guid id, RejectMaterialNeedRequest r, OrdersCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new RejectMaterialNeedCommand(id, r.Reason), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.OrdersApproveMaterialNeed);

        orders.MapPost("/material-needs/{id:guid}/convert-to-indent", async (Guid id, ConvertMaterialNeedToIndentRequest r, OrdersCommandHandler h, CancellationToken ct) =>
            Results.Ok(new { IndentId = await h.Handle(new ConvertMaterialNeedToIndentCommand(id, r.YearPart, r.TypeDigit, r.Title), ct) }))
            .RequirePermission(ApplicationPermissions.OrdersConvertNeedToIndent);

        orders.MapGet("/shortage-alerts", async (string? status, string? mescCode, int? pageNumber, int? pageSize, OrdersQueryHandler h, CancellationToken ct) =>
        {
            if (!TryParseNullableEnum<ShortageAlertStatus>(status, out var parsedStatus))
                return Results.BadRequest(new { error = "وضعیت هشدار کمبود نامعتبر است." });

            return Results.Ok(await h.Handle(new GetShortageAlertsQuery(new ShortageAlertListRequest(
                parsedStatus,
                string.IsNullOrWhiteSpace(mescCode) ? null : mescCode,
                pageNumber ?? 1,
                pageSize ?? 20)), ct));
        }).RequirePermission(ApplicationPermissions.OrdersManageShortageAlerts);

        orders.MapPost("/shortage-alerts/detect", async (DetectShortageAlertsRequest r, OrdersCommandHandler h, CancellationToken ct) =>
            Results.Ok(await h.Handle(new DetectShortageAlertsCommand(r.IncludeExistingOpen), ct)))
            .RequirePermission(ApplicationPermissions.OrdersManageShortageAlerts);

        orders.MapPost("/shortage-alerts/{id:guid}/convert-to-indent", async (Guid id, ConvertShortageToIndentRequest r, OrdersCommandHandler h, CancellationToken ct) =>
            Results.Ok(new { IndentId = await h.Handle(new ConvertShortageAlertToIndentCommand(id, r.YearPart, r.TypeDigit, r.RequestingDepartmentId, r.Title), ct) }))
            .RequirePermission(ApplicationPermissions.OrdersConvertShortageToIndent);

        orders.MapPost("/shortage-alerts/{id:guid}/resolve", async (Guid id, ResolveShortageAlertRequest r, OrdersCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new ResolveShortageAlertCommand(id, r.ResolutionNote), ct); return Results.NoContent(); }).RequirePermission(ApplicationPermissions.OrdersManageShortageAlerts);

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
}

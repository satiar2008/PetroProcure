using PetroProcure.Application.Mesc;
using PetroProcure.Application.Security;
using PetroProcure.Api.Security;
using PetroProcure.Contracts.V1.Mesc;
using PetroProcure.Api.Contracts;

namespace PetroProcure.Api.Endpoints;

public static class MescCatalogEndpoints
{
    public static IEndpointRouteBuilder MapMescCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var groups = app.MapGroup("/api/mesc/groups").WithTags("MESC Catalog");
        groups.MapGet("/", async (bool includeInactive, MescQueryHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetMescGeneralGroupsQuery(includeInactive), ct)).Select(x => x.ToContract()))
            .RequirePermission(ApplicationPermissions.ItemView);
        groups.MapPost("/", async (CreateMescGeneralGroupRequest request, MescCommandHandler handler, CancellationToken ct) =>
            await Execute(async () =>
        {
            var result = await handler.Handle(new CreateMescGeneralGroupCommand(request.Code, request.GeneralDescription), ct);
            return Results.Created($"/api/mesc/groups/{result.Id}", result.ToContract());
        })).RequirePermission(ApplicationPermissions.ItemCreate);
        groups.MapPut("/{id:guid}", async (Guid id, UpdateMescGeneralGroupRequest request, MescCommandHandler handler, CancellationToken ct) =>
            await Execute(async () => Results.Ok((await handler.Handle(new UpdateMescGeneralGroupCommand(id, request.Code, request.GeneralDescription), ct)).ToContract())))
            .RequirePermission(ApplicationPermissions.ItemEdit);

        var items = app.MapGroup("/api/mesc/items").WithTags("MESC Catalog");
        items.MapGet("/", async (bool includeInactive, MescQueryHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetMescItemsQuery(includeInactive), ct)).Select(x => x.ToContract())).RequirePermission(ApplicationPermissions.ItemView);
        items.MapGet("/search", async (string? term, bool includeInactive, MescQueryHandler handler, CancellationToken ct) =>
            (await handler.Handle(new SearchMescItemsQuery(term ?? string.Empty, includeInactive), ct)).Select(x => x.ToContract())).RequirePermission(ApplicationPermissions.ItemView);
        items.MapGet("/grouped", async (bool includeInactive, MescQueryHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetMescItemsGroupedByGeneralCodeQuery(includeInactive), ct)).Select(x => x.ToContract())).RequirePermission(ApplicationPermissions.ItemView);
        items.MapGet("/{code}", async (string code, bool includeInactive, MescQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetMescItemByCodeQuery(code, includeInactive), ct) is { } item
                ? Results.Ok(item.ToContract())
                : Results.NotFound()).RequirePermission(ApplicationPermissions.ItemView);
        items.MapPost("/", async (CreateMescItemRequest request, MescCommandHandler handler, CancellationToken ct) =>
            await Execute(async () =>
        {
            var result = await handler.Handle(new CreateMescItemCommand(
                request.Code, request.SpecificDescription, request.UnitOfMeasure, request.GeneralDescription, request.UnitOfMeasureId), ct);
            return Results.Created($"/api/mesc/items/{result.Code}", result.ToContract());
        })).RequirePermission(ApplicationPermissions.ItemCreate);
        items.MapPut("/{id:guid}", async (Guid id, UpdateMescItemRequest request, MescCommandHandler handler, CancellationToken ct) =>
            await Execute(async () => Results.Ok((await handler.Handle(new UpdateMescItemCommand(
                id, request.Code, request.SpecificDescription, request.UnitOfMeasure, request.GeneralDescription, request.UnitOfMeasureId), ct)).ToContract())))
            .RequirePermission(ApplicationPermissions.ItemEdit);
        items.MapPost("/{id:guid}/activate", async (Guid id, MescCommandHandler handler, CancellationToken ct) =>
            await Execute(async () =>
        {
            await handler.Handle(new ActivateMescItemCommand(id), ct);
            return Results.NoContent();
        })).RequirePermission(ApplicationPermissions.ItemActivateDeactivate);
        items.MapPost("/{id:guid}/deactivate", async (Guid id, MescCommandHandler handler, CancellationToken ct) =>
            await Execute(async () =>
        {
            await handler.Handle(new DeactivateMescItemCommand(id), ct);
            return Results.NoContent();
        })).RequirePermission(ApplicationPermissions.ItemActivateDeactivate);

        return app;
    }

    private static async Task<IResult> Execute(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (MescCatalogValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (MescCatalogNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (MescCatalogConflictException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}

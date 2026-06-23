using PetroProcure.Application.Indents;
using PetroProcure.Application.Security;
using PetroProcure.Api.Security;
using PetroProcure.Contracts.V1.Indents;
using PetroProcure.Api.Contracts;

namespace PetroProcure.Api.Endpoints;

public static class IndentEndpoints
{
    public static IEndpointRouteBuilder MapIndentEndpoints(this IEndpointRouteBuilder app)
    {
        var indents = app.MapGroup("/api/indents").WithTags("Indents");

        indents.MapGet("/", async (IndentQueryHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetIndentsQuery(), ct)).Select(x => x.ToContract())).RequirePermission(ApplicationPermissions.IndentView);
        indents.MapGet("/{id:guid}", async (Guid id, IndentQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetIndentByIdQuery(id), ct) is { } indent ? Results.Ok(indent.ToContract()) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.IndentView);
        indents.MapGet("/by-number/{indentNumber}", async (string indentNumber, IndentQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetIndentByNumberQuery(indentNumber), ct) is { } indent ? Results.Ok(indent.ToContract()) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.IndentView);
        indents.MapGet("/{id:guid}/items/grouped", async (Guid id, IndentQueryHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetIndentItemsGroupedByMescGeneralGroupQuery(id), ct)).Select(x => x.ToContract()))
            .RequirePermission(ApplicationPermissions.IndentView);

        indents.MapPost("/", async (CreateIndentRequest request, IndentCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateIndentCommand(
                request.YearPart, request.TypeDigit, request.Title, request.RequestingDepartmentId,
                request.ApplicantDepartmentId, request.Description), ct);
            return Results.Created($"/api/indents/{result.Id}", result.ToContract());
        }).RequirePermission(ApplicationPermissions.IndentCreate);
        indents.MapPost("/{id:guid}/items", async (Guid id, AddIndentItemRequest request, IndentCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new AddIndentItemCommand(
                id, request.MescItemId, request.UnitOfMeasureId, request.RequestedQuantity,
                request.TechnicalDescription, request.RequiredDate), ct);
            return Results.Created($"/api/indents/{id}/items/{result.Id}", result.ToContract());
        }).RequirePermission(ApplicationPermissions.IndentCreate);
        indents.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, IndentCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new RemoveIndentItemCommand(id, itemId), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.IndentCreate);
        indents.MapPost("/{id:guid}/submit", async (Guid id, IndentCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new SubmitIndentCommand(id), ct); return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.IndentCreate);
        indents.MapPost("/{id:guid}/approve", async (Guid id, IndentCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new ApproveIndentCommand(id), ct); return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.IndentApprove);
        indents.MapPost("/{id:guid}/reject", async (Guid id, IndentCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new RejectIndentCommand(id), ct); return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.IndentApprove);
        indents.MapPost("/{id:guid}/send-to-purchase", async (Guid id, IndentCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new SendIndentToPurchaseDepartmentCommand(id), ct); return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.IndentSendToPurchase);

        return app;
    }

}

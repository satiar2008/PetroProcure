using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Domain.Enums;
using PetroProcure.Application.Security;
using PetroProcure.Api.Security;
using PetroProcure.Contracts.V1.PurchaseFiles;
using PetroProcure.Api.Contracts;
using PetroProcure.Contracts.V1.Common;

namespace PetroProcure.Api.Endpoints;

public static class PurchaseFileEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseFileEndpoints(this IEndpointRouteBuilder app)
    {
        var files = app.MapGroup("/api/purchase-files").WithTags("Purchase Files");
        files.MapGet("/", async ([AsParameters] PurchaseFileListRequest request, PurchaseFileQueryHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetPurchaseFilesQuery(request), ct);
            return new PagedResult<PurchaseFileSummaryDto>(
                result.Items.Select(x => x.ToContract()).ToArray(),
                result.PageNumber, result.PageSize, result.TotalCount);
        })
            .RequirePermission(ApplicationPermissions.PurchaseFileView);
        files.MapGet("/{id:guid}", async (Guid id, PurchaseFileQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseFileByIdQuery(id), ct) is { } file ? Results.Ok(file.ToContract()) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.PurchaseFileView);
        files.MapGet("/by-number/{fileNumber}", async (string fileNumber, PurchaseFileQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseFileByNumberQuery(fileNumber), ct) is { } file ? Results.Ok(file.ToContract()) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.PurchaseFileView);
        files.MapGet("/{id:guid}/timeline", async (Guid id, PurchaseFileQueryHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetPurchaseFileTimelineQuery(id), ct)).ToContract()).RequirePermission(ApplicationPermissions.PurchaseFileView);
        files.MapGet("/{id:guid}/items/grouped", async (Guid id, PurchaseFileQueryHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetPurchaseFileItemsGroupedByMescGeneralGroupQuery(id), ct)).Select(x => x.ToContract())).RequirePermission(ApplicationPermissions.PurchaseFileView);

        files.MapPost("/", async (CreatePurchaseFileRequest request, PurchaseFileCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreatePurchaseFileCommand(
                request.Year, request.Title, request.Description, request.Priority, request.PurchaseDepartmentId,
                request.CurrentDepartmentId, request.ResponsibleUserId), ct);
            return Results.Created($"/api/purchase-files/{result.Id}", result.ToContract());
        }).RequirePermission(ApplicationPermissions.PurchaseFileCreate);
        files.MapPost("/from-indent/{indentId:guid}", async (Guid indentId, CreatePurchaseFileFromIndentRequest request, PurchaseFileCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreatePurchaseFileFromIndentCommand(
                indentId, request.Year, request.PurchaseDepartmentId, request.ResponsibleUserId,
                request.Priority), ct);
            return Results.Created($"/api/purchase-files/{result.Id}", result.ToContract());
        }).RequirePermission(ApplicationPermissions.PurchaseFileCreate);
        files.MapPost("/{id:guid}/items", async (Guid id, AddPurchaseFileItemRequest request, PurchaseFileCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new AddPurchaseFileItemCommand(
                id, request.MescItemId, request.UnitOfMeasureId, request.RequestedQuantity,
                request.ApprovedQuantity, request.TechnicalDescription), ct);
            return Results.Created($"/api/purchase-files/{id}/items/{result.Id}", result.ToContract());
        }).RequirePermission(ApplicationPermissions.PurchaseFileEdit);
        files.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, PurchaseFileCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new RemovePurchaseFileItemCommand(id, itemId), ct); return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.PurchaseFileEdit);
        files.MapPost("/{id:guid}/assign-department", async (Guid id, AssignPurchaseFileToDepartmentRequest request, PurchaseFileCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new AssignPurchaseFileToDepartmentCommand(
                id, request.DepartmentId, request.ResponsibleUserId, request.Reason), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.PurchaseFileSendToDepartment);
        files.MapPost("/{id:guid}/change-status", async (Guid id, ChangePurchaseFileStatusRequest request, PurchaseFileCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new ChangePurchaseFileStatusCommand(
                id, request.Status, request.Reason, request.DepartmentId), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.PurchaseFileEdit);
        files.MapPost("/{id:guid}/notes", async (Guid id, AddPurchaseFileNoteRequest request, PurchaseFileCommandHandler handler, CancellationToken ct) =>
            Results.Ok((await handler.Handle(new AddPurchaseFileNoteCommand(
                id, request.DepartmentId, request.NoteText, request.IsInternal), ct)).ToContract()))
            .RequirePermission(ApplicationPermissions.PurchaseFileEdit);
        files.MapPost("/{id:guid}/complete", async (Guid id, PurchaseFileLifecycleRequest request, PurchaseFileCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new CompletePurchaseFileCommand(id, request.Reason), ct); return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.PurchaseFileClose);
        files.MapPost("/{id:guid}/archive", async (Guid id, PurchaseFileLifecycleRequest request, PurchaseFileCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new ArchivePurchaseFileCommand(id, request.Reason), ct); return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.PurchaseFileArchive);
        return app;
    }

}

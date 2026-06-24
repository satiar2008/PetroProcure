using PetroProcure.Api.Security;
using PetroProcure.Application.Security;
using PetroProcure.Application.Suppliers;
using PetroProcure.Contracts.V1.Suppliers;

namespace PetroProcure.Api.Endpoints;

public static class SupplierEndpoints
{
    public static IEndpointRouteBuilder MapSupplierEndpoints(this IEndpointRouteBuilder app)
    {
        var suppliers = app.MapGroup("/api/suppliers").WithTags("Suppliers");

        suppliers.MapGet("/", async (
            string? searchTerm, Domain.Enums.SupplierStatus? status, Domain.Enums.SupplierType? supplierType,
            Guid? categoryId, bool? isActive, bool? isBlacklisted, string? city, bool? hasPrimaryContact,
            string? sortBy, bool? sortDescending, int? pageNumber, int? pageSize,
            SupplierQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetSuppliersQuery(new SupplierListRequest(
                searchTerm, status, supplierType, categoryId, isActive, isBlacklisted, city, hasPrimaryContact,
                sortBy ?? "Name", sortDescending ?? false, pageNumber ?? 1, pageSize ?? 20)), ct))
            .RequirePermission(ApplicationPermissions.SupplierView);

        suppliers.MapGet("/{id:guid}", async (Guid id, SupplierQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetSupplierByIdQuery(id), ct) is { } supplier ? Results.Ok(supplier) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.SupplierView);

        suppliers.MapGet("/by-code/{supplierCode}", async (string supplierCode, SupplierQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetSupplierByCodeQuery(supplierCode), ct) is { } supplier ? Results.Ok(supplier) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.SupplierView);

        suppliers.MapGet("/lookup", async (string? term, bool? includeInactive, bool? includeBlacklisted, SupplierQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetSupplierLookupQuery(term, includeInactive ?? false, includeBlacklisted ?? false), ct))
            .RequirePermission(ApplicationPermissions.SupplierView);

        suppliers.MapPost("/", async (CreateSupplierRequest request, SupplierCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                var result = await handler.Handle(new CreateSupplierCommand(
                    request.SupplierCode, request.Name, request.NationalId, request.EconomicCode, request.RegistrationNumber,
                    request.Phone, request.Email, request.Website, request.Address, request.City, request.Country,
                    request.PostalCode, request.SupplierType, request.Description, request.CategoryIds, request.PrimaryContact), ct);
                return Results.Created($"/api/suppliers/{result.Supplier.Id}", result);
            }
            catch (SupplierConflictException ex) { return Results.Conflict(new { error = ex.Message }); }
            catch (SupplierValidationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequirePermission(ApplicationPermissions.SupplierCreate);

        suppliers.MapPut("/{id:guid}", async (Guid id, UpdateSupplierRequest request, SupplierCommandHandler handler, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await handler.Handle(new UpdateSupplierCommand(
                    id, request.SupplierCode, request.Name, request.NationalId, request.EconomicCode, request.RegistrationNumber,
                    request.Phone, request.Email, request.Website, request.Address, request.City, request.Country,
                    request.PostalCode, request.SupplierType, request.Description), ct));
            }
            catch (SupplierNotFoundException) { return Results.NotFound(); }
            catch (SupplierConflictException ex) { return Results.Conflict(new { error = ex.Message }); }
            catch (SupplierValidationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequirePermission(ApplicationPermissions.SupplierEdit);

        suppliers.MapPost("/{id:guid}/activate", async (Guid id, SupplierCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new ActivateSupplierCommand(id), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.SupplierActivateDeactivate);
        suppliers.MapPost("/{id:guid}/deactivate", async (Guid id, SupplierCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new DeactivateSupplierCommand(id), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.SupplierActivateDeactivate);
        suppliers.MapPost("/{id:guid}/blacklist", async (Guid id, ChangeSupplierStatusRequest request, SupplierCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new BlacklistSupplierCommand(id, request.Reason ?? "ثبت در فهرست سیاه"), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.SupplierBlacklist);
        suppliers.MapPost("/{id:guid}/remove-from-blacklist", async (Guid id, SupplierCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new RemoveSupplierFromBlacklistCommand(id), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.SupplierBlacklist);

        suppliers.MapGet("/categories", async (bool? includeInactive, SupplierQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetSupplierCategoriesQuery(includeInactive ?? false), ct))
            .RequirePermission(ApplicationPermissions.SupplierView);

        suppliers.MapGet("/{id:guid}/contacts", async (Guid id, SupplierQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetSupplierContactsQuery(id), ct))
            .RequirePermission(ApplicationPermissions.SupplierView);
        suppliers.MapPost("/{id:guid}/contacts", async (Guid id, AddSupplierContactRequest request, SupplierCommandHandler handler, CancellationToken ct) =>
            Results.Created($"/api/suppliers/{id}/contacts", await handler.Handle(new AddSupplierContactCommand(
                id, request.FullName, request.Position, request.Phone, request.Mobile, request.Email, request.IsPrimary, request.Description), ct)))
            .RequirePermission(ApplicationPermissions.SupplierManageContacts);
        suppliers.MapPut("/{id:guid}/contacts/{contactId:guid}", async (Guid id, Guid contactId, UpdateSupplierContactRequest request, SupplierCommandHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new UpdateSupplierContactCommand(
                id, contactId, request.FullName, request.Position, request.Phone, request.Mobile, request.Email, request.IsPrimary, request.Description), ct)))
            .RequirePermission(ApplicationPermissions.SupplierManageContacts);
        suppliers.MapPost("/{id:guid}/contacts/{contactId:guid}/deactivate", async (Guid id, Guid contactId, SupplierCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new DeactivateSupplierContactCommand(id, contactId), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.SupplierManageContacts);

        suppliers.MapPost("/{id:guid}/categories", async (Guid id, AssignSupplierCategoryRequest request, SupplierCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new AssignSupplierCategoryCommand(id, request.CategoryId), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.SupplierManageCategories);
        suppliers.MapDelete("/{id:guid}/categories/{categoryId:guid}", async (Guid id, Guid categoryId, SupplierCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new RemoveSupplierCategoryCommand(id, categoryId), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.SupplierManageCategories);

        suppliers.MapGet("/{id:guid}/evaluations", async (Guid id, SupplierQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetSupplierEvaluationsQuery(id), ct))
            .RequirePermission(ApplicationPermissions.SupplierView);
        suppliers.MapPost("/{id:guid}/evaluations", async (Guid id, AddSupplierEvaluationRequest request, SupplierCommandHandler handler, CancellationToken ct) =>
            Results.Created($"/api/suppliers/{id}/evaluations", await handler.Handle(new AddSupplierEvaluationCommand(
                id, request.EvaluationDate, request.Score, request.Result, request.Description), ct)))
            .RequirePermission(ApplicationPermissions.SupplierEvaluate);

        return app;
    }
}

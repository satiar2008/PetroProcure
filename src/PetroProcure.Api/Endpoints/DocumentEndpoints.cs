using PetroProcure.Application.Documents;
using PetroProcure.Domain.Enums;
using PetroProcure.Application.Security;
using PetroProcure.Api.Security;
using PetroProcure.Api.Contracts;
using Microsoft.Extensions.Options;
using PetroProcure.Contracts.V1.Documents;

namespace PetroProcure.Api.Endpoints;

public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/documents/upload-limits", (IOptions<FileStorageOptions> options) =>
            new DocumentUploadLimitsDto(options.Value.MaxFileSizeMb, options.Value.AllowedExtensions, options.Value.AllowedMimeTypes))
            .RequirePermission(ApplicationPermissions.PurchaseFileView);
        app.MapPost("/api/purchase-files/{id:guid}/documents/upload", async (
            Guid id, IFormFile file, DocumentType documentType,
            Guid? departmentId, string? description, IFileStorageService storage,
            ICurrentUserService currentUser, CancellationToken ct) =>
        {
            await using var stream = file.OpenReadStream();
            var result = await storage.SaveFileAsync(
                id, documentType, file.FileName, stream, currentUser.UserId, departmentId,
                file.ContentType, description, ct);
            return Results.Created($"/api/documents/{result.Id}", result.ToContract());
        }).DisableAntiforgery().RequirePermission(ApplicationPermissions.PurchaseFileEdit);

        app.MapGet("/api/purchase-files/{id:guid}/documents", async (
            Guid id, bool includeDeleted, IFileStorageService storage, CancellationToken ct) =>
            (await storage.GetPurchaseFileDocumentsAsync(id, includeDeleted, ct)).Select(x => x.ToContract()))
            .RequirePermission(ApplicationPermissions.PurchaseFileView);

        app.MapGet("/api/documents/{documentId:guid}/download", async (
            Guid documentId, IFileStorageService storage, CancellationToken ct) =>
        {
            var content = await storage.OpenFileAsync(documentId, ct);
            return Results.File(content.Stream, content.MimeType, content.OriginalFileName);
        }).RequirePermission(ApplicationPermissions.PurchaseFileView);

        app.MapDelete("/api/documents/{documentId:guid}", async (
            Guid documentId, IFileStorageService storage, CancellationToken ct) =>
        {
            await storage.DeleteFileAsync(documentId, ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.PurchaseFileEdit);

        app.MapPost("/api/documents/{documentId:guid}/versions", async (
            Guid documentId, IFormFile file, IFileStorageService storage,
            ICurrentUserService currentUser, CancellationToken ct) =>
        {
            await using var stream = file.OpenReadStream();
            return Results.Ok((await storage.CreateNewVersionAsync(documentId, stream, currentUser.UserId, ct)).ToContract());
        }).DisableAntiforgery().RequirePermission(ApplicationPermissions.PurchaseFileEdit);

        return app;
    }
}

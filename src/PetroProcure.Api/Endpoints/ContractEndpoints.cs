using PetroProcure.Api.Security;
using PetroProcure.Application.Contracts;
using PetroProcure.Application.Documents;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Contracts;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Contracts;
using PetroProcure.Reporting;

namespace PetroProcure.Api.Endpoints;

public static class ContractEndpoints
{
    public static IEndpointRouteBuilder MapContractEndpoints(this IEndpointRouteBuilder app)
    {
        var contracts = app.MapGroup("/api/contracts").WithTags("Contracts");

        contracts.MapGet("/", async (
            string? searchTerm, ContractStatus? status, ContractType? contractType, Guid? supplierId,
            string? purchaseFileNumber, string? tenderNumber, DateTime? createdDateFrom, DateTime? createdDateTo,
            string? sortBy, bool? sortDescending, int? pageNumber, int? pageSize,
            ContractQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetContractsQuery(new ContractListRequest(
                searchTerm, status, contractType, supplierId, purchaseFileNumber, tenderNumber, createdDateFrom,
                createdDateTo, sortBy ?? "CreatedAt", sortDescending ?? true, pageNumber ?? 1, pageSize ?? 20)), ct))
            .RequirePermission(ApplicationPermissions.ContractView);

        contracts.MapGet("/{id:guid}", async (Guid id, ContractQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetContractByIdQuery(id), ct) is { } contract ? Results.Ok(contract) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.ContractView);

        contracts.MapGet("/by-number/{contractNumber}", async (string contractNumber, ContractQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetContractByNumberQuery(contractNumber), ct) is { } contract ? Results.Ok(contract) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.ContractView);

        app.MapGet("/api/purchase-files/{purchaseFileId:guid}/contracts", async (
            Guid purchaseFileId, ContractQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetContractsByPurchaseFileQuery(purchaseFileId), ct))
            .RequirePermission(ApplicationPermissions.ContractView);

        app.MapGet("/api/suppliers/{supplierId:guid}/contracts", async (
            Guid supplierId, ContractQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetContractsBySupplierQuery(supplierId), ct))
            .RequirePermission(ApplicationPermissions.ContractView);

        contracts.MapPost("/", async (CreateContractRequest request, ContractCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateContractCommand(request), ct);
            return Results.Created($"/api/contracts/{result.Contract.Id}", result);
        }).RequirePermission(ApplicationPermissions.ContractCreate);

        contracts.MapPost("/from-purchase-file/{purchaseFileId:guid}", async (
            Guid purchaseFileId, CreateContractFromPurchaseFileRequest request, ContractCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateContractFromPurchaseFileCommand(purchaseFileId, request), ct);
            return Results.Created($"/api/contracts/{result.Contract.Id}", result);
        }).RequirePermission(ApplicationPermissions.ContractCreate);

        contracts.MapPost("/from-tender/{tenderId:guid}", async (
            Guid tenderId, CreateContractFromTenderRequest request, ContractCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateContractFromTenderCommand(tenderId, request), ct);
            return Results.Created($"/api/contracts/{result.Contract.Id}", result);
        }).RequirePermission(ApplicationPermissions.ContractCreate);

        contracts.MapPost("/from-tender-bid/{tenderBidId:guid}", async (
            Guid tenderBidId, CreateContractFromTenderBidRequest request, ContractCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateContractFromTenderBidCommand(tenderBidId, request), ct);
            return Results.Created($"/api/contracts/{result.Contract.Id}", result);
        }).RequirePermission(ApplicationPermissions.ContractCreate);

        contracts.MapPut("/{id:guid}", async (Guid id, UpdateContractRequest request, ContractCommandHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new UpdateContractCommand(id, request), ct)))
            .RequirePermission(ApplicationPermissions.ContractEdit);

        contracts.MapPost("/{id:guid}/submit", async (Guid id, SubmitContractRequest request, ContractCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new SubmitContractCommand(id, request.Comment), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.ContractSubmit);
        contracts.MapPost("/{id:guid}/approve", async (Guid id, ApproveContractRequest request, ContractCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new ApproveContractCommand(id, request.Comment), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.ContractApprove);
        contracts.MapPost("/{id:guid}/reject", async (Guid id, RejectContractRequest request, ContractCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new RejectContractCommand(id, request.Comment), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.ContractReject);
        contracts.MapPost("/{id:guid}/sign", async (Guid id, SignContractRequest request, ContractCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new SignContractCommand(id, request.Comment), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.ContractSign);
        contracts.MapPost("/{id:guid}/cancel", async (Guid id, CancelContractRequest request, ContractCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new CancelContractCommand(id, request.Reason), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.ContractCancel);

        contracts.MapGet("/{id:guid}/items", async (Guid id, ContractQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetContractByIdQuery(id), ct) is { } contract ? Results.Ok(contract.Items) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.ContractView);
        contracts.MapPost("/{id:guid}/items", async (Guid id, AddContractItemRequest request, ContractCommandHandler handler, CancellationToken ct) =>
            Results.Created($"/api/contracts/{id}/items", await handler.Handle(new AddContractItemCommand(id, request), ct)))
            .RequirePermission(ApplicationPermissions.ContractEdit);
        contracts.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, ContractCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new RemoveContractItemCommand(id, itemId), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.ContractEdit);

        contracts.MapGet("/{id:guid}/clauses", async (Guid id, ContractQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetContractByIdQuery(id), ct) is { } contract ? Results.Ok(contract.Clauses) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.ContractView);
        contracts.MapPost("/{id:guid}/clauses", async (Guid id, AddContractClauseRequest request, ContractCommandHandler handler, CancellationToken ct) =>
            Results.Created($"/api/contracts/{id}/clauses", await handler.Handle(new AddContractClauseCommand(id, request), ct)))
            .RequirePermission(ApplicationPermissions.ContractManageClauses);
        contracts.MapPut("/{id:guid}/clauses/{clauseId:guid}", async (
            Guid id, Guid clauseId, UpdateContractClauseRequest request, ContractCommandHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new UpdateContractClauseCommand(id, clauseId, request), ct)))
            .RequirePermission(ApplicationPermissions.ContractManageClauses);
        contracts.MapDelete("/{id:guid}/clauses/{clauseId:guid}", async (
            Guid id, Guid clauseId, ContractCommandHandler handler, CancellationToken ct) =>
        { await handler.Handle(new RemoveContractClauseCommand(id, clauseId), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.ContractManageClauses);
        contracts.MapPost("/{id:guid}/apply-template/{templateId:guid}", async (
            Guid id, Guid templateId, ContractCommandHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new ApplyContractTemplateCommand(id, templateId), ct)))
            .RequirePermission(ApplicationPermissions.ContractManageClauses);

        contracts.MapGet("/templates", async (bool? includeInactive, ContractQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetContractTemplatesQuery(includeInactive ?? false), ct))
            .RequirePermission(ApplicationPermissions.ContractView);
        contracts.MapGet("/templates/{id:guid}", async (Guid id, ContractQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetContractTemplateByIdQuery(id), ct) is { } template ? Results.Ok(template) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.ContractView);
        contracts.MapPost("/templates", async (CreateContractTemplateRequest request, ContractCommandHandler handler, CancellationToken ct) =>
            Results.Created("/api/contracts/templates", await handler.Handle(new CreateContractTemplateCommand(request), ct)))
            .RequirePermission(ApplicationPermissions.ContractManageTemplates);
        contracts.MapPut("/templates/{id:guid}", async (Guid id, UpdateContractTemplateRequest request, ContractCommandHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new UpdateContractTemplateCommand(id, request), ct)))
            .RequirePermission(ApplicationPermissions.ContractManageTemplates);
        contracts.MapPost("/templates/{id:guid}/clauses", async (
            Guid id, AddContractTemplateClauseRequest request, ContractCommandHandler handler, CancellationToken ct) =>
            Results.Created($"/api/contracts/templates/{id}/clauses", await handler.Handle(new AddContractTemplateClauseCommand(id, request), ct)))
            .RequirePermission(ApplicationPermissions.ContractManageTemplates);
        contracts.MapPut("/templates/{id:guid}/clauses/{clauseId:guid}", async (
            Guid id, Guid clauseId, UpdateContractTemplateClauseRequest request, ContractCommandHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new UpdateContractTemplateClauseCommand(id, clauseId, request), ct)))
            .RequirePermission(ApplicationPermissions.ContractManageTemplates);

        contracts.MapPost("/{id:guid}/documents/upload", async (
            Guid id, IFormFile file, string? documentType, string? description, ContractQueryHandler query,
            IContractRepository repository, IFileStorageService storage, ICurrentUserService currentUser, CancellationToken ct) =>
        {
            var detail = await query.Handle(new GetContractByIdQuery(id), ct);
            if (detail is null) return Results.NotFound();
            await using var stream = file.OpenReadStream();
            var document = await storage.SaveFileAsync(detail.Contract.PurchaseFileId, DocumentType.Contract,
                file.FileName, stream, currentUser.UserId, mimeType: file.ContentType, description: description,
                cancellationToken: ct);
            var contractDocument = new ContractDocument(
                Guid.NewGuid(), id, document.Id, documentType ?? "ContractAttachment", file.FileName,
                description, currentUser.UserId);
            await repository.AddContractDocumentAsync(contractDocument, ct);
            await repository.SaveChangesAsync(ct);
            return Results.Created($"/api/contracts/{id}/documents", new ContractDocumentDto(
                contractDocument.Id, id, document.Id, contractDocument.DocumentType,
                contractDocument.OriginalFileName, contractDocument.Description,
                contractDocument.UploadedAt, contractDocument.UploadedByUserId));
        }).DisableAntiforgery().RequirePermission(ApplicationPermissions.ContractManageDocuments);

        contracts.MapGet("/{id:guid}/documents", async (Guid id, ContractQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetContractDocumentsQuery(id), ct))
            .RequirePermission(ApplicationPermissions.ContractView);

        contracts.MapGet("/{id:guid}/documents/{documentId:guid}/download", async (
            Guid id, Guid documentId, ContractQueryHandler query, IFileStorageService storage, CancellationToken ct) =>
        {
            var document = (await query.Handle(new GetContractDocumentsQuery(id), ct)).SingleOrDefault(x => x.Id == documentId);
            if (document?.FileDocumentId is not Guid fileDocumentId) return Results.NotFound();
            var content = await storage.OpenFileAsync(fileDocumentId, ct);
            return Results.File(content.Stream, content.MimeType, content.OriginalFileName);
        }).RequirePermission(ApplicationPermissions.ContractView);

        contracts.MapDelete("/{id:guid}/documents/{documentId:guid}", async (
            Guid id, Guid documentId, ContractQueryHandler query, IFileStorageService storage, CancellationToken ct) =>
        {
            var document = (await query.Handle(new GetContractDocumentsQuery(id), ct)).SingleOrDefault(x => x.Id == documentId);
            if (document?.FileDocumentId is Guid fileDocumentId) await storage.DeleteFileAsync(fileDocumentId, ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.ContractManageDocuments);

        contracts.MapGet("/{id:guid}/reports/contract/pdf", async (
            Guid id, IReportGenerator reports, CancellationToken ct) =>
        {
            var bytes = await reports.GeneratePdfAsync(ReportNames.Contract, new Dictionary<string, object?> { ["ContractId"] = id }, ct);
            return Results.File(bytes, "application/pdf", $"Contract-{id:N}.pdf");
        }).RequirePermission(ApplicationPermissions.ContractReportView);

        contracts.MapPost("/{id:guid}/reports/contract/save-to-file", async (
            Guid id, ContractQueryHandler query, IReportGenerator reports, CancellationToken ct) =>
        {
            var contract = await query.Handle(new GetContractByIdQuery(id), ct);
            if (contract is null) return Results.NotFound();
            var result = await reports.SaveGeneratedReportToPurchaseFileAsync(contract.Contract.PurchaseFileId,
                ReportNames.Contract, new Dictionary<string, object?>
                {
                    ["ContractId"] = id,
                    ["ContractNumber"] = contract.Contract.ContractNumber
                }, ct);
            return Results.Ok(new ContractReportResultDto(id, contract.Contract.ContractNumber, result.OriginalFileName));
        }).RequirePermission(ApplicationPermissions.ContractReportExport);

        return app;
    }
}

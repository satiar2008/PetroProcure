using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Domain.Enums;
using PetroProcure.Application.Security;
using PetroProcure.Api.Security;
using PetroProcure.Contracts.V1.PurchaseFiles;
using PetroProcure.Api.Contracts;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
        files.MapGet("/{id:guid}/lifecycle", async (Guid id, PetroProcureDbContext db, CancellationToken ct) =>
            await BuildLifecycleAsync(id, db, ct) is { } lifecycle ? Results.Ok(lifecycle) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.PurchaseFileView);
        files.MapGet("/{id:guid}/items/grouped", async (Guid id, PurchaseFileQueryHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetPurchaseFileItemsGroupedByMescGeneralGroupQuery(id), ct)).Select(x => x.ToContract())).RequirePermission(ApplicationPermissions.PurchaseFileView);
        files.MapGet("/{id:guid}/technical-reviews", async (Guid id, PurchaseFileTechnicalReviewHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetPurchaseFileTechnicalReviewsQuery(id), ct)).Select(x => x.ToContract()))
            .RequireAnyPermission(ApplicationPermissions.PurchaseFileViewTechnicalReview, ApplicationPermissions.PurchaseFileView);

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
        files.MapPost("/{id:guid}/technical-reviews/request", async (
            Guid id, RequestTechnicalReviewRequest request, PurchaseFileTechnicalReviewHandler handler, CancellationToken ct) =>
            Results.Created($"/api/purchase-files/{id}/technical-reviews",
                (await handler.Handle(new RequestPurchaseFileTechnicalReviewCommand(
                    id, request.DepartmentId, request.Comment, request.DueDate), ct)).ToContract()))
            .RequirePermission(ApplicationPermissions.PurchaseFileRequestTechnicalReview);
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

    private static async Task<PurchaseFileLifecycleDto?> BuildLifecycleAsync(Guid id, PetroProcureDbContext db, CancellationToken ct)
    {
        var file = await db.PurchaseFiles.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.FileNumber,
                x.Title,
                x.Status,
                x.SourceIndentId,
                x.CreatedAt,
                x.CompletedAt,
                x.ArchivedAt
            })
            .SingleOrDefaultAsync(ct);

        if (file is null) return null;

        var indent = file.SourceIndentId.HasValue
            ? await db.Indents.AsNoTracking()
                .Where(x => x.Id == file.SourceIndentId.Value)
                .Select(x => new
                {
                    x.Id,
                    x.IndentNumber,
                    x.Title,
                    x.Status,
                    x.CreatedAt,
                    x.SourceMaterialNeedId,
                    x.SourceShortageAlertId
                })
                .SingleOrDefaultAsync(ct)
            : null;

        var inquiries = await db.Inquiries.AsNoTracking()
            .Where(x => x.PurchaseFileId == id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PurchaseFileLifecycleRelatedEntityDto(
                x.Id, x.InquiryNumber, x.Title, x.Status.ToString(), x.IssueDate, $"/purchase/inquiries/{x.Id}"))
            .ToListAsync(ct);

        var tenders = await db.Tenders.AsNoTracking()
            .Where(x => x.PurchaseFileId == id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PurchaseFileLifecycleRelatedEntityDto(
                x.Id, x.TenderNumber, x.Title, x.Status.ToString(), x.IssueDate, $"/purchase/tenders/{x.Id}"))
            .ToListAsync(ct);

        var commissionSessions = await db.TenderCommissionSessions.AsNoTracking()
            .Where(x => x.PurchaseFileId == id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PurchaseFileLifecycleRelatedEntityDto(
                x.Id, x.SessionNumber, x.Title, x.Status.ToString(), x.SessionDate, $"/tender-commission/sessions/{x.Id}"))
            .ToListAsync(ct);

        var contracts = await db.PurchaseContracts.AsNoTracking()
            .Where(x => x.PurchaseFileId == id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PurchaseFileLifecycleRelatedEntityDto(
                x.Id, x.ContractNumber, x.Title, x.Status.ToString(), x.CreatedAt, $"/contracts/{x.Id}"))
            .ToListAsync(ct);

        var purchaseOrders = await db.PurchaseOrders.AsNoTracking()
            .Where(x => x.PurchaseFileId == id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PurchaseFileLifecycleRelatedEntityDto(
                x.Id, x.PurchaseOrderNumber, x.Title, x.Status.ToString(), x.OrderDate ?? x.CreatedAt, $"/purchase-orders/{x.Id}"))
            .ToListAsync(ct);

        var warehouseReceipts = await db.WarehouseReceipts.AsNoTracking()
            .Where(x => x.PurchaseFileId == id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PurchaseFileLifecycleRelatedEntityDto(
                x.Id, x.ReceiptNumber, x.DeliveryNoteNumber, x.Status.ToString(), x.ReceiptDate, $"/warehouse/receipts/{x.Id}"))
            .ToListAsync(ct);

        var documentsCount = await db.FileDocuments.AsNoTracking()
            .CountAsync(x => x.PurchaseFileId == id && !x.IsDeleted, ct);
        var reportsCount = await db.FileDocuments.AsNoTracking()
            .CountAsync(x => x.PurchaseFileId == id && !x.IsDeleted && x.DocumentType == DocumentType.FinalReport, ct);
        var legalEvaluationsCount = await db.LegalProcurementRuleEvaluations.AsNoTracking()
            .CountAsync(x => x.PurchaseFileId == id, ct);
        var aiEvaluationsCount = await db.AiAnalysisEvaluations.AsNoTracking()
            .CountAsync(x => x.EntityType == "PurchaseFile" && x.EntityId == id, ct);

        var sourceIndentItems = indent is null
            ? []
            : new[]
            {
                new PurchaseFileLifecycleRelatedEntityDto(
                    indent.Id,
                    indent.IndentNumber,
                    indent.Title,
                    indent.Status.ToString(),
                    indent.CreatedAt,
                    $"/indents/{indent.Id}")
            };

        var steps = new List<PurchaseFileLifecycleStepDto>
        {
            new("Indent", "تقاضای کالا", indent is not null, sourceIndentItems.Length, sourceIndentItems),
            new("Inquiry", "استعلام", inquiries.Count > 0, inquiries.Count, inquiries),
            new("Tender", "مناقصه", tenders.Count > 0, tenders.Count, tenders),
            new("Commission", "جلسه کمیسیون", commissionSessions.Count > 0, commissionSessions.Count, commissionSessions),
            new("Contract", "قرارداد", contracts.Count > 0, contracts.Count, contracts),
            new("PurchaseOrder", "سفارش خرید", purchaseOrders.Count > 0, purchaseOrders.Count, purchaseOrders),
            new("WarehouseReceipt", "رسید انبار", warehouseReceipts.Count > 0, warehouseReceipts.Count, warehouseReceipts),
            new("Documents", "اسناد", documentsCount > 0, documentsCount, []),
            new("LegalEvaluations", "ارزیابی حقوقی", legalEvaluationsCount > 0, legalEvaluationsCount, []),
            new("AiEvaluations", "ارزیابی هوش مصنوعی", aiEvaluationsCount > 0, aiEvaluationsCount, [])
        };

        var latestActionDate = new[]
            {
                file.CreatedAt,
                file.CompletedAt,
                file.ArchivedAt,
                indent?.CreatedAt
            }
            .Concat(inquiries.Select(x => x.Date))
            .Concat(tenders.Select(x => x.Date))
            .Concat(commissionSessions.Select(x => x.Date))
            .Concat(contracts.Select(x => x.Date))
            .Concat(purchaseOrders.Select(x => x.Date))
            .Concat(warehouseReceipts.Select(x => x.Date))
            .Where(x => x.HasValue)
            .Max();

        var currentStage = steps.LastOrDefault(x => x.Count > 0)?.Stage ?? file.Status.ToString();

        return new PurchaseFileLifecycleDto(
            file.Id,
            file.FileNumber,
            file.Title,
            file.Status.ToString(),
            file.SourceIndentId,
            indent?.IndentNumber,
            indent?.SourceMaterialNeedId,
            indent?.SourceShortageAlertId,
            documentsCount,
            reportsCount,
            latestActionDate,
            currentStage,
            steps);
    }

}

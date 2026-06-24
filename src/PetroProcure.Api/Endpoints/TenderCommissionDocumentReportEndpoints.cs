using Microsoft.EntityFrameworkCore;
using PetroProcure.Api.Security;
using PetroProcure.Application.Documents;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Commission;
using PetroProcure.Contracts.V1.Reports;
using PetroProcure.Contracts.V1.Tenders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.TenderCommission;
using PetroProcure.Domain.Modules.Tenders;
using PetroProcure.Infrastructure.Persistence;
using PetroProcure.Reporting;

namespace PetroProcure.Api.Endpoints;

public static class TenderCommissionDocumentReportEndpoints
{
    public static IEndpointRouteBuilder MapTenderCommissionDocumentReportEndpoints(this IEndpointRouteBuilder app)
    {
        MapTenderDocuments(app);
        MapCommissionAttachments(app);
        MapTenderReports(app);
        MapCommissionReports(app);
        return app;
    }

    private static void MapTenderDocuments(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/tenders/{id:guid}/documents/upload", async (
            Guid id, IFormFile file, string? documentType, string? description,
            PetroProcureDbContext db, IFileStorageService storage, ICurrentUserService currentUser,
            AdminAuditService audit, CancellationToken ct) =>
        {
            var tender = await db.Tenders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
            if (tender is null) return Results.NotFound();
            await using var stream = file.OpenReadStream();
            var stored = await storage.SaveFileAsync(tender.PurchaseFileId, DocumentType.TenderDocument,
                file.FileName, stream, currentUser.UserId, mimeType: file.ContentType,
                description: description, cancellationToken: ct);
            try
            {
                var link = new TenderDocument(Guid.NewGuid(), id, stored.Id,
                    string.IsNullOrWhiteSpace(documentType) ? "TenderDocument" : documentType.Trim(),
                    stored.OriginalFileName, description, currentUser.UserId);
                db.TenderDocuments.Add(link);
                await db.SaveChangesAsync(ct);
                await audit.LogAsync("TenderDocumentUploaded", "Tender", id.ToString(), stored.OriginalFileName, ct);
                return Results.Created($"/api/tenders/{id}/documents/{link.Id}", ToDto(link));
            }
            catch
            {
                await storage.DeleteFileAsync(stored.Id, ct);
                throw;
            }
        }).DisableAntiforgery().RequirePermission(ApplicationPermissions.TenderManageDocuments);

        app.MapGet("/api/tenders/{id:guid}/documents", async (Guid id, PetroProcureDbContext db, CancellationToken ct) =>
            await TenderDocuments(db, id).ToListAsync(ct)).RequirePermission(ApplicationPermissions.TenderView);

        app.MapGet("/api/tenders/{id:guid}/documents/{documentId:guid}/download", async (
            Guid id, Guid documentId, PetroProcureDbContext db, IFileStorageService storage, CancellationToken ct) =>
        {
            var fileId = await db.TenderDocuments.AsNoTracking()
                .Where(x => x.Id == documentId && x.TenderId == id).Select(x => x.FileDocumentId).SingleOrDefaultAsync(ct);
            if (!fileId.HasValue) return Results.NotFound();
            var content = await storage.OpenFileAsync(fileId.Value, ct);
            return Results.File(content.Stream, content.MimeType, content.OriginalFileName);
        }).RequirePermission(ApplicationPermissions.TenderView);

        app.MapDelete("/api/tenders/{id:guid}/documents/{documentId:guid}", async (
            Guid id, Guid documentId, PetroProcureDbContext db, IFileStorageService storage,
            AdminAuditService audit, CancellationToken ct) =>
        {
            var link = await db.TenderDocuments.SingleOrDefaultAsync(x => x.Id == documentId && x.TenderId == id, ct);
            if (link?.FileDocumentId is null) return Results.NotFound();
            await storage.DeleteFileAsync(link.FileDocumentId.Value, ct);
            await audit.LogAsync("TenderDocumentDeleted", "Tender", id.ToString(), link.OriginalFileName, ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.TenderManageDocuments);
    }

    private static void MapCommissionAttachments(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/commission/sessions/{id:guid}/attachments/upload", async (
            Guid id, IFormFile file, string? documentType, string? description,
            PetroProcureDbContext db, IFileStorageService storage, ICurrentUserService currentUser,
            AdminAuditService audit, CancellationToken ct) =>
        {
            var session = await db.TenderCommissionSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
            if (session is null) return Results.NotFound();
            await using var stream = file.OpenReadStream();
            var stored = await storage.SaveFileAsync(session.PurchaseFileId, DocumentType.TenderCommissionMinutes,
                file.FileName, stream, currentUser.UserId, mimeType: file.ContentType,
                description: description, cancellationToken: ct);
            try
            {
                var link = new TenderCommissionAttachment(Guid.NewGuid(), id, stored.Id,
                    string.IsNullOrWhiteSpace(documentType) ? "CommissionAttachment" : documentType.Trim(),
                    stored.OriginalFileName, description, currentUser.UserId);
                db.TenderCommissionAttachments.Add(link);
                await db.SaveChangesAsync(ct);
                await audit.LogAsync("CommissionAttachmentUploaded", "TenderCommissionSession", id.ToString(), stored.OriginalFileName, ct);
                return Results.Created($"/api/commission/sessions/{id}/attachments/{link.Id}", ToDto(link));
            }
            catch
            {
                await storage.DeleteFileAsync(stored.Id, ct);
                throw;
            }
        }).DisableAntiforgery().RequirePermission(ApplicationPermissions.CommissionManageDocuments);

        app.MapGet("/api/commission/sessions/{id:guid}/attachments", async (Guid id, PetroProcureDbContext db, CancellationToken ct) =>
            await CommissionAttachments(db, id).ToListAsync(ct)).RequirePermission(ApplicationPermissions.CommissionView);

        app.MapGet("/api/commission/sessions/{id:guid}/attachments/{attachmentId:guid}/download", async (
            Guid id, Guid attachmentId, PetroProcureDbContext db, IFileStorageService storage, CancellationToken ct) =>
        {
            var fileId = await db.TenderCommissionAttachments.AsNoTracking()
                .Where(x => x.Id == attachmentId && x.SessionId == id).Select(x => x.FileDocumentId).SingleOrDefaultAsync(ct);
            if (!fileId.HasValue) return Results.NotFound();
            var content = await storage.OpenFileAsync(fileId.Value, ct);
            return Results.File(content.Stream, content.MimeType, content.OriginalFileName);
        }).RequirePermission(ApplicationPermissions.CommissionView);

        app.MapDelete("/api/commission/sessions/{id:guid}/attachments/{attachmentId:guid}", async (
            Guid id, Guid attachmentId, PetroProcureDbContext db, IFileStorageService storage,
            AdminAuditService audit, CancellationToken ct) =>
        {
            var link = await db.TenderCommissionAttachments.SingleOrDefaultAsync(x => x.Id == attachmentId && x.SessionId == id, ct);
            if (link?.FileDocumentId is null) return Results.NotFound();
            await storage.DeleteFileAsync(link.FileDocumentId.Value, ct);
            await audit.LogAsync("CommissionAttachmentDeleted", "TenderCommissionSession", id.ToString(), link.OriginalFileName, ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.CommissionManageDocuments);
    }

    private static void MapTenderReports(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tenders/{id:guid}/reports/summary/pdf", async (Guid id, IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct) =>
            await TenderPdf(id, ReportNames.TenderSummary, "TenderSummary", generator, db, audit, ct))
            .RequireAnyPermission(ApplicationPermissions.TenderReportView, ApplicationPermissions.TenderReportExport);
        app.MapPost("/api/tenders/{id:guid}/reports/summary/save-to-file", async (Guid id, IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct) =>
            await SaveTenderReport(id, ReportNames.TenderSummary, generator, db, audit, ct))
            .RequirePermission(ApplicationPermissions.TenderReportExport);

        app.MapGet("/api/tenders/{id:guid}/reports/comparison/pdf", async (Guid id, IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct) =>
            await TenderPdf(id, ReportNames.TenderComparison, "TenderComparison", generator, db, audit, ct))
            .RequireAnyPermission(ApplicationPermissions.TenderReportView, ApplicationPermissions.TenderReportExport);
        app.MapPost("/api/tenders/{id:guid}/reports/comparison/save-to-file", async (Guid id, IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct) =>
            await SaveTenderReport(id, ReportNames.TenderComparison, generator, db, audit, ct))
            .RequirePermission(ApplicationPermissions.TenderReportExport);

        app.MapGet("/api/tenders/{id:guid}/reports/winner-decision/pdf", async (Guid id, IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct) =>
            await TenderPdf(id, ReportNames.TenderWinnerDecision, "TenderWinnerDecision", generator, db, audit, ct))
            .RequireAnyPermission(ApplicationPermissions.TenderReportView, ApplicationPermissions.TenderReportExport);
        app.MapPost("/api/tenders/{id:guid}/reports/winner-decision/save-to-file", async (Guid id, IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct) =>
            await SaveTenderReport(id, ReportNames.TenderWinnerDecision, generator, db, audit, ct))
            .RequirePermission(ApplicationPermissions.TenderReportExport);
    }

    private static void MapCommissionReports(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/commission/sessions/{id:guid}/reports/minutes/pdf", async (Guid id, IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct) =>
            await CommissionPdf(id, null, ReportNames.CommissionSessionMinutes, "CommissionMinutes", generator, db, audit, ct))
            .RequireAnyPermission(ApplicationPermissions.CommissionReportView, ApplicationPermissions.CommissionReportExport);
        app.MapPost("/api/commission/sessions/{id:guid}/reports/minutes/save-to-file", async (Guid id, IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct) =>
            await SaveCommissionReport(id, null, ReportNames.CommissionSessionMinutes, generator, db, audit, ct))
            .RequirePermission(ApplicationPermissions.CommissionReportExport);

        app.MapGet("/api/commission/sessions/{id:guid}/reports/decision/{decisionId:guid}/pdf", async (Guid id, Guid decisionId, IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct) =>
            await CommissionPdf(id, decisionId, ReportNames.CommissionDecision, "CommissionDecision", generator, db, audit, ct))
            .RequireAnyPermission(ApplicationPermissions.CommissionReportView, ApplicationPermissions.CommissionReportExport);
        app.MapPost("/api/commission/sessions/{id:guid}/reports/decision/{decisionId:guid}/save-to-file", async (Guid id, Guid decisionId, IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct) =>
            await SaveCommissionReport(id, decisionId, ReportNames.CommissionDecision, generator, db, audit, ct))
            .RequirePermission(ApplicationPermissions.CommissionReportExport);
    }

    private static async Task<IResult> TenderPdf(Guid id, string reportName, string prefix, IReportGenerator generator,
        PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct)
    {
        var tender = await db.Tenders.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.Id, x.TenderNumber }).SingleOrDefaultAsync(ct);
        if (tender is null) return Results.NotFound();
        var bytes = await generator.GeneratePdfAsync(reportName, TenderParameters(tender.Id, tender.TenderNumber), ct);
        await audit.LogAsync("ReportGenerated", "Tender", id.ToString(), reportName, ct);
        return Results.File(bytes, "application/pdf", Safe($"{prefix}-{tender.TenderNumber}.pdf"));
    }

    private static async Task<IResult> SaveTenderReport(Guid id, string reportName, IReportGenerator generator,
        PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct)
    {
        var tender = await db.Tenders.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.TenderNumber, x.PurchaseFileId }).SingleOrDefaultAsync(ct);
        if (tender is null) return Results.NotFound();
        var document = await generator.SaveGeneratedReportToPurchaseFileAsync(tender.PurchaseFileId, reportName,
            TenderParameters(tender.Id, tender.TenderNumber), ct);
        var link = new TenderDocument(Guid.NewGuid(), id, document.Id, reportName, document.OriginalFileName,
            document.Description, document.UploadedByUserId);
        db.TenderDocuments.Add(link);
        await db.SaveChangesAsync(ct);
        await audit.LogAsync("ReportSavedToPurchaseFile", "Tender", id.ToString(), reportName, ct);
        return Results.Ok(new GeneratedReportResultDto(document.Id, document.PurchaseFileId, document.OriginalFileName,
            document.RelativePath, reportName, document.UploadedAt));
    }

    private static async Task<IResult> CommissionPdf(Guid sessionId, Guid? decisionId, string reportName, string prefix,
        IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct)
    {
        var session = await db.TenderCommissionSessions.AsNoTracking()
            .Where(x => x.Id == sessionId).Select(x => new { x.Id, x.SessionNumber }).SingleOrDefaultAsync(ct);
        if (session is null) return Results.NotFound();
        var bytes = await generator.GeneratePdfAsync(reportName, CommissionParameters(session.Id, session.SessionNumber, decisionId), ct);
        await audit.LogAsync("ReportGenerated", "TenderCommissionSession", sessionId.ToString(), reportName, ct);
        return Results.File(bytes, "application/pdf", Safe($"{prefix}-{session.SessionNumber}{(decisionId.HasValue ? "-" + decisionId.Value.ToString("N")[..8] : "")}.pdf"));
    }

    private static async Task<IResult> SaveCommissionReport(Guid sessionId, Guid? decisionId, string reportName,
        IReportGenerator generator, PetroProcureDbContext db, AdminAuditService audit, CancellationToken ct)
    {
        var session = await db.TenderCommissionSessions.AsNoTracking()
            .Where(x => x.Id == sessionId).Select(x => new { x.Id, x.SessionNumber, x.PurchaseFileId }).SingleOrDefaultAsync(ct);
        if (session is null) return Results.NotFound();
        var document = await generator.SaveGeneratedReportToPurchaseFileAsync(session.PurchaseFileId, reportName,
            CommissionParameters(session.Id, session.SessionNumber, decisionId), ct);
        var link = new TenderCommissionAttachment(Guid.NewGuid(), sessionId, document.Id, reportName,
            document.OriginalFileName, document.Description, document.UploadedByUserId);
        db.TenderCommissionAttachments.Add(link);
        await db.SaveChangesAsync(ct);
        await audit.LogAsync("ReportSavedToPurchaseFile", "TenderCommissionSession", sessionId.ToString(), reportName, ct);
        return Results.Ok(new GeneratedReportResultDto(document.Id, document.PurchaseFileId, document.OriginalFileName,
            document.RelativePath, reportName, document.UploadedAt));
    }

    private static Dictionary<string, object?> TenderParameters(Guid tenderId, string tenderNumber) =>
        new() { ["TenderId"] = tenderId, ["TenderNumber"] = tenderNumber };

    private static Dictionary<string, object?> CommissionParameters(Guid sessionId, string sessionNumber, Guid? decisionId)
    {
        var parameters = new Dictionary<string, object?> { ["SessionId"] = sessionId, ["SessionNumber"] = sessionNumber };
        if (decisionId.HasValue) parameters["DecisionId"] = decisionId.Value;
        return parameters;
    }

    private static IQueryable<TenderDocumentDto> TenderDocuments(PetroProcureDbContext db, Guid tenderId) =>
        from doc in db.TenderDocuments.AsNoTracking()
        join file in db.FileDocuments.AsNoTracking() on doc.FileDocumentId equals file.Id
        where doc.TenderId == tenderId && !file.IsDeleted
        orderby doc.UploadedAt descending
        select new TenderDocumentDto(doc.Id, doc.TenderId, doc.FileDocumentId, doc.DocumentType,
            doc.OriginalFileName, doc.Description, doc.UploadedAt, doc.UploadedByUserId);

    private static IQueryable<CommissionAttachmentDto> CommissionAttachments(PetroProcureDbContext db, Guid sessionId) =>
        from attachment in db.TenderCommissionAttachments.AsNoTracking()
        join file in db.FileDocuments.AsNoTracking() on attachment.FileDocumentId equals file.Id
        where attachment.SessionId == sessionId && !file.IsDeleted
        orderby attachment.UploadedAt descending
        select new CommissionAttachmentDto(attachment.Id, attachment.SessionId, attachment.FileDocumentId,
            attachment.DocumentType, attachment.OriginalFileName, attachment.Description, attachment.UploadedAt,
            attachment.UploadedByUserId);

    private static TenderDocumentDto ToDto(TenderDocument doc) =>
        new(doc.Id, doc.TenderId, doc.FileDocumentId, doc.DocumentType, doc.OriginalFileName,
            doc.Description, doc.UploadedAt, doc.UploadedByUserId);

    private static CommissionAttachmentDto ToDto(TenderCommissionAttachment attachment) =>
        new(attachment.Id, attachment.SessionId, attachment.FileDocumentId, attachment.DocumentType,
            attachment.OriginalFileName, attachment.Description, attachment.UploadedAt, attachment.UploadedByUserId);

    private static string Safe(string fileName)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(invalid, '-');
        return fileName.Replace(' ', '-');
    }
}

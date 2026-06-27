using PetroProcure.Api.Contracts;
using PetroProcure.Api.Security;
using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.PurchaseFiles;

namespace PetroProcure.Api.Endpoints;

public static class ApplicantEndpoints
{
    public static IEndpointRouteBuilder MapApplicantEndpoints(this IEndpointRouteBuilder app)
    {
        var applicant = app.MapGroup("/api/applicant").WithTags("Applicant");

        applicant.MapGet("/dashboard", async (PurchaseFileTechnicalReviewHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetApplicantDashboardQuery(), ct)).ToContract())
            .RequirePermission(ApplicationPermissions.ApplicantViewDashboard);

        applicant.MapGet("/technical-reviews", async (PurchaseFileTechnicalReviewHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetApplicantTechnicalReviewsQuery(), ct)).Select(x => x.ToContract()))
            .RequirePermission(ApplicationPermissions.ApplicantViewTechnicalReviews);

        applicant.MapGet("/technical-reviews/{id:guid}", async (Guid id, PurchaseFileTechnicalReviewHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetApplicantTechnicalReviewByIdQuery(id), ct) is { } review
                ? Results.Ok(review.ToContract())
                : Results.NotFound())
            .RequirePermission(ApplicationPermissions.ApplicantViewTechnicalReviews);

        applicant.MapPost("/technical-reviews/{id:guid}/start", async (Guid id, PurchaseFileTechnicalReviewHandler handler, CancellationToken ct) =>
            Results.Ok((await handler.Handle(new StartPurchaseFileTechnicalReviewCommand(id), ct)).ToContract()))
            .RequirePermission(ApplicationPermissions.ApplicantSubmitTechnicalReview);

        applicant.MapPost("/technical-reviews/{id:guid}/submit", async (
            Guid id, SubmitTechnicalReviewRequest request, PurchaseFileTechnicalReviewHandler handler, CancellationToken ct) =>
            Results.Ok((await handler.Handle(new SubmitPurchaseFileTechnicalReviewCommand(
                id, request.Decision, request.Comments, request.RecommendationNotes), ct)).ToContract()))
            .RequirePermission(ApplicationPermissions.ApplicantSubmitTechnicalReview);

        applicant.MapPost("/technical-reviews/{id:guid}/request-clarification", async (
            Guid id, TechnicalReviewClarificationRequest request, PurchaseFileTechnicalReviewHandler handler, CancellationToken ct) =>
            Results.Ok((await handler.Handle(new RequestTechnicalReviewClarificationCommand(
                id, request.Comments, request.RecommendationNotes), ct)).ToContract()))
            .RequirePermission(ApplicationPermissions.ApplicantRequestClarification);

        applicant.MapPost("/technical-reviews/{id:guid}/cancel", async (
            Guid id, CancelTechnicalReviewRequest request, PurchaseFileTechnicalReviewHandler handler, CancellationToken ct) =>
            Results.Ok((await handler.Handle(new CancelPurchaseFileTechnicalReviewCommand(id, request.Comments), ct)).ToContract()))
            .RequirePermission(ApplicationPermissions.PurchaseFileRequestTechnicalReview);

        app.MapGet("/api/departments/{key}/dashboard", async (
            string key, PurchaseFileTechnicalReviewHandler handler, CancellationToken ct) =>
            (await handler.Handle(new GetDepartmentDashboardQuery(key), ct)).ToContract())
            .RequireAnyPermission(
                ApplicationPermissions.PurchaseFileView,
                ApplicationPermissions.OrdersViewDashboard,
                ApplicationPermissions.WarehouseView,
                ApplicationPermissions.ApplicantViewDashboard,
                ApplicationPermissions.CommissionView,
                ApplicationPermissions.ContractView,
                ApplicationPermissions.AdminManageDepartments);

        return app;
    }
}

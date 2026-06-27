using PetroProcure.Application.Security;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;

namespace PetroProcure.Application.PurchaseFiles;

public sealed record RequestPurchaseFileTechnicalReviewCommand(Guid PurchaseFileId, Guid? DepartmentId, string? Comment, DateOnly? DueDate);
public sealed record StartPurchaseFileTechnicalReviewCommand(Guid ReviewId);
public sealed record SubmitPurchaseFileTechnicalReviewCommand(Guid ReviewId, PurchaseFileTechnicalReviewDecision Decision,
    string Comments, string? RecommendationNotes);
public sealed record RequestTechnicalReviewClarificationCommand(Guid ReviewId, string Comments, string? RecommendationNotes);
public sealed record CancelPurchaseFileTechnicalReviewCommand(Guid ReviewId, string? Comments);

public sealed record GetPurchaseFileTechnicalReviewsQuery(Guid PurchaseFileId);
public sealed record GetApplicantTechnicalReviewsQuery;
public sealed record GetApplicantTechnicalReviewByIdQuery(Guid ReviewId);
public sealed record GetApplicantDashboardQuery;
public sealed record GetDepartmentDashboardQuery(string DepartmentKey);

public sealed class PurchaseFileTechnicalReviewHandler(IPurchaseFileRepository repository, ICurrentUserService currentUser)
{
    public async Task<PurchaseFileTechnicalReviewDto> Handle(RequestPurchaseFileTechnicalReviewCommand command, CancellationToken ct = default)
    {
        var file = await repository.FindAsync(command.PurchaseFileId, true, ct)
            ?? throw new PurchaseFileNotFoundException("Purchase file was not found.");
        var applicantDepartmentId = command.DepartmentId
            ?? await repository.GetDepartmentIdByTypeAsync(DepartmentType.Applicant, ct)
            ?? throw new PurchaseFileValidationException("Applicant department was not found.");
        if (await repository.HasActiveTechnicalReviewAsync(file.Id, applicantDepartmentId, ct))
            throw new PurchaseFileConflictException("An active technical review already exists for this department.");

        var workflow = await repository.FindPurchaseFileWorkflowAsync(file.Id, ct);
        if (workflow is null)
        {
            workflow = new WorkflowInstance(Guid.NewGuid(), "PurchaseFile", file.Id, file.CurrentDepartmentId, currentUser.UserId);
            await repository.AddWorkflowAsync(workflow, ct);
        }

        var review = new PurchaseFileTechnicalReview(Guid.NewGuid(), file.Id, applicantDepartmentId, currentUser.UserId, command.Comment);
        var step = workflow.Send(applicantDepartmentId, "درخواست بررسی فنی متقاضی", command.Comment, currentUser.UserId);
        var task = new InboxTask(Guid.NewGuid(), workflow.Id, file.Id, null, applicantDepartmentId, null,
            $"بررسی فنی پرونده {file.FileNumber}", command.Comment, command.DueDate);
        review.LinkApplicantTask(workflow.Id, task.Id);
        file.AddNote(new PurchaseFileNote(Guid.NewGuid(), file.Id, applicantDepartmentId, currentUser.UserId,
            $"درخواست بررسی فنی ثبت شد. گام گردش کار: {step.ActionName}", true), currentUser.IsSystemAdmin);

        await repository.AddTechnicalReviewAsync(review, ct);
        await repository.AddInboxTaskAsync(task, ct);
        await repository.SaveChangesAsync(ct);
        return await repository.GetTechnicalReviewDtoAsync(review.Id, ct)
            ?? throw new PurchaseFileValidationException("Technical review could not be loaded.");
    }

    public Task<IReadOnlyList<PurchaseFileTechnicalReviewDto>> Handle(GetPurchaseFileTechnicalReviewsQuery query, CancellationToken ct = default) =>
        repository.GetTechnicalReviewsByPurchaseFileAsync(query.PurchaseFileId, ct);

    public Task<IReadOnlyList<PurchaseFileTechnicalReviewDto>> Handle(GetApplicantTechnicalReviewsQuery query, CancellationToken ct = default) =>
        repository.GetTechnicalReviewsByDepartmentsAsync(currentUser.DepartmentIds, ct);

    public async Task<PurchaseFileTechnicalReviewDto?> Handle(GetApplicantTechnicalReviewByIdQuery query, CancellationToken ct = default)
    {
        var review = await repository.GetTechnicalReviewDtoAsync(query.ReviewId, ct);
        if (review is not null) EnsureApplicantDepartment(review.DepartmentId);
        return review;
    }

    public async Task<PurchaseFileTechnicalReviewDto> Handle(StartPurchaseFileTechnicalReviewCommand command, CancellationToken ct = default)
    {
        var review = await Required(command.ReviewId, ct);
        EnsureApplicantDepartment(review.DepartmentId);
        review.Start(currentUser.UserId);
        if (review.ApplicantInboxTaskId.HasValue && await repository.FindInboxTaskAsync(review.ApplicantInboxTaskId.Value, ct) is { } task)
            task.Assign(currentUser.UserId);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetTechnicalReviewDtoAsync(review.Id, ct))!;
    }

    public async Task<PurchaseFileTechnicalReviewDto> Handle(SubmitPurchaseFileTechnicalReviewCommand command, CancellationToken ct = default)
    {
        var review = await Required(command.ReviewId, ct);
        EnsureApplicantDepartment(review.DepartmentId);
        review.Submit(currentUser.UserId, command.Decision, command.Comments, command.RecommendationNotes);
        await CompleteApplicantTaskAndReturnAsync(review, command.Comments, ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetTechnicalReviewDtoAsync(review.Id, ct))!;
    }

    public async Task<PurchaseFileTechnicalReviewDto> Handle(RequestTechnicalReviewClarificationCommand command, CancellationToken ct = default)
    {
        var review = await Required(command.ReviewId, ct);
        EnsureApplicantDepartment(review.DepartmentId);
        review.RequestClarification(currentUser.UserId, command.Comments, command.RecommendationNotes);
        await CompleteApplicantTaskAndReturnAsync(review, command.Comments, ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetTechnicalReviewDtoAsync(review.Id, ct))!;
    }

    public async Task<PurchaseFileTechnicalReviewDto> Handle(CancelPurchaseFileTechnicalReviewCommand command, CancellationToken ct = default)
    {
        var review = await Required(command.ReviewId, ct);
        review.Cancel(currentUser.UserId, command.Comments);
        if (review.ApplicantInboxTaskId.HasValue && await repository.FindInboxTaskAsync(review.ApplicantInboxTaskId.Value, ct) is { } task)
            task.Complete();
        await repository.SaveChangesAsync(ct);
        return (await repository.GetTechnicalReviewDtoAsync(review.Id, ct))!;
    }

    public Task<ApplicantDashboardDto> Handle(GetApplicantDashboardQuery query, CancellationToken ct = default) =>
        repository.GetApplicantDashboardAsync(currentUser.DepartmentIds, ct);

    public Task<DepartmentDashboardDto> Handle(GetDepartmentDashboardQuery query, CancellationToken ct = default) =>
        repository.GetDepartmentDashboardAsync(query.DepartmentKey, currentUser.DepartmentIds, ct);

    private async Task CompleteApplicantTaskAndReturnAsync(PurchaseFileTechnicalReview review, string comments, CancellationToken ct)
    {
        if (review.ApplicantInboxTaskId.HasValue && await repository.FindInboxTaskAsync(review.ApplicantInboxTaskId.Value, ct) is { } applicantTask)
            applicantTask.Complete();
        var file = await repository.FindAsync(review.PurchaseFileId, true, ct)
            ?? throw new PurchaseFileNotFoundException("Purchase file was not found.");
        if (!review.WorkflowInstanceId.HasValue) return;
        var workflow = await repository.FindPurchaseFileWorkflowAsync(review.PurchaseFileId, ct);
        if (workflow is null) return;
        workflow.Send(file.PurchaseDepartmentId, "بازگشت نتیجه بررسی فنی", comments, currentUser.UserId);
        var returnTask = new InboxTask(Guid.NewGuid(), workflow.Id, file.Id, null, file.PurchaseDepartmentId, null,
            $"نتیجه بررسی فنی پرونده {file.FileNumber}", comments, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)));
        review.LinkReturnTask(returnTask.Id);
        file.AddNote(new PurchaseFileNote(Guid.NewGuid(), file.Id, review.DepartmentId, currentUser.UserId,
            $"نتیجه بررسی فنی: {DecisionText(review.Decision)} - {comments}", true), currentUser.IsSystemAdmin);
        await repository.AddInboxTaskAsync(returnTask, ct);
    }

    private async Task<PurchaseFileTechnicalReview> Required(Guid id, CancellationToken ct) =>
        await repository.FindTechnicalReviewAsync(id, true, ct)
        ?? throw new PurchaseFileNotFoundException("Technical review was not found.");

    private void EnsureApplicantDepartment(Guid departmentId)
    {
        if (!currentUser.IsSystemAdmin && !currentUser.DepartmentIds.Contains(departmentId))
            throw new CurrentUserForbiddenException("User does not have access to this technical review.");
    }

    private static string DecisionText(PurchaseFileTechnicalReviewDecision? decision) => decision switch
    {
        PurchaseFileTechnicalReviewDecision.TechnicallyAccepted => "تأیید فنی",
        PurchaseFileTechnicalReviewDecision.TechnicallyRejected => "رد فنی",
        PurchaseFileTechnicalReviewDecision.NeedsClarification => "نیازمند شفاف‌سازی",
        PurchaseFileTechnicalReviewDecision.ConditionalAcceptance => "تأیید مشروط",
        _ => "ثبت نشده"
    };
}

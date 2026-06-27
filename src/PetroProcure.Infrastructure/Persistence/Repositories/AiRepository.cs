using Microsoft.EntityFrameworkCore;
using PetroProcure.AI;
using DomainAiEvaluationJob = PetroProcure.Domain.Modules.Ai.AiEvaluationJob;
using DomainAiEvaluationResult = PetroProcure.Domain.Modules.Ai.AiEvaluationResult;
using DomainAiFinding = PetroProcure.Domain.Modules.Ai.AiFinding;
using DomainAiFindingSeverity = PetroProcure.Domain.Modules.Ai.AiFindingSeverity;
using DomainAiRecommendation = PetroProcure.Domain.Modules.Ai.AiRecommendation;
using DomainAiRecommendationPriority = PetroProcure.Domain.Modules.Ai.AiRecommendationPriority;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class AiRepository(PetroProcureDbContext db) : IAiEvaluationRepository, IPurchaseFileAiContextBuilder
{
    public async Task<PurchaseFileAiContext> BuildAsync(Guid id, CancellationToken ct = default)
    {
        var file = await db.PurchaseFiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new InvalidOperationException("Purchase file not found.");
        var department = await db.Departments.Where(x => x.Id == file.CurrentDepartmentId).Select(x => x.Name).SingleOrDefaultAsync(ct) ?? "—";
        var items = await db.PurchaseFileItems.AsNoTracking().Where(x => x.PurchaseFileId == id).ToListAsync(ct);
        var groups = AiContextFactory.GroupItems(items.Select(x => (x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, Unit(x.UnitOfMeasureId), x.RequestedQuantity)));
        var docs = await db.FileDocuments.AsNoTracking().Where(x => x.PurchaseFileId == id && !x.IsDeleted).Select(x => new AiContextDocument(x.DocumentType.ToString(), x.OriginalFileName)).ToListAsync(ct);
        var steps = await db.WorkflowSteps.AsNoTracking().Where(x => db.WorkflowInstances.Any(w => w.Id == x.WorkflowInstanceId && w.EntityType == "PurchaseFile" && w.EntityId == id)).Select(x => new AiContextWorkflowStep(x.ActionName, x.FromDepartmentId.ToString(), x.ToDepartmentId.ToString(), x.CreatedAt)).ToListAsync(ct);
        var notes = await db.PurchaseFileNotes.AsNoTracking().Where(x => x.PurchaseFileId == id).Select(x => x.NoteText).ToListAsync(ct);
        return new(id, file.FileNumber, file.Status.ToString(), department, groups, docs, steps, notes);
    }
    public async Task SaveAsync(AiEvaluationJob job, AiEvaluationResult result, CancellationToken ct)
    {
        var queueJob = new DomainAiEvaluationJob(job.Id, "PetroProcure", "PurchaseFile", job.PurchaseFileId,
            job.EvaluationType, 0, job.Id.ToString("N"), "{}", 0);
        queueJob.MarkCompleted(result.Summary, DateTime.UtcNow);
        var storedResult = new DomainAiEvaluationResult(result.Id, queueJob.Id, "PurchaseFile", result.PurchaseFileId,
            result.EvaluationType, result.Summary, result.Summary, null, "PetroProcure", null, null, null, null);
        foreach (var finding in result.Findings)
            storedResult.AddFinding(new DomainAiFinding(finding.Id, storedResult.Id, finding.Title, finding.Description,
                ToDomainSeverity(finding.Severity), finding.Code));
        foreach (var recommendation in result.Recommendations)
            storedResult.AddRecommendation(new DomainAiRecommendation(recommendation.Id, storedResult.Id,
                recommendation.Title, recommendation.Description, ToDomainPriority(recommendation.Severity)));
        db.AiEvaluationJobs.Add(queueJob);
        db.AiEvaluationResults.Add(storedResult);
        await db.SaveChangesAsync(ct);
    }
    public async Task<IReadOnlyList<AiEvaluationDto>> GetAsync(Guid id, CancellationToken ct) =>
        await db.AiEvaluationResults.AsNoTracking().Include(x => x.Findings).Include(x => x.Recommendations).Where(x => x.EntityType == "PurchaseFile" && x.EntityId == id).OrderByDescending(x => x.CreatedAtUtc)
        .Select(x => new AiEvaluationDto(x.Id, x.EntityId, x.AnalysisType, x.Summary, x.CreatedAtUtc, x.Findings.Select(f => new AiFindingDto(f.Id, f.Title, f.Description, ToLegacySeverity(f.Severity), f.RelatedClauseCode)).ToArray(), x.Recommendations.Select(r => new AiRecommendationDto(r.Id, r.Title, r.Description, ToLegacySeverity(r.Priority))).ToArray())).ToListAsync(ct);
    private static string Unit(Guid id) => id.ToString()[^1] switch { '1' => "عدد", '2' => "متر", '3' => "کیلوگرم", '4' => "لیتر", '5' => "بسته", '6' => "دستگاه", _ => "واحد" };
    private static DomainAiFindingSeverity ToDomainSeverity(AiSeverity severity) => severity switch
    {
        AiSeverity.Critical => DomainAiFindingSeverity.Critical,
        AiSeverity.High => DomainAiFindingSeverity.High,
        AiSeverity.Medium => DomainAiFindingSeverity.Medium,
        AiSeverity.Low => DomainAiFindingSeverity.Low,
        _ => DomainAiFindingSeverity.Info
    };
    private static DomainAiRecommendationPriority ToDomainPriority(AiSeverity severity) => severity switch
    {
        AiSeverity.Critical => DomainAiRecommendationPriority.Critical,
        AiSeverity.High => DomainAiRecommendationPriority.High,
        AiSeverity.Medium => DomainAiRecommendationPriority.Medium,
        _ => DomainAiRecommendationPriority.Low
    };
    private static AiSeverity ToLegacySeverity(DomainAiFindingSeverity severity) => severity switch
    {
        DomainAiFindingSeverity.Critical => AiSeverity.Critical,
        DomainAiFindingSeverity.High => AiSeverity.High,
        DomainAiFindingSeverity.Medium => AiSeverity.Medium,
        DomainAiFindingSeverity.Low => AiSeverity.Low,
        _ => AiSeverity.Info
    };
    private static AiSeverity ToLegacySeverity(DomainAiRecommendationPriority priority) => priority switch
    {
        DomainAiRecommendationPriority.Critical => AiSeverity.Critical,
        DomainAiRecommendationPriority.High => AiSeverity.High,
        DomainAiRecommendationPriority.Medium => AiSeverity.Medium,
        _ => AiSeverity.Low
    };
}

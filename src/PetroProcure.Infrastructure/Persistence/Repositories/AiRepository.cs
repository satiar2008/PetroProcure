using Microsoft.EntityFrameworkCore;
using PetroProcure.AI;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class AiRepository(PetroProcureDbContext db):IAiEvaluationRepository,IPurchaseFileAiContextBuilder
{
    public async Task<PurchaseFileAiContext> BuildAsync(Guid id,CancellationToken ct=default)
    {
        var file=await db.PurchaseFiles.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new InvalidOperationException("Purchase file not found.");
        var department=await db.Departments.Where(x=>x.Id==file.CurrentDepartmentId).Select(x=>x.Name).SingleOrDefaultAsync(ct)??"—";
        var items=await db.PurchaseFileItems.AsNoTracking().Where(x=>x.PurchaseFileId==id).ToListAsync(ct);
        var groups=AiContextFactory.GroupItems(items.Select(x=>(x.MescCode,x.MescGeneralGroupCode,x.GeneralDescription,x.SpecificDescription,Unit(x.UnitOfMeasureId),x.RequestedQuantity)));
        var docs=await db.FileDocuments.AsNoTracking().Where(x=>x.PurchaseFileId==id&&!x.IsDeleted).Select(x=>new AiContextDocument(x.DocumentType.ToString(),x.OriginalFileName)).ToListAsync(ct);
        var steps=await db.WorkflowSteps.AsNoTracking().Where(x=>db.WorkflowInstances.Any(w=>w.Id==x.WorkflowInstanceId&&w.EntityType=="PurchaseFile"&&w.EntityId==id)).Select(x=>new AiContextWorkflowStep(x.ActionName,x.FromDepartmentId.ToString(),x.ToDepartmentId.ToString(),x.CreatedAt)).ToListAsync(ct);
        var notes=await db.PurchaseFileNotes.AsNoTracking().Where(x=>x.PurchaseFileId==id).Select(x=>x.NoteText).ToListAsync(ct);
        return new(id,file.FileNumber,file.Status.ToString(),department,groups,docs,steps,notes);
    }
    public async Task SaveAsync(AiEvaluationJob job,AiEvaluationResult result,CancellationToken ct){db.AiEvaluationJobs.Add(job);db.AiEvaluationResults.Add(result);await db.SaveChangesAsync(ct);}
    public async Task<IReadOnlyList<AiEvaluationDto>> GetAsync(Guid id,CancellationToken ct)=>
        await db.AiEvaluationResults.AsNoTracking().Include(x=>x.Findings).Include(x=>x.Recommendations).Where(x=>x.PurchaseFileId==id).OrderByDescending(x=>x.CreatedAt)
        .Select(x=>new AiEvaluationDto(x.Id,x.PurchaseFileId,x.EvaluationType,x.Summary,x.CreatedAt,x.Findings.Select(f=>new AiFindingDto(f.Id,f.Title,f.Description,f.Severity,f.Code)).ToArray(),x.Recommendations.Select(r=>new AiRecommendationDto(r.Id,r.Title,r.Description,r.Severity)).ToArray())).ToListAsync(ct);
    private static string Unit(Guid id)=>id.ToString()[^1] switch{'1'=>"عدد",'2'=>"متر",'3'=>"کیلوگرم",'4'=>"لیتر",'5'=>"بسته",'6'=>"دستگاه",_=>"واحد"};
}

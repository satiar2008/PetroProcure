using Microsoft.Extensions.Options;

namespace PetroProcure.AI;

public sealed class ProcurementRuleEvaluator(IAiEvaluationRepository repository):IProcurementRuleEvaluator
{
    public async Task<AiEvaluationDto> EvaluateAsync(PurchaseFileAiContext context,CancellationToken ct=default)
    {
        var job=new AiEvaluationJob(Guid.NewGuid(),context.PurchaseFileId,"Rules");
        var result=new AiEvaluationResult(Guid.NewGuid(),job.Id,context.PurchaseFileId,"Rules","ارزیابی اولیه قوانین خرید انجام شد. نتیجه پیشنهادی است و جایگزین تصویب انسانی نیست.");
        if(context.ItemGroups.Count==0) result.AddFinding(new(Guid.NewGuid(),result.Id,"پرونده بدون قلم","هیچ قلم خریدی ثبت نشده است.",AiSeverity.High,"NO_ITEMS"));
        else result.AddFinding(new(Guid.NewGuid(),result.Id,"کنترل اقلام","پرونده دارای اقلام گروه‌بندی‌شده MESC است.",AiSeverity.Info,"ITEMS_OK"));
        if(context.Status.Contains("Archived",StringComparison.OrdinalIgnoreCase)) result.AddRecommendation(new(Guid.NewGuid(),result.Id,"عدم تغییر پرونده","پرونده آرشیوشده باید فقط خواندنی باقی بماند.",AiSeverity.High));
        job.Complete(); await repository.SaveAsync(job,result,ct); return Map(result);
    }
    public static AiEvaluationDto Map(AiEvaluationResult r)=>new(r.Id,r.PurchaseFileId,r.EvaluationType,r.Summary,r.CreatedAt,
        r.Findings.Select(x=>new AiFindingDto(x.Id,x.Title,x.Description,x.Severity,x.Code)).ToArray(),
        r.Recommendations.Select(x=>new AiRecommendationDto(x.Id,x.Title,x.Description,x.Severity)).ToArray());
}

public sealed class AiAgentService(IPurchaseFileAiContextBuilder contextBuilder,IAiChatProvider provider,
    IProcurementRuleEvaluator rules,IAiEvaluationRepository repository,IOptions<AiOptions> options):IAiAgentService
{
    private const string Disclaimer="این تحلیل صرفاً پیشنهادی است؛ مسئولیت تصمیم و تأیید نهایی با کاربران مجاز و کمیسیون‌ها است.";
    public async Task<AiEvaluationDto> SummarizeAsync(Guid id,CancellationToken ct=default)
    {
        var c=await contextBuilder.BuildAsync(id,ct); var response=await provider.CompleteAsync(new(Disclaimer,BuildPrompt(c)),ct);
        return await Store(c,"Summary",$"{response.Content}\n\n{Disclaimer}",[],[],ct);
    }
    public async Task<AiEvaluationDto> CheckMissingDocumentsAsync(Guid id,CancellationToken ct=default)
    {
        var c=await contextBuilder.BuildAsync(id,ct); var required=options.Value.RequiredDocumentTypes;
        var missing=required.Where(x=>!c.Documents.Any(d=>d.DocumentType.Equals(x,StringComparison.OrdinalIgnoreCase))).ToArray();
        var findings=missing.Select(x=>new FindingSeed($"مدرک الزامی موجود نیست: {x}","نوع سند مورد نیاز در مخزن پرونده یافت نشد.",AiSeverity.High,$"MISSING_{x.ToUpperInvariant()}")).ToArray();
        var recommendations=missing.Select(x=>new RecommendationSeed("تکمیل مدارک",$"سند {x} توسط واحد مسئول بارگذاری و بررسی شود.",AiSeverity.Medium)).ToArray();
        return await Store(c,"MissingDocuments",missing.Length==0?"مدارک الزامی اولیه موجود است.":$"{missing.Length} مدرک الزامی یافت نشد. {Disclaimer}",findings,recommendations,ct);
    }
    public Task<AiEvaluationDto> EvaluateRulesAsync(Guid id,CancellationToken ct=default)=>Evaluate(id,ct);
    private async Task<AiEvaluationDto> Evaluate(Guid id,CancellationToken ct)=>await rules.EvaluateAsync(await contextBuilder.BuildAsync(id,ct),ct);
    public Task<IReadOnlyList<AiEvaluationDto>> GetEvaluationsAsync(Guid id,CancellationToken ct=default)=>repository.GetAsync(id,ct);
    private async Task<AiEvaluationDto> Store(PurchaseFileAiContext c,string type,string summary,IReadOnlyList<FindingSeed> fs,IReadOnlyList<RecommendationSeed> rs,CancellationToken ct)
    {
        var job=new AiEvaluationJob(Guid.NewGuid(),c.PurchaseFileId,type); var result=new AiEvaluationResult(Guid.NewGuid(),job.Id,c.PurchaseFileId,type,summary);
        foreach(var f in fs) result.AddFinding(new(Guid.NewGuid(),result.Id,f.Title,f.Description,f.Severity,f.Code));
        foreach(var r in rs) result.AddRecommendation(new(Guid.NewGuid(),result.Id,r.Title,r.Description,r.Severity));
        job.Complete(); await repository.SaveAsync(job,result,ct); return ProcurementRuleEvaluator.Map(result);
    }
    private static string BuildPrompt(PurchaseFileAiContext c)=>$"شماره: {c.FileNumber}\nوضعیت: {c.Status}\nواحد: {c.CurrentDepartment}\nگروه‌های کالا: {c.ItemGroups.Count}\nاسناد: {c.Documents.Count}\nیادداشت‌ها: {c.Notes.Count}";
    private sealed record FindingSeed(string Title,string Description,AiSeverity Severity,string? Code);
    private sealed record RecommendationSeed(string Title,string Description,AiSeverity Severity);
}

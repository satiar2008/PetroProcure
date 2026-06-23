using PetroProcure.Domain.Common;

namespace PetroProcure.AI;

public sealed class AiProvider(Guid id,string name,string providerType,string? baseUrl,bool isActive=true):Entity<Guid>(id)
{ public string Name{get;private set;}=name; public string ProviderType{get;private set;}=providerType; public string? BaseUrl{get;private set;}=baseUrl; public bool IsActive{get;private set;}=isActive; }
public sealed class AiModel(Guid id,Guid aiProviderId,string name,string modelIdentifier,bool isActive=true):Entity<Guid>(id)
{ public Guid AiProviderId{get;private set;}=aiProviderId; public string Name{get;private set;}=name; public string ModelIdentifier{get;private set;}=modelIdentifier; public bool IsActive{get;private set;}=isActive; }
public sealed class AiAgentDefinition(Guid id,string name,string capability,bool isActive=true):Entity<Guid>(id)
{ public string Name{get;private set;}=name; public string Capability{get;private set;}=capability; public bool IsActive{get;private set;}=isActive; }
public sealed class AiPromptTemplate(Guid id,Guid agentDefinitionId,string name,string template,int version):Entity<Guid>(id)
{ public Guid AgentDefinitionId{get;private set;}=agentDefinitionId; public string Name{get;private set;}=name; public string Template{get;private set;}=template; public int Version{get;private set;}=version; }
public sealed class ProcurementRule(Guid id,string title,AiSeverity severity,bool isActive=true):Entity<Guid>(id)
{ private readonly List<ProcurementRuleClause> _clauses=[]; public string Title{get;private set;}=title; public AiSeverity Severity{get;private set;}=severity; public bool IsActive{get;private set;}=isActive; public IReadOnlyCollection<ProcurementRuleClause> Clauses=>_clauses; public void AddClause(ProcurementRuleClause c)=>_clauses.Add(c); }
public sealed class ProcurementRuleClause(Guid id,Guid procurementRuleId,string conditionType,string conditionValue,string conditionDescription):Entity<Guid>(id)
{ public Guid ProcurementRuleId{get;private set;}=procurementRuleId; public string ConditionType{get;private set;}=conditionType; public string ConditionValue{get;private set;}=conditionValue; public string ConditionDescription{get;private set;}=conditionDescription; }
public sealed class AiEvaluationJob(Guid id,Guid purchaseFileId,string evaluationType):Entity<Guid>(id)
{ public Guid PurchaseFileId{get;private set;}=purchaseFileId; public string EvaluationType{get;private set;}=evaluationType; public DateTime CreatedAt{get;private set;}=DateTime.UtcNow; public DateTime? CompletedAt{get;private set;} public void Complete()=>CompletedAt=DateTime.UtcNow; }
public sealed class AiEvaluationResult(Guid id,Guid aiEvaluationJobId,Guid purchaseFileId,string evaluationType,string summary):Entity<Guid>(id)
{
 private readonly List<AiFinding> _findings=[]; private readonly List<AiRecommendation> _recommendations=[];
 public Guid AiEvaluationJobId{get;private set;}=aiEvaluationJobId; public Guid PurchaseFileId{get;private set;}=purchaseFileId; public string EvaluationType{get;private set;}=evaluationType; public string Summary{get;private set;}=summary; public DateTime CreatedAt{get;private set;}=DateTime.UtcNow;
 public IReadOnlyCollection<AiFinding> Findings=>_findings; public IReadOnlyCollection<AiRecommendation> Recommendations=>_recommendations;
 public void AddFinding(AiFinding x)=>_findings.Add(x); public void AddRecommendation(AiRecommendation x)=>_recommendations.Add(x);
}
public sealed class AiFinding(Guid id,Guid evaluationResultId,string title,string description,AiSeverity severity,string? code):Entity<Guid>(id)
{ public Guid EvaluationResultId{get;private set;}=evaluationResultId; public string Title{get;private set;}=title; public string Description{get;private set;}=description; public AiSeverity Severity{get;private set;}=severity; public string? Code{get;private set;}=code; }
public sealed class AiRecommendation(Guid id,Guid evaluationResultId,string title,string description,AiSeverity severity):Entity<Guid>(id)
{ public Guid EvaluationResultId{get;private set;}=evaluationResultId; public string Title{get;private set;}=title; public string Description{get;private set;}=description; public AiSeverity Severity{get;private set;}=severity; }
public sealed class AiConversation(Guid id,Guid? purchaseFileId,Guid userId,string title):Entity<Guid>(id)
{ public Guid? PurchaseFileId{get;private set;}=purchaseFileId; public Guid UserId{get;private set;}=userId; public string Title{get;private set;}=title; public DateTime CreatedAt{get;private set;}=DateTime.UtcNow; }
public sealed class AiMessage(Guid id,Guid conversationId,string role,string content):Entity<Guid>(id)
{ public Guid ConversationId{get;private set;}=conversationId; public string Role{get;private set;}=role; public string Content{get;private set;}=content; public DateTime CreatedAt{get;private set;}=DateTime.UtcNow; }

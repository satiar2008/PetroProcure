namespace PetroProcure.Domain.Modules.Ai;

public enum AiJobStatus
{
    Queued = 1,
    Claimed = 2,
    BuildingContext = 3,
    SendingToAiCore = 4,
    SubmittedToAiCore = 5,
    Running = 6,
    Completed = 7,
    Failed = 8,
    Cancelled = 9,
    Expired = 10
}

public enum AiAnalysisType
{
    Summary = 1,
    LegalCompliance = 2,
    MissingDocuments = 3,
    RiskReview = 4,
    CustomQuestion = 5,
    EmbeddingIngestion = 6,
    AskAboutFile = 7,
    FindRelevantRegulations = 8,
    ExplainRuleFinding = 9,
    SummarizeLegalRisk = 10
}

public enum AiFindingSeverity
{
    Info = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    Critical = 5
}

public enum AiRecommendationPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

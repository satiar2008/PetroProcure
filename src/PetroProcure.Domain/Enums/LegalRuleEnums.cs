namespace PetroProcure.Domain.Enums;

public enum LegalDocumentStatus
{
    Draft = 0,
    PendingApproval = 1,
    Active = 2,
    Deprecated = 3,
    Archived = 4
}

public enum RuleStatus
{
    Draft = 0,
    PendingApproval = 1,
    Active = 2,
    Disabled = 3,
    Deprecated = 4
}

public enum RuleSeverity
{
    Info = 0,
    Warning = 1,
    Critical = 2,
    Blocking = 3
}

public enum RuleEvaluationMode
{
    Automatic = 0,
    SemiAutomatic = 1,
    ManualReview = 2
}

public enum RuleType
{
    Checklist = 0,
    Deadline = 1,
    Threshold = 2,
    Workflow = 3,
    Document = 4,
    Evaluation = 5,
    Exception = 6
}

public enum RuleResult
{
    Pass = 0,
    Fail = 1,
    Warning = 2,
    NotApplicable = 3,
    NeedHumanReview = 4
}

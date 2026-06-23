using PetroProcure.Contracts.V1.Workflow;

namespace PetroProcure.Web.Models;

public sealed record InboxTaskRow(InboxTaskDto Task, string? RelatedNumber);

using Microsoft.AspNetCore.Components;
using PetroProcure.Contracts.V1.Commission;
using PetroProcure.Domain.Enums;
using PetroProcure.Web.Services;

namespace PetroProcure.UnitTests.Web;

public sealed class CommissionWebUiTests
{
    [Fact]
    public void Commission_session_list_page_is_registered() =>
        AssertRoute("PetroProcure.Web.Components.Pages.Commission.CommissionSessionList", "/tender-commission/sessions");

    [Fact]
    public void Commission_session_create_page_is_registered() =>
        AssertRoute("PetroProcure.Web.Components.Pages.Commission.CommissionSessionCreate", "/tender-commission/sessions/create");

    [Fact]
    public void Commission_session_from_tender_page_is_registered() =>
        AssertRoute("PetroProcure.Web.Components.Pages.Commission.CommissionSessionFromTender", "/tender-commission/sessions/from-tender/{TenderId:guid}");

    [Fact]
    public void Commission_session_detail_page_is_registered() =>
        AssertRoute("PetroProcure.Web.Components.Pages.Commission.CommissionSessionDetail", "/tender-commission/sessions/{Id:guid}");

    [Fact]
    public void Approve_button_is_hidden_without_permission() =>
        Assert.False(CommissionUiPolicy.CanApprove(TenderCommissionSessionStatus.Completed, _ => false));

    [Fact]
    public void Decisions_are_grouped_by_decision_type()
    {
        var decisions = new[]
        {
            Decision(TenderCommissionDecisionType.ApproveWinner),
            Decision(TenderCommissionDecisionType.Other),
            Decision(TenderCommissionDecisionType.ApproveWinner)
        };

        var groups = CommissionUiPolicy.GroupDecisions(decisions);

        Assert.Equal(2, groups.Count);
        Assert.Contains(groups, x => x.DecisionType == TenderCommissionDecisionType.ApproveWinner && x.Decisions.Count == 2);
    }

    private static void AssertRoute(string typeName, string route)
    {
        var type = typeof(CommissionUiPolicy).Assembly.GetType(typeName);
        Assert.NotNull(type);
        Assert.Contains(type!.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            attribute => attribute.Template == route);
    }

    private static TenderCommissionDecisionDto Decision(TenderCommissionDecisionType type) =>
        new(Guid.NewGuid(), Guid.NewGuid(), type, Guid.NewGuid(), null, null, "تصمیم",
            null, TenderCommissionDecisionStatus.Draft, DateTime.UtcNow, Guid.NewGuid(), null, null);
}

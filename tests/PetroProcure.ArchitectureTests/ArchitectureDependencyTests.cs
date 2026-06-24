using System.Reflection;
using System.Xml.Linq;
using PetroProcure.AI;
using PetroProcure.Application;
using PetroProcure.Domain.Common;
using PetroProcure.Infrastructure;
using PetroProcure.Reporting;
using PetroProcure.Contracts.V1.PurchaseFiles;
using PetroProcure.Web.Services.Api;
using PetroProcure.Web.Services;
using PetroProcure.Web.Services.Auth;

namespace PetroProcure.ArchitectureTests;

public sealed class ArchitectureDependencyTests
{
    [Fact] public void DomainMustNotReferenceInfrastructure() => AssertNoAssemblyReference(typeof(Entity<>).Assembly, "PetroProcure.Infrastructure");
    [Fact] public void DomainMustNotReferenceEfCore() => Assert.DoesNotContain(typeof(Entity<>).Assembly.GetReferencedAssemblies(), x => x.Name!.StartsWith("Microsoft.EntityFrameworkCore"));
    [Fact] public void ApplicationMustNotReferenceWeb() => AssertNoAssemblyReference(typeof(PetroProcure.Application.DependencyInjection).Assembly, "PetroProcure.Web");
    [Fact] public void ApplicationMustNotReferenceInfrastructure() => AssertNoAssemblyReference(typeof(PetroProcure.Application.DependencyInjection).Assembly, "PetroProcure.Infrastructure");
    [Fact] public void WebMustNotDirectlyReferenceInfrastructure() => AssertNoProjectReference("src/PetroProcure.Web/PetroProcure.Web.csproj", "PetroProcure.Infrastructure");
    [Fact] public void ApiCanReferenceInfrastructure() => AssertProjectReference("src/PetroProcure.Api/PetroProcure.Api.csproj", "PetroProcure.Infrastructure");
    [Fact] public void ReportingMustNotDependOnWeb() => AssertNoAssemblyReference(typeof(IReportGenerator).Assembly, "PetroProcure.Web");
    [Fact]
    public void WebTypedClientUsesSharedContracts() =>
        Assert.Equal(typeof(Task<PurchaseFileDto>),
            typeof(IPetroProcureApiClient).GetMethod(nameof(IPetroProcureApiClient.CreatePurchaseFileAsync))!.ReturnType);

    [Fact]
    public void WebMustNotDeclareDuplicateApiDtos()
    {
        var forbiddenNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "PurchaseFileDto", "PurchaseFileSummaryDto", "PurchaseFileDetailDto", "PurchaseFileItemDto",
            "PurchaseFileGroupedItemsDto", "IndentDto", "IndentItemDto", "MescGeneralGroupDto",
            "MescItemDto", "MescItemGroupedDto", "FileDocumentDto", "DocumentVersionDto",
            "InboxTaskDto", "WorkflowStepDto", "WorkflowTimelineDto", "AiEvaluationResultDto",
            "AiFindingDto", "AiRecommendationDto"
        };

        var duplicates = typeof(IPetroProcureApiClient).Assembly.GetTypes()
            .Where(type => forbiddenNames.Contains(type.Name))
            .Select(type => type.FullName)
            .ToArray();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void WebUsesRealAuthenticationServices()
    {
        Assert.True(typeof(AuthService).IsAssignableTo(typeof(IAuthService)));
        Assert.True(typeof(PetroProcureAuthenticationStateProvider)
            .IsAssignableTo(typeof(Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider)));
        Assert.DoesNotContain(typeof(IUserAccessContext).Assembly.GetTypes(),
            type => type.Name.Contains("FakeAuth", StringComparison.OrdinalIgnoreCase)
                || type.Name == "FakeUserAccessContext");
    }

    [Fact]
    public void NavigationPolicyHidesUnauthorizedItems()
    {
        var items = new[]
        {
            new NavigationAccess("مجاز", "/allowed", "icon", "purchase", "PurchaseFile.View"),
            new NavigationAccess("غیرمجاز", "/denied", "icon", "purchase", "Admin.ManageUsers"),
            new NavigationAccess("واحد دیگر", "/other", "icon", "warehouse", "PurchaseFile.View")
        };
        var visible = NavigationAccessPolicy.VisibleForDepartment(
            items, "purchase", permission => permission == "PurchaseFile.View").ToArray();
        Assert.Single(visible);
        Assert.Equal("/allowed", visible[0].Route);
    }

    [Fact]
    public void AiModuleIsReplaceableViaInterfaces()
    {
        Assert.True(typeof(IAiChatProvider).IsInterface);
        Assert.True(typeof(IAiAgentService).IsInterface);
        Assert.True(typeof(IPurchaseFileAiContextBuilder).IsInterface);
        Assert.True(typeof(IProcurementRuleEvaluator).IsInterface);
        Assert.True(typeof(MockAiProvider).IsAssignableTo(typeof(IAiChatProvider)));
        Assert.True(typeof(OllamaProvider).IsAssignableTo(typeof(IAiChatProvider)));
        Assert.True(typeof(OpenAICompatibleProvider).IsAssignableTo(typeof(IAiChatProvider)));
    }

    private static void AssertNoAssemblyReference(Assembly assembly, string forbidden) =>
        Assert.DoesNotContain(assembly.GetReferencedAssemblies(), x => x.Name == forbidden);
    private static string Root => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    private static void AssertNoProjectReference(string project, string forbidden) =>
        Assert.DoesNotContain(References(project), x => x.Contains(forbidden, StringComparison.OrdinalIgnoreCase));
    private static void AssertProjectReference(string project, string expected) =>
        Assert.Contains(References(project), x => x.Contains(expected, StringComparison.OrdinalIgnoreCase));
    private static string[] References(string project) =>
        XDocument.Load(Path.Combine(Root, project)).Descendants("ProjectReference")
            .Select(x => x.Attribute("Include")?.Value ?? "").ToArray();
}

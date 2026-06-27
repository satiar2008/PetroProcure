using System.Text.Json;
using Microsoft.Extensions.Options;
using PetroProcure.Application.Ai;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.UnitTests.AI;

public sealed class PurchaseFileAiContextBuilderTests
{
    [Fact]
    public async Task BuildAsync_GroupsItemsByMescGeneralGroup()
    {
        var builder = CreateBuilder(out _);

        var context = await builder.BuildAsync(TestPurchaseFileId, AiAnalysisType.LegalCompliance, CancellationToken.None);

        Assert.Equal(2, context.ItemGroups.Count);
        Assert.Equal("111111", context.ItemGroups[0].GeneralGroupCode);
        Assert.Equal(2, context.ItemGroups[0].Items.Count);
        Assert.Equal("222222", context.ItemGroups[1].GeneralGroupCode);
        Assert.Single(context.ItemGroups[1].Items);
    }

    [Fact]
    public async Task BuildAsync_IncludesDocumentsMetadata()
    {
        var builder = CreateBuilder(out _);

        var context = await builder.BuildAsync(TestPurchaseFileId, AiAnalysisType.MissingDocuments, CancellationToken.None);

        Assert.Collection(context.Documents,
            document =>
            {
                Assert.Equal("Indent", document.DocumentType);
                Assert.Equal("indent.pdf", document.FileName);
                Assert.Equal(".pdf", document.Extension);
                Assert.Equal("application/pdf", document.MimeType);
            },
            document =>
            {
                Assert.Equal("TechnicalSpecification", document.DocumentType);
                Assert.Equal("technical.pdf", document.FileName);
            });
    }

    [Fact]
    public async Task BuildAsync_IncludesWorkflowTimeline()
    {
        var builder = CreateBuilder(out _);

        var context = await builder.BuildAsync(TestPurchaseFileId, AiAnalysisType.RiskReview, CancellationToken.None);

        Assert.Equal(2, context.WorkflowTimeline.Count);
        Assert.Equal("Create", context.WorkflowTimeline[0].Action);
        Assert.Equal("Orders", context.WorkflowTimeline[0].FromDepartment);
        Assert.Equal("Purchase", context.WorkflowTimeline[1].ToDepartment);
    }

    [Fact]
    public async Task BuildAsync_DoesNotIncludeAbsoluteFilePaths()
    {
        var builder = CreateBuilder(out _);

        var context = await builder.BuildAsync(TestPurchaseFileId, AiAnalysisType.MissingDocuments, CancellationToken.None);
        var json = JsonSerializer.Serialize(context, JsonOptions);

        Assert.DoesNotContain(@"C:\", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/var/", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("relativePath", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("storedFileName", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BuildAsync_RespectsConfiguredLimits()
    {
        var builder = CreateBuilder(out var source, maxItems: 2, maxNotes: 1);

        var context = await builder.BuildAsync(TestPurchaseFileId, AiAnalysisType.Summary, CancellationToken.None);

        Assert.Equal(2, source.LastMaxItems);
        Assert.Equal(1, source.LastMaxNotes);
        Assert.Equal(2, context.ItemGroups.Sum(x => x.Items.Count));
        Assert.Single(context.NotesSummary);
    }

    [Fact]
    public async Task BuildAsync_IsDeterministicForSameInput()
    {
        var builder = CreateBuilder(out _);

        var first = await builder.BuildAsync(TestPurchaseFileId, AiAnalysisType.LegalCompliance, CancellationToken.None);
        var second = await builder.BuildAsync(TestPurchaseFileId, AiAnalysisType.LegalCompliance, CancellationToken.None);

        Assert.Equal(JsonSerializer.Serialize(first, JsonOptions), JsonSerializer.Serialize(second, JsonOptions));
    }

    private static PurchaseFileAiContextBuilder CreateBuilder(
        out FakePurchaseFileAiContextDataSource source,
        int maxItems = 50,
        int maxNotes = 10)
    {
        source = new FakePurchaseFileAiContextDataSource();
        return new PurchaseFileAiContextBuilder(source, Options.Create(new PurchaseFileAiContextOptions
        {
            MaxContextItems = maxItems,
            MaxNotes = maxNotes,
            RequiredDocumentTypes = ["Indent", "TechnicalSpecification"]
        }));
    }

    private static readonly Guid TestPurchaseFileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed class FakePurchaseFileAiContextDataSource : IPurchaseFileAiContextDataSource
    {
        public int LastMaxItems { get; private set; }
        public int LastMaxNotes { get; private set; }

        public Task<PurchaseFileAiContextSnapshot?> LoadAsync(
            Guid purchaseFileId,
            AiAnalysisType analysisType,
            int maxItems,
            int maxNotes,
            CancellationToken ct)
        {
            LastMaxItems = maxItems;
            LastMaxNotes = maxNotes;

            var snapshot = new PurchaseFileAiContextSnapshot(
                purchaseFileId,
                "PF-1403-0001",
                "Pump procurement",
                "InPurchaseDepartment",
                "Purchase",
                "High",
                "2630001",
                Items.Take(maxItems).ToArray(),
                Documents,
                Workflow,
                Notes.Take(maxNotes).ToArray(),
                new AiContextTenderDto(
                    Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    "T-001",
                    "Pump tender",
                    "Draft",
                    "Public",
                    new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                    3,
                    0),
                new AiContextContractDto(
                    Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    "C-001",
                    "Pump contract",
                    "Draft",
                    "Purchase",
                    "IRR",
                    1000,
                    new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc)),
                Rules);

            return Task.FromResult<PurchaseFileAiContextSnapshot?>(snapshot);
        }

        private static readonly PurchaseFileAiContextItemSnapshot[] Items =
        [
            new("222222", "Valves", "2222220001", "Gate valve", "EA", 1, 1, "Class 150"),
            new("111111", "Pumps", "1111110002", "Seal kit", "EA", 2, 2, null),
            new("111111", "Pumps", "1111110001", "Centrifugal pump", "EA", 1, 1, "API 610")
        ];

        private static readonly AiContextDocumentDto[] Documents =
        [
            new(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Indent", "indent.pdf", ".pdf",
                "application/pdf", 2048, 1, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "Approved indent"),
            new(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "TechnicalSpecification", "technical.pdf", ".pdf",
                "application/pdf", 4096, 2, new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), "Technical document")
        ];

        private static readonly AiContextWorkflowStepDto[] Workflow =
        [
            new("SendToPurchase", "Planning", "Purchase", new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc), null, "Ready"),
            new("Create", "Orders", "Planning", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, "Created")
        ];

        private static readonly PurchaseFileAiNoteSnapshot[] Notes =
        [
            new(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc), "Third note"),
            new(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), "Second note"),
            new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "First note")
        ];

        private static readonly AiContextRuleClauseDto[] Rules =
        [
            new("RULE-002", "Commercial review", "2", "Review commercial terms.", "Legal-2", "Warning", "PurchaseFile"),
            new("RULE-001", "Document completeness", "1", "Indent is required.", "Legal-1", "Blocking", "PurchaseFile")
        ];
    }
}

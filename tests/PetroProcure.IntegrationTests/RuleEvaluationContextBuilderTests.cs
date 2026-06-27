using Microsoft.Extensions.DependencyInjection;
using PetroProcure.Application.Legal;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.IntegrationTests;

[Collection("sqlserver")]
public sealed class RuleEvaluationContextBuilderTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task BuildsExpandedContextForSeededPurchaseFile()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var builder = scope.ServiceProvider.GetRequiredService<IPurchaseFileRuleContextBuilder>();

        var ctx = await builder.BuildPurchaseFileAsync(SeedDataIds.SamplePurchaseFileId, CancellationToken.None);

        Assert.Equal("PurchaseFile", ctx.EntityType);
        Assert.Equal(SeedDataIds.SamplePurchaseFileId, ctx.EntityId);
        Assert.Equal(SeedDataIds.SamplePurchaseFileId, ctx.PurchaseFileId);
        Assert.False(string.IsNullOrWhiteSpace(ctx.FileNumber));
        Assert.False(string.IsNullOrWhiteSpace(ctx.Status));
        Assert.NotNull(ctx.CreatedAt);
        Assert.NotEqual(Guid.Empty, ctx.CurrentDepartmentId);
        Assert.True(ctx.ItemCount >= 0);
        Assert.True(ctx.TotalRequestedQuantity >= 0m);

        // Expanded fields must be null-safe even when tender/supplier/amount/approval data is missing.
        Assert.NotNull(ctx.DocumentTypes);
        Assert.NotNull(ctx.ApprovalStatuses);
        Assert.NotNull(ctx.WorkflowStatuses);
        Assert.NotNull(ctx.LegalReferences);
        Assert.NotNull(ctx.UserDepartmentIds);
    }

    [Fact]
    public async Task MissingPurchaseFileThrowsNotFound()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var builder = scope.ServiceProvider.GetRequiredService<IPurchaseFileRuleContextBuilder>();

        await Assert.ThrowsAsync<LegalRuleNotFoundException>(() =>
            builder.BuildPurchaseFileAsync(Guid.NewGuid(), CancellationToken.None));
    }
}

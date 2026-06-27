using PetroProcure.Application.Legal;

namespace PetroProcure.UnitTests.Application;

public sealed class RuleEvaluationContextTests
{
    [Fact]
    public void LegacyConstructionAppliesSafeNullSafeDefaults()
    {
        // The original 8-argument shape must still compile (AI-RAG-01 backward compatibility),
        // and all expanded fields must be null-safe (nullable null / collections empty).
        var ctx = new RuleEvaluationContext("PurchaseFile", Guid.NewGuid(), Guid.NewGuid(), null,
            "Draft", false, 3, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Indent" });

        Assert.Equal(3, ctx.ItemCount);
        Assert.Equal(0, ctx.ExistingDocumentCount);
        Assert.Equal(0, ctx.SupplierCount);
        Assert.Equal(0, ctx.OfferCount);
        Assert.Equal(0m, ctx.TotalRequestedQuantity);
        Assert.Null(ctx.FileNumber);
        Assert.Null(ctx.TenderType);
        Assert.Null(ctx.FinalAmount);
        Assert.Null(ctx.Currency);
        Assert.Null(ctx.CreatedAt);
        Assert.Null(ctx.InquiryDeadline);
        Assert.Null(ctx.TenderDeadline);
        Assert.Null(ctx.TechnicalReviewDeadline);
        Assert.Empty(ctx.ApprovalStatuses);
        Assert.Empty(ctx.WorkflowStatuses);
        Assert.Empty(ctx.LegalReferences);
        Assert.Empty(ctx.UserDepartmentIds);
    }

    [Fact]
    public void ExpandedConstructionRetainsAllFields()
    {
        var dept = Guid.NewGuid();
        var created = DateTime.UtcNow;
        var deadline = created.AddDays(7);

        var ctx = new RuleEvaluationContext("Tender", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Published", true, 5, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TenderDocument" })
        {
            FileNumber = "PF-1404-0001",
            TenderType = "PublicTender",
            FinalAmount = 1_000_000m,
            Currency = "IRR",
            TotalRequestedQuantity = 42m,
            SupplierCount = 4,
            OfferCount = 3,
            ExistingDocumentCount = 2,
            CreatedAt = created,
            TenderDeadline = deadline,
            ApprovalStatuses = ["Approved"],
            WorkflowStatuses = ["InProgress"],
            UserDepartmentIds = [dept]
        };

        Assert.Equal("PF-1404-0001", ctx.FileNumber);
        Assert.Equal("PublicTender", ctx.TenderType);
        Assert.Equal(1_000_000m, ctx.FinalAmount);
        Assert.Equal("IRR", ctx.Currency);
        Assert.Equal(42m, ctx.TotalRequestedQuantity);
        Assert.Equal(4, ctx.SupplierCount);
        Assert.Equal(3, ctx.OfferCount);
        Assert.Equal(2, ctx.ExistingDocumentCount);
        Assert.Equal(created, ctx.CreatedAt);
        Assert.Equal(deadline, ctx.TenderDeadline);
        Assert.Contains("Approved", ctx.ApprovalStatuses);
        Assert.Contains("InProgress", ctx.WorkflowStatuses);
        Assert.Contains(dept, ctx.UserDepartmentIds);
    }

    [Fact]
    public void ToDtoMapsAllFields()
    {
        var ctx = new RuleEvaluationContext("PurchaseFile", Guid.NewGuid(), Guid.NewGuid(), null,
            "Open", true, 2, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Indent", "TechnicalSpecification" })
        {
            FileNumber = "PF-1404-0002",
            FinalAmount = 500m,
            Currency = "IRR",
            TotalRequestedQuantity = 10m,
            SupplierCount = 1,
            OfferCount = 1,
            ExistingDocumentCount = 2,
            ApprovalStatuses = ["Pending"]
        };

        var dto = ctx.ToDto();

        Assert.Equal(ctx.EntityType, dto.EntityType);
        Assert.Equal(ctx.FileNumber, dto.FileNumber);
        Assert.Equal(ctx.FinalAmount, dto.FinalAmount);
        Assert.Equal(ctx.TotalRequestedQuantity, dto.TotalRequestedQuantity);
        Assert.Equal(2, dto.DocumentTypes.Count);
        Assert.Equal(ctx.ExistingDocumentCount, dto.ExistingDocumentCount);
        Assert.Contains("Pending", dto.ApprovalStatuses);
    }
}

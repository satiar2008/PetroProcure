using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Seeding;

internal static class SampleSeed
{
    public static readonly DateTime CreatedAt = new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
}

internal sealed class SampleIndentSeedConfiguration : IEntityTypeConfiguration<Indent>
{
    public void Configure(EntityTypeBuilder<Indent> b) => b.HasData(new
    {
        Id=SeedDataIds.SampleIndentId,IndentNumber="2630001",YearPart=26,TypeDigit=3,Sequence=1,
        IndentType=IndentType.Manual,Title="درخواست نمونه خرید لوله",RequestingDepartmentId=SeedDataIds.OrdersAndInventoryControlId,
        ApplicantDepartmentId=(Guid?)SeedDataIds.ApplicantId,CreatedByUserId=IdentitySeedData.DefaultAdminUserId,
        CreatedAt=SampleSeed.CreatedAt,Status=IndentStatus.Approved,Description="داده نمونه توسعه",
        CreatedAtUtc=SampleSeed.CreatedAt,CreatedBy=(string?)null,ModifiedAtUtc=(DateTime?)null,ModifiedBy=(string?)null
    });
}
internal sealed class SampleIndentItemSeedConfiguration : IEntityTypeConfiguration<IndentItem>
{
    public void Configure(EntityTypeBuilder<IndentItem> b) => b.HasData(new
    {
        Id=SeedDataIds.SampleIndentItemId,IndentId=SeedDataIds.SampleIndentId,MescItemId=SeedDataIds.PipeItemId,
        MescCode="1234560001",MescGeneralGroupCode="123456",GeneralDescription="لوله و اتصالات عمومی",
        SpecificDescription="لوله فولادی عمومی",UnitOfMeasureId=SeedDataIds.MeterUnitId,RequestedQuantity=25m,
        TechnicalDescription="نمونه تست",RequiredDate=(DateOnly?)null,CreatedAtUtc=SampleSeed.CreatedAt,
        CreatedBy=(string?)null,ModifiedAtUtc=(DateTime?)null,ModifiedBy=(string?)null
    });
}
internal sealed class SamplePurchaseFileSeedConfiguration : IEntityTypeConfiguration<PurchaseFile>
{
    public void Configure(EntityTypeBuilder<PurchaseFile> b) => b.HasData(new
    {
        Id=SeedDataIds.SamplePurchaseFileId,FileNumber="PF-2026-000001",Title="پرونده نمونه خرید لوله",
        Description="پرونده نمونه توسعه",Status=PurchaseFileStatus.InPurchaseDepartment,Priority=PurchaseFilePriority.Normal,
        SourceIndentId=(Guid?)SeedDataIds.SampleIndentId,PurchaseDepartmentId=SeedDataIds.PurchaseDepartmentId,
        CurrentDepartmentId=SeedDataIds.PurchaseDepartmentId,ResponsibleUserId=(Guid?)IdentitySeedData.DefaultAdminUserId,
        CreatedByUserId=IdentitySeedData.DefaultAdminUserId,CreatedAt=SampleSeed.CreatedAt,CompletedAt=(DateTime?)null,
        ArchivedAt=(DateTime?)null,CreatedAtUtc=SampleSeed.CreatedAt,CreatedBy=(string?)null,ModifiedAtUtc=(DateTime?)null,ModifiedBy=(string?)null
    });
}
internal sealed class SamplePurchaseFileItemSeedConfiguration : IEntityTypeConfiguration<PurchaseFileItem>
{
    public void Configure(EntityTypeBuilder<PurchaseFileItem> b) => b.HasData(new
    {
        Id=SeedDataIds.SamplePurchaseFileItemId,PurchaseFileId=SeedDataIds.SamplePurchaseFileId,MescItemId=SeedDataIds.PipeItemId,
        MescCode="1234560001",MescGeneralGroupCode="123456",GeneralDescription="لوله و اتصالات عمومی",
        SpecificDescription="لوله فولادی عمومی",UnitOfMeasureId=SeedDataIds.MeterUnitId,RequestedQuantity=25m,
        ApprovedQuantity=25m,TechnicalDescription="نمونه تست",SourceIndentItemId=(Guid?)SeedDataIds.SampleIndentItemId,
        CreatedAtUtc=SampleSeed.CreatedAt,CreatedBy=(string?)null,ModifiedAtUtc=(DateTime?)null,ModifiedBy=(string?)null
    });
}
internal sealed class SampleWorkflowSeedConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> b) => b.HasData(new
    {
        Id=SeedDataIds.SampleWorkflowId,EntityType="PurchaseFile",EntityId=SeedDataIds.SamplePurchaseFileId,
        CurrentDepartmentId=SeedDataIds.PurchaseDepartmentId,Status=WorkflowStatus.InProgress,
        StartedByUserId=IdentitySeedData.DefaultAdminUserId,StartedAt=SampleSeed.CreatedAt,CompletedAt=(DateTime?)null
    });
}
internal sealed class SampleWorkflowStepSeedConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> b) => b.HasData(new
    {
        Id=SeedDataIds.SampleWorkflowStepId,WorkflowInstanceId=SeedDataIds.SampleWorkflowId,
        FromDepartmentId=SeedDataIds.OrdersAndInventoryControlId,ToDepartmentId=SeedDataIds.PurchaseDepartmentId,
        ActionName="ارسال به واحد خرید",Comment="مرحله نمونه",CreatedByUserId=IdentitySeedData.DefaultAdminUserId,
        CreatedAt=SampleSeed.CreatedAt,CompletedByUserId=(Guid?)null,CompletedAt=(DateTime?)null
    });
}
internal sealed class SampleInboxTaskSeedConfiguration : IEntityTypeConfiguration<InboxTask>
{
    public void Configure(EntityTypeBuilder<InboxTask> b) => b.HasData(new
    {
        Id=SeedDataIds.SampleInboxTaskId,WorkflowInstanceId=SeedDataIds.SampleWorkflowId,
        PurchaseFileId=(Guid?)SeedDataIds.SamplePurchaseFileId,IndentId=(Guid?)null,
        AssignedDepartmentId=SeedDataIds.PurchaseDepartmentId,AssignedUserId=(Guid?)IdentitySeedData.DefaultAdminUserId,
        Title="بررسی پرونده نمونه",Description="وظیفه نمونه توسعه",Status=WorkflowStatus.Pending,
        DueDate=(DateOnly?)null,CreatedAt=SampleSeed.CreatedAt,CompletedAt=(DateTime?)null
    });
}
internal sealed class SampleDocumentSeedConfiguration : IEntityTypeConfiguration<FileDocument>
{
    public void Configure(EntityTypeBuilder<FileDocument> b) => b.HasData(new
    {
        Id=SeedDataIds.SampleDocumentId,PurchaseFileId=SeedDataIds.SamplePurchaseFileId,DepartmentId=(Guid?)SeedDataIds.PurchaseDepartmentId,
        DocumentType=DocumentType.Indent,OriginalFileName="sample-indent.pdf",StoredFileName="sample-indent-v1.pdf",
        RelativePath="PurchaseFiles/2026/PF-2026-000001/01-Indent/sample-indent-v1.pdf",Extension=".pdf",
        MimeType="application/pdf",Size=1024L,Hash=new string('0',64),VersionNo=1,
        UploadedByUserId=IdentitySeedData.DefaultAdminUserId,UploadedAt=SampleSeed.CreatedAt,IsDeleted=false,
        Description="متادیتای سند نمونه",CreatedAtUtc=SampleSeed.CreatedAt,CreatedBy=(string?)null,ModifiedAtUtc=(DateTime?)null,ModifiedBy=(string?)null
    });
}
internal sealed class AdminDepartmentSeedConfiguration : IEntityTypeConfiguration<UserDepartment>
{
    public void Configure(EntityTypeBuilder<UserDepartment> b) => b.HasData(new
    {
        Id=SeedDataIds.AdminPurchaseDepartmentId,UserProfileId=IdentitySeedData.DefaultAdminProfileId,
        DepartmentId=SeedDataIds.PurchaseDepartmentId,IsPrimary=true,CreatedAtUtc=SampleSeed.CreatedAt,
        CreatedBy=(string?)null,ModifiedAtUtc=(DateTime?)null,ModifiedBy=(string?)null
    });
}
internal sealed class SampleIndentSequenceSeedConfiguration : IEntityTypeConfiguration<IndentSequence>
{
    public void Configure(EntityTypeBuilder<IndentSequence> b) => b.HasData(new
    {
        Id=SeedDataIds.SampleIndentSequenceId,YearPart=26,TypeDigit=3,LastSequence=1
    });
}
internal sealed class SamplePurchaseFileSequenceSeedConfiguration : IEntityTypeConfiguration<PurchaseFileSequence>
{
    public void Configure(EntityTypeBuilder<PurchaseFileSequence> b) => b.HasData(new
    {
        Id=SeedDataIds.SamplePurchaseFileSequenceId,Year=2026,LastSequence=1
    });
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetroProcure.Application.Documents;
using PetroProcure.Application.Indents;
using PetroProcure.Application.Mesc;
using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Application.Workflow;
using PetroProcure.Domain.Enums;
using PetroProcure.Infrastructure.Persistence;
using PetroProcure.Infrastructure.Persistence.Seeding;
using PetroProcure.Reporting;

namespace PetroProcure.IntegrationTests;

[Collection("sqlserver")]
public sealed class ProcurementLifecycleIntegrationTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task DatabaseMigrationAppliesSuccessfully()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
    }

    [Fact]
    public async Task RequiredSeedDataExists()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
        Assert.True(await db.Departments.CountAsync() >= 5);
        Assert.True(await db.Roles.AnyAsync());
        Assert.True(await db.Permissions.AnyAsync());
        Assert.True(await db.Users.AnyAsync(x => x.UserName == "admin"));
        Assert.True(await db.UnitOfMeasures.CountAsync() >= 6);
        Assert.True(await db.MescGeneralGroups.CountAsync() >= 3);
        Assert.True(await db.MescItems.CountAsync() >= 10);
        Assert.True(await db.Indents.AnyAsync(x => x.IndentNumber == "2630001"));
        Assert.True(await db.PurchaseFiles.AnyAsync(x => x.FileNumber == "PF-2026-000001"));
        Assert.True(await db.InboxTasks.AnyAsync());
        Assert.True(await db.FileDocuments.AnyAsync());
    }

    [Fact]
    public async Task ApplyingMigrationsAgainDoesNotDuplicateSeedData()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();

        var before = await CountSeededCoreData(db);
        await db.Database.MigrateAsync();
        var after = await CountSeededCoreData(db);

        Assert.Equal(before, after);
    }

    [Fact]
    public async Task FullProcurementLifecycleSucceeds()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<PetroProcureDbContext>();
        var mesc = sp.GetRequiredService<MescCommandHandler>();
        var indentHandler = sp.GetRequiredService<IndentCommandHandler>();
        var purchaseHandler = sp.GetRequiredService<PurchaseFileCommandHandler>();
        var workflow = sp.GetRequiredService<WorkflowCommandHandler>();
        var storage = sp.GetRequiredService<IFileStorageService>();
        var reports = sp.GetRequiredService<IReportGenerator>();
        var suffix = Random.Shared.Next(100000, 999999).ToString();
        var groupCode = suffix[..6];

        var group = await mesc.Handle(new CreateMescGeneralGroupCommand(groupCode, "Integration group"));
        var item = await mesc.Handle(new CreateMescItemCommand(groupCode + "0001", "Integration item", "EA"));
        Assert.Equal(group.Code, item.GeneralGroupCode);

        var indent = await indentHandler.Handle(new CreateIndentCommand(
            26, 4, "Integration indent", SeedDataIds.OrdersAndInventoryControlId,
            SeedDataIds.ApplicantId, "Integration"));
        await indentHandler.Handle(new AddIndentItemCommand(indent.Id, item.Id, SeedDataIds.EachUnitId, 5, "Technical", null));
        await indentHandler.Handle(new SubmitIndentCommand(indent.Id));
        await indentHandler.Handle(new ApproveIndentCommand(indent.Id));
        await indentHandler.Handle(new SendIndentToPurchaseDepartmentCommand(indent.Id));

        var indentWorkflow = await db.WorkflowInstances.AsNoTracking()
            .SingleOrDefaultAsync(x => x.EntityType == "Indent" && x.EntityId == indent.Id);
        Assert.NotNull(indentWorkflow);
        Assert.True(await db.InboxTasks.AsNoTracking().AnyAsync(x =>
            x.WorkflowInstanceId == indentWorkflow.Id
            && x.IndentId == indent.Id
            && x.AssignedDepartmentId == SeedDataIds.PurchaseDepartmentId));

        var file = await purchaseHandler.Handle(new CreatePurchaseFileFromIndentCommand(
            indent.Id, 2026, SeedDataIds.PurchaseDepartmentId, IdentitySeedData.DefaultAdminUserId,
            PurchaseFilePriority.Normal));
        Assert.Single(file.Items);

        await using var content = new MemoryStream([1, 2, 3, 4, 5]);
        var document = await storage.SaveFileAsync(file.Id, DocumentType.TechnicalSpecification,
            "technical.pdf", content, IdentitySeedData.DefaultAdminUserId);
        Assert.False(Path.IsPathRooted(document.RelativePath));
        Assert.True(File.Exists(Path.Combine(fixture.RootPath, document.RelativePath.Replace('/', Path.DirectorySeparatorChar))));

        var workflowId = await workflow.Handle(new StartWorkflowCommand(
            "PurchaseFile", file.Id, SeedDataIds.PurchaseDepartmentId));
        await workflow.Handle(new SendToDepartmentCommand(
            workflowId, SeedDataIds.ApplicantId, "Technical review", "Review requested",
            "Applicant technical review", null, null));

        Assert.Equal(SeedDataIds.ApplicantId, (await db.PurchaseFiles.FindAsync(file.Id))!.CurrentDepartmentId);
        Assert.True(await db.InboxTasks.AnyAsync(x => x.WorkflowInstanceId == workflowId && x.AssignedDepartmentId == SeedDataIds.ApplicantId));

        var pdf = await reports.GeneratePdfAsync(ReportNames.PurchaseFileSummary,
            new Dictionary<string, object?> { ["PurchaseFileId"] = file.Id });
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }

    private static async Task<Dictionary<string, int>> CountSeededCoreData(PetroProcureDbContext db) =>
        new()
        {
            ["departments"] = await db.Departments.CountAsync(),
            ["users"] = await db.Users.CountAsync(),
            ["userDepartments"] = await db.UserDepartments.CountAsync(),
            ["roles"] = await db.Roles.CountAsync(),
            ["permissions"] = await db.Permissions.CountAsync(),
            ["units"] = await db.UnitOfMeasures.CountAsync(),
            ["mescGroups"] = await db.MescGeneralGroups.CountAsync(),
            ["mescItems"] = await db.MescItems.CountAsync(),
            ["indents"] = await db.Indents.CountAsync(),
            ["purchaseFiles"] = await db.PurchaseFiles.CountAsync(),
            ["workflowInstances"] = await db.WorkflowInstances.CountAsync(),
            ["inboxTasks"] = await db.InboxTasks.CountAsync(),
            ["fileDocuments"] = await db.FileDocuments.CountAsync(),
            ["workflowActions"] = await db.WorkflowActionDefinitions.CountAsync()
        };
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.Workflow;
internal sealed class InboxTaskConfiguration : IEntityTypeConfiguration<InboxTask>
{
    public void Configure(EntityTypeBuilder<InboxTask> b)
    {
        b.ToTable("InboxTasks", DatabaseSchemas.Workflow); b.ConfigureEntity();
        b.Property(x => x.Title).IsRequired().HasMaxLength(300); b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.Status).IsRequired(); b.Property(x => x.CreatedAt).IsRequired(); b.Property(x => x.DueDate).HasColumnType("date");
        b.HasIndex(x => new { x.AssignedDepartmentId, x.Status }); b.HasIndex(x => x.AssignedUserId);
        b.HasOne<WorkflowInstance>().WithMany().HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<PurchaseFile>().WithMany().HasForeignKey(x => x.PurchaseFileId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Indent>().WithMany().HasForeignKey(x => x.IndentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Department>().WithMany().HasForeignKey(x => x.AssignedDepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.AssignedUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

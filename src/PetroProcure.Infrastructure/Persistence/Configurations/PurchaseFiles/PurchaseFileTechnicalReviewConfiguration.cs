using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Configurations;

namespace PetroProcure.Infrastructure.Persistence.Configurations.PurchaseFiles;

internal sealed class PurchaseFileTechnicalReviewConfiguration : IEntityTypeConfiguration<PurchaseFileTechnicalReview>
{
    public void Configure(EntityTypeBuilder<PurchaseFileTechnicalReview> builder)
    {
        builder.ToTable("PurchaseFileTechnicalReviews", DatabaseSchemas.Purchase);
        builder.ConfigureAuditableEntity();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Decision);
        builder.Property(x => x.RequestComment).HasMaxLength(2000);
        builder.Property(x => x.Comments).HasMaxLength(4000);
        builder.Property(x => x.RecommendationNotes).HasMaxLength(4000);
        builder.Property(x => x.RequestedAt).IsRequired();
        builder.HasIndex(x => new { x.PurchaseFileId, x.DepartmentId, x.Status });
        builder.HasOne<PurchaseFile>().WithMany().HasForeignKey(x => x.PurchaseFileId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Department>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<WorkflowInstance>().WithMany().HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InboxTask>().WithMany().HasForeignKey(x => x.ApplicantInboxTaskId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InboxTask>().WithMany().HasForeignKey(x => x.ReturnInboxTaskId).OnDelete(DeleteBehavior.Restrict);
    }
}

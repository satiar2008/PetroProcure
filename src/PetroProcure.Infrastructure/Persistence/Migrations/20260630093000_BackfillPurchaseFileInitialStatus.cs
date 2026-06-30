using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(PetroProcureDbContext))]
    [Migration("20260630093000_BackfillPurchaseFileInitialStatus")]
    public partial class BackfillPurchaseFileInitialStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO [purchase].[PurchaseFileStatusHistories]
                    ([Id], [PurchaseFileId], [FromStatus], [ToStatus], [ChangedByUserId], [ChangedAt], [Reason], [DepartmentId])
                SELECT
                    NEWID(),
                    [Id],
                    1,
                    CASE WHEN [CurrentDepartmentId] = [PurchaseDepartmentId] THEN 4 ELSE 3 END,
                    [CreatedByUserId],
                    SYSUTCDATETIME(),
                    N'Initial purchase file routing backfill.',
                    [CurrentDepartmentId]
                FROM [purchase].[PurchaseFiles]
                WHERE [Status] = 1;

                UPDATE [purchase].[PurchaseFiles]
                SET [Status] = CASE WHEN [CurrentDepartmentId] = [PurchaseDepartmentId] THEN 4 ELSE 3 END,
                    [ModifiedAtUtc] = SYSUTCDATETIME(),
                    [ModifiedBy] = N'migration'
                WHERE [Status] = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [purchase].[PurchaseFileStatusHistories]
                WHERE [FromStatus] = 1
                  AND [Reason] = N'Initial purchase file routing backfill.';
                """);
        }
    }
}

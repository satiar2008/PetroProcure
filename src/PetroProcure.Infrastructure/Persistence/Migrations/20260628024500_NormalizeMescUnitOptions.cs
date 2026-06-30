using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(PetroProcureDbContext))]
    [Migration("20260628024500_NormalizeMescUnitOptions")]
    public partial class NormalizeMescUnitOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [item].[UnitOfMeasures]
                SET [IsActive] = 0
                WHERE [Id] IN (
                    '20000000-0000-0000-0000-000000000004',
                    '20000000-0000-0000-0000-000000000006'
                );
                """);

            migrationBuilder.Sql("""
                UPDATE [item].[MescItems]
                SET [UnitOfMeasureId] = '20000000-0000-0000-0000-000000000001',
                    [UnitOfMeasure] = N'عدد'
                WHERE [UnitOfMeasureId] = '20000000-0000-0000-0000-000000000006'
                   OR [UnitOfMeasure] = N'DEV'
                   OR [UnitOfMeasure] = N'دستگاه';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [item].[UnitOfMeasures]
                SET [IsActive] = 1
                WHERE [Id] IN (
                    '20000000-0000-0000-0000-000000000004',
                    '20000000-0000-0000-0000-000000000006'
                );
                """);
        }
    }
}

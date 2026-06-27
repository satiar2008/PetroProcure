using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HybridLegalEvaluationAiFindings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CitationReferences",
                schema: "rule",
                table: "ProcurementRuleFindings",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Confidence",
                schema: "rule",
                table: "ProcurementRuleFindings",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAiGenerated",
                schema: "rule",
                table: "ProcurementRuleFindings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedHumanReview",
                schema: "rule",
                table: "ProcurementRuleFindings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRuleFindings_IsAiGenerated",
                schema: "rule",
                table: "ProcurementRuleFindings",
                column: "IsAiGenerated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcurementRuleFindings_IsAiGenerated",
                schema: "rule",
                table: "ProcurementRuleFindings");

            migrationBuilder.DropColumn(
                name: "CitationReferences",
                schema: "rule",
                table: "ProcurementRuleFindings");

            migrationBuilder.DropColumn(
                name: "Confidence",
                schema: "rule",
                table: "ProcurementRuleFindings");

            migrationBuilder.DropColumn(
                name: "IsAiGenerated",
                schema: "rule",
                table: "ProcurementRuleFindings");

            migrationBuilder.DropColumn(
                name: "NeedHumanReview",
                schema: "rule",
                table: "ProcurementRuleFindings");
        }
    }
}

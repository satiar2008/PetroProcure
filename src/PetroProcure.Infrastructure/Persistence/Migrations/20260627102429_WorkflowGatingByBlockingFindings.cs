using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowGatingByBlockingFindings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FindingId",
                schema: "audit",
                table: "LegalRuleAuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewResult",
                schema: "audit",
                table: "LegalRuleAuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousResult",
                schema: "audit",
                table: "LegalRuleAuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseFileId",
                schema: "audit",
                table: "LegalRuleAuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "audit",
                table: "LegalRuleAuditLogs",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RuleId",
                schema: "audit",
                table: "LegalRuleAuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[] { new Guid("631aaa03-aba3-7e16-1f46-1a4ae62ebfc1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LegalRule.OverrideBlockingFinding", true, null, null, "LegalRule.OverrideBlockingFinding" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[] { new Guid("bdb80f1a-bfa7-92a8-49d8-f6312b3c1af2"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("631aaa03-aba3-7e16-1f46-1a4ae62ebfc1"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") });

            migrationBuilder.CreateIndex(
                name: "IX_LegalRuleAuditLogs_FindingId",
                schema: "audit",
                table: "LegalRuleAuditLogs",
                column: "FindingId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalRuleAuditLogs_PurchaseFileId",
                schema: "audit",
                table: "LegalRuleAuditLogs",
                column: "PurchaseFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LegalRuleAuditLogs_FindingId",
                schema: "audit",
                table: "LegalRuleAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_LegalRuleAuditLogs_PurchaseFileId",
                schema: "audit",
                table: "LegalRuleAuditLogs");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("bdb80f1a-bfa7-92a8-49d8-f6312b3c1af2"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("631aaa03-aba3-7e16-1f46-1a4ae62ebfc1"));

            migrationBuilder.DropColumn(
                name: "FindingId",
                schema: "audit",
                table: "LegalRuleAuditLogs");

            migrationBuilder.DropColumn(
                name: "NewResult",
                schema: "audit",
                table: "LegalRuleAuditLogs");

            migrationBuilder.DropColumn(
                name: "PreviousResult",
                schema: "audit",
                table: "LegalRuleAuditLogs");

            migrationBuilder.DropColumn(
                name: "PurchaseFileId",
                schema: "audit",
                table: "LegalRuleAuditLogs");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "audit",
                table: "LegalRuleAuditLogs");

            migrationBuilder.DropColumn(
                name: "RuleId",
                schema: "audit",
                table: "LegalRuleAuditLogs");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LegalRulesKnowledgeBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "legal");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "rule");

            migrationBuilder.CreateTable(
                name: "LegalDocuments",
                schema: "legal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SearchText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegalRuleAuditLogs",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalRuleAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementRuleEvaluations",
                schema: "rule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    EvaluatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementRuleEvaluations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementRules",
                schema: "rule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RuleSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementRuleSets",
                schema: "rule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementRuleSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegalArticles",
                schema: "legal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArticleNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    SearchText = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalArticles_LegalDocuments_LegalDocumentId",
                        column: x => x.LegalDocumentId,
                        principalSchema: "legal",
                        principalTable: "LegalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementRuleFindings",
                schema: "rule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcurementRuleEvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcurementRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    LegalReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LegalArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LegalClauseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementRuleFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementRuleFindings_ProcurementRuleEvaluations_ProcurementRuleEvaluationId",
                        column: x => x.ProcurementRuleEvaluationId,
                        principalSchema: "rule",
                        principalTable: "ProcurementRuleEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementRuleVersions",
                schema: "rule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcurementRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EvaluationMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LegalArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LegalClauseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LegalReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ConditionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConditionValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ConditionDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SearchText = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementRuleVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementRuleVersions_ProcurementRules_ProcurementRuleId",
                        column: x => x.ProcurementRuleId,
                        principalSchema: "rule",
                        principalTable: "ProcurementRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LegalClauses",
                schema: "legal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClauseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SearchText = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalClauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalClauses_LegalArticles_LegalArticleId",
                        column: x => x.LegalArticleId,
                        principalSchema: "legal",
                        principalTable: "LegalArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("6de6c76a-2d4d-042a-5500-d66b78d17feb"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ProcurementRule.Approve", true, null, null, "ProcurementRule.Approve" },
                    { new Guid("72b4023f-c5f5-18aa-1042-58a23aecabca"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ProcurementRule.Manage", true, null, null, "ProcurementRule.Manage" },
                    { new Guid("ac143c6f-89ad-c038-82fa-77b3fba76014"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ProcurementRule.View", true, null, null, "ProcurementRule.View" },
                    { new Guid("c247e927-997a-aff8-e1fe-ce1655470418"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LegalDocument.View", true, null, null, "LegalDocument.View" },
                    { new Guid("d68c140f-3ce3-317b-ee21-72e345b730a4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ProcurementRule.Evaluate", true, null, null, "ProcurementRule.Evaluate" },
                    { new Guid("dfc85a01-96aa-1df0-8d37-fd52d876788c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "LegalDocument.Manage", true, null, null, "LegalDocument.Manage" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("199a2351-2e32-b1bf-d536-9eea43a32bf6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ac143c6f-89ad-c038-82fa-77b3fba76014"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("1a4a765b-97ad-4d06-3557-a971efcea6e8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6de6c76a-2d4d-042a-5500-d66b78d17feb"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("2c83c748-ffe4-c08a-1706-a469f94e56e4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("dfc85a01-96aa-1df0-8d37-fd52d876788c"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("42e4d8af-0bad-8990-6294-a41e72a11305"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("72b4023f-c5f5-18aa-1042-58a23aecabca"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("80ea3b40-d3f0-e2a5-e538-e811873c1e28"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d68c140f-3ce3-317b-ee21-72e345b730a4"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("c04edffc-ed30-3f5e-ed6b-f3f4e40b67ce"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("c247e927-997a-aff8-e1fe-ce1655470418"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalArticles_LegalDocumentId_ArticleNumber",
                schema: "legal",
                table: "LegalArticles",
                columns: new[] { "LegalDocumentId", "ArticleNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalClauses_LegalArticleId_ClauseNumber",
                schema: "legal",
                table: "LegalClauses",
                columns: new[] { "LegalArticleId", "ClauseNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_FileHash",
                schema: "legal",
                table: "LegalDocuments",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_Status",
                schema: "legal",
                table: "LegalDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LegalRuleAuditLogs_EntityType_EntityId",
                schema: "audit",
                table: "LegalRuleAuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRuleEvaluations_PurchaseFileId",
                schema: "rule",
                table: "ProcurementRuleEvaluations",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRuleEvaluations_TenderId",
                schema: "rule",
                table: "ProcurementRuleEvaluations",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRuleFindings_ProcurementRuleEvaluationId",
                schema: "rule",
                table: "ProcurementRuleFindings",
                column: "ProcurementRuleEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRuleFindings_RuleVersionId",
                schema: "rule",
                table: "ProcurementRuleFindings",
                column: "RuleVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRules_Code",
                schema: "rule",
                table: "ProcurementRules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRuleVersions_ProcurementRuleId_VersionNo",
                schema: "rule",
                table: "ProcurementRuleVersions",
                columns: new[] { "ProcurementRuleId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementRuleVersions_Status",
                schema: "rule",
                table: "ProcurementRuleVersions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegalClauses",
                schema: "legal");

            migrationBuilder.DropTable(
                name: "LegalRuleAuditLogs",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ProcurementRuleFindings",
                schema: "rule");

            migrationBuilder.DropTable(
                name: "ProcurementRuleSets",
                schema: "rule");

            migrationBuilder.DropTable(
                name: "ProcurementRuleVersions",
                schema: "rule");

            migrationBuilder.DropTable(
                name: "LegalArticles",
                schema: "legal");

            migrationBuilder.DropTable(
                name: "ProcurementRuleEvaluations",
                schema: "rule");

            migrationBuilder.DropTable(
                name: "ProcurementRules",
                schema: "rule");

            migrationBuilder.DropTable(
                name: "LegalDocuments",
                schema: "legal");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("199a2351-2e32-b1bf-d536-9eea43a32bf6"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1a4a765b-97ad-4d06-3557-a971efcea6e8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("2c83c748-ffe4-c08a-1706-a469f94e56e4"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("42e4d8af-0bad-8990-6294-a41e72a11305"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("80ea3b40-d3f0-e2a5-e538-e811873c1e28"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c04edffc-ed30-3f5e-ed6b-f3f4e40b67ce"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6de6c76a-2d4d-042a-5500-d66b78d17feb"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("72b4023f-c5f5-18aa-1042-58a23aecabca"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ac143c6f-89ad-c038-82fa-77b3fba76014"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c247e927-997a-aff8-e1fe-ce1655470418"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d68c140f-3ce3-317b-ee21-72e345b730a4"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("dfc85a01-96aa-1df0-8d37-fd52d876788c"));
        }
    }
}

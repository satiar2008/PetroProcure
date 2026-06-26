using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AiCoreProviderIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliesTo",
                schema: "legal",
                table: "LegalClauses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                schema: "legal",
                table: "LegalClauses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                schema: "legal",
                table: "LegalClauses",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AiAnalysisEvaluations",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PromptSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResultSummary = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAnalysisEvaluations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiProviderRequestLogs",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TokenInput = table.Column<int>(type: "int", nullable: true),
                    TokenOutput = table.Column<int>(type: "int", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiProviderRequestLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiAnalysisFindings",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RelatedRuleClauseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Evidence = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Recommendation = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    LegalReference = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAnalysisFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiAnalysisFindings_AiAnalysisEvaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalSchema: "ai",
                        principalTable: "AiAnalysisEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiAnalysisRecommendations",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RelatedAction = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAnalysisRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiAnalysisRecommendations_AiAnalysisEvaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalSchema: "ai",
                        principalTable: "AiAnalysisEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("240cc9dc-34a6-993e-902d-cc049f139fce"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ai.AnalyzePurchaseOrder", true, null, null, "Ai.AnalyzePurchaseOrder" },
                    { new Guid("3cd45a10-afd7-70cf-d197-ad42685d65f8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ai.AnalyzeContract", true, null, null, "Ai.AnalyzeContract" },
                    { new Guid("4643b130-6d7e-9359-333d-0e946eb6cefc"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ai.Admin", true, null, null, "Ai.Admin" },
                    { new Guid("6fcd3d3c-ce4a-75f1-b6a8-012e25afbf1a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ai.AnalyzeTender", true, null, null, "Ai.AnalyzeTender" },
                    { new Guid("8bf3d6db-8312-adf4-bc7c-19cb6ccd65d9"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ai.ProviderManage", true, null, null, "Ai.ProviderManage" },
                    { new Guid("9d1b1ce6-817f-390d-9161-ff0848410a29"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ai.AnalyzeWarehouseReceipt", true, null, null, "Ai.AnalyzeWarehouseReceipt" },
                    { new Guid("cf7fad3a-d53a-da48-4378-cff936268636"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ai.ViewEvaluations", true, null, null, "Ai.ViewEvaluations" },
                    { new Guid("d7213324-0e14-c5fa-d827-ab71e1130599"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ai.ProviderTest", true, null, null, "Ai.ProviderTest" },
                    { new Guid("faf17e48-06b2-d689-8392-2dccd6898072"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ai.AnalyzePurchaseFile", true, null, null, "Ai.AnalyzePurchaseFile" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("013d7411-c704-1028-0e41-dc06872feefd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("faf17e48-06b2-d689-8392-2dccd6898072"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("09adb56b-35cc-0e8c-a815-4149e482705d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("240cc9dc-34a6-993e-902d-cc049f139fce"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("1974c95c-285c-b0c0-fe59-689ba8df4c83"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d7213324-0e14-c5fa-d827-ab71e1130599"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("1c61cb83-82ff-d313-69c1-ef6e3c54a8d8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("3cd45a10-afd7-70cf-d197-ad42685d65f8"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("4fd7ee53-afd4-8035-540d-ea1cca2e4a1a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6fcd3d3c-ce4a-75f1-b6a8-012e25afbf1a"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("9272e727-b568-4bad-7bf2-b60fea82b108"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("cf7fad3a-d53a-da48-4378-cff936268636"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("a19b03ff-4756-d8ef-94f9-7ad98c1fde59"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4643b130-6d7e-9359-333d-0e946eb6cefc"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("d0cc6fe2-b73d-3e8f-f4e5-69f64466042d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("8bf3d6db-8312-adf4-bc7c-19cb6ccd65d9"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("f81eaac3-9be7-10b2-e8d8-dd3b2f214193"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9d1b1ce6-817f-390d-9161-ff0848410a29"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalClauses_AppliesTo",
                schema: "legal",
                table: "LegalClauses",
                column: "AppliesTo");

            migrationBuilder.CreateIndex(
                name: "IX_LegalClauses_Severity",
                schema: "legal",
                table: "LegalClauses",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalysisEvaluations_CreatedAt",
                schema: "ai",
                table: "AiAnalysisEvaluations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalysisEvaluations_EntityType_EntityId",
                schema: "ai",
                table: "AiAnalysisEvaluations",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalysisFindings_EvaluationId",
                schema: "ai",
                table: "AiAnalysisFindings",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalysisRecommendations_EvaluationId",
                schema: "ai",
                table: "AiAnalysisRecommendations",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_AiProviderRequestLogs_EntityType_EntityId",
                schema: "ai",
                table: "AiProviderRequestLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AiProviderRequestLogs_StartedAt",
                schema: "ai",
                table: "AiProviderRequestLogs",
                column: "StartedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiAnalysisFindings",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "AiAnalysisRecommendations",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "AiProviderRequestLogs",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "AiAnalysisEvaluations",
                schema: "ai");

            migrationBuilder.DropIndex(
                name: "IX_LegalClauses_AppliesTo",
                schema: "legal",
                table: "LegalClauses");

            migrationBuilder.DropIndex(
                name: "IX_LegalClauses_Severity",
                schema: "legal",
                table: "LegalClauses");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("013d7411-c704-1028-0e41-dc06872feefd"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("09adb56b-35cc-0e8c-a815-4149e482705d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1974c95c-285c-b0c0-fe59-689ba8df4c83"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1c61cb83-82ff-d313-69c1-ef6e3c54a8d8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("4fd7ee53-afd4-8035-540d-ea1cca2e4a1a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("9272e727-b568-4bad-7bf2-b60fea82b108"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a19b03ff-4756-d8ef-94f9-7ad98c1fde59"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d0cc6fe2-b73d-3e8f-f4e5-69f64466042d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("f81eaac3-9be7-10b2-e8d8-dd3b2f214193"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("240cc9dc-34a6-993e-902d-cc049f139fce"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3cd45a10-afd7-70cf-d197-ad42685d65f8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4643b130-6d7e-9359-333d-0e946eb6cefc"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6fcd3d3c-ce4a-75f1-b6a8-012e25afbf1a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8bf3d6db-8312-adf4-bc7c-19cb6ccd65d9"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9d1b1ce6-817f-390d-9161-ff0848410a29"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cf7fad3a-d53a-da48-4378-cff936268636"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d7213324-0e14-c5fa-d827-ab71e1130599"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("faf17e48-06b2-d689-8392-2dccd6898072"));

            migrationBuilder.DropColumn(
                name: "AppliesTo",
                schema: "legal",
                table: "LegalClauses");

            migrationBuilder.DropColumn(
                name: "Severity",
                schema: "legal",
                table: "LegalClauses");

            migrationBuilder.DropColumn(
                name: "Tags",
                schema: "legal",
                table: "LegalClauses");
        }
    }
}

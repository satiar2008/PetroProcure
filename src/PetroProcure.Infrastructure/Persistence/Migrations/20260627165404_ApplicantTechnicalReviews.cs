using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApplicantTechnicalReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseFileTechnicalReviews",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: true),
                    RequestComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RecommendationNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicantInboxTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReturnInboxTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseFileTechnicalReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseFileTechnicalReviews_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "org",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseFileTechnicalReviews_InboxTasks_ApplicantInboxTaskId",
                        column: x => x.ApplicantInboxTaskId,
                        principalSchema: "workflow",
                        principalTable: "InboxTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseFileTechnicalReviews_InboxTasks_ReturnInboxTaskId",
                        column: x => x.ReturnInboxTaskId,
                        principalSchema: "workflow",
                        principalTable: "InboxTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseFileTechnicalReviews_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseFileTechnicalReviews_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseFileTechnicalReviews_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseFileTechnicalReviews_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "workflow",
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("15514f5e-6971-e6a1-b307-c18e8937ed95"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Applicant.ViewTechnicalReviews", true, null, null, "Applicant.ViewTechnicalReviews" },
                    { new Guid("289cd204-b438-15ba-8e5d-97ad4524afe4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Applicant.SubmitTechnicalReview", true, null, null, "Applicant.SubmitTechnicalReview" },
                    { new Guid("4cb6a0cd-8ff7-26fa-de74-b3a999903b0e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Applicant.RequestClarification", true, null, null, "Applicant.RequestClarification" },
                    { new Guid("6c303aff-b573-9060-6b72-dd8256947bba"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Applicant.ViewDashboard", true, null, null, "Applicant.ViewDashboard" },
                    { new Guid("9d3dbdc4-7767-adfb-4375-51a9d156eb91"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseFile.RequestTechnicalReview", true, null, null, "PurchaseFile.RequestTechnicalReview" },
                    { new Guid("f833367c-fae2-a955-e975-2d42d74f8605"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseFile.ViewTechnicalReview", true, null, null, "PurchaseFile.ViewTechnicalReview" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("06823fb4-9c3f-49f8-beb0-38df85c87ea7"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("289cd204-b438-15ba-8e5d-97ad4524afe4"), new Guid("feb60493-d451-b8d4-d9b4-751e8ea5efd0") },
                    { new Guid("2972f1c3-7ad8-fa76-b12c-8b20b954d10e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6c303aff-b573-9060-6b72-dd8256947bba"), new Guid("feb60493-d451-b8d4-d9b4-751e8ea5efd0") },
                    { new Guid("37a5c04c-e0ed-c231-b39d-76811316373c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f833367c-fae2-a955-e975-2d42d74f8605"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("396bf5e3-19be-1f5c-5961-551a90980f2f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4cb6a0cd-8ff7-26fa-de74-b3a999903b0e"), new Guid("feb60493-d451-b8d4-d9b4-751e8ea5efd0") },
                    { new Guid("413f82e8-b647-864c-932b-cd7f565a6b66"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f833367c-fae2-a955-e975-2d42d74f8605"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("48113f79-872e-69fd-b745-bdee1c548116"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f833367c-fae2-a955-e975-2d42d74f8605"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("580c6c2c-fa4e-d479-82e9-703207614776"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4cb6a0cd-8ff7-26fa-de74-b3a999903b0e"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("5e92697a-8d1d-a16d-9398-71302ae54392"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9d3dbdc4-7767-adfb-4375-51a9d156eb91"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("7821935d-fa7d-ebb8-8cb3-21da8e61990f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6c303aff-b573-9060-6b72-dd8256947bba"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("7e0859ae-8b8b-c539-ccf0-d1768f4eedfb"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("289cd204-b438-15ba-8e5d-97ad4524afe4"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("87be1ed9-a79a-aab8-685c-4bbdfbad6302"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9d3dbdc4-7767-adfb-4375-51a9d156eb91"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("8f0da775-0b89-17f7-8b5a-93776e454e21"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("15514f5e-6971-e6a1-b307-c18e8937ed95"), new Guid("feb60493-d451-b8d4-d9b4-751e8ea5efd0") },
                    { new Guid("a9f82319-e53c-fa3e-34ae-8f1fd941ef41"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9d3dbdc4-7767-adfb-4375-51a9d156eb91"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("bf9445e9-0816-87bc-5dab-923e33eb6d3d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f833367c-fae2-a955-e975-2d42d74f8605"), new Guid("feb60493-d451-b8d4-d9b4-751e8ea5efd0") },
                    { new Guid("e94eb7b2-c323-edd9-41a7-9c892145291d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("15514f5e-6971-e6a1-b307-c18e8937ed95"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileTechnicalReviews_ApplicantInboxTaskId",
                schema: "purchase",
                table: "PurchaseFileTechnicalReviews",
                column: "ApplicantInboxTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileTechnicalReviews_DepartmentId",
                schema: "purchase",
                table: "PurchaseFileTechnicalReviews",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileTechnicalReviews_PurchaseFileId_DepartmentId_Status",
                schema: "purchase",
                table: "PurchaseFileTechnicalReviews",
                columns: new[] { "PurchaseFileId", "DepartmentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileTechnicalReviews_RequestedByUserId",
                schema: "purchase",
                table: "PurchaseFileTechnicalReviews",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileTechnicalReviews_ReturnInboxTaskId",
                schema: "purchase",
                table: "PurchaseFileTechnicalReviews",
                column: "ReturnInboxTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileTechnicalReviews_ReviewedByUserId",
                schema: "purchase",
                table: "PurchaseFileTechnicalReviews",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileTechnicalReviews_WorkflowInstanceId",
                schema: "purchase",
                table: "PurchaseFileTechnicalReviews",
                column: "WorkflowInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseFileTechnicalReviews",
                schema: "purchase");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("06823fb4-9c3f-49f8-beb0-38df85c87ea7"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("2972f1c3-7ad8-fa76-b12c-8b20b954d10e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("37a5c04c-e0ed-c231-b39d-76811316373c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("396bf5e3-19be-1f5c-5961-551a90980f2f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("413f82e8-b647-864c-932b-cd7f565a6b66"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("48113f79-872e-69fd-b745-bdee1c548116"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("580c6c2c-fa4e-d479-82e9-703207614776"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("5e92697a-8d1d-a16d-9398-71302ae54392"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7821935d-fa7d-ebb8-8cb3-21da8e61990f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7e0859ae-8b8b-c539-ccf0-d1768f4eedfb"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("87be1ed9-a79a-aab8-685c-4bbdfbad6302"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8f0da775-0b89-17f7-8b5a-93776e454e21"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a9f82319-e53c-fa3e-34ae-8f1fd941ef41"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("bf9445e9-0816-87bc-5dab-923e33eb6d3d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e94eb7b2-c323-edd9-41a7-9c892145291d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("15514f5e-6971-e6a1-b307-c18e8937ed95"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("289cd204-b438-15ba-8e5d-97ad4524afe4"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4cb6a0cd-8ff7-26fa-de74-b3a999903b0e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6c303aff-b573-9060-6b72-dd8256947bba"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9d3dbdc4-7767-adfb-4375-51a9d156eb91"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f833367c-fae2-a955-e975-2d42d74f8605"));
        }
    }
}

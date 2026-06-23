using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDevelopmentSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "indent",
                table: "Indents",
                columns: new[] { "Id", "ApplicantDepartmentId", "CreatedAt", "CreatedAtUtc", "CreatedBy", "CreatedByUserId", "Description", "IndentNumber", "IndentType", "ModifiedAtUtc", "ModifiedBy", "RequestingDepartmentId", "Sequence", "Status", "Title", "TypeDigit", "YearPart" },
                values: new object[] { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000004"), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), "داده نمونه توسعه", "2630001", 2, null, null, new Guid("10000000-0000-0000-0000-000000000002"), 1, 3, "درخواست نمونه خرید لوله", 3, 26 });

            migrationBuilder.InsertData(
                schema: "org",
                table: "UserDepartments",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DepartmentId", "IsPrimary", "ModifiedAtUtc", "ModifiedBy", "UserProfileId" },
                values: new object[] { new Guid("11000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("10000000-0000-0000-0000-000000000001"), true, null, null, new Guid("ea73b3d2-ada1-3014-e807-0287736b56c8") });

            migrationBuilder.InsertData(
                schema: "workflow",
                table: "WorkflowInstances",
                columns: new[] { "Id", "CompletedAt", "CurrentDepartmentId", "EntityId", "EntityType", "StartedAt", "StartedByUserId", "Status" },
                values: new object[] { new Guid("80000000-0000-0000-0000-000000000001"), null, new Guid("10000000-0000-0000-0000-000000000001"), new Guid("70000000-0000-0000-0000-000000000001"), "PurchaseFile", new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), 2 });

            migrationBuilder.InsertData(
                schema: "indent",
                table: "IndentItems",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "GeneralDescription", "IndentId", "MescCode", "MescGeneralGroupCode", "MescItemId", "ModifiedAtUtc", "ModifiedBy", "RequestedQuantity", "RequiredDate", "SpecificDescription", "TechnicalDescription", "UnitOfMeasureId" },
                values: new object[] { new Guid("61000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, "لوله و اتصالات عمومی", new Guid("60000000-0000-0000-0000-000000000001"), "1234560001", "123456", new Guid("40000000-0000-0000-0000-000000000001"), null, null, 25m, null, "لوله فولادی عمومی", "نمونه تست", new Guid("20000000-0000-0000-0000-000000000002") });

            migrationBuilder.InsertData(
                schema: "purchase",
                table: "PurchaseFiles",
                columns: new[] { "Id", "ArchivedAt", "CompletedAt", "CreatedAt", "CreatedAtUtc", "CreatedBy", "CreatedByUserId", "CurrentDepartmentId", "Description", "FileNumber", "ModifiedAtUtc", "ModifiedBy", "Priority", "PurchaseDepartmentId", "ResponsibleUserId", "SourceIndentId", "Status", "Title" },
                values: new object[] { new Guid("70000000-0000-0000-0000-000000000001"), null, null, new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), new Guid("10000000-0000-0000-0000-000000000001"), "پرونده نمونه توسعه", "PF-2026-000001", null, null, 2, new Guid("10000000-0000-0000-0000-000000000001"), new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), new Guid("60000000-0000-0000-0000-000000000001"), 4, "پرونده نمونه خرید لوله" });

            migrationBuilder.InsertData(
                schema: "workflow",
                table: "WorkflowSteps",
                columns: new[] { "Id", "ActionName", "Comment", "CompletedAt", "CompletedByUserId", "CreatedAt", "CreatedByUserId", "FromDepartmentId", "ToDepartmentId", "WorkflowInstanceId" },
                values: new object[] { new Guid("81000000-0000-0000-0000-000000000001"), "ارسال به واحد خرید", "مرحله نمونه", null, null, new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), new Guid("10000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000001"), new Guid("80000000-0000-0000-0000-000000000001") });

            migrationBuilder.InsertData(
                schema: "doc",
                table: "FileDocuments",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DepartmentId", "Description", "DocumentType", "Extension", "Hash", "IsDeleted", "MimeType", "ModifiedAtUtc", "ModifiedBy", "OriginalFileName", "PurchaseFileId", "RelativePath", "Size", "StoredFileName", "UploadedAt", "UploadedByUserId", "VersionNo" },
                values: new object[] { new Guid("90000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("10000000-0000-0000-0000-000000000001"), "متادیتای سند نمونه", 1, ".pdf", "0000000000000000000000000000000000000000000000000000000000000000", false, "application/pdf", null, null, "sample-indent.pdf", new Guid("70000000-0000-0000-0000-000000000001"), "PurchaseFiles/2026/PF-2026-000001/01-Indent/sample-indent-v1.pdf", 1024L, "sample-indent-v1.pdf", new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), 1 });

            migrationBuilder.InsertData(
                schema: "workflow",
                table: "InboxTasks",
                columns: new[] { "Id", "AssignedDepartmentId", "AssignedUserId", "CompletedAt", "CreatedAt", "Description", "DueDate", "IndentId", "PurchaseFileId", "Status", "Title", "WorkflowInstanceId" },
                values: new object[] { new Guid("82000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000001"), new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), null, new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), "وظیفه نمونه توسعه", null, null, new Guid("70000000-0000-0000-0000-000000000001"), 1, "بررسی پرونده نمونه", new Guid("80000000-0000-0000-0000-000000000001") });

            migrationBuilder.InsertData(
                schema: "purchase",
                table: "PurchaseFileItems",
                columns: new[] { "Id", "ApprovedQuantity", "CreatedAtUtc", "CreatedBy", "GeneralDescription", "MescCode", "MescGeneralGroupCode", "MescItemId", "ModifiedAtUtc", "ModifiedBy", "PurchaseFileId", "RequestedQuantity", "SourceIndentItemId", "SpecificDescription", "TechnicalDescription", "UnitOfMeasureId" },
                values: new object[] { new Guid("71000000-0000-0000-0000-000000000001"), 25m, new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, "لوله و اتصالات عمومی", "1234560001", "123456", new Guid("40000000-0000-0000-0000-000000000001"), null, null, new Guid("70000000-0000-0000-0000-000000000001"), 25m, new Guid("61000000-0000-0000-0000-000000000001"), "لوله فولادی عمومی", "نمونه تست", new Guid("20000000-0000-0000-0000-000000000002") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "doc",
                table: "FileDocuments",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "workflow",
                table: "InboxTasks",
                keyColumn: "Id",
                keyValue: new Guid("82000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "purchase",
                table: "PurchaseFileItems",
                keyColumn: "Id",
                keyValue: new Guid("71000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "org",
                table: "UserDepartments",
                keyColumn: "Id",
                keyValue: new Guid("11000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "workflow",
                table: "WorkflowSteps",
                keyColumn: "Id",
                keyValue: new Guid("81000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "indent",
                table: "IndentItems",
                keyColumn: "Id",
                keyValue: new Guid("61000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "purchase",
                table: "PurchaseFiles",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "workflow",
                table: "WorkflowInstances",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "indent",
                table: "Indents",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"));
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TenderCommissionReportsDocumentIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("29835514-b329-8e87-416c-06af74e74a3e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.ReportView", true, null, null, "Tender.ReportView" },
                    { new Guid("8f6a3377-c38a-3157-7101-b73b77d8bda5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.ReportExport", true, null, null, "Tender.ReportExport" },
                    { new Guid("9523c8c2-711d-5d97-643a-3be19cc5ecd3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.ReportExport", true, null, null, "Commission.ReportExport" },
                    { new Guid("a946fd73-3ccf-188c-bbff-8b2dce218107"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.ReportView", true, null, null, "Commission.ReportView" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("c6dfab87-231f-f621-9a09-8d751898c4dd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("060b993c-39b6-b892-cb63-6fcd6bfc8ead"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("0bd0708a-2483-4836-4327-2e7775cd4837"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("29835514-b329-8e87-416c-06af74e74a3e"), new Guid("84dfd3fc-e85b-d990-896c-f99f7933d3e4") },
                    { new Guid("0f65be8c-d5e1-4bf3-b4c1-ef340c4a2ff5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9523c8c2-711d-5d97-643a-3be19cc5ecd3"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("17c306bb-f7bf-77a6-fe85-6ea43df3bf7f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("8f6a3377-c38a-3157-7101-b73b77d8bda5"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("222a9951-7b00-ee81-3adb-94ea823e7bb6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("29835514-b329-8e87-416c-06af74e74a3e"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("48abafe0-1a6a-6fc2-90a5-03cd8e2dcff6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("29835514-b329-8e87-416c-06af74e74a3e"), new Guid("92db8232-7861-b5b6-fba1-d5a94cfac12e") },
                    { new Guid("4d964280-fc84-e137-566a-c534220e0e19"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("29835514-b329-8e87-416c-06af74e74a3e"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("52319fed-cd29-6cd9-b788-13bf7994193c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a946fd73-3ccf-188c-bbff-8b2dce218107"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("524bb488-ea8b-f934-9752-0bb653021392"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("8f6a3377-c38a-3157-7101-b73b77d8bda5"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("5e8956eb-be43-fd33-5e71-3550512f5adc"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("8f6a3377-c38a-3157-7101-b73b77d8bda5"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("5f262377-2f74-4b7a-b30f-5c5b03662726"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a946fd73-3ccf-188c-bbff-8b2dce218107"), new Guid("84dfd3fc-e85b-d990-896c-f99f7933d3e4") },
                    { new Guid("7170e8cf-e2a4-9488-96be-f965cee55db3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("29835514-b329-8e87-416c-06af74e74a3e"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("7181b130-ea0d-6b38-6cfd-32d788253e01"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9523c8c2-711d-5d97-643a-3be19cc5ecd3"), new Guid("84dfd3fc-e85b-d990-896c-f99f7933d3e4") },
                    { new Guid("7c9a24c7-d723-9950-b8f9-5fa326ee5db7"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a946fd73-3ccf-188c-bbff-8b2dce218107"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("8900bc64-5871-5f90-87d1-74e4dcfff94d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("8f6a3377-c38a-3157-7101-b73b77d8bda5"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("8945aebb-3e8f-857f-373d-01211d72e8f0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("29835514-b329-8e87-416c-06af74e74a3e"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("99711bfd-8016-3049-982d-fde6471339db"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a946fd73-3ccf-188c-bbff-8b2dce218107"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("a16d6025-b8b8-e4d1-ed71-0d8c18be3b52"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("8f6a3377-c38a-3157-7101-b73b77d8bda5"), new Guid("84dfd3fc-e85b-d990-896c-f99f7933d3e4") },
                    { new Guid("b8cffcca-161c-b686-9429-8fc72e300b99"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a946fd73-3ccf-188c-bbff-8b2dce218107"), new Guid("92db8232-7861-b5b6-fba1-d5a94cfac12e") },
                    { new Guid("f34e6ac5-1e10-17ea-c9de-29eaefc80db5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9523c8c2-711d-5d97-643a-3be19cc5ecd3"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("f7978155-ffa7-83d0-8ff4-ece731cd9188"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9523c8c2-711d-5d97-643a-3be19cc5ecd3"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0bd0708a-2483-4836-4327-2e7775cd4837"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0f65be8c-d5e1-4bf3-b4c1-ef340c4a2ff5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("17c306bb-f7bf-77a6-fe85-6ea43df3bf7f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("222a9951-7b00-ee81-3adb-94ea823e7bb6"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("48abafe0-1a6a-6fc2-90a5-03cd8e2dcff6"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("4d964280-fc84-e137-566a-c534220e0e19"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("52319fed-cd29-6cd9-b788-13bf7994193c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("524bb488-ea8b-f934-9752-0bb653021392"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("5e8956eb-be43-fd33-5e71-3550512f5adc"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("5f262377-2f74-4b7a-b30f-5c5b03662726"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7170e8cf-e2a4-9488-96be-f965cee55db3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7181b130-ea0d-6b38-6cfd-32d788253e01"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7c9a24c7-d723-9950-b8f9-5fa326ee5db7"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8900bc64-5871-5f90-87d1-74e4dcfff94d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8945aebb-3e8f-857f-373d-01211d72e8f0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("99711bfd-8016-3049-982d-fde6471339db"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a16d6025-b8b8-e4d1-ed71-0d8c18be3b52"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b8cffcca-161c-b686-9429-8fc72e300b99"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c6dfab87-231f-f621-9a09-8d751898c4dd"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("f34e6ac5-1e10-17ea-c9de-29eaefc80db5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("f7978155-ffa7-83d0-8ff4-ece731cd9188"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("29835514-b329-8e87-416c-06af74e74a3e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8f6a3377-c38a-3157-7101-b73b77d8bda5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9523c8c2-711d-5d97-643a-3be19cc5ecd3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a946fd73-3ccf-188c-bbff-8b2dce218107"));
        }
    }
}

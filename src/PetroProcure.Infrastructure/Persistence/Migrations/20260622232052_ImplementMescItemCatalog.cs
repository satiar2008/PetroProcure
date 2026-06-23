using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImplementMescItemCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                schema: "item",
                table: "MescItems",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "UnitOfMeasure",
                value: "M");

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                column: "UnitOfMeasure",
                value: "EA");

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"),
                column: "UnitOfMeasure",
                value: "EA");

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"),
                column: "UnitOfMeasure",
                value: "DEV");

            migrationBuilder.InsertData(
                schema: "item",
                table: "MescItems",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "CreatedBy", "Description", "GeneralGroupCode", "IsActive", "ModifiedAtUtc", "ModifiedBy", "UnitOfMeasure" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000005"), "1234560003", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "فلنج فولادی", "123456", true, null, null, "EA" },
                    { new Guid("40000000-0000-0000-0000-000000000006"), "1234560004", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "سه راهی فولادی", "123456", true, null, null, "EA" },
                    { new Guid("40000000-0000-0000-0000-000000000007"), "2233440002", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "شیر توپی صنعتی", "223344", true, null, null, "EA" },
                    { new Guid("40000000-0000-0000-0000-000000000008"), "2233440003", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "شیر یک طرفه صنعتی", "223344", true, null, null, "EA" },
                    { new Guid("40000000-0000-0000-0000-000000000009"), "3344550002", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "پمپ دنده‌ای", "334455", true, null, null, "DEV" },
                    { new Guid("40000000-0000-0000-0000-000000000010"), "3344550003", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "مکانیکال سیل پمپ", "334455", true, null, null, "EA" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000010"));

            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                schema: "item",
                table: "MescItems");
        }
    }
}

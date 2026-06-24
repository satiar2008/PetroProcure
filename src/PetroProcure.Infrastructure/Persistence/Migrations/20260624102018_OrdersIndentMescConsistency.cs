using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrdersIndentMescConsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UnitOfMeasureId",
                schema: "item",
                table: "MescItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE item.MescItems
                SET UnitOfMeasureId =
                    CASE UPPER(UnitOfMeasure)
                        WHEN 'M' THEN '20000000-0000-0000-0000-000000000002'
                        WHEN 'METER' THEN '20000000-0000-0000-0000-000000000002'
                        WHEN 'KG' THEN '20000000-0000-0000-0000-000000000003'
                        WHEN 'L' THEN '20000000-0000-0000-0000-000000000004'
                        WHEN 'PKG' THEN '20000000-0000-0000-0000-000000000005'
                        WHEN 'DEV' THEN '20000000-0000-0000-0000-000000000006'
                        ELSE '20000000-0000-0000-0000-000000000001'
                    END
                WHERE UnitOfMeasureId = '00000000-0000-0000-0000-000000000000'
                """);

            migrationBuilder.AddColumn<string>(
                name: "SourceDescription",
                schema: "indent",
                table: "Indents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceMaterialNeedId",
                schema: "indent",
                table: "Indents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceShortageAlertId",
                schema: "indent",
                table: "Indents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                schema: "indent",
                table: "Indents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "indent",
                table: "Indents",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"),
                columns: new[] { "SourceDescription", "SourceMaterialNeedId", "SourceShortageAlertId", "SourceType" },
                values: new object[] { "نمونه دستی", null, null, 0 });

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "UnitOfMeasureId",
                value: new Guid("20000000-0000-0000-0000-000000000002"));

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                column: "UnitOfMeasureId",
                value: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"),
                column: "UnitOfMeasureId",
                value: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"),
                column: "UnitOfMeasureId",
                value: new Guid("20000000-0000-0000-0000-000000000006"));

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000005"),
                column: "UnitOfMeasureId",
                value: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000006"),
                column: "UnitOfMeasureId",
                value: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000007"),
                column: "UnitOfMeasureId",
                value: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000008"),
                column: "UnitOfMeasureId",
                value: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000009"),
                column: "UnitOfMeasureId",
                value: new Guid("20000000-0000-0000-0000-000000000006"));

            migrationBuilder.UpdateData(
                schema: "item",
                table: "MescItems",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000010"),
                column: "UnitOfMeasureId",
                value: new Guid("20000000-0000-0000-0000-000000000001"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                schema: "item",
                table: "MescItems");

            migrationBuilder.DropColumn(
                name: "SourceDescription",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropColumn(
                name: "SourceMaterialNeedId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropColumn(
                name: "SourceShortageAlertId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropColumn(
                name: "SourceType",
                schema: "indent",
                table: "Indents");
        }
    }
}

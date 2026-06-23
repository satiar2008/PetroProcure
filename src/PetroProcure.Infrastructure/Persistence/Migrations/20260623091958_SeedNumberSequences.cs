using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedNumberSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "indent",
                table: "IndentSequences",
                columns: new[] { "Id", "LastSequence", "TypeDigit", "YearPart" },
                values: new object[] { new Guid("62000000-0000-0000-0000-000000000001"), 1, 3, 26 });

            migrationBuilder.InsertData(
                schema: "purchase",
                table: "PurchaseFileSequences",
                columns: new[] { "Id", "LastSequence", "Year" },
                values: new object[] { new Guid("72000000-0000-0000-0000-000000000001"), 1, 2026 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "indent",
                table: "IndentSequences",
                keyColumn: "Id",
                keyValue: new Guid("62000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                schema: "purchase",
                table: "PurchaseFileSequences",
                keyColumn: "Id",
                keyValue: new Guid("72000000-0000-0000-0000-000000000001"));
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminDefaultPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "identity",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"),
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEP4Pkm7AMVlZbxtV8i/536d40zis6sJQNNNupy7R4tqFLRzt2sPJTRQwhfqlkWaq3g==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "identity",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"),
                column: "PasswordHash",
                value: null);
        }
    }
}

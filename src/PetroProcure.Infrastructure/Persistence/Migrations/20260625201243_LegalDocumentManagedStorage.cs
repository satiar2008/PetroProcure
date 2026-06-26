using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LegalDocumentManagedStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "legal",
                table: "LegalDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                schema: "legal",
                table: "LegalDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                schema: "legal",
                table: "LegalDocuments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "legal",
                table: "LegalDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                schema: "legal",
                table: "LegalDocuments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Size",
                schema: "legal",
                table: "LegalDocuments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "SourceDocumentDate",
                schema: "legal",
                table: "LegalDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDocumentNumber",
                schema: "legal",
                table: "LegalDocuments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceDocumentTitle",
                schema: "legal",
                table: "LegalDocuments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                schema: "legal",
                table: "LegalDocuments",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_IsDeleted",
                schema: "legal",
                table: "LegalDocuments",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LegalDocuments_IsDeleted",
                schema: "legal",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "legal",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                schema: "legal",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "Extension",
                schema: "legal",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "legal",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "MimeType",
                schema: "legal",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "Size",
                schema: "legal",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "SourceDocumentDate",
                schema: "legal",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "SourceDocumentNumber",
                schema: "legal",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "SourceDocumentTitle",
                schema: "legal",
                table: "LegalDocuments");

            migrationBuilder.DropColumn(
                name: "StoredFileName",
                schema: "legal",
                table: "LegalDocuments");
        }
    }
}

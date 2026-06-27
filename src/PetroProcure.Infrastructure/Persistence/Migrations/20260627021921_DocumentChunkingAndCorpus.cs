using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DocumentChunkingAndCorpus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DocumentId",
                schema: "ai",
                table: "AiDocumentChunks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "ai",
                table: "AiDocumentChunks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LegalClauseId",
                schema: "ai",
                table: "AiDocumentChunks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseFileId",
                schema: "ai",
                table: "AiDocumentChunks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_DocumentId",
                schema: "ai",
                table: "AiDocumentChunks",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_LegalClauseId",
                schema: "ai",
                table: "AiDocumentChunks",
                column: "LegalClauseId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_PurchaseFileId",
                schema: "ai",
                table: "AiDocumentChunks",
                column: "PurchaseFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AiDocumentChunks_DocumentId",
                schema: "ai",
                table: "AiDocumentChunks");

            migrationBuilder.DropIndex(
                name: "IX_AiDocumentChunks_LegalClauseId",
                schema: "ai",
                table: "AiDocumentChunks");

            migrationBuilder.DropIndex(
                name: "IX_AiDocumentChunks_PurchaseFileId",
                schema: "ai",
                table: "AiDocumentChunks");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                schema: "ai",
                table: "AiDocumentChunks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "ai",
                table: "AiDocumentChunks");

            migrationBuilder.DropColumn(
                name: "LegalClauseId",
                schema: "ai",
                table: "AiDocumentChunks");

            migrationBuilder.DropColumn(
                name: "PurchaseFileId",
                schema: "ai",
                table: "AiDocumentChunks");
        }
    }
}

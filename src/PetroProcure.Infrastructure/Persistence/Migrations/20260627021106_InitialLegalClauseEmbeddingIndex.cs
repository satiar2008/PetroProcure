using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialLegalClauseEmbeddingIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiDocumentChunks",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ordinal = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenCount = table.Column<int>(type: "int", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiDocumentChunks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiEmbeddings",
                schema: "ai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChunkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VectorJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dimensions = table.Column<int>(type: "int", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiEmbeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiEmbeddings_AiDocumentChunks_ChunkId",
                        column: x => x.ChunkId,
                        principalSchema: "ai",
                        principalTable: "AiDocumentChunks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_ContentHash",
                schema: "ai",
                table: "AiDocumentChunks",
                column: "ContentHash");

            migrationBuilder.CreateIndex(
                name: "IX_AiDocumentChunks_SourceType_SourceId_Ordinal",
                schema: "ai",
                table: "AiDocumentChunks",
                columns: new[] { "SourceType", "SourceId", "Ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiEmbeddings_ChunkId",
                schema: "ai",
                table: "AiEmbeddings",
                column: "ChunkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiEmbeddings_Model_Dimensions",
                schema: "ai",
                table: "AiEmbeddings",
                columns: new[] { "Model", "Dimensions" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiEmbeddings",
                schema: "ai");

            migrationBuilder.DropTable(
                name: "AiDocumentChunks",
                schema: "ai");
        }
    }
}

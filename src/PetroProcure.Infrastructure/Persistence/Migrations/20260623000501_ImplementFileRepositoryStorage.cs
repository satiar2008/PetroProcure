using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImplementFileRepositoryStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileDocuments_Type",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropIndex(
                name: "IX_DocumentVersions_FileDocumentId",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "ModifiedAtUtc",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "doc",
                table: "FileDocuments",
                newName: "DocumentType");

            migrationBuilder.RenameColumn(
                name: "VersionNumber",
                schema: "doc",
                table: "DocumentVersions",
                newName: "VersionNo");

            migrationBuilder.RenameColumn(
                name: "UploadedAtUtc",
                schema: "doc",
                table: "DocumentVersions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "OriginalFileName",
                schema: "doc",
                table: "DocumentVersions",
                newName: "StoredFileName");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentVersions_FileDocumentId_VersionNumber",
                schema: "doc",
                table: "DocumentVersions",
                newName: "IX_DocumentVersions_FileDocumentId_VersionNo");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                schema: "doc",
                table: "FileDocuments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "doc",
                table: "FileDocuments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VersionNo",
                schema: "doc",
                table: "FileDocuments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                schema: "doc",
                table: "FileDocuments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                schema: "doc",
                table: "FileDocuments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "doc",
                table: "FileDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                schema: "doc",
                table: "FileDocuments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                schema: "doc",
                table: "FileDocuments",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelativePath",
                schema: "doc",
                table: "FileDocuments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Size",
                schema: "doc",
                table: "FileDocuments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                schema: "doc",
                table: "FileDocuments",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                schema: "doc",
                table: "FileDocuments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "UploadedByUserId",
                schema: "doc",
                table: "FileDocuments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "doc",
                table: "DocumentVersions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                schema: "doc",
                table: "DocumentVersions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Size",
                schema: "doc",
                table: "DocumentVersions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_FileDocuments_DepartmentId",
                schema: "doc",
                table: "FileDocuments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDocuments_PurchaseFileId_DocumentType",
                schema: "doc",
                table: "FileDocuments",
                columns: new[] { "PurchaseFileId", "DocumentType" });

            migrationBuilder.CreateIndex(
                name: "IX_FileDocuments_UploadedByUserId",
                schema: "doc",
                table: "FileDocuments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_CreatedByUserId",
                schema: "doc",
                table: "DocumentVersions",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentVersions_Users_CreatedByUserId",
                schema: "doc",
                table: "DocumentVersions",
                column: "CreatedByUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FileDocuments_Departments_DepartmentId",
                schema: "doc",
                table: "FileDocuments",
                column: "DepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FileDocuments_Users_UploadedByUserId",
                schema: "doc",
                table: "FileDocuments",
                column: "UploadedByUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentVersions_Users_CreatedByUserId",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_FileDocuments_Departments_DepartmentId",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_FileDocuments_Users_UploadedByUserId",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropIndex(
                name: "IX_FileDocuments_DepartmentId",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropIndex(
                name: "IX_FileDocuments_PurchaseFileId_DocumentType",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropIndex(
                name: "IX_FileDocuments_UploadedByUserId",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropIndex(
                name: "IX_DocumentVersions_CreatedByUserId",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "VersionNo",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "Extension",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "Hash",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "MimeType",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "RelativePath",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "Size",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "StoredFileName",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "UploadedByUserId",
                schema: "doc",
                table: "FileDocuments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "Hash",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "Size",
                schema: "doc",
                table: "DocumentVersions");

            migrationBuilder.RenameColumn(
                name: "DocumentType",
                schema: "doc",
                table: "FileDocuments",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "VersionNo",
                schema: "doc",
                table: "DocumentVersions",
                newName: "VersionNumber");

            migrationBuilder.RenameColumn(
                name: "StoredFileName",
                schema: "doc",
                table: "DocumentVersions",
                newName: "OriginalFileName");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "doc",
                table: "DocumentVersions",
                newName: "UploadedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentVersions_FileDocumentId_VersionNo",
                schema: "doc",
                table: "DocumentVersions",
                newName: "IX_DocumentVersions_FileDocumentId_VersionNumber");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "doc",
                table: "FileDocuments",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                schema: "doc",
                table: "DocumentVersions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "doc",
                table: "DocumentVersions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "doc",
                table: "DocumentVersions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAtUtc",
                schema: "doc",
                table: "DocumentVersions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "doc",
                table: "DocumentVersions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileDocuments_Type",
                schema: "doc",
                table: "FileDocuments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_FileDocumentId",
                schema: "doc",
                table: "DocumentVersions",
                column: "FileDocumentId");
        }
    }
}

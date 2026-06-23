using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImplementPurchaseFileCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFileItems_MescItems_MescCode",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFileNotes_ApplicationUserProfiles_AuthorUserId",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFiles_Indents_IndentId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFileStatusHistories_ApplicationUserProfiles_ChangedByUserId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFiles_IndentId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFileNotes_AuthorUserId",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFileItems_MescCode",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_MescItems_Code",
                schema: "item",
                table: "MescItems");

            migrationBuilder.DropColumn(
                name: "AuthorUserId",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropColumn(
                name: "ModifiedAtUtc",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.RenameColumn(
                name: "Note",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "ChangedAtUtc",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                newName: "ChangedAt");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseFileStatusHistories_ChangedAtUtc",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                newName: "IX_PurchaseFileStatusHistories_ChangedAt");

            migrationBuilder.RenameColumn(
                name: "IndentId",
                schema: "purchase",
                table: "PurchaseFiles",
                newName: "SourceIndentId");

            migrationBuilder.RenameColumn(
                name: "Text",
                schema: "purchase",
                table: "PurchaseFileNotes",
                newName: "NoteText");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "purchase",
                table: "PurchaseFileNotes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                schema: "purchase",
                table: "PurchaseFileItems",
                newName: "RequestedQuantity");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "purchase",
                table: "PurchaseFileItems",
                newName: "SpecificDescription");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChangedByUserId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileNumber",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsibleUserId",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsInternal",
                schema: "purchase",
                table: "PurchaseFileNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedQuantity",
                schema: "purchase",
                table: "PurchaseFileItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GeneralDescription",
                schema: "purchase",
                table: "PurchaseFileItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MescGeneralGroupCode",
                schema: "purchase",
                table: "PurchaseFileItems",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "MescItemId",
                schema: "purchase",
                table: "PurchaseFileItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SourceIndentItemId",
                schema: "purchase",
                table: "PurchaseFileItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalDescription",
                schema: "purchase",
                table: "PurchaseFileItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitOfMeasureId",
                schema: "purchase",
                table: "PurchaseFileItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "PurchaseFileSequences",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastSequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseFileSequences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileStatusHistories_DepartmentId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFiles_CreatedByUserId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFiles_CurrentDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "CurrentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFiles_PurchaseDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "PurchaseDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFiles_ResponsibleUserId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFiles_SourceIndentId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "SourceIndentId",
                unique: true,
                filter: "[SourceIndentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileNotes_DepartmentId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileNotes_UserId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileItems_MescGeneralGroupCode",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "MescGeneralGroupCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileItems_MescItemId",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "MescItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileItems_SourceIndentItemId",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "SourceIndentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileItems_UnitOfMeasureId",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileSequences_Year",
                schema: "purchase",
                table: "PurchaseFileSequences",
                column: "Year",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFileItems_IndentItems_SourceIndentItemId",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "SourceIndentItemId",
                principalSchema: "indent",
                principalTable: "IndentItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFileItems_MescItems_MescItemId",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "MescItemId",
                principalSchema: "item",
                principalTable: "MescItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFileItems_UnitOfMeasures_UnitOfMeasureId",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "UnitOfMeasureId",
                principalSchema: "item",
                principalTable: "UnitOfMeasures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFileNotes_Departments_DepartmentId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                column: "DepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFileNotes_Users_UserId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFiles_Departments_CurrentDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "CurrentDepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFiles_Departments_PurchaseDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "PurchaseDepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFiles_Indents_SourceIndentId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "SourceIndentId",
                principalSchema: "indent",
                principalTable: "Indents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFiles_Users_CreatedByUserId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "CreatedByUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFiles_Users_ResponsibleUserId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "ResponsibleUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFileStatusHistories_Departments_DepartmentId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                column: "DepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFileStatusHistories_Users_ChangedByUserId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                column: "ChangedByUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFileItems_IndentItems_SourceIndentItemId",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFileItems_MescItems_MescItemId",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFileItems_UnitOfMeasures_UnitOfMeasureId",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFileNotes_Departments_DepartmentId",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFileNotes_Users_UserId",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFiles_Departments_CurrentDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFiles_Departments_PurchaseDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFiles_Indents_SourceIndentId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFiles_Users_CreatedByUserId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFiles_Users_ResponsibleUserId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFileStatusHistories_Departments_DepartmentId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFileStatusHistories_Users_ChangedByUserId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories");

            migrationBuilder.DropTable(
                name: "PurchaseFileSequences",
                schema: "purchase");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFileStatusHistories_DepartmentId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFiles_CreatedByUserId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFiles_CurrentDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFiles_PurchaseDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFiles_ResponsibleUserId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFiles_SourceIndentId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFileNotes_DepartmentId",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFileNotes_UserId",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFileItems_MescGeneralGroupCode",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFileItems_MescItemId",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFileItems_SourceIndentItemId",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseFileItems_UnitOfMeasureId",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropColumn(
                name: "CurrentDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropColumn(
                name: "PurchaseDepartmentId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserId",
                schema: "purchase",
                table: "PurchaseFiles");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropColumn(
                name: "IsInternal",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "purchase",
                table: "PurchaseFileNotes");

            migrationBuilder.DropColumn(
                name: "ApprovedQuantity",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropColumn(
                name: "GeneralDescription",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropColumn(
                name: "MescGeneralGroupCode",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropColumn(
                name: "MescItemId",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropColumn(
                name: "SourceIndentItemId",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropColumn(
                name: "TechnicalDescription",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                schema: "purchase",
                table: "PurchaseFileItems");

            migrationBuilder.RenameColumn(
                name: "Reason",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "ChangedAt",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                newName: "ChangedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseFileStatusHistories_ChangedAt",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                newName: "IX_PurchaseFileStatusHistories_ChangedAtUtc");

            migrationBuilder.RenameColumn(
                name: "SourceIndentId",
                schema: "purchase",
                table: "PurchaseFiles",
                newName: "IndentId");

            migrationBuilder.RenameColumn(
                name: "NoteText",
                schema: "purchase",
                table: "PurchaseFileNotes",
                newName: "Text");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "purchase",
                table: "PurchaseFileNotes",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "SpecificDescription",
                schema: "purchase",
                table: "PurchaseFileItems",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "RequestedQuantity",
                schema: "purchase",
                table: "PurchaseFileItems",
                newName: "Quantity");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChangedByUserId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "FileNumber",
                schema: "purchase",
                table: "PurchaseFiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorUserId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "purchase",
                table: "PurchaseFileNotes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAtUtc",
                schema: "purchase",
                table: "PurchaseFileNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "purchase",
                table: "PurchaseFileNotes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_MescItems_Code",
                schema: "item",
                table: "MescItems",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFiles_IndentId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "IndentId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileNotes_AuthorUserId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileItems_MescCode",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "MescCode");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFileItems_MescItems_MescCode",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "MescCode",
                principalSchema: "item",
                principalTable: "MescItems",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFileNotes_ApplicationUserProfiles_AuthorUserId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                column: "AuthorUserId",
                principalSchema: "org",
                principalTable: "ApplicationUserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFiles_Indents_IndentId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "IndentId",
                principalSchema: "indent",
                principalTable: "Indents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseFileStatusHistories_ApplicationUserProfiles_ChangedByUserId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                column: "ChangedByUserId",
                principalSchema: "org",
                principalTable: "ApplicationUserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImplementIndentPurchaseRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndentItems_MescItems_MescCode",
                schema: "indent",
                table: "IndentItems");

            migrationBuilder.DropIndex(
                name: "IX_IndentItems_MescCode",
                schema: "indent",
                table: "IndentItems");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "indent",
                table: "Indents",
                newName: "IndentType");

            migrationBuilder.RenameIndex(
                name: "UX_Indents_YearPart_TypeDigit_Sequence",
                schema: "indent",
                table: "Indents",
                newName: "IX_Indents_YearPart_TypeDigit_Sequence");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                schema: "indent",
                table: "IndentItems",
                newName: "RequestedQuantity");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "indent",
                table: "IndentItems",
                newName: "SpecificDescription");

            migrationBuilder.AlterColumn<string>(
                name: "IndentNumber",
                schema: "indent",
                table: "Indents",
                type: "nchar(7)",
                fixedLength: true,
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(7)",
                oldMaxLength: 7);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicantDepartmentId",
                schema: "indent",
                table: "Indents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "indent",
                table: "Indents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "indent",
                table: "Indents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "indent",
                table: "Indents",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "indent",
                table: "Indents",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestingDepartmentId",
                schema: "indent",
                table: "Indents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "indent",
                table: "Indents",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "UnitOfMeasureId",
                schema: "indent",
                table: "IndentItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneralDescription",
                schema: "indent",
                table: "IndentItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MescGeneralGroupCode",
                schema: "indent",
                table: "IndentItems",
                type: "nchar(6)",
                fixedLength: true,
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "MescItemId",
                schema: "indent",
                table: "IndentItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateOnly>(
                name: "RequiredDate",
                schema: "indent",
                table: "IndentItems",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalDescription",
                schema: "indent",
                table: "IndentItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IndentSequences",
                schema: "indent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YearPart = table.Column<int>(type: "int", nullable: false),
                    TypeDigit = table.Column<int>(type: "int", nullable: false),
                    LastSequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndentSequences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Indents_ApplicantDepartmentId",
                schema: "indent",
                table: "Indents",
                column: "ApplicantDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Indents_CreatedByUserId",
                schema: "indent",
                table: "Indents",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Indents_RequestingDepartmentId",
                schema: "indent",
                table: "Indents",
                column: "RequestingDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IndentItems_MescGeneralGroupCode",
                schema: "indent",
                table: "IndentItems",
                column: "MescGeneralGroupCode");

            migrationBuilder.CreateIndex(
                name: "IX_IndentItems_MescItemId",
                schema: "indent",
                table: "IndentItems",
                column: "MescItemId");

            migrationBuilder.CreateIndex(
                name: "IX_IndentSequences_YearPart_TypeDigit",
                schema: "indent",
                table: "IndentSequences",
                columns: new[] { "YearPart", "TypeDigit" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_IndentItems_MescItems_MescItemId",
                schema: "indent",
                table: "IndentItems",
                column: "MescItemId",
                principalSchema: "item",
                principalTable: "MescItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Indents_Departments_ApplicantDepartmentId",
                schema: "indent",
                table: "Indents",
                column: "ApplicantDepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Indents_Departments_RequestingDepartmentId",
                schema: "indent",
                table: "Indents",
                column: "RequestingDepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Indents_Users_CreatedByUserId",
                schema: "indent",
                table: "Indents",
                column: "CreatedByUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndentItems_MescItems_MescItemId",
                schema: "indent",
                table: "IndentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Indents_Departments_ApplicantDepartmentId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropForeignKey(
                name: "FK_Indents_Departments_RequestingDepartmentId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropForeignKey(
                name: "FK_Indents_Users_CreatedByUserId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropTable(
                name: "IndentSequences",
                schema: "indent");

            migrationBuilder.DropIndex(
                name: "IX_Indents_ApplicantDepartmentId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropIndex(
                name: "IX_Indents_CreatedByUserId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropIndex(
                name: "IX_Indents_RequestingDepartmentId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropIndex(
                name: "IX_IndentItems_MescGeneralGroupCode",
                schema: "indent",
                table: "IndentItems");

            migrationBuilder.DropIndex(
                name: "IX_IndentItems_MescItemId",
                schema: "indent",
                table: "IndentItems");

            migrationBuilder.DropColumn(
                name: "ApplicantDepartmentId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropColumn(
                name: "RequestingDepartmentId",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "indent",
                table: "Indents");

            migrationBuilder.DropColumn(
                name: "GeneralDescription",
                schema: "indent",
                table: "IndentItems");

            migrationBuilder.DropColumn(
                name: "MescGeneralGroupCode",
                schema: "indent",
                table: "IndentItems");

            migrationBuilder.DropColumn(
                name: "MescItemId",
                schema: "indent",
                table: "IndentItems");

            migrationBuilder.DropColumn(
                name: "RequiredDate",
                schema: "indent",
                table: "IndentItems");

            migrationBuilder.DropColumn(
                name: "TechnicalDescription",
                schema: "indent",
                table: "IndentItems");

            migrationBuilder.RenameColumn(
                name: "IndentType",
                schema: "indent",
                table: "Indents",
                newName: "Type");

            migrationBuilder.RenameIndex(
                name: "IX_Indents_YearPart_TypeDigit_Sequence",
                schema: "indent",
                table: "Indents",
                newName: "UX_Indents_YearPart_TypeDigit_Sequence");

            migrationBuilder.RenameColumn(
                name: "SpecificDescription",
                schema: "indent",
                table: "IndentItems",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "RequestedQuantity",
                schema: "indent",
                table: "IndentItems",
                newName: "Quantity");

            migrationBuilder.AlterColumn<string>(
                name: "IndentNumber",
                schema: "indent",
                table: "Indents",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(7)",
                oldFixedLength: true,
                oldMaxLength: 7);

            migrationBuilder.AlterColumn<Guid>(
                name: "UnitOfMeasureId",
                schema: "indent",
                table: "IndentItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_IndentItems_MescCode",
                schema: "indent",
                table: "IndentItems",
                column: "MescCode");

            migrationBuilder.AddForeignKey(
                name: "FK_IndentItems_MescItems_MescCode",
                schema: "indent",
                table: "IndentItems",
                column: "MescCode",
                principalSchema: "item",
                principalTable: "MescItems",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

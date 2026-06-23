using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImplementWorkflowActionMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowActionDefinitions",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FromDepartmentType = table.Column<int>(type: "int", nullable: false),
                    ToDepartmentType = table.Column<int>(type: "int", nullable: true),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    RequiredPermission = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequiresComment = table.Column<bool>(type: "bit", nullable: false),
                    IsReturnAction = table.Column<bool>(type: "bit", nullable: false),
                    IsFinalAction = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowActionDefinitions", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "workflow",
                table: "WorkflowActionDefinitions",
                columns: new[] { "Id", "Code", "FromDepartmentType", "FromStatus", "IsActive", "IsFinalAction", "IsReturnAction", "RequiredPermission", "RequiresComment", "Title", "ToDepartmentType", "ToStatus" },
                values: new object[,]
                {
                    { new Guid("0897f8e3-0842-8106-1c51-d93a1f9ab303"), "APPLICANT_TO_PURCHASE", 4, 5, true, false, false, "PurchaseFile.SendToDepartment", true, "اتمام بررسی فنی و بازگشت به خرید", 1, 4 },
                    { new Guid("26447f13-df5b-6502-e794-8b2f60304bac"), "PURCHASE_TO_WAREHOUSE", 1, 4, true, false, false, "PurchaseFile.SendToDepartment", false, "ارسال به انبار", 3, 10 },
                    { new Guid("2790be44-d75d-8810-1265-dbf32bd896b7"), "ORDERS_TO_PURCHASE", 2, 3, true, false, false, "PurchaseFile.SendToDepartment", false, "ارسال به واحد خرید", 1, 4 },
                    { new Guid("7b4f9380-0c9a-641d-8cc9-5465be18c446"), "PURCHASE_TO_TENDER", 1, 4, true, false, false, "PurchaseFile.SendToDepartment", true, "ارسال به کمیسیون مناقصه", 5, 6 },
                    { new Guid("7bfe60ea-7d74-6b91-53c4-1fe1463f0b54"), "RETURN_PREVIOUS", 1, 4, true, false, true, "PurchaseFile.SendToDepartment", true, "بازگشت به واحد قبلی", null, 5 },
                    { new Guid("ac7c9aca-b5c1-8b9a-e285-6d7dc085ef78"), "TENDER_TO_PURCHASE", 5, 6, true, false, false, "PurchaseFile.SendToDepartment", true, "بازگشت نتیجه کمیسیون به خرید", 1, 4 },
                    { new Guid("b55829a7-f0e0-a6a6-2a51-bdb697b65016"), "WAREHOUSE_TO_PURCHASE", 3, 10, true, false, false, "PurchaseFile.SendToDepartment", true, "بازگشت رسید انبار به خرید", 1, 4 },
                    { new Guid("c5da76fd-c7e2-b386-84c5-173f1b03ff63"), "PURCHASE_COMPLETE", 1, 4, true, true, false, "PurchaseFile.SendToDepartment", true, "تکمیل پرونده", null, 11 },
                    { new Guid("f3145048-cd50-4764-874d-c15c8d47280b"), "PURCHASE_TO_APPLICANT", 1, 4, true, false, false, "PurchaseFile.SendToDepartment", true, "ارسال برای بررسی فنی", 4, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowActionDefinitions_Code",
                schema: "workflow",
                table: "WorkflowActionDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowActionDefinitions_FromDepartmentType_FromStatus_IsActive",
                schema: "workflow",
                table: "WorkflowActionDefinitions",
                columns: new[] { "FromDepartmentType", "FromStatus", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowActionDefinitions",
                schema: "workflow");
        }
    }
}

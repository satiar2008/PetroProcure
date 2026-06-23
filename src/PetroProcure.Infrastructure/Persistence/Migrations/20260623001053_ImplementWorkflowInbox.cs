using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImplementWorkflowInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InboxTasks_ApplicationUserProfiles_AssignedUserId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_InboxTasks_PurchaseFiles_PurchaseFileId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_PurchaseFiles_PurchaseFileId",
                schema: "workflow",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_WorkflowInstanceId_Order",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_InboxTasks_AssignedDepartment",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropIndex(
                name: "IX_InboxTasks_Status",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "DepartmentType",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "Order",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "workflow",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "workflow",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "AssignedDepartment",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropColumn(
                name: "CreatedAtTaskUtc",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.RenameColumn(
                name: "ModifiedAtUtc",
                schema: "workflow",
                table: "WorkflowSteps",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "workflow",
                table: "WorkflowSteps",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "PurchaseFileId",
                schema: "workflow",
                table: "WorkflowInstances",
                newName: "StartedByUserId");

            migrationBuilder.RenameColumn(
                name: "ModifiedAtUtc",
                schema: "workflow",
                table: "WorkflowInstances",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "workflow",
                table: "WorkflowInstances",
                newName: "StartedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstances_PurchaseFileId",
                schema: "workflow",
                table: "WorkflowInstances",
                newName: "IX_WorkflowInstances_StartedByUserId");

            migrationBuilder.RenameColumn(
                name: "ModifiedAtUtc",
                schema: "workflow",
                table: "InboxTasks",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "workflow",
                table: "InboxTasks",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "ActionName",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FromDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ToDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentDepartmentId",
                schema: "workflow",
                table: "WorkflowInstances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                schema: "workflow",
                table: "WorkflowInstances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                schema: "workflow",
                table: "WorkflowInstances",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "PurchaseFileId",
                schema: "workflow",
                table: "InboxTasks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedDepartmentId",
                schema: "workflow",
                table: "InboxTasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "workflow",
                table: "InboxTasks",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                schema: "workflow",
                table: "InboxTasks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IndentId",
                schema: "workflow",
                table: "InboxTasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowInstanceId",
                schema: "workflow",
                table: "InboxTasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_CompletedByUserId",
                schema: "workflow",
                table: "WorkflowSteps",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_CreatedByUserId",
                schema: "workflow",
                table: "WorkflowSteps",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_FromDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps",
                column: "FromDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_ToDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps",
                column: "ToDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_CurrentDepartmentId",
                schema: "workflow",
                table: "WorkflowInstances",
                column: "CurrentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_EntityType_EntityId",
                schema: "workflow",
                table: "WorkflowInstances",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxTasks_AssignedDepartmentId_Status",
                schema: "workflow",
                table: "InboxTasks",
                columns: new[] { "AssignedDepartmentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxTasks_IndentId",
                schema: "workflow",
                table: "InboxTasks",
                column: "IndentId");

            migrationBuilder.CreateIndex(
                name: "IX_InboxTasks_WorkflowInstanceId",
                schema: "workflow",
                table: "InboxTasks",
                column: "WorkflowInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InboxTasks_Departments_AssignedDepartmentId",
                schema: "workflow",
                table: "InboxTasks",
                column: "AssignedDepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InboxTasks_Indents_IndentId",
                schema: "workflow",
                table: "InboxTasks",
                column: "IndentId",
                principalSchema: "indent",
                principalTable: "Indents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InboxTasks_PurchaseFiles_PurchaseFileId",
                schema: "workflow",
                table: "InboxTasks",
                column: "PurchaseFileId",
                principalSchema: "purchase",
                principalTable: "PurchaseFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InboxTasks_Users_AssignedUserId",
                schema: "workflow",
                table: "InboxTasks",
                column: "AssignedUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InboxTasks_WorkflowInstances_WorkflowInstanceId",
                schema: "workflow",
                table: "InboxTasks",
                column: "WorkflowInstanceId",
                principalSchema: "workflow",
                principalTable: "WorkflowInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Departments_CurrentDepartmentId",
                schema: "workflow",
                table: "WorkflowInstances",
                column: "CurrentDepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Users_StartedByUserId",
                schema: "workflow",
                table: "WorkflowInstances",
                column: "StartedByUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Departments_FromDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps",
                column: "FromDepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Departments_ToDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps",
                column: "ToDepartmentId",
                principalSchema: "org",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Users_CompletedByUserId",
                schema: "workflow",
                table: "WorkflowSteps",
                column: "CompletedByUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowSteps_Users_CreatedByUserId",
                schema: "workflow",
                table: "WorkflowSteps",
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
                name: "FK_InboxTasks_Departments_AssignedDepartmentId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_InboxTasks_Indents_IndentId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_InboxTasks_PurchaseFiles_PurchaseFileId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_InboxTasks_Users_AssignedUserId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_InboxTasks_WorkflowInstances_WorkflowInstanceId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Departments_CurrentDepartmentId",
                schema: "workflow",
                table: "WorkflowInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Users_StartedByUserId",
                schema: "workflow",
                table: "WorkflowInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Departments_FromDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Departments_ToDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Users_CompletedByUserId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowSteps_Users_CreatedByUserId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_CompletedByUserId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_CreatedByUserId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_FromDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowSteps_ToDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_CurrentDepartmentId",
                schema: "workflow",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowInstances_EntityType_EntityId",
                schema: "workflow",
                table: "WorkflowInstances");

            migrationBuilder.DropIndex(
                name: "IX_InboxTasks_AssignedDepartmentId_Status",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropIndex(
                name: "IX_InboxTasks_IndentId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropIndex(
                name: "IX_InboxTasks_WorkflowInstanceId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropColumn(
                name: "ActionName",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "Comment",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "FromDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "ToDepartmentId",
                schema: "workflow",
                table: "WorkflowSteps");

            migrationBuilder.DropColumn(
                name: "CurrentDepartmentId",
                schema: "workflow",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "EntityId",
                schema: "workflow",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "EntityType",
                schema: "workflow",
                table: "WorkflowInstances");

            migrationBuilder.DropColumn(
                name: "AssignedDepartmentId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropColumn(
                name: "DueDate",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropColumn(
                name: "IndentId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.DropColumn(
                name: "WorkflowInstanceId",
                schema: "workflow",
                table: "InboxTasks");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "workflow",
                table: "WorkflowSteps",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                schema: "workflow",
                table: "WorkflowSteps",
                newName: "ModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "StartedByUserId",
                schema: "workflow",
                table: "WorkflowInstances",
                newName: "PurchaseFileId");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                schema: "workflow",
                table: "WorkflowInstances",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                schema: "workflow",
                table: "WorkflowInstances",
                newName: "ModifiedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstances_StartedByUserId",
                schema: "workflow",
                table: "WorkflowInstances",
                newName: "IX_WorkflowInstances_PurchaseFileId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "workflow",
                table: "InboxTasks",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                schema: "workflow",
                table: "InboxTasks",
                newName: "ModifiedAtUtc");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentType",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "workflow",
                table: "WorkflowSteps",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "workflow",
                table: "WorkflowInstances",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "workflow",
                table: "WorkflowInstances",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PurchaseFileId",
                schema: "workflow",
                table: "InboxTasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedDepartment",
                schema: "workflow",
                table: "InboxTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                schema: "workflow",
                table: "InboxTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtTaskUtc",
                schema: "workflow",
                table: "InboxTasks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "workflow",
                table: "InboxTasks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "workflow",
                table: "InboxTasks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowInstanceId_Order",
                schema: "workflow",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowInstanceId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboxTasks_AssignedDepartment",
                schema: "workflow",
                table: "InboxTasks",
                column: "AssignedDepartment");

            migrationBuilder.CreateIndex(
                name: "IX_InboxTasks_Status",
                schema: "workflow",
                table: "InboxTasks",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_InboxTasks_ApplicationUserProfiles_AssignedUserId",
                schema: "workflow",
                table: "InboxTasks",
                column: "AssignedUserId",
                principalSchema: "org",
                principalTable: "ApplicationUserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InboxTasks_PurchaseFiles_PurchaseFileId",
                schema: "workflow",
                table: "InboxTasks",
                column: "PurchaseFileId",
                principalSchema: "purchase",
                principalTable: "PurchaseFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_PurchaseFiles_PurchaseFileId",
                schema: "workflow",
                table: "WorkflowInstances",
                column: "PurchaseFileId",
                principalSchema: "purchase",
                principalTable: "PurchaseFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

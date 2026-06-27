using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImplementAiAsyncJobQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiFindings_AiEvaluationResults_EvaluationResultId",
                schema: "ai",
                table: "AiFindings");

            migrationBuilder.DropForeignKey(
                name: "FK_AiRecommendations_AiEvaluationResults_EvaluationResultId",
                schema: "ai",
                table: "AiRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_AiEvaluationJobs_PurchaseFileId",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "Severity",
                schema: "ai",
                table: "AiRecommendations");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "ai",
                table: "AiFindings");

            migrationBuilder.RenameColumn(
                name: "EvaluationResultId",
                schema: "ai",
                table: "AiRecommendations",
                newName: "ResultId");

            migrationBuilder.RenameIndex(
                name: "IX_AiRecommendations_EvaluationResultId",
                schema: "ai",
                table: "AiRecommendations",
                newName: "IX_AiRecommendations_ResultId");

            migrationBuilder.RenameColumn(
                name: "EvaluationResultId",
                schema: "ai",
                table: "AiFindings",
                newName: "ResultId");

            migrationBuilder.RenameIndex(
                name: "IX_AiFindings_EvaluationResultId",
                schema: "ai",
                table: "AiFindings",
                newName: "IX_AiFindings_ResultId");

            migrationBuilder.RenameColumn(
                name: "PurchaseFileId",
                schema: "ai",
                table: "AiEvaluationResults",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "EvaluationType",
                schema: "ai",
                table: "AiEvaluationResults",
                newName: "AnalysisType");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "ai",
                table: "AiEvaluationResults",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "AiEvaluationJobId",
                schema: "ai",
                table: "AiEvaluationResults",
                newName: "JobId");

            migrationBuilder.RenameColumn(
                name: "PurchaseFileId",
                schema: "ai",
                table: "AiEvaluationJobs",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "EvaluationType",
                schema: "ai",
                table: "AiEvaluationJobs",
                newName: "AnalysisType");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "ai",
                table: "AiEvaluationJobs",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                schema: "ai",
                table: "AiEvaluationJobs",
                newName: "CompletedAtUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "AiRecommendations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                schema: "ai",
                table: "AiRecommendations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SuggestedAction",
                schema: "ai",
                table: "AiRecommendations",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                schema: "ai",
                table: "AiFindings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "AiFindings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RelatedClauseCode",
                schema: "ai",
                table: "AiFindings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedDocumentId",
                schema: "ai",
                table: "AiFindings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DurationMs",
                schema: "ai",
                table: "AiEvaluationResults",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InputTokens",
                schema: "ai",
                table: "AiEvaluationResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                schema: "ai",
                table: "AiEvaluationResults",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutputTokens",
                schema: "ai",
                table: "AiEvaluationResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                schema: "ai",
                table: "AiEvaluationResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawResultJson",
                schema: "ai",
                table: "AiEvaluationResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalTokens",
                schema: "ai",
                table: "AiEvaluationResults",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                schema: "ai",
                table: "AiEvaluationResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "PurchaseFile");

            migrationBuilder.AddColumn<string>(
                name: "SourceSystem",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "PetroProcure");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedToAiCoreAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContextJson",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalJobId",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LockedBy",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetryCount",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRetryAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercent",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequestJson",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResultJson",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "ai",
                table: "AiEvaluationJobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Queued");

            migrationBuilder.Sql("""
                UPDATE ai.AiEvaluationJobs
                SET SourceSystem = 'PetroProcure',
                    EntityType = 'PurchaseFile',
                    CorrelationId = CONVERT(nvarchar(100), Id),
                    RequestJson = '{}',
                    Status = CASE WHEN CompletedAtUtc IS NULL THEN 'Queued' ELSE 'Completed' END,
                    ProgressPercent = CASE WHEN CompletedAtUtc IS NULL THEN 0 ELSE 100 END
                WHERE SourceSystem = '' OR EntityType = '' OR CorrelationId = '' OR RequestJson = '' OR Status = '';
                """);

            migrationBuilder.Sql("""
                UPDATE ai.AiEvaluationResults
                SET EntityType = 'PurchaseFile',
                    RawResultJson = Summary
                WHERE EntityType = '' OR RawResultJson = '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AiEvaluationResults_JobId",
                schema: "ai",
                table: "AiEvaluationResults",
                column: "JobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiEvaluationJobs_CorrelationId",
                schema: "ai",
                table: "AiEvaluationJobs",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiEvaluationJobs_EntityType_EntityId",
                schema: "ai",
                table: "AiEvaluationJobs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AiEvaluationJobs_ExternalJobId",
                schema: "ai",
                table: "AiEvaluationJobs",
                column: "ExternalJobId",
                unique: true,
                filter: "[ExternalJobId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AiEvaluationJobs_Status_NextRetryAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs",
                columns: new[] { "Status", "NextRetryAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_AiEvaluationResults_AiEvaluationJobs_JobId",
                schema: "ai",
                table: "AiEvaluationResults",
                column: "JobId",
                principalSchema: "ai",
                principalTable: "AiEvaluationJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AiFindings_AiEvaluationResults_ResultId",
                schema: "ai",
                table: "AiFindings",
                column: "ResultId",
                principalSchema: "ai",
                principalTable: "AiEvaluationResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AiRecommendations_AiEvaluationResults_ResultId",
                schema: "ai",
                table: "AiRecommendations",
                column: "ResultId",
                principalSchema: "ai",
                principalTable: "AiEvaluationResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiEvaluationResults_AiEvaluationJobs_JobId",
                schema: "ai",
                table: "AiEvaluationResults");

            migrationBuilder.DropForeignKey(
                name: "FK_AiFindings_AiEvaluationResults_ResultId",
                schema: "ai",
                table: "AiFindings");

            migrationBuilder.DropForeignKey(
                name: "FK_AiRecommendations_AiEvaluationResults_ResultId",
                schema: "ai",
                table: "AiRecommendations");

            migrationBuilder.DropIndex(
                name: "IX_AiEvaluationResults_JobId",
                schema: "ai",
                table: "AiEvaluationResults");

            migrationBuilder.DropIndex(
                name: "IX_AiEvaluationJobs_CorrelationId",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropIndex(
                name: "IX_AiEvaluationJobs_EntityType_EntityId",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropIndex(
                name: "IX_AiEvaluationJobs_ExternalJobId",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropIndex(
                name: "IX_AiEvaluationJobs_Status_NextRetryAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "AiRecommendations");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "ai",
                table: "AiRecommendations");

            migrationBuilder.DropColumn(
                name: "SuggestedAction",
                schema: "ai",
                table: "AiRecommendations");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "AiFindings");

            migrationBuilder.DropColumn(
                name: "RelatedClauseCode",
                schema: "ai",
                table: "AiFindings");

            migrationBuilder.DropColumn(
                name: "RelatedDocumentId",
                schema: "ai",
                table: "AiFindings");

            migrationBuilder.DropColumn(
                name: "EntityType",
                schema: "ai",
                table: "AiEvaluationResults");

            migrationBuilder.DropColumn(
                name: "DurationMs",
                schema: "ai",
                table: "AiEvaluationResults");

            migrationBuilder.DropColumn(
                name: "InputTokens",
                schema: "ai",
                table: "AiEvaluationResults");

            migrationBuilder.DropColumn(
                name: "Model",
                schema: "ai",
                table: "AiEvaluationResults");

            migrationBuilder.DropColumn(
                name: "OutputTokens",
                schema: "ai",
                table: "AiEvaluationResults");

            migrationBuilder.DropColumn(
                name: "Provider",
                schema: "ai",
                table: "AiEvaluationResults");

            migrationBuilder.DropColumn(
                name: "RawResultJson",
                schema: "ai",
                table: "AiEvaluationResults");

            migrationBuilder.DropColumn(
                name: "TotalTokens",
                schema: "ai",
                table: "AiEvaluationResults");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "ContextJson",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "EntityType",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "ExternalJobId",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "LockedAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "LockedBy",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "MaxRetryCount",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "NextRetryAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "ProgressPercent",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "RequestJson",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "ResultJson",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "SourceSystem",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "ai",
                table: "AiEvaluationJobs");

            migrationBuilder.RenameColumn(
                name: "ResultId",
                schema: "ai",
                table: "AiRecommendations",
                newName: "EvaluationResultId");

            migrationBuilder.RenameIndex(
                name: "IX_AiRecommendations_ResultId",
                schema: "ai",
                table: "AiRecommendations",
                newName: "IX_AiRecommendations_EvaluationResultId");

            migrationBuilder.RenameColumn(
                name: "ResultId",
                schema: "ai",
                table: "AiFindings",
                newName: "EvaluationResultId");

            migrationBuilder.RenameIndex(
                name: "IX_AiFindings_ResultId",
                schema: "ai",
                table: "AiFindings",
                newName: "IX_AiFindings_EvaluationResultId");

            migrationBuilder.RenameColumn(
                name: "JobId",
                schema: "ai",
                table: "AiEvaluationResults",
                newName: "AiEvaluationJobId");

            migrationBuilder.RenameColumn(
                name: "AnalysisType",
                schema: "ai",
                table: "AiEvaluationResults",
                newName: "EvaluationType");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                schema: "ai",
                table: "AiEvaluationResults",
                newName: "PurchaseFileId");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "AiEvaluationResults",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CompletedAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "AnalysisType",
                schema: "ai",
                table: "AiEvaluationJobs",
                newName: "EvaluationType");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                schema: "ai",
                table: "AiEvaluationJobs",
                newName: "PurchaseFileId");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                schema: "ai",
                table: "AiEvaluationJobs",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<int>(
                name: "Severity",
                schema: "ai",
                table: "AiRecommendations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Severity",
                schema: "ai",
                table: "AiFindings",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "ai",
                table: "AiFindings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiEvaluationJobs_PurchaseFileId",
                schema: "ai",
                table: "AiEvaluationJobs",
                column: "PurchaseFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiFindings_AiEvaluationResults_EvaluationResultId",
                schema: "ai",
                table: "AiFindings",
                column: "EvaluationResultId",
                principalSchema: "ai",
                principalTable: "AiEvaluationResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AiRecommendations_AiEvaluationResults_EvaluationResultId",
                schema: "ai",
                table: "AiRecommendations",
                column: "EvaluationResultId",
                principalSchema: "ai",
                principalTable: "AiEvaluationResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

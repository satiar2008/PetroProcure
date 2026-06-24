using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TenderFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tender");

            migrationBuilder.CreateTable(
                name: "Tenders",
                schema: "tender",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceInquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpeningDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenderSequences",
                schema: "tender",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastSequence = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenderBids",
                schema: "tender",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BidNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TechnicalScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CommercialScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FinalScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderBids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderBids_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderDecisions",
                schema: "tender",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedTenderBidId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SelectedSupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderDecisions_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderDocuments",
                schema: "tender",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderDocuments_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderEvaluations",
                schema: "tender",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderBidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EvaluatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderEvaluations_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderItems",
                schema: "tender",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MescItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MescGeneralGroupCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GeneralDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SpecificDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TechnicalDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderItems_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderParticipants",
                schema: "tender",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvitedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeclinedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeclineReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderParticipants_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderStages",
                schema: "tender",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderStages_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderBidItems",
                schema: "tender",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderBidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MescGeneralGroupCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GeneralDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SpecificDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TechnicalComplianceStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TechnicalNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderBidItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderBidItems_TenderBids_TenderBidId",
                        column: x => x.TenderBidId,
                        principalSchema: "tender",
                        principalTable: "TenderBids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("060b993c-39b6-b892-cb63-6fcd6bfc8ead"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.ManageDocuments", true, null, null, "Tender.ManageDocuments" },
                    { new Guid("16710564-5b73-748f-8789-d6167fa0c525"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.EvaluateQualification", true, null, null, "Tender.EvaluateQualification" },
                    { new Guid("1e9d4d40-408a-8d3c-d37b-8c96b3a513d0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.ReceiveBid", true, null, null, "Tender.ReceiveBid" },
                    { new Guid("26f066cd-c95a-3e10-cedb-35920a4d958c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.ManageParticipants", true, null, null, "Tender.ManageParticipants" },
                    { new Guid("2d022d6c-7f50-0d2b-7de6-cf9ddadd551a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.EvaluateCommercial", true, null, null, "Tender.EvaluateCommercial" },
                    { new Guid("305ce48e-a427-53e0-6a47-d4e331c09085"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.ManageItems", true, null, null, "Tender.ManageItems" },
                    { new Guid("466c9891-9a53-ba97-7dc0-859fb4382477"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.Cancel", true, null, null, "Tender.Cancel" },
                    { new Guid("5c5bb6f2-1aa8-0f54-e64c-ed40255ca40e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.Close", true, null, null, "Tender.Close" },
                    { new Guid("7b4ef4a2-04fc-2623-5312-6990db731812"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.Publish", true, null, null, "Tender.Publish" },
                    { new Guid("883411ca-7931-22c8-f720-19181575fd11"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.CompareBids", true, null, null, "Tender.CompareBids" },
                    { new Guid("8f1aec43-0462-75aa-5d76-6f790006fc4f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.SelectWinner", true, null, null, "Tender.SelectWinner" },
                    { new Guid("d628e2b5-ef09-40b8-2d8f-3e4219874ec3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.EvaluateTechnical", true, null, null, "Tender.EvaluateTechnical" },
                    { new Guid("f7462e4d-df55-b2d1-6f70-e52cf05e8173"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tender.Edit", true, null, null, "Tender.Edit" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("274de30c-ce80-7544-6d6f-d9a4e0501699"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("3d0bccfd-85cc-bbb7-32ef-74500ed50c77"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("38425a7c-1095-bd18-9843-8658f2cf410c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9c752dc9-c777-9dd1-8b40-5b0f0698690b"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("f4b0a4e1-d44c-c118-d69a-9e4529c44344"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9c752dc9-c777-9dd1-8b40-5b0f0698690b"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("0312a40e-1317-2a57-e8c4-1e284531031a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("883411ca-7931-22c8-f720-19181575fd11"), new Guid("92db8232-7861-b5b6-fba1-d5a94cfac12e") },
                    { new Guid("07b9a0fb-b4fe-d48c-fefa-a59d5adc6770"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("8f1aec43-0462-75aa-5d76-6f790006fc4f"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("0b7a4c99-c65e-2f65-de52-c8cde36d39b5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("305ce48e-a427-53e0-6a47-d4e331c09085"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("120f91b6-cdea-7b00-efc2-6433fffb44cc"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("8f1aec43-0462-75aa-5d76-6f790006fc4f"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("13f1379d-c0a8-8c40-27e1-df32ce9a08c4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2d022d6c-7f50-0d2b-7de6-cf9ddadd551a"), new Guid("92db8232-7861-b5b6-fba1-d5a94cfac12e") },
                    { new Guid("15253c4c-c971-7d22-47fd-03fb9916bff0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("883411ca-7931-22c8-f720-19181575fd11"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("18e293fe-74fe-e4f7-eb1e-b6a6e7c12bcf"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("060b993c-39b6-b892-cb63-6fcd6bfc8ead"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("22012ffc-cb90-6a2c-ec96-ae370beecc3b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f7462e4d-df55-b2d1-6f70-e52cf05e8173"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("23b9ba29-e12f-e815-2b93-a47da9180ac0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("1e9d4d40-408a-8d3c-d37b-8c96b3a513d0"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("29da5741-a4ca-6a84-3932-449b852bf8c5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("16710564-5b73-748f-8789-d6167fa0c525"), new Guid("92db8232-7861-b5b6-fba1-d5a94cfac12e") },
                    { new Guid("4e317c49-5b74-6fc5-8df2-fe45ff1b0946"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("466c9891-9a53-ba97-7dc0-859fb4382477"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("53b28b97-889d-4be5-cbe4-d0f97c9962b0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("16710564-5b73-748f-8789-d6167fa0c525"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("5e02de01-878e-0f26-1e1a-15de6609ff3d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("883411ca-7931-22c8-f720-19181575fd11"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("6556759a-51a3-f7f2-e92f-9fd234f6efd7"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("466c9891-9a53-ba97-7dc0-859fb4382477"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("6d02abce-d123-0f39-4268-7b1ec771d170"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("8f1aec43-0462-75aa-5d76-6f790006fc4f"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("6fa85ae2-eb9c-2389-3f16-c60349077ad8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d628e2b5-ef09-40b8-2d8f-3e4219874ec3"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("713dbf0a-5428-4a5c-9138-e4d11bb77793"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f7462e4d-df55-b2d1-6f70-e52cf05e8173"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("756f6967-b6f4-fc6d-c101-364910e6cc3d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("26f066cd-c95a-3e10-cedb-35920a4d958c"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("76d4c3db-f8af-4f67-4cbf-3ca6e97713d9"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("7b4ef4a2-04fc-2623-5312-6990db731812"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("7d9676d5-34ba-0d5a-ce38-a7289e7c3302"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d628e2b5-ef09-40b8-2d8f-3e4219874ec3"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("803f628a-5496-56d8-e185-0cbb1e126e88"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("26f066cd-c95a-3e10-cedb-35920a4d958c"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("860d8a29-973a-c50e-4644-8676a6701a79"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("26f066cd-c95a-3e10-cedb-35920a4d958c"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("8b99559b-0a21-66bd-f9b9-854cef61c2b0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("305ce48e-a427-53e0-6a47-d4e331c09085"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("8bc52db3-f170-0e9c-3b78-6fd500d4076c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f7462e4d-df55-b2d1-6f70-e52cf05e8173"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("8c35dfa2-8763-c249-74d3-968301f64e6e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("305ce48e-a427-53e0-6a47-d4e331c09085"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("8e317bd6-35a4-c9a3-baaf-d7600509e215"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("883411ca-7931-22c8-f720-19181575fd11"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("994ff1b8-9662-5ab4-306f-bdf92173ebe5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d628e2b5-ef09-40b8-2d8f-3e4219874ec3"), new Guid("92db8232-7861-b5b6-fba1-d5a94cfac12e") },
                    { new Guid("9b2efa4c-68c0-e795-9513-1812223ae641"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("1e9d4d40-408a-8d3c-d37b-8c96b3a513d0"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("9d02b727-da4d-7d6e-a25a-22529acd0b40"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2d022d6c-7f50-0d2b-7de6-cf9ddadd551a"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("a34c0ca8-8992-fb46-f29e-8cfc341bc5c6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2d022d6c-7f50-0d2b-7de6-cf9ddadd551a"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("af9675b4-74c2-ae1a-b110-ee8b690265a3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5c5bb6f2-1aa8-0f54-e64c-ed40255ca40e"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("c573f6dd-eaee-3608-2696-190f09221d36"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("883411ca-7931-22c8-f720-19181575fd11"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("ce8d7dc0-dabe-9e26-130b-0bb9653e95b5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("060b993c-39b6-b892-cb63-6fcd6bfc8ead"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("d6a0a34e-05fd-8144-3993-1f5125b4b8f1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("16710564-5b73-748f-8789-d6167fa0c525"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("d8da9d6f-6cb7-71c2-094f-60f46b75a9fc"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("7b4ef4a2-04fc-2623-5312-6990db731812"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("dcd1b5c6-4b60-cece-6192-7e8a680a140d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("1e9d4d40-408a-8d3c-d37b-8c96b3a513d0"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("ff28c3f0-07b8-630a-21c2-4e4959f3ff77"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5c5bb6f2-1aa8-0f54-e64c-ed40255ca40e"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenderBidItems_TenderBidId",
                schema: "tender",
                table: "TenderBidItems",
                column: "TenderBidId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderBids_TenderId_SupplierId",
                schema: "tender",
                table: "TenderBids",
                columns: new[] { "TenderId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_TenderDecisions_TenderId",
                schema: "tender",
                table: "TenderDecisions",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderDocuments_TenderId",
                schema: "tender",
                table: "TenderDocuments",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderEvaluations_TenderId",
                schema: "tender",
                table: "TenderEvaluations",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderItems_TenderId",
                schema: "tender",
                table: "TenderItems",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderParticipants_SupplierId",
                schema: "tender",
                table: "TenderParticipants",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderParticipants_TenderId",
                schema: "tender",
                table: "TenderParticipants",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenders_CreatedAt",
                schema: "tender",
                table: "Tenders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tenders_PurchaseFileId",
                schema: "tender",
                table: "Tenders",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenders_SourceInquiryId",
                schema: "tender",
                table: "Tenders",
                column: "SourceInquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenders_Status",
                schema: "tender",
                table: "Tenders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tenders_SubmissionDeadline",
                schema: "tender",
                table: "Tenders",
                column: "SubmissionDeadline");

            migrationBuilder.CreateIndex(
                name: "IX_Tenders_TenderNumber",
                schema: "tender",
                table: "Tenders",
                column: "TenderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenderSequences_Year",
                schema: "tender",
                table: "TenderSequences",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenderStages_TenderId",
                schema: "tender",
                table: "TenderStages",
                column: "TenderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenderBidItems",
                schema: "tender");

            migrationBuilder.DropTable(
                name: "TenderDecisions",
                schema: "tender");

            migrationBuilder.DropTable(
                name: "TenderDocuments",
                schema: "tender");

            migrationBuilder.DropTable(
                name: "TenderEvaluations",
                schema: "tender");

            migrationBuilder.DropTable(
                name: "TenderItems",
                schema: "tender");

            migrationBuilder.DropTable(
                name: "TenderParticipants",
                schema: "tender");

            migrationBuilder.DropTable(
                name: "TenderSequences",
                schema: "tender");

            migrationBuilder.DropTable(
                name: "TenderStages",
                schema: "tender");

            migrationBuilder.DropTable(
                name: "TenderBids",
                schema: "tender");

            migrationBuilder.DropTable(
                name: "Tenders",
                schema: "tender");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0312a40e-1317-2a57-e8c4-1e284531031a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("07b9a0fb-b4fe-d48c-fefa-a59d5adc6770"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0b7a4c99-c65e-2f65-de52-c8cde36d39b5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("120f91b6-cdea-7b00-efc2-6433fffb44cc"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("13f1379d-c0a8-8c40-27e1-df32ce9a08c4"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("15253c4c-c971-7d22-47fd-03fb9916bff0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("18e293fe-74fe-e4f7-eb1e-b6a6e7c12bcf"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("22012ffc-cb90-6a2c-ec96-ae370beecc3b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("23b9ba29-e12f-e815-2b93-a47da9180ac0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("274de30c-ce80-7544-6d6f-d9a4e0501699"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("29da5741-a4ca-6a84-3932-449b852bf8c5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("38425a7c-1095-bd18-9843-8658f2cf410c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("4e317c49-5b74-6fc5-8df2-fe45ff1b0946"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("53b28b97-889d-4be5-cbe4-d0f97c9962b0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("5e02de01-878e-0f26-1e1a-15de6609ff3d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6556759a-51a3-f7f2-e92f-9fd234f6efd7"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6d02abce-d123-0f39-4268-7b1ec771d170"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6fa85ae2-eb9c-2389-3f16-c60349077ad8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("713dbf0a-5428-4a5c-9138-e4d11bb77793"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("756f6967-b6f4-fc6d-c101-364910e6cc3d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("76d4c3db-f8af-4f67-4cbf-3ca6e97713d9"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7d9676d5-34ba-0d5a-ce38-a7289e7c3302"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("803f628a-5496-56d8-e185-0cbb1e126e88"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("860d8a29-973a-c50e-4644-8676a6701a79"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8b99559b-0a21-66bd-f9b9-854cef61c2b0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8bc52db3-f170-0e9c-3b78-6fd500d4076c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8c35dfa2-8763-c249-74d3-968301f64e6e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8e317bd6-35a4-c9a3-baaf-d7600509e215"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("994ff1b8-9662-5ab4-306f-bdf92173ebe5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("9b2efa4c-68c0-e795-9513-1812223ae641"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("9d02b727-da4d-7d6e-a25a-22529acd0b40"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a34c0ca8-8992-fb46-f29e-8cfc341bc5c6"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("af9675b4-74c2-ae1a-b110-ee8b690265a3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c573f6dd-eaee-3608-2696-190f09221d36"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ce8d7dc0-dabe-9e26-130b-0bb9653e95b5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d6a0a34e-05fd-8144-3993-1f5125b4b8f1"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d8da9d6f-6cb7-71c2-094f-60f46b75a9fc"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("dcd1b5c6-4b60-cece-6192-7e8a680a140d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("f4b0a4e1-d44c-c118-d69a-9e4529c44344"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ff28c3f0-07b8-630a-21c2-4e4959f3ff77"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("060b993c-39b6-b892-cb63-6fcd6bfc8ead"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("16710564-5b73-748f-8789-d6167fa0c525"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1e9d4d40-408a-8d3c-d37b-8c96b3a513d0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("26f066cd-c95a-3e10-cedb-35920a4d958c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2d022d6c-7f50-0d2b-7de6-cf9ddadd551a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("305ce48e-a427-53e0-6a47-d4e331c09085"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("466c9891-9a53-ba97-7dc0-859fb4382477"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5c5bb6f2-1aa8-0f54-e64c-ed40255ca40e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b4ef4a2-04fc-2623-5312-6990db731812"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("883411ca-7931-22c8-f720-19181575fd11"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8f1aec43-0462-75aa-5d76-6f790006fc4f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d628e2b5-ef09-40b8-2d8f-3e4219874ec3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f7462e4d-df55-b2d1-6f70-e52cf05e8173"));
        }
    }
}

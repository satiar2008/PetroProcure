using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TenderCommissionSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "commission");

            migrationBuilder.CreateTable(
                name: "TenderCommissionSessions",
                schema: "commission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SessionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderCommissionSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderCommissionSessions_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenderCommissionSessions_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenderCommissionSessionSequences",
                schema: "commission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderCommissionSessionSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenderCommissionAgendaItems",
                schema: "commission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RelatedTenderBidId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedSupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderCommissionAgendaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderCommissionAgendaItems_Suppliers_RelatedSupplierId",
                        column: x => x.RelatedSupplierId,
                        principalSchema: "supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenderCommissionAgendaItems_TenderBids_RelatedTenderBidId",
                        column: x => x.RelatedTenderBidId,
                        principalSchema: "tender",
                        principalTable: "TenderBids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenderCommissionAgendaItems_TenderCommissionSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "commission",
                        principalTable: "TenderCommissionSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderCommissionAttachments",
                schema: "commission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_TenderCommissionAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderCommissionAttachments_FileDocuments_FileDocumentId",
                        column: x => x.FileDocumentId,
                        principalSchema: "doc",
                        principalTable: "FileDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenderCommissionAttachments_TenderCommissionSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "commission",
                        principalTable: "TenderCommissionSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderCommissionDecisions",
                schema: "commission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DecisionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedTenderBidId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SelectedSupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DecisionText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderCommissionDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderCommissionDecisions_Suppliers_SelectedSupplierId",
                        column: x => x.SelectedSupplierId,
                        principalSchema: "supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenderCommissionDecisions_TenderBids_SelectedTenderBidId",
                        column: x => x.SelectedTenderBidId,
                        principalSchema: "tender",
                        principalTable: "TenderBids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenderCommissionDecisions_TenderCommissionSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "commission",
                        principalTable: "TenderCommissionSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenderCommissionDecisions_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenderCommissionMembers",
                schema: "commission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullNameSnapshot = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PositionSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AttendanceStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VoteStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VoteNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderCommissionMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderCommissionMembers_TenderCommissionSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "commission",
                        principalTable: "TenderCommissionSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderCommissionMinutes",
                schema: "commission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgendaItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderCommissionMinutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenderCommissionMinutes_TenderCommissionAgendaItems_AgendaItemId",
                        column: x => x.AgendaItemId,
                        principalSchema: "commission",
                        principalTable: "TenderCommissionAgendaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenderCommissionMinutes_TenderCommissionSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "commission",
                        principalTable: "TenderCommissionSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("3e9cc39c-94f4-dd9a-414a-4e485befb873"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.ManageAgenda", true, null, null, "Commission.ManageAgenda" },
                    { new Guid("427b013b-56ae-e153-9814-fb486b74a15c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.ManageMembers", true, null, null, "Commission.ManageMembers" },
                    { new Guid("4564acf5-7bd3-d480-4c24-90ca8d4ea5cb"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.Approve", true, null, null, "Commission.Approve" },
                    { new Guid("74c4beb3-578d-15fe-8017-c7a1bc8afb7e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.ManageDocuments", true, null, null, "Commission.ManageDocuments" },
                    { new Guid("84ca70ff-c4bd-6167-b445-0cdfa90da72c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.ManageMinutes", true, null, null, "Commission.ManageMinutes" },
                    { new Guid("99baaeef-560a-3a17-3780-50d9d264eff5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.Start", true, null, null, "Commission.Start" },
                    { new Guid("b2742550-1168-9871-636f-96f6e8317829"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.View", true, null, null, "Commission.View" },
                    { new Guid("bb10e3b6-1b66-ccb1-245c-795eb2252d71"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.Create", true, null, null, "Commission.Create" },
                    { new Guid("bf67f175-3503-4d60-db6e-01982084a6cc"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.Schedule", true, null, null, "Commission.Schedule" },
                    { new Guid("c7c5fdd0-07fe-393e-13ed-d31f6debfe27"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.Edit", true, null, null, "Commission.Edit" },
                    { new Guid("d7911f7a-ae45-45a1-19f2-bb822a9ff5fe"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.Cancel", true, null, null, "Commission.Cancel" },
                    { new Guid("f9c2e546-0054-6969-0e68-36d4a1d7c04c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.Complete", true, null, null, "Commission.Complete" },
                    { new Guid("fce70fbd-53e2-4745-1d8f-eee73349a94a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Commission.ManageDecisions", true, null, null, "Commission.ManageDecisions" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("13112e39-4089-a01a-fdf8-593dd2401d7e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("bb10e3b6-1b66-ccb1-245c-795eb2252d71"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("1c00a932-4a86-bd8f-a65b-2376dc50311f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("b2742550-1168-9871-636f-96f6e8317829"), new Guid("92db8232-7861-b5b6-fba1-d5a94cfac12e") },
                    { new Guid("1cd9c5e7-3caa-9a3f-fd99-d4cb0744c27a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("fce70fbd-53e2-4745-1d8f-eee73349a94a"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("1f413c0b-b8bc-031b-df95-b61e7d930a1b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("fce70fbd-53e2-4745-1d8f-eee73349a94a"), new Guid("92db8232-7861-b5b6-fba1-d5a94cfac12e") },
                    { new Guid("3ce58e30-2875-e9f0-6784-ff0995c5df15"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("427b013b-56ae-e153-9814-fb486b74a15c"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("4503d254-3ac1-cbe7-3ae0-541fa45aa8ef"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("84ca70ff-c4bd-6167-b445-0cdfa90da72c"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("4e8f3b96-fdb3-8360-5ba4-75fe9e983b24"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("3e9cc39c-94f4-dd9a-414a-4e485befb873"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("56661386-7193-c64a-ff31-ac87346a399d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("bb10e3b6-1b66-ccb1-245c-795eb2252d71"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("5a8aea2e-5425-0a39-fdf6-bfdc8d2894a0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("c7c5fdd0-07fe-393e-13ed-d31f6debfe27"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("689a7818-0a16-5f75-eb1f-7a7c01f44580"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d7911f7a-ae45-45a1-19f2-bb822a9ff5fe"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("68ed8209-b680-4be5-d613-103add92eae1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("99baaeef-560a-3a17-3780-50d9d264eff5"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("6c246492-e3f3-849a-576d-9d9ead85dbbc"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("bf67f175-3503-4d60-db6e-01982084a6cc"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("6c268eae-107b-6e57-d22c-2afaf120caa9"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d7911f7a-ae45-45a1-19f2-bb822a9ff5fe"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("6cc26e68-1056-bd52-4fd1-dbb8f556a6bd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("b2742550-1168-9871-636f-96f6e8317829"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("76ed5d6f-4def-bec2-78cb-0aaceec9e84d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f9c2e546-0054-6969-0e68-36d4a1d7c04c"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("780194f9-643b-53b9-7688-45bd4b32d653"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f9c2e546-0054-6969-0e68-36d4a1d7c04c"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("9394567e-9b66-034d-644c-17c2f890d979"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("84ca70ff-c4bd-6167-b445-0cdfa90da72c"), new Guid("92db8232-7861-b5b6-fba1-d5a94cfac12e") },
                    { new Guid("97855817-febd-0e52-8a7d-ab2f6d1332aa"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("74c4beb3-578d-15fe-8017-c7a1bc8afb7e"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("9b0df786-015d-1a20-0d60-7b04af30040f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("fce70fbd-53e2-4745-1d8f-eee73349a94a"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("a6392c6e-e20f-96f8-5e7b-ad1d0e37d2e1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("74c4beb3-578d-15fe-8017-c7a1bc8afb7e"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("a9c7b203-fd66-ee08-e3bb-d9bdcacede98"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4564acf5-7bd3-d480-4c24-90ca8d4ea5cb"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("ac04d45f-f1a7-b50a-7faa-50b6e1c2d19b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("99baaeef-560a-3a17-3780-50d9d264eff5"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("b46f39f0-924d-f1ff-1ec9-ea6fe8f504fb"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4564acf5-7bd3-d480-4c24-90ca8d4ea5cb"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("b6a414b2-535c-3b99-40ca-e1fe43d89a7d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("84ca70ff-c4bd-6167-b445-0cdfa90da72c"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("c3060e65-e346-a5c5-e4c0-e3607a67927b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("b2742550-1168-9871-636f-96f6e8317829"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("e1e72e20-9f65-8950-b99e-054616638f8f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("b2742550-1168-9871-636f-96f6e8317829"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("e310c715-3f51-4b83-353e-356454739427"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("b2742550-1168-9871-636f-96f6e8317829"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("e678f2bc-de5e-abe9-4709-fb88a8b1e410"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("427b013b-56ae-e153-9814-fb486b74a15c"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("ec8ad4aa-f4a0-ed8b-7ab1-5c284ed799be"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("bf67f175-3503-4d60-db6e-01982084a6cc"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("ef26b58f-35aa-8987-7788-ff8538376b43"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("c7c5fdd0-07fe-393e-13ed-d31f6debfe27"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("f5de0941-6059-31eb-a753-5e30283bf3f7"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("3e9cc39c-94f4-dd9a-414a-4e485befb873"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionAgendaItems_RelatedSupplierId",
                schema: "commission",
                table: "TenderCommissionAgendaItems",
                column: "RelatedSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionAgendaItems_RelatedTenderBidId",
                schema: "commission",
                table: "TenderCommissionAgendaItems",
                column: "RelatedTenderBidId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionAgendaItems_SessionId",
                schema: "commission",
                table: "TenderCommissionAgendaItems",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionAttachments_FileDocumentId",
                schema: "commission",
                table: "TenderCommissionAttachments",
                column: "FileDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionAttachments_SessionId",
                schema: "commission",
                table: "TenderCommissionAttachments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionDecisions_SelectedSupplierId",
                schema: "commission",
                table: "TenderCommissionDecisions",
                column: "SelectedSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionDecisions_SelectedTenderBidId",
                schema: "commission",
                table: "TenderCommissionDecisions",
                column: "SelectedTenderBidId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionDecisions_SessionId",
                schema: "commission",
                table: "TenderCommissionDecisions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionDecisions_TenderId",
                schema: "commission",
                table: "TenderCommissionDecisions",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionMembers_SessionId",
                schema: "commission",
                table: "TenderCommissionMembers",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionMembers_UserId",
                schema: "commission",
                table: "TenderCommissionMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionMinutes_AgendaItemId",
                schema: "commission",
                table: "TenderCommissionMinutes",
                column: "AgendaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionMinutes_SessionId",
                schema: "commission",
                table: "TenderCommissionMinutes",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionSessions_PurchaseFileId",
                schema: "commission",
                table: "TenderCommissionSessions",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionSessions_SessionDate",
                schema: "commission",
                table: "TenderCommissionSessions",
                column: "SessionDate");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionSessions_SessionNumber",
                schema: "commission",
                table: "TenderCommissionSessions",
                column: "SessionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionSessions_Status",
                schema: "commission",
                table: "TenderCommissionSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionSessions_TenderId",
                schema: "commission",
                table: "TenderCommissionSessions",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderCommissionSessionSequences_Year",
                schema: "commission",
                table: "TenderCommissionSessionSequences",
                column: "Year",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenderCommissionAttachments",
                schema: "commission");

            migrationBuilder.DropTable(
                name: "TenderCommissionDecisions",
                schema: "commission");

            migrationBuilder.DropTable(
                name: "TenderCommissionMembers",
                schema: "commission");

            migrationBuilder.DropTable(
                name: "TenderCommissionMinutes",
                schema: "commission");

            migrationBuilder.DropTable(
                name: "TenderCommissionSessionSequences",
                schema: "commission");

            migrationBuilder.DropTable(
                name: "TenderCommissionAgendaItems",
                schema: "commission");

            migrationBuilder.DropTable(
                name: "TenderCommissionSessions",
                schema: "commission");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("13112e39-4089-a01a-fdf8-593dd2401d7e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1c00a932-4a86-bd8f-a65b-2376dc50311f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1cd9c5e7-3caa-9a3f-fd99-d4cb0744c27a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1f413c0b-b8bc-031b-df95-b61e7d930a1b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("3ce58e30-2875-e9f0-6784-ff0995c5df15"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("4503d254-3ac1-cbe7-3ae0-541fa45aa8ef"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("4e8f3b96-fdb3-8360-5ba4-75fe9e983b24"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("56661386-7193-c64a-ff31-ac87346a399d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("5a8aea2e-5425-0a39-fdf6-bfdc8d2894a0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("689a7818-0a16-5f75-eb1f-7a7c01f44580"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("68ed8209-b680-4be5-d613-103add92eae1"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6c246492-e3f3-849a-576d-9d9ead85dbbc"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6c268eae-107b-6e57-d22c-2afaf120caa9"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6cc26e68-1056-bd52-4fd1-dbb8f556a6bd"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("76ed5d6f-4def-bec2-78cb-0aaceec9e84d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("780194f9-643b-53b9-7688-45bd4b32d653"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("9394567e-9b66-034d-644c-17c2f890d979"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("97855817-febd-0e52-8a7d-ab2f6d1332aa"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("9b0df786-015d-1a20-0d60-7b04af30040f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a6392c6e-e20f-96f8-5e7b-ad1d0e37d2e1"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a9c7b203-fd66-ee08-e3bb-d9bdcacede98"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ac04d45f-f1a7-b50a-7faa-50b6e1c2d19b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b46f39f0-924d-f1ff-1ec9-ea6fe8f504fb"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b6a414b2-535c-3b99-40ca-e1fe43d89a7d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c3060e65-e346-a5c5-e4c0-e3607a67927b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e1e72e20-9f65-8950-b99e-054616638f8f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e310c715-3f51-4b83-353e-356454739427"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e678f2bc-de5e-abe9-4709-fb88a8b1e410"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ec8ad4aa-f4a0-ed8b-7ab1-5c284ed799be"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ef26b58f-35aa-8987-7788-ff8538376b43"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("f5de0941-6059-31eb-a753-5e30283bf3f7"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3e9cc39c-94f4-dd9a-414a-4e485befb873"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("427b013b-56ae-e153-9814-fb486b74a15c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4564acf5-7bd3-d480-4c24-90ca8d4ea5cb"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("74c4beb3-578d-15fe-8017-c7a1bc8afb7e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("84ca70ff-c4bd-6167-b445-0cdfa90da72c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("99baaeef-560a-3a17-3780-50d9d264eff5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b2742550-1168-9871-636f-96f6e8317829"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("bb10e3b6-1b66-ccb1-245c-795eb2252d71"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("bf67f175-3503-4d60-db6e-01982084a6cc"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c7c5fdd0-07fe-393e-13ed-d31f6debfe27"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d7911f7a-ae45-45a1-19f2-bb822a9ff5fe"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f9c2e546-0054-6969-0e68-36d4a1d7c04c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fce70fbd-53e2-4745-1d8f-eee73349a94a"));
        }
    }
}

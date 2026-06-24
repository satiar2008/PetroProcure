using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InquiryManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inquiry");

            migrationBuilder.CreateTable(
                name: "Inquiries",
                schema: "inquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InquiryNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InquiryType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeadlineDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquiries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InquirySequences",
                schema: "inquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastSequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InquirySequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InquiryDocuments",
                schema: "inquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InquiryDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InquiryDocuments_Inquiries_InquiryId",
                        column: x => x.InquiryId,
                        principalSchema: "inquiry",
                        principalTable: "Inquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InquiryItems",
                schema: "inquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MescItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MescGeneralGroupCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GeneralDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SpecificDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TechnicalDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InquiryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InquiryItems_Inquiries_InquiryId",
                        column: x => x.InquiryId,
                        principalSchema: "inquiry",
                        principalTable: "Inquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InquirySuppliers",
                schema: "inquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvitedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeclinedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeclineReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InquirySuppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InquirySuppliers_Inquiries_InquiryId",
                        column: x => x.InquiryId,
                        principalSchema: "inquiry",
                        principalTable: "Inquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierQuotes",
                schema: "inquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InquirySupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuoteNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    QuoteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TechnicalNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CommercialNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    SelectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierQuotes_Inquiries_InquiryId",
                        column: x => x.InquiryId,
                        principalSchema: "inquiry",
                        principalTable: "Inquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierQuoteItems",
                schema: "inquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierQuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InquiryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MescGeneralGroupCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GeneralDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SpecificDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TechnicalComplianceStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TechnicalNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierQuoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierQuoteItems_SupplierQuotes_SupplierQuoteId",
                        column: x => x.SupplierQuoteId,
                        principalSchema: "inquiry",
                        principalTable: "SupplierQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("19921d5e-22a4-fd2a-7834-8907c323da0e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inquiry.Send", true, null, null, "Inquiry.Send" },
                    { new Guid("5300216b-3965-73f2-2886-8a5e3e3a1f00"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inquiry.View", true, null, null, "Inquiry.View" },
                    { new Guid("5e39445e-46dc-ab1a-1a97-2b01d7867f8e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inquiry.ManageSuppliers", true, null, null, "Inquiry.ManageSuppliers" },
                    { new Guid("6a0379a2-35c3-ec0e-ff3b-48dbe2e1117f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inquiry.ReceiveQuote", true, null, null, "Inquiry.ReceiveQuote" },
                    { new Guid("7e45d61a-f8d5-5802-56a7-a7ff5bdb7861"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inquiry.Create", true, null, null, "Inquiry.Create" },
                    { new Guid("a03366f1-73c1-abac-7eeb-46130e633729"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inquiry.ManageDocuments", true, null, null, "Inquiry.ManageDocuments" },
                    { new Guid("aa1a554a-0e4d-d165-ed6d-3998289d8367"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inquiry.CompareQuotes", true, null, null, "Inquiry.CompareQuotes" },
                    { new Guid("af01a418-589e-c131-24a0-4bb2d18ea9c4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inquiry.SelectSupplier", true, null, null, "Inquiry.SelectSupplier" },
                    { new Guid("c42ddef0-0b64-0cda-2198-1ffd90fdf6ef"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inquiry.Edit", true, null, null, "Inquiry.Edit" },
                    { new Guid("f3641ec1-a51a-08ce-a353-a311845b2876"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inquiry.Cancel", true, null, null, "Inquiry.Cancel" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("00b2e569-d335-48a9-1063-55a8d3fc5595"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("aa1a554a-0e4d-d165-ed6d-3998289d8367"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("0a780626-3f4e-115c-072c-a3739a694be9"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f3641ec1-a51a-08ce-a353-a311845b2876"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("1393b5d2-8791-992b-81e2-a8e75c0dc852"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("c42ddef0-0b64-0cda-2198-1ffd90fdf6ef"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("17b03583-75e9-6a63-2cd3-727f7c7393fc"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("aa1a554a-0e4d-d165-ed6d-3998289d8367"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("1e7fdf58-99a9-f1ea-ae09-e3b3145b989f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("c42ddef0-0b64-0cda-2198-1ffd90fdf6ef"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("1e813f54-d78f-fef4-b351-721b9b2a2a5b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6a0379a2-35c3-ec0e-ff3b-48dbe2e1117f"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("22babedb-0a30-6918-5275-81c97285a86e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("af01a418-589e-c131-24a0-4bb2d18ea9c4"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("22f4773a-4a7b-8e60-73d7-1d5d062355cf"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5300216b-3965-73f2-2886-8a5e3e3a1f00"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("2353249b-6aa1-cc4e-2b97-23502efcd887"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5300216b-3965-73f2-2886-8a5e3e3a1f00"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("30d01e30-ebaa-5cac-913a-33a6d29546cb"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("19921d5e-22a4-fd2a-7834-8907c323da0e"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("35a8ac78-c444-120f-a77c-a559b13ada4f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5e39445e-46dc-ab1a-1a97-2b01d7867f8e"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("404d2b5d-1c39-a3f9-64da-cfecc0913847"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a03366f1-73c1-abac-7eeb-46130e633729"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("46f5a5c9-6e74-d902-3633-95b9e05d3438"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5e39445e-46dc-ab1a-1a97-2b01d7867f8e"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("7b59389f-e6cb-f7ad-572e-de9367421597"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("c42ddef0-0b64-0cda-2198-1ffd90fdf6ef"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("82866b6d-9c20-7196-76be-584e07d8259e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f3641ec1-a51a-08ce-a353-a311845b2876"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("85056424-23b3-e52e-2abc-11e0c31629be"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("7e45d61a-f8d5-5802-56a7-a7ff5bdb7861"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("8e73aaca-fdc1-3a99-a9a8-8b64388cf785"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("7e45d61a-f8d5-5802-56a7-a7ff5bdb7861"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("9a8fcd6e-278f-4b50-a649-29fe15b795de"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5300216b-3965-73f2-2886-8a5e3e3a1f00"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("9f968f67-bba4-46e7-f8d6-447ab15afa55"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("aa1a554a-0e4d-d165-ed6d-3998289d8367"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("a724447f-b611-c666-b8a6-c57d0a740376"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("af01a418-589e-c131-24a0-4bb2d18ea9c4"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("a86c74f6-d844-0ade-157f-5746ea312ca4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a03366f1-73c1-abac-7eeb-46130e633729"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("b07d16a9-f2cc-7f76-c567-34756ae3e7ad"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("19921d5e-22a4-fd2a-7834-8907c323da0e"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("b24f3370-97ea-6901-025d-12a90577a5fd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5300216b-3965-73f2-2886-8a5e3e3a1f00"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("c15d61c6-88cf-28df-7f0e-2a04a8aff0e8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5e39445e-46dc-ab1a-1a97-2b01d7867f8e"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("c6aa6dbe-f4ce-dd8c-6f3f-fc46f311afb1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6a0379a2-35c3-ec0e-ff3b-48dbe2e1117f"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("c7a47e88-67e8-f82c-6ef7-1ce3690864bd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("7e45d61a-f8d5-5802-56a7-a7ff5bdb7861"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("d4112834-582b-af49-6048-7af07716e275"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("aa1a554a-0e4d-d165-ed6d-3998289d8367"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("da1b83ae-c786-2e52-6db3-991e4b4aaeab"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("19921d5e-22a4-fd2a-7834-8907c323da0e"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("fb8d1e92-a14e-9645-a155-da26fd6c9f0e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6a0379a2-35c3-ec0e-ff3b-48dbe2e1117f"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_CreatedAt",
                schema: "inquiry",
                table: "Inquiries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_DeadlineDate",
                schema: "inquiry",
                table: "Inquiries",
                column: "DeadlineDate");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_InquiryNumber",
                schema: "inquiry",
                table: "Inquiries",
                column: "InquiryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_PurchaseFileId",
                schema: "inquiry",
                table: "Inquiries",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_Status",
                schema: "inquiry",
                table: "Inquiries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InquiryDocuments_InquiryId",
                schema: "inquiry",
                table: "InquiryDocuments",
                column: "InquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_InquiryItems_InquiryId",
                schema: "inquiry",
                table: "InquiryItems",
                column: "InquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_InquirySequences_Year",
                schema: "inquiry",
                table: "InquirySequences",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InquirySuppliers_InquiryId",
                schema: "inquiry",
                table: "InquirySuppliers",
                column: "InquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_InquirySuppliers_SupplierId",
                schema: "inquiry",
                table: "InquirySuppliers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierQuoteItems_SupplierQuoteId",
                schema: "inquiry",
                table: "SupplierQuoteItems",
                column: "SupplierQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierQuotes_InquiryId_SupplierId",
                schema: "inquiry",
                table: "SupplierQuotes",
                columns: new[] { "InquiryId", "SupplierId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InquiryDocuments",
                schema: "inquiry");

            migrationBuilder.DropTable(
                name: "InquiryItems",
                schema: "inquiry");

            migrationBuilder.DropTable(
                name: "InquirySequences",
                schema: "inquiry");

            migrationBuilder.DropTable(
                name: "InquirySuppliers",
                schema: "inquiry");

            migrationBuilder.DropTable(
                name: "SupplierQuoteItems",
                schema: "inquiry");

            migrationBuilder.DropTable(
                name: "SupplierQuotes",
                schema: "inquiry");

            migrationBuilder.DropTable(
                name: "Inquiries",
                schema: "inquiry");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("00b2e569-d335-48a9-1063-55a8d3fc5595"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0a780626-3f4e-115c-072c-a3739a694be9"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1393b5d2-8791-992b-81e2-a8e75c0dc852"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("17b03583-75e9-6a63-2cd3-727f7c7393fc"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1e7fdf58-99a9-f1ea-ae09-e3b3145b989f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1e813f54-d78f-fef4-b351-721b9b2a2a5b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("22babedb-0a30-6918-5275-81c97285a86e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("22f4773a-4a7b-8e60-73d7-1d5d062355cf"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("2353249b-6aa1-cc4e-2b97-23502efcd887"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("30d01e30-ebaa-5cac-913a-33a6d29546cb"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("35a8ac78-c444-120f-a77c-a559b13ada4f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("404d2b5d-1c39-a3f9-64da-cfecc0913847"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("46f5a5c9-6e74-d902-3633-95b9e05d3438"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7b59389f-e6cb-f7ad-572e-de9367421597"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("82866b6d-9c20-7196-76be-584e07d8259e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("85056424-23b3-e52e-2abc-11e0c31629be"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8e73aaca-fdc1-3a99-a9a8-8b64388cf785"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("9a8fcd6e-278f-4b50-a649-29fe15b795de"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("9f968f67-bba4-46e7-f8d6-447ab15afa55"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a724447f-b611-c666-b8a6-c57d0a740376"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a86c74f6-d844-0ade-157f-5746ea312ca4"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b07d16a9-f2cc-7f76-c567-34756ae3e7ad"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b24f3370-97ea-6901-025d-12a90577a5fd"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c15d61c6-88cf-28df-7f0e-2a04a8aff0e8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c6aa6dbe-f4ce-dd8c-6f3f-fc46f311afb1"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c7a47e88-67e8-f82c-6ef7-1ce3690864bd"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d4112834-582b-af49-6048-7af07716e275"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("da1b83ae-c786-2e52-6db3-991e4b4aaeab"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("fb8d1e92-a14e-9645-a155-da26fd6c9f0e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("19921d5e-22a4-fd2a-7834-8907c323da0e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5300216b-3965-73f2-2886-8a5e3e3a1f00"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5e39445e-46dc-ab1a-1a97-2b01d7867f8e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6a0379a2-35c3-ec0e-ff3b-48dbe2e1117f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7e45d61a-f8d5-5802-56a7-a7ff5bdb7861"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a03366f1-73c1-abac-7eeb-46130e633729"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("aa1a554a-0e4d-d165-ed6d-3998289d8367"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("af01a418-589e-c131-24a0-4bb2d18ea9c4"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c42ddef0-0b64-0cda-2198-1ffd90fdf6ef"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f3641ec1-a51a-08ce-a353-a311845b2876"));
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ContractManagementFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "contract");

            migrationBuilder.CreateTable(
                name: "ContractSequences",
                schema: "contract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractTemplates",
                schema: "contract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ContractType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractTemplateClauses",
                schema: "contract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    ClauseType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTemplateClauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractTemplateClauses_ContractTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "contract",
                        principalTable: "ContractTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseContracts",
                schema: "contract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenderBidId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CommissionDecisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContractType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WarrantyTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PenaltyTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SignedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseContracts_ContractTemplates_ContractTemplateId",
                        column: x => x.ContractTemplateId,
                        principalSchema: "contract",
                        principalTable: "ContractTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseContracts_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseContracts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseContracts_TenderBids_TenderBidId",
                        column: x => x.TenderBidId,
                        principalSchema: "tender",
                        principalTable: "TenderBids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseContracts_TenderCommissionDecisions_CommissionDecisionId",
                        column: x => x.CommissionDecisionId,
                        principalSchema: "commission",
                        principalTable: "TenderCommissionDecisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseContracts_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseContracts_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractApprovals",
                schema: "contract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalStep = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApproverUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractApprovals_PurchaseContracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "contract",
                        principalTable: "PurchaseContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractClauses",
                schema: "contract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    ClauseType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractClauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractClauses_PurchaseContracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "contract",
                        principalTable: "PurchaseContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractDocuments",
                schema: "contract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractDocuments_FileDocuments_FileDocumentId",
                        column: x => x.FileDocumentId,
                        principalSchema: "doc",
                        principalTable: "FileDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContractDocuments_PurchaseContracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "contract",
                        principalTable: "PurchaseContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseContractItems",
                schema: "contract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenderBidItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MescItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MescGeneralGroupCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    GeneralDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SpecificDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TechnicalDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseContractItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseContractItems_PurchaseContracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "contract",
                        principalTable: "PurchaseContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseContractItems_PurchaseFileItems_PurchaseFileItemId",
                        column: x => x.PurchaseFileItemId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFileItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseContractItems_TenderBidItems_TenderBidItemId",
                        column: x => x.TenderBidItemId,
                        principalSchema: "tender",
                        principalTable: "TenderBidItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "contract",
                table: "ContractTemplates",
                columns: new[] { "Id", "ContractType", "CreatedAt", "CreatedByUserId", "Description", "IsActive", "TemplateCode", "Title" },
                values: new object[,]
                {
                    { new Guid("d1000000-0000-0000-0000-000000000001"), "DirectPurchase", new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), "قالب عمومی برای قراردادهای خرید مستقیم", true, "DIRECT-BASE", "قالب پایه قرارداد خرید مستقیم" },
                    { new Guid("d1000000-0000-0000-0000-000000000002"), "TenderBased", new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), "قالب عمومی برای قراردادهای مبتنی بر مناقصه", true, "TENDER-BASE", "قالب پایه قرارداد مناقصه" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("1e3a011d-f7fd-af15-a73f-9e5fbf609122"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.View", true, null, null, "Contract.View" },
                    { new Guid("45b02f53-b89b-5436-ddef-0d4a0182655d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.ManageTemplates", true, null, null, "Contract.ManageTemplates" },
                    { new Guid("63003674-e43a-6f76-033c-77a6a2894593"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.Submit", true, null, null, "Contract.Submit" },
                    { new Guid("7b0491f8-e4ea-4923-47a6-e2aa0281b925"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.Sign", true, null, null, "Contract.Sign" },
                    { new Guid("7b69a705-fb7e-8e28-2961-4e611837c6d2"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.ManageClauses", true, null, null, "Contract.ManageClauses" },
                    { new Guid("a6bb6d3d-f540-f762-2ea1-15e49a490fb0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.Approve", true, null, null, "Contract.Approve" },
                    { new Guid("ba24f924-10f4-bc09-698d-eda4d3417651"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.Reject", true, null, null, "Contract.Reject" },
                    { new Guid("c8fdf696-3ff1-5906-2567-0a132d583199"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.Edit", true, null, null, "Contract.Edit" },
                    { new Guid("ca9fab48-43ad-9c10-483f-c3c26d14578a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.Create", true, null, null, "Contract.Create" },
                    { new Guid("e96e6a01-0707-8acb-abb0-d0883271bdc3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.ReportExport", true, null, null, "Contract.ReportExport" },
                    { new Guid("ecd57d26-cf5f-c44d-7fb2-543bfe31444e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.ReportView", true, null, null, "Contract.ReportView" },
                    { new Guid("f13f1b70-3dbb-8a64-d971-9a00f0317ac3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.Cancel", true, null, null, "Contract.Cancel" },
                    { new Guid("f734cf14-f540-15a6-463b-2f417367a62b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contract.ManageDocuments", true, null, null, "Contract.ManageDocuments" }
                });

            migrationBuilder.InsertData(
                schema: "contract",
                table: "ContractTemplateClauses",
                columns: new[] { "Id", "Body", "ClauseType", "IsEditable", "IsRequired", "OrderNo", "TemplateId", "Title" },
                values: new object[,]
                {
                    { new Guid("d2000000-0000-0000-0000-000000000001"), "موضوع قرارداد عبارت است از تأمین کالا/خدمات مندرج در پیوست فنی.", "General", true, true, 1, new Guid("d1000000-0000-0000-0000-000000000001"), "موضوع قرارداد" },
                    { new Guid("d2000000-0000-0000-0000-000000000002"), "پرداخت طبق تأیید واحد خرید و پس از تحویل/ارائه مدارک معتبر انجام می‌شود.", "Payment", true, true, 2, new Guid("d1000000-0000-0000-0000-000000000001"), "شرایط پرداخت" },
                    { new Guid("d2000000-0000-0000-0000-000000000003"), "تأمین‌کننده موظف است اقلام را طبق زمان‌بندی توافق‌شده تحویل دهد.", "Delivery", true, true, 3, new Guid("d1000000-0000-0000-0000-000000000001"), "تحویل" },
                    { new Guid("d2000000-0000-0000-0000-000000000004"), "این قرارداد براساس مناقصه و تصمیم/مصوبه کمیسیون مربوطه تنظیم شده است.", "Legal", true, true, 1, new Guid("d1000000-0000-0000-0000-000000000002"), "مبنای قرارداد" },
                    { new Guid("d2000000-0000-0000-0000-000000000005"), "مشخصات فنی و مقادیر مطابق اسناد مناقصه و پیشنهاد منتخب ملاک عمل است.", "Technical", true, true, 2, new Guid("d1000000-0000-0000-0000-000000000002"), "مشخصات فنی" },
                    { new Guid("d2000000-0000-0000-0000-000000000006"), "ضمانت‌ها و جرائم تأخیر مطابق شرایط اختصاصی قرارداد اعمال می‌شود.", "Penalty", true, true, 3, new Guid("d1000000-0000-0000-0000-000000000002"), "ضمانت و جریمه" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("0978a3fb-53af-6416-eef1-be3ecd512125"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("e96e6a01-0707-8acb-abb0-d0883271bdc3"), new Guid("84dfd3fc-e85b-d990-896c-f99f7933d3e4") },
                    { new Guid("0c48584c-b71f-5d50-c8d6-6d1dde1a9e7a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("1e3a011d-f7fd-af15-a73f-9e5fbf609122"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("1992a3b0-740c-06ed-12f3-b04bf18b9e2a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f734cf14-f540-15a6-463b-2f417367a62b"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("1fbba4a6-5ed3-7d33-3a42-ad474051a92c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("1e3a011d-f7fd-af15-a73f-9e5fbf609122"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("211d61e8-8c3a-73f5-3493-e4d7691243a0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ba24f924-10f4-bc09-698d-eda4d3417651"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("27e1db4c-86d5-897d-7901-e22fb3305603"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("e96e6a01-0707-8acb-abb0-d0883271bdc3"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("2b298b0f-4361-1d2b-e67f-ad6c1dbaedda"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("63003674-e43a-6f76-033c-77a6a2894593"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("3b4c5fb6-3cfe-ba31-2078-f1555b41940c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("e96e6a01-0707-8acb-abb0-d0883271bdc3"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("3e93df33-5e32-6dbb-abf6-782e90ce92bd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f734cf14-f540-15a6-463b-2f417367a62b"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("43ee4cec-941f-0453-1a92-ea30d1d3f87b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ecd57d26-cf5f-c44d-7fb2-543bfe31444e"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("61c6655b-4179-7351-547d-caecbd088e3b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ca9fab48-43ad-9c10-483f-c3c26d14578a"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("67ac100a-8486-f81d-edfe-b60c590fa197"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("c8fdf696-3ff1-5906-2567-0a132d583199"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("697c0870-a603-9edf-524f-ac5de69ca84c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f13f1b70-3dbb-8a64-d971-9a00f0317ac3"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("6ad237b4-9f3d-886e-167d-dfec3f750ffa"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("c8fdf696-3ff1-5906-2567-0a132d583199"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("74149676-233a-ff9c-fa54-16821a636c48"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("7b69a705-fb7e-8e28-2961-4e611837c6d2"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("7b5faf46-c19f-5fea-dead-20b3dd275407"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ecd57d26-cf5f-c44d-7fb2-543bfe31444e"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("7d0ecac4-cb62-4e4f-d445-5b84a2f612a2"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("7b69a705-fb7e-8e28-2961-4e611837c6d2"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("8d2a5cc1-665e-ea44-9351-87c5f661b6c5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("7b69a705-fb7e-8e28-2961-4e611837c6d2"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("a479be48-84cd-62e5-daf0-0f01c4c84b16"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("63003674-e43a-6f76-033c-77a6a2894593"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("a5daa007-1762-03c0-7b92-d4a509689fc7"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ca9fab48-43ad-9c10-483f-c3c26d14578a"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("a8be35f2-a5a2-a674-c468-2635cdb5c608"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("e96e6a01-0707-8acb-abb0-d0883271bdc3"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("b4626787-ae00-1587-fcd9-971d9731c577"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("7b0491f8-e4ea-4923-47a6-e2aa0281b925"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("b62e45d9-e538-9e47-d7c5-4ad28f52116a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f734cf14-f540-15a6-463b-2f417367a62b"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("b7a90e95-82e1-be64-cc06-dfeed14e7116"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("c8fdf696-3ff1-5906-2567-0a132d583199"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("ba9b5bde-b010-a263-397f-0738a90a3e5c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("7b0491f8-e4ea-4923-47a6-e2aa0281b925"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("bcac29a9-cca1-9f70-1aa4-25bd607b0042"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("1e3a011d-f7fd-af15-a73f-9e5fbf609122"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("c5c75b5b-9c6c-9149-044e-26e38d6d23cb"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ca9fab48-43ad-9c10-483f-c3c26d14578a"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("ce5574b9-3641-a1d1-fa93-0f4a831d9088"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("45b02f53-b89b-5436-ddef-0d4a0182655d"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("d4ea7de4-6eb0-2718-1733-c7f551c00c95"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ba24f924-10f4-bc09-698d-eda4d3417651"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("d9b67d39-2a72-96c4-d462-dd103ac835ce"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f13f1b70-3dbb-8a64-d971-9a00f0317ac3"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("dbcaac4b-defb-e690-17da-3d76ec0b96cd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("63003674-e43a-6f76-033c-77a6a2894593"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("e901438f-c7ed-9f00-714b-6294425024fd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("1e3a011d-f7fd-af15-a73f-9e5fbf609122"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("ed46a330-8fd8-81b6-4c43-2c116b427ac6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a6bb6d3d-f540-f762-2ea1-15e49a490fb0"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("f20b6953-d44f-ec8e-3e4e-5b46942d8379"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ecd57d26-cf5f-c44d-7fb2-543bfe31444e"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("f99fe08d-56c1-4190-c48f-8bc727c7a262"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a6bb6d3d-f540-f762-2ea1-15e49a490fb0"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("fc809bbc-d937-854e-4490-ebfdae51c4a2"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ecd57d26-cf5f-c44d-7fb2-543bfe31444e"), new Guid("84dfd3fc-e85b-d990-896c-f99f7933d3e4") },
                    { new Guid("feef17d9-8c31-1a5d-1885-b9fcff5400eb"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("45b02f53-b89b-5436-ddef-0d4a0182655d"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractApprovals_ContractId_Status",
                schema: "contract",
                table: "ContractApprovals",
                columns: new[] { "ContractId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractClauses_ContractId_OrderNo",
                schema: "contract",
                table: "ContractClauses",
                columns: new[] { "ContractId", "OrderNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocuments_ContractId",
                schema: "contract",
                table: "ContractDocuments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocuments_FileDocumentId",
                schema: "contract",
                table: "ContractDocuments",
                column: "FileDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSequences_Year",
                schema: "contract",
                table: "ContractSequences",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplateClauses_TemplateId_OrderNo",
                schema: "contract",
                table: "ContractTemplateClauses",
                columns: new[] { "TemplateId", "OrderNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplates_TemplateCode",
                schema: "contract",
                table: "ContractTemplates",
                column: "TemplateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractItems_ContractId",
                schema: "contract",
                table: "PurchaseContractItems",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractItems_MescGeneralGroupCode",
                schema: "contract",
                table: "PurchaseContractItems",
                column: "MescGeneralGroupCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractItems_PurchaseFileItemId",
                schema: "contract",
                table: "PurchaseContractItems",
                column: "PurchaseFileItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractItems_TenderBidItemId",
                schema: "contract",
                table: "PurchaseContractItems",
                column: "TenderBidItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_CommissionDecisionId",
                schema: "contract",
                table: "PurchaseContracts",
                column: "CommissionDecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_ContractNumber",
                schema: "contract",
                table: "PurchaseContracts",
                column: "ContractNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_ContractTemplateId",
                schema: "contract",
                table: "PurchaseContracts",
                column: "ContractTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_CreatedAt",
                schema: "contract",
                table: "PurchaseContracts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_CreatedByUserId",
                schema: "contract",
                table: "PurchaseContracts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_EndDate",
                schema: "contract",
                table: "PurchaseContracts",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_PurchaseFileId",
                schema: "contract",
                table: "PurchaseContracts",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_StartDate",
                schema: "contract",
                table: "PurchaseContracts",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_Status",
                schema: "contract",
                table: "PurchaseContracts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_SupplierId",
                schema: "contract",
                table: "PurchaseContracts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_TenderBidId",
                schema: "contract",
                table: "PurchaseContracts",
                column: "TenderBidId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContracts_TenderId",
                schema: "contract",
                table: "PurchaseContracts",
                column: "TenderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractApprovals",
                schema: "contract");

            migrationBuilder.DropTable(
                name: "ContractClauses",
                schema: "contract");

            migrationBuilder.DropTable(
                name: "ContractDocuments",
                schema: "contract");

            migrationBuilder.DropTable(
                name: "ContractSequences",
                schema: "contract");

            migrationBuilder.DropTable(
                name: "ContractTemplateClauses",
                schema: "contract");

            migrationBuilder.DropTable(
                name: "PurchaseContractItems",
                schema: "contract");

            migrationBuilder.DropTable(
                name: "PurchaseContracts",
                schema: "contract");

            migrationBuilder.DropTable(
                name: "ContractTemplates",
                schema: "contract");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0978a3fb-53af-6416-eef1-be3ecd512125"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0c48584c-b71f-5d50-c8d6-6d1dde1a9e7a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1992a3b0-740c-06ed-12f3-b04bf18b9e2a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1fbba4a6-5ed3-7d33-3a42-ad474051a92c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("211d61e8-8c3a-73f5-3493-e4d7691243a0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("27e1db4c-86d5-897d-7901-e22fb3305603"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("2b298b0f-4361-1d2b-e67f-ad6c1dbaedda"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("3b4c5fb6-3cfe-ba31-2078-f1555b41940c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("3e93df33-5e32-6dbb-abf6-782e90ce92bd"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("43ee4cec-941f-0453-1a92-ea30d1d3f87b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("61c6655b-4179-7351-547d-caecbd088e3b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("67ac100a-8486-f81d-edfe-b60c590fa197"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("697c0870-a603-9edf-524f-ac5de69ca84c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6ad237b4-9f3d-886e-167d-dfec3f750ffa"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("74149676-233a-ff9c-fa54-16821a636c48"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7b5faf46-c19f-5fea-dead-20b3dd275407"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7d0ecac4-cb62-4e4f-d445-5b84a2f612a2"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8d2a5cc1-665e-ea44-9351-87c5f661b6c5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a479be48-84cd-62e5-daf0-0f01c4c84b16"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a5daa007-1762-03c0-7b92-d4a509689fc7"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a8be35f2-a5a2-a674-c468-2635cdb5c608"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b4626787-ae00-1587-fcd9-971d9731c577"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b62e45d9-e538-9e47-d7c5-4ad28f52116a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b7a90e95-82e1-be64-cc06-dfeed14e7116"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ba9b5bde-b010-a263-397f-0738a90a3e5c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("bcac29a9-cca1-9f70-1aa4-25bd607b0042"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c5c75b5b-9c6c-9149-044e-26e38d6d23cb"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ce5574b9-3641-a1d1-fa93-0f4a831d9088"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d4ea7de4-6eb0-2718-1733-c7f551c00c95"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d9b67d39-2a72-96c4-d462-dd103ac835ce"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("dbcaac4b-defb-e690-17da-3d76ec0b96cd"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e901438f-c7ed-9f00-714b-6294425024fd"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ed46a330-8fd8-81b6-4c43-2c116b427ac6"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("f20b6953-d44f-ec8e-3e4e-5b46942d8379"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("f99fe08d-56c1-4190-c48f-8bc727c7a262"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("fc809bbc-d937-854e-4490-ebfdae51c4a2"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("feef17d9-8c31-1a5d-1885-b9fcff5400eb"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1e3a011d-f7fd-af15-a73f-9e5fbf609122"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("45b02f53-b89b-5436-ddef-0d4a0182655d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("63003674-e43a-6f76-033c-77a6a2894593"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b0491f8-e4ea-4923-47a6-e2aa0281b925"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b69a705-fb7e-8e28-2961-4e611837c6d2"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a6bb6d3d-f540-f762-2ea1-15e49a490fb0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ba24f924-10f4-bc09-698d-eda4d3417651"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c8fdf696-3ff1-5906-2567-0a132d583199"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ca9fab48-43ad-9c10-483f-c3c26d14578a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e96e6a01-0707-8acb-abb0-d0883271bdc3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ecd57d26-cf5f-c44d-7fb2-543bfe31444e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f13f1b70-3dbb-8a64-d971-9a00f0317ac3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f734cf14-f540-15a6-463b-2f417367a62b"));
        }
    }
}

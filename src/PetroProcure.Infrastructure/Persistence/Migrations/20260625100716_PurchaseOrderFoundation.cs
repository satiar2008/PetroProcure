using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseOrderFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "po");

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                schema: "po",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenderBidId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PurchaseOrderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WarrantyTerms = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_PurchaseContracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "contract",
                        principalTable: "PurchaseContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_TenderBids_TenderBidId",
                        column: x => x.TenderBidId,
                        principalSchema: "tender",
                        principalTable: "TenderBids",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Tenders_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "tender",
                        principalTable: "Tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderSequences",
                schema: "po",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderApprovals",
                schema: "po",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_PurchaseOrderApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderApprovals_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "po",
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDocuments",
                schema: "po",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDocuments_FileDocuments_FileDocumentId",
                        column: x => x.FileDocumentId,
                        principalSchema: "doc",
                        principalTable: "FileDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDocuments_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "po",
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderItems",
                schema: "po",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenderBidItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MescItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MescGeneralGroupCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    GeneralDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SpecificDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RemainingQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TechnicalDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseContractItems_ContractItemId",
                        column: x => x.ContractItemId,
                        principalSchema: "contract",
                        principalTable: "PurchaseContractItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseFileItems_PurchaseFileItemId",
                        column: x => x.PurchaseFileItemId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFileItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "po",
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderItems_TenderBidItems_TenderBidItemId",
                        column: x => x.TenderBidItemId,
                        principalSchema: "tender",
                        principalTable: "TenderBidItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "org",
                table: "DepartmentMenuItems",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DepartmentType", "IsVisible", "ModifiedAtUtc", "ModifiedBy", "Order", "RequiredPermission", "Route", "Title" },
                values: new object[,]
                {
                    { new Guid("6d1111e5-0cb6-bf4d-2322-090473a81a1b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, true, null, null, 2, "PurchaseOrder.View", "/warehouse/purchase-orders", "سفارش‌های در انتظار رسید" },
                    { new Guid("f2ecc540-5ff5-b46b-27c8-1e7d4d6d1ac8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, null, null, 3, "PurchaseOrder.View", "/purchase/purchase-orders", "سفارش‌های خرید" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("02094b68-67f7-735e-d68b-47cde4b3e07f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.Reject", true, null, null, "PurchaseOrder.Reject" },
                    { new Guid("145be573-0dc4-e326-2b79-40427d85c426"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.ReportExport", true, null, null, "PurchaseOrder.ReportExport" },
                    { new Guid("2d438811-f305-1360-16f4-e4b5ac0a25e8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.Submit", true, null, null, "PurchaseOrder.Submit" },
                    { new Guid("2e3cec74-cb5c-e05f-b921-df1e96c1100f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.Create", true, null, null, "PurchaseOrder.Create" },
                    { new Guid("5e4b90b9-0470-4dac-5d7d-6af22ba22e1c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.ManageDocuments", true, null, null, "PurchaseOrder.ManageDocuments" },
                    { new Guid("67e86362-81e2-ad6b-38d0-3dad53dfe875"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.ReportView", true, null, null, "PurchaseOrder.ReportView" },
                    { new Guid("927af92d-e8af-306a-19cd-fbdb17af80ab"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.Edit", true, null, null, "PurchaseOrder.Edit" },
                    { new Guid("96813069-cf12-e82d-f2c1-4b6f874bb94a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.View", true, null, null, "PurchaseOrder.View" },
                    { new Guid("ab2035ea-5d03-379a-1c89-30d693aeb566"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.Approve", true, null, null, "PurchaseOrder.Approve" },
                    { new Guid("d41666a7-be65-3e87-b53c-174546761739"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.Issue", true, null, null, "PurchaseOrder.Issue" },
                    { new Guid("e684184d-ce42-44ce-a37e-46f4dd8a2ce1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.ManageItems", true, null, null, "PurchaseOrder.ManageItems" },
                    { new Guid("e7b74352-ab0a-fabc-a5c2-38318d3e55b1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "PurchaseOrder.Cancel", true, null, null, "PurchaseOrder.Cancel" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("2c52dfd6-ddd9-04a8-b340-93bdcb26a3f1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5e4b90b9-0470-4dac-5d7d-6af22ba22e1c"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("2ee9e612-5484-72c4-479d-521044a16a9e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2e3cec74-cb5c-e05f-b921-df1e96c1100f"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("2fb53c6e-ca75-6397-c01d-d0566e3dde2e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("e7b74352-ab0a-fabc-a5c2-38318d3e55b1"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("3222b190-b923-ece5-91d2-b291f1fecff4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ab2035ea-5d03-379a-1c89-30d693aeb566"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("39d6feaa-0da2-ce30-97bb-a040ac57b241"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2d438811-f305-1360-16f4-e4b5ac0a25e8"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("39f9eb56-df0c-8ed2-3388-77d719a02b4d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("96813069-cf12-e82d-f2c1-4b6f874bb94a"), new Guid("08e0b8c8-0ce3-698c-c760-498a3746bb1f") },
                    { new Guid("3c00b3c9-0c6c-1db3-72be-f22c278e69d4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("145be573-0dc4-e326-2b79-40427d85c426"), new Guid("84dfd3fc-e85b-d990-896c-f99f7933d3e4") },
                    { new Guid("437f3b97-f841-3f68-cc39-6816ffe20c4f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("96813069-cf12-e82d-f2c1-4b6f874bb94a"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("45218ae0-848e-a956-0b9c-170a88dd9b46"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("96813069-cf12-e82d-f2c1-4b6f874bb94a"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("48db21b0-01ff-dc95-083a-d0f6bcc65230"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("02094b68-67f7-735e-d68b-47cde4b3e07f"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("4a3ac932-a914-b2a3-2977-2707d1835e86"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ab2035ea-5d03-379a-1c89-30d693aeb566"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("4e30d62e-cf47-674f-6035-9a4735dd4c72"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("67e86362-81e2-ad6b-38d0-3dad53dfe875"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("5cce9b6a-c9a7-6f1e-6d0b-8fdd638ab7cc"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("927af92d-e8af-306a-19cd-fbdb17af80ab"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("6121b20e-48c6-07cf-4457-4c8f4d52c97d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("927af92d-e8af-306a-19cd-fbdb17af80ab"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("62900fe7-87cf-356f-6dfd-369a98e1bb64"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("e684184d-ce42-44ce-a37e-46f4dd8a2ce1"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("69d8b901-1c59-3de3-b8aa-2293b8f42309"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2e3cec74-cb5c-e05f-b921-df1e96c1100f"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("6a9f698b-4c3d-cf7d-8ce3-f5758e3fa8d7"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2e3cec74-cb5c-e05f-b921-df1e96c1100f"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("7a6a2948-97d0-f41c-8453-292181204fbe"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("67e86362-81e2-ad6b-38d0-3dad53dfe875"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("81b3e8cd-fb52-4e63-daa8-4fca725183ec"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5e4b90b9-0470-4dac-5d7d-6af22ba22e1c"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("85183553-844c-7229-3595-2efcd6a908ff"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("e684184d-ce42-44ce-a37e-46f4dd8a2ce1"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("898fdbdf-5410-ce3a-6593-c9dced35702f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("5e4b90b9-0470-4dac-5d7d-6af22ba22e1c"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("992ebce7-b26c-b5f3-283b-41e1e2662db2"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("96813069-cf12-e82d-f2c1-4b6f874bb94a"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("a31a5219-8610-24ea-e4ab-f638022682f8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d41666a7-be65-3e87-b53c-174546761739"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("a31d6209-52ba-96fb-9eba-d813974de1d1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("e7b74352-ab0a-fabc-a5c2-38318d3e55b1"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("c46e5886-8aa0-9d10-abca-75b6b61d0ca6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("e684184d-ce42-44ce-a37e-46f4dd8a2ce1"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("cc74358f-ddc1-f42d-e0dc-77a4856599f8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("145be573-0dc4-e326-2b79-40427d85c426"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("cec03410-85be-be4c-405d-afe58da41f47"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("67e86362-81e2-ad6b-38d0-3dad53dfe875"), new Guid("84dfd3fc-e85b-d990-896c-f99f7933d3e4") },
                    { new Guid("d053ce99-c606-6a03-c169-b8e0989bc1e8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("145be573-0dc4-e326-2b79-40427d85c426"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("d0620f82-d677-c701-3dc0-69546c88e40e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("927af92d-e8af-306a-19cd-fbdb17af80ab"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("d86fa46b-29f5-1639-fbda-79a27b9b27d9"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("67e86362-81e2-ad6b-38d0-3dad53dfe875"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("e0272948-baca-b885-c3f4-754f243d34aa"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2d438811-f305-1360-16f4-e4b5ac0a25e8"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("e2f10b35-cbf6-121a-8f12-ff2bf18a1e29"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2d438811-f305-1360-16f4-e4b5ac0a25e8"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("e8a36775-4d45-0720-777c-1945e3597f84"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("145be573-0dc4-e326-2b79-40427d85c426"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("e8e8a216-56cc-6d54-922d-1b5fe8bcfbbf"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("96813069-cf12-e82d-f2c1-4b6f874bb94a"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("ec290c71-2813-6ee1-af52-58d00712fe0f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("02094b68-67f7-735e-d68b-47cde4b3e07f"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("eebe0568-9ba0-3f4a-78eb-5afde4fe4841"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d41666a7-be65-3e87-b53c-174546761739"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderApprovals_PurchaseOrderId_Status",
                schema: "po",
                table: "PurchaseOrderApprovals",
                columns: new[] { "PurchaseOrderId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDocuments_FileDocumentId",
                schema: "po",
                table: "PurchaseOrderDocuments",
                column: "FileDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDocuments_PurchaseOrderId",
                schema: "po",
                table: "PurchaseOrderDocuments",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_ContractItemId",
                schema: "po",
                table: "PurchaseOrderItems",
                column: "ContractItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_MescGeneralGroupCode",
                schema: "po",
                table: "PurchaseOrderItems",
                column: "MescGeneralGroupCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseFileItemId",
                schema: "po",
                table: "PurchaseOrderItems",
                column: "PurchaseFileItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_PurchaseOrderId",
                schema: "po",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_TenderBidItemId",
                schema: "po",
                table: "PurchaseOrderItems",
                column: "TenderBidItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_ContractId",
                schema: "po",
                table: "PurchaseOrders",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CreatedAt",
                schema: "po",
                table: "PurchaseOrders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CreatedByUserId",
                schema: "po",
                table: "PurchaseOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_ExpectedDeliveryDate",
                schema: "po",
                table: "PurchaseOrders",
                column: "ExpectedDeliveryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_OrderDate",
                schema: "po",
                table: "PurchaseOrders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PurchaseFileId",
                schema: "po",
                table: "PurchaseOrders",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PurchaseOrderNumber",
                schema: "po",
                table: "PurchaseOrders",
                column: "PurchaseOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Status",
                schema: "po",
                table: "PurchaseOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierId",
                schema: "po",
                table: "PurchaseOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_TenderBidId",
                schema: "po",
                table: "PurchaseOrders",
                column: "TenderBidId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_TenderId",
                schema: "po",
                table: "PurchaseOrders",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderSequences_Year",
                schema: "po",
                table: "PurchaseOrderSequences",
                column: "Year",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrderApprovals",
                schema: "po");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDocuments",
                schema: "po");

            migrationBuilder.DropTable(
                name: "PurchaseOrderItems",
                schema: "po");

            migrationBuilder.DropTable(
                name: "PurchaseOrderSequences",
                schema: "po");

            migrationBuilder.DropTable(
                name: "PurchaseOrders",
                schema: "po");

            migrationBuilder.DeleteData(
                schema: "org",
                table: "DepartmentMenuItems",
                keyColumn: "Id",
                keyValue: new Guid("6d1111e5-0cb6-bf4d-2322-090473a81a1b"));

            migrationBuilder.DeleteData(
                schema: "org",
                table: "DepartmentMenuItems",
                keyColumn: "Id",
                keyValue: new Guid("f2ecc540-5ff5-b46b-27c8-1e7d4d6d1ac8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("2c52dfd6-ddd9-04a8-b340-93bdcb26a3f1"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("2ee9e612-5484-72c4-479d-521044a16a9e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("2fb53c6e-ca75-6397-c01d-d0566e3dde2e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("3222b190-b923-ece5-91d2-b291f1fecff4"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("39d6feaa-0da2-ce30-97bb-a040ac57b241"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("39f9eb56-df0c-8ed2-3388-77d719a02b4d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("3c00b3c9-0c6c-1db3-72be-f22c278e69d4"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("437f3b97-f841-3f68-cc39-6816ffe20c4f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("45218ae0-848e-a956-0b9c-170a88dd9b46"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("48db21b0-01ff-dc95-083a-d0f6bcc65230"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("4a3ac932-a914-b2a3-2977-2707d1835e86"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("4e30d62e-cf47-674f-6035-9a4735dd4c72"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("5cce9b6a-c9a7-6f1e-6d0b-8fdd638ab7cc"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6121b20e-48c6-07cf-4457-4c8f4d52c97d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("62900fe7-87cf-356f-6dfd-369a98e1bb64"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("69d8b901-1c59-3de3-b8aa-2293b8f42309"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6a9f698b-4c3d-cf7d-8ce3-f5758e3fa8d7"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7a6a2948-97d0-f41c-8453-292181204fbe"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("81b3e8cd-fb52-4e63-daa8-4fca725183ec"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("85183553-844c-7229-3595-2efcd6a908ff"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("898fdbdf-5410-ce3a-6593-c9dced35702f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("992ebce7-b26c-b5f3-283b-41e1e2662db2"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a31a5219-8610-24ea-e4ab-f638022682f8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a31d6209-52ba-96fb-9eba-d813974de1d1"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c46e5886-8aa0-9d10-abca-75b6b61d0ca6"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("cc74358f-ddc1-f42d-e0dc-77a4856599f8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("cec03410-85be-be4c-405d-afe58da41f47"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d053ce99-c606-6a03-c169-b8e0989bc1e8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d0620f82-d677-c701-3dc0-69546c88e40e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d86fa46b-29f5-1639-fbda-79a27b9b27d9"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e0272948-baca-b885-c3f4-754f243d34aa"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e2f10b35-cbf6-121a-8f12-ff2bf18a1e29"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e8a36775-4d45-0720-777c-1945e3597f84"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e8e8a216-56cc-6d54-922d-1b5fe8bcfbbf"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ec290c71-2813-6ee1-af52-58d00712fe0f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("eebe0568-9ba0-3f4a-78eb-5afde4fe4841"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("02094b68-67f7-735e-d68b-47cde4b3e07f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("145be573-0dc4-e326-2b79-40427d85c426"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2d438811-f305-1360-16f4-e4b5ac0a25e8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2e3cec74-cb5c-e05f-b921-df1e96c1100f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5e4b90b9-0470-4dac-5d7d-6af22ba22e1c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("67e86362-81e2-ad6b-38d0-3dad53dfe875"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("927af92d-e8af-306a-19cd-fbdb17af80ab"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("96813069-cf12-e82d-f2c1-4b6f874bb94a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ab2035ea-5d03-379a-1c89-30d693aeb566"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d41666a7-be65-3e87-b53c-174546761739"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e684184d-ce42-44ce-a37e-46f4dd8a2ce1"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e7b74352-ab0a-fabc-a5c2-38318d3e55b1"));
        }
    }
}

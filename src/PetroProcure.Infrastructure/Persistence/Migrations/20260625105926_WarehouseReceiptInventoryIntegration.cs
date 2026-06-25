using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WarehouseReceiptInventoryIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "warehouse");

            migrationBuilder.CreateTable(
                name: "InventoryTransactionSequences",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactionSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseReceiptSequences",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseReceiptSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionNumber = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    MescItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_MescItems_MescItemId",
                        column: x => x.MescItemId,
                        principalSchema: "item",
                        principalTable: "MescItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "warehouse",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseReceipts",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryNoteNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CarrierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VehicleNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReceivedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseReceipts_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseReceipts_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "po",
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseReceipts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseReceipts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "warehouse",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseReceiptDocuments",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseReceiptDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseReceiptDocuments_FileDocuments_FileDocumentId",
                        column: x => x.FileDocumentId,
                        principalSchema: "doc",
                        principalTable: "FileDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WarehouseReceiptDocuments_WarehouseReceipts_WarehouseReceiptId",
                        column: x => x.WarehouseReceiptId,
                        principalSchema: "warehouse",
                        principalTable: "WarehouseReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseReceiptItems",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MescGeneralGroupCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    GeneralDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SpecificDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PreviouslyReceivedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AcceptedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    RejectedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    RemainingQuantityAfterReceipt = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QualityStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseReceiptItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseReceiptItems_PurchaseOrderItems_PurchaseOrderItemId",
                        column: x => x.PurchaseOrderItemId,
                        principalSchema: "po",
                        principalTable: "PurchaseOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseReceiptItems_WarehouseReceipts_WarehouseReceiptId",
                        column: x => x.WarehouseReceiptId,
                        principalSchema: "warehouse",
                        principalTable: "WarehouseReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("02269edb-c2e9-4bb3-2a61-e0e61ca60eed"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "WarehouseReceipt.Approve", true, null, null, "WarehouseReceipt.Approve" },
                    { new Guid("0af103a3-842d-c621-c1b8-5679d8532ab6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "WarehouseReceipt.Create", true, null, null, "WarehouseReceipt.Create" },
                    { new Guid("2f7d2116-df4b-9236-e1a1-a89e7916c7e3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "WarehouseReceipt.Edit", true, null, null, "WarehouseReceipt.Edit" },
                    { new Guid("4dc55e7b-527c-a0d5-0d6e-bd03abffe055"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "WarehouseReceipt.ReportExport", true, null, null, "WarehouseReceipt.ReportExport" },
                    { new Guid("64392951-b89f-a94d-b83f-edc655a67b53"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "WarehouseReceipt.Submit", true, null, null, "WarehouseReceipt.Submit" },
                    { new Guid("6c9ae34d-d023-26d2-0931-a68c4b1b617d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "WarehouseReceipt.View", true, null, null, "WarehouseReceipt.View" },
                    { new Guid("743b5801-dcd2-513f-ea82-76c97c399f69"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inventory.AdjustStock", true, null, null, "Inventory.AdjustStock" },
                    { new Guid("874fd4b0-5eca-276f-037f-8a7db9d18e94"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inventory.ViewStockBalance", true, null, null, "Inventory.ViewStockBalance" },
                    { new Guid("a358b88f-a122-dedf-5038-99a845fd4882"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "WarehouseReceipt.Cancel", true, null, null, "WarehouseReceipt.Cancel" },
                    { new Guid("d6e7c773-bee7-d7ce-bdb2-2d52e68c6a88"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "WarehouseReceipt.ManageDocuments", true, null, null, "WarehouseReceipt.ManageDocuments" },
                    { new Guid("ec1f39ea-473f-faf5-2a3c-27524b56a268"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "WarehouseReceipt.ReportView", true, null, null, "WarehouseReceipt.ReportView" },
                    { new Guid("f5028680-3796-1e8b-daee-91a6ba67f567"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Warehouse.ManageWarehouses", true, null, null, "Warehouse.ManageWarehouses" },
                    { new Guid("fe0e020e-750f-9a22-ee97-beb1185b0ef6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Inventory.ViewTransactions", true, null, null, "Inventory.ViewTransactions" }
                });

            migrationBuilder.InsertData(
                schema: "warehouse",
                table: "Warehouses",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Location", "Name" },
                values: new object[] { new Guid("eeee0000-0000-0000-0000-000000000001"), "MAIN", "انبار پیش‌فرض سیستم", true, "انبار مرکزی", "انبار مرکزی" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("061a08a2-a29d-ce5a-8c34-5f275b236943"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4dc55e7b-527c-a0d5-0d6e-bd03abffe055"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("0810915d-dc21-b41e-c1e4-1dc6d05c7cd7"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a358b88f-a122-dedf-5038-99a845fd4882"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("0babe2cb-e96b-1d19-7115-fa71cbfb9889"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a358b88f-a122-dedf-5038-99a845fd4882"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("15767b51-f2ff-e555-4f1c-84d2ef808194"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6c9ae34d-d023-26d2-0931-a68c4b1b617d"), new Guid("08e0b8c8-0ce3-698c-c760-498a3746bb1f") },
                    { new Guid("1c78032a-4b19-eeb0-7499-a4254771b953"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("874fd4b0-5eca-276f-037f-8a7db9d18e94"), new Guid("08e0b8c8-0ce3-698c-c760-498a3746bb1f") },
                    { new Guid("291b6743-8856-1704-9cf6-d318077e3837"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("fe0e020e-750f-9a22-ee97-beb1185b0ef6"), new Guid("08e0b8c8-0ce3-698c-c760-498a3746bb1f") },
                    { new Guid("4dd8694b-575b-83aa-165c-06708d28c5e5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2f7d2116-df4b-9236-e1a1-a89e7916c7e3"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("55e1b6c0-047e-dfbb-e171-386bf22f0918"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("64392951-b89f-a94d-b83f-edc655a67b53"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("5aef0e19-0f9f-4f27-1ee3-8b8602490d8d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f5028680-3796-1e8b-daee-91a6ba67f567"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("66569459-c766-c0d0-56c0-a98780906853"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4dc55e7b-527c-a0d5-0d6e-bd03abffe055"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("6c8c2cb3-1420-d5aa-6265-c9b22e6aaeca"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d6e7c773-bee7-d7ce-bdb2-2d52e68c6a88"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("762ecce6-5e37-9095-14ac-bebede8631dd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ec1f39ea-473f-faf5-2a3c-27524b56a268"), new Guid("84dfd3fc-e85b-d990-896c-f99f7933d3e4") },
                    { new Guid("81a80a83-369a-cbf3-7d28-5ca3f1453305"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("02269edb-c2e9-4bb3-2a61-e0e61ca60eed"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("848af1f9-5046-f68b-9c4c-84f387ac8dd1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6c9ae34d-d023-26d2-0931-a68c4b1b617d"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("87205596-3806-0f00-d75f-e3710ed84942"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4dc55e7b-527c-a0d5-0d6e-bd03abffe055"), new Guid("84dfd3fc-e85b-d990-896c-f99f7933d3e4") },
                    { new Guid("8bfe2acb-19b8-742a-7106-3d40253a57a2"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("743b5801-dcd2-513f-ea82-76c97c399f69"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("96476200-ccec-4d54-aab7-a060d78c8415"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2f7d2116-df4b-9236-e1a1-a89e7916c7e3"), new Guid("08e0b8c8-0ce3-698c-c760-498a3746bb1f") },
                    { new Guid("968b84d9-b83c-01db-73cb-20b6056d4fe5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("0af103a3-842d-c621-c1b8-5679d8532ab6"), new Guid("08e0b8c8-0ce3-698c-c760-498a3746bb1f") },
                    { new Guid("a047e0f2-08aa-ff58-6a1e-4e95f54e3e5f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("64392951-b89f-a94d-b83f-edc655a67b53"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("a04b09df-1dc9-2fb5-7b72-e631b4e93798"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ec1f39ea-473f-faf5-2a3c-27524b56a268"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("a1591f8c-1db9-48dc-5607-63ce9d437a4a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("02269edb-c2e9-4bb3-2a61-e0e61ca60eed"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("b5c9d6d2-c419-db5c-580a-42811b2b2453"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6c9ae34d-d023-26d2-0931-a68c4b1b617d"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("b61a8a36-5e72-c2ed-4f5f-3c118157d0a6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("0af103a3-842d-c621-c1b8-5679d8532ab6"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("b71de7ab-dfbd-21f5-a672-24a73423603b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("2f7d2116-df4b-9236-e1a1-a89e7916c7e3"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("b71fb4d5-99fa-3edd-0150-bb0faf31c15c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d6e7c773-bee7-d7ce-bdb2-2d52e68c6a88"), new Guid("08e0b8c8-0ce3-698c-c760-498a3746bb1f") },
                    { new Guid("bb19c265-5113-2d60-febd-23488c60c40b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("874fd4b0-5eca-276f-037f-8a7db9d18e94"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("c23c1099-ca60-b6d4-ccdb-eded56c1cdcb"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("0af103a3-842d-c621-c1b8-5679d8532ab6"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("c65ecb93-077b-76ec-7a64-dc3cd79cb9ee"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("874fd4b0-5eca-276f-037f-8a7db9d18e94"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("d38a92d8-07f3-6d35-d0ad-9b008f59e9e9"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("f5028680-3796-1e8b-daee-91a6ba67f567"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("e4b04751-48c9-7ce0-7155-5cb5a504cdd0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ec1f39ea-473f-faf5-2a3c-27524b56a268"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("ed8ca05f-e24c-cf36-e133-00793d9f7c41"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("fe0e020e-750f-9a22-ee97-beb1185b0ef6"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("edd60152-57a9-03d2-4b55-d05cf32f67b2"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("64392951-b89f-a94d-b83f-edc655a67b53"), new Guid("08e0b8c8-0ce3-698c-c760-498a3746bb1f") },
                    { new Guid("f355c58c-cbf0-f8d9-d3f3-8110de17a313"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d6e7c773-bee7-d7ce-bdb2-2d52e68c6a88"), new Guid("82cfcfc0-5a42-fba0-38b8-a4aa21c9213c") },
                    { new Guid("f5a16e95-0e08-7428-20d6-45ff965014f8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("fe0e020e-750f-9a22-ee97-beb1185b0ef6"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_MescItemId",
                schema: "warehouse",
                table: "InventoryTransactions",
                column: "MescItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ReferenceType_ReferenceId",
                schema: "warehouse",
                table: "InventoryTransactions",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionNumber",
                schema: "warehouse",
                table: "InventoryTransactions",
                column: "TransactionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_WarehouseId",
                schema: "warehouse",
                table: "InventoryTransactions",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionSequences_Year",
                schema: "warehouse",
                table: "InventoryTransactionSequences",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceiptDocuments_FileDocumentId",
                schema: "warehouse",
                table: "WarehouseReceiptDocuments",
                column: "FileDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceiptDocuments_WarehouseReceiptId",
                schema: "warehouse",
                table: "WarehouseReceiptDocuments",
                column: "WarehouseReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceiptItems_MescItemId",
                schema: "warehouse",
                table: "WarehouseReceiptItems",
                column: "MescItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceiptItems_PurchaseOrderItemId",
                schema: "warehouse",
                table: "WarehouseReceiptItems",
                column: "PurchaseOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceiptItems_WarehouseReceiptId",
                schema: "warehouse",
                table: "WarehouseReceiptItems",
                column: "WarehouseReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceipts_PurchaseFileId",
                schema: "warehouse",
                table: "WarehouseReceipts",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceipts_PurchaseOrderId",
                schema: "warehouse",
                table: "WarehouseReceipts",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceipts_ReceiptDate",
                schema: "warehouse",
                table: "WarehouseReceipts",
                column: "ReceiptDate");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceipts_ReceiptNumber",
                schema: "warehouse",
                table: "WarehouseReceipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceipts_Status",
                schema: "warehouse",
                table: "WarehouseReceipts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceipts_SupplierId",
                schema: "warehouse",
                table: "WarehouseReceipts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceipts_WarehouseId",
                schema: "warehouse",
                table: "WarehouseReceipts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceiptSequences_Year",
                schema: "warehouse",
                table: "WarehouseReceiptSequences",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                schema: "warehouse",
                table: "Warehouses",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryTransactions",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "InventoryTransactionSequences",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "WarehouseReceiptDocuments",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "WarehouseReceiptItems",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "WarehouseReceiptSequences",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "WarehouseReceipts",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "Warehouses",
                schema: "warehouse");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("061a08a2-a29d-ce5a-8c34-5f275b236943"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0810915d-dc21-b41e-c1e4-1dc6d05c7cd7"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0babe2cb-e96b-1d19-7115-fa71cbfb9889"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("15767b51-f2ff-e555-4f1c-84d2ef808194"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("1c78032a-4b19-eeb0-7499-a4254771b953"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("291b6743-8856-1704-9cf6-d318077e3837"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("4dd8694b-575b-83aa-165c-06708d28c5e5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("55e1b6c0-047e-dfbb-e171-386bf22f0918"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("5aef0e19-0f9f-4f27-1ee3-8b8602490d8d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("66569459-c766-c0d0-56c0-a98780906853"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6c8c2cb3-1420-d5aa-6265-c9b22e6aaeca"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("762ecce6-5e37-9095-14ac-bebede8631dd"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("81a80a83-369a-cbf3-7d28-5ca3f1453305"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("848af1f9-5046-f68b-9c4c-84f387ac8dd1"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("87205596-3806-0f00-d75f-e3710ed84942"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8bfe2acb-19b8-742a-7106-3d40253a57a2"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("96476200-ccec-4d54-aab7-a060d78c8415"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("968b84d9-b83c-01db-73cb-20b6056d4fe5"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a047e0f2-08aa-ff58-6a1e-4e95f54e3e5f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a04b09df-1dc9-2fb5-7b72-e631b4e93798"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("a1591f8c-1db9-48dc-5607-63ce9d437a4a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b5c9d6d2-c419-db5c-580a-42811b2b2453"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b61a8a36-5e72-c2ed-4f5f-3c118157d0a6"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b71de7ab-dfbd-21f5-a672-24a73423603b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b71fb4d5-99fa-3edd-0150-bb0faf31c15c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("bb19c265-5113-2d60-febd-23488c60c40b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c23c1099-ca60-b6d4-ccdb-eded56c1cdcb"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c65ecb93-077b-76ec-7a64-dc3cd79cb9ee"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d38a92d8-07f3-6d35-d0ad-9b008f59e9e9"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e4b04751-48c9-7ce0-7155-5cb5a504cdd0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ed8ca05f-e24c-cf36-e133-00793d9f7c41"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("edd60152-57a9-03d2-4b55-d05cf32f67b2"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("f355c58c-cbf0-f8d9-d3f3-8110de17a313"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("f5a16e95-0e08-7428-20d6-45ff965014f8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("02269edb-c2e9-4bb3-2a61-e0e61ca60eed"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0af103a3-842d-c621-c1b8-5679d8532ab6"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2f7d2116-df4b-9236-e1a1-a89e7916c7e3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4dc55e7b-527c-a0d5-0d6e-bd03abffe055"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("64392951-b89f-a94d-b83f-edc655a67b53"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6c9ae34d-d023-26d2-0931-a68c4b1b617d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("743b5801-dcd2-513f-ea82-76c97c399f69"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("874fd4b0-5eca-276f-037f-8a7db9d18e94"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a358b88f-a122-dedf-5038-99a845fd4882"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d6e7c773-bee7-d7ce-bdb2-2d52e68c6a88"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ec1f39ea-473f-faf5-2a3c-27524b56a268"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f5028680-3796-1e8b-daee-91a6ba67f567"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fe0e020e-750f-9a22-ee97-beb1185b0ef6"));
        }
    }
}

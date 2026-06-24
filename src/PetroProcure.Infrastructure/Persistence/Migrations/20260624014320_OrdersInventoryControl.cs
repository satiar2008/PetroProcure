using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrdersInventoryControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "orders");

            migrationBuilder.CreateTable(
                name: "InventoryControlItems",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MescGeneralGroupCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    GeneralDescription = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    SpecificDescription = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinimumStockLevel = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReorderPoint = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MaximumStockLevel = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    SafetyStock = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    IsStockControlled = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryControlItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialNeeds",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NeedNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MescItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MescGeneralGroupCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    GeneralDescription = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    SpecificDescription = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    NeededByDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedIndentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialNeeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialNeedSequences",
                schema: "orders",
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
                    table.PrimaryKey("PK_MaterialNeedSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShortageAlerts",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    MescGeneralGroupCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    GeneralDescription = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    SpecificDescription = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReorderPoint = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ShortageQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedIndentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolutionNote = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortageAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockBalances",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReservedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OnOrderQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBalances", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "org",
                table: "DepartmentMenuItems",
                keyColumn: "Id",
                keyValue: new Guid("e755fc22-f997-ceeb-4688-d3015885d8dd"),
                columns: new[] { "Order", "Route" },
                values: new object[] { 5, "/orders/indents" });

            migrationBuilder.InsertData(
                schema: "org",
                table: "DepartmentMenuItems",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DepartmentType", "IsVisible", "ModifiedAtUtc", "ModifiedBy", "Order", "RequiredPermission", "Route", "Title" },
                values: new object[,]
                {
                    { new Guid("305761b9-297b-0ba6-975c-5ee9d7a643a1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, null, null, 3, "Orders.CreateMaterialNeed", "/orders/material-needs", "نیازهای کالا" },
                    { new Guid("4bab8657-b8e7-f713-037d-c455289f71fe"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, null, null, 1, "Orders.ViewDashboard", "/orders", "داشبورد سفارشات" },
                    { new Guid("4e5d4dbc-57b7-0fdc-4de9-bbdb23c9df2d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, null, null, 2, "Orders.ViewInventory", "/orders/inventory-control", "کنترل موجودی" },
                    { new Guid("f8cc80c5-eb3a-7e0b-d506-7f5fa721a491"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, true, null, null, 4, "Orders.ManageShortageAlerts", "/orders/shortage-alerts", "هشدارهای کمبود" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("4324f541-58db-3c7f-7841-5391a060c45d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Orders.ManageShortageAlerts", true, null, null, "Orders.ManageShortageAlerts" },
                    { new Guid("6dcb9583-f266-637b-2498-c317ccafc31a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Orders.ApproveMaterialNeed", true, null, null, "Orders.ApproveMaterialNeed" },
                    { new Guid("94c8d580-33ae-530b-6d52-efac0cf3fc28"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Orders.ViewDashboard", true, null, null, "Orders.ViewDashboard" },
                    { new Guid("96872bda-3d33-5c47-bbd2-e6dbbb1de909"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Orders.ConvertNeedToIndent", true, null, null, "Orders.ConvertNeedToIndent" },
                    { new Guid("96b97ff0-c80a-26d1-b9b1-41db6521d38a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Orders.ConvertShortageToIndent", true, null, null, "Orders.ConvertShortageToIndent" },
                    { new Guid("9a2f5bdb-3afc-86ce-4464-aa92d682f91f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Orders.ManageInventoryControl", true, null, null, "Orders.ManageInventoryControl" },
                    { new Guid("b00efef6-8e69-7130-5654-4d2b166d4063"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Orders.CreateMaterialNeed", true, null, null, "Orders.CreateMaterialNeed" },
                    { new Guid("caf9b346-4405-e8b5-4d30-0e5b787944ef"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Orders.ViewInventory", true, null, null, "Orders.ViewInventory" },
                    { new Guid("ddb5f59a-810c-7f17-51c5-62219ce56f6b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Orders.ReviewMaterialNeed", true, null, null, "Orders.ReviewMaterialNeed" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("02077577-3ad5-8174-9114-6da45c8d280f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("96872bda-3d33-5c47-bbd2-e6dbbb1de909"), new Guid("317af55d-dd61-bbd7-6290-1ff6508f3f8a") },
                    { new Guid("054f3bd4-3d8c-ebd1-db07-eabc1f42e714"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("b00efef6-8e69-7130-5654-4d2b166d4063"), new Guid("317af55d-dd61-bbd7-6290-1ff6508f3f8a") },
                    { new Guid("090cd4d5-aaa9-954a-4810-6f85bd874256"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4324f541-58db-3c7f-7841-5391a060c45d"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("268d76d1-3966-5d1f-21aa-5584ef4cf34b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("94c8d580-33ae-530b-6d52-efac0cf3fc28"), new Guid("304d970e-93fc-82d5-ea7e-68bd7be5d96f") },
                    { new Guid("360cae35-80dc-6551-1bc0-020b905fd905"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("b00efef6-8e69-7130-5654-4d2b166d4063"), new Guid("304d970e-93fc-82d5-ea7e-68bd7be5d96f") },
                    { new Guid("3658f2e3-7afe-1c6f-2ba4-fcb272f13153"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("96872bda-3d33-5c47-bbd2-e6dbbb1de909"), new Guid("304d970e-93fc-82d5-ea7e-68bd7be5d96f") },
                    { new Guid("3c75222f-95a9-8628-bcaf-de77622ce787"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9a2f5bdb-3afc-86ce-4464-aa92d682f91f"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("403affc4-1875-5b81-2746-6573ffa94489"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6dcb9583-f266-637b-2498-c317ccafc31a"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("47a295a7-92e6-0f0d-c20a-b6a4b94ee7d0"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("caf9b346-4405-e8b5-4d30-0e5b787944ef"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("5339ffed-7760-d858-2cd6-0923da5f3776"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("6dcb9583-f266-637b-2498-c317ccafc31a"), new Guid("317af55d-dd61-bbd7-6290-1ff6508f3f8a") },
                    { new Guid("56ef7c05-7fcf-8b6c-b703-b6d3041b1e97"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("b00efef6-8e69-7130-5654-4d2b166d4063"), new Guid("feb60493-d451-b8d4-d9b4-751e8ea5efd0") },
                    { new Guid("6837cd09-3647-1cf2-5eb4-fd008abe2206"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ddb5f59a-810c-7f17-51c5-62219ce56f6b"), new Guid("304d970e-93fc-82d5-ea7e-68bd7be5d96f") },
                    { new Guid("70119563-e2c2-d4ce-7ae7-992e5ffc5288"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("94c8d580-33ae-530b-6d52-efac0cf3fc28"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("744bc2b9-08a0-5570-d4e9-ec38fc70bdd7"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("96872bda-3d33-5c47-bbd2-e6dbbb1de909"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("7904a46b-946e-efff-4c82-78a6a5b558aa"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ddb5f59a-810c-7f17-51c5-62219ce56f6b"), new Guid("317af55d-dd61-bbd7-6290-1ff6508f3f8a") },
                    { new Guid("7f4b9534-1292-1950-b7b8-7cb937c01591"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("caf9b346-4405-e8b5-4d30-0e5b787944ef"), new Guid("304d970e-93fc-82d5-ea7e-68bd7be5d96f") },
                    { new Guid("9c186096-65e3-eb23-1b7b-15e18b72129a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("96b97ff0-c80a-26d1-b9b1-41db6521d38a"), new Guid("317af55d-dd61-bbd7-6290-1ff6508f3f8a") },
                    { new Guid("9e477f64-07d3-6f4a-a794-1b77e21fa41e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("caf9b346-4405-e8b5-4d30-0e5b787944ef"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("aceee537-7fcd-ca0d-d836-115da29c1154"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("94c8d580-33ae-530b-6d52-efac0cf3fc28"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("ae2c65fc-3ae7-1f2f-da39-4ec7ac1902e9"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("94c8d580-33ae-530b-6d52-efac0cf3fc28"), new Guid("317af55d-dd61-bbd7-6290-1ff6508f3f8a") },
                    { new Guid("bf150f60-6cda-4244-e530-191e5dd8d204"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("96b97ff0-c80a-26d1-b9b1-41db6521d38a"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("c5c95a31-b797-f4a6-d900-5258c82723fe"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("ddb5f59a-810c-7f17-51c5-62219ce56f6b"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("ca402aea-bed9-b702-6a53-a28249c7885a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9a2f5bdb-3afc-86ce-4464-aa92d682f91f"), new Guid("317af55d-dd61-bbd7-6290-1ff6508f3f8a") },
                    { new Guid("cf11a139-4023-636b-d7e8-bbb2d3caa578"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4324f541-58db-3c7f-7841-5391a060c45d"), new Guid("304d970e-93fc-82d5-ea7e-68bd7be5d96f") },
                    { new Guid("d4ee79c9-dd88-8649-980f-bcfcf930951e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("b00efef6-8e69-7130-5654-4d2b166d4063"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("d8067995-911d-ba02-be59-bb1cbcff98c6"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("caf9b346-4405-e8b5-4d30-0e5b787944ef"), new Guid("317af55d-dd61-bbd7-6290-1ff6508f3f8a") },
                    { new Guid("e342a08b-38b1-e94f-2052-8bed37e7dd6e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4324f541-58db-3c7f-7841-5391a060c45d"), new Guid("317af55d-dd61-bbd7-6290-1ff6508f3f8a") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryControlItems_MescItemId",
                schema: "orders",
                table: "InventoryControlItems",
                column: "MescItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialNeeds_NeedNumber",
                schema: "orders",
                table: "MaterialNeeds",
                column: "NeedNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialNeedSequences_Year",
                schema: "orders",
                table: "MaterialNeedSequences",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortageAlerts_MescItemId_Status",
                schema: "orders",
                table: "ShortageAlerts",
                columns: new[] { "MescItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_MescItemId_WarehouseId",
                schema: "orders",
                table: "StockBalances",
                columns: new[] { "MescItemId", "WarehouseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryControlItems",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "MaterialNeeds",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "MaterialNeedSequences",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "ShortageAlerts",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "StockBalances",
                schema: "orders");

            migrationBuilder.DeleteData(
                schema: "org",
                table: "DepartmentMenuItems",
                keyColumn: "Id",
                keyValue: new Guid("305761b9-297b-0ba6-975c-5ee9d7a643a1"));

            migrationBuilder.DeleteData(
                schema: "org",
                table: "DepartmentMenuItems",
                keyColumn: "Id",
                keyValue: new Guid("4bab8657-b8e7-f713-037d-c455289f71fe"));

            migrationBuilder.DeleteData(
                schema: "org",
                table: "DepartmentMenuItems",
                keyColumn: "Id",
                keyValue: new Guid("4e5d4dbc-57b7-0fdc-4de9-bbdb23c9df2d"));

            migrationBuilder.DeleteData(
                schema: "org",
                table: "DepartmentMenuItems",
                keyColumn: "Id",
                keyValue: new Guid("f8cc80c5-eb3a-7e0b-d506-7f5fa721a491"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("02077577-3ad5-8174-9114-6da45c8d280f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("054f3bd4-3d8c-ebd1-db07-eabc1f42e714"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("090cd4d5-aaa9-954a-4810-6f85bd874256"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("268d76d1-3966-5d1f-21aa-5584ef4cf34b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("360cae35-80dc-6551-1bc0-020b905fd905"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("3658f2e3-7afe-1c6f-2ba4-fcb272f13153"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("3c75222f-95a9-8628-bcaf-de77622ce787"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("403affc4-1875-5b81-2746-6573ffa94489"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("47a295a7-92e6-0f0d-c20a-b6a4b94ee7d0"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("5339ffed-7760-d858-2cd6-0923da5f3776"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("56ef7c05-7fcf-8b6c-b703-b6d3041b1e97"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("6837cd09-3647-1cf2-5eb4-fd008abe2206"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("70119563-e2c2-d4ce-7ae7-992e5ffc5288"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("744bc2b9-08a0-5570-d4e9-ec38fc70bdd7"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7904a46b-946e-efff-4c82-78a6a5b558aa"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("7f4b9534-1292-1950-b7b8-7cb937c01591"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("9c186096-65e3-eb23-1b7b-15e18b72129a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("9e477f64-07d3-6f4a-a794-1b77e21fa41e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("aceee537-7fcd-ca0d-d836-115da29c1154"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ae2c65fc-3ae7-1f2f-da39-4ec7ac1902e9"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("bf150f60-6cda-4244-e530-191e5dd8d204"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c5c95a31-b797-f4a6-d900-5258c82723fe"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("ca402aea-bed9-b702-6a53-a28249c7885a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("cf11a139-4023-636b-d7e8-bbb2d3caa578"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d4ee79c9-dd88-8649-980f-bcfcf930951e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("d8067995-911d-ba02-be59-bb1cbcff98c6"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e342a08b-38b1-e94f-2052-8bed37e7dd6e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4324f541-58db-3c7f-7841-5391a060c45d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6dcb9583-f266-637b-2498-c317ccafc31a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("94c8d580-33ae-530b-6d52-efac0cf3fc28"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("96872bda-3d33-5c47-bbd2-e6dbbb1de909"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("96b97ff0-c80a-26d1-b9b1-41db6521d38a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9a2f5bdb-3afc-86ce-4464-aa92d682f91f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b00efef6-8e69-7130-5654-4d2b166d4063"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("caf9b346-4405-e8b5-4d30-0e5b787944ef"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ddb5f59a-810c-7f17-51c5-62219ce56f6b"));

            migrationBuilder.UpdateData(
                schema: "org",
                table: "DepartmentMenuItems",
                keyColumn: "Id",
                keyValue: new Guid("e755fc22-f997-ceeb-4688-d3015885d8dd"),
                columns: new[] { "Order", "Route" },
                values: new object[] { 1, "/indents" });
        }
    }
}

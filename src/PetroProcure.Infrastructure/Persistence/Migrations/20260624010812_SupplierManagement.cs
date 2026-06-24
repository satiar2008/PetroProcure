using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SupplierManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "supplier");

            migrationBuilder.CreateTable(
                name: "SupplierCategories",
                schema: "supplier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                schema: "supplier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EconomicCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsBlacklisted = table.Column<bool>(type: "bit", nullable: false),
                    BlacklistReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierCategoryAssignments",
                schema: "supplier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCategoryAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierCategoryAssignments_SupplierCategories_SupplierCategoryId",
                        column: x => x.SupplierCategoryId,
                        principalSchema: "supplier",
                        principalTable: "SupplierCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierCategoryAssignments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierContacts",
                schema: "supplier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContacts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierDocuments",
                schema: "supplier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDocuments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierEvaluations",
                schema: "supplier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EvaluatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierEvaluations_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "supplier",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "org",
                table: "DepartmentMenuItems",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DepartmentType", "IsVisible", "ModifiedAtUtc", "ModifiedBy", "Order", "RequiredPermission", "Route", "Title" },
                values: new object[] { new Guid("b115fbe1-2df9-c387-bda0-05fa77b5b109"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, true, null, null, 2, "Supplier.View", "/purchase/suppliers", "تأمین‌کنندگان" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Permissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("4d1a015c-3899-1cbf-d725-5648a82f708c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Supplier.Create", true, null, null, "Supplier.Create" },
                    { new Guid("702f57c5-e87b-fa6d-b918-0a0b7839fae3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Supplier.ManageContacts", true, null, null, "Supplier.ManageContacts" },
                    { new Guid("83e7e0f0-1241-dc5e-1058-d84e840d8cef"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Supplier.ManageDocuments", true, null, null, "Supplier.ManageDocuments" },
                    { new Guid("9e362b12-f0ae-3e4c-43fc-b875ee3ccb19"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Supplier.Evaluate", true, null, null, "Supplier.Evaluate" },
                    { new Guid("a6bb94d0-1313-ed09-9b66-731815bde1ca"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Supplier.Edit", true, null, null, "Supplier.Edit" },
                    { new Guid("a88b6d41-12c2-141e-6481-93cc2ee5ee3f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Supplier.Blacklist", true, null, null, "Supplier.Blacklist" },
                    { new Guid("a95d6b1c-b611-4959-06ff-0bf868581055"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Supplier.View", true, null, null, "Supplier.View" },
                    { new Guid("d58b1e80-5716-7a94-57b7-a68432dc150e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Supplier.ActivateDeactivate", true, null, null, "Supplier.ActivateDeactivate" },
                    { new Guid("d63cf6a5-444e-e4b8-4033-9cb617377d33"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Supplier.ManageCategories", true, null, null, "Supplier.ManageCategories" }
                });

            migrationBuilder.InsertData(
                schema: "supplier",
                table: "SupplierCategories",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Title" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), "MECH", null, true, "تجهیزات مکانیکی" },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), "ELEC", null, true, "تجهیزات برقی" },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), "INST", null, true, "ابزار دقیق" },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), "CHEM", null, true, "مواد شیمیایی" },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), "CONT", null, true, "خدمات پیمانکاری" }
                });

            migrationBuilder.InsertData(
                schema: "supplier",
                table: "Suppliers",
                columns: new[] { "Id", "Address", "BlacklistReason", "City", "Country", "CreatedAt", "CreatedAtUtc", "CreatedBy", "CreatedByUserId", "Description", "EconomicCode", "Email", "IsActive", "IsBlacklisted", "ModifiedAtUtc", "ModifiedBy", "Name", "NationalId", "Phone", "PostalCode", "RegistrationNumber", "Status", "SupplierCode", "SupplierType", "UpdatedAt", "UpdatedByUserId", "Website" },
                values: new object[,]
                {
                    { new Guid("a2000000-0000-0000-0000-000000000001"), null, null, "اهواز", "ایران", new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), "داده نمونه تأمین‌کننده", null, "sup-0001@example.local", true, false, null, null, "تأمین صنعت جنوب", null, "00000000", null, null, "Active", "SUP-0001", "Distributor", null, null, null },
                    { new Guid("a2000000-0000-0000-0000-000000000002"), null, null, "تهران", "ایران", new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), "داده نمونه تأمین‌کننده", null, "sup-0002@example.local", true, false, null, null, "پارس تجهیز پالایش", null, "00000000", null, null, "Active", "SUP-0002", "Manufacturer", null, null, null },
                    { new Guid("a2000000-0000-0000-0000-000000000003"), null, null, "بوشهر", "ایران", new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), "داده نمونه تأمین‌کننده", null, "sup-0003@example.local", true, false, null, null, "ابزار دقیق خلیج فارس", null, "00000000", null, null, "Active", "SUP-0003", "Distributor", null, null, null },
                    { new Guid("a2000000-0000-0000-0000-000000000004"), null, null, "شیراز", "ایران", new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), "داده نمونه تأمین‌کننده", null, "sup-0004@example.local", true, false, null, null, "شیمی گستر ایرانیان", null, "00000000", null, null, "Active", "SUP-0004", "Manufacturer", null, null, null },
                    { new Guid("a2000000-0000-0000-0000-000000000005"), null, null, "بندرعباس", "ایران", new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 15, 8, 0, 0, 0, DateTimeKind.Utc), null, new Guid("946e2251-f534-9be8-7aa7-e7cc5a303ab7"), "داده نمونه تأمین‌کننده", null, "sup-0005@example.local", true, false, null, null, "پیمانکاران انرژی ساحل", null, "00000000", null, null, "Active", "SUP-0005", "Contractor", null, null, null }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "ModifiedAtUtc", "ModifiedBy", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("0904fd4d-7679-ad3f-a201-6952b13db444"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4d1a015c-3899-1cbf-d725-5648a82f708c"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("0976bb07-b54b-f71b-551c-2f1eec5643d1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d63cf6a5-444e-e4b8-4033-9cb617377d33"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("18f86391-bd5f-fef8-7c82-0963f776ff87"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4d1a015c-3899-1cbf-d725-5648a82f708c"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("28200ed8-b44d-2049-0c08-3dd4f988146d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a95d6b1c-b611-4959-06ff-0bf868581055"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("28b0f287-b14e-d881-61f4-647f5a7cf05b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a88b6d41-12c2-141e-6481-93cc2ee5ee3f"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("2a045757-5866-d67d-9b7b-09c326fde66d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("83e7e0f0-1241-dc5e-1058-d84e840d8cef"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("3f72bdce-55de-2322-2064-9ded1e4c8dfe"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("702f57c5-e87b-fa6d-b918-0a0b7839fae3"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("52e49221-d9d1-1f71-b74f-a0d480da2791"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9e362b12-f0ae-3e4c-43fc-b875ee3ccb19"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("5b73315c-71cb-47a6-b6ce-58ad63d62af3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d63cf6a5-444e-e4b8-4033-9cb617377d33"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("61f9255c-6479-3cf0-eebb-caf5f02eb189"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a6bb94d0-1313-ed09-9b66-731815bde1ca"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("8185f41e-2500-a59c-03f3-e7d3c084de34"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a6bb94d0-1313-ed09-9b66-731815bde1ca"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("89a5d3eb-e509-1807-122e-e71bde0416aa"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("4d1a015c-3899-1cbf-d725-5648a82f708c"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("8bf08062-f625-c2fc-b3cf-7a8c220eeb0e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d58b1e80-5716-7a94-57b7-a68432dc150e"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("901a3b52-6e2e-53c3-5018-9cbed2703474"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d63cf6a5-444e-e4b8-4033-9cb617377d33"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("9acf03d6-fbf1-b42d-20d7-0cb7a54e07e3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("702f57c5-e87b-fa6d-b918-0a0b7839fae3"), new Guid("963ff902-7536-ff69-e0a0-cc853740b340") },
                    { new Guid("aa765a36-76ae-f76f-a78b-dda3028a9b4e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a88b6d41-12c2-141e-6481-93cc2ee5ee3f"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("af5f1fb0-fa0d-8f47-42b2-87a621da4620"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a95d6b1c-b611-4959-06ff-0bf868581055"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("b7a715b1-cba7-cd0a-972c-d22bf6adcda1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a95d6b1c-b611-4959-06ff-0bf868581055"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("c25a5a82-9d1d-c522-b884-5d27a9df24f4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("83e7e0f0-1241-dc5e-1058-d84e840d8cef"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("c5f51796-5a18-34fa-2b35-fecc94e8f074"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a6bb94d0-1313-ed09-9b66-731815bde1ca"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("db449019-c9e4-83c0-268f-26102165f1ec"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("a95d6b1c-b611-4959-06ff-0bf868581055"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("e1cf9907-3b89-dee6-2208-5003e6de753a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9e362b12-f0ae-3e4c-43fc-b875ee3ccb19"), new Guid("a60291f2-2475-430d-a9eb-c6b0b2222f5a") },
                    { new Guid("e4a7c666-674a-053f-0c5a-321856abd885"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("702f57c5-e87b-fa6d-b918-0a0b7839fae3"), new Guid("7d43902b-c8bb-ce7d-e803-3ee387198dea") },
                    { new Guid("f8317671-558d-6233-afc7-290c0dc71060"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d58b1e80-5716-7a94-57b7-a68432dc150e"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") },
                    { new Guid("fa34003d-cddd-6471-fd3f-6f3c2c2813a8"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("9e362b12-f0ae-3e4c-43fc-b875ee3ccb19"), new Guid("ae1bb199-f970-51c7-ef1b-26eeff76e625") }
                });

            migrationBuilder.InsertData(
                schema: "supplier",
                table: "SupplierCategoryAssignments",
                columns: new[] { "Id", "SupplierCategoryId", "SupplierId" },
                values: new object[,]
                {
                    { new Guid("a4000000-0000-0000-0000-000000000001"), new Guid("a1000000-0000-0000-0000-000000000001"), new Guid("a2000000-0000-0000-0000-000000000001") },
                    { new Guid("a4000000-0000-0000-0000-000000000002"), new Guid("a1000000-0000-0000-0000-000000000001"), new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("a4000000-0000-0000-0000-000000000003"), new Guid("a1000000-0000-0000-0000-000000000003"), new Guid("a2000000-0000-0000-0000-000000000003") },
                    { new Guid("a4000000-0000-0000-0000-000000000004"), new Guid("a1000000-0000-0000-0000-000000000004"), new Guid("a2000000-0000-0000-0000-000000000004") },
                    { new Guid("a4000000-0000-0000-0000-000000000005"), new Guid("a1000000-0000-0000-0000-000000000005"), new Guid("a2000000-0000-0000-0000-000000000005") }
                });

            migrationBuilder.InsertData(
                schema: "supplier",
                table: "SupplierContacts",
                columns: new[] { "Id", "Description", "Email", "FullName", "IsActive", "IsPrimary", "Mobile", "Phone", "Position", "SupplierId" },
                values: new object[,]
                {
                    { new Guid("a3000000-0000-0000-0000-000000000001"), null, null, "علی رضایی", true, true, "96199999991", "06100000001", "مدیر فروش", new Guid("a2000000-0000-0000-0000-000000000001") },
                    { new Guid("a3000000-0000-0000-0000-000000000002"), null, null, "مریم احمدی", true, true, "92199999992", "02100000002", "کارشناس بازرگانی", new Guid("a2000000-0000-0000-0000-000000000002") },
                    { new Guid("a3000000-0000-0000-0000-000000000003"), null, null, "حسین کریمی", true, true, "97799999993", "07700000003", "مدیر حساب", new Guid("a2000000-0000-0000-0000-000000000003") },
                    { new Guid("a3000000-0000-0000-0000-000000000004"), null, null, "زهرا محمدی", true, true, "97199999994", "07100000004", "فروش سازمانی", new Guid("a2000000-0000-0000-0000-000000000004") },
                    { new Guid("a3000000-0000-0000-0000-000000000005"), null, null, "رضا امینی", true, true, "97699999995", "07600000005", "مدیر پروژه", new Guid("a2000000-0000-0000-0000-000000000005") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategories_Code",
                schema: "supplier",
                table: "SupplierCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategoryAssignments_SupplierCategoryId",
                schema: "supplier",
                table: "SupplierCategoryAssignments",
                column: "SupplierCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategoryAssignments_SupplierId_SupplierCategoryId",
                schema: "supplier",
                table: "SupplierCategoryAssignments",
                columns: new[] { "SupplierId", "SupplierCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContacts_SupplierId_IsPrimary_IsActive",
                schema: "supplier",
                table: "SupplierContacts",
                columns: new[] { "SupplierId", "IsPrimary", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_SupplierId",
                schema: "supplier",
                table: "SupplierDocuments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierEvaluations_SupplierId",
                schema: "supplier",
                table: "SupplierEvaluations",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_EconomicCode",
                schema: "supplier",
                table: "Suppliers",
                column: "EconomicCode");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_IsActive",
                schema: "supplier",
                table: "Suppliers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                schema: "supplier",
                table: "Suppliers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_NationalId",
                schema: "supplier",
                table: "Suppliers",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Status",
                schema: "supplier",
                table: "Suppliers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierCode",
                schema: "supplier",
                table: "Suppliers",
                column: "SupplierCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierCategoryAssignments",
                schema: "supplier");

            migrationBuilder.DropTable(
                name: "SupplierContacts",
                schema: "supplier");

            migrationBuilder.DropTable(
                name: "SupplierDocuments",
                schema: "supplier");

            migrationBuilder.DropTable(
                name: "SupplierEvaluations",
                schema: "supplier");

            migrationBuilder.DropTable(
                name: "SupplierCategories",
                schema: "supplier");

            migrationBuilder.DropTable(
                name: "Suppliers",
                schema: "supplier");

            migrationBuilder.DeleteData(
                schema: "org",
                table: "DepartmentMenuItems",
                keyColumn: "Id",
                keyValue: new Guid("b115fbe1-2df9-c387-bda0-05fa77b5b109"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0904fd4d-7679-ad3f-a201-6952b13db444"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("0976bb07-b54b-f71b-551c-2f1eec5643d1"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("18f86391-bd5f-fef8-7c82-0963f776ff87"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("28200ed8-b44d-2049-0c08-3dd4f988146d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("28b0f287-b14e-d881-61f4-647f5a7cf05b"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("2a045757-5866-d67d-9b7b-09c326fde66d"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("3f72bdce-55de-2322-2064-9ded1e4c8dfe"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("52e49221-d9d1-1f71-b74f-a0d480da2791"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("5b73315c-71cb-47a6-b6ce-58ad63d62af3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("61f9255c-6479-3cf0-eebb-caf5f02eb189"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8185f41e-2500-a59c-03f3-e7d3c084de34"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("89a5d3eb-e509-1807-122e-e71bde0416aa"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("8bf08062-f625-c2fc-b3cf-7a8c220eeb0e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("901a3b52-6e2e-53c3-5018-9cbed2703474"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("9acf03d6-fbf1-b42d-20d7-0cb7a54e07e3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("aa765a36-76ae-f76f-a78b-dda3028a9b4e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("af5f1fb0-fa0d-8f47-42b2-87a621da4620"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("b7a715b1-cba7-cd0a-972c-d22bf6adcda1"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c25a5a82-9d1d-c522-b884-5d27a9df24f4"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("c5f51796-5a18-34fa-2b35-fecc94e8f074"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("db449019-c9e4-83c0-268f-26102165f1ec"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e1cf9907-3b89-dee6-2208-5003e6de753a"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("e4a7c666-674a-053f-0c5a-321856abd885"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("f8317671-558d-6233-afc7-290c0dc71060"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: new Guid("fa34003d-cddd-6471-fd3f-6f3c2c2813a8"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4d1a015c-3899-1cbf-d725-5648a82f708c"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("702f57c5-e87b-fa6d-b918-0a0b7839fae3"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("83e7e0f0-1241-dc5e-1058-d84e840d8cef"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9e362b12-f0ae-3e4c-43fc-b875ee3ccb19"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a6bb94d0-1313-ed09-9b66-731815bde1ca"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a88b6d41-12c2-141e-6481-93cc2ee5ee3f"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a95d6b1c-b611-4959-06ff-0bf868581055"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d58b1e80-5716-7a94-57b7-a68432dc150e"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d63cf6a5-444e-e4b8-4033-9cb617377d33"));
        }
    }
}

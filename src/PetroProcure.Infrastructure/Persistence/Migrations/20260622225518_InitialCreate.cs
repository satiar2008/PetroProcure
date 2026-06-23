using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetroProcure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "org");

            migrationBuilder.EnsureSchema(
                name: "doc");

            migrationBuilder.EnsureSchema(
                name: "workflow");

            migrationBuilder.EnsureSchema(
                name: "indent");

            migrationBuilder.EnsureSchema(
                name: "item");

            migrationBuilder.EnsureSchema(
                name: "purchase");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "supplier");

            migrationBuilder.EnsureSchema(
                name: "tender");

            migrationBuilder.EnsureSchema(
                name: "warehouse");

            migrationBuilder.EnsureSchema(
                name: "report");

            migrationBuilder.EnsureSchema(
                name: "ai");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "ApplicationUserProfiles",
                schema: "org",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                schema: "org",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Indents",
                schema: "indent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IndentNumber = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    YearPart = table.Column<int>(type: "int", nullable: false),
                    TypeDigit = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MescGeneralGroups",
                schema: "item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MescGeneralGroups", x => x.Id);
                    table.UniqueConstraint("AK_MescGeneralGroups_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasures",
                schema: "item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDepartments",
                schema: "org",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDepartments_ApplicationUserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalSchema: "org",
                        principalTable: "ApplicationUserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "org",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseFiles",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IndentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseFiles_Indents_IndentId",
                        column: x => x.IndentId,
                        principalSchema: "indent",
                        principalTable: "Indents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MescItems",
                schema: "item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GeneralGroupCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MescItems", x => x.Id);
                    table.UniqueConstraint("AK_MescItems_Code", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MescItems_MescGeneralGroups_GeneralGroupCode",
                        column: x => x.GeneralGroupCode,
                        principalSchema: "item",
                        principalTable: "MescGeneralGroups",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileDocuments",
                schema: "doc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileDocuments_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InboxTasks",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedDepartment = table.Column<int>(type: "int", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtTaskUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboxTasks_ApplicationUserProfiles_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalSchema: "org",
                        principalTable: "ApplicationUserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InboxTasks_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseFileNotes",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseFileNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseFileNotes_ApplicationUserProfiles_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalSchema: "org",
                        principalTable: "ApplicationUserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseFileNotes_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseFileStatusHistories",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseFileStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseFileStatusHistories_ApplicationUserProfiles_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalSchema: "org",
                        principalTable: "ApplicationUserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseFileStatusHistories_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndentItems",
                schema: "indent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IndentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndentItems_Indents_IndentId",
                        column: x => x.IndentId,
                        principalSchema: "indent",
                        principalTable: "Indents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndentItems_MescItems_MescCode",
                        column: x => x.MescCode,
                        principalSchema: "item",
                        principalTable: "MescItems",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndentItems_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "item",
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseFileItems",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MescCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseFileItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseFileItems_MescItems_MescCode",
                        column: x => x.MescCode,
                        principalSchema: "item",
                        principalTable: "MescItems",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseFileItems_PurchaseFiles_PurchaseFileId",
                        column: x => x.PurchaseFileId,
                        principalSchema: "purchase",
                        principalTable: "PurchaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentVersions",
                schema: "doc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentVersions_FileDocuments_FileDocumentId",
                        column: x => x.FileDocumentId,
                        principalSchema: "doc",
                        principalTable: "FileDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSteps",
                schema: "workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalSchema: "workflow",
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "org",
                table: "Departments",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name", "Type" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "واحد خرید", 1 },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "سفارشات و کنترل موجودی", 2 },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "انبار", 3 },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "متقاضی", 4 },
                    { new Guid("10000000-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "کمیسیون مناقصه", 5 }
                });

            migrationBuilder.InsertData(
                schema: "item",
                table: "MescGeneralGroups",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "CreatedBy", "Description", "IsActive", "ModifiedAtUtc", "ModifiedBy" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), "123456", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "لوله و اتصالات عمومی", true, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000002"), "223344", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "شیرآلات صنعتی", true, null, null },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "334455", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "پمپ‌ها و تجهیزات دوار", true, null, null }
                });

            migrationBuilder.InsertData(
                schema: "item",
                table: "UnitOfMeasures",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "CreatedBy", "IsActive", "ModifiedAtUtc", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "EA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "عدد" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "M", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "متر" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "KG", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "کیلوگرم" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "L", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "لیتر" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "PKG", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "بسته" },
                    { new Guid("20000000-0000-0000-0000-000000000006"), "DEV", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, null, "دستگاه" }
                });

            migrationBuilder.InsertData(
                schema: "item",
                table: "MescItems",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "CreatedBy", "Description", "GeneralGroupCode", "IsActive", "ModifiedAtUtc", "ModifiedBy" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), "1234560001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "لوله فولادی عمومی", "123456", true, null, null },
                    { new Guid("40000000-0000-0000-0000-000000000002"), "1234560002", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "زانو فولادی عمومی", "123456", true, null, null },
                    { new Guid("40000000-0000-0000-0000-000000000003"), "2233440001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "شیر کشویی صنعتی", "223344", true, null, null },
                    { new Guid("40000000-0000-0000-0000-000000000004"), "3344550001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "پمپ سانتریفیوژ عمومی", "334455", true, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserProfiles_Email",
                schema: "org",
                table: "ApplicationUserProfiles",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                schema: "org",
                table: "Departments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Type",
                schema: "org",
                table: "Departments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_FileDocumentId",
                schema: "doc",
                table: "DocumentVersions",
                column: "FileDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentVersions_FileDocumentId_VersionNumber",
                schema: "doc",
                table: "DocumentVersions",
                columns: new[] { "FileDocumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileDocuments_PurchaseFileId",
                schema: "doc",
                table: "FileDocuments",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_FileDocuments_Type",
                schema: "doc",
                table: "FileDocuments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_InboxTasks_AssignedDepartment",
                schema: "workflow",
                table: "InboxTasks",
                column: "AssignedDepartment");

            migrationBuilder.CreateIndex(
                name: "IX_InboxTasks_AssignedUserId",
                schema: "workflow",
                table: "InboxTasks",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InboxTasks_PurchaseFileId",
                schema: "workflow",
                table: "InboxTasks",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_InboxTasks_Status",
                schema: "workflow",
                table: "InboxTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IndentItems_IndentId",
                schema: "indent",
                table: "IndentItems",
                column: "IndentId");

            migrationBuilder.CreateIndex(
                name: "IX_IndentItems_MescCode",
                schema: "indent",
                table: "IndentItems",
                column: "MescCode");

            migrationBuilder.CreateIndex(
                name: "IX_IndentItems_UnitOfMeasureId",
                schema: "indent",
                table: "IndentItems",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Indents_IndentNumber",
                schema: "indent",
                table: "Indents",
                column: "IndentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Indents_YearPart_TypeDigit_Sequence",
                schema: "indent",
                table: "Indents",
                columns: new[] { "YearPart", "TypeDigit", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MescGeneralGroups_Code",
                schema: "item",
                table: "MescGeneralGroups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MescItems_Code",
                schema: "item",
                table: "MescItems",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MescItems_GeneralGroupCode",
                schema: "item",
                table: "MescItems",
                column: "GeneralGroupCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileItems_MescCode",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "MescCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileItems_PurchaseFileId",
                schema: "purchase",
                table: "PurchaseFileItems",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileNotes_AuthorUserId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileNotes_PurchaseFileId",
                schema: "purchase",
                table: "PurchaseFileNotes",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFiles_FileNumber",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "FileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFiles_IndentId",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "IndentId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFiles_Status",
                schema: "purchase",
                table: "PurchaseFiles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileStatusHistories_ChangedAtUtc",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                column: "ChangedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileStatusHistories_ChangedByUserId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFileStatusHistories_PurchaseFileId",
                schema: "purchase",
                table: "PurchaseFileStatusHistories",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasures_Code",
                schema: "item",
                table: "UnitOfMeasures",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasures_Name",
                schema: "item",
                table: "UnitOfMeasures",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartments_DepartmentId",
                schema: "org",
                table: "UserDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDepartments_UserProfileId_DepartmentId",
                schema: "org",
                table: "UserDepartments",
                columns: new[] { "UserProfileId", "DepartmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_PurchaseFileId",
                schema: "workflow",
                table: "WorkflowInstances",
                column: "PurchaseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowInstanceId",
                schema: "workflow",
                table: "WorkflowSteps",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowInstanceId_Order",
                schema: "workflow",
                table: "WorkflowSteps",
                columns: new[] { "WorkflowInstanceId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentVersions",
                schema: "doc");

            migrationBuilder.DropTable(
                name: "InboxTasks",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "IndentItems",
                schema: "indent");

            migrationBuilder.DropTable(
                name: "PurchaseFileItems",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "PurchaseFileNotes",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "PurchaseFileStatusHistories",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "UserDepartments",
                schema: "org");

            migrationBuilder.DropTable(
                name: "WorkflowSteps",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "FileDocuments",
                schema: "doc");

            migrationBuilder.DropTable(
                name: "UnitOfMeasures",
                schema: "item");

            migrationBuilder.DropTable(
                name: "MescItems",
                schema: "item");

            migrationBuilder.DropTable(
                name: "ApplicationUserProfiles",
                schema: "org");

            migrationBuilder.DropTable(
                name: "Departments",
                schema: "org");

            migrationBuilder.DropTable(
                name: "WorkflowInstances",
                schema: "workflow");

            migrationBuilder.DropTable(
                name: "MescGeneralGroups",
                schema: "item");

            migrationBuilder.DropTable(
                name: "PurchaseFiles",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "Indents",
                schema: "indent");
        }
    }
}

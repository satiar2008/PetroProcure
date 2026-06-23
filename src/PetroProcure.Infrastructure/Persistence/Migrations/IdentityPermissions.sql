IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'org') IS NULL EXEC(N'CREATE SCHEMA [org];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'doc') IS NULL EXEC(N'CREATE SCHEMA [doc];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'workflow') IS NULL EXEC(N'CREATE SCHEMA [workflow];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'indent') IS NULL EXEC(N'CREATE SCHEMA [indent];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'item') IS NULL EXEC(N'CREATE SCHEMA [item];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'purchase') IS NULL EXEC(N'CREATE SCHEMA [purchase];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'identity') IS NULL EXEC(N'CREATE SCHEMA [identity];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'supplier') IS NULL EXEC(N'CREATE SCHEMA [supplier];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'tender') IS NULL EXEC(N'CREATE SCHEMA [tender];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'warehouse') IS NULL EXEC(N'CREATE SCHEMA [warehouse];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'report') IS NULL EXEC(N'CREATE SCHEMA [report];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'ai') IS NULL EXEC(N'CREATE SCHEMA [ai];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF SCHEMA_ID(N'audit') IS NULL EXEC(N'CREATE SCHEMA [audit];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [org].[ApplicationUserProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [DisplayName] nvarchar(200) NOT NULL,
        [Email] nvarchar(320) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_ApplicationUserProfiles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [org].[Departments] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Type] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [indent].[Indents] (
        [Id] uniqueidentifier NOT NULL,
        [IndentNumber] nvarchar(7) NOT NULL,
        [YearPart] int NOT NULL,
        [TypeDigit] int NOT NULL,
        [Sequence] int NOT NULL,
        [Type] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_Indents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [item].[MescGeneralGroups] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(6) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_MescGeneralGroups] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_MescGeneralGroups_Code] UNIQUE ([Code])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [item].[UnitOfMeasures] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_UnitOfMeasures] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [org].[UserDepartments] (
        [Id] uniqueidentifier NOT NULL,
        [UserProfileId] uniqueidentifier NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [IsPrimary] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_UserDepartments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserDepartments_ApplicationUserProfiles_UserProfileId] FOREIGN KEY ([UserProfileId]) REFERENCES [org].[ApplicationUserProfiles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserDepartments_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [org].[Departments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [purchase].[PurchaseFiles] (
        [Id] uniqueidentifier NOT NULL,
        [FileNumber] nvarchar(50) NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [IndentId] uniqueidentifier NULL,
        [Status] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_PurchaseFiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseFiles_Indents_IndentId] FOREIGN KEY ([IndentId]) REFERENCES [indent].[Indents] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [item].[MescItems] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [GeneralGroupCode] nvarchar(6) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_MescItems] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_MescItems_Code] UNIQUE ([Code]),
        CONSTRAINT [FK_MescItems_MescGeneralGroups_GeneralGroupCode] FOREIGN KEY ([GeneralGroupCode]) REFERENCES [item].[MescGeneralGroups] ([Code]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [doc].[FileDocuments] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseFileId] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_FileDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FileDocuments_PurchaseFiles_PurchaseFileId] FOREIGN KEY ([PurchaseFileId]) REFERENCES [purchase].[PurchaseFiles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [workflow].[InboxTasks] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseFileId] uniqueidentifier NOT NULL,
        [AssignedDepartment] int NOT NULL,
        [AssignedUserId] uniqueidentifier NULL,
        [Title] nvarchar(300) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAtTaskUtc] datetime2 NOT NULL,
        [CompletedAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_InboxTasks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InboxTasks_ApplicationUserProfiles_AssignedUserId] FOREIGN KEY ([AssignedUserId]) REFERENCES [org].[ApplicationUserProfiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InboxTasks_PurchaseFiles_PurchaseFileId] FOREIGN KEY ([PurchaseFileId]) REFERENCES [purchase].[PurchaseFiles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [purchase].[PurchaseFileNotes] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseFileId] uniqueidentifier NOT NULL,
        [Text] nvarchar(2000) NOT NULL,
        [AuthorUserId] uniqueidentifier NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_PurchaseFileNotes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseFileNotes_ApplicationUserProfiles_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [org].[ApplicationUserProfiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PurchaseFileNotes_PurchaseFiles_PurchaseFileId] FOREIGN KEY ([PurchaseFileId]) REFERENCES [purchase].[PurchaseFiles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [purchase].[PurchaseFileStatusHistories] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseFileId] uniqueidentifier NOT NULL,
        [FromStatus] int NOT NULL,
        [ToStatus] int NOT NULL,
        [ChangedByUserId] uniqueidentifier NULL,
        [Note] nvarchar(1000) NULL,
        [ChangedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_PurchaseFileStatusHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseFileStatusHistories_ApplicationUserProfiles_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [org].[ApplicationUserProfiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PurchaseFileStatusHistories_PurchaseFiles_PurchaseFileId] FOREIGN KEY ([PurchaseFileId]) REFERENCES [purchase].[PurchaseFiles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [workflow].[WorkflowInstances] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseFileId] uniqueidentifier NOT NULL,
        [Status] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_WorkflowInstances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkflowInstances_PurchaseFiles_PurchaseFileId] FOREIGN KEY ([PurchaseFileId]) REFERENCES [purchase].[PurchaseFiles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [indent].[IndentItems] (
        [Id] uniqueidentifier NOT NULL,
        [IndentId] uniqueidentifier NOT NULL,
        [MescCode] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Quantity] decimal(18,4) NOT NULL,
        [UnitOfMeasureId] uniqueidentifier NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_IndentItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IndentItems_Indents_IndentId] FOREIGN KEY ([IndentId]) REFERENCES [indent].[Indents] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_IndentItems_MescItems_MescCode] FOREIGN KEY ([MescCode]) REFERENCES [item].[MescItems] ([Code]) ON DELETE NO ACTION,
        CONSTRAINT [FK_IndentItems_UnitOfMeasures_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [item].[UnitOfMeasures] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [purchase].[PurchaseFileItems] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseFileId] uniqueidentifier NOT NULL,
        [MescCode] nvarchar(50) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Quantity] decimal(18,4) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_PurchaseFileItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseFileItems_MescItems_MescCode] FOREIGN KEY ([MescCode]) REFERENCES [item].[MescItems] ([Code]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PurchaseFileItems_PurchaseFiles_PurchaseFileId] FOREIGN KEY ([PurchaseFileId]) REFERENCES [purchase].[PurchaseFiles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [doc].[DocumentVersions] (
        [Id] uniqueidentifier NOT NULL,
        [FileDocumentId] uniqueidentifier NOT NULL,
        [VersionNumber] int NOT NULL,
        [OriginalFileName] nvarchar(260) NOT NULL,
        [RelativePath] nvarchar(1000) NOT NULL,
        [ContentHash] nvarchar(128) NULL,
        [UploadedAtUtc] datetime2 NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_DocumentVersions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DocumentVersions_FileDocuments_FileDocumentId] FOREIGN KEY ([FileDocumentId]) REFERENCES [doc].[FileDocuments] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE TABLE [workflow].[WorkflowSteps] (
        [Id] uniqueidentifier NOT NULL,
        [WorkflowInstanceId] uniqueidentifier NOT NULL,
        [DepartmentType] int NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [Order] int NOT NULL,
        [Status] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_WorkflowSteps] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkflowSteps_WorkflowInstances_WorkflowInstanceId] FOREIGN KEY ([WorkflowInstanceId]) REFERENCES [workflow].[WorkflowInstances] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'CreatedBy', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy', N'Name', N'Type') AND [object_id] = OBJECT_ID(N'[org].[Departments]'))
        SET IDENTITY_INSERT [org].[Departments] ON;
    EXEC(N'INSERT INTO [org].[Departments] ([Id], [CreatedAtUtc], [CreatedBy], [IsActive], [ModifiedAtUtc], [ModifiedBy], [Name], [Type])
    VALUES (''10000000-0000-0000-0000-000000000001'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''واحد خرید'', 1),
    (''10000000-0000-0000-0000-000000000002'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''سفارشات و کنترل موجودی'', 2),
    (''10000000-0000-0000-0000-000000000003'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''انبار'', 3),
    (''10000000-0000-0000-0000-000000000004'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''متقاضی'', 4),
    (''10000000-0000-0000-0000-000000000005'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''کمیسیون مناقصه'', 5)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'CreatedBy', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy', N'Name', N'Type') AND [object_id] = OBJECT_ID(N'[org].[Departments]'))
        SET IDENTITY_INSERT [org].[Departments] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'CreatedBy', N'Description', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy') AND [object_id] = OBJECT_ID(N'[item].[MescGeneralGroups]'))
        SET IDENTITY_INSERT [item].[MescGeneralGroups] ON;
    EXEC(N'INSERT INTO [item].[MescGeneralGroups] ([Id], [Code], [CreatedAtUtc], [CreatedBy], [Description], [IsActive], [ModifiedAtUtc], [ModifiedBy])
    VALUES (''30000000-0000-0000-0000-000000000001'', N''123456'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''لوله و اتصالات عمومی'', CAST(1 AS bit), NULL, NULL),
    (''30000000-0000-0000-0000-000000000002'', N''223344'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''شیرآلات صنعتی'', CAST(1 AS bit), NULL, NULL),
    (''30000000-0000-0000-0000-000000000003'', N''334455'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''پمپ‌ها و تجهیزات دوار'', CAST(1 AS bit), NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'CreatedBy', N'Description', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy') AND [object_id] = OBJECT_ID(N'[item].[MescGeneralGroups]'))
        SET IDENTITY_INSERT [item].[MescGeneralGroups] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'CreatedBy', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[item].[UnitOfMeasures]'))
        SET IDENTITY_INSERT [item].[UnitOfMeasures] ON;
    EXEC(N'INSERT INTO [item].[UnitOfMeasures] ([Id], [Code], [CreatedAtUtc], [CreatedBy], [IsActive], [ModifiedAtUtc], [ModifiedBy], [Name])
    VALUES (''20000000-0000-0000-0000-000000000001'', N''EA'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''عدد''),
    (''20000000-0000-0000-0000-000000000002'', N''M'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''متر''),
    (''20000000-0000-0000-0000-000000000003'', N''KG'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''کیلوگرم''),
    (''20000000-0000-0000-0000-000000000004'', N''L'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''لیتر''),
    (''20000000-0000-0000-0000-000000000005'', N''PKG'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''بسته''),
    (''20000000-0000-0000-0000-000000000006'', N''DEV'', ''2026-01-01T00:00:00.0000000Z'', NULL, CAST(1 AS bit), NULL, NULL, N''دستگاه'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'CreatedBy', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[item].[UnitOfMeasures]'))
        SET IDENTITY_INSERT [item].[UnitOfMeasures] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'CreatedBy', N'Description', N'GeneralGroupCode', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy') AND [object_id] = OBJECT_ID(N'[item].[MescItems]'))
        SET IDENTITY_INSERT [item].[MescItems] ON;
    EXEC(N'INSERT INTO [item].[MescItems] ([Id], [Code], [CreatedAtUtc], [CreatedBy], [Description], [GeneralGroupCode], [IsActive], [ModifiedAtUtc], [ModifiedBy])
    VALUES (''40000000-0000-0000-0000-000000000001'', N''1234560001'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''لوله فولادی عمومی'', N''123456'', CAST(1 AS bit), NULL, NULL),
    (''40000000-0000-0000-0000-000000000002'', N''1234560002'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''زانو فولادی عمومی'', N''123456'', CAST(1 AS bit), NULL, NULL),
    (''40000000-0000-0000-0000-000000000003'', N''2233440001'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''شیر کشویی صنعتی'', N''223344'', CAST(1 AS bit), NULL, NULL),
    (''40000000-0000-0000-0000-000000000004'', N''3344550001'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''پمپ سانتریفیوژ عمومی'', N''334455'', CAST(1 AS bit), NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'CreatedBy', N'Description', N'GeneralGroupCode', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy') AND [object_id] = OBJECT_ID(N'[item].[MescItems]'))
        SET IDENTITY_INSERT [item].[MescItems] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ApplicationUserProfiles_Email] ON [org].[ApplicationUserProfiles] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Departments_Name] ON [org].[Departments] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Departments_Type] ON [org].[Departments] ([Type]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DocumentVersions_FileDocumentId] ON [doc].[DocumentVersions] ([FileDocumentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DocumentVersions_FileDocumentId_VersionNumber] ON [doc].[DocumentVersions] ([FileDocumentId], [VersionNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FileDocuments_PurchaseFileId] ON [doc].[FileDocuments] ([PurchaseFileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FileDocuments_Type] ON [doc].[FileDocuments] ([Type]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InboxTasks_AssignedDepartment] ON [workflow].[InboxTasks] ([AssignedDepartment]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InboxTasks_AssignedUserId] ON [workflow].[InboxTasks] ([AssignedUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InboxTasks_PurchaseFileId] ON [workflow].[InboxTasks] ([PurchaseFileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InboxTasks_Status] ON [workflow].[InboxTasks] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_IndentItems_IndentId] ON [indent].[IndentItems] ([IndentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_IndentItems_MescCode] ON [indent].[IndentItems] ([MescCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_IndentItems_UnitOfMeasureId] ON [indent].[IndentItems] ([UnitOfMeasureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Indents_IndentNumber] ON [indent].[Indents] ([IndentNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Indents_YearPart_TypeDigit_Sequence] ON [indent].[Indents] ([YearPart], [TypeDigit], [Sequence]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MescGeneralGroups_Code] ON [item].[MescGeneralGroups] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MescItems_Code] ON [item].[MescItems] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MescItems_GeneralGroupCode] ON [item].[MescItems] ([GeneralGroupCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseFileItems_MescCode] ON [purchase].[PurchaseFileItems] ([MescCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseFileItems_PurchaseFileId] ON [purchase].[PurchaseFileItems] ([PurchaseFileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseFileNotes_AuthorUserId] ON [purchase].[PurchaseFileNotes] ([AuthorUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseFileNotes_PurchaseFileId] ON [purchase].[PurchaseFileNotes] ([PurchaseFileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchaseFiles_FileNumber] ON [purchase].[PurchaseFiles] ([FileNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseFiles_IndentId] ON [purchase].[PurchaseFiles] ([IndentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseFiles_Status] ON [purchase].[PurchaseFiles] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseFileStatusHistories_ChangedAtUtc] ON [purchase].[PurchaseFileStatusHistories] ([ChangedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseFileStatusHistories_ChangedByUserId] ON [purchase].[PurchaseFileStatusHistories] ([ChangedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseFileStatusHistories_PurchaseFileId] ON [purchase].[PurchaseFileStatusHistories] ([PurchaseFileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UnitOfMeasures_Code] ON [item].[UnitOfMeasures] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UnitOfMeasures_Name] ON [item].[UnitOfMeasures] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserDepartments_DepartmentId] ON [org].[UserDepartments] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserDepartments_UserProfileId_DepartmentId] ON [org].[UserDepartments] ([UserProfileId], [DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkflowInstances_PurchaseFileId] ON [workflow].[WorkflowInstances] ([PurchaseFileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkflowSteps_WorkflowInstanceId] ON [workflow].[WorkflowSteps] ([WorkflowInstanceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_WorkflowSteps_WorkflowInstanceId_Order] ON [workflow].[WorkflowSteps] ([WorkflowInstanceId], [Order]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622225518_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260622225518_InitialCreate', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    IF SCHEMA_ID(N'identity') IS NULL EXEC(N'CREATE SCHEMA [identity];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE TABLE [org].[DepartmentMenuItems] (
        [Id] uniqueidentifier NOT NULL,
        [DepartmentType] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Route] nvarchar(300) NOT NULL,
        [RequiredPermission] nvarchar(150) NULL,
        [Order] int NOT NULL,
        [IsVisible] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_DepartmentMenuItems] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE TABLE [identity].[Permissions] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE TABLE [identity].[Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE TABLE [identity].[Users] (
        [Id] uniqueidentifier NOT NULL,
        [UserProfileId] uniqueidentifier NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_ApplicationUserProfiles_UserProfileId] FOREIGN KEY ([UserProfileId]) REFERENCES [org].[ApplicationUserProfiles] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE TABLE [identity].[RoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_RoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoleClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [identity].[Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE TABLE [identity].[RolePermissions] (
        [Id] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [PermissionId] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedBy] nvarchar(100) NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [identity].[Permissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [identity].[Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE TABLE [identity].[UserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE TABLE [identity].[UserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE TABLE [identity].[UserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [identity].[Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE TABLE [identity].[UserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'CreatedBy', N'DisplayName', N'Email', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy') AND [object_id] = OBJECT_ID(N'[org].[ApplicationUserProfiles]'))
        SET IDENTITY_INSERT [org].[ApplicationUserProfiles] ON;
    EXEC(N'INSERT INTO [org].[ApplicationUserProfiles] ([Id], [CreatedAtUtc], [CreatedBy], [DisplayName], [Email], [IsActive], [ModifiedAtUtc], [ModifiedBy])
    VALUES (''ea73b3d2-ada1-3014-e807-0287736b56c8'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''مدیر سامانه'', N''admin@petroprocure.local'', CAST(1 AS bit), NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'CreatedBy', N'DisplayName', N'Email', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy') AND [object_id] = OBJECT_ID(N'[org].[ApplicationUserProfiles]'))
        SET IDENTITY_INSERT [org].[ApplicationUserProfiles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'CreatedBy', N'DepartmentType', N'IsVisible', N'ModifiedAtUtc', N'ModifiedBy', N'Order', N'RequiredPermission', N'Route', N'Title') AND [object_id] = OBJECT_ID(N'[org].[DepartmentMenuItems]'))
        SET IDENTITY_INSERT [org].[DepartmentMenuItems] ON;
    EXEC(N'INSERT INTO [org].[DepartmentMenuItems] ([Id], [CreatedAtUtc], [CreatedBy], [DepartmentType], [IsVisible], [ModifiedAtUtc], [ModifiedBy], [Order], [RequiredPermission], [Route], [Title])
    VALUES (''2f80ca31-8359-9994-135a-3a782c71bf4f'', ''2026-01-01T00:00:00.0000000Z'', NULL, 4, CAST(1 AS bit), NULL, NULL, 1, N''Indent.Create'', N''/applicant/requests'', N''درخواست‌های من''),
    (''31bf56ae-e556-b51a-f761-6de74c50d4b5'', ''2026-01-01T00:00:00.0000000Z'', NULL, 5, CAST(1 AS bit), NULL, NULL, 1, N''Tender.View'', N''/tenders'', N''کمیسیون مناقصه''),
    (''45dfbc5f-2534-97b2-2305-4eac32110b27'', ''2026-01-01T00:00:00.0000000Z'', NULL, 1, CAST(1 AS bit), NULL, NULL, 1, N''PurchaseFile.View'', N''/purchase-files'', N''پرونده‌های خرید''),
    (''d8d58a49-2d1e-6882-b5a2-52756b006e96'', ''2026-01-01T00:00:00.0000000Z'', NULL, 3, CAST(1 AS bit), NULL, NULL, 1, N''Warehouse.View'', N''/warehouse'', N''عملیات انبار''),
    (''e755fc22-f997-ceeb-4688-d3015885d8dd'', ''2026-01-01T00:00:00.0000000Z'', NULL, 2, CAST(1 AS bit), NULL, NULL, 1, N''Indent.View'', N''/indents'', N''درخواست‌های خرید'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'CreatedBy', N'DepartmentType', N'IsVisible', N'ModifiedAtUtc', N'ModifiedBy', N'Order', N'RequiredPermission', N'Route', N'Title') AND [object_id] = OBJECT_ID(N'[org].[DepartmentMenuItems]'))
        SET IDENTITY_INSERT [org].[DepartmentMenuItems] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'CreatedBy', N'Description', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [CreatedAtUtc], [CreatedBy], [Description], [IsActive], [ModifiedAtUtc], [ModifiedBy], [Name])
    VALUES (''02eecd9e-94ae-ef20-8cbf-bf5922f4ccfd'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Warehouse.Issue'', CAST(1 AS bit), NULL, NULL, N''Warehouse.Issue''),
    (''12ad9300-f80e-f386-070c-c93f0117b66f'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''AiAgent.EvaluatePurchaseRules'', CAST(1 AS bit), NULL, NULL, N''AiAgent.EvaluatePurchaseRules''),
    (''1ab75a4b-ef24-8a2a-bc82-9ae054003d86'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Warehouse.Receive'', CAST(1 AS bit), NULL, NULL, N''Warehouse.Receive''),
    (''1d35913f-cea9-6fa2-2bb6-17009ea75417'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Report.ExportPdf'', CAST(1 AS bit), NULL, NULL, N''Report.ExportPdf''),
    (''1edcbcf3-5ff0-1b70-4cb2-ce687a9b6b5b'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''PurchaseFile.View'', CAST(1 AS bit), NULL, NULL, N''PurchaseFile.View''),
    (''3d0bccfd-85cc-bbb7-32ef-74500ed50c77'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Tender.View'', CAST(1 AS bit), NULL, NULL, N''Tender.View''),
    (''439721fe-7142-ccc3-e55b-63d639a8f9a5'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Indent.Create'', CAST(1 AS bit), NULL, NULL, N''Indent.Create''),
    (''4af786fa-8324-fbdd-5e66-8f6308a2874e'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Tender.Evaluate'', CAST(1 AS bit), NULL, NULL, N''Tender.Evaluate''),
    (''593e28d2-5014-7026-81e0-d0b498c5cc5a'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Warehouse.View'', CAST(1 AS bit), NULL, NULL, N''Warehouse.View''),
    (''59d99406-fa77-512c-96c2-04ec0e579211'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Report.Print'', CAST(1 AS bit), NULL, NULL, N''Report.Print''),
    (''5dd9cf0f-211b-0ead-b79b-f073a991273d'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Admin.ManageSettings'', CAST(1 AS bit), NULL, NULL, N''Admin.ManageSettings''),
    (''6006903b-c5ce-7812-a96e-f7e86730e539'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Tender.ApproveWinner'', CAST(1 AS bit), NULL, NULL, N''Tender.ApproveWinner''),
    (''675c962c-a44e-4aac-54ad-4c3c62f42320'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''PurchaseFile.Edit'', CAST(1 AS bit), NULL, NULL, N''PurchaseFile.Edit''),
    (''6afc6b76-9a9e-e0e4-f489-4bb6ce2c761a'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Report.View'', CAST(1 AS bit), NULL, NULL, N''Report.View''),
    (''6eb9c528-4970-1994-66d7-bd3768b67258'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Indent.SendToPurchase'', CAST(1 AS bit), NULL, NULL, N''Indent.SendToPurchase''),
    (''7424c2ca-7028-df48-f183-9a56ee943fcf'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''PurchaseFile.Archive'', CAST(1 AS bit), NULL, NULL, N''PurchaseFile.Archive''),
    (''796da89d-ac5a-2966-d800-da144f76feca'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Item.Edit'', CAST(1 AS bit), NULL, NULL, N''Item.Edit''),
    (''7bfd8226-7d65-2266-950c-dad7ec66bfa1'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Admin.ManageRoles'', CAST(1 AS bit), NULL, NULL, N''Admin.ManageRoles''),
    (''9c752dc9-c777-9dd1-8b40-5b0f0698690b'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Tender.Create'', CAST(1 AS bit), NULL, NULL, N''Tender.Create''),
    (''ac91044f-8fba-3aac-47e8-1d8c69e80072'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Admin.ManageDepartments'', CAST(1 AS bit), NULL, NULL, N''Admin.ManageDepartments''),
    (''b729bfd5-2478-04ce-366c-3b9581242c9a'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Item.ActivateDeactivate'', CAST(1 AS bit), NULL, NULL, N''Item.ActivateDeactivate''),
    (''c96f84f9-230b-3db2-d200-56eb90580926'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''PurchaseFile.Create'', CAST(1 AS bit), NULL, NULL, N''PurchaseFile.Create''),
    (''cce440c9-4fd8-147a-57f3-3c35c901317c'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Admin.ManageUsers'', CAST(1 AS bit), NULL, NULL, N''Admin.ManageUsers''),
    (''cf9889f5-70fb-5778-1f13-9494b550f041'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Item.Create'', CAST(1 AS bit), NULL, NULL, N''Item.Create''),
    (''cfd51e9b-aaf4-4cac-d168-b68dbf5b06a1'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Indent.Approve'', CAST(1 AS bit), NULL, NULL, N''Indent.Approve''),
    (''db4f9706-7664-8a19-30db-04693d571e1d'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''PurchaseFile.SendToDepartment'', CAST(1 AS bit), NULL, NULL, N''PurchaseFile.SendToDepartment''),
    (''e87e46f6-e214-cddf-93cf-ec8ca9ecbedf'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Indent.View'', CAST(1 AS bit), NULL, NULL, N''Indent.View''),
    (''ec405465-b1d3-7b08-2885-5eb3e545aae4'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''PurchaseFile.Close'', CAST(1 AS bit), NULL, NULL, N''PurchaseFile.Close''),
    (''f1dd1f5a-1a54-0922-6b34-bb8a88f4a5b3'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''AiAgent.Use'', CAST(1 AS bit), NULL, NULL, N''AiAgent.Use''),
    (''f38e8043-d0dc-2620-c7a9-2ce286929560'', ''2026-01-01T00:00:00.0000000Z'', NULL, N''Item.View'', CAST(1 AS bit), NULL, NULL, N''Item.View'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'CreatedBy', N'Description', N'IsActive', N'ModifiedAtUtc', N'ModifiedBy', N'Name') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[identity].[Roles]'))
        SET IDENTITY_INSERT [identity].[Roles] ON;
    EXEC(N'INSERT INTO [identity].[Roles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
    VALUES (''08e0b8c8-0ce3-698c-c760-498a3746bb1f'', N''7b3d912a-5331-cda0-cf79-e778655755e0'', N''WarehouseUser'', N''WAREHOUSEUSER''),
    (''304d970e-93fc-82d5-ea7e-68bd7be5d96f'', N''d3d4afe0-543d-30a2-b539-56e29f143e16'', N''OrdersUser'', N''ORDERSUSER''),
    (''317af55d-dd61-bbd7-6290-1ff6508f3f8a'', N''e909aae3-45be-48e6-f394-9731ac4bae99'', N''OrdersManager'', N''ORDERSMANAGER''),
    (''75694b9e-798f-b74a-f2b7-65adca46674f'', N''a62f2dd1-9536-64dd-996c-a7c7b74abea0'', N''AiAgentUser'', N''AIAGENTUSER''),
    (''7d43902b-c8bb-ce7d-e803-3ee387198dea'', N''11b2221a-5862-fa90-3dfa-91a5c45d4fb5'', N''SystemAdmin'', N''SYSTEMADMIN''),
    (''82cfcfc0-5a42-fba0-38b8-a4aa21c9213c'', N''055fbfef-a4e0-9318-fe6c-fdf55420b055'', N''WarehouseManager'', N''WAREHOUSEMANAGER''),
    (''84dfd3fc-e85b-d990-896c-f99f7933d3e4'', N''4f9d2104-117a-56cd-eb81-5eb1c2457b5d'', N''ReportViewer'', N''REPORTVIEWER''),
    (''92db8232-7861-b5b6-fba1-d5a94cfac12e'', N''04e96555-9a4b-42ff-9ef0-fbb52ac43225'', N''TenderCommissionMember'', N''TENDERCOMMISSIONMEMBER''),
    (''963ff902-7536-ff69-e0a0-cc853740b340'', N''ca28d655-4427-c006-a3f8-ff8b1256c99a'', N''PurchaseExpert'', N''PURCHASEEXPERT''),
    (''a60291f2-2475-430d-a9eb-c6b0b2222f5a'', N''aba91818-c32a-d4ee-1015-8f4a153ecf89'', N''TenderCommissionManager'', N''TENDERCOMMISSIONMANAGER''),
    (''ae1bb199-f970-51c7-ef1b-26eeff76e625'', N''c92b6d1a-cac5-be79-1a0b-58b0cb3ce047'', N''PurchaseManager'', N''PURCHASEMANAGER''),
    (''feb60493-d451-b8d4-d9b4-751e8ea5efd0'', N''0250f789-1985-58fa-bd0a-6b689591cbc0'', N''ApplicantUser'', N''APPLICANTUSER'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[identity].[Roles]'))
        SET IDENTITY_INSERT [identity].[Roles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'CreatedBy', N'ModifiedAtUtc', N'ModifiedBy', N'PermissionId', N'RoleId') AND [object_id] = OBJECT_ID(N'[identity].[RolePermissions]'))
        SET IDENTITY_INSERT [identity].[RolePermissions] ON;
    EXEC(N'INSERT INTO [identity].[RolePermissions] ([Id], [CreatedAtUtc], [CreatedBy], [ModifiedAtUtc], [ModifiedBy], [PermissionId], [RoleId])
    VALUES (''07d91f9b-12ea-28d1-5aec-0a69c20ef8d9'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''6eb9c528-4970-1994-66d7-bd3768b67258'', ''317af55d-dd61-bbd7-6290-1ff6508f3f8a''),
    (''0b705857-c495-5a60-dd05-dc16c979ebaa'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''f38e8043-d0dc-2620-c7a9-2ce286929560'', ''304d970e-93fc-82d5-ea7e-68bd7be5d96f''),
    (''0c5951b5-b283-e84e-b925-54f035eb8670'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''6afc6b76-9a9e-e0e4-f489-4bb6ce2c761a'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''10d1298c-8778-87d1-6446-46d8a86912c7'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''f38e8043-d0dc-2620-c7a9-2ce286929560'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''12cd0f68-0518-6473-7d51-8a7ee520a358'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''cf9889f5-70fb-5778-1f13-9494b550f041'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''149930a7-9d52-f270-70d5-f024bdb318f7'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''f38e8043-d0dc-2620-c7a9-2ce286929560'', ''82cfcfc0-5a42-fba0-38b8-a4aa21c9213c''),
    (''16dadbe0-a522-1c67-e59e-0191db90c1f5'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''59d99406-fa77-512c-96c2-04ec0e579211'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''19691ace-25ca-e584-e543-c5f0e2220dd0'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''9c752dc9-c777-9dd1-8b40-5b0f0698690b'', ''a60291f2-2475-430d-a9eb-c6b0b2222f5a''),
    (''1d07be2f-7534-fb53-6c00-ffdbae1b707c'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1d35913f-cea9-6fa2-2bb6-17009ea75417'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''1f9aac53-b28e-0081-955f-435f70ead604'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''5dd9cf0f-211b-0ead-b79b-f073a991273d'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''26d56920-2568-4631-db61-6d86f8173f70'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''c96f84f9-230b-3db2-d200-56eb90580926'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''274a7740-ca5a-a289-1748-83c0c4d26291'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''c96f84f9-230b-3db2-d200-56eb90580926'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''2c668c1d-cc88-5bb4-0066-5d2bccb396f1'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''7bfd8226-7d65-2266-950c-dad7ec66bfa1'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''2f85d552-e0e0-d47d-36c5-3300bdbc9ae5'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''439721fe-7142-ccc3-e55b-63d639a8f9a5'', ''304d970e-93fc-82d5-ea7e-68bd7be5d96f''),
    (''323c0e0d-3b1c-affc-af2d-d33c37e354a4'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''db4f9706-7664-8a19-30db-04693d571e1d'', ''963ff902-7536-ff69-e0a0-cc853740b340''),
    (''36b89b35-f4de-9016-8483-d23d412f0293'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''3d0bccfd-85cc-bbb7-32ef-74500ed50c77'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''3a3ccc2b-2d67-5345-ab7b-6652bad3fa16'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''f38e8043-d0dc-2620-c7a9-2ce286929560'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''3d333ee0-bc6a-909e-a09b-0a178cc21fc1'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''ac91044f-8fba-3aac-47e8-1d8c69e80072'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''3d5b2d6a-ded3-4d5a-1926-e9d8016928d1'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''4af786fa-8324-fbdd-5e66-8f6308a2874e'', ''a60291f2-2475-430d-a9eb-c6b0b2222f5a''),
    (''4632e241-0cff-b20c-42c6-3b9a24ade606'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''6eb9c528-4970-1994-66d7-bd3768b67258'', ''304d970e-93fc-82d5-ea7e-68bd7be5d96f''),
    (''474dbe3d-a76e-abcb-0adf-158bcf51c1b4'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''12ad9300-f80e-f386-070c-c93f0117b66f'', ''75694b9e-798f-b74a-f2b7-65adca46674f''),
    (''4df8011f-13fa-a5b9-ec44-0b9aec67249f'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''9c752dc9-c777-9dd1-8b40-5b0f0698690b'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''521fe02f-4cab-ff19-76ce-ce73aff922d2'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''3d0bccfd-85cc-bbb7-32ef-74500ed50c77'', ''a60291f2-2475-430d-a9eb-c6b0b2222f5a''),
    (''555973ed-906d-1add-1f9f-a4d55bd4a198'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''59d99406-fa77-512c-96c2-04ec0e579211'', ''84dfd3fc-e85b-d990-896c-f99f7933d3e4''),
    (''5bf2d89e-d4d9-3ed4-bfeb-1b2931b81d20'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''02eecd9e-94ae-ef20-8cbf-bf5922f4ccfd'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''5c88768a-a3e4-4712-9916-a04c89921b35'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1edcbcf3-5ff0-1b70-4cb2-ce687a9b6b5b'', ''75694b9e-798f-b74a-f2b7-65adca46674f''),
    (''64e86873-6e58-d639-bf54-350b962a6da7'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''f38e8043-d0dc-2620-c7a9-2ce286929560'', ''08e0b8c8-0ce3-698c-c760-498a3746bb1f''),
    (''695bd8d0-70a3-a3cb-218d-a6d85ba75e8c'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1edcbcf3-5ff0-1b70-4cb2-ce687a9b6b5b'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''706d703a-107c-49a6-335c-59d30e68d94d'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''7424c2ca-7028-df48-f183-9a56ee943fcf'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''7116ac0b-5022-16f0-5e37-b8c1857ab200'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''ec405465-b1d3-7b08-2885-5eb3e545aae4'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''71beaa7e-c944-6538-95ab-9f805a636860'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''db4f9706-7664-8a19-30db-04693d571e1d'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''729996c5-50a1-2beb-cca7-4720eec7b160'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''e87e46f6-e214-cddf-93cf-ec8ca9ecbedf'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''737ab60c-dd81-c710-9bfc-0f55112a0d9b'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1edcbcf3-5ff0-1b70-4cb2-ce687a9b6b5b'', ''feb60493-d451-b8d4-d9b4-751e8ea5efd0''),
    (''73b01072-e8c1-3405-fffb-7985aa276948'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1d35913f-cea9-6fa2-2bb6-17009ea75417'', ''84dfd3fc-e85b-d990-896c-f99f7933d3e4''),
    (''7413c782-62a1-f66a-6ee8-f9533531d07e'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''e87e46f6-e214-cddf-93cf-ec8ca9ecbedf'', ''304d970e-93fc-82d5-ea7e-68bd7be5d96f''),
    (''74654f7b-ef97-edc0-0a58-74914ea16548'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''4af786fa-8324-fbdd-5e66-8f6308a2874e'', ''92db8232-7861-b5b6-fba1-d5a94cfac12e''),
    (''77d5d332-ec13-75af-7268-2973926911dd'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''b729bfd5-2478-04ce-366c-3b9581242c9a'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''77dc8388-0534-7090-6c0e-499b56313959'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1edcbcf3-5ff0-1b70-4cb2-ce687a9b6b5b'', ''963ff902-7536-ff69-e0a0-cc853740b340''),
    (''7c0f93b8-3398-f384-4878-031ec26da04a'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1ab75a4b-ef24-8a2a-bc82-9ae054003d86'', ''08e0b8c8-0ce3-698c-c760-498a3746bb1f''),
    (''7d4949ce-15dc-ba0a-3d83-60143b0af675'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''f38e8043-d0dc-2620-c7a9-2ce286929560'', ''963ff902-7536-ff69-e0a0-cc853740b340''),
    (''7ecc5277-98d3-60bb-b7c6-17eccd98153b'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''593e28d2-5014-7026-81e0-d0b498c5cc5a'', ''08e0b8c8-0ce3-698c-c760-498a3746bb1f''),
    (''84f44318-20ec-dff5-26d0-896617fdbbf8'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''439721fe-7142-ccc3-e55b-63d639a8f9a5'', ''feb60493-d451-b8d4-d9b4-751e8ea5efd0'');
    INSERT INTO [identity].[RolePermissions] ([Id], [CreatedAtUtc], [CreatedBy], [ModifiedAtUtc], [ModifiedBy], [PermissionId], [RoleId])
    VALUES (''87b2fa08-1315-e18f-bc77-930f9b459894'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''3d0bccfd-85cc-bbb7-32ef-74500ed50c77'', ''92db8232-7861-b5b6-fba1-d5a94cfac12e''),
    (''89fd74d7-3de5-1523-0597-b119b9091ddf'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''439721fe-7142-ccc3-e55b-63d639a8f9a5'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''8ae8cef7-3601-4d7d-a3af-b0e0ddfaad83'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1ab75a4b-ef24-8a2a-bc82-9ae054003d86'', ''82cfcfc0-5a42-fba0-38b8-a4aa21c9213c''),
    (''8d0ff239-e0aa-47b1-9034-8d8d68169b23'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''cce440c9-4fd8-147a-57f3-3c35c901317c'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''8efe51e8-2a5c-22c5-5883-89f7c9a7132b'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''ec405465-b1d3-7b08-2885-5eb3e545aae4'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''94db112f-921e-5eb7-b597-6c083d81c7e9'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''cfd51e9b-aaf4-4cac-d168-b68dbf5b06a1'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''9695f95b-b2b2-ba51-e37b-b114183ac406'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''db4f9706-7664-8a19-30db-04693d571e1d'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''9907e6d0-dfdd-f9a5-bad1-59ef64884d70'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''f38e8043-d0dc-2620-c7a9-2ce286929560'', ''317af55d-dd61-bbd7-6290-1ff6508f3f8a''),
    (''9ba35d4b-6a49-1b71-d43c-68bb376a7429'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''675c962c-a44e-4aac-54ad-4c3c62f42320'', ''963ff902-7536-ff69-e0a0-cc853740b340''),
    (''a53fe41d-624e-8ef3-d81c-f4f051238894'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''e87e46f6-e214-cddf-93cf-ec8ca9ecbedf'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''aa991b55-b815-ceaa-7432-b43eb5c89141'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''e87e46f6-e214-cddf-93cf-ec8ca9ecbedf'', ''317af55d-dd61-bbd7-6290-1ff6508f3f8a''),
    (''ad1fc51a-6d69-e6b5-e4a4-61702635f892'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1d35913f-cea9-6fa2-2bb6-17009ea75417'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''ad35c8cf-f283-8045-cba9-a1cf94864bdb'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''593e28d2-5014-7026-81e0-d0b498c5cc5a'', ''82cfcfc0-5a42-fba0-38b8-a4aa21c9213c''),
    (''aebafa1b-2765-6f6b-9b97-59155a3e10b3'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''796da89d-ac5a-2966-d800-da144f76feca'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''b06bc847-234d-3938-216d-d5c2e64fc97a'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''6006903b-c5ce-7812-a96e-f7e86730e539'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''b27d4110-e0cd-3e83-be7f-e369b88bd082'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1edcbcf3-5ff0-1b70-4cb2-ce687a9b6b5b'', ''92db8232-7861-b5b6-fba1-d5a94cfac12e''),
    (''b4f8a423-b579-d055-156d-bcf6113bb4e1'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''cfd51e9b-aaf4-4cac-d168-b68dbf5b06a1'', ''317af55d-dd61-bbd7-6290-1ff6508f3f8a''),
    (''b5b9cda3-cf57-a0d6-2d37-265d182bdb08'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''675c962c-a44e-4aac-54ad-4c3c62f42320'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''b6bab265-249d-7952-876d-a66bcdeb4af6'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''f1dd1f5a-1a54-0922-6b34-bb8a88f4a5b3'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''bb55f11f-9589-8278-fd8e-3d87bf88a76a'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''f1dd1f5a-1a54-0922-6b34-bb8a88f4a5b3'', ''75694b9e-798f-b74a-f2b7-65adca46674f''),
    (''bb5a97bf-da2c-b300-ae74-60e4a8a9377a'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''f38e8043-d0dc-2620-c7a9-2ce286929560'', ''feb60493-d451-b8d4-d9b4-751e8ea5efd0''),
    (''bdaffd94-7383-32ef-a779-a03abfdbc98f'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''675c962c-a44e-4aac-54ad-4c3c62f42320'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''c3b06465-6522-e459-9d9e-e176eb7127ce'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''e87e46f6-e214-cddf-93cf-ec8ca9ecbedf'', ''feb60493-d451-b8d4-d9b4-751e8ea5efd0''),
    (''c3e15d1f-1a10-1526-bd01-4e9915e6d2b9'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''4af786fa-8324-fbdd-5e66-8f6308a2874e'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''c6e4f088-3989-749e-75a7-e0bb310aa5d3'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''c96f84f9-230b-3db2-d200-56eb90580926'', ''963ff902-7536-ff69-e0a0-cc853740b340''),
    (''c8ef7ae7-3bde-cded-0b4f-fb1849ac5b63'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1edcbcf3-5ff0-1b70-4cb2-ce687a9b6b5b'', ''a60291f2-2475-430d-a9eb-c6b0b2222f5a''),
    (''ccdb4e52-960f-c90e-d3cf-7b381ee1f5a4'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''12ad9300-f80e-f386-070c-c93f0117b66f'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''d2aeccac-59ca-9ba2-d182-21649b67f14a'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''59d99406-fa77-512c-96c2-04ec0e579211'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''d40b52b5-dbdb-fb70-0e54-11c1fafb7b33'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''7424c2ca-7028-df48-f183-9a56ee943fcf'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''e44465ba-85a1-c759-1de4-36f6116685db'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1edcbcf3-5ff0-1b70-4cb2-ce687a9b6b5b'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''eafc0b0a-128a-bb9e-36ee-503b86dc89ec'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''439721fe-7142-ccc3-e55b-63d639a8f9a5'', ''317af55d-dd61-bbd7-6290-1ff6508f3f8a''),
    (''ee75ae35-ed54-e7d7-b9e4-149b8483cbc0'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''6afc6b76-9a9e-e0e4-f489-4bb6ce2c761a'', ''84dfd3fc-e85b-d990-896c-f99f7933d3e4''),
    (''f091826c-8bd6-0371-b40e-3ba110fa234d'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''1ab75a4b-ef24-8a2a-bc82-9ae054003d86'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''f1140413-86a6-93c4-8af2-a6d3999683ff'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''6eb9c528-4970-1994-66d7-bd3768b67258'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''f18cfcb3-6ad7-2ce1-b934-d2311c4bafea'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''6afc6b76-9a9e-e0e4-f489-4bb6ce2c761a'', ''ae1bb199-f970-51c7-ef1b-26eeff76e625''),
    (''f233f3b7-3cf4-7959-e53c-aa8f519b42d5'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''02eecd9e-94ae-ef20-8cbf-bf5922f4ccfd'', ''82cfcfc0-5a42-fba0-38b8-a4aa21c9213c''),
    (''f9eab832-377a-30b2-c357-fb582df0fc6e'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''3d0bccfd-85cc-bbb7-32ef-74500ed50c77'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea''),
    (''fa2d2cd3-07bb-ea8f-f181-f9e3951d5b6a'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''e87e46f6-e214-cddf-93cf-ec8ca9ecbedf'', ''963ff902-7536-ff69-e0a0-cc853740b340''),
    (''fa6f00df-3c78-7d8c-5d25-2196792583f5'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''6006903b-c5ce-7812-a96e-f7e86730e539'', ''a60291f2-2475-430d-a9eb-c6b0b2222f5a''),
    (''fcbcb157-0827-5fcf-970a-364ac4eb502b'', ''2026-01-01T00:00:00.0000000Z'', NULL, NULL, NULL, ''593e28d2-5014-7026-81e0-d0b498c5cc5a'', ''7d43902b-c8bb-ce7d-e803-3ee387198dea'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'CreatedBy', N'ModifiedAtUtc', N'ModifiedBy', N'PermissionId', N'RoleId') AND [object_id] = OBJECT_ID(N'[identity].[RolePermissions]'))
        SET IDENTITY_INSERT [identity].[RolePermissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessFailedCount', N'ConcurrencyStamp', N'Email', N'EmailConfirmed', N'LockoutEnabled', N'LockoutEnd', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'PhoneNumber', N'PhoneNumberConfirmed', N'SecurityStamp', N'TwoFactorEnabled', N'UserName', N'UserProfileId') AND [object_id] = OBJECT_ID(N'[identity].[Users]'))
        SET IDENTITY_INSERT [identity].[Users] ON;
    EXEC(N'INSERT INTO [identity].[Users] ([Id], [AccessFailedCount], [ConcurrencyStamp], [Email], [EmailConfirmed], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [SecurityStamp], [TwoFactorEnabled], [UserName], [UserProfileId])
    VALUES (''946e2251-f534-9be8-7aa7-e7cc5a303ab7'', 0, N''c969acbf-e590-2c52-ffd2-51a1ac68ca21'', N''admin@petroprocure.local'', CAST(1 AS bit), CAST(0 AS bit), NULL, N''ADMIN@PETROPROCURE.LOCAL'', N''ADMIN'', N''AQAAAAIAAYagAAAAEI8eafd4qIeAe6gsTyBHPs5F7Ja7t0D/hFgb1Mja6iQgM0KkLXToH/Gtk2XHTwfmVQ=='', NULL, CAST(0 AS bit), N''8b21e6ae-25bc-c551-a430-fd8cb7180125'', CAST(0 AS bit), N''admin'', ''ea73b3d2-ada1-3014-e807-0287736b56c8'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessFailedCount', N'ConcurrencyStamp', N'Email', N'EmailConfirmed', N'LockoutEnabled', N'LockoutEnd', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'PhoneNumber', N'PhoneNumberConfirmed', N'SecurityStamp', N'TwoFactorEnabled', N'UserName', N'UserProfileId') AND [object_id] = OBJECT_ID(N'[identity].[Users]'))
        SET IDENTITY_INSERT [identity].[Users] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[identity].[UserRoles]'))
        SET IDENTITY_INSERT [identity].[UserRoles] ON;
    EXEC(N'INSERT INTO [identity].[UserRoles] ([RoleId], [UserId])
    VALUES (''7d43902b-c8bb-ce7d-e803-3ee387198dea'', ''946e2251-f534-9be8-7aa7-e7cc5a303ab7'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[identity].[UserRoles]'))
        SET IDENTITY_INSERT [identity].[UserRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE INDEX [IX_DepartmentMenuItems_DepartmentType] ON [org].[DepartmentMenuItems] ([DepartmentType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE INDEX [IX_DepartmentMenuItems_RequiredPermission] ON [org].[DepartmentMenuItems] ([RequiredPermission]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_Name] ON [identity].[Permissions] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE INDEX [IX_RoleClaims_RoleId] ON [identity].[RoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [identity].[RolePermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RolePermissions_RoleId_PermissionId] ON [identity].[RolePermissions] ([RoleId], [PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [identity].[Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE INDEX [IX_UserClaims_UserId] ON [identity].[UserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE INDEX [IX_UserLogins_UserId] ON [identity].[UserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE INDEX [IX_UserRoles_RoleId] ON [identity].[UserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [identity].[Users] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_UserProfileId] ON [identity].[Users] ([UserProfileId]) WHERE [UserProfileId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [identity].[Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622230910_AddIdentityDepartmentsRolesPermissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260622230910_AddIdentityDepartmentsRolesPermissions', N'9.0.0');
END;

COMMIT;
GO


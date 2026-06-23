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

COMMIT;
GO


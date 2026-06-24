# PetroProcure

PetroProcure is a refinery procurement management system built with ASP.NET Aspire, ASP.NET Core Web API, Blazor/MudBlazor, SQL Server, EF Core, DevExpress Reports foundations, and an extendable AI Agent foundation.

The central business aggregate is the Purchase File. Indents, MESC items, documents, workflow tasks, reports, and AI evaluations are connected around it.

## Current modules

- ASP.NET Aspire AppHost for orchestrating API, Web, Worker, and SQL Server resources
- ASP.NET Core Web API with JWT authentication and permission-based authorization
- Persian RTL MudBlazor Web interface
- SQL Server persistence through EF Core migrations
- Identity, roles, permissions, departments, refresh tokens, sessions, lockout, and admin audit log
- MESC catalog with general groups, item validation, search, activation/deactivation, and grouped views
- Indent / Purchase Request with 7-digit numbering, workflow actions, item snapshots, and grouped MESC items
- Purchase File core with file numbering, status history, notes, items, timeline, and creation from approved indents
- Root folder file repository with relative paths, hashing, versioning, soft delete, upload validation, and scanner abstraction
- Workflow and Inbox foundation with department-aware tasks and action matrix
- Reporting foundation for official printable PDF reports
- AI foundation for purchase file summaries, missing document checks, rule evaluation, and future RAG integration
- Admin UI foundation for users, roles, permissions, departments, settings, audit log, and workflow matrix
- Unit, integration, and architecture tests
- GitHub Actions CI preparation

## Solution layout

```text
src/
  PetroProcure.AppHost/          Aspire orchestration
  PetroProcure.ServiceDefaults/  shared Aspire defaults
  PetroProcure.Api/              ASP.NET Core API
  PetroProcure.Web/              Blazor/MudBlazor Persian RTL Web UI
  PetroProcure.Domain/           domain model and business rules
  PetroProcure.Application/      application commands, queries, services, and interfaces
  PetroProcure.Infrastructure/   EF Core, SQL Server, storage, identity, and integrations
  PetroProcure.Contracts/        shared V1 API contracts used by API and Web
  PetroProcure.Reporting/        report generator foundations
  PetroProcure.AI/               AI provider and agent foundations
  PetroProcure.Worker/           background service placeholders

tests/
  PetroProcure.UnitTests/
  PetroProcure.IntegrationTests/
  PetroProcure.ArchitectureTests/

docs/
  architecture, development, deployment, and security documentation
```

## Configuration and secrets

Do not commit real secrets. The committed `appsettings.json` contains only safe defaults. Use:

- `src/PetroProcure.Api/appsettings.example.json` as a template;
- `dotnet user-secrets` for development secrets;
- environment variables or an approved secret manager for production.

Important keys:

```text
ConnectionStrings__PetroProcureDb
Authentication__Jwt__SigningKey
Security__BootstrapAdmin__Password
PetroProcure__AI__OpenAiApiKey
```

See [docs/security/secrets-and-github.md](docs/security/secrets-and-github.md) for the repository security rules.

## Run locally

Restore, build, and test:

```bash
dotnet restore
dotnet build
dotnet test
```

Run with Aspire:

```bash
dotnet run --project src/PetroProcure.AppHost/PetroProcure.AppHost.csproj
```

Aspire supplies secret parameters such as the JWT signing key and bootstrap admin password to the API. For direct API execution, configure those values through user-secrets or environment variables.

## Architecture rules

- `PetroProcure.Domain` must not reference Infrastructure, EF Core, ASP.NET Core, or Web.
- `PetroProcure.Application` must not reference Web or Infrastructure.
- Web must use `PetroProcure.Contracts` V1 DTOs and must not directly reference Infrastructure.
- API can reference Infrastructure.
- Reporting must not directly depend on Web.
- AI must remain replaceable through interfaces.
- The client must not send `CreatedByUserId`, `ActingUserId`, `UploadedByUserId`, or `IsAdmin`; identity comes from authenticated claims.

## Not implemented yet

The following business modules are intentionally not implemented yet:

- Supplier management
- Inquiry
- Tender
- Contract
- Purchase Order
- Warehouse Receipt
- Inventory integration

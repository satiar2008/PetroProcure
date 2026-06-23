# Build and Test

## Prerequisites

- .NET SDK 9
- SQL Server LocalDB on Windows, or a SQL Server connection supplied through `ConnectionStrings__PetroProcureDb`
- Docker Desktop for running the full Aspire application
- DevExpress Reporting 24.2 packages and a valid license for redistribution builds

## Local commands

```powershell
dotnet restore PetroProcure.sln
dotnet build PetroProcure.sln --no-restore
dotnet test PetroProcure.sln --no-build
```

Integration tests create a uniquely named temporary LocalDB database, apply every EF Core migration, run the procurement lifecycle, and delete the database afterward.

## Aspire

```powershell
dotnet run --project src/PetroProcure.AppHost
```

Docker must be healthy because the AppHost provisions SQL Server as a container.

## Database migrations

```powershell
dotnet ef database update `
  --project src/PetroProcure.Infrastructure `
  --startup-project src/PetroProcure.Api
```

Never place production passwords or API keys in committed appsettings files. Use environment variables, user secrets, or the deployment secret store.

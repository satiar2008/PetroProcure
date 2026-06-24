# Secrets and GitHub hygiene

PetroProcure is a public repository. Treat every committed file as public, even if a later commit removes it.

## Never commit secrets

Do not commit:

- SQL Server passwords or production connection strings
- JWT signing keys
- bootstrap admin passwords
- AI provider API keys
- certificate private keys such as `.pfx`, `.pem`, or `.key`
- `.env` files or local secret files

`src/PetroProcure.Api/appsettings.json` must contain only safe defaults. Use `src/PetroProcure.Api/appsettings.example.json` as a template for required keys.

## Development secrets

Use `dotnet user-secrets` for local development values:

```powershell
dotnet user-secrets set "Authentication:Jwt:SigningKey" "<random-value-at-least-32-characters>" --project src/PetroProcure.Api
dotnet user-secrets set "ConnectionStrings:PetroProcureDb" "Server=<server>;Database=PetroProcureDb;User Id=<user>;Password=<password>;Encrypt=True;TrustServerCertificate=False" --project src/PetroProcure.Api
dotnet user-secrets set "Security:BootstrapAdmin:Enabled" "true" --project src/PetroProcure.Api
dotnet user-secrets set "Security:BootstrapAdmin:Password" "<strong-development-password>" --project src/PetroProcure.Api
```

For Aspire, prefer secret parameters in AppHost or user-secrets attached to the AppHost project.

## Production secrets

In production, provide secrets through environment variables, Kubernetes secrets, Azure Key Vault, GitHub Actions secrets, or the organization's approved secret manager.

Environment variable examples:

```text
ConnectionStrings__PetroProcureDb
Authentication__Jwt__SigningKey
Security__BootstrapAdmin__Password
PetroProcure__AI__OpenAiApiKey
```

## SQL connection strings

SQL authentication example:

```text
Server=sql.example;Database=PetroProcureDb;User Id=petroprocure;Password=<secret>;Encrypt=True;TrustServerCertificate=False
```

Windows authentication example:

```text
Server=sql.example;Database=PetroProcureDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=False
```

Do not combine `Trusted_Connection=True` with `User Id` and `Password`.

## If a secret was leaked

Removing a secret from the latest commit is not enough. Immediately:

1. rotate the SQL password, JWT signing key, API key, or certificate;
2. invalidate existing tokens/sessions where applicable;
3. review recent database and API access logs;
4. remove the secret from future commits;
5. consider rewriting Git history only after coordinating with the team.

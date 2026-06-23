# Secrets and configuration

Never commit SQL passwords, JWT signing keys, AI API keys, or bootstrap passwords.

Development user-secrets example:

```powershell
dotnet user-secrets set "Authentication:Jwt:SigningKey" "<at-least-32-random-characters>" --project src/PetroProcure.Api
dotnet user-secrets set "Security:BootstrapAdmin:Enabled" "true" --project src/PetroProcure.Api
dotnet user-secrets set "Security:BootstrapAdmin:Password" "<strong-one-time-password>" --project src/PetroProcure.Api
```

Environment variables use double underscores:

```text
Authentication__Jwt__SigningKey
Security__BootstrapAdmin__Enabled
Security__BootstrapAdmin__Password
ConnectionStrings__PetroProcureDb
PetroProcure__AI__OpenAiApiKey
```

SQL authentication:

```text
Server=sql.example;Database=PetroProcureDb;User Id=petroprocure;Password=<secret>;Encrypt=True;TrustServerCertificate=False
```

Windows authentication:

```text
Server=sql.example;Database=PetroProcureDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=False
```

Do not combine `Trusted_Connection=True` with `User Id` and `Password`.

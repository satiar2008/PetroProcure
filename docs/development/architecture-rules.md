# Architecture Rules

## Dependency direction

```text
Domain <- Application <- Infrastructure <- Api
                      <- Reporting
                      <- AI contracts/providers

Web -> Contracts / typed HTTP clients -> Api
```

- Domain must not reference Infrastructure, EF Core, Web, or Api.
- Application must not reference Infrastructure or Web.
- Web must not directly reference Infrastructure.
- Api is the composition root and may reference Infrastructure.
- Reporting must not reference Web.
- AI providers must be selected through `IAiChatProvider`; business features depend on interfaces.

## Persistence

- EF Core mappings and migrations belong to Infrastructure.
- Cross-aggregate reporting and AI context reads are implemented as Infrastructure providers.
- Sequence generation uses database transactions and unique constraints.

## Validation

Architecture tests inspect both compiled assembly references and project references. A failed dependency rule must be fixed rather than suppressed.

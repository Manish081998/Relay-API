# Project Relay — API

Modular monolith .NET 8 solution for ADTI Corp's Project Relay. Consolidates three
legacy systems (Intranet, Documentum, WebSelect) behind a single ASP.NET Core host
while preserving strict module boundaries so that any module can later be extracted
into a standalone service without rewriting its consumers.

## Solution layout

```
API/
├── ProjectRelay.sln
├── src/
│   ├── Host/
│   │   └── Relay.Api                          # ASP.NET Core composition root
│   ├── BuildingBlocks/
│   │   ├── Relay.SharedKernel                 # Entity, AggregateRoot, Result, ICommand/IQuery
│   │   ├── Relay.CrossCutting                 # Logging, auth, validation, events, cache abstractions
│   │   └── Relay.Infrastructure.Core          # IDbConnectionFactory, IDbExecutor, resilience
│   └── Modules/
│       ├── Intranet/
│       │   ├── Relay.Intranet.Domain
│       │   ├── Relay.Intranet.Application
│       │   ├── Relay.Intranet.Infrastructure
│       │   └── Relay.Intranet.Contracts       # The ONLY project other modules may reference
│       ├── Documentum/
│       │   └── ... (same four layers)
│       └── WebSelect/
│           └── ... (same four layers)
└── tests/
    ├── Unit/                                  # Per-module Domain + Application unit tests
    ├── Integration/                           # Testcontainers-backed end-to-end tests
    └── Architecture/                          # NetArchTest module-boundary enforcement
```

### Module boundary rule

A module may depend on:

1. Its own Domain → Application → Infrastructure layers.
2. The three BuildingBlocks projects.
3. **Only the `.Contracts` project of any other module.**

Architecture tests (`tests/Architecture/Relay.Architecture.Tests`) fail the build if
a module reaches across into another module's Domain, Application, or Infrastructure.

## Technical choices

| Concern          | Decision                                            |
|------------------|-----------------------------------------------------|
| Runtime          | .NET 8 (LTS)                                        |
| Data access      | ADO.NET via `Microsoft.Data.SqlClient` (no ORM)     |
| Command / Query  | Custom CQRS (`ICommand`, `IQuery`, `Result<T>`)     |
| Validation       | FluentValidation + `ValidationPipeline<TRequest>`   |
| Logging          | Serilog (console + rolling file)                    |
| Resilience       | Polly (HTTP retries, circuit breaker)               |
| Testing          | xUnit + FluentAssertions + NSubstitute              |
| Integration      | Testcontainers.MsSql                                |
| Architecture     | NetArchTest.Rules                                   |

Database uses a **schema per module** (`intranet.*`, `documentum.*`, `webselect.*`)
so the modules can later migrate to separate databases without changing SQL.

## Local development

### Prerequisites

- .NET 8.0 SDK (`global.json` pins `8.0.100`)
- SQL Server (LocalDB, Docker, or a shared dev instance)
- Docker (optional, for Testcontainers-based integration tests)

### First run

```bash
dotnet restore
dotnet build
dotnet run --project src/Host/Relay.Api
```

Navigate to `https://localhost:7057/swagger` for the API surface.

### Tests

```bash
dotnet test                                    # everything
dotnet test tests/Unit                         # unit suites
dotnet test tests/Architecture                 # boundary enforcement (fast)
dotnet test tests/Integration                  # requires Docker for Testcontainers
```

## Configuration

Connection strings are injected per module so each can point at a different server:

```json
"ConnectionStrings": {
  "Intranet":    "Server=...;Database=ProjectRelay;...",
  "Documentum":  "Server=...;Database=ProjectRelay;...",
  "WebSelect":   "Server=...;Database=ProjectRelay;..."
}
```

`SqlServerConnectionFactory` reads `ConnectionStrings:{ModuleName}` where
`{ModuleName}` is provided by each module's `*InfrastructureModule`.

## Migrations

Raw `.sql` migration scripts live under each module's
`Infrastructure/Migrations/` folder. They are idempotent (`IF NOT EXISTS` checks)
and applied in lexical order. Wire them into your deployment tool of choice
(DbUp, Flyway, sqlcmd in CI) — the runtime host does not apply schema changes.

## Adding a module

1. Create four projects under `src/Modules/{NewModule}/`: `Domain`, `Application`,
   `Infrastructure`, `Contracts`.
2. Implement `*ApplicationModule.Add{NewModule}Application` and
   `*InfrastructureModule.Add{NewModule}Infrastructure` DI extensions.
3. Register them in `Host/Relay.Api/Extensions/ModuleRegistrationExtensions.cs`.
4. Add the projects to `ProjectRelay.sln` and the architecture test project.
5. Add migration SQL under `Infrastructure/Migrations/`.

## Documentation

- `docs/adr/` — architecture decision records
- `docs/diagrams/` — context, container, and sequence diagrams

## Project status

This repository contains the initial scaffold produced per the Project Relay
architecture spec: 26 projects, full DDD scaffolding for three modules, CQRS
pipeline, validation, events, correlation, integration & architecture tests.
Legacy data migrations, connector APIs, and the Angular front end are tracked
separately.

# ADR 0002 — ADO.NET with a central DbExecutor (no ORM)

- Status: Accepted
- Date: 2026-04-20
- Deciders: Relay architecture guild
- Related: ADR 0001

## Context

Relay's three source systems today run hand-written stored procedures and raw
SQL. The operations team knows exactly where to look when a query goes slow.
Introducing an ORM would:

- Re-open long-settled debates about model shape and lazy loading.
- Obscure SQL that DBAs are already tuning.
- Couple Domain aggregates to ORM attributes — the opposite of what the
  modular-monolith design wants.

At the same time, scattering `SqlConnection` / `SqlCommand` across every
repository would be hard to instrument for logging, resilience, and
transactions.

## Decision

- Data access uses `Microsoft.Data.SqlClient` directly — no EF Core, no
  Dapper, no custom ORM.
- A single abstraction, `IDbExecutor`, exposes the operations repositories
  need: `ExecuteAsync`, `QueryAsync<T>`, `QuerySingleOrDefaultAsync<T>`,
  `ExecuteScalarAsync`, and `BeginTransactionAsync`. The concrete
  `DbExecutor` handles connection lifetime, parameter binding, logging, and
  resilience in one place.
- Connections are opened through `IDbConnectionFactory` keyed by module name
  (`Intranet`, `Documentum`, `WebSelect`) so each module can use a distinct
  connection string.
- Aggregates are rehydrated by mapping functions on per-module `*DataModel`
  classes; Domain types have no knowledge of persistence.

## Consequences

**Positive**

- SQL is visible, reviewable, and profileable. DBAs can grep for queries.
- Domain aggregates stay free of persistence concerns.
- Instrumentation (timing, Polly retries, correlation IDs) is added once,
  inside `DbExecutor`, and inherited by every repository.

**Negative**

- More boilerplate than an ORM for simple CRUD.
- Engineers must understand parameterization and SqlBulkCopy patterns.
- Manual migration management — we are wiring DbUp / sqlcmd separately.

## Alternatives considered

- **EF Core** — rejected: hides SQL, encourages navigation-property coupling,
  and clashes with the no-shared-model rule across modules.
- **Dapper** — considered. The marginal gain over a thin `DbExecutor` did
  not justify an extra dependency, and we wanted uniform logging and
  retry semantics.

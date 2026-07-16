# ADR 0001 — Modular monolith as the Relay target architecture

- Status: Accepted
- Date: 2026-04-20
- Deciders: Relay architecture guild

## Context

ADTI Corp is consolidating three legacy systems (Intranet, Documentum, WebSelect)
into Project Relay. The systems today share users and content but run as
independent apps with divergent data models and deployment cadences. The
business goal is a single, coherent product that the operations team can
maintain as one unit.

We evaluated three options:

1. **Single shared-schema monolith.** Fast to build, but the three teams have
   historically stepped on each other's data. Consolidating their tables into
   one schema would re-create the coupling we are trying to remove.
2. **Microservices up front.** Adds operational overhead (service discovery,
   deployment topology, distributed tracing, saga coordination) that does not
   match the current team size (8 engineers) or on-call capability.
3. **Modular monolith with strict internal boundaries.** One deployable, but
   module code is physically isolated and enforced by build-time architecture
   tests. Any module can be lifted into its own service later by swapping its
   in-process event bus for a broker and its `.Contracts` interface
   implementations for HTTP clients.

## Decision

We are adopting option 3. Each module lives under `src/Modules/{Module}/`
with four projects: `Domain`, `Application`, `Infrastructure`, and
`Contracts`. Other modules may reference **only** `Contracts`.

The single ASP.NET Core host (`Relay.Api`) composes every module through
DI extensions. Cross-module integration events flow through an in-process
`IEventBus` (`InProcessEventBus`) today; the interface is designed to be
replaced by a broker-backed implementation without touching publishers or
handlers.

SQL Server uses a **schema per module** (`intranet.*`, `documentum.*`,
`webselect.*`). Each module's connection string is resolved by name, which
means a module can move to its own database — or its own server — by editing
configuration only.

## Consequences

**Positive**

- One artifact to build, deploy, and monitor for the initial release.
- Architecture tests (`tests/Architecture`) fail the CI build if a module
  reaches across a boundary, catching coupling regressions at review time.
- Future extraction is a config/infra change, not a code rewrite: the module
  already owns its schema, its data access code, its DTOs, and its events.

**Negative**

- Developers must be disciplined about using `.Contracts`. Architecture tests
  mitigate this but do not eliminate the learning curve.
- A misbehaving module (e.g. memory leak) can still affect the whole host.
  Process-level isolation requires extraction.
- In-process events lose message durability; any workflow that must survive a
  host crash needs an outbox before we can safely treat it as fire-and-forget.

## Alternatives considered

- **Single-schema monolith** — rejected: reintroduces the coupling we are
  migrating away from.
- **Microservices from day one** — rejected: the team does not yet run the
  platform maturity (observability, deployment automation, data replication)
  needed for production microservices.
- **Module federation at the UI** — orthogonal to the API; decided separately.

## Follow-ups

- ADR 0002: ADO.NET with central `DbExecutor` (no ORM).
- ADR 0003: In-process event bus and migration path to a broker.
- ADR 0004: Per-module schema ownership & migration strategy.

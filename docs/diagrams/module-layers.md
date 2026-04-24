# Module layer dependency rules

```mermaid
flowchart TB
    classDef layer fill:#eef,stroke:#335,stroke-width:1px;
    classDef host fill:#efe,stroke:#353,stroke-width:1px;

    subgraph Host
        api["Relay.Api"]:::host
    end

    subgraph ModuleA["Module (e.g. Intranet)"]
        direction TB
        domA["Domain"]:::layer
        appA["Application"]:::layer
        infA["Infrastructure"]:::layer
        conA["Contracts"]:::layer

        appA --> domA
        infA --> domA
        infA --> appA
    end

    subgraph ModuleB["Other module (e.g. Documentum)"]
        direction TB
        appB["Application"]:::layer
        conB["Contracts"]:::layer
    end

    api --> appA
    api --> infA
    api --> appB

    appB -.->|"ONLY via Contracts"| conA
```

## Rules enforced by `tests/Architecture`

1. `*.Domain` does not reference `AspNetCore`, `SqlClient`, or
   `Infrastructure.Core`.
2. `*.Domain` does not reference any other module's projects.
3. `*.Application` of module X may reference module Y **only** through
   `Y.Contracts`.
4. `*.Infrastructure` of module X does not reference `Infrastructure` of
   any other module.
5. Handler, validator, and repository types follow their naming conventions
   (`*CommandHandler`, `*QueryHandler`, `*Validator`, `*Repository`).

A violation of any of these rules fails the CI build, not just a review
comment.

# Project Relay API — Claude Code Guide

## Architecture Overview

This is a **.NET 8 modular monolith** using **CQRS** with a custom dispatcher pattern (no MediatR, no ORM).

### Three Modules

| Module | Controller | Aggregate | Existing APIs |
|--------|-----------|-----------|---------------|
| Documentum | `DocumentsController` | `Document` | GetById, GetByName, UpdateById |
| Intranet | `UsersController` | `User` | GetById, UpdateByEmail |
| WebSelect | `SelectionsController` | `Selection` | GetById |

### Key Rules

- **Queries** (read-only) → implement `IQuery<TResponse>` + `IQueryHandler<TQuery, TResponse>`
- **Commands** (write) → implement `ICommand<TResponse>` or `ICommand` + matching handler
- **No exceptions for business errors** — return `Result.Failure(AppError)` instead
- **No ORM** — write raw SQL in the module's `SqlQueries` class
- **Dispatchers** resolve handlers from DI at runtime — controllers only inject `IQueryDispatcher` and `ICommandDispatcher`
- **Request records** (HTTP body shape) live in `Relay.Api/Requests/{Module}/`
- **Response DTOs** live in `Relay.{Module}.Contracts/Dtos/`
- **Never alter existing code** — when adding a new endpoint, only create new files or append new members (new methods, new SQL constants, new interface members, new controller actions). Never modify the body or signature of any existing handler, repository method, SQL constant, or controller action. If touching existing code seems necessary, stop and ask the user before making any change.

---

## Dispatcher Decision Guide

**Rule:** The HTTP method determines which dispatcher and which CQRS path to use. There are no exceptions to this mapping.

| Operation | HTTP Method | Dispatcher | Controller field | Call pattern |
|-----------|-------------|------------|-----------------|--------------|
| Read (single or list) | `GET` | `IQueryDispatcher` | `_queries` | `await _queries.SendAsync<GetXxxQuery, XxxDto>(new GetXxxQuery(...))` |
| Create | `POST` | `ICommandDispatcher` | `_commands` | `await _commands.SendAsync(new CreateXxxCommand(...))` |
| Full replace | `PUT` | `ICommandDispatcher` | `_commands` | `await _commands.SendAsync(new UpdateXxxCommand(...))` |
| Partial update | `PATCH` | `ICommandDispatcher` | `_commands` | `await _commands.SendAsync(new PatchXxxCommand(...))` |
| Delete | `DELETE` | `ICommandDispatcher` | `_commands` | `await _commands.SendAsync(new DeleteXxxCommand(...))` |

**Quick memory aid:**
- `GET` → **Query** → `_queries` → lives in `Application/Queries/`
- `POST / PUT / PATCH / DELETE` → **Command** → `_commands` → lives in `Application/Commands/`

### Command Dispatcher — Two Overloads

`ICommandDispatcher` has two methods. Choose based on whether the endpoint returns a response body:

| Scenario | Command interface | Handler interface | Dispatcher returns | Controller action |
|----------|-----------------|-----------------|-------------------|-------------------|
| **No response body** (DELETE, fire-and-forget) | `ICommand` | `ICommandHandler<TCommand>` | `Task<Result>` | `NoContent()` or `Ok()` |
| **With response body** (POST returns created resource, PUT/PATCH returns updated resource) | `ICommand<TResponse>` | `ICommandHandler<TCommand, TResponse>` | `Task<Result<TResponse>>` | `Ok(result.Value)` or `CreatedAtAction(...)` |

```csharp
// No response body → use ICommand overload
await _commands.SendAsync(new DeleteXxxCommand(id), cancellationToken);  // returns Task<Result>

// With response body → use ICommand<TResponse> overload
await _commands.SendAsync(new CreateXxxCommand(...), cancellationToken);  // returns Task<Result<XxxDto>>
```

**Expected status codes per operation:**

| HTTP Method | Success | Not Found | Bad Input |
|-------------|---------|-----------|-----------|
| `GET` | `200 OK` | `404 Not Found` | — |
| `POST` | `201 Created` | — | `400 Bad Request` |
| `PUT` | `200 OK` | `404 Not Found` | `400 Bad Request` |
| `PATCH` | `200 OK` | `404 Not Found` | `400 Bad Request` |
| `DELETE` | `204 No Content` | `404 Not Found` | — |

---

## Adding a New API Endpoint

**Before writing any code, run the pre-flight controller check below, then ask the user questions ONE AT A TIME. For each question, validate the answer. If invalid, explain why and repeat the question. Only if valid, proceed to the next question.**

---

### Pre-flight: Controller Location Check

**Trigger:** The user's request mentions a specific controller by name (e.g., _"create an API in UserController"_).

**Steps:**

1. Check whether a controller with that name exists inside any module folder:
   `src/Host/Relay.Api/Controllers/{Documentum|Intranet|WebSelect}/{ControllerName}.cs`

2. Check whether a controller with the same name exists **outside** any module folder (directly under `src/Host/Relay.Api/Controllers/`).

3. Apply the decision table:

| Found inside a module? | Found outside a module? | Action |
|------------------------|------------------------|--------|
| Yes | — | Proceed normally — controller is in the correct location. Continue to Question 1. |
| No | Yes | **Warn the user** (see warning script below), then wait for confirmation before continuing. |
| No | No | Treat as a new controller. Continue to Question 1 and Question 2 (Option B). |

**Warning script (use when controller is only found outside a module folder):**

> _"I found `{ControllerName}` at `src/Host/Relay.Api/Controllers/{ControllerName}.cs`, which is **not inside any module folder**. The standard location for controllers in this project is `src/Host/Relay.Api/Controllers/{Module}/{ControllerName}.cs`._
>
> _Are you sure you want to add the new endpoint to this controller at its current (non-module) location? (Yes / No)"_

- **If the user answers No:** Resume from Question 1 and let them choose a module and controller normally.
- **If the user answers Yes:** Skip Question 1 and Question 2 (module and controller are already decided). Proceed directly to Question 3, using the confirmed controller as the target. When writing the controller endpoint in the final step, append it to the existing file at `src/Host/Relay.Api/Controllers/{ControllerName}.cs`.

---

> **Question 1: Which module should this API belong to?**
>    - Valid answers: `Documentum`, `Intranet`, or `WebSelect`
>    - _(New modules cannot be created in this API. If the user requests a new module, inform them it is not supported and ask them to choose one of the three existing modules.)_
>    - **Validation:** The answer MUST be exactly one of: `Documentum`, `Intranet`, `WebSelect`
>    - **If INVALID:** Respond — _"That module is not recognized. The available modules are: `Documentum`, `Intranet`, `WebSelect`. Please choose one of these."_ Then repeat Question 1.
>    - **If VALID:** Proceed to Question 2.
>
> **Question 2: Within that module, do you want to:**
>    - Option A: **Add to an existing controller** inside the chosen module?
>    - Option B: **Create a new controller** inside the chosen module?
>
>    _(A new controller does NOT automatically mean a new module. A module can have multiple controllers.)_
>
>    - **Validation:** Answer must be either "Add to existing controller" or "Create a new controller"
>    - **If INVALID:** Respond — _"Please choose either 'Add to existing controller' or 'Create a new controller'."_ Then repeat Question 2.
>    - **If VALID (Option A — existing controller):** Ask — _"What is the name of the existing controller?"_ Then check `src/Host/Relay.Api/Controllers/{Module}/` to confirm it exists.
>       - **If controller NOT found:** Respond — _"I could not find a controller with that name under the `{Module}` module. Please double-check the controller name — the controllers available in that module are: [list them]. Did you mean one of these?"_ Then repeat the controller name request.
>       - **If controller IS found:** Proceed to Question 3.
>    - **If VALID (Option B — new controller):** Ask — _"What should the new controller be named? (e.g., `ReportsController`)"_ Then proceed to Question 3.
>
> **Question 3: What HTTP method will this endpoint use?**
>    - Valid answers: `GET`, `POST`, `PUT`, `PATCH`, or `DELETE`
>    - `GET` → **Query** → use `IQueryDispatcher` (`_queries.SendAsync`)
>    - `POST` → **Command** (create) → use `ICommandDispatcher` (`_commands.SendAsync`)
>    - `PUT` → **Command** (full update) → use `ICommandDispatcher` (`_commands.SendAsync`)
>    - `PATCH` → **Command** (partial update) → use `ICommandDispatcher` (`_commands.SendAsync`)
>    - `DELETE` → **Command** (delete) → use `ICommandDispatcher` (`_commands.SendAsync`)
>    - **Validation:** Answer MUST be exactly one of: `GET`, `POST`, `PUT`, `PATCH`, `DELETE`
>    - **If INVALID:** Respond — _"That HTTP method is not recognized. Please choose one of: `GET`, `POST`, `PUT`, `PATCH`, `DELETE`."_ Then repeat Question 3.
>    - **If VALID:** Proceed to Question 4.
>
> **Question 4: Will this endpoint accept a request body?**
>
>    _Ask for every HTTP method. Apply the rules below:_
>
>    - **`GET`** — parameters travel via route or query string, not a JSON body. **Skip this question and proceed directly to Question 5.**
>    - **`POST / PUT / PATCH`** — ask: _"Will this endpoint accept a JSON request body? (Yes / No)"_
>      - **If Yes:** Ask — _"What fields should the request body contain? List each field as `FieldName: CSharpType` (e.g., `Name: string, Status: string, StartDate: DateTime, IsActive: bool`)."_ Record the field list — these become the properties of the `{Verb}{Entity}Request.cs` record.
>      - **If No:** Parameters come from the route or query string only. No request record will be created.
>    - **`DELETE`** — DELETE endpoints typically use only a route parameter (e.g., `{id:guid}`). Ask: _"Does this DELETE endpoint require a JSON request body? (Yes / No)"_
>      - **If Yes:** Ask — _"What fields should the request body contain? List each field as `FieldName: CSharpType`."_ Record the field list.
>      - **If No:** No request record needed.
>    - **Validation:** Answer MUST be either `Yes` or `No`.
>    - **If INVALID:** Respond — _"Please answer either 'Yes' or 'No'."_ Then repeat Question 4.
>    - **If VALID:** Proceed to Question 5.
>
> **Question 5: Will this endpoint return a response body?**
>
>    _Ask for every HTTP method. Apply the rules below:_
>
>    - **`GET`** — a GET endpoint always returns data. Ask — _"What fields should the response contain? List each field as `FieldName: CSharpType` (e.g., `Id: Guid, Name: string, Status: string, CreatedAt: DateTime`)."_ Record the field list — these become the properties of the `XxxDto.cs` record. Then proceed to implementation.
>    - **`POST / PUT / PATCH / DELETE`** — ask: _"Will this endpoint return a response body? (Yes / No)"_
>      - **If Yes:** Ask — _"What fields should the response body contain? List each field as `FieldName: CSharpType` (e.g., `Id: Guid, Name: string, Status: string`)."_ Record the field list — these become the properties of `XxxDto.cs`. Command implements `ICommand<XxxDto>` → handler returns `Task<Result<XxxDto>>` → controller returns `Ok(result.Value)` or `CreatedAtAction(...)`.
>      - **If No:** No DTO needed. Command implements `ICommand` (no generic) → handler returns `Task<Result>` → controller returns `NoContent()` or `Ok()`.
>    - **Validation:** Answer MUST be either `Yes` or `No` (for `POST / PUT / PATCH / DELETE`).
>    - **If INVALID:** Respond — _"Please answer either 'Yes' or 'No'."_ Then repeat Question 5.
>    - **If VALID:** All information is gathered. Proceed with implementation.

---

## Option 1 — Add to Existing Controller

### 1A. New QUERY (read data, GET endpoint)

Replace `{Module}` with `Documentum`, `Intranet`, or `WebSelect`.
Replace `GetXxx` with your query name (e.g. `GetDocumentByStatus`).
Replace `XxxDto` with the response type.

**Files to create:**

```
src/Modules/{Module}/Relay.{Module}.Application/Queries/GetXxx/
    GetXxxQuery.cs
    GetXxxQueryHandler.cs
```

**Step 1 — Query record**
```csharp
// Queries/GetXxx/GetXxxQuery.cs
namespace Relay.{Module}.Application.Queries.GetXxx;

public sealed record GetXxxQuery(/* parameters */) : IQuery<XxxDto>;
```

**Step 2 — Handler**
```csharp
// Queries/GetXxx/GetXxxQueryHandler.cs
namespace Relay.{Module}.Application.Queries.GetXxx;

internal sealed class GetXxxQueryHandler : IQueryHandler<GetXxxQuery, XxxDto>
{
    private readonly I{Module}Repository _{repo};

    public GetXxxQueryHandler(I{Module}Repository {repo})
    {
        _{repo} = {repo} ?? throw new ArgumentNullException(nameof({repo}));
    }

    public async Task<Result<XxxDto>> HandleAsync(GetXxxQuery query, CancellationToken cancellationToken = default)
    {
        // 1. Validate inputs if needed
        // 2. Call repository
        // 3. Map to Dto and return Result.Success(dto)
    }
}
```

**Step 3 — SQL constant** (if new query needed)
```csharp
// Infrastructure/Persistence/SqlQueries/{Module}Queries.cs
public const string GetXxx = """
    SELECT ...
    FROM {schema}.{Table}
    WHERE ...
    """;
```

**Step 4 — Repository** (if new method needed)

Add to `Domain/Repositories/I{Module}Repository.cs`:
```csharp
Task<XxxType?> GetXxxAsync(/* params */, CancellationToken cancellationToken = default);
```

Add implementation to `Infrastructure/Persistence/Repositories/{Module}Repository.cs`:
```csharp
public async Task<XxxType?> GetXxxAsync(/* params */, CancellationToken cancellationToken = default)
{
    var data = await _db.QuerySingleOrDefaultAsync(
        Module, {Module}Queries.GetXxx, {Module}DataModel.FromRecord,
        new { /* params */ }, cancellationToken: cancellationToken);
    return data?.ToAggregate();
}
```

**Step 5 — DTO** (if new response shape needed)
```csharp
// Contracts/Dtos/XxxDto.cs
namespace Relay.{Module}.Contracts.Dtos;

public sealed record XxxDto(/* properties */);
```

**Step 6 — Controller endpoint**

Add to `src/Host/Relay.Api/Controllers/{Module}/{Module}Controller.cs`:
```csharp
[HttpGet("route")]
[ProducesResponseType(typeof(XxxDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetXxx([FromQuery] string param, CancellationToken cancellationToken = default)
{
    // Both type args required: <QueryType, ResponseType> — enables F12 navigation to the handler
    var result = await _queries.SendAsync<GetXxxQuery, XxxDto>(new GetXxxQuery(param), cancellationToken);
    return result.IsSuccess && result.Value is not null
        ? Ok(result.Value)
        : NotFound();
}
```

---

### 1B. New COMMAND (write data, POST/PUT/PATCH/DELETE endpoint)

First determine which of the two paths applies by answering Q4 from the pre-flight checklist:

---

#### Path 1B-i — Command **with** response body (`ICommand<TResponse>`)

_Use when: POST that returns the created resource, PUT/PATCH that returns the updated resource._

**Files to create:**
```
src/Modules/{Module}/Relay.{Module}.Application/Commands/DoXxx/
    DoXxxCommand.cs
    DoXxxCommandHandler.cs

src/Modules/{Module}/Relay.{Module}.Contracts/Dtos/
    XxxDto.cs                ← if new response shape (ask user for field names + types)

src/Host/Relay.Api/Requests/{Module}/
    DoXxxRequest.cs          ← if endpoint has a JSON body (ask user for field names + types)
```

**Step 1 — DTO** _(ask user: "What fields should the response contain? List name and type for each.")_
```csharp
// Contracts/Dtos/XxxDto.cs
namespace Relay.{Module}.Contracts.Dtos;

public sealed record XxxDto(
    /* properties provided by user, e.g.: */
    Guid Id,
    string Name,
    string Status);
```

**Step 2 — Request record** _(only if endpoint has a JSON body — ask user: "What fields does the request body contain?")_
```csharp
// Relay.Api/Requests/{Module}/DoXxxRequest.cs
namespace Relay.Api.Requests.{Module};

public sealed record DoXxxRequest(
    /* properties provided by user, e.g.: */
    string Name,
    string Status);
```

**Step 3 — Command record**
```csharp
// Commands/DoXxx/DoXxxCommand.cs
namespace Relay.{Module}.Application.Commands.DoXxx;

public sealed record DoXxxCommand(/* route params + mapped from request */) : ICommand<XxxDto>;
```

**Step 4 — Handler**
```csharp
// Commands/DoXxx/DoXxxCommandHandler.cs
namespace Relay.{Module}.Application.Commands.DoXxx;

internal sealed class DoXxxCommandHandler : ICommandHandler<DoXxxCommand, XxxDto>
{
    private readonly I{Module}Repository _{repo};

    public DoXxxCommandHandler(I{Module}Repository {repo})
    {
        _{repo} = {repo} ?? throw new ArgumentNullException(nameof({repo}));
    }

    public async Task<Result<XxxDto>> HandleAsync(DoXxxCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Load aggregate (if updating existing)
        var entity = await _{repo}.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
            return Result.Failure<XxxDto>(new AppError("{Module}.NotFound", "...not found."));

        // 2. Call domain method
        // 3. Persist changes
        await _{repo}.UpdateAsync(entity, cancellationToken);
        // 4. Map to DTO and return
        return Result.Success(new XxxDto(entity.Id, entity.Name, entity.Status));
    }
}
```

**Step 5 — SQL constant** (if new SQL needed)
```csharp
public const string DoXxx = """
    UPDATE/INSERT ...
    """;
```

**Step 6 — Repository method** (if new method needed) — same pattern as 1A Step 4.

**Step 7 — Controller endpoint**
```csharp
using Relay.Api.Requests.{Module};

[HttpPut("{id:guid}")]
[ProducesResponseType(typeof(XxxDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> DoXxx(Guid id, [FromBody] DoXxxRequest request, CancellationToken cancellationToken = default)
{
    var result = await _commands.SendAsync(
        new DoXxxCommand(id, request.Name, request.Status), cancellationToken);

    if (!result.IsSuccess)
    {
        return result.Error.Code == "{Module}.NotFound"
            ? NotFound(result.Error.Description)
            : BadRequest(result.Error.Description);
    }

    return Ok(result.Value);
}
```

---

#### Path 1B-ii — Command **without** response body (`ICommand`)

_Use when: DELETE, or any operation where the caller only needs success/failure confirmation._

**Files to create:**
```
src/Modules/{Module}/Relay.{Module}.Application/Commands/DoXxx/
    DoXxxCommand.cs
    DoXxxCommandHandler.cs
```
_(No DTO, no Request record needed unless the endpoint has a body.)_

**Step 1 — Command record**
```csharp
// Commands/DoXxx/DoXxxCommand.cs
namespace Relay.{Module}.Application.Commands.DoXxx;

public sealed record DoXxxCommand(/* route params */) : ICommand;
```

**Step 2 — Handler**
```csharp
// Commands/DoXxx/DoXxxCommandHandler.cs
namespace Relay.{Module}.Application.Commands.DoXxx;

internal sealed class DoXxxCommandHandler : ICommandHandler<DoXxxCommand>
{
    private readonly I{Module}Repository _{repo};

    public DoXxxCommandHandler(I{Module}Repository {repo})
    {
        _{repo} = {repo} ?? throw new ArgumentNullException(nameof({repo}));
    }

    public async Task<Result> HandleAsync(DoXxxCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Load aggregate
        var entity = await _{repo}.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(new AppError("{Module}.NotFound", "...not found."));

        // 2. Perform operation (delete/archive/etc.)
        await _{repo}.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }
}
```

**Step 3 — SQL constant** (if new SQL needed)
```csharp
public const string DoXxx = """
    DELETE FROM {schema}.{Table} WHERE Id = @Id
    """;
```

**Step 4 — Repository method** (if new method needed) — same pattern as 1A Step 4.

**Step 5 — Controller endpoint**
```csharp
[HttpDelete("{id:guid}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> DoXxx(Guid id, CancellationToken cancellationToken = default)
{
    var result = await _commands.SendAsync(new DoXxxCommand(id), cancellationToken);

    if (!result.IsSuccess)
    {
        return result.Error.Code == "{Module}.NotFound"
            ? NotFound(result.Error.Description)
            : BadRequest(result.Error.Description);
    }

    return NoContent();
}
```

---

## Option 2 — Create a New Controller inside an Existing Module

_Use when the chosen module needs a new controller for a different resource/aggregate._

Ask the user:
> What should the new controller be named? (e.g. `ReportsController` inside the `Documentum` module)

Steps:
1. Create `src/Host/Relay.Api/Controllers/{Module}/{NewEntity}Controller.cs` following the same pattern as the existing controller in that module (inject `IQueryDispatcher` and/or `ICommandDispatcher`).
2. Add any required Queries/Commands/Repository methods inside the existing module's projects — follow Option 1A / 1B steps as needed.

> **Note:** Creating a new module is not supported in this API. All new controllers must live inside one of the three existing modules: `Documentum`, `Intranet`, or `WebSelect`.

---

## Post-Generation: Build Verification

**After all files for a new endpoint have been written**, always run a solution build to confirm nothing is broken:

```bash
dotnet build
```

- If the build **succeeds** — report success and list the new files created.
- If the build **fails** — show the compiler errors, fix them, and re-run the build. Do not report the task as complete until the build is green.
- Do **not** skip this step, even for small changes.

---

## Post-Generation: Test Case Creation

**This step runs immediately after a successful build.**

> **Ask the user:**
> _"The endpoint has been created and the build is green. Do you want me to generate test cases for this endpoint? (Yes / No)"_
>
> - **If No:** The task is complete.
> - **If Yes:** Generate the test cases by strictly following **`RELAY_TEST_CASE_DEVELOPMENT_GUIDE.md`**. That file defines all templates, file placement, naming conventions, and build verification steps for this codebase.

---

## File Naming Conventions

| What | Convention | Example |
|------|-----------|---------|
| Query | `Get{Subject}By{Filter}Query` | `GetDocumentByNameQuery` |
| Command | `{Verb}{Subject}By{Filter}Command` | `UpdateDocumentByIdCommand` |
| Handler | same name + `Handler` suffix | `GetDocumentByNameQueryHandler` |
| Request record | `{Verb}{Subject}Request` | `UpdateDocumentRequest` |
| DTO | `{Subject}Dto` | `DocumentDto` |
| SQL constant | `{Verb}{Subject}` | `GetByName`, `Update` |

---

## Quick Reference — Existing File Locations

| File type | Path pattern |
|-----------|-------------|
| Aggregate | `src/Modules/{M}/Relay.{M}.Domain/Aggregates/{Entity}.cs` |
| Repository interface | `src/Modules/{M}/Relay.{M}.Domain/Repositories/I{Entity}Repository.cs` |
| Repository impl | `src/Modules/{M}/Relay.{M}.Infrastructure/Persistence/Repositories/{Entity}Repository.cs` |
| SQL queries | `src/Modules/{M}/Relay.{M}.Infrastructure/Persistence/SqlQueries/{Entity}Queries.cs` |
| Data model | `src/Modules/{M}/Relay.{M}.Infrastructure/Persistence/DataModels/{Entity}DataModel.cs` |
| Query handler | `src/Modules/{M}/Relay.{M}.Application/Queries/{QueryName}/{QueryName}Handler.cs` |
| Command handler | `src/Modules/{M}/Relay.{M}.Application/Commands/{CommandName}/{CommandName}Handler.cs` |
| Response DTO | `src/Modules/{M}/Relay.{M}.Contracts/Dtos/{Entity}Dto.cs` |
| Request record | `src/Host/Relay.Api/Requests/{M}/{Verb}{Entity}Request.cs` |
| Controller | `src/Host/Relay.Api/Controllers/{M}/{Entity}sController.cs` |
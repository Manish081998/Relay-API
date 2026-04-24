# Project Relay — Test Generation Guide

This document is the authoritative reference for writing tests in this repository.
Read it before writing any new test file. All examples are drawn from the actual test
structure in this codebase.

---

## Table of Contents

1. [Test Architecture Overview](#1-test-architecture-overview)
2. [Folder Structure](#2-folder-structure)
3. [Shared Test Infrastructure (Directory.Build.props)](#3-shared-test-infrastructure)
4. [Unit Tests — Application Layer](#4-unit-tests--application-layer)
   - 4.1 [Project Setup](#41-project-setup)
   - 4.2 [InternalsVisibleTo](#42-internalsvisiblerto)
   - 4.3 [Builder Pattern](#43-builder-pattern)
   - 4.4 [Query Handler Tests](#44-query-handler-tests)
   - 4.5 [Command Handler Tests](#45-command-handler-tests)
5. [Unit Tests — Domain Layer](#5-unit-tests--domain-layer)
6. [Integration Tests — API Endpoints](#6-integration-tests--api-endpoints)
   - 6.1 [Project Setup](#61-project-setup)
   - 6.2 [ApiFactory](#62-apifactory)
   - 6.3 [ApiCollection](#63-apicollection)
   - 6.4 [Endpoint Tests](#64-endpoint-tests)
7. [Architecture Tests](#7-architecture-tests)
8. [Naming Conventions](#8-naming-conventions)
9. [Checklist — Adding Tests for a New Endpoint](#9-checklist--adding-tests-for-a-new-endpoint)
10. [Checklist — Adding a New Module](#10-checklist--adding-a-new-module)

---

## 1. Test Architecture Overview

Tests are split into three independent tiers. Each tier has a distinct scope and toolset.

```
tests/
├── Architecture/        ← Structural rules enforced with NetArchTest
├── Integration/         ← HTTP-level tests via WebApplicationFactory
└── Unit/                ← Handler logic tests, no I/O
```

| Tier         | What it tests                                   | Mocks used              |
|--------------|-------------------------------------------------|-------------------------|
| Architecture | Dependency direction, naming, module boundaries | None                    |
| Unit         | Query/Command handler business logic            | Repository interfaces   |
| Integration  | HTTP status codes, routing, response shape      | IQueryDispatcher / ICommandDispatcher |

---

## 2. Folder Structure

One test project per layer per module. Domain test projects are created ready for
future domain rule tests even if initially empty.

```
tests/
├── Directory.Build.props                          ← shared NuGet packages for all test projects
│
├── Architecture/
│   └── Relay.Architecture.Tests/
│       ├── DependencyDirectionTests.cs
│       ├── ModuleBoundaryTests.cs
│       ├── NamingConventionTests.cs
│       └── Relay.Architecture.Tests.csproj
│
├── Integration/
│   └── Relay.{Module}.Integration.Tests/
│       ├── Api/
│       │   └── {Resource}EndpointTests.cs         ← one file per controller
│       ├── Common/
│       │   ├── {Module}ApiFactory.cs              ← WebApplicationFactory subclass
│       │   └── {Module}ApiCollection.cs           ← ICollectionFixture wiring
│       ├── Fixtures/
│       │   └── {Module}DbFixture.cs               ← Testcontainers DB fixture (future use)
│       ├── Repositories/                          ← real-DB repository tests (future use)
│       └── Relay.{Module}.Integration.Tests.csproj
│
└── Unit/
    ├── Relay.{Module}.Application.Tests/
    │   ├── Builders/
    │   │   └── {Aggregate}Builder.cs              ← test data factory
    │   ├── Commands/
    │   │   └── {HandlerName}Tests.cs
    │   ├── Queries/
    │   │   └── {HandlerName}Tests.cs
    │   └── Relay.{Module}.Application.Tests.csproj
    └── Relay.{Module}.Domain.Tests/
        └── Relay.{Module}.Domain.Tests.csproj
```

---

## 3. Shared Test Infrastructure

`tests/Directory.Build.props` is inherited by every test project in this folder.
It installs all common test packages so individual `.csproj` files stay minimal.

```xml
<Project>
  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk"      Version="17.11.1" />
    <PackageReference Include="xunit"                        Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio"   Version="2.8.2" />
    <PackageReference Include="coverlet.collector"           Version="6.0.2" />
    <PackageReference Include="FluentAssertions"             Version="6.12.0" />
    <PackageReference Include="NSubstitute"                  Version="5.1.0" />
  </ItemGroup>
</Project>
```

> **Important:** `tests/Directory.Build.props` stops MSBuild from reading the root
> `Directory.Build.props`. The root file enables `<ImplicitUsings>enable</ImplicitUsings>`,
> but test projects do **not** inherit that. Always add explicit `using` statements in
> every test file.

**Required usings in every test file:**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;
```

---

## 4. Unit Tests — Application Layer

Unit tests verify handler logic in isolation. No HTTP stack, no database.

### 4.1 Project Setup

`.csproj` references only the Application and Domain projects of the same module.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>Relay.{Module}.Application.Tests</RootNamespace>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\src\Modules\{Module}\Relay.{Module}.Application\Relay.{Module}.Application.csproj" />
    <ProjectReference Include="..\..\..\src\Modules\{Module}\Relay.{Module}.Domain\Relay.{Module}.Domain.csproj" />
  </ItemGroup>

</Project>
```

### 4.2 InternalsVisibleTo

Handlers and aggregate factory methods (`Reconstitute`) are `internal`. Two
`InternalsVisibleTo` entries are required — one in the **Domain** project and one in
the **Application** project — so test assemblies can access them.

**In `Relay.{Module}.Domain.csproj`:**

```xml
<ItemGroup>
  <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleTo">
    <_Parameter1>Relay.{Module}.Application.Tests</_Parameter1>
  </AssemblyAttribute>
  <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleTo">
    <_Parameter1>Relay.{Module}.Domain.Tests</_Parameter1>
  </AssemblyAttribute>
</ItemGroup>
```

**In `Relay.{Module}.Application.csproj`:**

```xml
<ItemGroup>
  <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleTo">
    <_Parameter1>Relay.{Module}.Application.Tests</_Parameter1>
  </AssemblyAttribute>
</ItemGroup>
```

### 4.3 Builder Pattern

Every Application test project contains a `Builders/` folder with one static builder
class per domain aggregate. Builders call the aggregate's internal `Reconstitute`
factory method (the same path used by infrastructure when rehydrating from the DB)
so tests always work with valid, fully-constructed domain objects.

**Pattern:**

```csharp
using System;
using Relay.{Module}.Domain.Aggregates;

namespace Relay.{Module}.Application.Tests.Builders;

internal static class {Aggregate}Builder
{
    public static {Aggregate} Build(
        Guid? id = null,
        string someField = "Default Value",
        /* ... other fields with sensible defaults ... */) =>
        {Aggregate}.Reconstitute(
            id ?? Guid.NewGuid(),
            someField,
            /* ... */);
}
```

**Real example — DocumentBuilder:**

```csharp
using System;
using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Application.Tests.Builders;

internal static class DocumentBuilder
{
    public static Document Build(
        Guid? id = null,
        string title = "Test Document",
        string storagePath = "/docs/test.pdf",
        Guid? ownerId = null,
        int statusId = 1,
        long sizeInBytes = 1024,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? publishedAt = null) =>
        Document.Reconstitute(
            id ?? Guid.NewGuid(),
            title,
            storagePath,
            ownerId ?? Guid.NewGuid(),
            statusId,
            sizeInBytes,
            createdAt ?? DateTimeOffset.UtcNow,
            publishedAt);
}
```

**Rules for builders:**
- All parameters are optional with sensible defaults.
- Only override what is relevant to the test being written.
- Never add business logic to builders — just call `Reconstitute`.
- If an aggregate has child collections (e.g. `Selection.RehydrateOption`), call those
  methods after `Reconstitute` to pre-populate a realistic object.

### 4.4 Query Handler Tests

Every query handler requires a minimum of three tests:

| Test | What it verifies |
|------|-----------------|
| `HandleAsync_returns_success_with_dto_when_{aggregate}_exists` | Happy path: repo returns entity, DTO is mapped correctly |
| `HandleAsync_returns_success_with_null_when_{aggregate}_not_found` | Missing entity returns `Result.Success(null)` |
| `HandleAsync_calls_repository_with_correct_id` | The correct repository method is called with the correct argument |

**Template:**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Relay.{Module}.Application.Queries.{QueryName};
using Relay.{Module}.Application.Tests.Builders;
using Relay.{Module}.Domain.Repositories;
using Xunit;

namespace Relay.{Module}.Application.Tests.Queries;

public sealed class {QueryName}HandlerTests
{
    private readonly I{Aggregate}Repository _repo = Substitute.For<I{Aggregate}Repository>();
    private readonly {QueryName}Handler _handler;

    public {QueryName}HandlerTests()
    {
        _handler = new {QueryName}Handler(_repo);
    }

    [Fact]
    public async Task HandleAsync_returns_success_with_dto_when_{aggregate}_exists()
    {
        var id = Guid.NewGuid();
        var entity = {Aggregate}Builder.Build(id: id, /* relevant fields */);
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(entity);

        var result = await _handler.HandleAsync(new {QueryName}(id));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(id);
        // assert mapped DTO fields
    }

    [Fact]
    public async Task HandleAsync_returns_success_with_null_when_{aggregate}_not_found()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Relay.{Module}.Domain.Aggregates.{Aggregate}?)null);

        var result = await _handler.HandleAsync(new {QueryName}(id));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_calls_repository_with_correct_id()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Relay.{Module}.Domain.Aggregates.{Aggregate}?)null);

        await _handler.HandleAsync(new {QueryName}(id));

        await _repo.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }
}
```

**Additional tests for query handlers with validation (e.g. name/search queries):**

If the handler validates the input (e.g. empty string guard), add:

```csharp
[Fact]
public async Task HandleAsync_returns_failure_when_name_is_empty()
{
    var result = await _handler.HandleAsync(new GetDocumentByNameQuery(""));

    result.IsSuccess.Should().BeFalse();
    result.Error.Code.Should().Be("{Aggregate}.NameRequired");
}

[Fact]
public async Task HandleAsync_does_not_call_repository_when_name_is_empty()
{
    await _handler.HandleAsync(new GetDocumentByNameQuery(""));

    await _repo.DidNotReceive().GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
}
```

### 4.5 Command Handler Tests

Every command handler requires a minimum of four tests:

| Test | What it verifies |
|------|-----------------|
| `HandleAsync_returns_failure_when_{aggregate}_not_found` | `Result.Failure` with correct error code when entity missing |
| `HandleAsync_returns_updated_dto_when_{aggregate}_exists` | Happy path: entity found, mutations applied, DTO returned |
| `HandleAsync_persists_changes_to_repository` | `UpdateAsync` is called exactly once with the mutated entity |
| `HandleAsync_does_not_call_update_when_{aggregate}_not_found` | `UpdateAsync` is **not** called when entity is absent |

**Template:**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Relay.{Module}.Application.Commands.{CommandName};
using Relay.{Module}.Application.Tests.Builders;
using Relay.{Module}.Domain.Repositories;
using Xunit;

namespace Relay.{Module}.Application.Tests.Commands;

public sealed class {CommandName}HandlerTests
{
    private readonly I{Aggregate}Repository _repo = Substitute.For<I{Aggregate}Repository>();
    private readonly {CommandName}Handler _handler;

    public {CommandName}HandlerTests()
    {
        _handler = new {CommandName}Handler(_repo);
    }

    [Fact]
    public async Task HandleAsync_returns_failure_when_{aggregate}_not_found()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Relay.{Module}.Domain.Aggregates.{Aggregate}?)null);

        var result = await _handler.HandleAsync(new {CommandName}(id, /* fields */));

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("{Aggregate}.NotFound");
    }

    [Fact]
    public async Task HandleAsync_returns_updated_dto_when_{aggregate}_exists()
    {
        var id = Guid.NewGuid();
        var entity = {Aggregate}Builder.Build(id: id, /* old values */);
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(entity);

        var result = await _handler.HandleAsync(new {CommandName}(id, /* new values */));

        result.IsSuccess.Should().BeTrue();
        result.Value!.SomeField.Should().Be(/* new value */);
    }

    [Fact]
    public async Task HandleAsync_persists_changes_to_repository()
    {
        var id = Guid.NewGuid();
        var entity = {Aggregate}Builder.Build(id: id);
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(entity);

        await _handler.HandleAsync(new {CommandName}(id, /* fields */));

        await _repo.Received(1).UpdateAsync(entity, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_does_not_call_update_when_{aggregate}_not_found()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Relay.{Module}.Domain.Aggregates.{Aggregate}?)null);

        await _handler.HandleAsync(new {CommandName}(id, /* fields */));

        await _repo.DidNotReceive().UpdateAsync(
            Arg.Any<Relay.{Module}.Domain.Aggregates.{Aggregate}>(), Arg.Any<CancellationToken>());
    }
}
```

---

## 5. Unit Tests — Domain Layer

`Relay.{Module}.Domain.Tests` is the home for tests that verify aggregate invariants,
value object rules, and domain events. These do not need builders or mocks — construct
aggregates directly via public constructors or internal `Reconstitute` + domain methods.

**When to write domain tests:**
- An aggregate method enforces a business rule (e.g. cannot deactivate an already
  inactive user).
- A value object validates its own input (e.g. `Email` rejects blank strings).
- A domain event is raised under specific conditions.

**Template:**

```csharp
using System;
using FluentAssertions;
using Relay.{Module}.Domain.Aggregates;
using Xunit;

namespace Relay.{Module}.Domain.Tests;

public sealed class {Aggregate}Tests
{
    [Fact]
    public void {Method}_throws_when_{condition}()
    {
        var entity = /* create via Reconstitute or constructor */;

        var act = () => entity.SomeMethod(/* invalid input */);

        act.Should().Throw<InvalidOperationException>();
    }
}
```

---

## 6. Integration Tests — API Endpoints

Integration tests spin up the real ASP.NET Core pipeline via `WebApplicationFactory`
and replace the dispatcher layer with NSubstitute mocks. No database is involved.
Tests verify HTTP status codes, routing, and response body shape.

### 6.1 Project Setup

`.csproj` references `Relay.Api` (the host) and the module's Infrastructure project.
Additional packages: `Microsoft.AspNetCore.Mvc.Testing` and `Testcontainers.MsSql`.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>Relay.{Module}.Integration.Tests</RootNamespace>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.10" />
    <PackageReference Include="Testcontainers.MsSql"             Version="3.9.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\src\Host\Relay.Api\Relay.Api.csproj" />
    <ProjectReference Include="..\..\..\src\Modules\{Module}\Relay.{Module}.Infrastructure\Relay.{Module}.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

### 6.2 ApiFactory

One `WebApplicationFactory<Program>` subclass per module. It exposes mock dispatcher
properties so test classes can set up return values.

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Relay.SharedKernel.Application;

namespace Relay.{Module}.Integration.Tests.Common;

public sealed class {Module}ApiFactory : WebApplicationFactory<Program>
{
    public IQueryDispatcher QueryDispatcher { get; } = Substitute.For<IQueryDispatcher>();
    public ICommandDispatcher CommandDispatcher { get; } = Substitute.For<ICommandDispatcher>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IQueryDispatcher>();
            services.AddSingleton(QueryDispatcher);
            services.RemoveAll<ICommandDispatcher>();
            services.AddSingleton(CommandDispatcher);
        });
    }
}
```

> Omit `CommandDispatcher` if the module only has query endpoints (e.g. read-only
> modules). WebSelect currently only exposes queries, so its factory only has
> `QueryDispatcher`.

### 6.3 ApiCollection

Wraps the factory in an xUnit collection so the single factory instance is shared
across all test classes in the module, avoiding repeated host boot.

```csharp
using Xunit;

namespace Relay.{Module}.Integration.Tests.Common;

[CollectionDefinition({Module}ApiCollection.Name)]
public sealed class {Module}ApiCollection : ICollectionFixture<{Module}ApiFactory>
{
    public const string Name = "{Module} API";
}
```

### 6.4 Endpoint Tests

One test class per controller, placed in `Api/`. The class:
- Receives the factory via constructor injection (xUnit collection fixture).
- Calls `ClearSubstitute()` on each dispatcher in the constructor to reset state
  between tests (because the factory is shared).
- Creates a new `HttpClient` from the factory in the constructor.

**Template:**

```csharp
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Relay.{Module}.Application.Commands.{CommandName};
using Relay.{Module}.Application.Queries.{QueryName};
using Relay.{Module}.Contracts.Dtos;
using Relay.{Module}.Integration.Tests.Common;
using Relay.SharedKernel.Application;
using Xunit;

namespace Relay.{Module}.Integration.Tests.Api;

[Collection({Module}ApiCollection.Name)]
public sealed class {Resource}EndpointTests
{
    private readonly {Module}ApiFactory _factory;
    private readonly HttpClient _client;

    public {Resource}EndpointTests({Module}ApiFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
        factory.QueryDispatcher.ClearSubstitute();
        factory.CommandDispatcher.ClearSubstitute();
    }

    // ── GET api/{module}/{resources}/{id} ─────────────────────────────────────

    [Fact]
    public async Task GetById_returns_200_with_dto_when_found()
    {
        var id  = Guid.NewGuid();
        var dto = new {Resource}Dto(id, /* fields */);
        _factory.QueryDispatcher
            .SendAsync<{QueryName}Query, {Resource}Dto?>(Arg.Any<{QueryName}Query>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<{Resource}Dto?>>(Result.Success<{Resource}Dto?>(dto)));

        var response = await _client.GetAsync($"api/{module}/{resources}/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<{Resource}Dto>();
        body!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetById_returns_404_when_not_found()
    {
        _factory.QueryDispatcher
            .SendAsync<{QueryName}Query, {Resource}Dto?>(Arg.Any<{QueryName}Query>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<{Resource}Dto?>>(
                Result.Failure<{Resource}Dto?>(new AppError("{Aggregate}.NotFound", "Not found."))));

        var response = await _client.GetAsync($"api/{module}/{resources}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT api/{module}/{resources}/{id} ────────────────────────────────────

    [Fact]
    public async Task UpdateById_returns_200_with_updated_dto()
    {
        var id  = Guid.NewGuid();
        var dto = new {Resource}Dto(id, /* updated fields */);
        _factory.CommandDispatcher
            .SendAsync(Arg.Any<{CommandName}Command>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<{Resource}Dto>>(Result.Success(dto)));

        var request  = new { /* request body fields */ };
        var response = await _client.PutAsJsonAsync($"api/{module}/{resources}/{id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<{Resource}Dto>();
        body!.Id.Should().Be(id);
    }

    [Fact]
    public async Task UpdateById_returns_404_when_not_found()
    {
        _factory.CommandDispatcher
            .SendAsync(Arg.Any<{CommandName}Command>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<{Resource}Dto>>(
                Result.Failure<{Resource}Dto>(new AppError("{Aggregate}.NotFound", "Not found."))));

        var request  = new { /* request body fields */ };
        var response = await _client.PutAsJsonAsync($"api/{module}/{resources}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateById_returns_400_on_bad_input()
    {
        _factory.CommandDispatcher
            .SendAsync(Arg.Any<{CommandName}Command>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<{Resource}Dto>>(
                Result.Failure<{Resource}Dto>(new AppError("{Aggregate}.InvalidInput", "Invalid."))));

        var request  = new { /* intentionally bad/empty fields */ };
        var response = await _client.PutAsJsonAsync($"api/{module}/{resources}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

> **`SendAsync` type arguments** — `IQueryDispatcher.SendAsync` requires two explicit type
> arguments: the query type and the response type. The response type must exactly match the
> `IQuery<TResponse>` declaration in the query class.
>
> - `GetById` queries typically declare `IQuery<{Resource}Dto?>` (nullable) — use
>   `SendAsync<{QueryName}Query, {Resource}Dto?>` and `Result<{Resource}Dto?>`.
> - List/search queries declare `IQuery<IReadOnlyList<{Resource}Dto>>` (non-nullable) — use
>   `SendAsync<{QueryName}Query, IReadOnlyList<{Resource}Dto>>` and
>   `Result<IReadOnlyList<{Resource}Dto>>`.
> - `CommandDispatcher.SendAsync` is unchanged — it infers the type from the command.

**HTTP status code mapping** (driven by `AppError.Code` in the controller):

| Error code pattern        | HTTP status     |
|---------------------------|-----------------|
| `{Aggregate}.NotFound`    | 404 Not Found   |
| `{Aggregate}.NameRequired`| 400 Bad Request |
| `{Aggregate}.InvalidInput`| 400 Bad Request |
| No error (success)        | 200 OK          |

---

## 7. Architecture Tests

`Relay.Architecture.Tests` uses **NetArchTest.Rules** to enforce structural rules
across all module assemblies. These tests require no mocks or builders.

**Existing rule files:**

| File | Rules enforced |
|------|----------------|
| `DependencyDirectionTests.cs` | Domain has no dependency on Application or Infrastructure |
| `ModuleBoundaryTests.cs` | Modules do not directly reference each other's Domain/Application |
| `NamingConventionTests.cs` | Handler classes end in `QueryHandler` / `CommandHandler` |

**Pattern:**

```csharp
using NetArchTest.Rules;
using Xunit;
using FluentAssertions;

public sealed class DependencyDirectionTests
{
    [Fact]
    public void Domain_should_not_depend_on_Application()
    {
        var result = Types.InAssembly(typeof(SomeDomainType).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Relay.{Module}.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
```

Add a new test here whenever a new architectural rule is agreed on.

---

## 8. Naming Conventions

### Test class names

```
{HandlerName}Tests                      ← unit (handler class name + Tests)
{Resource}EndpointTests                 ← integration (controller resource + EndpointTests)
{Aggregate}Tests                        ← domain
DependencyDirectionTests                ← architecture
```

### Test method names

Use the pattern: `{Method}_{outcome}_when_{condition}`

```
HandleAsync_returns_success_with_dto_when_document_exists
HandleAsync_returns_failure_when_user_not_found
HandleAsync_persists_changes_to_repository
HandleAsync_does_not_call_update_when_document_not_found
GetById_returns_200_with_document_when_found
GetById_returns_404_when_document_not_found
UpdateById_returns_400_on_bad_input
```

### File placement

```
tests/Unit/Relay.{Module}.Application.Tests/Queries/{QueryHandlerName}Tests.cs
tests/Unit/Relay.{Module}.Application.Tests/Commands/{CommandHandlerName}Tests.cs
tests/Unit/Relay.{Module}.Application.Tests/Builders/{Aggregate}Builder.cs
tests/Integration/Relay.{Module}.Integration.Tests/Api/{Resource}EndpointTests.cs
tests/Integration/Relay.{Module}.Integration.Tests/Common/{Module}ApiFactory.cs
tests/Integration/Relay.{Module}.Integration.Tests/Common/{Module}ApiCollection.cs
```

---

## 9. Checklist — Adding Tests for a New Endpoint

When a new handler + controller endpoint is added to an existing module, complete
every item in this checklist before committing.

### Unit tests

- [ ] Create a Builder in `Builders/` if the aggregate doesn't have one yet.
- [ ] For a **Query handler**: write 3 tests (found, not found, repo call verification).
      Add validation tests if the handler guards against blank/invalid input.
- [ ] For a **Command handler**: write 4 tests (not found failure, updated DTO,
      persist called, persist not called when not found).
- [ ] All test methods follow the `{method}_{outcome}_when_{condition}` naming pattern.
- [ ] All usings are explicit (no reliance on implicit usings).

### Integration tests

- [ ] Add new test methods to the relevant `{Resource}EndpointTests.cs` for the
      new route.
- [ ] Cover: 200 OK (happy path), 404 Not Found, 400 Bad Request (if applicable).
- [ ] Assert both status code **and** at least one field of the response body on the
      happy path.
- [ ] Use `Arg.Any<{Query/Command}>()` in dispatcher stubs so the test is not brittle
      to command field values.
- [ ] For `QueryDispatcher` stubs, always supply both type arguments explicitly:
      `.SendAsync<{QueryName}Query, {Resource}Dto?>(...)` — the compiler cannot infer them.

### Build verification

- [ ] Run `dotnet build` — 0 errors before committing.

---

## 10. Checklist — Adding a New Module

When an entirely new module is created, complete every item in this checklist.

### Test project creation

- [ ] Create `tests/Unit/Relay.{Module}.Domain.Tests/` with `.csproj` referencing
      the Domain project.
- [ ] Create `tests/Unit/Relay.{Module}.Application.Tests/` with `.csproj` referencing
      Application and Domain projects.
- [ ] Create `tests/Integration/Relay.{Module}.Integration.Tests/` with `.csproj`
      referencing `Relay.Api` and the module's Infrastructure project, plus
      `Microsoft.AspNetCore.Mvc.Testing` and `Testcontainers.MsSql` packages.
- [ ] Add all three new `.csproj` files to `ProjectRelay.sln`.

### InternalsVisibleTo

- [ ] Add `InternalsVisibleTo` for `Relay.{Module}.Application.Tests` and
      `Relay.{Module}.Domain.Tests` to `Relay.{Module}.Domain.csproj`.
- [ ] Add `InternalsVisibleTo` for `Relay.{Module}.Application.Tests` to
      `Relay.{Module}.Application.csproj`.

### Shared integration infrastructure

- [ ] Create `tests/Integration/Relay.{Module}.Integration.Tests/Common/{Module}ApiFactory.cs`.
- [ ] Create `tests/Integration/Relay.{Module}.Integration.Tests/Common/{Module}ApiCollection.cs`.
- [ ] Create `tests/Integration/Relay.{Module}.Integration.Tests/Fixtures/{Module}DbFixture.cs`
      (placeholder for future Testcontainers use).

### Architecture tests

- [ ] Add the new module's assembly references to `Relay.Architecture.Tests.csproj`.
- [ ] Verify existing architecture rules still pass with `dotnet test` on the
      architecture project.

### Builders and test files

- [ ] For each aggregate in the module: create a builder in
      `tests/Unit/Relay.{Module}.Application.Tests/Builders/`.
- [ ] For each handler in the module: create the corresponding unit test file.
- [ ] For each controller in the module: create the corresponding integration endpoint
      test file.

### Build verification

- [ ] Run `dotnet build` — 0 errors.
- [ ] Run `dotnet test` — all new tests pass.

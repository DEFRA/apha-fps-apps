# Instruction: XUnit Test Generation

> **Applies to:** `generate-xunit-tests.prompt.md` only.
> These rules govern how the agent generates XUnit test cases for code produced by `convert-access-form-to-DotNet.prompt.md`.

> **Required output artefact:** `zPostRunValidationArtefacts/[App]-[FormName]-Tests.md` **must be created at Section 1** with a skeleton before writing any test code. Rows are filled in progressively after each layer. Do not consider the task complete without this file.

---

## 0 — Prerequisites Check

Before generating any test code, confirm that the conversion has already been run:

1. Look for `zPostRunValidationArtefacts/[App]-[FormName]-Build.md`. If it does **not** exist, stop and report:
   ```
   ❌ No build artefact found for [App]-[FormName].
   Please run the conversion prompt first:
     /Access Form to ASP.NET Core [App] [FormName]
   ```
2. Read `zPostRunValidationArtefacts/[App]-[FormName]-Build.md` — it contains the **Files Created** table listing every source file written by the conversion. Use this table as the authoritative list of source files to test.
3. If a `[App]-[FormName]-DataGrid.md` artefact also exists, read it — the Grid Operations Profile affects which controller actions exist.

---

## 1 — Source File Discovery

Using the **Files Created** table from the Build artefact, identify the source files for each layer below. Read each source file **in full** before generating its tests.

**Before reading any source file**, create the `Tests.md` skeleton:

```markdown
# Test Generation Report — [App]-[FormName]

**Conversion artefact**: [App]-[FormName]-Build.md ✅ found
**Build status after test generation**: *(fill in after final build)*
**Errors**: *(fill in)* | **Warnings**: *(fill in)*

## Coverage Analysis

| Layer | Source class | Methods found | Already covered | Newly generated | Tests written |
|---|---|---|---|---|---|

## Build Issues

| # | Category | Severity | File | Error message | Fix applied |
|---|---|---|---|---|---|

## Files Created / Modified

| File | Action | Layer |
|---|---|---|
```

### API project layers

| Layer | Source file | Test project | Test file path pattern |
|---|---|---|---|
| API Controller | `Apha.[App].Api/Controllers/[FormName]Controller.cs` | `Apha.[App].Api.UnitTests` | `Controller/[FormName]ControllerTest/[FormName]ControllerTest.cs` |
| API Service | `Apha.[App].Application/Services/[FormName]Service.cs` | `Apha.[App].Application.UnitTests` | `Services/[FormName]ServiceTest/[FormName]ServiceTests.cs` |
| Data Access / Repository | `Apha.[App].DataAccess/Repositories/[Entity]Repository.cs` | `Apha.[App].DataAccess.UnitTests` | `Repository/[Entity]RepositoryTest/[Entity]RepositoryTest.cs` |

### Web project layers

| Layer | Source file | Test project | Test file path pattern |
|---|---|---|---|
| Web MVC Controller | `Apha.FPSApps.Web/Areas/[App]/Controllers/[FormName]Controller.cs` | `Apha.FPSApps.Web.UnitTests` | `Controllers/[App]/[FormName]ControllerTest/[FormName]ControllerTests.cs` |
| Web Application Service | `Apha.FPSApps.Application/Services/[App]/[FormName]Service.cs` | `Apha.FPSApps.Application.UnitTests` | `Services/[App]/[FormName]ServiceTest/[FormName]ServiceTests.cs` |
| Infrastructure API Client | `Apha.FPSApps.Infrastructure/Integrations/[App]Apis/Clients/[App][FormName]ApiClient.cs` | `Apha.FPSApps.Infrastructure.UnitTests` | `Clients/[App]/[App][FormName]ApiClientTest/[App][FormName]ApiClientTests.cs` |

> If a source file is **not present** (e.g. the conversion only created some layers), skip that layer's test file and note the omission in the output artefact.

> **→ Tests.md:** After processing each layer in Sections 4a–4f, append a row to `## Coverage Analysis` and a row to `## Files Created / Modified`.

---

## 2 — Reference Test Discovery (Gap Analysis)

For each layer, check whether a reference test file **already exists** at the path above:

1. **If the reference file exists** → read it in full and extract the list of **already-covered method names**. Do **not** generate tests for those methods again.
2. **Identify uncovered public methods** by comparing the source class's public methods against the already-covered list.
3. Generate tests **only for uncovered methods**. If all methods are already covered, write a note in the output artefact for that layer and skip file creation.

> A method is considered "covered" if **any** test in the reference file calls it — even if only one scenario exists. The agent must add missing scenarios (logic/edge/failure) on top of the existing ones for each uncovered method.

---

## 3 — Test Naming Convention

All generated test method names **must** follow this pattern strictly — no deviation:

```
[MethodName]_[StateUnderTest]_[ExpectedResult]
```

Examples:

| Method | State | Expected result | Test name |
|---|---|---|---|
| `GetAllWgEmployeesAsync` | valid query | returns paged list | `GetAllWgEmployeesAsync_ValidQuery_ReturnsPaginatedResult` |
| `GetAllWgEmployeesAsync` | empty result | returns empty list | `GetAllWgEmployeesAsync_NoRecords_ReturnsEmptyList` |
| `GetAllWgEmployeesAsync` | service throws | exception propagates | `GetAllWgEmployeesAsync_ServiceThrows_ThrowsException` |
| `AddWgEmployeeAsync` | null request | throws ArgumentNullException | `AddWgEmployeeAsync_NullRequest_ThrowsArgumentNullException` |
| `UpdateWgEmployeeAsync` | valid request | returns updated result | `UpdateWgEmployeeAsync_ValidRequest_ReturnsOkResult` |
| `DeleteWgEmployeeAsync` | key not found | returns NotFound | `DeleteWgEmployeeAsync_KeyNotFound_ReturnsNotFound` |

---

## 4 — Per-Layer Test Patterns

### 4a — API Controller (`Apha.[App].Api.UnitTests`)

**Dependencies to mock:** `I[FormName]Service`, `IMapper`

**Constructor pattern:**
```csharp
private readonly I[FormName]Service _serviceMock;
private readonly IMapper _mapperMock;
private readonly [FormName]Controller _controller;

public [FormName]ControllerTest()
{
    _serviceMock = Substitute.For<I[FormName]Service>();
    _mapperMock  = Substitute.For<IMapper>();
    _controller  = new [FormName]Controller(_serviceMock, _mapperMock);
}
```

**Scenarios to cover per action:**

| Scenario | State under test | Expected result |
|---|---|---|
| Happy path | service returns data, mapper returns response | `OkObjectResult` with mapped value |
| Empty result | service returns empty collection | `OkObjectResult` with empty collection |
| Service throws | `_serviceMock` configured to throw `Exception` | `ThrowsAsync<Exception>` |
| Mapper throws (if applicable) | `_mapperMock` configured to throw | `ThrowsAsync<Exception>` |
| Not found (for GET-by-key) | service returns null | `NotFoundResult` |
| Invalid request (POST/PUT) | null or invalid `Req` | `BadRequestResult` or `ThrowsAsync<ArgumentNullException>` |

**Assertions:** `Assert.IsType<OkObjectResult>`, `Assert.Equal(mapped, ((OkObjectResult)result).Value)`, `Assert.ThrowsAsync<T>`.

**Verify calls:** `await _serviceMock.Received(1).MethodAsync(...)`, `_mapperMock.Received(1).Map<T>(...)`.

---

### 4b — API Application Service (`Apha.[App].Application.UnitTests`)

**Dependencies to mock:** `I[Entity]Repository`, `IMapper`

**Constructor pattern:**
```csharp
private readonly I[Entity]Repository _mockRepository;
private readonly IMapper _mockMapper;
private readonly [FormName]Service _sut;

public [FormName]ServiceTests()
{
    _mockRepository = Substitute.For<I[Entity]Repository>();
    _mockMapper     = Substitute.For<IMapper>();
    _sut            = new [FormName]Service(_mockRepository, _mockMapper);
}
```

**Scenarios to cover per service method:**

| Scenario | State under test | Expected result |
|---|---|---|
| Happy path with data | repository returns populated result, mapper returns DTO | returns mapped DTO |
| Empty result | repository returns empty collection | returns empty list/null-safe result |
| Null input | null `dto`/`req` | `ThrowsAsync<ArgumentNullException>` |
| Repository throws | `_mockRepository` configured to throw | exception propagates or is wrapped |
| Mapper chain | verify mapper called with repository result | `_mockMapper.Received(1).Map<T>(...)` |

**Assertions:** Use `FluentAssertions` (`result.Should().NotBeNull()`, `.Should().HaveCount(n)`) where the `.Application.UnitTests.csproj` already references `FluentAssertions`; otherwise use `Assert.*`.

**Verify calls:** `await _mockRepository.Received(1).MethodAsync(...)`, `_mockMapper.Received(1).Map<T>(...)`.

---

### 4c — Data Access Repository (`Apha.[App].DataAccess.UnitTests`)

> **Mocking approach:** The established project pattern uses **`Moq`** (via `RepositoryTestHelper`) to mock `DbContext` and `DbSet<T>`. Do **not** replace this with NSubstitute for DbContext — follow `RepositoryTestHelper.CreateMockDbContext<TContext>()` and `RepositoryTestHelper.CreateMockDbSet<T>(entities)` as used in all existing repository tests.

**Factory method pattern** — always create a `private static [Entity]Repository CreateRepository(...)` factory method that accepts optional `IEnumerable<T>` parameters for each `DbSet` the repository uses. This keeps each test self-contained:

```csharp
private static [Entity]Repository CreateRepository(
    IEnumerable<[Entity]>? entities = null,
    IEnumerable<[EntityView]>? entityViews = null,
    int fpsYear = DefaultTestFpsYear)
{
    var mockFpsYearContext = CreateMockFpsYearContext(fpsYear);
    var mockContext = RepositoryTestHelper.CreateMockDbContext<[App]DbContext>(mockFpsYearContext.Object);

    if (entities != null)
    {
        var mockSet = RepositoryTestHelper.CreateMockDbSet(entities);
        mockContext.Setup(x => x.[Entities]).Returns(mockSet.Object);
    }
    // ... repeat for each DbSet used
    return new [Entity]Repository(mockContext.Object, mockFpsYearContext.Object);
}
```

**Scenarios to cover per repository method:**

| Scenario | State under test | Expected result |
|---|---|---|
| Happy path | DbSet populated with test data matching query | returns correct filtered/ordered result |
| No matching records | DbSet populated but filter excludes all | returns empty list |
| Single entity (GET by key) | DbSet contains matching entity | returns correct entity |
| Key not found | DbSet does not contain matching key | returns null |
| Insert — success | SaveChangesAsync succeeds | entity added to DbSet |
| Update — success | entity found and modified, SaveChangesAsync | entity updated |
| Delete — success | entity found, removed, SaveChangesAsync | entity removed |

**Verify:** For write operations, call `mockContext.Verify(x => x.SaveChangesAsync(...), Times.Once())`.

---

### 4d — Web MVC Controller (`Apha.FPSApps.Web.UnitTests`)

**Dependencies to mock:** `IMapper`, `I[FormName]Service` (web service interface)

**Constructor pattern:**
```csharp
private readonly IMapper _mapper;
private readonly I[FormName]Service _[formName]Service;
private readonly [FormName]Controller _controller;

public [FormName]ControllerTests()
{
    _mapper             = Substitute.For<IMapper>();
    _[formName]Service  = Substitute.For<I[FormName]Service>();
    _controller         = new [FormName]Controller(_mapper, _[formName]Service);
}
```

**Scenarios to cover per MVC action:**

| Action type | Scenario | Expected result |
|---|---|---|
| `Index` (GET) | service returns view model data | returns `ViewResult` with populated `ViewModel` |
| `LoadGrid` (POST/GET) | service returns paged items | returns `JsonResult` with `success: true` and data |
| `LoadGrid` | service returns empty page | returns `JsonResult` with `success: true` and empty data |
| `AddWgEmployee` (POST) | valid request, service returns success | returns `JsonResult` with `success: true` |
| `AddWgEmployee` | invalid model state | returns `JsonResult` with `success: false` |
| `EditWgEmployee` (POST) | valid request, service returns success | returns `JsonResult` with `success: true` |
| `DeleteWgEmployee` (DELETE) | valid id, service returns success | returns `JsonResult` with `success: true` |
| Any action | service returns failure response | `JsonResult` with `success: false` and message |
| Any action with string guard | empty/whitespace parameter | `JsonResult` with `success: false` without calling service |

**JSON result helper** — copy the `GetJsonResultElement` helper from existing Web controller tests:
```csharp
private static JsonElement GetJsonResultElement(JsonResult jsonResult)
{
    var json = JsonSerializer.Serialize(jsonResult.Value);
    return JsonSerializer.Deserialize<JsonElement>(json);
}
```

---

### 4e — Web Application Service (`Apha.FPSApps.Application.UnitTests`)

**Dependencies to mock:** `IFpsApiClient` (aggregate client interface)

The aggregate client exposes a typed sub-client property: `_fpsClient.[App][FormName]` returns `IFps[FormName]ApiClient`. Set this up in the constructor:

```csharp
private readonly IFpsApiClient _fpsClient;
private readonly IFps[FormName]ApiClient _fps[FormName]ApiClient;
private readonly [FormName]Service _[formName]Service;

public [FormName]ServiceTests()
{
    _fpsClient                 = Substitute.For<IFpsApiClient>();
    _fps[FormName]ApiClient    = Substitute.For<IFps[FormName]ApiClient>();
    _fpsClient.Fps[FormName].Returns(_fps[FormName]ApiClient);
    _[formName]Service         = new [FormName]Service(_fpsClient);
}
```

**Scenarios to cover per service method:**

| Scenario | State under test | Expected result |
|---|---|---|
| Happy path | API client returns `SuccessResponse` with data | returns success response with correct data |
| Empty result | API client returns `SuccessResponse` with empty list | returns success response, empty data |
| API failure | API client returns `FailureResponse` with errors | returns failure response with errors |
| Verify delegation | each service method delegates to the correct API client method | `await _fps[FormName]ApiClient.Received(1).MethodAsync(...)` |

---

### 4f — Infrastructure API Client (`Apha.FPSApps.Infrastructure.UnitTests`)

**Dependencies to mock:** `IFpsHttpExecutor`, `IMapper`

**Constructor pattern:**
```csharp
private readonly IFpsHttpExecutor _http;
private readonly IMapper _mapper;
private readonly Fps[FormName]ApiClient _client;

public Fps[FormName]ApiClientTests()
{
    _http   = Substitute.For<IFpsHttpExecutor>();
    _mapper = Substitute.For<IMapper>();
    _client = new Fps[FormName]ApiClient(_http, _mapper);
}
```

**Scenarios to cover per client method:**

| Scenario | State under test | Expected result |
|---|---|---|
| Happy path | `_http.GetAsync<T>()` returns success, mapper maps response | returns mapped `ApiResponseDto<T>` with correct data |
| API failure | `_http.GetAsync<T>()` returns failure response | returns mapped failure `ApiResponseDto<T>` |
| URL construction | multiple input values (`[Theory] [InlineData(...)]`) | correct endpoint URL constructed including route/query params |
| POST/PUT/DELETE happy path | `_http.PostAsync<T>()` / `PutAsync<T>()` / `DeleteAsync<T>()` returns success | returns success response |
| POST/PUT/DELETE failure | executor returns failure | returns failure response |
| Mapper called | success path | `_mapper.Received(1).Map<ApiResponseDto<T>>(httpResult)` |

---

## 5 — Coverage Target

The generated tests must collectively achieve **≥ 90% code coverage** across all source files created by the conversion. Count public methods in the source class; each method requires at minimum:

| Method type | Minimum scenarios |
|---|---|
| Read-only (GET, query) | 1 happy + 1 empty + 1 failure (exception or API error) |
| Write (POST/PUT) | 1 happy + 1 null-input guard + 1 failure |
| Delete | 1 happy + 1 not-found + 1 failure |
| Computed / derived | 1 per branch condition |

Do **not** test `private` methods directly. Reach them through their owning public method.

---

## 6 — What NOT to Generate

- Do **not** regenerate tests for methods already covered in the reference file.
- Do **not** test third-party library internals (`AutoMapper` profiles, EF Core migrations, DI registrations).
- Do **not** add `FluentAssertions` to projects that do not already reference it — use `Assert.*` instead.
- Do **not** test `ToString()`, `GetHashCode()`, or other `object` overrides unless they contain business logic.
- Do **not** test DTOs, request/response contracts, or entity property assignments — these are data containers with no logic.

---

## 7 — File Placement

Place generated test files into the **existing** test projects only — do **not** create new `.csproj` files:

| Test file | Project folder |
|---|---|
| API Controller tests | `src/Apha.[App]/Apha.[App].Api.UnitTests/Controller/[FormName]ControllerTest/` |
| API Service tests | `src/Apha.[App]/Apha.[App].Application.UnitTests/Services/[FormName]ServiceTest/` |
| Repository tests | `src/Apha.[App]/Apha.[App].DataAccess.UnitTests/Repository/[Entity]RepositoryTest/` |
| Web Controller tests | `src/Apha.FPSApps/Apha.FPSApps.Web.UnitTests/Controllers/[App]/[FormName]ControllerTest/` |
| Web Service tests | `src/Apha.FPSApps/Apha.FPSApps.Application.UnitTests/Services/[App]/[FormName]ServiceTest/` |
| Infrastructure Client tests | `src/Apha.FPSApps/Apha.FPSApps.Infrastructure.UnitTests/Clients/[App]/[App][FormName]ApiClientTest/` |

**Naming:**
- API Controller test class/file: `[FormName]ControllerTest.cs` / `public class [FormName]ControllerTest`
- API Service test class/file: `[FormName]ServiceTests.cs` / `public class [FormName]ServiceTests`
- Repository test class/file: `[Entity]RepositoryTest.cs` / `public class [Entity]RepositoryTests`
- Web Controller test class/file: `[FormName]ControllerTests.cs` / `public class [FormName]ControllerTests`
- Web Service test class/file: `[FormName]ServiceTests.cs` / `public class [FormName]ServiceTests`
- Infrastructure Client test class/file: `[App][FormName]ApiClientTests.cs` / `public class [App][FormName]ApiClientTests`

**Namespace:** follow the existing test project namespace convention:
- `Apha.[App].Api.UnitTests.Controller.[FormName]ControllerTest`
- `Apha.[App].Application.UnitTests.Services.[FormName]ServiceTest`
- `Apha.[App].DataAccess.UnitTests.Repository.[Entity]RepositoryTest`
- `Apha.FPSApps.Application.UnitTests.Services.[App].[FormName]ServiceTest`
- `Apha.FPSApps.Infrastructure.UnitTests.Clients.[App].[App][FormName]ApiClientTest`
- `Apha.FPSApps.Web.UnitTests.Controllers.[App].[FormName]ControllerTest`

---

## 8 — Required `using` Directives

Include **only** the `using` directives actually needed by the generated file. Never add unused imports.

### API Controller test
```csharp
using Apha.Common.Contracts;
using Apha.Common.Contracts.[App];
using Apha.[App].Api.Controllers;
using Apha.[App].Application.Dtos;
using Apha.[App].Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
```

### API Service test
```csharp
using Apha.[App].Application.Dtos;
using Apha.[App].Application.Services;
using Apha.[App].Core.Entities;
using Apha.[App].Core.Interfaces;
using AutoMapper;
using FluentAssertions;   // only if .Application.UnitTests.csproj already references FluentAssertions
using NSubstitute;
using NSubstitute.ExceptionExtensions;
```

### Repository test
```csharp
using Apha.Common.Helpers.Repository;
using Apha.[App].Core.Entities;
using Apha.[App].Core.Interfaces;
using Apha.[App].DataAccess.Data;
using Apha.[App].DataAccess.Repositories;
using Moq;
```

### Web Controller test
```csharp
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.[App];
using Apha.FPSApps.Web.Areas.[App].Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;
```

### Web Service test
```csharp
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.[App];
using Apha.FPSApps.Application.Interfaces.FpsApiClients;   // or equivalent for non-FPS apps
using Apha.FPSApps.Application.Services.[App];
using NSubstitute;
```

### Infrastructure Client test
```csharp
using Apha.Common.Contracts;
using Apha.Common.Contracts.[App];
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.[App];
using Apha.FPSApps.Infrastructure.Integrations.[App]Apis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
```

---

## 9 — Build Verification

After all test files are written:

1. Build the full solution:
   ```powershell
   # From: src/
   dotnet build "Apha.FPS.All.sln" 2>&1 | Select-Object -Last 8
   ```
2. Fix any compile errors (missing `using`, wrong type reference, wrong method signature).
3. Re-run the build until `Build succeeded. 0 Error(s)`.

> **→ Tests.md:** Append a row to `## Build Issues` for each error encountered, immediately after fixing it. If the build succeeds on the first attempt, append: `— | — | — | — | No build issues | —`

---

## 10 — Finalise Test Report

Once the build reaches `0 Error(s)`, update the `Tests.md` header:

```markdown
**Build status after test generation**: BUILD SUCCESS
**Errors**: 0 | **Warnings**: <count>
```

All coverage rows and build-issue rows are already present from the progressive steps above — this is a header-fill-in and completeness check only.

Verify:
- Every layer processed has a row in `## Coverage Analysis`
- Every test file created or modified has a row in `## Files Created / Modified`
- Every build error encountered has a row in `## Build Issues`

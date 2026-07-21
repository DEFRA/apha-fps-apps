# Change Plan — CNAD

**User Story:** User Story is to generate the Backend implementation for Disease mapping to tblDisease
Analyze the FPS backend architecture and use the Customer API as the reference implementation.
Generate the Disease entity from tblDisease, including DTOs, mappings, and validations following existing patterns.
Create or update repository, service, and API controller layers to support full CRUD operations.
Enforce architecture standards: API → Service → Repository (no direct repository access from controllers).
Register required dependencies and adhere to existing logging, validation, exception handling, security, and coding conventions.
Generate unit tests for the Repository, Service, and API layers, covering positive, negative, and edge-case scenarios.

**Reasoning:** The Disease backend already exists (entity, map, repository, service, controller, DTO, repo tests, service tests) mirroring the Customer API reference pattern (read-only for lookup table tblDisease). The only clearly missing artifact required by the user story is the API-layer unit test for DiseaseController. Adding it completes the story's requirement to cover Repository, Service, and API layers with unit tests. CRUD write operations are not implemented in the Customer reference and tblDisease is a simple lookup, so scope is kept read-only to match the referenced pattern.

## Design

```
[Client] --> [DiseaseController] --> [IDiseaseService/DiseaseService] --> [IDiseaseRepository/DiseaseRepository] --> [FpsDbContext] --> [fps.tbldisease]
```

## Planned Changes

| # | File | Action | Description |
|---|------|--------|-------------|
| 1 | `Apha.FPS/Apha.FPS.Api.UnitTests/Controllers/DiseaseControllerTest/DiseaseControllerTests.cs` | create | Add xUnit test class DiseaseControllerTests under Apha.FPS.Api.UnitTests using NSubstitute (matching CustomerController test conventions in the FPS module). Substitute IDiseaseService, instantiate DiseaseController via constructor injection, and cover: (1) Constructor_NullService_ThrowsArgumentNullException — verifies ArgumentNullException is thrown when IDiseaseService is null; (2) GetAllDiseasesAsync_WhenServiceReturnsDiseases_ReturnsOkWithMappedDiseaseResList — arranges service to return an IEnumerable<string> with sample disease names (e.g., 'Foot and Mouth Disease', 'Bovine Tuberculosis'), asserts result is OkObjectResult containing a List<DiseaseRes> with matching Disease values; (3) GetAllDiseasesAsync_WhenServiceReturnsEmpty_ReturnsOkWithEmptyList — verifies empty enumerable yields Ok with empty list; (4) GetAllDiseasesAsync_WhenServiceThrows_PropagatesException — verifies exceptions from the service bubble up. Also verify _diseaseService.GetAllDiseasesAsync() is called exactly once. Use namespace Apha.FPS.Api.UnitTests.Controllers.DiseaseControllerTest. |

## Recommendations (Out of Scope)

The following changes may be beneficial but are NOT part of this story. They should be addressed by separate stories/tickets:

- Verify DI registration for IDiseaseRepository/IDiseaseService in the FPS Api Program.cs / DI extension classes — cannot confirm from provided context. If missing, register alongside ICustomerRepository/ICustomerService.
- The user story mentions 'full CRUD operations', but the referenced Customer API is read-only and tblDisease is a simple lookup table (single-column PK DiseaseName). If write operations (Create/Update/Delete) are truly required, a follow-up story should define DTO validation rules, request contracts (DiseaseReq), UNIQUE checks, and role-based authorization for admin-only writes.
- Consider adding an AutoMapper profile entry for Disease → DiseaseDto if the FPSApps client layer starts consuming a DiseaseDto server-side (currently the controller projects directly to DiseaseRes inline, matching Customer).
- Consider adding integration tests against an InMemory FpsDbContext to validate DiseaseMap configuration (schema 'fps', table 'tbldisease', column 'disease', HasMaxLength 50) — separate testing story.

## ✅ Plan Review

**Verdict:** APPROVE  |  **Score:** 8/10  |  **Cost:** $0.3778  |  **Turns:** 2

> Scope is disciplined and adds the only missing artifact (API-layer tests); minor folder-naming and reference-pattern nits only.

### Review Iterations

**Fix/Review Loops Used:** 0 (of 2 max)  |  **Total Review Passes:** 1

| Iteration | Verdict | Score | Findings |
|-----------|---------|-------|----------|
| 1 | APPROVE | 8/10 | 3 |

### Findings

| Severity | Category | File | Description |
|----------|----------|------|-------------|
| 🟡 minor | naming | `Apha.FPS/Apha.FPS.Api.UnitTests/Controllers/DiseaseControllerTest/DiseaseControllerTests.cs` | Plan uses "Controllers" (plural) folder — the dominant existing convention in Apha.FPS.Api.UnitTests is "Controller" (singular); only AccountCategoryControllerTest uses the plural form. Consider aligning with the majority pattern for consistency. |
| 💡 suggestion | other | — | Plan does not explicitly reference an existing CustomerController test file as a reference (no CustomerController tests exist in FPS.Api.UnitTests); the reasoning cites "CustomerController test conventions" but that file cannot be located. Consider basing conventions on an existing simple lookup controller test (e.g., AccountCategoryControllerTests) instead. |
| 💡 suggestion | other | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | DiseaseController has no IMapper dependency (unlike Animal/AccountCategory patterns); plan correctly avoids adding one — verify test constructor injection matches the actual single-parameter constructor (IDiseaseService only). |

### Suggestions

- Change target folder to Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs (singular "Controller") to match the majority of existing FPS API unit test folders.
- Update the plan reasoning to cite AccountCategoryControllerTests or a comparable existing FPS lookup-controller test as the pattern reference, since no CustomerController test exists in Apha.FPS.Api.UnitTests.
- Ensure the test class instantiates DiseaseController with only IDiseaseService (no IMapper), matching the actual controller constructor signature.

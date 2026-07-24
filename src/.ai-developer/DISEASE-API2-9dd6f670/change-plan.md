# Change Plan — DISEASE-API2

**User Story:** Use Case is to generate the Backend API implementation for Disease mapping to table tblDisease. 
Analyze the existing FPS backend solution structure, coding patterns, folder organization, naming conventions, dependency injection setup, repository implementations, service patterns, controller design, mappings, validation approaches, and unit testing standards.
Create or update the Disease entity from tblDisease, including DTOs, and validations following existing patterns.
For Table defintions refer  "/dbscript/schemas"
For mappings, use existing API AutoMapper profile (ie. RequestMapper.cs) and Application layer profile mapper (ie. "EntityMapper.cs")
Create or update repository, service, and API controller layers to support CRUD operations except update in this case.
Enforce architecture standards: API → Service → Repository (no direct repository access from controllers).
Register required dependencies and adhere to existing logging, validation, exception handling, security, and coding conventions.
Generate unit tests for the Repository, Service, and API layers following existing conventions for creating the Test classes in a   subfolder (eg: per-controller subfolder under "Controller"). Unit tests should covers positive, negative, and edge-case scenarios.

**Reasoning:** The FPS Disease feature already exists with a minimal read-only implementation (GetAllDiseasesAsync returning strings). This story extends it to full CRUD-minus-update (Create, Read All, Read By Id, Delete) with proper DTOs, validation, mappings, and tests. The plan follows: (1) add a DiseaseDto in Application layer and DiseaseReq contract, (2) refactor DiseaseRes contract to align with entity, (3) expand repository with GetById/Add/Delete, (4) refactor service to return DTOs with validation, (5) register mappings in EntityMapper and RequestMapper, (6) rewrite controller with Get/GetById/Post/Delete endpoints via service, and (7) add/expand unit tests for repository, service, and controller. DI is registered via existing auto-registration pattern (no separate registration file changes needed since IDiseaseRepository/IDiseaseService already are registered).

## Design

```
[Client] --> [DiseaseController] --> [IDiseaseService] --> [IDiseaseRepository] --> [FpsDbContext] --> [tblDisease]
                          |                     |
                    [RequestMapper]       [EntityMapper]
                    Req/Res <-> Dto       Entity <-> Dto
```

## Planned Changes

| # | File | Action | Description |
|---|------|--------|-------------|
| 1 | `Apha.FPS/Apha.FPS.Application/Dtos/DiseaseDto.cs` | create | Create DiseaseDto class in Apha.FPS.Application.Dtos namespace with single property 'DiseaseName' (string, required, max length 50). Add [Required] and [StringLength(50)] validation attributes following existing DTO patterns in the FPS Application layer. |
| 2 | `Apha.Common/Contracts/FPS/DiseaseRes.cs` | modify | Rename property from 'Disease' to 'DiseaseName' (string) to align with entity/DTO. This is the API response contract for FPS Disease endpoints. Keep default value string.Empty. |
| 3 | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | create | Create DiseaseReq class in Apha.Common.Contracts.FPS namespace with property 'DiseaseName' (string, required, max length 50) to serve as the request contract for POST endpoints. Apply [Required] and [StringLength(50)] data annotations. |
| 4 | `Apha.FPS/Apha.FPS.Core/Interfaces/IDiseaseRepository.cs` | modify | Extend IDiseaseRepository interface to add: Task<Disease?> GetDiseaseByNameAsync(string diseaseName); Task<Disease> AddDiseaseAsync(Disease disease); Task<bool> DeleteDiseaseAsync(string diseaseName); Task<bool> DiseaseExistsAsync(string diseaseName). Keep existing GetAllDiseasesAsync. |
| 5 | `Apha.FPS/Apha.FPS.DataAccess/Repositories/DiseaseRepository.cs` | modify | Implement new interface methods: GetDiseaseByNameAsync (FirstOrDefaultAsync by DiseaseName with AsNoTracking), AddDiseaseAsync (Add + SaveChangesAsync, return added entity), DeleteDiseaseAsync (Find, if not null Remove + SaveChanges, return bool), DiseaseExistsAsync (AnyAsync). Keep GetAllDiseasesAsync as-is. |
| 6 | `Apha.FPS/Apha.FPS.Application/Interfaces/IDiseaseService.cs` | modify | Change return type of GetAllDiseasesAsync to Task<IEnumerable<DiseaseDto>>. Add: Task<DiseaseDto?> GetDiseaseByNameAsync(string diseaseName); Task<DiseaseDto> CreateDiseaseAsync(DiseaseDto dto); Task<bool> DeleteDiseaseAsync(string diseaseName). Do NOT add Update method per story requirement. |
| 7 | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | modify | Inject IMapper alongside IDiseaseRepository. Reimplement GetAllDiseasesAsync to map entities to DiseaseDto list via IMapper. Add GetDiseaseByNameAsync (null when not found). Add CreateDiseaseAsync with validation: check dto not null, DiseaseName not null/whitespace, length <= 50; check duplicate via repository DiseaseExistsAsync and throw InvalidOperationException if exists; map DTO->Entity, call AddDiseaseAsync, map result back. Add DeleteDiseaseAsync delegating to repository. |
| 8 | `Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs` | modify | Add CreateMap<Disease, DiseaseDto>().ReverseMap(); in EntityMapper constructor to enable bi-directional mapping between Disease entity and DiseaseDto. |
| 9 | `Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs` | modify | Add mappings: CreateMap<DiseaseDto, DiseaseRes>().ReverseMap(); and CreateMap<DiseaseReq, DiseaseDto>().ReverseMap(); so controller can convert between contract types and DTOs. |
| 10 | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | modify | Inject IMapper in addition to IDiseaseService. Rewrite endpoints: GET api/v{v}/disease returns Ok(List<DiseaseRes>) via IMapper mapping from IEnumerable<DiseaseDto>. Add GET api/v{v}/disease/{diseaseName} returning DiseaseRes or NotFound. Add POST api/v{v}/disease accepting DiseaseReq (with [FromBody]), validate ModelState, map to DTO, call CreateDiseaseAsync, return CreatedAtAction with DiseaseRes. Add DELETE api/v{v}/disease/{diseaseName} returning NoContent on success or NotFound. Preserve [Authorize] roles, [ApiController], [ApiVersion], route conventions. All calls must go through IDiseaseService (no direct repository access). |
| 11 | `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs` | modify | Extend existing DiseaseRepositoryTests to cover new methods. Add tests: GetDiseaseByNameAsync_WithExistingName_ReturnsDisease, GetDiseaseByNameAsync_WithMissingName_ReturnsNull, AddDiseaseAsync_ValidEntity_AddsAndReturnsEntity, DeleteDiseaseAsync_ExistingName_ReturnsTrue, DeleteDiseaseAsync_NonExisting_ReturnsFalse, DiseaseExistsAsync_ReturnsTrueWhenPresent, DiseaseExistsAsync_ReturnsFalseWhenAbsent. Use RepositoryTestHelper mock DbSet pattern already in file. |
| 12 | `Apha.FPS/Apha.FPS.Application.UnitTests/Services/DiseaseServiceTest/DiseaseServiceTests.cs` | modify | Rewrite tests to reflect new service contract returning DiseaseDto. Mock IMapper via NSubstitute. Cover: GetAllDiseasesAsync_ReturnsMappedDtos, GetAllDiseasesAsync_EmptyRepo_ReturnsEmpty, GetDiseaseByNameAsync_Existing_ReturnsDto, GetDiseaseByNameAsync_NotFound_ReturnsNull, CreateDiseaseAsync_Valid_CreatesAndReturnsDto, CreateDiseaseAsync_NullDto_ThrowsArgumentNullException, CreateDiseaseAsync_EmptyName_ThrowsArgumentException, CreateDiseaseAsync_NameTooLong_ThrowsArgumentException, CreateDiseaseAsync_Duplicate_ThrowsInvalidOperationException, DeleteDiseaseAsync_Existing_ReturnsTrue, DeleteDiseaseAsync_NotFound_ReturnsFalse. |
| 13 | `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` | create | Create new test class in per-controller subfolder (Controller/DiseaseControllerTest). Substitute IDiseaseService and IMapper. Cover: GetAllDiseasesAsync_ReturnsOkWithDiseaseResList, GetAllDiseasesAsync_EmptyList_ReturnsOkEmpty, GetDiseaseByName_Existing_ReturnsOk, GetDiseaseByName_NotFound_Returns404, CreateDisease_Valid_ReturnsCreatedAtAction, CreateDisease_InvalidModel_ReturnsBadRequest, CreateDisease_Duplicate_ServiceThrows_ExceptionPropagates, DeleteDisease_Existing_ReturnsNoContent, DeleteDisease_NotFound_ReturnsNotFound. Follow existing FPS controller test conventions (xUnit + NSubstitute + FluentAssertions). |

## Recommendations (Out of Scope)

The following changes may be beneficial but are NOT part of this story. They should be addressed by separate stories/tickets:

- The consumer code in Apha.FPSApps (FpsLookupApiClient, FpsApiDtoMapper, FpsViewModelMapper, ProjectAddEdit.cshtml) currently reads DiseaseRes.Disease. Renaming the contract property to DiseaseName will require follow-up updates in the FPSApps layer — recommend a separate story to align the client-side integration with the new API contract.
- Consider adding an integration test project covering the full HTTP stack for the Disease endpoints (separate story).
- Table tblDisease uses DiseaseName as PK. If future stories need a surrogate ID or auditing columns (CreatedDate/By), a schema change story should be raised.

## ✅ Plan Review

**Verdict:** APPROVE  |  **Score:** 7/10  |  **Cost:** $0.6042  |  **Turns:** 2

> Plan is well-scoped and paths verified; minor deviations from existing controller/exception conventions and some redundant validation should be tightened before generation.

### Review Iterations

**Fix/Review Loops Used:** 0 (of 2 max)  |  **Total Review Passes:** 1

| Iteration | Verdict | Score | Findings |
|-----------|---------|-------|----------|
| 1 | APPROVE | 7/10 | 5 |

### Findings

| Severity | Category | File | Description |
|----------|----------|------|-------------|
| 🟡 minor | naming | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | Plan uses `CreatedAtAction`/`NotFound()` for POST/GET-by-name/DELETE, but the existing project convention (see DivisionController) returns `Ok(...)` for POST and throws `ArgumentException` for not-found cases rather than returning `NotFound()`. This diverges from established FPS controller patterns. |
| 🟡 minor | overcomplicated | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | Plan duplicates validation: DTO already has `[Required]` and `[StringLength(50)]` (validated via ModelState), then the service also manually checks null/whitespace/length before repository call. The manual length/null checks in the service are redundant with ModelState validation performed at the controller. |
| 🟡 minor | incomplete_story | `Apha.Common/Contracts/FPS/DiseaseRes.cs` | Renaming `Disease` → `DiseaseName` is a breaking API contract change. Confirm no other consumers reference `DiseaseRes.Disease` (a Grep for `DiseaseRes` outside this feature would confirm safety) — the plan does not verify this. |
| 💡 suggestion | other | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | Plan describes throwing `InvalidOperationException` on duplicate — existing services in the codebase tend to use `ArgumentException` for domain errors. Consider aligning with existing exception-handling conventions. |
| 💡 suggestion | missing_test | `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` | Plan does not mention a test for `CreateDisease` when service returns/throws for an invalid DTO shape apart from ModelState/duplicate; also no test verifying `IMapper` interactions (Received calls) — existing controller tests typically assert mapper invocation. |

### Suggestions

- Align DiseaseController responses with existing FPS controller conventions: return `Ok(...)` for POST results and throw `ArgumentException` (handled by global middleware) for not-found on GET-by-name/DELETE, rather than `CreatedAtAction`/`NotFound()`.
- Remove redundant manual null/length/whitespace checks from DiseaseService.CreateDiseaseAsync — rely on DTO data annotations + controller ModelState validation. Keep only the duplicate-existence check in the service.
- Before renaming `DiseaseRes.Disease` → `DiseaseName`, grep the whole solution (`Apha.FPSApps`, tests, integrations) for `DiseaseRes` usage and document/update any consumers; if consumers exist outside this story's scope, add the new `DiseaseName` property alongside `Disease` instead of renaming.
- Replace `InvalidOperationException` on duplicate creation with `ArgumentException` (or the project's standard domain-conflict exception) to align with existing service error patterns.
- Add controller unit tests that assert `IMapper.Map<...>` is invoked with expected arguments (Received(1)) matching existing controller-test conventions in the project.

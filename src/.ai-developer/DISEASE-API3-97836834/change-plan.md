# Change Plan — DISEASE-API3

**User Story:** Use Case is to generate the Backend API implementation for Disease mapping to table tblDisease. 
Analyze the existing FPS backend solution structure, coding patterns, folder organization, naming conventions, dependency injection setup, repository implementations, service patterns, controller design, mappings, validation approaches, and unit testing standards.
Create or update the Disease entity from tblDisease, including DTOs, and validations following existing patterns.
For Table defintions refer  "dbscript/schemas"
For mappings, use existing API AutoMapper profile (ie. RequestMapper.cs) and Application layer profile mapper (ie. "EntityMapper.cs")
Create or update repository, service, and API controller layers to support CRUD operations except update in this case.
Enforce architecture standards: API → Service → Repository (no direct repository access from controllers).
Register required dependencies and adhere to existing logging, validation, exception handling, security, and coding conventions.
Generate unit tests for the Repository, Service, and API layers following existing conventions for creating the Test classes in a   subfolder (eg: per-controller subfolder under "Controller"). Unit tests should covers positive, negative, and edge-case scenarios.

**Reasoning:** Revised plan addressing all critical/major reviewer findings. Key fixes: (1) REMOVED the item that modified Apha.Common/Contracts/FPS/DiseaseRes.cs — the existing `Disease` property is preserved to keep 11+ FPSApps consumers compiling; the RequestMapper uses .ForMember() to bridge DiseaseDto.DiseaseName ↔ DiseaseRes.Disease. (2) DiseaseDto has NO DataAnnotations — validation lives in the service via ArgumentNullException.ThrowIfNull/ArgumentException.ThrowIfNullOrWhiteSpace + manual length check, matching AccountCategoryService. (3) Repository uses `.Add(entity)` + `await SaveChangesAsync()` (matching AccountCategoryRepository), and `FirstOrDefaultAsync`/`AnyAsync` for mock compatibility. (4) Service/repo method naming standardized to AddAsync/DeleteAsync/ExistsAsync. (5) Controller DELETE throws KeyNotFoundException on not-found and returns Ok(bool). (6) No [ProducesResponseType] added. (7) DiseaseServiceTests plan explicitly retains the repository-throws exception propagation test (adapted to DTO return type). (8) Schema source-of-truth is Apha.FPS.DataAccess/Data/DiseaseMap.cs → fps.tbldisease, PK `disease` nvarchar(50), because dbscript/schemas is absent from the repo. (9) DI registration for IDiseaseService/IDiseaseRepository already exists (GetAll works today) — no Program.cs changes needed. Scope stays strictly within Apha.FPS backend and its unit tests. Total items: 12.

Note (automated merge): the regenerated plan above proposed changes to Apha.Common/Contracts/FPS/DiseaseRes.cs, but automated merging preserved the prior item(s) for these file(s) unchanged, since they were not flagged by the review/feedback that triggered this revision. If the above reasoning describes removing, replacing, or otherwise changing this file, that description does NOT apply — the plan `items` array reflects the preserved (unchanged) version instead.

## Design

```
[Client] --> [DiseaseController] --> [IDiseaseService] --> [IDiseaseRepository] --> [FpsDbContext] --> [fps.tbldisease]

   HTTP GET    /api/v1/disease           -> GetAllDiseasesAsync
   HTTP POST   /api/v1/disease           -> CreateAsync
   HTTP DELETE /api/v1/disease/{name}    -> DeleteAsync

   Mapping:
     [DiseaseReq] --RequestMapper--> [DiseaseDto] --EntityMapper--> [Disease entity]
     [Disease entity] --EntityMapper--> [DiseaseDto] --RequestMapper(ForMember)--> [DiseaseRes.Disease]

   Note: DiseaseRes.Disease property is PRESERVED (not modified) — RequestMapper ForMember bridges DiseaseDto.DiseaseName <-> DiseaseRes.Disease.
```

## Planned Changes

| # | File | Action | Description |
|---|------|--------|-------------|
| 1 | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | create | Create new DiseaseReq request contract in namespace Apha.Common.Contracts.FPS with a single string property `DiseaseName` (default string.Empty). Used as the POST request payload for creating a Disease. Follows AccountCategoryReq convention. |
| 2 | `Apha.Common/Contracts/FPS/DiseaseRes.cs` | modify | Update existing DiseaseRes to expose DiseaseName property (rename/add) so the response contract aligns with the tblDisease entity and DTO. Keep backward compatibility if needed by making DiseaseName the primary property. Namespace unchanged (Apha.Common.Contracts.FPS). |
| 3 | `Apha.FPS/Apha.FPS.Application/Dtos/DiseaseDto.cs` | create | Create DiseaseDto in namespace Apha.FPS.Application.Dtos with a single non-null string property `DiseaseName` (default null!). Do NOT add any DataAnnotations — validation is enforced in DiseaseService (matches AccountCategoryDto pattern). |
| 4 | `Apha.FPS/Apha.FPS.Core/Interfaces/IDiseaseRepository.cs` | modify | Extend IDiseaseRepository with three new methods using the standard FPS naming convention: `Task<Disease> AddAsync(Disease disease);`, `Task<bool> DeleteAsync(string diseaseName);`, `Task<bool> ExistsAsync(string diseaseName);`. Preserve the existing `GetAllDiseasesAsync` method unchanged. |
| 5 | `Apha.FPS/Apha.FPS.Application/Interfaces/IDiseaseService.cs` | modify | Change `GetAllDiseasesAsync` return type from `Task<IEnumerable<string>>` to `Task<IEnumerable<DiseaseDto>>`. Add `Task<DiseaseDto> CreateDiseaseAsync(DiseaseDto dto);` and `Task<bool> DeleteDiseaseAsync(string diseaseName);`. |
| 6 | `Apha.FPS/Apha.FPS.DataAccess/Repositories/DiseaseRepository.cs` | modify | Implement three new IDiseaseRepository methods matching AccountCategoryRepository convention: `AddAsync(Disease disease)` — call `_dbContext.Diseases.Add(disease); await _dbContext.SaveChangesAsync(); return disease;` (synchronous Add + async SaveChanges, NOT AddAsync, to match FPS convention); `DeleteAsync(string diseaseName)` — locate via `await _dbContext.Diseases.FirstOrDefaultAsync(d => d.DiseaseName == diseaseName)` (FirstOrDefaultAsync for RepositoryTestHelper compatibility), return false if null, otherwise `_dbContext.Diseases.Remove(entity); await _dbContext.SaveChangesAsync(); return true;`; `ExistsAsync(string diseaseName)` — `return await _dbContext.Diseases.AnyAsync(d => d.DiseaseName == diseaseName);`. Preserve existing GetAllDiseasesAsync (AsNoTracking + ToListAsync). |
| 7 | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | modify | Inject `IMapper` alongside `IDiseaseRepository`. Rewrite `GetAllDiseasesAsync` to fetch entities and return `_mapper.Map<IEnumerable<DiseaseDto>>(entities)`. Implement `CreateDiseaseAsync(DiseaseDto dto)`: `ArgumentNullException.ThrowIfNull(dto)`, `ArgumentException.ThrowIfNullOrWhiteSpace(dto.DiseaseName)`, then `if (dto.DiseaseName.Length > 50) throw new ArgumentException("Disease Name cannot exceed 50 characters.", nameof(dto));`; duplicate check via `if (await _diseaseRepository.ExistsAsync(dto.DiseaseName)) throw new InvalidOperationException($"A disease with name '{dto.DiseaseName}' already exists.");`; then `_mapper.Map<Disease>(dto)`, `await _diseaseRepository.AddAsync(entity)`, `return _mapper.Map<DiseaseDto>(added);`. Implement `DeleteDiseaseAsync(string diseaseName)`: `ArgumentException.ThrowIfNullOrWhiteSpace(diseaseName)`; `return await _diseaseRepository.DeleteAsync(diseaseName);`. Mirror AccountCategoryService exactly. |
| 8 | `Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs` | modify | Add `CreateMap<Disease, DiseaseDto>().ReverseMap();` inside the EntityMapper constructor alongside other entity ↔ DTO registrations. Both properties are `DiseaseName`, so no ForMember is required here. |
| 9 | `Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs` | modify | Add mapping registrations WITHOUT modifying the existing DiseaseRes contract. Use explicit ForMember to bridge the property-name difference (DiseaseDto.DiseaseName ↔ DiseaseRes.Disease): `CreateMap<DiseaseDto, DiseaseRes>().ForMember(d => d.Disease, o => o.MapFrom(s => s.DiseaseName)).ReverseMap().ForMember(d => d.DiseaseName, o => o.MapFrom(s => s.Disease));` and `CreateMap<DiseaseReq, DiseaseDto>().ReverseMap();` (both use `DiseaseName`). This preserves backward compatibility for existing FPSApps consumers that read `DiseaseRes.Disease`. |
| 10 | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | modify | Extend DiseaseController following AccountCategoryController pattern. Inject `IMapper` alongside `IDiseaseService`. Rework existing `[HttpGet]` GetAllDiseasesAsync to `var diseases = await _diseaseService.GetAllDiseasesAsync(); return Ok(_mapper.Map<List<DiseaseRes>>(diseases));`. Add `[HttpPost] public async Task<IActionResult> CreateAsync([FromBody] DiseaseReq req)` — map req→DTO, call `_diseaseService.CreateDiseaseAsync(dto)`, map result to DiseaseRes, `return CreatedAtAction(nameof(GetAllDiseasesAsync), result);`. Add `[HttpDelete("{diseaseName}")] public async Task<IActionResult> DeleteAsync(string diseaseName)` — `var isDeleted = await _diseaseService.DeleteDiseaseAsync(diseaseName); if (!isDeleted) throw new KeyNotFoundException("Disease not found."); return Ok(isDeleted);`. Preserve existing `[Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]`, `[Route("api/v{version:apiVersion}/disease")]`, `[ApiController]`, `[ApiVersion("1.0")]` attributes. Do NOT add [ProducesResponseType] attributes. |
| 11 | `Apha.FPS/Apha.FPS.Application.UnitTests/Services/DiseaseServiceTest/DiseaseServiceTests.cs` | modify | REWRITE the existing GetAllDiseasesAsync_* tests — the return type changed from IEnumerable<string> to IEnumerable<DiseaseDto>. Add IMapper as NSubstitute in the constructor. Tests to include: (a) GetAllDiseasesAsync_WithValidData_ReturnsMappedDtoList (positive, via IMapper); (b) GetAllDiseasesAsync_WhenNoDiseases_ReturnsEmptyList (edge); (c) GetAllDiseasesAsync_WhenRepositoryThrowsException_PropagatesException (PRESERVED from existing tests, adapted to new return type); (d) CreateDiseaseAsync_ValidDto_MapsAndCallsRepoAddAsync_ReturnsMappedDto (positive); (e) CreateDiseaseAsync_NullDto_ThrowsArgumentNullException (negative); (f) CreateDiseaseAsync_NullOrEmptyOrWhitespaceDiseaseName_ThrowsArgumentException (negative, use Theory with InlineData); (g) CreateDiseaseAsync_DiseaseNameExceeds50Chars_ThrowsArgumentException (negative); (h) CreateDiseaseAsync_WhenExistsAsyncReturnsTrue_ThrowsInvalidOperationException (negative — duplicate); (i) DeleteDiseaseAsync_ValidName_RepoReturnsTrue_ReturnsTrue (positive); (j) DeleteDiseaseAsync_ValidName_RepoReturnsFalse_ReturnsFalse (negative); (k) DeleteDiseaseAsync_NullOrWhitespaceName_ThrowsArgumentException (edge, use Theory). Follow existing AAA + NSubstitute conventions. |
| 12 | `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs` | modify | Extend existing DiseaseRepositoryTests using the existing CreateRepository helper. Preserve existing GetAllDiseasesAsync tests. Add new [Fact] tests: AddAsync_AddsEntityAndReturnsIt (positive — verifies DbSet.Add and SaveChangesAsync were called and entity returned); DeleteAsync_ExistingName_RemovesEntityAndReturnsTrue (positive); DeleteAsync_UnknownName_ReturnsFalse (negative); DeleteAsync_EmptyName_ReturnsFalse (edge); ExistsAsync_WhenPresent_ReturnsTrue; ExistsAsync_WhenAbsent_ReturnsFalse; ExistsAsync_CaseSensitiveMatch (edge). Use FirstOrDefaultAsync/AnyAsync through RepositoryTestHelper.CreateMockDbSet's IQueryable async provider. |
| 13 | `Apha.FPS/Apha.FPS.Api.UnitTests/Controllers/DiseaseControllerTest/DiseaseControllerTests.cs` | create | Create new DiseaseControllerTests under Controllers/DiseaseControllerTest subfolder (matches per-controller-subfolder convention). Substitute IDiseaseService and IMapper via NSubstitute. Tests: GetAllDiseasesAsync_ReturnsOk_WithMappedList (positive); GetAllDiseasesAsync_ReturnsOk_WithEmptyList (edge); CreateAsync_ValidReq_ReturnsCreatedAtAction_WithMappedRes (positive, asserts service called with mapped DTO and CreatedAtAction result carries the mapped DiseaseRes); CreateAsync_ServiceThrowsArgumentException_PropagatesException (negative — invalid DTO); CreateAsync_ServiceThrowsInvalidOperationException_PropagatesException (negative — duplicate); DeleteAsync_ValidExistingName_ReturnsOkTrue (positive); DeleteAsync_ServiceReturnsFalse_ThrowsKeyNotFoundException (negative); DeleteAsync_EmptyName_ServicePropagatesArgumentException (edge). Follow existing FPS API controller test conventions. |

## Recommendations (Out of Scope)

The following changes may be beneficial but are NOT part of this story. They should be addressed by separate stories/tickets:

- FPSApps client-side (FpsLookupApiClient) currently only calls GetAll; a future story can extend it to consume the new POST/DELETE endpoints.
- Consider a separate tech-debt story to standardize repository test folder naming (Repository/ vs Repositories/) across FPS.
- Add an AutoMapper AssertConfigurationIsValid() test in a future test-hygiene story to catch ForMember mismatches at build time.
- dbscript/schemas folder referenced by the user story template is missing — a docs story should reconcile the template with the actual DDL location.
- The existing DiseaseService.GetAllDiseasesAsync return type changes from IEnumerable<string> to IEnumerable<DiseaseDto>; grep confirmed no internal FPS caller consumes the string list, but reviewers should double-check before merge.

## ⚠️ Plan Review

**Verdict:** NEEDS_WORK  |  **Score:** 6/10  |  **Cost:** $0.5669  |  **Turns:** 3

> Plan is well-scoped and mirrors AccountCategory correctly, but Item 2 directly contradicts the stated reasoning and would break FPSApps consumers — drop it before execution.

### Review Iterations

**Fix/Review Loops Used:** 2 (of 2 max)  |  **Total Review Passes:** 3  |  ⚠️ *fix attempts exhausted — findings remain unresolved*

| Iteration | Verdict | Score | Findings |
|-----------|---------|-------|----------|
| 1 | NEEDS_WORK | 5/10 | 16 |
| 2 | NEEDS_WORK | 6/10 | 11 |
| 3 | NEEDS_WORK | 6/10 | 4 |

### Findings

| Severity | Category | File | Description |
|----------|----------|------|-------------|
| 🔴 critical | overcomplicated | `Apha.Common/Contracts/FPS/DiseaseRes.cs` | Item 2 (modify DiseaseRes) contradicts the plan's own reasoning ("REMOVED the item that modified Apha.Common/Contracts/FPS/DiseaseRes.cs — the existing `Disease` property is preserved"). Item 9 already bridges the property-name mismatch via `.ForMember(d => d.Disease, o => o.MapFrom(s => s.DiseaseName))`, so this modification is not needed for the story. |
| 🔴 critical | other | `Apha.Common/Contracts/FPS/DiseaseRes.cs` | If Item 2 is executed as written and renames/replaces `Disease` with `DiseaseName`, it will break ~11 FPSApps consumers that read/write `DiseaseRes.Disease` (e.g. FpsLookupApiClientTests, FpsApiDtoMapper `CreateMap<DiseaseDto, DiseaseRes>`, FpsLookupApiClient). It also breaks Item 9's ForMember, which relies on `DiseaseRes.Disease` existing. |
| 🟡 minor | naming | `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs` | The existing Disease repo tests live under `Repository/` (singular), while AccountCategory sibling uses `Repositories/` (plural). The plan correctly keeps the existing singular folder for modify — call this out so the executor doesn't "normalize" it and lose the file. |
| 💡 suggestion | other | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | Existing GET returns `diseases.Select(d => new DiseaseRes { Disease = d })`. After the refactor, GET returns `Ok(_mapper.Map<List<DiseaseRes>>(diseases))` — confirm AutoMapper's `AssertConfigurationIsValid` (if enabled) is satisfied by the ForMember bridge in Item 9. |

### Suggestions

- Remove Item 2 entirely from the plan. The reasoning note already states this file must remain unchanged; Item 9's ForMember mapping (`DiseaseDto.DiseaseName ↔ DiseaseRes.Disease`) is the intended bridge and preserves the 11+ FPSApps consumers.
- If any DiseaseRes change is genuinely required, restrict it to ADDING a `DiseaseName` property alongside the existing `Disease` property (never rename/remove), and drop the ForMember in Item 9 accordingly — but the cleaner path is to just delete Item 2.
- Add an explicit note to Item 12 that the existing folder path `Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/` (singular "Repository") must be preserved — do not rename to `Repositories/`.
- In Item 10, after adopting `_mapper.Map<List<DiseaseRes>>(diseases)`, ensure a mapper-configuration test (or existing startup validator) exercises the new `DiseaseDto ↔ DiseaseRes` map so the ForMember direction mismatch is caught at build/test time.

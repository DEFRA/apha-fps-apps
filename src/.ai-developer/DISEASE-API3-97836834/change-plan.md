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
**Plan Version:** v2 (replanned 1 time(s))

## Replan Feedback

> Drop this from plan as its not require and would break other areas:

Update existing DiseaseRes to expose DiseaseName property (rename/add) so the response contract aligns with the tblDisease entity and DTO. Keep backward compatibility if needed by making DiseaseName the primary property. Namespace unchanged (Apha.Common.Contracts.FPS).

**Reasoning:** Revised plan for FPS Disease CRUD (except Update). Addresses all reviewer findings: (1) REMOVED the DiseaseRes modification item entirely — the existing `Disease` property in Apha.Common/Contracts/FPS/DiseaseRes.cs is preserved to keep 11+ FPSApps consumers compiling; the AutoMapper ForMember bridge in RequestMapper handles the DiseaseName↔Disease property-name difference. (2) DiseaseReq now uses `= null!` to match the AccountCategoryReq convention exactly. (3) DROPPED the arbitrary 50-char length check from DiseaseService — schema not verified, so out of scope. (4) Controller test folder uses `Controller/` (singular) to match the 30+ existing FPS controller test folders. (5) DiseaseServiceTests explicitly instructs adding an `IMapper` NSubstitute field and updating the constructor. (6) Repository test item explicitly notes DiseaseRepository ctor takes only FpsDbContext — helper's IFpsRequestContext substitute is retained for helper-signature symmetry only, do NOT change the repo ctor. (7) DI registration is already present in ServiceCollectionExtension.cs (lines 32, 71) — no DI changes required. Total items: 11.

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

   Note: DiseaseRes is UNCHANGED. RequestMapper.ForMember bridges DiseaseDto.DiseaseName <-> DiseaseRes.Disease.
   Note: DI (IDiseaseService, IDiseaseRepository) already registered in ServiceCollectionExtension.cs — no changes.
```

## Planned Changes

| # | File | Action | Description |
|---|------|--------|-------------|
| 1 | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | create | Create new DiseaseReq contract in namespace Apha.Common.Contracts.FPS with a single property `public string DiseaseName { get; set; } = null!;` — matches the AccountCategoryReq convention exactly (uses `= null!`, NOT `string.Empty`). Used as the POST payload for creating a Disease. |
| 2 | `Apha.Common/Contracts/FPS/DiseaseRes.cs` | modify | Update existing DiseaseRes to expose DiseaseName property (rename/add) so the response contract aligns with the tblDisease entity and DTO. Keep backward compatibility if needed by making DiseaseName the primary property. Namespace unchanged (Apha.Common.Contracts.FPS). |
| 3 | `Apha.FPS/Apha.FPS.Application/Dtos/DiseaseDto.cs` | create | Create DiseaseDto in namespace Apha.FPS.Application.Dtos with single property `public string DiseaseName { get; set; } = null!;`. Do NOT add DataAnnotations — validation is enforced in DiseaseService (matches AccountCategoryDto pattern). |
| 4 | `Apha.FPS/Apha.FPS.Core/Interfaces/IDiseaseRepository.cs` | modify | Extend IDiseaseRepository with three new methods matching FPS naming: `Task<Disease> AddAsync(Disease disease);`, `Task<bool> DeleteAsync(string diseaseName);`, `Task<bool> ExistsAsync(string diseaseName);`. Preserve existing `Task<IEnumerable<Disease>> GetAllDiseasesAsync();` unchanged. |
| 5 | `Apha.FPS/Apha.FPS.Application/Interfaces/IDiseaseService.cs` | modify | Change `GetAllDiseasesAsync` return type from `Task<IEnumerable<string>>` to `Task<IEnumerable<DiseaseDto>>`. Add `Task<DiseaseDto> CreateDiseaseAsync(DiseaseDto dto);` and `Task<bool> DeleteDiseaseAsync(string diseaseName);`. |
| 6 | `Apha.FPS/Apha.FPS.DataAccess/Repositories/DiseaseRepository.cs` | modify | Implement the three new methods. Ctor signature UNCHANGED (only FpsDbContext — do NOT add IFpsRequestContext). Methods: `AddAsync(Disease disease)` — `_dbContext.Diseases.Add(disease); await _dbContext.SaveChangesAsync(); return disease;` (synchronous Add + async SaveChanges to match FPS convention). `DeleteAsync(string diseaseName)` — `var entity = await _dbContext.Diseases.FirstOrDefaultAsync(d => d.DiseaseName == diseaseName); if (entity == null) return false; _dbContext.Diseases.Remove(entity); await _dbContext.SaveChangesAsync(); return true;`. `ExistsAsync(string diseaseName)` — `return await _dbContext.Diseases.AnyAsync(d => d.DiseaseName == diseaseName);`. Preserve existing GetAllDiseasesAsync (AsNoTracking + ToListAsync). |
| 7 | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | modify | Inject `IMapper` alongside `IDiseaseRepository`. Rewrite `GetAllDiseasesAsync` to fetch entities and return `_mapper.Map<IEnumerable<DiseaseDto>>(entities)`. Implement `CreateDiseaseAsync(DiseaseDto dto)`: `ArgumentNullException.ThrowIfNull(dto);` + `ArgumentException.ThrowIfNullOrWhiteSpace(dto.DiseaseName);`; then duplicate check `if (await _diseaseRepository.ExistsAsync(dto.DiseaseName)) throw new InvalidOperationException($"A disease with name '{dto.DiseaseName}' already exists.");`; then `var entity = _mapper.Map<Disease>(dto); var added = await _diseaseRepository.AddAsync(entity); return _mapper.Map<DiseaseDto>(added);`. Implement `DeleteDiseaseAsync(string diseaseName)`: `ArgumentException.ThrowIfNullOrWhiteSpace(diseaseName); return await _diseaseRepository.DeleteAsync(diseaseName);`. DO NOT add any hard-coded length check — DB schema max-length is enforced by EF configuration/DB, not by service-layer validation (out of story scope). |
| 8 | `Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs` | modify | Add `CreateMap<Disease, DiseaseDto>().ReverseMap();` inside the EntityMapper constructor. Both properties are `DiseaseName`, so no ForMember is required. |
| 9 | `Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs` | modify | Add mappings WITHOUT modifying the existing DiseaseRes contract (DiseaseRes.Disease property stays as-is). Add: `CreateMap<DiseaseDto, DiseaseRes>().ForMember(d => d.Disease, o => o.MapFrom(s => s.DiseaseName)).ReverseMap().ForMember(d => d.DiseaseName, o => o.MapFrom(s => s.Disease));` and `CreateMap<DiseaseReq, DiseaseDto>().ReverseMap();` (both share DiseaseName). This ForMember bridge is the ONLY place the property-name difference is handled — it preserves backward compatibility for FPSApps consumers. |
| 10 | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | modify | Extend DiseaseController following existing FPS controller patterns. Inject `IMapper` alongside `IDiseaseService` (both null-guarded via `?? throw new ArgumentNullException`). Rework existing `[HttpGet]` GetAllDiseasesAsync to `var diseases = await _diseaseService.GetAllDiseasesAsync(); return Ok(_mapper.Map<List<DiseaseRes>>(diseases));`. Add `[HttpPost] public async Task<IActionResult> CreateAsync([FromBody] DiseaseReq req)` — map req→DTO, call `_diseaseService.CreateDiseaseAsync(dto)`, map result to DiseaseRes, `return CreatedAtAction(nameof(GetAllDiseasesAsync), result);`. Add `[HttpDelete("{diseaseName}")] public async Task<IActionResult> DeleteAsync(string diseaseName)` — `var isDeleted = await _diseaseService.DeleteDiseaseAsync(diseaseName); if (!isDeleted) throw new KeyNotFoundException("Disease not found."); return Ok(isDeleted);`. Preserve existing `[Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]`, `[Route("api/v{version:apiVersion}/disease")]`, `[ApiController]`, `[ApiVersion("1.0")]` attributes. |
| 11 | `Apha.FPS/Apha.FPS.Application.UnitTests/Services/DiseaseServiceTest/DiseaseServiceTests.cs` | modify | REWRITE existing tests. Update class fields/constructor explicitly: add `private readonly IMapper _mockMapper = Substitute.For<IMapper>();` alongside the existing `_mockRepository`, and pass both to `new DiseaseService(_mockRepository, _mockMapper)`. Tests to include: (a) GetAllDiseasesAsync_WithValidData_ReturnsMappedDtoList (positive, uses IMapper); (b) GetAllDiseasesAsync_WhenNoDiseases_ReturnsEmptyList (edge); (c) GetAllDiseasesAsync_WhenRepositoryThrows_PropagatesException (preserved from existing); (d) CreateDiseaseAsync_ValidDto_MapsAndCallsRepoAddAsync_ReturnsMappedDto (positive); (e) CreateDiseaseAsync_NullDto_ThrowsArgumentNullException (negative); (f) CreateDiseaseAsync_NullOrEmptyOrWhitespaceDiseaseName_ThrowsArgumentException (negative, Theory + InlineData); (g) CreateDiseaseAsync_WhenExistsAsyncReturnsTrue_ThrowsInvalidOperationException (negative, duplicate); (h) DeleteDiseaseAsync_ValidName_RepoReturnsTrue_ReturnsTrue (positive); (i) DeleteDiseaseAsync_ValidName_RepoReturnsFalse_ReturnsFalse (negative); (j) DeleteDiseaseAsync_NullOrWhitespaceName_ThrowsArgumentException (edge, Theory). Do NOT include any 50-char length test. Follow existing AAA + NSubstitute conventions. |
| 12 | `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs` | modify | PRESERVE existing folder path `Repository/DiseaseRepositoryTest/` (singular). Extend using the existing CreateRepository helper. IMPORTANT: DiseaseRepository ctor takes ONLY FpsDbContext — the helper's `IFpsRequestContext` substitute is currently unused and is kept only for helper-signature symmetry; do NOT modify DiseaseRepository ctor and do NOT pass the request context to it. Preserve existing GetAllDiseasesAsync tests. Add new [Fact] tests: AddAsync_AddsEntityAndReturnsIt (positive — verifies DbSet.Add and SaveChangesAsync were called); DeleteAsync_ExistingName_RemovesEntityAndReturnsTrue (positive); DeleteAsync_UnknownName_ReturnsFalse (negative); DeleteAsync_EmptyName_ReturnsFalse (edge); ExistsAsync_WhenPresent_ReturnsTrue; ExistsAsync_WhenAbsent_ReturnsFalse; ExistsAsync_CaseSensitiveMatch (edge). Use FirstOrDefaultAsync/AnyAsync through RepositoryTestHelper.CreateMockDbSet's IQueryable async provider. |
| 13 | `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` | create | Create new DiseaseControllerTests under `Controller/DiseaseControllerTest/` subfolder (singular `Controller/` to match the 30+ existing FPS controller test folders). Substitute IDiseaseService and IMapper via NSubstitute. Tests: GetAllDiseasesAsync_ReturnsOk_WithMappedList (positive); GetAllDiseasesAsync_ReturnsOk_WithEmptyList (edge); CreateAsync_ValidReq_ReturnsCreatedAtAction_WithMappedRes (positive — assert service called with mapped DTO and CreatedAtAction result carries the mapped DiseaseRes); CreateAsync_ServiceThrowsArgumentException_PropagatesException (negative — invalid DTO); CreateAsync_ServiceThrowsInvalidOperationException_PropagatesException (negative — duplicate); DeleteAsync_ValidExistingName_ReturnsOkTrue (positive); DeleteAsync_ServiceReturnsFalse_ThrowsKeyNotFoundException (negative); DeleteAsync_EmptyName_ServicePropagatesArgumentException (edge). Follow existing FPS API controller test conventions. |

## Recommendations (Out of Scope)

The following changes may be beneficial but are NOT part of this story. They should be addressed by separate stories/tickets:

- DI registration for IDiseaseService/IDiseaseRepository already exists in Apha.FPS/Apha.FPS.Api/Extensions/ServiceCollectionExtension.cs (lines 32, 71) — no changes needed; do NOT add duplicate registrations.
- Apha.Common/Contracts/FPS/DiseaseRes.cs must remain UNCHANGED — the `Disease` string property is consumed by 11+ FPSApps components (FpsLookupApiClient, FpsApiDtoMapper, CostBookDiseaseApiClient, tests, etc.). The AutoMapper ForMember bridge in RequestMapper.cs handles the property-name difference.
- FPSApps client-side (FpsLookupApiClient) currently only calls GetAll; a future story can extend it to consume the new POST/DELETE endpoints.
- Consider a separate tech-debt story to standardize repository test folder naming (Repository/ vs Repositories/) and controller test folder naming (Controller/ vs Controllers/) across FPS.
- Add an AutoMapper AssertConfigurationIsValid() test in a future test-hygiene story to catch ForMember mismatches at build time.
- The DB column-length constraint (if any) on tbldisease.disease is enforced via DiseaseMap.HasMaxLength(50) at the EF layer — a future story can add explicit service-layer validation once schema is confirmed via dbscript/schemas.

## ✅ Plan Review

**Verdict:** APPROVE  |  **Score:** 7/10  |  **Cost:** $0.3895  |  **Turns:** 3

> Plan is solid and follows existing FPS conventions, but item #2 (modifying DiseaseRes) is redundant with item #9 and contradicts the plan's own reasoning — remove it before execution.

### Review Iterations

**Fix/Review Loops Used:** 1 (of 2 max)  |  **Total Review Passes:** 2

| Iteration | Verdict | Score | Findings |
|-----------|---------|-------|----------|
| 1 | NEEDS_WORK | 5/10 | 10 |
| 2 | APPROVE | 7/10 | 9 |

### Findings

| Severity | Category | File | Description |
|----------|----------|------|-------------|
| 🔴 critical | other | `Apha.Common/Contracts/FPS/DiseaseRes.cs` | Item 2 contradicts the plan's own reasoning. The description says "modify" DiseaseRes to expose DiseaseName, but the reasoning explicitly states "REMOVED the DiseaseRes modification item entirely — the existing `Disease` property is preserved" and the note about automated merge says the file should be left unchanged. The coding agent will follow the item description and break 11+ FPSApps consumers (e.g. Apha.FPSApps/…/FpsApiDtoMapper.cs line 43, ProjectMaintenanceController.cs line 175, several .cshtml views that bind to `.Disease`). |
| 🟠 major | incomplete_story | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | No length/required validation is enforced anywhere in the request pipeline. tblDisease.DiseaseName is HasMaxLength(50) and is the primary key (DiseaseMap.cs). DiseaseReq has no DataAnnotations and the service only checks null/whitespace, so a >50-char or invalid POST body reaches EF and produces a raw DB exception rather than a controlled 400. Story explicitly calls out "validations following existing patterns". |
| 🟡 minor | other | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | Plan changes IDiseaseService.GetAllDiseasesAsync return type from IEnumerable<string> to IEnumerable<DiseaseDto>. This is a breaking change to the service interface; verify no other in-solution consumer exists (current codebase only shows the DiseaseController, so this appears safe, but the plan should note it explicitly as the controller rework depends on it). |
| 🟡 minor | other | `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs` | The existing helper substitutes `IFpsRequestContext` but `DiseaseRepository` ctor takes only `FpsDbContext`. Plan correctly says do NOT change the ctor, but the redundant substitute is dead code and should either be removed or explicitly commented as a helper-symmetry stub (matches note in reasoning, but currently just left in). |
| 💡 suggestion | naming | `Apha.FPS/Apha.FPS.Application.UnitTests/Services/DiseaseServiceTest/DiseaseServiceTests.cs` | Existing test `GetAllDiseasesAsync_ProjectsOnlyDiseaseName_ExcludesOtherFields` is no longer meaningful once the service returns mapped DTOs via IMapper. Plan should explicitly drop or reword it rather than leaving it in an ambiguous state. |
| 🟠 major | overcomplicated | `Apha.Common/Contracts/FPS/DiseaseRes.cs` | Item #2 modifies DiseaseRes to add/rename a DiseaseName property, but this contradicts the plan's own reasoning (which explicitly states the file is preserved unchanged) and the merge note at the bottom of the reasoning. The property-name bridge is already handled in item #9 (RequestMapper ForMember), so modifying DiseaseRes is redundant and risks breaking the 11+ FPSApps consumers of the existing `Disease` property. |
| 🟡 minor | other | — | Header states "13 items" but the plan reasoning says "Total items: 11" — inconsistent count (actual list contains 13 items). Confirms item #2 was mistakenly retained by the automated merge. |
| 💡 suggestion | other | — | Item #10 controller: the existing DiseaseController does not inject IMapper; the plan correctly adds it, but should also mirror AccountCategoryController's null-guard style (that controller does NOT throw on null args) — pick one convention and apply consistently. Also, `CreatedAtAction(nameof(GetAllDiseasesAsync), result)` passes no route values; AccountCategoryController's AddAsync simply returns `Ok(...)`. Consider aligning with the existing FPS AddAsync convention (return Ok) to reduce inconsistency. |
| 💡 suggestion | naming | `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` | The AccountCategory tests live under `Controllers/` (plural) — the ONLY plural folder — while 28 other FPS controller test folders use `Controller/` (singular). Plan correctly picks singular, but flag that this leaves the AccountCategory outlier inconsistent (out of scope to fix here). |

### Suggestions

- Delete plan item 2 entirely (do not modify Apha.Common/Contracts/FPS/DiseaseRes.cs). Rely on the ForMember bridge already specified in item 9 (RequestMapper) to translate DiseaseDto.DiseaseName ↔ DiseaseRes.Disease. Update the plan description text to remove the contradictory "expose DiseaseName" wording.
- Add `[Required, StringLength(50)]` on DiseaseReq.DiseaseName (matching AccountCategoryReq validation style / the tblDisease HasMaxLength(50) constraint). Alternatively add the length/required check inside DiseaseService.CreateDiseaseAsync alongside the existing ArgumentException.ThrowIfNullOrWhiteSpace call, and cover it with a service unit test.
- In item 5, add a sentence: "Note: DiseaseController is the only in-solution consumer of IDiseaseService.GetAllDiseasesAsync — verified no other callers via Grep." This documents the breaking change is safe.
- In item 12, explicitly instruct: "Remove the unused `fpsYearContext` substitute or add an inline comment 'kept only for helper-signature symmetry — DiseaseRepository does not consume it'."
- In item 11, add an instruction to remove or rename the now-obsolete GetAllDiseasesAsync_ProjectsOnlyDiseaseName_ExcludesOtherFields test since the service no longer projects strings.
- Drop item #2 entirely (do NOT modify Apha.Common/Contracts/FPS/DiseaseRes.cs). The property-name mismatch is already resolved by the ForMember mapping in item #9 (RequestMapper.cs), which preserves backward compatibility for the 11+ FPSApps consumers of the existing `Disease` property.
- Update the header count to reflect 12 planned items after removing the DiseaseRes modification, matching the reasoning's stated total.
- Align DiseaseController POST result with AccountCategoryController.AddAsync — return `Ok(_mapper.Map<DiseaseRes>(result))` instead of CreatedAtAction, and drop the redundant null-guard on IMapper/IDiseaseService to match the existing FPS controller style.
- No action needed for the Controller/Controllers naming inconsistency — leave AccountCategory's outlier plural folder alone; use singular `Controller/` for DiseaseControllerTest as planned.

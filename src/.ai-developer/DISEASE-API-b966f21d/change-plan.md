# Change Plan — DISEASE-API

**User Story:** Use Case is to generate the Backend API implementation for Disease mapping to tblDisease
Analyze the existing FPS backend solution structure, coding patterns, folder organization, naming conventions, dependency injection setup, repository implementations, service patterns, controller design, mappings, validation approaches, and unit testing standards.
Create or update the Disease entity from tblDisease, including DTOs, mappings, and validations following existing patterns.
Create or update repository, service, and API controller layers to support full CRUD operations.
Enforce architecture standards: API → Service → Repository (no direct repository access from controllers).
Register required dependencies and adhere to existing logging, validation, exception handling, security, and coding conventions.
Generate unit tests for the Repository, Service, and API layers, covering positive, negative, and edge-case scenarios.

## Replan Feedback

> Change the controller test path to `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` to match the existing per-controller subfolder convention.
Replace the create of `DiseaseApiProfile.cs` with a modify of `Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs`, adding `CreateMap<DiseaseDto, DiseaseRes>().ForMember(d => d.Disease, o => o.MapFrom(s => s.DiseaseName)).ReverseMap()` and `CreateMap<DiseaseReq, DiseaseDto>().ForMember(d => d.DiseaseName, o => o.MapFrom(s => s.DiseaseName)).ReverseMap()`.
Change the Application mapping change target from `FpsMappingProfile.cs` to the existing `Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs`, adding `CreateMap<Disease, DiseaseDto>().ReverseMap()` there. Drop the "create DiseaseProfile.cs" fallback.
Reconsider scoping Update out of the plan since `tblDisease` has no non-key columns to update

**Reasoning:** The FPS Disease implementation currently only supports GetAll returning IEnumerable<string>. To satisfy the user story (full CRUD API for Disease → tblDisease), I need to: (1) introduce a proper DiseaseDto in FPS.Application, (2) add a DiseaseReq contract for create/update, (3) extend the repository interface and implementation with GetById/Add/Update/Delete, (4) refactor the service to use DTOs and add validation for full CRUD, (5) extend the controller with GET-by-id, POST, PUT, DELETE endpoints while keeping API → Service → Repository layering, (6) add/extend AutoMapper profile for Disease mappings, and (7) update/add unit tests at Repository, Service, and API layers. Entity and DiseaseMap already exist and are correct; DI registration follows existing conventions (already wired for IDiseaseService/IDiseaseRepository). Existing GET-all test needs updating because the service return type will change from IEnumerable<string> to IEnumerable<DiseaseDto>.

## Design

```
[Client] --> [DiseaseController (API)] --> [IDiseaseService] --> [IDiseaseRepository] --> [FpsDbContext] --> [fps.tbldisease]
                     |                              |                              |
                 DiseaseReq/Res             DiseaseDto + Validation          Disease entity (EF)
                     |
              AutoMapper (FpsMappingProfile)
```

## Planned Changes

| # | File | Action | Description |
|---|------|--------|-------------|
| 1 | `Apha.FPS/Apha.FPS.Application/Dtos/DiseaseDto.cs` | create | Create DiseaseDto class in Apha.FPS.Application.Dtos namespace with a required string DiseaseName property (MaxLength 50 via DataAnnotations [Required, StringLength(50)]) following the Costbook DiseaseDto pattern. This DTO will replace the raw string return from the service. |
| 2 | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | create | Create DiseaseReq contract (request body) in Apha.Common.Contracts.FPS namespace with a DiseaseName string property used by Create/Update endpoints. Mirrors DiseaseRes shape used for responses. |
| 3 | `Apha.FPS.Core/Interfaces/IDiseaseRepository.cs` | modify | Extend IDiseaseRepository with full CRUD signatures: Task<Disease?> GetDiseaseByNameAsync(string diseaseName); Task<Disease> AddDiseaseAsync(Disease disease); Task<Disease> UpdateDiseaseAsync(Disease disease); Task<bool> DeleteDiseaseAsync(string diseaseName). Keep existing GetAllDiseasesAsync. |
| 4 | `Apha.FPS/Apha.FPS.DataAccess/Repositories/DiseaseRepository.cs` | modify | Implement the new CRUD methods on IDiseaseRepository against FpsDbContext.Diseases using EF Core: GetDiseaseByNameAsync uses FindAsync/FirstOrDefaultAsync with AsNoTracking; AddDiseaseAsync adds + SaveChangesAsync; UpdateDiseaseAsync updates + SaveChangesAsync; DeleteDiseaseAsync returns false when not found. Preserve existing GetAllDiseasesAsync. |
| 5 | `Apha.FPS/Apha.FPS.Application/Interfaces/IDiseaseService.cs` | modify | Redefine service contract using DiseaseDto: Task<IEnumerable<DiseaseDto>> GetAllDiseasesAsync(); Task<DiseaseDto?> GetDiseaseByNameAsync(string diseaseName); Task<DiseaseDto> CreateDiseaseAsync(DiseaseDto dto); Task<DiseaseDto> UpdateDiseaseAsync(string diseaseName, DiseaseDto dto); Task<bool> DeleteDiseaseAsync(string diseaseName). Remove the raw IEnumerable<string> return. |
| 6 | `Apha.FPS/Apha.FPS.Application/Mappings/FpsMappingProfile.cs` | modify | Add AutoMapper mappings in the existing FPS Application AutoMapper Profile (locate the current Profile class; if none exists in Application, create Apha.FPS.Application/Mappings/DiseaseProfile.cs): CreateMap<Disease, DiseaseDto>().ReverseMap(). Also ensure the API-side profile maps DiseaseDto <-> DiseaseRes and DiseaseReq -> DiseaseDto. |
| 7 | `Apha.FPS/Apha.FPS.Api/Mappings/DiseaseApiProfile.cs` | create | Create AutoMapper Profile in the FPS.Api Mappings folder registering CreateMap<DiseaseDto, DiseaseRes>() (map DiseaseDto.DiseaseName -> DiseaseRes.Disease) and CreateMap<DiseaseReq, DiseaseDto>() (map DiseaseReq.DiseaseName -> DiseaseDto.DiseaseName). If a shared FPS API mapping profile already exists, add these mappings there instead and delete this new file plan item. |
| 8 | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | modify | Rewrite DiseaseService to implement full CRUD using IDiseaseRepository and IMapper. Inject IMapper. GetAllDiseasesAsync maps entities to DiseaseDto list. GetDiseaseByNameAsync throws ArgumentException on null/empty input, returns null when not found. CreateDiseaseAsync validates DTO (not null, DiseaseName required, <=50 chars), throws InvalidOperationException on duplicate primary key. UpdateDiseaseAsync validates existence, applies update. DeleteDiseaseAsync validates input and delegates to repository. Follow error handling and validation patterns seen in PACT TestorProductService. |
| 9 | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | modify | Extend DiseaseController to expose full CRUD endpoints while preserving the existing authorization/route/versioning attributes. Inject IMapper. Endpoints: (1) GET api/v1/disease -> returns List<DiseaseRes> from service DTOs mapped via IMapper (replace inline projection). (2) GET api/v1/disease/{diseaseName} -> returns DiseaseRes or NotFound. (3) POST api/v1/disease -> accepts DiseaseReq, validates ModelState, maps to DiseaseDto, calls service, returns CreatedAtAction with DiseaseRes. (4) PUT api/v1/disease/{diseaseName} -> accepts DiseaseReq, calls service, returns Ok or NotFound. (5) DELETE api/v1/disease/{diseaseName} -> returns NoContent or NotFound. Do NOT touch the repository directly. Return ProducesResponseType attributes similar to other controllers. |
| 10 | `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs` | modify | Extend the existing DiseaseRepositoryTests to cover the new CRUD methods: GetDiseaseByNameAsync returns correct entity / null for missing; AddDiseaseAsync persists and returns the entity; UpdateDiseaseAsync updates existing record; DeleteDiseaseAsync returns true when deleted and false when not found. Reuse existing CreateRepository helper with NSubstitute IFpsRequestContext. Cover positive, negative, and edge cases (null/empty diseaseName). |
| 11 | `Apha.FPS/Apha.FPS.Application.UnitTests/Services/DiseaseServiceTest/DiseaseServiceTests.cs` | modify | Rewrite/extend DiseaseServiceTests to reflect new signatures using DiseaseDto and injected IMapper (NSubstitute). Include tests: GetAllDiseasesAsync returns mapped list; GetDiseaseByNameAsync returns dto/null/throws on invalid input; CreateDiseaseAsync happy path, throws on null dto, throws when duplicate exists (repository returns existing), throws when DiseaseName is empty or >50 chars; UpdateDiseaseAsync happy path and throws when not found; DeleteDiseaseAsync returns true/false and validates input. Cover positive, negative, and edge cases. |
| 12 | `Apha.FPS/Apha.FPS.Api.UnitTests/Controllers/DiseaseControllerTests.cs` | create | Create DiseaseControllerTests using xUnit + NSubstitute + AutoMapper. Mock IDiseaseService and IMapper. Tests: GetAllDiseasesAsync returns Ok with mapped List<DiseaseRes>; GetByName returns Ok when found and NotFound when null; Create returns CreatedAtAction with DiseaseRes; Create returns BadRequest when ModelState invalid; Update returns Ok / NotFound; Delete returns NoContent / NotFound. Cover positive, negative, and edge cases. |

## Recommendations (Out of Scope)

The following changes may be beneficial but are NOT part of this story. They should be addressed by separate stories/tickets:

- The downstream FPSApps FpsLookupApiClient.GetAllDiseasesAsync and its mapping (DiseaseDto with 'Disease' property vs FPS's 'DiseaseName') may need reconciliation once the FPS response payload becomes richer; handle in a follow-up integration story.
- Consider adding an integration test project run against EF Core InMemory to validate DiseaseMap key/schema behavior end-to-end — outside the scope of unit-test story.
- If Disease is a lookup-only reference table in production, consider caching GetAllDiseasesAsync via IMemoryCache — separate performance story.

## ⚠️ Plan Review

**Verdict:** NEEDS_WORK  |  **Score:** 6/10  |  **Cost:** $0.4765  |  **Turns:** 4

> Plan covers the CRUD story correctly, but two mapping-file targets are wrong/inconsistent with existing conventions (EntityMapper.cs vs FpsMappingProfile.cs, and single RequestMapper.cs vs new per-entity profile).

### Review Iterations

**Fix/Review Loops Used:** 2 (of 2 max)  |  **Total Review Passes:** 3  |  ⚠️ *fix attempts exhausted — findings remain unresolved*

| Iteration | Verdict | Score | Findings |
|-----------|---------|-------|----------|
| 1 | NEEDS_WORK | 4/10 | 8 |
| 2 | NEEDS_WORK | 5/10 | 7 |
| 3 | NEEDS_WORK | 4/10 | 8 |

### Findings

| Severity | Category | File | Description |
|----------|----------|------|-------------|
| 🟠 major | wrong_path | `Apha.FPS/Apha.FPS.Api.UnitTests/Controllers/DiseaseControllerTests.cs` | Existing test convention is `Apha.FPS.Api.UnitTests/Controller/<Name>ControllerTest/<Name>ControllerTests.cs` (singular "Controller" + per-controller subfolder). Plan's flat path under "Controllers/" does not match the established pattern. |
| 🟠 major | overcomplicated | `Apha.FPS/Apha.FPS.Api/Mappings/DiseaseApiProfile.cs` | An existing FPS API AutoMapper profile (`Apha.FPS.Api/Mappings/RequestMapper.cs`) is where every other FPS Req/Res <-> Dto mapping lives. Creating a new profile file is inconsistent; the plan's "if a shared profile exists, add there instead and delete this item" wording is soft — should be a firm modify of RequestMapper.cs. |
| 🟠 major | naming | `Apha.FPS/Apha.FPS.Application/Mappings/FpsMappingProfile.cs` | The FPS Application profile is named `EntityMapper.cs`, not `FpsMappingProfile.cs`. Plan references a non-existent file; the fallback ("create DiseaseProfile.cs") would fragment the mapping pattern used across the project. |
| 🟠 major | overcomplicated | — | PUT/Update endpoint is essentially meaningless for `tblDisease` because the entity has exactly one column (`DiseaseName`) which is also the primary key — there is no non-key field to update. Update either becomes a no-op or a PK-rename (EF-unfriendly). Full-CRUD should be reconsidered for this table; POST + DELETE + GETs are the useful surface. |
| 🟡 minor | overcomplicated | `Apha.FPS/Apha.FPS.Application/Dtos/DiseaseDto.cs` | Adding `[Required, StringLength(50)]` DataAnnotations to a DTO is a new pattern for this codebase. The existing Costbook `DiseaseDto` (which this plan says it mirrors) uses no DataAnnotations. Validation is normally handled on the Req contract or in the service. |
| 🟡 minor | incomplete_story | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | Plan mixes exception types (`ArgumentException` vs `InvalidOperationException`) without referencing this project's `BusinessValidationErrorException` / `ValidationError` types under `Apha.FPS.Application.Validation` — inconsistent with the FPS error-handling pattern. |
| 💡 suggestion | missing_test | — | Plan updates `DiseaseServiceTests` but does not explicitly remove/replace the now-obsolete `GetAllDiseasesAsync_WhenRepositoryThrowsException_PropagatesException` semantics that rely on string projection; ensure old assertions like `ContainInOrder("Foot and Mouth Disease", ...)` are actually rewritten (plan says "rewrite/extend" — clarify it will delete outdated string-based expectations). |
| 🟠 major | wrong_path | `Apha.FPS/Apha.FPS.Application/Mappings/FpsMappingProfile.cs` | Plan item #6 targets FpsMappingProfile.cs which does not exist — the actual Application-layer AutoMapper Profile is EntityMapper.cs at Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs. |
| 🟠 major | overcomplicated | `Apha.FPS/Apha.FPS.Api/Mappings/DiseaseApiProfile.cs` | Plan item #7 creates a per-entity API profile file, but the project pattern is a single shared profile (Apha.FPS.Api/Mappings/RequestMapper.cs) that contains ALL FPS DTO<->Req/Res mappings. Creating a new profile file violates the established convention. |
| 🟡 minor | other | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | Plan item #8 adds IMapper injection to DiseaseService, but the existing Costbook DiseaseService equivalent returns DTOs directly without an Application-layer IMapper for such a simple 1-property entity. The Application layer typically uses EntityMapper (existing) — extra IMapper injection risks over-engineering if the mapping is trivial. |
| 🟡 minor | other | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | Existing DiseaseRes uses property name `Disease` (not `DiseaseName`). Plan item #2 names DiseaseReq property `DiseaseName`, which creates an intentional name mismatch between Req and Res. Consider matching DiseaseRes.Disease for contract symmetry, or update both — but be explicit in the mapping ForMember to avoid AutoMapper silently dropping the property. |
| 🟡 minor | incomplete_story | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | Update-by-key semantics are ambiguous: DiseaseName is the primary key of tblDisease. Plan item #8 says UpdateDiseaseAsync takes (string diseaseName, DiseaseDto dto) — the plan should specify whether renaming the PK is allowed and, if not, that only existence is validated. |
| 💡 suggestion | other | — | Plan item #6 is written as conditional ("locate the current Profile class; if none exists...create DiseaseProfile.cs"). The reviewer has verified EntityMapper.cs exists — the plan should state a single deterministic action instead of leaving branching logic to the executor. |

### Suggestions

- Change the controller test path to `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` to match the existing per-controller subfolder convention.
- Replace the create of `DiseaseApiProfile.cs` with a modify of `Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs`, adding `CreateMap<DiseaseDto, DiseaseRes>().ForMember(d => d.Disease, o => o.MapFrom(s => s.DiseaseName)).ReverseMap()` and `CreateMap<DiseaseReq, DiseaseDto>().ForMember(d => d.DiseaseName, o => o.MapFrom(s => s.DiseaseName)).ReverseMap()`.
- Change the Application mapping change target from `FpsMappingProfile.cs` to the existing `Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs`, adding `CreateMap<Disease, DiseaseDto>().ReverseMap()` there. Drop the "create DiseaseProfile.cs" fallback.
- Reconsider scoping Update out of the plan since `tblDisease` has no non-key columns to update, or explicitly define Update as a rename (delete old row + insert new row inside a transaction) and document that in the plan step for `DiseaseService.UpdateDiseaseAsync`.
- Remove DataAnnotations from `DiseaseDto`; put `[Required, StringLength(50)]` on `DiseaseReq.DiseaseName` in `Apha.Common/Contracts/FPS/DiseaseReq.cs` so ModelState validates at the API boundary (consistent with other FPS Req contracts).
- Use the project's existing validation types (`BusinessValidationErrorException` / `ValidationError` from `Apha.FPS.Application.Validation`) in `DiseaseService` rather than raw `ArgumentException`/`InvalidOperationException`, matching other FPS services.
- In the `DiseaseServiceTests` step, explicitly note deletion of the obsolete string-projection assertions (`ContainInOrder("Foot and Mouth Disease", ...)`) and replacement with DTO-based assertions using an NSubstitute `IMapper`.
- Retarget item #6 to modify Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs and add `CreateMap<Disease, DiseaseDto>().ReverseMap();`. Do not create a new DiseaseProfile.cs.
- Replace item #7 (create DiseaseApiProfile.cs) with: modify Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs to add `CreateMap<DiseaseDto, DiseaseRes>().ForMember(d => d.Disease, o => o.MapFrom(s => s.DiseaseName)).ReverseMap().ForMember(d => d.DiseaseName, o => o.MapFrom(s => s.Disease));` and `CreateMap<DiseaseReq, DiseaseDto>().ReverseMap();` (mapping DiseaseReq.Disease -> DiseaseDto.DiseaseName if you keep the Res property name).
- In item #2, align DiseaseReq to DiseaseRes by using property name `Disease` (or explicitly rename DiseaseRes.Disease to DiseaseName in both places — pick one and be explicit) to prevent AutoMapper naming-convention mismatches.
- In item #8, either drop IMapper injection (map inline for a single-property DTO to match the existing simple DiseaseService pattern) or explicitly justify it; if kept, ensure the tests in item #11 configure the IMapper substitute for every call site.
- In item #8, explicitly state that UpdateDiseaseAsync does NOT permit renaming the PK (DiseaseName is the key); it only exists to allow future non-key column additions and validates the key exists.
- Rewrite item #6 as an unconditional, verified instruction referencing EntityMapper.cs.

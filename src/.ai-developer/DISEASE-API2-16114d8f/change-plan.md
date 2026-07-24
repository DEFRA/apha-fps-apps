# Change Plan — DISEASE-API2

**User Story:** Use Case is to generate the Backend API implementation for Disease mapping to table tblDisease. 
Analyze the existing FPS backend solution structure, coding patterns, folder organization, naming conventions, dependency injection setup, repository implementations, service patterns, controller design, mappings, validation approaches, and unit testing standards.
Create or update the Disease entity from tblDisease, including DTOs, and validations following existing patterns.
For Table defintions refer  "/dbscript/schemas"
For mappings, use existing API AutoMapper profile (ie. RequestMapper.cs) and Application layer profile mapper (ie. "EntityMapper.cs")
Create or update repository, service, and API controller layers to support full CRUD operations.
Enforce architecture standards: API → Service → Repository (no direct repository access from controllers).
Register required dependencies and adhere to existing logging, validation, exception handling, security, and coding conventions.
Generate unit tests for the Repository, Service, and API layers following existing conventions for creating the Test classes in a   subfolder (eg: per-controller subfolder under "Controller"). Unit tests should covers positive, negative, and edge-case scenarios.
**Plan Version:** v2 (replanned 1 time(s))

## Replan Feedback

> Update is not meaningful for a single-PK-column entity. Drop UpdateDiseaseAsync from the plan and document that Disease is add/delete only

**Reasoning:** Applied the user feedback: dropped UpdateDiseaseAsync from the plan. Disease has a single-PK column so update is not semantically meaningful — the entity is now add/delete only. Updated the descriptions of the repository interface, repository implementation, service interface, service, controller, and their tests to remove Update-related work. All other items remain unchanged.

## Design

```
[Client] --> [DiseaseController (API)] --> [IDiseaseService] --> [DiseaseService] --> [IDiseaseRepository] --> [DiseaseRepository] --> [FpsDbContext] --> [tblDisease]
                                    |                                |
                             [RequestMapper]                  [EntityMapper]
                             DiseaseDto<->DiseaseRes/Req      Disease<->DiseaseDto

Note: Disease is add/delete only (single-PK-column entity — Update is not meaningful).
```

## Planned Changes

| # | File | Action | Description |
|---|------|--------|-------------|
| 1 | `Apha.FPS/Apha.FPS.Application/Dtos/DiseaseDto.cs` | create | Create DiseaseDto in FPS.Application layer with a single 'DiseaseName' string property (matching Disease entity). Follow the DTO convention used for other FPS DTOs (public class, non-null init). |
| 2 | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | create | Create request contract DiseaseReq under Apha.Common/Contracts/FPS with property 'Disease' (string, required, MaxLength 50 to match tblDisease.disease column). Add DataAnnotations validation attributes ([Required], [StringLength(50)]) consistent with other FPS Req contracts. |
| 3 | `Apha.FPS/Apha.FPS.Core/Interfaces/IDiseaseRepository.cs` | modify | Extend IDiseaseRepository with add/read/delete signatures only (Disease is single-PK-column so Update is not meaningful and is intentionally omitted): GetAllDiseasesAsync (already exists), GetDiseaseByNameAsync(string diseaseName) returning Disease?, AddDiseaseAsync(Disease entity) returning Disease, DeleteDiseaseAsync(string diseaseName) returning bool. Keep async task signatures aligned with existing FPS repository interface conventions (e.g. IStatusRepository, IAnimalRepository). |
| 4 | `Apha.FPS/Apha.FPS.DataAccess/Repositories/DiseaseRepository.cs` | modify | Implement the newly added add/read/delete methods on DiseaseRepository using FpsDbContext.Diseases (Update is intentionally NOT implemented — Disease has a single-PK column so update is meaningless; consumers should delete + add if they need to rename): GetDiseaseByNameAsync uses AsNoTracking + FirstOrDefaultAsync on DiseaseName; AddDiseaseAsync validates non-null, calls Add + SaveChangesAsync, returns entity; DeleteDiseaseAsync validates non-empty, finds entity, if null returns false, else Remove + SaveChangesAsync returns true. Follow patterns from AnimalRepository/StatusRepository (ArgumentNullException/ArgumentException guards, AsNoTracking on reads). |
| 5 | `Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs` | modify | Add CreateMap<Disease, DiseaseDto>().ReverseMap(); inside the EntityMapper constructor so the service can map between entity and DTO (mirrors Costbook EntityMapper). |
| 6 | `Apha.FPS/Apha.FPS.Application/Interfaces/IDiseaseService.cs` | modify | Change IDiseaseService to return DTOs instead of strings and add add/read/delete signatures (no Update — Disease is add/delete only because DiseaseName is the sole PK column): Task<IEnumerable<DiseaseDto>> GetAllDiseasesAsync(); Task<DiseaseDto?> GetDiseaseByNameAsync(string diseaseName); Task<DiseaseDto> AddDiseaseAsync(DiseaseDto dto); Task<bool> DeleteDiseaseAsync(string diseaseName). Add using for DiseaseDto namespace. |
| 7 | `Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs` | modify | Add mappings in FPS Api RequestMapper: CreateMap<DiseaseDto, DiseaseRes>().ForMember(dest => dest.Disease, opt => opt.MapFrom(src => src.DiseaseName)).ReverseMap(); CreateMap<DiseaseReq, DiseaseDto>().ForMember(dest => dest.DiseaseName, opt => opt.MapFrom(src => src.Disease)).ReverseMap(); Add necessary using statements. |
| 8 | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | modify | Refactor DiseaseService: inject IMapper alongside IDiseaseRepository (constructor). Implement GetAllDiseasesAsync returning IEnumerable<DiseaseDto> via mapper.Map; implement GetDiseaseByNameAsync (validate arg not null/whitespace, call repo, map result); AddDiseaseAsync (validate dto and DiseaseName length ≤ 50, guard duplicates by calling GetDiseaseByNameAsync — throw InvalidOperationException if exists, map dto->entity, call repo.Add, map back); DeleteDiseaseAsync (validate, delegate to repo.Delete). Do NOT implement Update — Disease has only a single PK column so update is not meaningful; document this intent with a comment in the class. Follow validation/exception style used in Costbook ProjectService. |
| 9 | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | modify | Extend DiseaseController with add/read/delete endpoints while keeping architecture (controller -> service -> repo). Disease is intentionally add/delete only (single-PK-column entity — no Update endpoint). Inject IMapper. Endpoints: [HttpGet] GetAllDiseasesAsync -> maps List<DiseaseDto> to List<DiseaseRes>; [HttpGet("{diseaseName}")] GetDiseaseByNameAsync -> 200/404; [HttpPost] CreateDiseaseAsync([FromBody] DiseaseReq req) with ModelState validation -> 201 Created + DiseaseRes; [HttpDelete("{diseaseName}")] DeleteDiseaseAsync -> 204/404. Maintain existing [Authorize] role attributes and route/versioning. Handle service exceptions (ArgumentException -> 400, InvalidOperationException -> 409/404 as appropriate). Follow existing FPS controller patterns. |
| 10 | `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs` | modify | Extend existing DiseaseRepositoryTests with tests for the new add/read/delete methods (no Update tests — Disease is add/delete only) following the file's existing helper style (RepositoryTestHelper.CreateMockDbContext / CreateMockDbSet / SetupSaveChanges). Cover positive cases (found/added/deleted), negative cases (not found returns null/false, null entity throws ArgumentNullException, empty/whitespace name throws ArgumentException), and edge cases (empty collection, duplicate keys). Keep existing GetAllDiseasesAsync tests intact. |
| 11 | `Apha.FPS/Apha.FPS.Application.UnitTests/Services/DiseaseServiceTest/DiseaseServiceTests.cs` | modify | Rewrite/extend DiseaseServiceTests to reflect the new service signature (no Update tests — Disease is add/delete only). Use NSubstitute for IDiseaseRepository and IMapper. Test: GetAllDiseasesAsync returns mapped DTOs; GetDiseaseByNameAsync returns dto/null/throws on invalid input; AddDiseaseAsync happy path, duplicate throws InvalidOperationException, invalid dto throws ArgumentException, length > 50 throws; DeleteDiseaseAsync true/false; positive, negative, and edge scenarios per file convention. |
| 12 | `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` | create | Create new test file DiseaseControllerTests in a per-controller subfolder (Controller/DiseaseControllerTest/) mirroring existing FPS API test convention. Use NSubstitute for IDiseaseService and IMapper. Cover (no Update tests — Disease is add/delete only): GetAll returns Ok with list; GetByName returns Ok(res) or NotFound; Create returns CreatedAtAction with DiseaseRes, 400 on invalid ModelState, 409 on duplicate (InvalidOperationException); Delete returns NoContent/NotFound. Include positive, negative, and edge cases. |

## Recommendations (Out of Scope)

The following changes may be beneficial but are NOT part of this story. They should be addressed by separate stories/tickets:

- Consider migrating other simple lookup endpoints (Status, Contract, Customer) in Apha.FPS to full CRUD following the same pattern if business needs align — out of scope for this story.
- The existing IDiseaseService in Apha.FPS returns IEnumerable<string>; the FPSApps consumers (FpsLookupApiClient) still expect DiseaseDto with a 'Disease' property. Verify downstream FPSApps mapping still resolves correctly after the RequestMapper change; if any inconsistency surfaces, a separate integration follow-up story should address end-to-end contract alignment.
- If a 'rename disease' use case emerges later, implement it as a dedicated Rename operation (delete-old + add-new inside a transaction) rather than a generic Update — separate story.
- Consider adding pagination + filtering to GetAllDiseasesAsync consistent with AnimalRepository if the dataset grows — separate performance story.
- The FPS DiseaseMap uses schema 'fps' vs Costbook's DbConstants.FpsSchemaName; a shared constants cleanup could be tackled as tech-debt.

## ✅ Plan Review

**Verdict:** APPROVE  |  **Score:** 8/10  |  **Cost:** $0.6747  |  **Turns:** 3

> Plan is well-scoped, file paths verified, actions correct, and Update is properly excluded; minor clarifications needed around DI, existing test rewrites, and missing dbscript folder.

### Review Iterations

**Fix/Review Loops Used:** 0 (of 2 max)  |  **Total Review Passes:** 1

| Iteration | Verdict | Score | Findings |
|-----------|---------|-------|----------|
| 1 | APPROVE | 8/10 | 11 |

### Findings

| Severity | Category | File | Description |
|----------|----------|------|-------------|
| 🟡 minor | other | — | User story references "/dbscript/schemas" for table definitions but no such folder exists in the repo — the plan correctly derives the schema (MaxLength 50) from existing DiseaseMap.cs, but this dependency on an absent path is worth acknowledging |
| 🟡 minor | other | `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` | Test project has both `Controller/` (majority) and `Controllers/` (used by AccountCategoryControllerTest) subfolders — plan targets `Controller/` which matches the dominant convention, but verify the .csproj/test discovery includes both |
| 🟡 minor | incomplete_story | — | User story explicitly asks for "full CRUD operations" but plan intentionally omits Update; reasoning (single-column PK) is documented and the user feedback in "Plan Reasoning" confirms this decision — flagged only so the reviewer confirms it's an accepted deviation |
| 💡 suggestion | other | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | Existing GetAllDiseasesAsync currently returns `new DiseaseRes { Disease = d }` inline; when refactoring to use IMapper the plan should ensure the existing endpoint's response shape remains identical (Disease field name preserved) — the RequestMapper mapping `DiseaseDto.DiseaseName -> DiseaseRes.Disease` covers this but be explicit |
| 💡 suggestion | missing_test | `Apha.FPS/Apha.FPS.Application.UnitTests/Dtos/` | Other DTOs have DTO tests (e.g. GradeDtoTests.cs) — a DiseaseDtoTests.cs is not required but would match precedent; not blocking |
| 💡 suggestion | other | — | Plan does not mention updating the existing GetAllDiseasesAsync service test (which asserts `IEnumerable<string>`) — item 11 says "Rewrite/extend" which implicitly covers it, but call out that the existing string-based assertions will need to be replaced with DTO assertions |
| 🟡 minor | other | — | The user story references "/dbscript/schemas" for table definitions but that folder does not exist in the repo; the plan silently relies on the existing `Disease` entity + `DiseaseMap` (tbldisease, column `disease`, MaxLength 50) instead. Acceptable, but should be called out explicitly. |
| 🟡 minor | incomplete_story | — | Plan does not mention verifying/adding DI registration; investigation confirms `IDiseaseService`/`IDiseaseRepository` are already registered in `ServiceCollectionExtension.cs`, so no change is needed — worth stating explicitly to avoid the agent adding duplicates. |
| 💡 suggestion | other | `Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs` | Existing `DiseaseRes.Disease` property already differs from planned `DiseaseDto.DiseaseName`; plan correctly handles it with `ForMember`, but the `DiseaseReq` field name should stay `Disease` (as declared in item 2) so the two `ForMember` mappings mirror each other — confirm the agent does not rename `DiseaseRes.Disease`. |
| 💡 suggestion | other | `Apha.FPS/Apha.FPS.Application.UnitTests/Services/DiseaseServiceTest/DiseaseServiceTests.cs` | Existing service tests assert `IEnumerable<string>` return values (e.g. `ContainInOrder("Foot and Mouth Disease", ...)`). Once `GetAllDiseasesAsync` returns `IEnumerable<DiseaseDto>`, those existing assertions will not compile — plan says "rewrite/extend" but should call out that the four existing test methods must be updated, not left as-is. |
| 💡 suggestion | missing_test | — | No unit tests are planned for the new `DiseaseReq` DataAnnotations validation ([Required], [StringLength(50)]). Other FPS Req contracts don't have dedicated validation tests either, so this matches convention, but mention explicitly that controller ModelState tests cover this indirectly. |

### Suggestions

- Since /dbscript/schemas is unavailable, explicitly note in the plan that tblDisease schema is taken from Apha.FPS.DataAccess/Data/DiseaseMap.cs (column `disease`, MaxLength 50, PK on DiseaseName)
- Verify Apha.FPS.Api.UnitTests.csproj discovers tests under Controller/ (singular) — if not, place DiseaseControllerTests under Controllers/ (plural) to match AccountCategoryControllerTest
- Add an explicit note in item 9 that response payload shape must remain `{ "disease": "..." }` (existing DiseaseRes.Disease) so the mapper rule `DiseaseDto.DiseaseName -> DiseaseRes.Disease` is enforced by an AutoMapper AssertConfigurationIsValid test or an integration test
- In item 11, explicitly state that the existing four GetAllDiseasesAsync tests (which use IEnumerable<string>) must be rewritten to expect IEnumerable<DiseaseDto>
- Optionally add DiseaseDtoTests.cs to Apha.FPS.Application.UnitTests/Dtos/ mirroring GradeDtoTests.cs
- Confirm with product owner that omitting Update is acceptable given the story's "full CRUD" wording; otherwise reintroduce Update via delete+add pattern
- Add a note to item 3/4 stating that `dbscript/schemas` is absent and the schema is being derived from `Apha.FPS.DataAccess/Data/DiseaseMap.cs` (single PK column `disease` varchar(50), schema `fps.tbldisease`).
- Add a short note to the plan (or item 9) that DI for `IDiseaseService`/`IDiseaseRepository` is already registered in `Apha.FPS.Api/Extensions/ServiceCollectionExtension.cs` and must NOT be duplicated.
- In item 7, keep `DiseaseRes.Disease` as-is and only add the two `ForMember` mappings; do not modify `DiseaseRes` itself.
- In item 11, explicitly instruct the agent to delete/rewrite the four existing tests (`GetAllDiseasesAsync_WithValidData_ReturnsDiseaseNameList`, `_WithEmptyList_ReturnsEmptyStringList`, `_ProjectsOnlyDiseaseName_ExcludesOtherFields`, `_WhenRepositoryThrowsException_PropagatesException`) because they assert against `IEnumerable<string>` and will no longer compile after the interface change; also add IMapper mocking to the test constructor.
- In item 12, ensure the test file namespace follows the existing convention `Apha.FPS.Api.UnitTests.Controller.DiseaseControllerTest` (mirroring AnimalControllerTest — note singular "Controller" folder), and mock both `IDiseaseService` and `IMapper` as the AnimalControllerTests example does.

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

**Reasoning:** The FPS Disease feature currently only supports a single read operation (GetAllDiseasesAsync). This story requires extending it to full CRUD following existing FPS patterns (like AnimalRepository/StatusRepository, similar controller/service/repository layering). Plan: add DiseaseDto in FPS.Application, a DiseaseReq contract in Apha.Common, extend the repository/service/controller with Create/Read/Update/Delete, wire up AutoMapper profiles (EntityMapper and RequestMapper), and add matching unit tests per layer under the existing per-target Test subfolder convention. No new NuGet packages are required (AutoMapper is already referenced). The tblDisease table has a single 'disease' column (PK, varchar(50)) so the entity does not need new fields — only new operations.

## Design

```
[Client] --> [DiseaseController (API)] --> [IDiseaseService] --> [DiseaseService] --> [IDiseaseRepository] --> [DiseaseRepository] --> [FpsDbContext] --> [tblDisease]
                                    |                                |
                             [RequestMapper]                  [EntityMapper]
                             DiseaseDto<->DiseaseRes/Req      Disease<->DiseaseDto
```

## Planned Changes

| # | File | Action | Description |
|---|------|--------|-------------|
| 1 | `Apha.FPS/Apha.FPS.Application/Dtos/DiseaseDto.cs` | create | Create DiseaseDto in FPS.Application layer with a single 'DiseaseName' string property (matching Disease entity). Follow the DTO convention used for other FPS DTOs (public class, non-null init). |
| 2 | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | create | Create request contract DiseaseReq under Apha.Common/Contracts/FPS with property 'Disease' (string, required, MaxLength 50 to match tblDisease.disease column). Add DataAnnotations validation attributes ([Required], [StringLength(50)]) consistent with other FPS Req contracts. |
| 3 | `Apha.FPS/Apha.FPS.Core/Interfaces/IDiseaseRepository.cs` | modify | Extend IDiseaseRepository with full CRUD signatures: GetAllDiseasesAsync (already exists), GetDiseaseByNameAsync(string diseaseName) returning Disease?, AddDiseaseAsync(Disease entity) returning Disease, UpdateDiseaseAsync(Disease entity) returning Disease, DeleteDiseaseAsync(string diseaseName) returning bool. Keep async task signatures aligned with existing FPS repository interface conventions (e.g. IStatusRepository, IAnimalRepository). |
| 4 | `Apha.FPS/Apha.FPS.DataAccess/Repositories/DiseaseRepository.cs` | modify | Implement the newly added CRUD methods on DiseaseRepository using FpsDbContext.Diseases: GetDiseaseByNameAsync uses AsNoTracking + FirstOrDefaultAsync on DiseaseName; AddDiseaseAsync validates non-null, calls Add + SaveChangesAsync, returns entity; UpdateDiseaseAsync validates non-null, Update + SaveChangesAsync; DeleteDiseaseAsync validates non-empty, finds entity, if null returns false, else Remove + SaveChangesAsync returns true. Follow patterns from AnimalRepository/StatusRepository (ArgumentNullException/ArgumentException guards, AsNoTracking on reads). |
| 5 | `Apha.FPS/Apha.FPS.Application/Interfaces/IDiseaseService.cs` | modify | Change IDiseaseService to return DTOs instead of strings and add CRUD signatures: Task<IEnumerable<DiseaseDto>> GetAllDiseasesAsync(); Task<DiseaseDto?> GetDiseaseByNameAsync(string diseaseName); Task<DiseaseDto> AddDiseaseAsync(DiseaseDto dto); Task<DiseaseDto> UpdateDiseaseAsync(DiseaseDto dto); Task<bool> DeleteDiseaseAsync(string diseaseName). Add using for DiseaseDto namespace. |
| 6 | `Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs` | modify | Add CreateMap<Disease, DiseaseDto>().ReverseMap(); inside the EntityMapper constructor so the service can map between entity and DTO (mirrors Costbook EntityMapper). |
| 7 | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | modify | Refactor DiseaseService: inject IMapper alongside IDiseaseRepository (constructor). Implement GetAllDiseasesAsync returning IEnumerable<DiseaseDto> via mapper.Map; implement GetDiseaseByNameAsync (validate arg not null/whitespace, call repo, map result); AddDiseaseAsync (validate dto and DiseaseName length ≤ 50, guard duplicates by calling GetDiseaseByNameAsync — throw InvalidOperationException if exists, map dto->entity, call repo.Add, map back); UpdateDiseaseAsync (validate, verify exists, map, repo.Update, map back); DeleteDiseaseAsync (validate, delegate to repo.Delete). Follow validation/exception style used in Costbook ProjectService. |
| 8 | `Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs` | modify | Add mappings in FPS Api RequestMapper: CreateMap<DiseaseDto, DiseaseRes>().ForMember(dest => dest.Disease, opt => opt.MapFrom(src => src.DiseaseName)).ReverseMap(); CreateMap<DiseaseReq, DiseaseDto>().ForMember(dest => dest.DiseaseName, opt => opt.MapFrom(src => src.Disease)).ReverseMap(); Add necessary using statements. |
| 9 | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | modify | Extend DiseaseController with full CRUD endpoints while keeping architecture (controller -> service -> repo). Inject IMapper. Endpoints: [HttpGet] GetAllDiseasesAsync -> maps List<DiseaseDto> to List<DiseaseRes>; [HttpGet("{diseaseName}")] GetDiseaseByNameAsync -> 200/404; [HttpPost] CreateDiseaseAsync([FromBody] DiseaseReq req) with ModelState validation -> 201 Created + DiseaseRes; [HttpPut("{diseaseName}")] UpdateDiseaseAsync -> 200/404; [HttpDelete("{diseaseName}")] DeleteDiseaseAsync -> 204/404. Maintain existing [Authorize] role attributes and route/versioning. Handle service exceptions (ArgumentException -> 400, InvalidOperationException -> 409/404 as appropriate). Follow existing FPS controller patterns. |
| 10 | `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs` | modify | Extend existing DiseaseRepositoryTests with tests for the new CRUD methods following the file's existing helper style (RepositoryTestHelper.CreateMockDbContext / CreateMockDbSet / SetupSaveChanges). Cover positive cases (found/added/updated/deleted), negative cases (not found returns null/false, null entity throws ArgumentNullException, empty/whitespace name throws ArgumentException), and edge cases (empty collection, duplicate keys). Keep existing GetAllDiseasesAsync tests intact. |
| 11 | `Apha.FPS/Apha.FPS.Application.UnitTests/Services/DiseaseServiceTest/DiseaseServiceTests.cs` | modify | Rewrite/extend DiseaseServiceTests to reflect the new service signature. Use NSubstitute for IDiseaseRepository and IMapper. Test: GetAllDiseasesAsync returns mapped DTOs; GetDiseaseByNameAsync returns dto/null/throws on invalid input; AddDiseaseAsync happy path, duplicate throws InvalidOperationException, invalid dto throws ArgumentException, length > 50 throws; UpdateDiseaseAsync happy path, not-found path; DeleteDiseaseAsync true/false; positive, negative, and edge scenarios per file convention. |
| 12 | `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` | create | Create new test file DiseaseControllerTests in a per-controller subfolder (Controller/DiseaseControllerTest/) mirroring existing FPS API test convention. Use NSubstitute for IDiseaseService and IMapper. Cover: GetAll returns Ok with list; GetByName returns Ok(res) or NotFound; Create returns CreatedAtAction with DiseaseRes, 400 on invalid ModelState, 409 on duplicate (InvalidOperationException); Update returns Ok/NotFound; Delete returns NoContent/NotFound. Include positive, negative, and edge cases. |

## Recommendations (Out of Scope)

The following changes may be beneficial but are NOT part of this story. They should be addressed by separate stories/tickets:

- Consider migrating other simple lookup endpoints (Status, Contract, Customer) in Apha.FPS to full CRUD following the same pattern if business needs align — out of scope for this story.
- The existing IDiseaseService in Apha.FPS returns IEnumerable<string>; the FPSApps consumers (FpsLookupApiClient) still expect DiseaseDto with a 'Disease' property. Verify downstream FPSApps mapping still resolves correctly after the RequestMapper change; if any inconsistency surfaces, a separate integration follow-up story should address end-to-end contract alignment.
- Consider adding pagination + filtering to GetAllDiseasesAsync consistent with AnimalRepository if the dataset grows — separate performance story.
- The FPS DiseaseMap uses schema 'fps' vs Costbook's DbConstants.FpsSchemaName; a shared constants cleanup could be tackled as tech-debt.

## ✅ Plan Review

**Verdict:** APPROVE  |  **Score:** 7/10  |  **Cost:** $0.7925  |  **Turns:** 5

> Plan covers scope correctly and file paths/actions are accurate; minor deviations from existing FPS transaction/validation/exception patterns should be tightened before code generation.

### Review Iterations

**Fix/Review Loops Used:** 0 (of 2 max)  |  **Total Review Passes:** 1

| Iteration | Verdict | Score | Findings |
|-----------|---------|-------|----------|
| 1 | APPROVE | 7/10 | 12 |

### Findings

| Severity | Category | File | Description |
|----------|----------|------|-------------|
| 🟠 major | incomplete_story | — | Disease entity has only ONE field (DiseaseName) which is also the primary key — an Update operation is semantically meaningless (no non-key columns to modify). Plan includes UpdateDiseaseAsync but does not address this concern. |
| 🟡 minor | other | `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs` | Plan proposes duplicate check in service via GetDiseaseByNameAsync before Add — this is a race-prone pattern; the tblDisease PK constraint will already enforce uniqueness. Consider relying on DbUpdateException instead, or accept the plan's approach but document that it's best-effort. |
| 🟡 minor | other | `Apha.FPS/Apha.FPS.Application/Interfaces/IDiseaseService.cs` | Changing GetAllDiseasesAsync return type from IEnumerable<string> to IEnumerable<DiseaseDto> is a breaking change to existing callers. Verify no other consumers (e.g., FPSApps clients) rely on the current string signature. |
| 🟡 minor | missing_test | `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` | Action is listed as "create" — verify folder DiseaseControllerTest/ does not already exist. Glob confirms it does not, so "create" is correct — noted for confirmation. |
| 💡 suggestion | other | — | dbscript/schemas path referenced in the user story could not be located in the repo; plan relies on existing entity/DiseaseMap for schema (varchar(50), single PK column) which is a reasonable fallback but worth noting. |
| 💡 suggestion | naming | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | Existing DiseaseRes uses property name `Disease` (not `DiseaseName`); plan correctly mirrors this in DiseaseReq. Ensure the RequestMapper mapping (`Disease` <-> `DiseaseName`) is added on both directions for Req<->Dto (plan step 8 does this). |
| 🟡 minor | naming | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | Existing FPS Req contracts (e.g. AnimalReq.cs, DivisionReq.cs) do NOT use DataAnnotations attributes — adding [Required]/[StringLength] deviates from the current FPS pattern; validation is generally handled in the Service layer for this codebase. |
| 🟡 minor | other | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | Plan proposes catching ArgumentException/InvalidOperationException in the controller to return 400/404/409 — this deviates from existing FPS controller pattern (see AnimalController.cs) where controllers let exceptions propagate to middleware. |
| 🟡 minor | other | `Apha.FPS/Apha.FPS.DataAccess/Repositories/DiseaseRepository.cs` | Plan proposes plain Add/Update/Delete + SaveChangesAsync. Existing FPS pattern (AnimalRepository) wraps writes in Database.CreateExecutionStrategy().ExecuteAsync with BeginTransactionAsync/Commit/Rollback — Disease CRUD should follow the same transactional pattern for consistency. |
| 🟡 minor | naming | `Apha.Common/Contracts/FPS/DiseaseReq.cs` | Existing DiseaseRes uses property name 'Disease' (not 'DiseaseName'); plan correctly aligns DiseaseReq.Disease with DiseaseRes.Disease — noting explicitly so mapper ForMember (DiseaseDto.DiseaseName ↔ Req/Res.Disease) is consistent both directions. |
| 💡 suggestion | incomplete_story | — | dbscript/schemas directory was not found in the repo — plan assumes tblDisease has a single 'disease' varchar(50) column based on the DiseaseMap.cs entity mapping only; unverified against the schema file referenced in the user story. |
| 💡 suggestion | other | `Apha.FPSApps/Apha.FPSApps.Infrastructure/Integrations/FPSApis/Clients/FpsLookupApiClient.cs` | Downstream consumer FpsLookupApiClient.GetAllDiseasesAsync reads List<DiseaseRes> — plan's controller still returns List<DiseaseRes> so contract holds, but this dependency should be noted so the AutoMapper mapping (DiseaseDto.DiseaseName → DiseaseRes.Disease) is verified during implementation. |

### Suggestions

- Reconsider whether Update is meaningful for a single-PK-column entity. Options: (a) drop UpdateDiseaseAsync from the plan and document that Disease is add/delete only, or (b) implement Update as delete-old + add-new inside a transaction and explicitly note this in the service.
- In DiseaseService.AddDiseaseAsync, prefer catching DbUpdateException from the repository to detect duplicates atomically instead of a separate GetDiseaseByNameAsync check; or keep the pre-check but document the TOCTOU limitation.
- Before changing IDiseaseService.GetAllDiseasesAsync signature, run a grep for existing consumers (e.g., in Apha.FPSApps or elsewhere) to ensure no callers break. If callers exist, coordinate the change or keep a compatibility overload.
- Add an explicit note in the plan that dbscript/schemas was not found and that the schema is being derived from DiseaseMap.cs (varchar(50), PK on 'disease' column, table fps.tbldisease).
- Ensure DiseaseReq validation attributes ([Required], [StringLength(50)]) match the DiseaseMap HasMaxLength(50) and the PK not-null constraint.
- Drop [Required]/[StringLength] from DiseaseReq to align with existing FPS Req pattern; keep validation in DiseaseService (ArgumentException on null/whitespace, length > 50) as already planned in item 7.
- In DiseaseController, do not catch service exceptions — let them propagate (matches AnimalController pattern); rely on the FPS global exception middleware to map ArgumentException/InvalidOperationException to appropriate HTTP responses.
- Update repository plan (item 4) to wrap Add/Update/Delete in Database.CreateExecutionStrategy().ExecuteAsync + BeginTransactionAsync/Commit/Rollback, mirroring AnimalRepository.AddAnimalAsync/UpdateAnimalAsync/DeleteAnimalAsync.
- Before generating code, read the actual tblDisease schema file (search for dbscript/schemas via the repo host or confirm with the user) to verify no columns beyond 'disease' exist and no additional PK/uniqueness constraints are required.
- Include an explicit note in item 8 (RequestMapper) that AutoMapper AssertConfigurationIsValid should be added to a unit test to ensure DiseaseReq/DiseaseRes/DiseaseDto mapping does not break the existing FpsLookupApiClient consumer.

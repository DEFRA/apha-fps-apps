# ✅ Code Review Report — DISEASE-API3-97836834

**Jira Story:** DISEASE-API3
**User Story:** Use Case is to generate the Backend API implementation for Disease mapping to table tblDisease. 
Analyze the existing FPS backend solution structure, coding patterns, folder organization, naming conventions, dependency injection setup, repository implementations, service patterns, controller design, mappings, validation approaches, and unit testing standards.
Create or update the Disease entity from tblDisease, including DTOs, and validations following existing patterns.
For Table defintions refer  "dbscript/schemas"
For mappings, use existing API AutoMapper profile (ie. RequestMapper.cs) and Application layer profile mapper (ie. "EntityMapper.cs")
Create or update repository, service, and API controller layers to support CRUD operations except update in this case.
Enforce architecture standards: API → Service → Repository (no direct repository access from controllers).
Register required dependencies and adhere to existing logging, validation, exception handling, security, and coding conventions.
Generate unit tests for the Repository, Service, and API layers following existing conventions for creating the Test classes in a   subfolder (eg: per-controller subfolder under "Controller"). Unit tests should covers positive, negative, and edge-case scenarios.
**Status:** CLEAN
**Issues Found:** 4
**Cost (USD):** 4.2825

**Auto-Fix:** 3 fix pass(es) applied  |  **Issues Remaining:** 0 critical/major

## Files Reviewed

- `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs`
- `Apha.FPS/Apha.FPS.Application/Services/DiseaseService.cs`
- `Apha.FPS/Apha.FPS.Application.UnitTests/Services/DiseaseServiceTest/DiseaseServiceTests.cs`
- `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs`
- `Apha.Common/Contracts/FPS/DiseaseReq.cs`
- `Apha.FPS/Apha.FPS.Api/Mappings/RequestMapper.cs`
- `Apha.FPS/Apha.FPS.Application/Dtos/DiseaseDto.cs`
- `Apha.FPS/Apha.FPS.Application/Mappings/EntityMapper.cs`

## Initial Review (4 issues)

| Severity | File | Category | Description |
|----------|------|----------|-------------|
| 🟡 major | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | logic_error | CreateAsync's CreatedAtAction references nameof(GetAllDiseasesAsync), a parameterless collection endpoint with no route key parameter, unlike the established pattern in ProjectController/StaffJobController which reference a GetById-style action with route-value(s) identifying the created resource (e.g. CreatedAtAction(nameof(GetByIdAsync), new { staffId = ... }, res)). Since DiseaseController has no GetByIdAsync/GetByNameAsync action, the generated Location header will point to the "get all" endpoint and cannot resolve a specific created Disease resource — also, the call omits the routeValues argument entirely (positional overload treats the DiseaseRes as routeValues, not body), so at runtime this binds to the wrong CreatedAtAction overload/behavior. |
| 🔵 minor | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | missing_error_handling | CreateAsync and DeleteAsync do not check ModelState.IsValid before invoking the service, so DataAnnotations validation declared on DiseaseReq (Required/StringLength) is only enforced if [ApiController] auto-validation is relied upon implicitly; there is no explicit guard or documented reliance the way other CRUD create actions in this codebase are consistently structured, making the validation behavior inconsistent/unclear compared to peer controllers. |
| 🔵 minor | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | convention | DeleteAsync route uses [HttpDelete("{diseaseName}")] passing the disease name directly in the URL path; since DiseaseName can be up to 50 characters and may contain spaces/special characters (e.g. "Foot and Mouth Disease"), this requires URL encoding by callers and diverges from other delete-by-code endpoints (e.g. GradeController "{gradeCode}") that use short, URL-safe codes — no encoding guidance or validation is present. |
| 🟡 major | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | logic_error | CreatedAtAction(nameof(GetAllDiseasesAsync), ...) in CreateAsync sets the Location header to the collection-listing action with no route values, instead of pointing at the created resource (e.g., a GetById-style action or Ok() as AccountCategoryController's POST does) — no other FPS controller uses CreatedAtAction against a parameterless "GetAll" action, so this deviates from convention and produces a misleading 201 Location header. |

## Fix Pass 1 (6 issues)

| Severity | File | Category | Description |
|----------|------|----------|-------------|
| 🟡 major | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | wrong_api | DeleteAsync is decorated with plain [HttpDelete] and binds diseaseName via [FromQuery], producing DELETE /api/v{version}/disease?diseaseName=X. This contradicts the documented design ("HTTP DELETE /api/v1/disease/{name}") and diverges from the existing convention used by other FPS controllers (e.g. AccountCategoryController.DeleteAsync uses [HttpDelete("{accShortName}")] with a route parameter). Any client built against the documented route-based contract will get a 404. |
| 🟡 major | `Apha.FPS/Apha.FPS.Application.UnitTests/Services/DiseaseServiceTest/DiseaseServiceTests.cs` | other | No tests exist for GetDiseaseByNameAsync (the only remaining untested public method on IDiseaseService), despite the user story requiring positive/negative/edge-case coverage for all CRUD-except-update operations. Missing cases: found-by-name returns mapped DTO, not-found returns null, null/whitespace name throws ArgumentException. |
| 🟡 major | `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs` | other | No tests exist for GetByNameAsync even though it was added to IDiseaseRepository/DiseaseRepository as part of this change. Missing cases: existing name returns entity, unknown name returns null, and (given the underlying fps.tbldisease.disease column is citext, i.e. case-insensitive at the DB level) no case-sensitivity check corresponding to the one already written for ExistsAsync. |
| 🟡 major | `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` | other | No tests exist for GetByNameAsync, the remaining controller action. Missing cases: found name returns Ok with mapped DiseaseRes, not-found returns/throws KeyNotFoundException (the controller's documented 404 behavior is completely unverified). |
| 🔵 minor | `Apha.FPS/Apha.FPS.DataAccess.UnitTests/Repository/DiseaseRepositoryTest/DiseaseRepositoryTests.cs` | other | ExistsAsync_CaseSensitiveMatch asserts case-sensitive comparison behavior, but fps.tbldisease.disease is declared as citext in dbscript/schemas/01fps/01tables/tbldisease.sql, which PostgreSQL treats as case-insensitive at the database level regardless of the C# `==` operator used in the LINQ predicate. The in-memory mock test passes, but it documents behavior that will not hold against the real database, giving false confidence. |
| 🔵 minor | `Apha.FPS/Apha.FPS.Api/Controllers/DiseaseController.cs` | convention | CreateAsync and DeleteAsync both perform a manual `if (!ModelState.IsValid) return BadRequest(ModelState);` check, which is redundant under [ApiController] (automatic 400 on invalid model binding) and is not used by sibling controllers such as AccountCategoryController. For DeleteAsync specifically, diseaseName has no validation attributes, so the check is dead code that can never evaluate false. |

## Fix Pass 2 (1 issue)

| Severity | File | Category | Description |
|----------|------|----------|-------------|
| 🟡 major | `Apha.FPS/Apha.FPS.Api.UnitTests/Controller/DiseaseControllerTest/DiseaseControllerTests.cs` | logic_error | Test `CreateAsync_ValidReq_ReturnsCreatedAtAction_WithMappedRes` asserts `createdResult.ActionName` equals `nameof(DiseaseController.GetAllDiseasesAsync)`, but `DiseaseController.CreateAsync` actually calls `CreatedAtAction(nameof(GetByNameAsync), ...)`. The assertion will fail at runtime since the real ActionName returned is "GetByNameAsync", not "GetAllDiseasesAsync". |

> ✅ All critical/major issues were automatically resolved by the fix agent.

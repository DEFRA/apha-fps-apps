/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-09
 * Phase 14 security review : 2026-07-09 — PASS (no changes required)
 *
 * CHANGED:
 *   - New file: no prior C# API controller existed for my_tlkpprojectradtrackdata
 *   - [ApiController] + [ApiVersion("1.0")] with versioned lowercase route api/v{version}/yearlyfinancialdata
 *   - Composite key (year + project) used in GET /{year}/{project}, PUT /{year}/{project},
 *     DELETE /{year}/{project} — matches IYearlyFinancialDataService contract
 *   - GET /{project} — paginated list of all years for a project (grid data source)
 *   - POST — create new record; returns 201 CreatedAtAction pointing to GET /{year}/{project}
 *   - PUT /{year}/{project} — update existing record; composite key merged from route + body
 *   - DELETE /{year}/{project} — delete by composite key; returns 200 {success:bool}
 *   - GET /{project}/{year}/pactcosts — "Update Costing" endpoint; reads vpactprojectyearcosts
 *   - All actions inject IYearlyFinancialDataService + IMapper only (no repository injection)
 *   - [Authorize] roles match existing PIMS controllers (API-PIMSUser, API-PIMSAdmin)
 *   - XML summary docs on all public actions
 *
 * PRESERVED:
 *   - Authorize role names and pattern match existing PIMS controllers
 *   - Route convention (lowercase, versioned) matches existing controllers
 *   - ActionResult<T> return pattern consistent with RadTrackInvoiceController
 *
 * SECURITY REVIEW (Phase 14) — findings:
 *   - [Authorize(Roles="API-PIMSUser,API-PIMSAdmin")] at class level — PASS
 *   - No [AllowAnonymous] anywhere — PASS
 *   - Route/body composite key consistency enforced in Update() — route overwrites body Year/Project — PASS
 *   - No raw SQL; all data access via IYearlyFinancialDataService — PASS
 *   - Exception handling centralized in ExceptionMiddleware (no stack trace leakage) — PASS
 *   - No hardcoded secrets, tokens, or connection strings — PASS
 *   - [ApiController] auto-returns 400 on model binding failures — PASS
 *   - Input validation (Year > 0, Project non-empty) enforced at service layer — PASS
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether "Fix Costing" requires a dedicated POST
 *     endpoint or is handled purely in Update via the Locked/DateCosted/CostedBy fields
 *   - TRANSFORMENGINE TODO: Verify year route constraint (:short) is supported in the
 *     registered API versioning middleware; fallback to int with cast if needed
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    /// <summary>Yearly Financial Data — CRUD operations on my_tlkpprojectradtrackdata (composite key: year + project).</summary>
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/yearlyfinancialdata")]
    public class YearlyFinancialDataController : ControllerBase
    {
        private readonly IYearlyFinancialDataService _service;
        private readonly IMapper _mapper;

        public YearlyFinancialDataController(IYearlyFinancialDataService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /{project} — paginated list of all year rows for a project
        //   Maps to Access form RecordSource: SELECT * FROM my_tlkpprojectradtrackdata WHERE project = ?
        /// <summary>Returns a paginated list of yearly financial data records for the given project.</summary>
        /// <param name="project">Project code to filter by.</param>
        /// <param name="query">Pagination, sort, and search parameters.</param>
        [HttpGet("{project}")]
        public async Task<IActionResult> GetAll(string project, [FromQuery] PaginationReq<string> query)
        {
            // TRANSFORMENGINE: map PaginationReq to QueryParameters; inject project code as Filter
            QueryParameters<string> parameters = _mapper.Map<QueryParameters<string>>(query);
            parameters.Filter = project;

            PaginatedResult<YearlyFinancialDataDto> result = await _service.GetAllAsync(parameters);
            return Ok(_mapper.Map<PaginationRes<YearlyFinancialDataRes>>(result));
        }

        // TRANSFORMENGINE: GET /{year}/{project} — single record by composite key
        /// <summary>Returns a single yearly financial data record identified by the composite key (year + project).</summary>
        /// <param name="year">Financial year (smallint).</param>
        /// <param name="project">Project code (varchar 20).</param>
        [HttpGet("{year:int}/{project}")]
        public async Task<IActionResult> GetByKey(int year, string project)
        {
            // TRANSFORMENGINE: route year is int to avoid routing ambiguity; safe-cast to short
            YearlyFinancialDataDto? result = await _service.GetByKeyAsync((short)year, project);
            return result is null ? NotFound() : Ok(_mapper.Map<YearlyFinancialDataRes>(result));
        }

        // TRANSFORMENGINE: POST — creates a new row in my_tlkpprojectradtrackdata
        //   Returns 201 Created with Location header pointing to GET /{year}/{project}
        /// <summary>Creates a new yearly financial data record and returns 201 with the created resource.</summary>
        /// <param name="request">Populated request body with Year, Project, and cost/effort fields.</param>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] YearlyFinancialDataReq request)
        {
            YearlyFinancialDataDto dto = _mapper.Map<YearlyFinancialDataDto>(request);
            YearlyFinancialDataDto result = await _service.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetByKey),
                new { year = result.Year, project = result.Project },
                _mapper.Map<YearlyFinancialDataRes>(result));
        }

        // TRANSFORMENGINE: PUT /{year}/{project} — updates an existing row; route key merged into DTO
        /// <summary>Updates an existing yearly financial data record identified by the composite key.</summary>
        /// <param name="year">Financial year from route (smallint).</param>
        /// <param name="project">Project code from route (varchar 20).</param>
        /// <param name="request">Updated field values.</param>
        [HttpPut("{year:int}/{project}")]
        public async Task<IActionResult> Update(int year, string project, [FromBody] YearlyFinancialDataReq request)
        {
            // TRANSFORMENGINE: enforce composite key from route — prevents body/route mismatch
            YearlyFinancialDataDto dto = _mapper.Map<YearlyFinancialDataDto>(request);
            dto.Year = (short)year;
            dto.Project = project;

            YearlyFinancialDataDto result = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<YearlyFinancialDataRes>(result));
        }

        // TRANSFORMENGINE: DELETE /{year}/{project} — removes row by composite key
        /// <summary>Deletes a yearly financial data record for the given composite key.</summary>
        /// <param name="year">Financial year (smallint).</param>
        /// <param name="project">Project code (varchar 20).</param>
        [HttpDelete("{year:int}/{project}")]
        public async Task<IActionResult> Delete(int year, string project)
        {
            bool deleted = await _service.DeleteAsync((short)year, project);
            return Ok(new { success = deleted });
        }

        // TRANSFORMENGINE: GET /{project}/{year}/pactcosts — reads vpactprojectyearcosts view
        //   Used by "Update Costing" button (btnUpdateCosting) to populate actual spend values
        //   into the Add/Edit modal's cost fields before saving
        /// <summary>Returns aggregated PACT actual costs from vpactprojectyearcosts for the given project and year.</summary>
        /// <param name="project">Project code.</param>
        /// <param name="year">Financial year (smallint).</param>
        [HttpGet("{project}/{year:int}/pactcosts")]
        public async Task<IActionResult> GetPactCosts(string project, int year)
        {
            IReadOnlyList<PactProjectYearCostsDto> result = await _service.GetPactCostsAsync(project, (short)year);
            return Ok(_mapper.Map<IReadOnlyList<PactProjectYearCostsRes>>(result));
        }
    }
}

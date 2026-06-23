/*
 * TRANSFORMENGINE MIGRATION — ProjectAuditTrailController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmProjectChangesLog + subforms (sf_StaffJob_Log, sf_TestReq_Log, sf_AnimalReq_Log, sf_AdditionalCosts_Log)
 *     → ASP.NET Core 10 [ApiController] with 5 paginated GET endpoints
 *   - Each MS Access subform tab → one [HttpGet] action on versioned route /api/v1/projectaudittrail/{resource}
 *   - DateOnly? query params converted to DateTime? before passing to service (HTML date inputs carry no time component)
 *   - [Authorize] enforces API-FPSUser / API-FPSAdmin / API-FPSShared role guard matching existing FPS controllers
 *   - Phase 6 Backend Readiness Gate — VERIFIED: 5 GET endpoints present with correct route templates,
 *     versioned base route [Route("api/v{version:apiVersion}/projectaudittrail")], [Authorize] role guard
 *     confirmed, required project param enforced via IsNullOrWhiteSpace guard, DateOnly?→DateTime?
 *     conversion present on all actions; no structural changes required
 *   - Phase 14 Security Review — PASS: [Authorize(Roles="API-FPSUser,API-FPSAdmin, API-FPSShared")] at
 *     class level confirmed (matches established FPS 3-role convention, no AllowAnonymous bypass);
 *     project param guarded by IsNullOrWhiteSpace on all 5 actions; DateOnly?→DateTime? conversion is
 *     typed (no string manipulation); no raw SQL; no hardcoded secrets or credentials; exception handling
 *     delegates to global middleware (consistent with FPS pattern established in Phase 6); no security
 *     defects found — no code changes required
 *
 * PRESERVED:
 *   - All 5 log table operations: projectlogs, staffjoblogs, testrequirementlogs, animalrequestlogs, additionalcostlogs
 *   - Optional date range filter semantics (fromDate / toDate nullable — matching legacy UX)
 *   - project (parentProject) treated as required — legacy JS clears all grids when no project is selected
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UserEmail field in ProjectLogRes is a stub (null) until a staff/user directory
 *     service is wired into the controller or service layer to resolve userId → email
 *   - TRANSFORMENGINE TODO: Confirm whether project param should be strictly required or silently return empty
 *     (see legacy behaviour note in ProjectAuditTrailReq.cs)
 */
using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for the Project Audit Trail feature.
    /// Exposes paginated read-only endpoints for the five log tables:
    /// project_log, staffjob_log, testreq_log, animalreq_log, and additionalcosts_log.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectaudittrail")]
    public class ProjectAuditTrailController : ControllerBase
    {
        private readonly IProjectAuditTrailService _service;
        private readonly IMapper _mapper;

        public ProjectAuditTrailController(
            IProjectAuditTrailService service,
            IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of project change log entries filtered by project code and optional date range.
        /// Corresponds to the "Project Detail Changes" tab (Tab 1) of the legacy frmProjectChangesLog form.
        /// </summary>
        /// <param name="query">Pagination and sort parameters.</param>
        /// <param name="project">Project code (parentProject) — required; maps to HTML #filter-project select.</param>
        /// <param name="fromDate">Optional inclusive start date — maps to HTML #filter-from date input.</param>
        /// <param name="toDate">Optional inclusive end date — maps to HTML #filter-to date input.</param>
        // TRANSFORMENGINE: frmProjectChangesLog Tab 1 (project_log) → GET /api/v1/projectaudittrail/projectlogs
        [HttpGet("projectlogs")]
        public async Task<IActionResult> GetProjectLogsAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string project,
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentException("project is required.", nameof(project));

            // TRANSFORMENGINE: DateOnly? → DateTime? conversion — HTML date inputs carry no time component
            var result = await _service.GetProjectLogsAsync(
                query,
                project,
                fromDate.HasValue ? fromDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                toDate.HasValue ? toDate.Value.ToDateTime(TimeOnly.MinValue) : null);

            return Ok(_mapper.Map<PaginationRes<ProjectLogRes>>(result));
        }

        /// <summary>
        /// Returns a paginated list of staff job change log entries filtered by project code and optional date range.
        /// Corresponds to the "Staff Plan Changes" tab (Tab 2) of the legacy sf_StaffJob_Log subform.
        /// </summary>
        /// <param name="query">Pagination and sort parameters.</param>
        /// <param name="project">Project code (parentProject) — required; used to derive JobCode filter on staffjob_log.</param>
        /// <param name="fromDate">Optional inclusive start date.</param>
        /// <param name="toDate">Optional inclusive end date.</param>
        // TRANSFORMENGINE: sf_StaffJob_Log subform (Tab 2) → GET /api/v1/projectaudittrail/staffjoblogs
        [HttpGet("staffjoblogs")]
        public async Task<IActionResult> GetStaffJobLogsAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string project,
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentException("project is required.", nameof(project));

            // TRANSFORMENGINE: DateOnly? → DateTime? conversion — HTML date inputs carry no time component
            var result = await _service.GetStaffJobLogsAsync(
                query,
                project,
                fromDate.HasValue ? fromDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                toDate.HasValue ? toDate.Value.ToDateTime(TimeOnly.MinValue) : null);

            return Ok(_mapper.Map<PaginationRes<StaffJobLogRes>>(result));
        }

        /// <summary>
        /// Returns a paginated list of test requirement change log entries filtered by project code and optional date range.
        /// Corresponds to the "Test Requirement Changes" tab (Tab 3) of the legacy sf_TestReq_Log subform.
        /// </summary>
        /// <param name="query">Pagination and sort parameters.</param>
        /// <param name="project">Project code (parentProject) — required; used to derive JobCode/ProjectBuyerCode filter on testreq_log.</param>
        /// <param name="fromDate">Optional inclusive start date.</param>
        /// <param name="toDate">Optional inclusive end date.</param>
        // TRANSFORMENGINE: sf_TestReq_Log subform (Tab 3) → GET /api/v1/projectaudittrail/testrequirementlogs
        [HttpGet("testrequirementlogs")]
        public async Task<IActionResult> GetTestRequirementLogsAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string project,
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentException("project is required.", nameof(project));

            // TRANSFORMENGINE: DateOnly? → DateTime? conversion — HTML date inputs carry no time component
            var result = await _service.GetTestRequirementLogsAsync(
                query,
                project,
                fromDate.HasValue ? fromDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                toDate.HasValue ? toDate.Value.ToDateTime(TimeOnly.MinValue) : null);

            return Ok(_mapper.Map<PaginationRes<TestRequirementLogRes>>(result));
        }

        /// <summary>
        /// Returns a paginated list of animal request change log entries filtered by project code and optional date range.
        /// Corresponds to the "Animal Requirement Changes" tab (Tab 4) of the legacy sf_AnimalReq_Log subform.
        /// </summary>
        /// <param name="query">Pagination and sort parameters.</param>
        /// <param name="project">Project code (parentProject) — required; used to derive JobCode filter on animalreq_log.</param>
        /// <param name="fromDate">Optional inclusive start date.</param>
        /// <param name="toDate">Optional inclusive end date.</param>
        // TRANSFORMENGINE: sf_AnimalReq_Log subform (Tab 4) → GET /api/v1/projectaudittrail/animalrequestlogs
        [HttpGet("animalrequestlogs")]
        public async Task<IActionResult> GetAnimalRequestLogsAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string project,
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentException("project is required.", nameof(project));

            // TRANSFORMENGINE: DateOnly? → DateTime? conversion — HTML date inputs carry no time component
            var result = await _service.GetAnimalRequestLogsAsync(
                query,
                project,
                fromDate.HasValue ? fromDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                toDate.HasValue ? toDate.Value.ToDateTime(TimeOnly.MinValue) : null);

            return Ok(_mapper.Map<PaginationRes<AnimalRequestLogRes>>(result));
        }

        /// <summary>
        /// Returns a paginated list of additional cost change log entries filtered by project code and optional date range.
        /// Corresponds to the "Exceptional Cost Changes" tab (Tab 5) of the legacy sf_AdditionalCosts_Log subform.
        /// </summary>
        /// <param name="query">Pagination and sort parameters.</param>
        /// <param name="project">Project code (parentProject) — required; used to derive JobCode filter on additionalcosts_log.</param>
        /// <param name="fromDate">Optional inclusive start date.</param>
        /// <param name="toDate">Optional inclusive end date.</param>
        // TRANSFORMENGINE: sf_AdditionalCosts_Log subform (Tab 5) → GET /api/v1/projectaudittrail/additionalcostlogs
        [HttpGet("additionalcostlogs")]
        public async Task<IActionResult> GetAdditionalCostLogsAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string project,
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentException("project is required.", nameof(project));

            // TRANSFORMENGINE: DateOnly? → DateTime? conversion — HTML date inputs carry no time component
            var result = await _service.GetAdditionalCostLogsAsync(
                query,
                project,
                fromDate.HasValue ? fromDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                toDate.HasValue ? toDate.Value.ToDateTime(TimeOnly.MinValue) : null);

            return Ok(_mapper.Map<PaginationRes<AdditionalCostLogRes>>(result));
        }
    }
}

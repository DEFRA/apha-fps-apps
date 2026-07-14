/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New [ApiController] created for the DepartmentIncome reporting resource family
 *   - Six GET endpoints derived from frmDeptIncome VBA query types and fnDeptIncome* helpers:
 *       GET /api/v1/department-income/time        — time-based income rows
 *       GET /api/v1/department-income/tests       — test-based income rows
 *       GET /api/v1/department-income/animals     — animal-based income rows
 *       GET /api/v1/department-income/additional  — additional/exceptional cost rows
 *       GET /api/v1/department-income/totals      — per-project pivot totals (all four areas)
 *       GET /api/v1/department-income/periods     — period dropdown lookup (no filter params)
 *   - Versioned lowercase REST route: api/v{version:apiVersion}/department-income
 *   - Injection of IDepartmentIncomeService and IMapper only (no repository injection per rules)
 *   - Exception-driven flow: throws ArgumentException / KeyNotFoundException; caught by ExceptionMiddleware
 *   - fnDeptIncomeMonthFrom / fnDeptIncomeMonthTo VBA default logic delegated to service layer
 *
 * PRESERVED:
 *   - Query parameter names match the transform-plan handoff notes: project, monthFrom, monthTo
 *   - All 6 query types from the original MS Access form retained as separate REST endpoints
 *   - Nullable project allows "all projects" scenario (no filter) — preserved from VBA fnDeptIncomeProject()
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify that [Authorize] roles ("API-FPSUser","API-FPSAdmin","API-FPSShared")
 *     match the actual Azure AD app roles configured for the FPS API application registration
 */

using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for Department Income reporting queries.
    /// Provides read-only reporting endpoints for time, tests, animals,
    /// additional/exceptional costs, totals, and period lookups.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/department-income")]
    public class DepartmentIncomeController : ControllerBase
    {
        private readonly IDepartmentIncomeService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepartmentIncomeController"/> class.
        /// </summary>
        /// <param name="service">The Department Income service.</param>
        /// <param name="mapper">The AutoMapper instance.</param>
        public DepartmentIncomeController(
            IDepartmentIncomeService service,
            IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns time-based department income rows for the specified project and period range.
        /// Month defaults: monthFrom defaults to 1, monthTo defaults to 12 (or monthFrom) when null.
        /// </summary>
        /// <param name="project">Optional project code filter. When null, all projects are returned.</param>
        /// <param name="monthFrom">Optional period-from filter (1–12). Defaults to 1 when null.</param>
        /// <param name="monthTo">Optional period-to filter (1–12). Defaults to 12 (or monthFrom) when null.</param>
        /// <returns>List of time-based income rows.</returns>
        // TRANSFORMENGINE: Maps frmDeptIncome "Time" query type — VBA month default logic applied in service layer
        [HttpGet("time")]
        public async Task<IActionResult> GetTimeAsync(
            [FromQuery] string? project,
            [FromQuery] int? monthFrom,
            [FromQuery] int? monthTo)
        {
            var dtos = await _service.GetTimeIncomeAsync(project, monthFrom, monthTo);
            return Ok(_mapper.Map<List<DepartmentIncomeTimeRes>>(dtos));
        }

        /// <summary>
        /// Returns test-based department income rows for the specified project and period range.
        /// Month defaults: monthFrom defaults to 1, monthTo defaults to 12 (or monthFrom) when null.
        /// </summary>
        /// <param name="project">Optional project code filter. When null, all projects are returned.</param>
        /// <param name="monthFrom">Optional period-from filter (1–12). Defaults to 1 when null.</param>
        /// <param name="monthTo">Optional period-to filter (1–12). Defaults to 12 (or monthFrom) when null.</param>
        /// <returns>List of test-based income rows.</returns>
        // TRANSFORMENGINE: Maps frmDeptIncome "Tests" query type — mirrors qryDeptIncomeTests MS Access query
        [HttpGet("tests")]
        public async Task<IActionResult> GetTestsAsync(
            [FromQuery] string? project,
            [FromQuery] int? monthFrom,
            [FromQuery] int? monthTo)
        {
            var dtos = await _service.GetTestIncomeAsync(project, monthFrom, monthTo);
            return Ok(_mapper.Map<List<DepartmentIncomeTestRes>>(dtos));
        }

        /// <summary>
        /// Returns animal-based department income rows for the specified project and period range.
        /// Month defaults: monthFrom defaults to 1, monthTo defaults to 12 (or monthFrom) when null.
        /// </summary>
        /// <param name="project">Optional project code filter. When null, all projects are returned.</param>
        /// <param name="monthFrom">Optional period-from filter (1–12). Defaults to 1 when null.</param>
        /// <param name="monthTo">Optional period-to filter (1–12). Defaults to 12 (or monthFrom) when null.</param>
        /// <returns>List of animal-based income rows.</returns>
        // TRANSFORMENGINE: Maps frmDeptIncome "Animals" query type — fnAnimalDesc/fnAnimalDays/fnAnimalRate VBA helpers ported to service/repository
        [HttpGet("animals")]
        public async Task<IActionResult> GetAnimalsAsync(
            [FromQuery] string? project,
            [FromQuery] int? monthFrom,
            [FromQuery] int? monthTo)
        {
            var dtos = await _service.GetAnimalIncomeAsync(project, monthFrom, monthTo);
            return Ok(_mapper.Map<List<DepartmentIncomeAnimalRes>>(dtos));
        }

        /// <summary>
        /// Returns additional/exceptional department income rows for the specified project and period range.
        /// Month defaults: monthFrom defaults to 1, monthTo defaults to 12 (or monthFrom) when null.
        /// </summary>
        /// <param name="project">Optional project code filter. When null, all projects are returned.</param>
        /// <param name="monthFrom">Optional period-from filter (1–12). Defaults to 1 when null.</param>
        /// <param name="monthTo">Optional period-to filter (1–12). Defaults to 12 (or monthFrom) when null.</param>
        /// <returns>List of additional/exceptional income rows.</returns>
        // TRANSFORMENGINE: Maps frmDeptIncome "Exceptional/Additional" query type — mirrors qryDeptIncomeExceptional MS Access query
        [HttpGet("additional")]
        public async Task<IActionResult> GetAdditionalAsync(
            [FromQuery] string? project,
            [FromQuery] int? monthFrom,
            [FromQuery] int? monthTo)
        {
            var dtos = await _service.GetAdditionalIncomeAsync(project, monthFrom, monthTo);
            return Ok(_mapper.Map<List<DepartmentIncomeAdditionalRes>>(dtos));
        }

        /// <summary>
        /// Returns per-project pivot totals across all four income areas (Time, Tests, Animals, Project-specifics).
        /// Month defaults: monthFrom defaults to 1, monthTo defaults to 12 (or monthFrom) when null.
        /// </summary>
        /// <param name="project">Optional project code filter. When null, all projects are returned.</param>
        /// <param name="monthFrom">Optional period-from filter (1–12). Defaults to 1 when null.</param>
        /// <param name="monthTo">Optional period-to filter (1–12). Defaults to 12 (or monthFrom) when null.</param>
        /// <returns>List of per-project totals.</returns>
        // TRANSFORMENGINE: Maps frmDeptIncome "Totals" query type — mirrors qryDeptIncomeTotals TRANSFORM/PIVOT MS Access query
        [HttpGet("totals")]
        public async Task<IActionResult> GetTotalsAsync(
            [FromQuery] string? project,
            [FromQuery] int? monthFrom,
            [FromQuery] int? monthTo)
        {
            var dtos = await _service.GetTotalsAsync(project, monthFrom, monthTo);
            return Ok(_mapper.Map<List<DepartmentIncomeTotalsRes>>(dtos));
        }

        /// <summary>
        /// Returns all available fiscal periods for the from/to period dropdown filters.
        /// No filter parameters — always returns the full period list.
        /// </summary>
        /// <returns>List of available fiscal periods.</returns>
        // TRANSFORMENGINE: Lookup endpoint — powers frmDeptIncome pickMonthFrom/pickMonthTo dropdown controls; no pagination needed
        [HttpGet("periods")]
        public async Task<IActionResult> GetPeriodsAsync()
        {
            var dtos = await _service.GetPeriodsAsync();
            return Ok(_mapper.Map<List<PeriodLookupRes>>(dtos));
        }
    }
}

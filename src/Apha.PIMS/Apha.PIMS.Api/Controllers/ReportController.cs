/*
 * TRANSFORMENGINE MIGRATION — ReportController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmReport) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form CRUD operations -> REST endpoints: GET /report, GET /report/{id}, POST /report, PUT /report/{id}, DELETE /report/{id}
 *   - Access DAO data binding -> IReportService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (ReportReq <-> ReportDto <-> ReportRes)
 *
 * PRESERVED:
 *   - All CRUD semantics from the original form operations
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm role requirements match environment-specific access policy for report management
 *
 * PHASE 6 — Backend Readiness Gate (VERIFIED 2026-07-06):
 *   - Route confirmed: [Route("api/v{version:apiVersion}/report")] → base path GET/POST /api/v1/report
 *   - GET /api/v1/report              → GetAll()      — no required params
 *   - GET /api/v1/report/{id:int}     → GetById(id)   — required: id (integer PK, route)
 *   - POST /api/v1/report             → Create(req)   — required: ReportReq body (all writable fields)
 *   - PUT /api/v1/report/{id:int}     → Update(id,req)— required: id (route, authoritative), ReportReq body
 *   - DELETE /api/v1/report/{id:int}  → Delete(id)    — required: id (route)
 *   - Contracts: ReportReq (body), ReportRes (response) — both registered in RequestMapper
 *   - Lookup separation: ReportGroup is a separate lookup endpoint on ReportGroupController; not mixed here
 *   - No pagination required — Reports Tab grid loads full list
 *   - All route parameters are satisfiable from the page context (integer PK from grid row selection)
 */
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/report")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _service;
        private readonly IMapper _mapper;

        public ReportController(IReportService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all reports.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /report (list all reports)
            List<ReportDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<ReportRes>>(result));
        }

        /// <summary>Get a single report by id.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            ReportDto? result = await _service.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(_mapper.Map<ReportRes>(result));
        }

        /// <summary>Create a new report.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportReq request)
        {
            ReportDto dto = _mapper.Map<ReportDto>(request);
            ReportDto created = await _service.CreateAsync(dto);
            ReportRes res = _mapper.Map<ReportRes>(created);
            return CreatedAtAction(nameof(GetById), new { id = res.Id, version = "1.0" }, res);
        }

        /// <summary>Update an existing report.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ReportReq request)
        {
            ReportDto dto = _mapper.Map<ReportDto>(request);
            // TRANSFORMENGINE: Route id is authoritative — set before service call
            dto.Id = id;
            ReportDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<ReportRes>(updated));
        }

        /// <summary>Delete a report by id.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { success = true });
        }
    }
}

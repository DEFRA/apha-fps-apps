/*
 * TRANSFORMENGINE MIGRATION — ReportGroupLinkController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmReportGroupLink) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form operations -> REST endpoints using composite PK (reportid, groupid)
 *   - Routes: GET /reportgrouplink, GET /reportgrouplink/{reportid}, GET /reportgrouplink/{reportid}/{groupid}, POST /reportgrouplink, DELETE /reportgrouplink/{reportid}/{groupid}
 *   - Access DAO data binding -> IReportGroupLinkService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (ReportGroupLinkReq <-> ReportGroupLinkDto <-> ReportGroupLinkRes)
 *
 * PRESERVED:
 *   - Composite PK semantics (reportid + groupid)
 *   - GetByReportId filter endpoint preserved as scoped list
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm composite delete route strategy is acceptable (DELETE /reportgrouplink/{reportid}/{groupid})
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
    [Route("api/v{version:apiVersion}/reportgrouplink")]
    public class ReportGroupLinkController : ControllerBase
    {
        private readonly IReportGroupLinkService _service;
        private readonly IMapper _mapper;

        public ReportGroupLinkController(IReportGroupLinkService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all report group links.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /reportgrouplink (full list)
            List<ReportGroupLinkDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<ReportGroupLinkRes>>(result));
        }

        /// <summary>Get all report group links for a specific report.</summary>
        [HttpGet("{reportid:int}")]
        public async Task<IActionResult> GetByReportId(int reportid)
        {
            List<ReportGroupLinkDto> result = await _service.GetByReportIdAsync(reportid);
            return Ok(_mapper.Map<List<ReportGroupLinkRes>>(result));
        }

        /// <summary>Get a specific report group link by composite key.</summary>
        [HttpGet("{reportid:int}/{groupid:int}")]
        public async Task<IActionResult> GetById(int reportid, int groupid)
        {
            ReportGroupLinkDto? result = await _service.GetByIdAsync(reportid, groupid);
            return result is null ? NotFound() : Ok(_mapper.Map<ReportGroupLinkRes>(result));
        }

        /// <summary>Create a new report group link.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportGroupLinkReq request)
        {
            ReportGroupLinkDto dto = _mapper.Map<ReportGroupLinkDto>(request);
            ReportGroupLinkDto created = await _service.CreateAsync(dto);
            ReportGroupLinkRes res = _mapper.Map<ReportGroupLinkRes>(created);
            return CreatedAtAction(nameof(GetById), new { reportid = res.ReportId, groupid = res.GroupId, version = "1.0" }, res);
        }

        /// <summary>Delete a report group link by composite key.</summary>
        [HttpDelete("{reportid:int}/{groupid:int}")]
        public async Task<IActionResult> Delete(int reportid, int groupid)
        {
            // TRANSFORMENGINE: Composite PK delete — both reportid and groupid required in route
            await _service.DeleteAsync(reportid, groupid);
            return Ok(new { success = true });
        }
    }
}

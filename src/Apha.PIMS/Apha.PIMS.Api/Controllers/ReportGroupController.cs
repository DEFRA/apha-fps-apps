/*
 * TRANSFORMENGINE MIGRATION — ReportGroupController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmReportGroup) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form operations -> REST endpoints: GET /reportgroup, GET /reportgroup/{groupid}, POST /reportgroup, PUT /reportgroup/{groupid}, DELETE /reportgroup/{groupid}
 *   - Access DAO data binding -> IReportGroupService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (ReportGroupReq <-> ReportGroupDto <-> ReportGroupRes)
 *
 * PRESERVED:
 *   - All CRUD semantics from the original form operations
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm role requirements match environment-specific access policy
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
    [Route("api/v{version:apiVersion}/reportgroup")]
    public class ReportGroupController : ControllerBase
    {
        private readonly IReportGroupService _service;
        private readonly IMapper _mapper;

        public ReportGroupController(IReportGroupService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all report groups.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /reportgroup (lookup list)
            List<ReportGroupDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<ReportGroupRes>>(result));
        }

        /// <summary>Get a single report group by groupid.</summary>
        [HttpGet("{groupid:int}")]
        public async Task<IActionResult> GetById(int groupid)
        {
            ReportGroupDto? result = await _service.GetByIdAsync(groupid);
            return result is null ? NotFound() : Ok(_mapper.Map<ReportGroupRes>(result));
        }

        /// <summary>Create a new report group.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportGroupReq request)
        {
            ReportGroupDto dto = _mapper.Map<ReportGroupDto>(request);
            ReportGroupDto created = await _service.CreateAsync(dto);
            ReportGroupRes res = _mapper.Map<ReportGroupRes>(created);
            return CreatedAtAction(nameof(GetById), new { groupid = res.GroupId, version = "1.0" }, res);
        }

        /// <summary>Update an existing report group.</summary>
        [HttpPut("{groupid:int}")]
        public async Task<IActionResult> Update(int groupid, [FromBody] ReportGroupReq request)
        {
            ReportGroupDto dto = _mapper.Map<ReportGroupDto>(request);
            // TRANSFORMENGINE: Route groupid is authoritative — set before service call
            dto.Groupid = groupid;
            ReportGroupDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<ReportGroupRes>(updated));
        }

        /// <summary>Delete a report group by groupid.</summary>
        [HttpDelete("{groupid:int}")]
        public async Task<IActionResult> Delete(int groupid)
        {
            await _service.DeleteAsync(groupid);
            return Ok(new { success = true });
        }
    }
}

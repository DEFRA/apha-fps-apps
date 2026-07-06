/*
 * TRANSFORMENGINE MIGRATION — AccessLevelController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmAccessLevel) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form operations -> REST lookup endpoints with composite PK (systemid int + accesslevelid int)
 *   - Routes: GET /accesslevel, GET /accesslevel/{systemid}, GET /accesslevel/{systemid}/{accesslevelid}, POST /accesslevel, PUT /accesslevel/{systemid}/{accesslevelid}, DELETE /accesslevel/{systemid}/{accesslevelid}
 *   - Access DAO data binding -> IAccessLevelService dependency injection
 *   - Response contracts mapped via AutoMapper (AccessLevelDto <-> AccessLevelRes)
 *   - No AccessLevelReq exists — request body uses AccessLevelRes shape (full resource including IDs)
 *
 * PRESERVED:
 *   - Composite PK semantics (systemid + accesslevelid)
 *   - GetBySystemId scoped list endpoint preserved
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: AccessLevelReq contract does not exist — using AccessLevelRes for create/update body; create dedicated request contract if write semantics differ from read
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
    [Route("api/v{version:apiVersion}/accesslevel")]
    public class AccessLevelController : ControllerBase
    {
        private readonly IAccessLevelService _service;
        private readonly IMapper _mapper;

        public AccessLevelController(IAccessLevelService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all access levels.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /accesslevel (full lookup list)
            List<AccessLevelDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<AccessLevelRes>>(result));
        }

        /// <summary>Get all access levels for a specific system.</summary>
        [HttpGet("{systemid:int}")]
        public async Task<IActionResult> GetBySystemId(int systemid)
        {
            List<AccessLevelDto> result = await _service.GetBySystemIdAsync(systemid);
            return Ok(_mapper.Map<List<AccessLevelRes>>(result));
        }

        /// <summary>Get a specific access level by composite key.</summary>
        [HttpGet("{systemid:int}/{accesslevelid:int}")]
        public async Task<IActionResult> GetById(int systemid, int accesslevelid)
        {
            AccessLevelDto? result = await _service.GetByIdAsync(systemid, accesslevelid);
            return result is null ? NotFound() : Ok(_mapper.Map<AccessLevelRes>(result));
        }

        /// <summary>Create a new access level.</summary>
        // TRANSFORMENGINE TODO: AccessLevelReq does not exist — body uses AccessLevelRes shape as workaround
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AccessLevelRes request)
        {
            AccessLevelDto dto = _mapper.Map<AccessLevelDto>(request);
            AccessLevelDto created = await _service.CreateAsync(dto);
            AccessLevelRes res = _mapper.Map<AccessLevelRes>(created);
            return CreatedAtAction(nameof(GetById), new { systemid = res.SystemId, accesslevelid = res.AccessLevelId, version = "1.0" }, res);
        }

        /// <summary>Update an existing access level.</summary>
        [HttpPut("{systemid:int}/{accesslevelid:int}")]
        public async Task<IActionResult> Update(int systemid, int accesslevelid, [FromBody] AccessLevelRes request)
        {
            AccessLevelDto dto = _mapper.Map<AccessLevelDto>(request);
            // TRANSFORMENGINE: Route composite PK is authoritative — set before service call
            dto.Systemid = systemid;
            dto.Accesslevelid = accesslevelid;
            AccessLevelDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<AccessLevelRes>(updated));
        }

        /// <summary>Delete an access level by composite key.</summary>
        [HttpDelete("{systemid:int}/{accesslevelid:int}")]
        public async Task<IActionResult> Delete(int systemid, int accesslevelid)
        {
            await _service.DeleteAsync(systemid, accesslevelid);
            return Ok(new { success = true });
        }
    }
}

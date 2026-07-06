/*
 * TRANSFORMENGINE MIGRATION — AccessUserLevelController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmAccessUserLevel) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form operations -> REST endpoints using triple composite PK (systemid int + ntlogin string + accesslevelid int)
 *   - Routes: GET /accessuserlevel, GET /accessuserlevel/{systemid}, GET /accessuserlevel/{systemid}/{ntlogin}, GET /accessuserlevel/{systemid}/{ntlogin}/{accesslevelid}, POST /accessuserlevel, DELETE /accessuserlevel/{systemid}/{ntlogin}/{accesslevelid}
 *   - Access DAO data binding -> IAccessUserLevelService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (AccessUserLevelReq <-> AccessUserLevelDto <-> AccessUserLevelRes)
 *   - URL encoding/decoding applied for ntlogin string segment
 *
 * PRESERVED:
 *   - Triple composite PK semantics (systemid + ntlogin + accesslevelid)
 *   - GetBySystemId and GetByUser scoped list endpoints preserved
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm triple composite delete route is acceptable for client consumers
 */
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/accessuserlevel")]
    public class AccessUserLevelController : ControllerBase
    {
        private readonly IAccessUserLevelService _service;
        private readonly IMapper _mapper;

        public AccessUserLevelController(IAccessUserLevelService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all access user levels.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /accessuserlevel (full list)
            List<AccessUserLevelDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<AccessUserLevelRes>>(result));
        }

        /// <summary>Get all access user levels for a specific system.</summary>
        [HttpGet("{systemid:int}")]
        public async Task<IActionResult> GetBySystemId(int systemid)
        {
            List<AccessUserLevelDto> result = await _service.GetBySystemIdAsync(systemid);
            return Ok(_mapper.Map<List<AccessUserLevelRes>>(result));
        }

        /// <summary>Get all access user levels for a specific user within a system.</summary>
        [HttpGet("{systemid:int}/{ntlogin}")]
        public async Task<IActionResult> GetByUser(int systemid, string ntlogin)
        {
            var decodedLogin = HttpUtility.UrlDecode(ntlogin);
            List<AccessUserLevelDto> result = await _service.GetByUserAsync(systemid, decodedLogin);
            return Ok(_mapper.Map<List<AccessUserLevelRes>>(result));
        }

        /// <summary>Get a specific access user level by triple composite key.</summary>
        [HttpGet("{systemid:int}/{ntlogin}/{accesslevelid:int}")]
        public async Task<IActionResult> GetById(int systemid, string ntlogin, int accesslevelid)
        {
            var decodedLogin = HttpUtility.UrlDecode(ntlogin);
            AccessUserLevelDto? result = await _service.GetByIdAsync(systemid, decodedLogin, accesslevelid);
            return result is null ? NotFound() : Ok(_mapper.Map<AccessUserLevelRes>(result));
        }

        /// <summary>Create a new access user level assignment.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AccessUserLevelReq request)
        {
            AccessUserLevelDto dto = _mapper.Map<AccessUserLevelDto>(request);
            AccessUserLevelDto created = await _service.CreateAsync(dto);
            AccessUserLevelRes res = _mapper.Map<AccessUserLevelRes>(created);
            return CreatedAtAction(nameof(GetById), new { systemid = res.SystemId, ntlogin = res.NtLogin, accesslevelid = res.AccessLevelId, version = "1.0" }, res);
        }

        /// <summary>Delete an access user level assignment by triple composite key.</summary>
        [HttpDelete("{systemid:int}/{ntlogin}/{accesslevelid:int}")]
        public async Task<IActionResult> Delete(int systemid, string ntlogin, int accesslevelid)
        {
            // TRANSFORMENGINE: Triple composite PK delete — systemid + URL-decoded ntlogin + accesslevelid
            var decodedLogin = HttpUtility.UrlDecode(ntlogin);
            await _service.DeleteAsync(systemid, decodedLogin, accesslevelid);
            return Ok(new { success = true });
        }
    }
}

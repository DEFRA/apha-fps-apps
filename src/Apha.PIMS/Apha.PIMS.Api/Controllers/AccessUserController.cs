/*
 * TRANSFORMENGINE MIGRATION — AccessUserController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmAccessUser) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form CRUD operations -> REST endpoints using composite PK (systemid int + ntlogin string)
 *   - Routes: GET /accessuser, GET /accessuser/{systemid}, GET /accessuser/{systemid}/{ntlogin}, POST /accessuser, PUT /accessuser/{systemid}/{ntlogin}, DELETE /accessuser/{systemid}/{ntlogin}
 *   - Access DAO data binding -> IAccessUserService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (AccessUserReq <-> AccessUserDto <-> AccessUserRes)
 *   - URL encoding/decoding applied for ntlogin string segment
 *
 * PRESERVED:
 *   - Composite PK semantics (systemid + ntlogin)
 *   - GetBySystemId scoped list endpoint preserved
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm systemid is client-provided vs session-derived — if session-derived, remove from request body and derive from claims/context
 *
 * PHASE 6 — Backend Readiness Gate (VERIFIED 2026-07-06):
 *   - Route confirmed: [Route("api/v{version:apiVersion}/accessuser")] → base path /api/v1/accessuser
 *   - GET /api/v1/accessuser                            → GetAll()            — no required params
 *   - GET /api/v1/accessuser/{systemid:int}             → GetBySystemId(sid)  — required: systemid (route, integer); satisfiable from Admin tab system selector
 *   - GET /api/v1/accessuser/{systemid:int}/{ntlogin}   → GetById(sid,login)  — required: systemid + ntlogin (composite PK, route)
 *   - POST /api/v1/accessuser                           → Create(req)         — required: AccessUserReq body
 *   - PUT /api/v1/accessuser/{systemid:int}/{ntlogin}   → Update(sid,login,req) — required: composite PK (route, authoritative) + AccessUserReq body
 *   - DELETE /api/v1/accessuser/{systemid:int}/{ntlogin}→ Delete(sid,login)   — required: composite PK (route)
 *   - Contracts: AccessUserReq (body), AccessUserRes (response) — both registered in RequestMapper
 *   - Lookup separation: AccessLevel lookup is on AccessLevelController; AccessSystem lookup is on AccessSystemController — CRUD here is independent
 *   - systemId filter: GetBySystemId supports page filtering by PIMS system; all params satisfiable from Admin tab UI controls
 *   - ntlogin URL-encoding: HttpUtility.UrlDecode applied consistently on GetById, Update, Delete
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
    [Route("api/v{version:apiVersion}/accessuser")]
    public class AccessUserController : ControllerBase
    {
        private readonly IAccessUserService _service;
        private readonly IMapper _mapper;

        public AccessUserController(IAccessUserService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all access users.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /accessuser (full list)
            List<AccessUserDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<AccessUserRes>>(result));
        }

        /// <summary>Get all access users for a specific system.</summary>
        [HttpGet("{systemid:int}")]
        public async Task<IActionResult> GetBySystemId(int systemid)
        {
            List<AccessUserDto> result = await _service.GetBySystemIdAsync(systemid);
            return Ok(_mapper.Map<List<AccessUserRes>>(result));
        }

        /// <summary>Get a specific access user by composite key.</summary>
        [HttpGet("{systemid:int}/{ntlogin}")]
        public async Task<IActionResult> GetById(int systemid, string ntlogin)
        {
            var decodedLogin = HttpUtility.UrlDecode(ntlogin);
            AccessUserDto? result = await _service.GetByIdAsync(systemid, decodedLogin);
            return result is null ? NotFound() : Ok(_mapper.Map<AccessUserRes>(result));
        }

        /// <summary>Create a new access user.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AccessUserReq request)
        {
            AccessUserDto dto = _mapper.Map<AccessUserDto>(request);
            AccessUserDto created = await _service.CreateAsync(dto);
            AccessUserRes res = _mapper.Map<AccessUserRes>(created);
            return CreatedAtAction(nameof(GetById), new { systemid = res.SystemId, ntlogin = res.NtLogin, version = "1.0" }, res);
        }

        /// <summary>Update an existing access user.</summary>
        [HttpPut("{systemid:int}/{ntlogin}")]
        public async Task<IActionResult> Update(int systemid, string ntlogin, [FromBody] AccessUserReq request)
        {
            var decodedLogin = HttpUtility.UrlDecode(ntlogin);
            AccessUserDto dto = _mapper.Map<AccessUserDto>(request);
            // TRANSFORMENGINE: Route composite PK is authoritative — set before service call
            dto.Systemid = systemid;
            dto.Ntlogin = decodedLogin;
            AccessUserDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<AccessUserRes>(updated));
        }

        /// <summary>Delete an access user by composite key.</summary>
        [HttpDelete("{systemid:int}/{ntlogin}")]
        public async Task<IActionResult> Delete(int systemid, string ntlogin)
        {
            // TRANSFORMENGINE: Composite PK delete — systemid + URL-decoded ntlogin
            var decodedLogin = HttpUtility.UrlDecode(ntlogin);
            await _service.DeleteAsync(systemid, decodedLogin);
            return Ok(new { success = true });
        }
    }
}

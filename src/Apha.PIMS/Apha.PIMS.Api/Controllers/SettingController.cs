/*
 * TRANSFORMENGINE MIGRATION — SettingController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmSetting) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form read/update operations -> REST endpoints: GET /setting, GET /setting/userupdateable, GET /setting/{id}, PUT /setting/{id}
 *   - No create/delete endpoints: settings are pre-configured rows; only update is allowed
 *   - Access DAO data binding -> ISettingService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (SettingReq <-> SettingDto <-> SettingRes)
 *   - URL encoding/decoding applied for string PK segment
 *
 * PRESERVED:
 *   - Read-only list of all settings and user-updateable-only list
 *   - String PK (setting id) semantics
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether admin-only guard is required on PUT /setting/{id} (currently all PIMSUser+Admin can update)
 *   - TRANSFORMENGINE TODO: Confirm TestSetting environment-conditional editing — review if non-production settings need different access control
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
    [Route("api/v{version:apiVersion}/setting")]
    public class SettingController : ControllerBase
    {
        private readonly ISettingService _service;
        private readonly IMapper _mapper;

        public SettingController(ISettingService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all settings.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /setting (full settings list)
            List<SettingDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<SettingRes>>(result));
        }

        /// <summary>Get all user-updateable settings.</summary>
        [HttpGet("userupdateable")]
        public async Task<IActionResult> GetAllUserUpdateable()
        {
            // TRANSFORMENGINE: GetAllUserUpdateableAsync -> GET /setting/userupdateable (filtered list for user UI)
            List<SettingDto> result = await _service.GetAllUserUpdateableAsync();
            return Ok(_mapper.Map<List<SettingRes>>(result));
        }

        /// <summary>Get a single setting by id.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var decoded = HttpUtility.UrlDecode(id);
            SettingDto? result = await _service.GetByIdAsync(decoded);
            return result is null ? NotFound() : Ok(_mapper.Map<SettingRes>(result));
        }

        /// <summary>Update an existing setting value.</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] SettingReq request)
        {
            // TRANSFORMENGINE TODO: Confirm admin-only guard requirement on update endpoint
            var decoded = HttpUtility.UrlDecode(id);
            SettingDto dto = _mapper.Map<SettingDto>(request);
            // TRANSFORMENGINE: Route id is authoritative — set before service call
            dto.Id = decoded;
            SettingDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<SettingRes>(updated));
        }
    }
}

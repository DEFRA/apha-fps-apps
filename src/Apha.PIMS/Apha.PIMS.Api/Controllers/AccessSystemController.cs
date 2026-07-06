/*
 * TRANSFORMENGINE MIGRATION — AccessSystemController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmAccessSystem) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form read-only operations -> REST lookup endpoints: GET /accesssystem, GET /accesssystem/{systemid}
 *   - Access DAO data binding -> IAccessSystemService dependency injection
 *   - Response contracts mapped via AutoMapper (AccessSystemDto <-> AccessSystemRes)
 *   - Read-only resource: no create/update/delete endpoints (reference data)
 *
 * PRESERVED:
 *   - Integer PK semantics (systemid)
 *   - Lookup-only semantics from the original form
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED: none — fully automated.
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
    [Route("api/v{version:apiVersion}/accesssystem")]
    public class AccessSystemController : ControllerBase
    {
        private readonly IAccessSystemService _service;
        private readonly IMapper _mapper;

        public AccessSystemController(IAccessSystemService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all access systems (lookup list).</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /accesssystem (reference data lookup)
            List<AccessSystemDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<AccessSystemRes>>(result));
        }

        /// <summary>Get a specific access system by systemid.</summary>
        [HttpGet("{systemid:int}")]
        public async Task<IActionResult> GetById(int systemid)
        {
            AccessSystemDto? result = await _service.GetByIdAsync(systemid);
            return result is null ? NotFound() : Ok(_mapper.Map<AccessSystemRes>(result));
        }
    }
}

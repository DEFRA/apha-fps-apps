/*
 * TRANSFORMENGINE MIGRATION — RadTrackProgController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmPIMSMainForm / Programme Tab) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form CRUD operations -> REST endpoints:
 *       GET    /api/v1/radtrackprog            — list all programmes
 *       GET    /api/v1/radtrackprog/{program}  — get single programme by natural PK
 *       POST   /api/v1/radtrackprog            — create new programme
 *       PUT    /api/v1/radtrackprog/{program}  — update existing programme
 *       DELETE /api/v1/radtrackprog/{program}  — delete programme
 *   - Access DAO data binding -> IRadTrackProgService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (RadTrackProgReq <-> RadTrackProgDto <-> RadTrackProgRes)
 *
 * PRESERVED:
 *   - Natural string PK semantics (program varchar(10))
 *   - All CRUD semantics from the original Programme Tab form
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *   - radtrackprog boolean flag and publicationprefix optional field preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm Programme Tab in form maps solely to tblradtrackprog or also tblaccessprograms
 *   - TRANSFORMENGINE TODO: verify publicationprefix varchar(5) max length enforced via validation attribute on RadTrackProgReq
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
    // TRANSFORMENGINE: MS Access Programme Tab CRUD -> versioned REST controller; natural string PK (program varchar(10))
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/radtrackprog")]
    public class RadTrackProgController : ControllerBase
    {
        private readonly IRadTrackProgService _service;
        private readonly IMapper _mapper;

        public RadTrackProgController(IRadTrackProgService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all RadTrack programmes.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /radtrackprog (full list for Programme Tab grid)
            List<RadTrackProgDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<RadTrackProgRes>>(result));
        }

        /// <summary>Get a single RadTrack programme by its natural string PK (program).</summary>
        [HttpGet("{program}")]
        public async Task<IActionResult> GetById(string program)
        {
            RadTrackProgDto? result = await _service.GetByIdAsync(program);
            return result is null ? NotFound() : Ok(_mapper.Map<RadTrackProgRes>(result));
        }

        /// <summary>Create a new RadTrack programme.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RadTrackProgReq request)
        {
            // TRANSFORMENGINE: natural PK create — Program is client-supplied; CreatedAtAction references GET by program
            RadTrackProgDto dto = _mapper.Map<RadTrackProgDto>(request);
            RadTrackProgDto created = await _service.CreateAsync(dto);
            RadTrackProgRes res = _mapper.Map<RadTrackProgRes>(created);
            return CreatedAtAction(nameof(GetById), new { program = res.Program, version = "1.0" }, res);
        }

        /// <summary>Update an existing RadTrack programme.</summary>
        [HttpPut("{program}")]
        public async Task<IActionResult> Update(string program, [FromBody] RadTrackProgReq request)
        {
            RadTrackProgDto dto = _mapper.Map<RadTrackProgDto>(request);
            // TRANSFORMENGINE: Route program is authoritative — set before service call to prevent body/route mismatch
            dto.Program = program;
            RadTrackProgDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<RadTrackProgRes>(updated));
        }

        /// <summary>Delete a RadTrack programme by its natural string PK (program).</summary>
        [HttpDelete("{program}")]
        public async Task<IActionResult> Delete(string program)
        {
            await _service.DeleteAsync(program);
            return Ok(new { success = true });
        }
    }
}

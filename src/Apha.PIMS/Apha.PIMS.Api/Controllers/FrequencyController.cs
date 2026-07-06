/*
 * TRANSFORMENGINE MIGRATION — FrequencyController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmFrequency) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form CRUD operations -> REST endpoints: GET /frequency, GET /frequency/{frequencyid}, POST /frequency, PUT /frequency/{frequencyid}, DELETE /frequency/{frequencyid}
 *   - Access DAO data binding -> IFrequencyService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (FrequencyReq <-> FrequencyDto <-> FrequencyRes)
 *
 * PRESERVED:
 *   - Integer PK semantics (frequencyid)
 *   - All CRUD semantics from the original form
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm integer PK (frequencyid) generation strategy — verify DB identity/sequence vs application-assigned
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
    [Route("api/v{version:apiVersion}/frequency")]
    public class FrequencyController : ControllerBase
    {
        private readonly IFrequencyService _service;
        private readonly IMapper _mapper;

        public FrequencyController(IFrequencyService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all frequencies.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /frequency (full list)
            List<FrequencyDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<FrequencyRes>>(result));
        }

        /// <summary>Get a single frequency by frequencyid.</summary>
        [HttpGet("{frequencyid:int}")]
        public async Task<IActionResult> GetById(int frequencyid)
        {
            FrequencyDto? result = await _service.GetByIdAsync(frequencyid);
            return result is null ? NotFound() : Ok(_mapper.Map<FrequencyRes>(result));
        }

        /// <summary>Create a new frequency.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FrequencyReq request)
        {
            FrequencyDto dto = _mapper.Map<FrequencyDto>(request);
            FrequencyDto created = await _service.CreateAsync(dto);
            FrequencyRes res = _mapper.Map<FrequencyRes>(created);
            return CreatedAtAction(nameof(GetById), new { frequencyid = res.FrequencyId, version = "1.0" }, res);
        }

        /// <summary>Update an existing frequency.</summary>
        [HttpPut("{frequencyid:int}")]
        public async Task<IActionResult> Update(int frequencyid, [FromBody] FrequencyReq request)
        {
            FrequencyDto dto = _mapper.Map<FrequencyDto>(request);
            // TRANSFORMENGINE: Route frequencyid is authoritative — set before service call
            dto.Frequencyid = frequencyid;
            FrequencyDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<FrequencyRes>(updated));
        }

        /// <summary>Delete a frequency by frequencyid.</summary>
        [HttpDelete("{frequencyid:int}")]
        public async Task<IActionResult> Delete(int frequencyid)
        {
            await _service.DeleteAsync(frequencyid);
            return Ok(new { success = true });
        }
    }
}

/*
 * TRANSFORMENGINE MIGRATION — ProgramManagerLinkController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmProgramManagerLink) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form operations -> REST endpoints using composite natural PK (program string + manager string)
 *   - Routes: GET /programmanagerlink, GET /programmanagerlink/{program}, GET /programmanagerlink/{program}/{manager}, POST /programmanagerlink, DELETE /programmanagerlink/{program}/{manager}
 *   - Access DAO data binding -> IProgramManagerLinkService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (ProgramManagerLinkReq <-> ProgramManagerLinkDto <-> ProgramManagerLinkRes)
 *   - URL encoding/decoding applied for string PK segments
 *
 * PRESERVED:
 *   - Composite natural PK semantics (program + manager)
 *   - GetByProgram scoped list endpoint preserved
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm composite natural PK delete route with URL-encoded string segments is acceptable
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
    [Route("api/v{version:apiVersion}/programmanagerlink")]
    public class ProgramManagerLinkController : ControllerBase
    {
        private readonly IProgramManagerLinkService _service;
        private readonly IMapper _mapper;

        public ProgramManagerLinkController(IProgramManagerLinkService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all program manager links.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /programmanagerlink (full list)
            List<ProgramManagerLinkDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<ProgramManagerLinkRes>>(result));
        }

        /// <summary>Get all program manager links for a specific program.</summary>
        [HttpGet("{program}")]
        public async Task<IActionResult> GetByProgram(string program)
        {
            var decoded = HttpUtility.UrlDecode(program);
            List<ProgramManagerLinkDto> result = await _service.GetByProgramAsync(decoded);
            return Ok(_mapper.Map<List<ProgramManagerLinkRes>>(result));
        }

        /// <summary>Get a specific program manager link by composite key.</summary>
        [HttpGet("{program}/{manager}")]
        public async Task<IActionResult> GetById(string program, string manager)
        {
            var decodedProgram = HttpUtility.UrlDecode(program);
            var decodedManager = HttpUtility.UrlDecode(manager);
            ProgramManagerLinkDto? result = await _service.GetByIdAsync(decodedProgram, decodedManager);
            return result is null ? NotFound() : Ok(_mapper.Map<ProgramManagerLinkRes>(result));
        }

        /// <summary>Create a new program manager link.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProgramManagerLinkReq request)
        {
            ProgramManagerLinkDto dto = _mapper.Map<ProgramManagerLinkDto>(request);
            ProgramManagerLinkDto created = await _service.CreateAsync(dto);
            ProgramManagerLinkRes res = _mapper.Map<ProgramManagerLinkRes>(created);
            return CreatedAtAction(nameof(GetById), new { program = res.Program, manager = res.Manager, version = "1.0" }, res);
        }

        /// <summary>Delete a program manager link by composite key.</summary>
        [HttpDelete("{program}/{manager}")]
        public async Task<IActionResult> Delete(string program, string manager)
        {
            // TRANSFORMENGINE: Composite natural PK delete — both URL-decoded string segments required
            var decodedProgram = HttpUtility.UrlDecode(program);
            var decodedManager = HttpUtility.UrlDecode(manager);
            await _service.DeleteAsync(decodedProgram, decodedManager);
            return Ok(new { success = true });
        }
    }
}

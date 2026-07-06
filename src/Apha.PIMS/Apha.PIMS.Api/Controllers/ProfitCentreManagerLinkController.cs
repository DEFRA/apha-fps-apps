/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreManagerLinkController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmProfitCentreManagerLink) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form operations -> REST endpoints using composite natural PK (profitcentre string + manager string)
 *   - Routes: GET /profitcentremanagerlink, GET /profitcentremanagerlink/{profitcentre}, GET /profitcentremanagerlink/{profitcentre}/{manager}, POST /profitcentremanagerlink, DELETE /profitcentremanagerlink/{profitcentre}/{manager}
 *   - Access DAO data binding -> IProfitCentreManagerLinkService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (ProfitCentreManagerLinkReq <-> ProfitCentreManagerLinkDto <-> ProfitCentreManagerLinkRes)
 *   - URL encoding/decoding applied for string PK segments
 *
 * PRESERVED:
 *   - Composite natural PK semantics (profitcentre + manager)
 *   - GetByProfitCentre scoped list endpoint preserved
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
    [Route("api/v{version:apiVersion}/profitcentremanagerlink")]
    public class ProfitCentreManagerLinkController : ControllerBase
    {
        private readonly IProfitCentreManagerLinkService _service;
        private readonly IMapper _mapper;

        public ProfitCentreManagerLinkController(IProfitCentreManagerLinkService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all profit centre manager links.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /profitcentremanagerlink (full list)
            List<ProfitCentreManagerLinkDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<ProfitCentreManagerLinkRes>>(result));
        }

        /// <summary>Get all profit centre manager links for a specific profit centre.</summary>
        [HttpGet("{profitcentre}")]
        public async Task<IActionResult> GetByProfitCentre(string profitcentre)
        {
            var decoded = HttpUtility.UrlDecode(profitcentre);
            List<ProfitCentreManagerLinkDto> result = await _service.GetByProfitCentreAsync(decoded);
            return Ok(_mapper.Map<List<ProfitCentreManagerLinkRes>>(result));
        }

        /// <summary>Get a specific profit centre manager link by composite key.</summary>
        [HttpGet("{profitcentre}/{manager}")]
        public async Task<IActionResult> GetById(string profitcentre, string manager)
        {
            var decodedProfitCentre = HttpUtility.UrlDecode(profitcentre);
            var decodedManager = HttpUtility.UrlDecode(manager);
            ProfitCentreManagerLinkDto? result = await _service.GetByIdAsync(decodedProfitCentre, decodedManager);
            return result is null ? NotFound() : Ok(_mapper.Map<ProfitCentreManagerLinkRes>(result));
        }

        /// <summary>Create a new profit centre manager link.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProfitCentreManagerLinkReq request)
        {
            ProfitCentreManagerLinkDto dto = _mapper.Map<ProfitCentreManagerLinkDto>(request);
            ProfitCentreManagerLinkDto created = await _service.CreateAsync(dto);
            ProfitCentreManagerLinkRes res = _mapper.Map<ProfitCentreManagerLinkRes>(created);
            return CreatedAtAction(nameof(GetById), new { profitcentre = res.ProfitCentre, manager = res.Manager, version = "1.0" }, res);
        }

        /// <summary>Delete a profit centre manager link by composite key.</summary>
        [HttpDelete("{profitcentre}/{manager}")]
        public async Task<IActionResult> Delete(string profitcentre, string manager)
        {
            // TRANSFORMENGINE: Composite natural PK delete — both URL-decoded string segments required
            var decodedProfitCentre = HttpUtility.UrlDecode(profitcentre);
            var decodedManager = HttpUtility.UrlDecode(manager);
            await _service.DeleteAsync(decodedProfitCentre, decodedManager);
            return Ok(new { success = true });
        }
    }
}

/*
 * TRANSFORMENGINE MIGRATION — ProjectManagerController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmProjectManager) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form CRUD operations -> REST endpoints with natural varchar PK (projectmanager name string)
 *   - Routes: GET /projectmanager, GET /projectmanager/{projectmanager}, POST /projectmanager, PUT /projectmanager/{projectmanager}, DELETE /projectmanager/{projectmanager}
 *   - Access DAO data binding -> IProjectManagerService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (ProjectManagerReq <-> ProjectManagerDto <-> ProjectManagerRes)
 *   - URL encoding/decoding applied for string PK segments
 *
 * PRESERVED:
 *   - Natural varchar PK semantics (projectmanager name as identifier)
 *   - All CRUD operations from the original form
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm rename scenario (changing projectmanager name string) — currently handled as delete+create; review if update-in-place is needed
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
    [Route("api/v{version:apiVersion}/projectmanager")]
    public class ProjectManagerController : ControllerBase
    {
        private readonly IProjectManagerService _service;
        private readonly IMapper _mapper;

        public ProjectManagerController(IProjectManagerService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all project managers.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /projectmanager (full list)
            List<ProjectManagerDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<ProjectManagerRes>>(result));
        }

        /// <summary>Get a single project manager by name.</summary>
        [HttpGet("{projectmanager}")]
        public async Task<IActionResult> GetById(string projectmanager)
        {
            // TRANSFORMENGINE: URL-decode natural varchar PK before lookup
            var decoded = HttpUtility.UrlDecode(projectmanager);
            ProjectManagerDto? result = await _service.GetByIdAsync(decoded);
            return result is null ? NotFound() : Ok(_mapper.Map<ProjectManagerRes>(result));
        }

        /// <summary>Create a new project manager.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjectManagerReq request)
        {
            ProjectManagerDto dto = _mapper.Map<ProjectManagerDto>(request);
            ProjectManagerDto created = await _service.CreateAsync(dto);
            ProjectManagerRes res = _mapper.Map<ProjectManagerRes>(created);
            return CreatedAtAction(nameof(GetById), new { projectmanager = res.ProjectManager, version = "1.0" }, res);
        }

        /// <summary>Update an existing project manager.</summary>
        [HttpPut("{projectmanager}")]
        public async Task<IActionResult> Update(string projectmanager, [FromBody] ProjectManagerReq request)
        {
            var decoded = HttpUtility.UrlDecode(projectmanager);
            ProjectManagerDto dto = _mapper.Map<ProjectManagerDto>(request);
            // TRANSFORMENGINE: Route projectmanager is authoritative — set before service call
            dto.Projectmanager = decoded;
            ProjectManagerDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<ProjectManagerRes>(updated));
        }

        /// <summary>Delete a project manager by name.</summary>
        [HttpDelete("{projectmanager}")]
        public async Task<IActionResult> Delete(string projectmanager)
        {
            var decoded = HttpUtility.UrlDecode(projectmanager);
            await _service.DeleteAsync(decoded);
            return Ok(new { success = true });
        }
    }
}

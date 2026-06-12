using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for Workgroup lookup operations.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/workgroups")]
    public class WorkgroupController : ControllerBase
    {
        private readonly IWorkgroupService _workgroupService;

        public WorkgroupController(IWorkgroupService workgroupService)
        {
            _workgroupService = workgroupService ?? throw new ArgumentNullException(nameof(workgroupService));
        }

        /// <summary>Returns all Workgroup names for dropdown population.</summary>
        [HttpGet("names")]
        public async Task<ActionResult<List<string>>> GetAllWorkgroupNamesAsync()
        {
            var result = await _workgroupService.GetAllWorkgroupNamesAsync();
            return Ok(result);
        }
    }
}

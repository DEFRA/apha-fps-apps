using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for the Work Group section in the Generic Bid feature.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/workgroups")]
    public class WorkGroupController : ControllerBase
    {
        private readonly IWorkGroupService _service;

        public WorkGroupController(IWorkGroupService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Returns workgroups for a given profit centre.
        /// </summary>
        /// <param name="profitCentre">The profit centre identifier.</param>
        /// <returns>List of workgroups.</returns>
        [HttpGet]
        public async Task<IActionResult> GetWorkGroupsAsync([FromQuery] string profitCentre)
        {
            var result = await _service.GetWorkGroupsAsync(profitCentre);
            return Ok(result);
        }
    }
}

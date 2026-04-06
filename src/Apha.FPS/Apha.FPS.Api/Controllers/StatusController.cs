using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [Route("api/v{version:apiVersion}/status")]
    [ApiController]
    [ApiVersion("1.0")]
    public class StatusController : ControllerBase
    {
        private readonly IStatusService _statusService;

        public StatusController(IStatusService statusService)
        {
            _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        }

        [HttpGet]
        public async Task<ActionResult<List<StatusRes>>> GetAllStatusesAsync()
        {
            var statuses = await _statusService.GetAllStatusesAsync();
            return Ok(statuses.Select(s => new StatusRes { Status = s }).ToList());
        }
    }
}

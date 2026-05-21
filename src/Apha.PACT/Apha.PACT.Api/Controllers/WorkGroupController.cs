using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/workgroup")]
    public class WorkGroupController : ControllerBase
    {
        private readonly IWorkGroupService _service;
        private readonly IMapper _mapper;

        public WorkGroupController(IWorkGroupService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all work groups across every profit centre, unfiltered and unpaged.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllWorkGroupsAsync();
            return Ok(_mapper.Map<IEnumerable<WorkGroupRes>>(items));
        }

        /// <summary>
        /// Returns a paged, filtered, and sorted list of work groups for the specified profit centre.
        /// Pagination, sort column, sort direction, and column filters are supplied via
        /// <paramref name="query"/>; the target profit centre is supplied via <paramref name="profitCentre"/>.
        /// </summary>
        /// <param name="query">Pagination and filter parameters forwarded from the DataGrid client.</param>
        /// <param name="profitCentre">The profit-centre code used to scope the work-group query.</param>
        [HttpGet("profitcentre")]
        public async Task<IActionResult> GetWorkGroupsByProfitCentre(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string profitCentre)
        {
            var result = await _service.GetWorkGroupsByProfitCentreAsync(query, profitCentre);
            return Ok(_mapper.Map<PaginationRes<WorkGroupRes>>(result));
        }

        /// <summary>
        /// Sets or clears the <c>SendEmail</c> flag for all work groups belonging to the specified
        /// profit centre. Pass <c>SendEmail = 1</c> to flag for sending, or <c>0</c> to clear.
        /// </summary>
        /// <param name="request">Contains the target profit-centre code and the desired flag value.</param>
        [HttpPut("setsendemail/profitcentre")]
        public async Task<IActionResult> SetSendEmailForProfitCentreWorkGroupsAsync([FromBody] UpdateSendEmailFlagReq request)
        {
            if (string.IsNullOrWhiteSpace(request.ProfitCentre))
                return BadRequest("ProfitCentre is required.");

            var success = await _service.SetSendEmailForProfitCentreWorkGroupsAsync(request.ProfitCentre, request.SendEmail);
            return Ok(success);
        }

        /// <summary>
        /// Clears the <c>SendEmail</c> flag (sets to <c>0</c>) for every work group across all
        /// profit centres. Typically called before a fresh selection is made.
        /// </summary>
        /// <param name="request">Contains the flag value to apply (expected <c>0</c> to clear).</param>
        [HttpPut("setsendemail/all")]
        public async Task<IActionResult> SetSendEmailForAllWorkGroupsAsync([FromBody] UpdateSendEmailFlagReq request)
        {
            var success = await _service.SetSendEmailForAllWorkGroupsAsync(request.SendEmail);
            return Ok(success);
        }

        /// <summary>
        /// Updates the <c>SendEmail</c> flag and <c>EmailRecipient</c> for a single work group
        /// identified by <paramref name="workGroupName"/>.
        /// </summary>
        /// <param name="workGroupName">The unique name of the work group to update.</param>
        /// <param name="request">Contains the new send-email flag value and optional email recipient address.</param>
        [HttpPut("{workGroupName}/email")]
        public async Task<IActionResult> UpdateWorkGroupEmail(
            string workGroupName,
            [FromBody] UpdateWorkGroupEmailReq request)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return BadRequest("WorkGroupName is required.");

            var success = await _service.UpdateWorkGroupEmailAsync(workGroupName, request.SendEmail, request.EmailRecipient);
            return Ok(success);
        }
    }
}

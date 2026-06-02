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
    [Route("api/v{version:apiVersion}/recreatereleasesummary")]
    public class RecreateAndReleaseSummaryController : ControllerBase
    {
        private readonly IRecreateAndReleaseSummaryService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="RecreateAndReleaseSummaryController"/> with the required
        /// service and AutoMapper dependencies.
        /// </summary>
        /// <param name="service">Application service used to retrieve recreate summaries log and release summary data.</param>
        /// <param name="mapper">AutoMapper instance used to project application DTOs to API response contracts.</param>
        public RecreateAndReleaseSummaryController(IRecreateAndReleaseSummaryService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all recreate summaries logs from the system with pagination and sorting support.
        /// </summary>
        /// <param name="query">Pagination and sorting parameters from FPSApps client.</param>
        /// <returns>
        /// <c>200 OK</c> with a <see cref="PaginationRes{T}"/> containing the logs and pagination metadata.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetRecreateSummariesAllLogs([FromQuery] QueryParameters<string> query)
        {
            var result = await _service.GetRecreateSummariesAllLogsAsync(query);
            return Ok(_mapper.Map<PaginationRes<RecreateSummariesLogRes>>(result));
        }

        /// <summary>
        /// Retrieves all release summary periods from <c>tblPeriod</c>.
        /// </summary>
        /// <returns><c>200 OK</c> with a list of <see cref="ReleasePeriodRes"/>.</returns>
        [HttpGet("/api/v{version:apiVersion}/releasesummary")]
        public async Task<IActionResult> GetReleaseSummaries()
        {
            var result = await _service.GetReleaseSummariesAsync();
            return Ok(_mapper.Map<IReadOnlyList<ReleasePeriodRes>>(result));
        }

        /// <summary>
        /// Updates the <c>FinalSummariesRun</c> flag for the specified release period.
        /// </summary>
        /// <param name="request">The period name and new flag value.</param>
        /// <returns><c>200 OK</c> with the updated <see cref="ReleasePeriodRes"/> on success; <c>404 Not Found</c> if the period does not exist.</returns>
        [HttpPut("/api/v{version:apiVersion}/releasesummary/finalrun")]
        public async Task<IActionResult> SetFinalSummaryRun([FromBody] ReleasePeriodReq request)
        {
            var result = await _service.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun);
           
            return Ok(_mapper.Map<ReleasePeriodRes>(result));
        }
    }
}

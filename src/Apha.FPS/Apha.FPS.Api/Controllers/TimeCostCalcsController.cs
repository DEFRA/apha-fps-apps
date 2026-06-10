using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for Time Cost Calculations data.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/timecostcalcs")]
    public class TimeCostCalcsController : ControllerBase
    {
        private readonly ITimeCostCalcsService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeCostCalcsController"/> class.
        /// </summary>
        /// <param name="service">Service for actual staff time data.</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
        public TimeCostCalcsController(ITimeCostCalcsService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated list of actual staff time records for a given project.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="projectCode">The project code to filter by.</param>
        /// <returns>Paginated list of actual staff time results.</returns>
        [HttpGet]
        public async Task<IActionResult> GetTimeCostCalcsByProjectAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                throw new ArgumentException("projectCode is required.");

            var result = await _service.GetTimeCostCalcsByProjectAsync(query, projectCode);
            return Ok(_mapper.Map<PaginationRes<TimeCostCalcsViewRes>>(result));
        }

        /// <summary>
        /// Retrieves total actual hours and cost for a given project.
        /// </summary>
        /// <param name="projectCode">The project code to filter by.</param>
        /// <returns>Total actual hours and cost.</returns>
        [HttpGet("totals")]
        public async Task<IActionResult> GetTotalActualByProjectAsync([FromQuery] string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                throw new ArgumentException("projectCode is required.");

            var result = await _service.GetTotalActualByProjectAsync(projectCode);
            return Ok(_mapper.Map<TimeCostCalcsTotalsRes>(result));
        }

        /// <summary>
        /// Deletes a single time cost record by its composite key.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteTimeCostCalcsAsync([FromBody] TimeCostCalcsReq req)
        {
            if (req == null)
                throw new ArgumentException("Request body is required.");

            if (string.IsNullOrWhiteSpace(req.WorkGroup) || string.IsNullOrWhiteSpace(req.JobCode)
                || string.IsNullOrWhiteSpace(req.Project) || string.IsNullOrWhiteSpace(req.StaffId))
                throw new ArgumentException("workgroup, jobCode, project and staffId are required.");

            var deleted = await _service.DeleteTimeCostCalcsAsync(req.WorkGroup, req.JobCode, req.Project, req.Month, req.StaffId);
            if (!deleted)
                throw new KeyNotFoundException("Record not found.");

            return Ok(deleted);
        }
    }
}

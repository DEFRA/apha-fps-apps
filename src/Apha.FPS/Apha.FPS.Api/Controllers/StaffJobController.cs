using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for managing staff job assignments and related data.
    /// </summary>
    /// 
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [Route("api/staffjob")]
    public class StaffJobController : ControllerBase
    {
        private readonly IStaffJobService _staffJobService;
        private readonly IMapper _mapper;
        /// <summary>
        /// Initializes a new instance of the <see cref="StaffJobController"/> class.
        /// </summary>
        /// <param name="staffJobService">Service for staff job operations.</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
        public StaffJobController(
                        IStaffJobService staffJobService,
                        IMapper mapper)
        {
            _staffJobService = staffJobService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated list of staff job costs.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <returns>Paginated list of staff job cost view results.</returns>
        [HttpGet]
        public async Task<IActionResult> GetJobStaffCostAsync([FromQuery] PaginationReq<string> query)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _staffJobService.GetJobStaffCostAsync(filter);
            return Ok(_mapper.Map<PaginationRes<StaffJobViewRes>>(result));
        }

        /// <summary>
        /// Retrieves a lookup list of staff workgroups.
        /// </summary>
        /// <returns>List of staff workgroup lookup results.</returns>
        [HttpGet("workgrouplookup")]
        public async Task<IActionResult> GetStaffWorkgroupLookup()
        {
            var result = await _staffJobService.GetStaffWorkgroupLookup();
            return Ok(_mapper.Map<List<StaffWorkgroupLookupRes>>(result));
        }

        /// <summary>
        /// Retrieves the charge rate for a specific staff member and job code.
        /// </summary>
        /// <param name="staffId">The staff member's identifier.</param>
        /// <param name="jobcode">The job code.</param>
        /// <returns>The charge rate as a decimal value.</returns>
        [HttpGet("chargerate")]
        public async Task<IActionResult> GetStaffChargeRate([FromQuery] string staffId, [FromQuery] string jobcode)
        {
            var chargeRate = await _staffJobService.GetStaffChargeRate(staffId, jobcode);
            return Ok(chargeRate);
        }

        /// <summary>
        /// Retrieves a staff job assignment by staff ID and job code.
        /// </summary>
        /// <param name="staffId">The staff member's identifier.</param>
        /// <param name="jobCode">The job code.</param>
        /// <returns>The staff job assignment details.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the staff job is not found.</exception>
        [HttpGet("{staffId}/{jobCode}")]
        public async Task<IActionResult> GetByIdAsync(string staffId, string jobCode)
        {
            var result = await _staffJobService.GetByIdAsync(staffId, jobCode);
            if (result == null)
                throw new KeyNotFoundException("Data not found.");
            return Ok(_mapper.Map<StaffJobRes>(result));
        }

        /// <summary>
        /// Adds a new staff job assignment.
        /// </summary>
        /// <param name="staffJobReq">The staff job request data.</param>
        /// <returns>The created staff job assignment.</returns>
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] StaffJobReq staffJobReq)
        {
            var staffJobDto = _mapper.Map<StaffJobDto>(staffJobReq);
            var result = await _staffJobService.AddAsync(staffJobDto);
            return CreatedAtAction(nameof(GetByIdAsync), new { staffId = result.StaffId, jobCode = result.JobCode }, _mapper.Map<StaffJobRes>(result));
        }

        /// Updates an existing staff job assignment.
        /// </summary>
        /// <param name="staffJobReq">The staff job request data.</param>
        /// <returns>The updated staff job assignment.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] StaffJobReq staffJobReq)
        {
            var staffJobDto = _mapper.Map<StaffJobDto>(staffJobReq);
            var result = await _staffJobService.UpdateAsync(staffJobDto);
            return Ok(_mapper.Map<StaffJobRes>(result));
        }

        /// <summary>
        /// Deletes a staff job assignment by staff ID and job code.
        /// </summary>
        /// <param name="staffId">The staff member's identifier.</param>
        /// <param name="jobCode">The job code.</param>
        /// <returns>No content if deletion is successful; NotFound if not found.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] string staffId, [FromQuery] string jobCode)
        {
            var success = await _staffJobService.DeleteAsync(staffId, jobCode);
            if (!success)
                throw new KeyNotFoundException("Data not found.");
            return NoContent();
        }
    }
}

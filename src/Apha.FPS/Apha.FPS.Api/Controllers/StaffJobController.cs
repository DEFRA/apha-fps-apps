using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [ApiController]
    [Route("api/staffjob")]
    public class StaffJobController : ControllerBase
    {
        private readonly IStaffJobService _staffJobService;
        private readonly IMapper _mapper;

        public StaffJobController(
                        IStaffJobService staffJobService,
                        IMapper mapper)
        {
            _staffJobService = staffJobService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobStaffCostAsync([FromQuery] PaginationReq<object> query)
        {
            var filter = _mapper.Map<QueryParameters<object>>(query);
            var result = await _staffJobService.GetJobStaffCostAsync(filter);
            return Ok(_mapper.Map<PaginationRes<StaffJobViewRes>>(result));
        }

        [HttpGet("workgrouplookup")]
        public async Task<IActionResult> GetStaffWorkgroupLookup()
        {
            var result = await _staffJobService.GetStaffWorkgroupLookup();
            return Ok(_mapper.Map<List<StaffWorkgroupLookupRes>>(result));
        }

        [HttpGet("chargerate")]
        public async Task<IActionResult> GetStaffChargeRate([FromQuery] string staffId, [FromQuery] string jobcode)
        {
            var chargeRate = await _staffJobService.GetStaffChargeRate(staffId, jobcode);
            return Ok(chargeRate);
        }

        [HttpGet("{staffId}/{jobCode}")]
        public async Task<IActionResult> GetByIdAsync(string staffId, string jobCode)
        {
            var result = await _staffJobService.GetByIdAsync(staffId, jobCode);
            if (result == null)
                throw new KeyNotFoundException("Data not found.");
            return Ok(_mapper.Map<StaffJobRes>(result));
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] StaffJobReq staffJobReq)
        {
            var staffJobDto = _mapper.Map<StaffJobDto>(staffJobReq);
            var result = await _staffJobService.AddAsync(staffJobDto);
            return CreatedAtAction(nameof(GetByIdAsync), new { staffId = result.StaffId, jobCode = result.JobCode }, _mapper.Map<StaffJobRes>(result));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] StaffJobReq staffJobReq)
        {
            var staffJobDto = _mapper.Map<StaffJobDto>(staffJobReq);
            var result = await _staffJobService.UpdateAsync(staffJobDto);
            return Ok(_mapper.Map<StaffJobRes>(result));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] string staffId, [FromQuery] string jobCode)
        {
            var success = await _staffJobService.DeleteAsync(staffId, jobCode);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }
}

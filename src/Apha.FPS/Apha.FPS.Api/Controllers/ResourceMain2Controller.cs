using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for Stage 2 Check Resource Allocation (frmResourceMain2).
    /// Provides read-only grid data for staff allocations and staff job lines.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/resourcemain2")]
    public class ResourceMain2Controller : ControllerBase
    {
        private readonly IResourceMain2Service _service;
        private readonly IMapper _mapper;

        public ResourceMain2Controller(IResourceMain2Service service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns staff allocation rows for a workgroup grade.
        /// </summary>
        /// <param name="workGroupGrade">The WG Grade code.</param>
        [HttpGet("staffallocations")]
        public async Task<IActionResult> GetStaffAllocationsByWorkGroupGradeAsync([FromQuery] string workGroupGrade)
        {
            var dtos = await _service.GetStaffAllocationsByWorkGroupGradeAsync(workGroupGrade);
            return Ok(_mapper.Map<List<ResourceStaffAllocationRes>>(dtos));
        }

        /// <summary>
        /// Returns job rows for a staff member.
        /// </summary>
        /// <param name="staffId">The staff member ID.</param>
        [HttpGet("staffjobs")]
        public async Task<IActionResult> GetStaffJobsByStaffIdAsync([FromQuery] int staffId)
        {
            var dtos = await _service.GetStaffJobsByStaffIdAsync(staffId);
            return Ok(_mapper.Map<List<ResourceStaffJobRes>>(dtos));
        }
    }
}

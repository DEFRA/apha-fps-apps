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
    /// API controller for WG Staff (employees) within a given WG grade.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/wgstaff")]
    public class WorkGroupEmployeeController : ControllerBase
    {
        private readonly IWorkGroupEmployeeService _WorkGroupEmployeeService;
        private readonly IMapper _mapper;

        public WorkGroupEmployeeController(IWorkGroupEmployeeService WorkGroupEmployeeService, IMapper mapper)
        {
            _WorkGroupEmployeeService = WorkGroupEmployeeService ?? throw new ArgumentNullException(nameof(WorkGroupEmployeeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of staff for the given WG grade.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="wgGrade">The WG grade code.</param>
        [HttpGet]
        public async Task<IActionResult> GetWorkGroupEmployeeAsync([FromQuery] PaginationReq<string> query, [FromQuery] string wgGrade)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _WorkGroupEmployeeService.GetWorkGroupEmployeeAsync(filter, wgGrade);
            return Ok(_mapper.Map<PaginationRes<WgEmployeeViewRes>>(result));
        }

        /// <summary>
        /// Returns a single WG employee by PACTid.
        /// </summary>
        /// <param name="pactId">The PACTid of the employee.</param>
        [HttpGet("{pactId}")]
        public async Task<IActionResult> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            var result = await _WorkGroupEmployeeService.GetWorkGroupEmployeeByIdAsync(pactId);
            if (result == null)
                throw new KeyNotFoundException("WgEmployee not found.");
            return Ok(_mapper.Map<WgEmployeeRes>(result));
        }

        /// <summary>
        /// Updates an existing WG employee record. HrsAvail is computed server-side as HrsPaid - (Leave + SickSpecial).
        /// </summary>
        /// <param name="req">The WG employee update request.</param>
        [HttpPut]
        public async Task<IActionResult> UpdateWorkGroupEmployeeAsync([FromBody] WgEmployeeReq req)
        {
            var dto = _mapper.Map<WgEmployeeDto>(req);
            var result = await _WorkGroupEmployeeService.UpdateWorkGroupEmployeeAsync(dto);
            return Ok(_mapper.Map<WgEmployeeRes>(result));
        }

        /// <summary>
        /// Deletes a WG employee by PACTid.
        /// </summary>
        /// <param name="pactId">The PACTid of the employee to delete.</param>
        [HttpDelete("{pactId}")]
        public async Task<IActionResult> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            await _WorkGroupEmployeeService.DeleteWorkGroupEmployeeAsync(pactId);
            return NoContent();
        }
    }
}

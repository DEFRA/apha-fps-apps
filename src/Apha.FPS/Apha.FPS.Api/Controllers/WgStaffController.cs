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
    public class WgStaffController : ControllerBase
    {
        private readonly IWgStaffService _wgStaffService;
        private readonly IMapper _mapper;

        public WgStaffController(IWgStaffService wgStaffService, IMapper mapper)
        {
            _wgStaffService = wgStaffService ?? throw new ArgumentNullException(nameof(wgStaffService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of staff for the given WG grade.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="wgGrade">The WG grade code.</param>
        [HttpGet]
        public async Task<IActionResult> GetWgStaffAsync([FromQuery] PaginationReq<string> query, [FromQuery] string wgGrade, CancellationToken cancellationToken)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _wgStaffService.GetWgStaffAsync(filter, wgGrade, cancellationToken);
            return Ok(_mapper.Map<PaginationRes<WgEmployeeViewRes>>(result));
        }

        /// <summary>
        /// Returns a single WG employee by PACTid.
        /// </summary>
        /// <param name="pactId">The PACTid of the employee.</param>
        [HttpGet("{pactId}")]
        public async Task<IActionResult> GetWgEmployeeByIdAsync(string pactId, CancellationToken cancellationToken)
        {
            var result = await _wgStaffService.GetWgEmployeeByIdAsync(pactId, cancellationToken);
            if (result == null)
                throw new KeyNotFoundException("WgEmployee not found.");
            return Ok(_mapper.Map<WgEmployeeRes>(result));
        }

        /// <summary>
        /// Updates an existing WG employee record. HrsAvail is computed server-side as HrsPaid - (Leave + SickSpecial).
        /// </summary>
        /// <param name="req">The WG employee update request.</param>
        [HttpPut]
        public async Task<IActionResult> UpdateWgEmployeeAsync([FromBody] WgEmployeeReq req, CancellationToken cancellationToken)
        {
            var dto = _mapper.Map<WgEmployeeDto>(req);
            var result = await _wgStaffService.UpdateWgEmployeeAsync(dto, cancellationToken);
            return Ok(_mapper.Map<WgEmployeeRes>(result));
        }

        /// <summary>
        /// Deletes a WG employee by PACTid.
        /// </summary>
        /// <param name="pactId">The PACTid of the employee to delete.</param>
        [HttpDelete("{pactId}")]
        public async Task<IActionResult> DeleteWgEmployeeAsync(string pactId, CancellationToken cancellationToken)
        {
            await _wgStaffService.DeleteWgEmployeeAsync(pactId, cancellationToken);
            return NoContent();
        }
    }
}

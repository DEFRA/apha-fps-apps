using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for WG Grades available within a given RC grade.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/wggrades")]
    public class WorkGroupGradeController : ControllerBase
    {
        private readonly IWorkGroupGradeService _WorkGroupGradeService;
        private readonly IMapper _mapper;

        public WorkGroupGradeController(IWorkGroupGradeService WorkGroupGradeService, IMapper mapper)
        {
            _WorkGroupGradeService = WorkGroupGradeService ?? throw new ArgumentNullException(nameof(WorkGroupGradeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of WG grades available within the given RC grade.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="pcGrade">The profit centre grade code.</param>
        [HttpGet]
        public async Task<IActionResult> GetWorkGroupGradeAsync([FromQuery] PaginationReq<string> query, [FromQuery] string pcGrade)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _WorkGroupGradeService.GetWorkGroupGradeAsync(filter, profitCentreGrade: pcGrade);
            return Ok(_mapper.Map<PaginationRes<WorkgroupGradeRes>>(result));
        }

        /// <summary>
        /// Deletes a WG grade by its grade code.
        /// </summary>
        /// <param name="wgGrade">The WG grade code to delete.</param>
        [HttpDelete("{wgGrade}")]
        public async Task<IActionResult> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            var isDeleted = await _WorkGroupGradeService.DeleteWorkGroupGradeAsync(wgGrade);
            if (!isDeleted)
                throw new KeyNotFoundException("WorkGroupGrade not found.");
            return Ok(isDeleted);
        }
    }
}

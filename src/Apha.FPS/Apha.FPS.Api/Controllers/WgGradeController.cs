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
    public class WgGradeController : ControllerBase
    {
        private readonly IWgGradeService _wgGradeService;
        private readonly IMapper _mapper;

        public WgGradeController(IWgGradeService wgGradeService, IMapper mapper)
        {
            _wgGradeService = wgGradeService ?? throw new ArgumentNullException(nameof(wgGradeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of WG grades available within the given RC grade.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="pcGrade">The profit centre grade code.</param>
        [HttpGet]
        public async Task<IActionResult> GetWgGradesAsync([FromQuery] PaginationReq<string> query, [FromQuery] string pcGrade, CancellationToken cancellationToken)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _wgGradeService.GetWgGradesAsync(filter, pcGrade, cancellationToken);
            return Ok(_mapper.Map<PaginationRes<WorkgroupGradeRes>>(result));
        }

        /// <summary>
        /// Deletes a WG grade by its grade code.
        /// </summary>
        /// <param name="wgGrade">The WG grade code to delete.</param>
        [HttpDelete("{wgGrade}")]
        public async Task<IActionResult> DeleteWgGradeAsync(string wgGrade, CancellationToken cancellationToken)
        {
            await _wgGradeService.DeleteWgGradeAsync(wgGrade, cancellationToken);
            return NoContent();
        }
    }
}

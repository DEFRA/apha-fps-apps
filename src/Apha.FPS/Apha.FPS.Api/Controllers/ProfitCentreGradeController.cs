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
    /// API controller for RC Grades available for a given profit centre.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/pcgrades")]
    public class ProfitCentreGradeController : ControllerBase
    {
        private readonly IProfitCentreGradeService _ProfitCentreGradeService;
        private readonly IMapper _mapper;

        public ProfitCentreGradeController(IProfitCentreGradeService ProfitCentreGradeService, IMapper mapper)
        {
            _ProfitCentreGradeService = ProfitCentreGradeService ?? throw new ArgumentNullException(nameof(ProfitCentreGradeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of RC grades available for the given profit centre.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="profitCentre">The profit centre code.</param>
        [HttpGet]
        public async Task<IActionResult> GetProfitCentreGradesAsync([FromQuery] PaginationReq<string> query, [FromQuery] string profitCentre)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _ProfitCentreGradeService.GetProfitCentreGradesAsync(filter, profitCentre);
            return Ok(_mapper.Map<PaginationRes<ProfitCentreGradeRes>>(result));
        }

        /// <summary>Returns all Profit Centre Grade codes for dropdown population.</summary>
        [HttpGet("allpcgrades")]
        public async Task<ActionResult<List<string>>> GetAllPcGradesAsync()
        {
            var result = await _ProfitCentreGradeService.GetAllPcGradesAsync();
            return Ok(result);
        }
    }
}

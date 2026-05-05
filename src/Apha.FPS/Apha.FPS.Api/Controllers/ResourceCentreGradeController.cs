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
    [Route("api/v{version:apiVersion}/rcgrades")]
    public class ResourceCentreGradeController : ControllerBase
    {
        private readonly IResourceCentreGradeService _resourceCentreGradeService;
        private readonly IMapper _mapper;

        public ResourceCentreGradeController(IResourceCentreGradeService resourceCentreGradeService, IMapper mapper)
        {
            _resourceCentreGradeService = resourceCentreGradeService ?? throw new ArgumentNullException(nameof(resourceCentreGradeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of RC grades available for the given profit centre.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="profitCentre">The profit centre code.</param>
        [HttpGet]
        public async Task<IActionResult> GetResourceCentreGradesAsync([FromQuery] PaginationReq<string> query, [FromQuery] string profitCentre)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _resourceCentreGradeService.GetResourceCentreGradesAsync(filter, profitCentre);
            return Ok(_mapper.Map<PaginationRes<ProfitCentreGradeRes>>(result));
        }
    }
}

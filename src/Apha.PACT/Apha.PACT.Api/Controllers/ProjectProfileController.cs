using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{

    /// <summary>
    /// API controller for Project Profile graph data operations.
    /// </summary>
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectprofile")]
    public class ProjectProfileController : ControllerBase
    {
        private readonly IProjectProfileService _service;
        private readonly IMapper _mapper;

        public ProjectProfileController(IProjectProfileService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves the monthly profile and cost data for a given project, used to render the non-cumulative graph.</summary>
        /// <param name="project">The project code to retrieve profile graph data for.</param>
        /// <returns>Returns <c>200 OK</c> with a list of <see cref="ProjectProfileGraphRes"/> objects.</returns>
        [HttpGet("{project}/graph")]
        public async Task<IActionResult> GetProfileGraph(string project)
        {
            IList<ProjectProfileGraphDto> data = await _service.GetProfileGraphDataAsync(project);
            return Ok(_mapper.Map<IList<ProjectProfileGraphRes>>(data));
        }

        /// <summary>Retrieves the cumulative profile and cost data for a given project, used to render the cumulative graph.</summary>
        /// <param name="project">The project code to retrieve cumulative graph data for.</param>
        /// <returns>Returns <c>200 OK</c> with a list of <see cref="ProjectProfileCumulativeGraphRes"/> objects.</returns>
        [HttpGet("{project}/graph/cumulative")]
        public async Task<IActionResult> GetCumulativeGraph(string project)
        {
            IList<ProjectProfileCumulativeGraphDto> data = await _service.GetCumulativeGraphDataAsync(project);
            return Ok(_mapper.Map<IList<ProjectProfileCumulativeGraphRes>>(data));
        }
    }
}

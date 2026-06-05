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
    /// API controller for job code lookups.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/jobcode")]
    public class JobCodeController : ControllerBase
    {
        private readonly IJobCodeService _jobCodeService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobCodeController"/> class.
        /// </summary>
        /// <param name="jobCodeService">Service for job code operations.</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
        public JobCodeController(IJobCodeService jobCodeService, IMapper mapper)
        {
            _jobCodeService = jobCodeService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all ZT-type job codes for the current FPS year.
        /// </summary>
        /// <returns>List of ZT job code results.</returns>
        [HttpGet("zt")]
        public async Task<IActionResult> GetZtCodesAsync()
        {
            var result = await _jobCodeService.GetZtCodeLookupAsync();
            return Ok(_mapper.Map<IEnumerable<JobCodeRes>>(result));
        }

        /// <summary>
        /// Retrieves all job codes for the current FPS year.
        /// </summary>
        /// <returns>List of job code results.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllJobCodesAsync()
        {
            var result = await _jobCodeService.GetJobCodeListAsync();
            return Ok(_mapper.Map<IEnumerable<JobCodeRes>>(result));
        }
    }
}

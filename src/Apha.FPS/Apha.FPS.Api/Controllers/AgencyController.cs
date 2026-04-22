using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/agency")]
    [ApiController]
    public class AgencyController : ControllerBase
    {
        private readonly IAgencyService _agencyService;
        private readonly IMapper _mapper;

        public AgencyController(
            IAgencyService agencyService,
            IMapper mapper)
        {
            _agencyService = agencyService ?? throw new ArgumentNullException(nameof(agencyService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult> GetAllAgenciesAsync()
        {
            var agencies = await _agencyService.GetAllAgenciesAsync();
            if (agencies == null || !agencies.Any())
            {
                return Ok(new List<AgencyRes>());
            }
            return Ok(_mapper.Map<List<AgencyRes>>(agencies));
        }
    }
}

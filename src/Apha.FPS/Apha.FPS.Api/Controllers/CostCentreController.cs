using Apha.Common.Contracts.FPS;
using Apha.FPS.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/costcentre")]
    public class CostCentreController : ControllerBase
    {
        private readonly IStoredProcRepository _storedProcRepository;
        private readonly IMapper _mapper;

        public CostCentreController(IStoredProcRepository storedProcRepository, IMapper mapper)
        {
            _storedProcRepository = storedProcRepository ?? throw new ArgumentNullException(nameof(storedProcRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CostCentreWorkgroupRes>>> GetAllCostCentresAsync()
        {
            var costCentres = await _storedProcRepository.GetAllCostCentreWorkgroupAsync();
            return Ok(_mapper.Map<IEnumerable<CostCentreWorkgroupRes>>(costCentres));
        }
    }
}

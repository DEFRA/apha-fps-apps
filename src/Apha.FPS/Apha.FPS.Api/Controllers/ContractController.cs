using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/contract")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IMapper _mapper;

        public ContractController(IContractService contractService, IMapper mapper)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<List<ContractRes>>> GetAllContractsAsync()
        {
            var contracts = await _contractService.GetAllContractsAsync();
            return Ok(_mapper.Map<List<ContractRes>>(contracts));
        }

        [HttpGet("by-user")]
        public async Task<ActionResult<List<ContractRes>>> GetContractsByUserAsync()
        {
            var contracts = await _contractService.GetAllContractsByUserAsync();
            return Ok(_mapper.Map<List<ContractRes>>(contracts));
        }
    }
}

using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [Route("api/contract")]
    [ApiController]
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
    }
}

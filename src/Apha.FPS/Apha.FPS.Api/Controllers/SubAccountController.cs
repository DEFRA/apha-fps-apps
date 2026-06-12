using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/subaccount")]
    public class SubAccountController : ControllerBase
    {
        private readonly ISubAccountService _subAccountService;
        private readonly IMapper _mapper;

        public SubAccountController(ISubAccountService subAccountService, IMapper mapper)
        {
            _subAccountService = subAccountService ?? throw new ArgumentNullException(nameof(subAccountService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubAccountRes>>> GetAllSubAccountsAsync()
        {
            var subAccounts = await _subAccountService.GetAllSubAccountsAsync();
            return Ok(_mapper.Map<IEnumerable<SubAccountRes>>(subAccounts));
        }
    }
}

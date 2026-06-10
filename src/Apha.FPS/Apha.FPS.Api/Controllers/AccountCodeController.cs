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
    [Route("api/v{version:apiVersion}/accountcode")]
    public class AccountCodeController : ControllerBase
    {
        private readonly IAccountCodeService _accountCodeService;
        private readonly IMapper _mapper;

        public AccountCodeController(IAccountCodeService accountCodeService, IMapper mapper)
        {
            _accountCodeService = accountCodeService ?? throw new ArgumentNullException(nameof(accountCodeService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountCodeRes>>> GetAllAccountCodesAsync()
        {
            var accountCodes = await _accountCodeService.GetAllAccountCodeAsync();
            return Ok(_mapper.Map<IEnumerable<AccountCodeRes>>(accountCodes));
        }
    }
}

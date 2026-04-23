using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for project-related lookup data (managers, cost centres, account codes, etc.).
    /// Separated from ProjectController to avoid ambiguous route conflicts with parameterised routes.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/project-support")]
    public class ProjectSupportController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAccountCodeService _accountCodeService;
        private readonly ISubAccountService _subAccountService;
        private readonly IProjectGroupService _projectGroupService;
        private readonly IStoredProcRepository _storedProcRepository;
        private readonly IMapper _mapper;

        public ProjectSupportController(
            IEmployeeService employeeService,
            IAccountCodeService accountCodeService,
            ISubAccountService subAccountService,
            IProjectGroupService projectGroupService,
            IStoredProcRepository storedProcRepository,
            IMapper mapper)
        {
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            _accountCodeService = accountCodeService ?? throw new ArgumentNullException(nameof(accountCodeService));
            _subAccountService = subAccountService ?? throw new ArgumentNullException(nameof(subAccountService));
            _projectGroupService = projectGroupService ?? throw new ArgumentNullException(nameof(projectGroupService));
            _storedProcRepository = storedProcRepository ?? throw new ArgumentNullException(nameof(storedProcRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("managers")]
        public async Task<ActionResult<IEnumerable<ManagerRes>>> GetManagersAsync()
        {
            var managers = await _employeeService.GetAllManagersAsync();
            return Ok(_mapper.Map<IEnumerable<ManagerRes>>(managers));
        }

        [HttpGet("cost-centres")]
        public async Task<ActionResult<IEnumerable<CostCentreWorkgroupRes>>> GetCostCentresAsync()
        {
            var costCentres = await _storedProcRepository.GetAllCostCentreWorkgroupAsync();
            return Ok(_mapper.Map<IEnumerable<CostCentreWorkgroupRes>>(costCentres));
        }

        [HttpGet("project-groups")]
        public async Task<ActionResult<IEnumerable<ProjectGroupRes>>> GetProjectGroupsAsync()
        {
            var projectGroups = await _projectGroupService.GetAllProjectGroupsAsync();
            return Ok(_mapper.Map<IEnumerable<ProjectGroupRes>>(projectGroups));
        }

        [HttpGet("account-codes")]
        public async Task<ActionResult<IEnumerable<AccountCodeRes>>> GetAccountCodesAsync()
        {
            var accountCodes = await _accountCodeService.GetAllAccountCodeAsync();
            return Ok(_mapper.Map<IEnumerable<AccountCodeRes>>(accountCodes));
        }

        [HttpGet("sub-accounts")]
        public async Task<ActionResult<IEnumerable<SubAccountRes>>> GetSubAccountsAsync()
        {
            var subAccounts = await _subAccountService.GetAllSubAccountsAsync();
            return Ok(_mapper.Map<IEnumerable<SubAccountRes>>(subAccounts));
        }
    }
}

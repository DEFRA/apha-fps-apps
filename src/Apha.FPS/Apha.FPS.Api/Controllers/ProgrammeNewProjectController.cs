using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [Route("api/v{version:apiVersion}/programme-new-project")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ProgrammeNewProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IEmployeeService _employeeService;
        private readonly IAccountCodeService _accountCodeService;
        private readonly ISubAccountService _subAccountService;
        private readonly IProjectGroupService _projectGroupService;
        private readonly IStoredProcRepository _storedProcRepository;
        private readonly IMapper _mapper;

        public ProgrammeNewProjectController(
            IProjectService projectService,
            IEmployeeService employeeService,
            IAccountCodeService accountCodeService,
            ISubAccountService subAccountService,
            IProjectGroupService projectGroupService,
            IStoredProcRepository storedProcRepository,
            IMapper mapper)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            _accountCodeService = accountCodeService ?? throw new ArgumentNullException(nameof(accountCodeService));
            _subAccountService = subAccountService ?? throw new ArgumentNullException(nameof(subAccountService));
            _projectGroupService = projectGroupService ?? throw new ArgumentNullException(nameof(projectGroupService));
            _storedProcRepository = storedProcRepository ?? throw new ArgumentNullException(nameof(storedProcRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("{parentProject}")]
        public async Task<ActionResult<ProgrammeNewProjectRes>> GetProjectByIdAsync(string parentProject)
        {
            var project = await _projectService.GetProjectByIdAsync(parentProject);
            if (project == null)
                return NotFound();
            return Ok(_mapper.Map<ProgrammeNewProjectRes>(project));
        }

        [HttpPost]
        public async Task<ActionResult<ProgrammeNewProjectRes>> CreateProjectAsync([FromBody] ProgrammeNewProjectReq request)
        {
            var projectDto = _mapper.Map<ProjectDto>(request);
            var created = await _projectService.CreateProjectAsync(projectDto);
            return CreatedAtAction(nameof(GetProjectByIdAsync),
                new { parentProject = created.ParentProject },
                _mapper.Map<ProgrammeNewProjectRes>(created));
        }

        [HttpPut("{parentProject}")]
        public async Task<ActionResult<ProgrammeNewProjectRes>> UpdateProjectAsync(string parentProject, [FromBody] ProgrammeNewProjectReq request)
        {
            if (parentProject != request.ParentProject)
                throw new ArgumentException("Route project code does not match request body.");
            var projectDto = _mapper.Map<ProjectDto>(request);
            var updated = await _projectService.UpdateProjectAsync(projectDto);
            return Ok(_mapper.Map<ProgrammeNewProjectRes>(updated));
        }

        [HttpDelete("{parentProject}/delete-with-children")]
        public async Task<IActionResult> DeleteProjectAndChildrenAsync(string parentProject)
        {
            if (string.IsNullOrWhiteSpace(parentProject))
                throw new ArgumentException("Parent project cannot be empty.", nameof(parentProject));
            await _projectService.DeleteProjectAndChildrenAsync(parentProject);
            return Ok(true);
        }

        [HttpPost("change-code")]
        public async Task<IActionResult> ChangeProjectCodeAsync([FromBody] ChangeProjectCodeReq request)
        {
            if (string.IsNullOrWhiteSpace(request.OldCode) || string.IsNullOrWhiteSpace(request.NewCode))
                throw new ArgumentException("Both old and new project codes are required.");
            var existing = await _projectService.GetProjectByIdAsync(request.OldCode);
            if (existing == null)
                return NotFound($"Project with code '{request.OldCode}' not found.");
            await _projectService.ChangeProjectCodeAsync(request.OldCode, request.NewCode);
            return Ok(true);
        }

        [HttpGet("check-exists/{code}")]
        public async Task<ActionResult<bool>> CheckProjectExistsAsync(string code)
        {
            var exists = await _projectService.CheckProjectExistsAsync(code);
            return Ok(exists);
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

    public record ChangeProjectCodeReq(string OldCode, string NewCode);
}

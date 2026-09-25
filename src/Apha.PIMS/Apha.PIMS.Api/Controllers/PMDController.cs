using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PMDAdmin,API-PIMSProjectManager")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/pmd")]
    public class PMDController : ControllerBase
    {
        private readonly IMilestoneService _service;
        private readonly IMapper _mapper;

        public PMDController(IMilestoneService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("projectyearmanagers/{year:int}")]
        public async Task<IActionResult> GetProjectYearManagers(int year, [FromQuery] string email, [FromQuery] bool isAdmin)
        {
            Console.WriteLine($"GetProjectYearManagers roles: API-PMDAdmin={User.IsInRole("API-PMDAdmin")}, API-PIMSProjectManager={User.IsInRole("API-PIMSProjectManager")}, isAdmin={isAdmin}");
            Console.WriteLine($"GetProjectYearManagers email: {email}");

            List<ProjectYearManagerDto> result = await _service.GetProjectYearManagersAsync(year, email, isAdmin);
            return Ok(_mapper.Map<List<ProjectYearManagerRes>>(result) ?? []);
        }

        [HttpGet("milestones")]
        public async Task<IActionResult> GetPMDMilestones([FromQuery] QueryParameters<string> parameters, [FromQuery] string project)
        {
            PaginatedResult<MilestoneDto> result = await _service.GetPMDMilestonesAsync(parameters, project);
            return Ok(_mapper.Map<PaginationRes<MilestoneRes>>(result));
        }

        [HttpGet("milestone")]
        public async Task<IActionResult> GetMilestoneAsync_PMD([FromQuery] string project, [FromQuery] string number)
        {
            MilestoneDto? result = await _service.GetMilestoneAsync(project, HttpUtility.UrlDecode(number));

            if (result is null)
            {
                return new JsonResult(new Apha.Common.Contracts.ApiResponse<MilestoneRes>
                {
                    Success = true,
                    Data = null,
                    Meta = new Apha.Common.Contracts.ApiMeta
                    {
                        CorrelationId = Guid.NewGuid().ToString(),
                        TimestampUtc = DateTime.UtcNow
                    }
                });
            }

            return Ok(_mapper.Map<MilestoneRes>(result));
        }

        [HttpGet("formdates")]
        public async Task<IActionResult> GetMilestoneFormDatesAsync_PMD([FromQuery] string parentProject, [FromQuery] short year)
        {
            MilestoneFormDatesDto? result = await _service.GetMilestoneFormDatesAsync(year, parentProject);

            if (result is null)
            {
                return new JsonResult(new Apha.Common.Contracts.ApiResponse<MilestoneFormDatesRes>
                {
                    Success = true,
                    Data = null,
                    Meta = new Apha.Common.Contracts.ApiMeta
                    {
                        CorrelationId = Guid.NewGuid().ToString(),
                        TimestampUtc = DateTime.UtcNow
                    }
                });
            }

            return Ok(_mapper.Map<MilestoneFormDatesRes>(result));
        }

        [HttpPost("formdates")]
        public async Task<IActionResult> SaveMilestoneFormDatesAsync_PMD([FromQuery] string parentProject, [FromBody] MilestoneFormDatesReq request)
        {
            MilestoneFormDatesDto dto = _mapper.Map<MilestoneFormDatesDto>(request);
            dto.ParentProject = parentProject;
            MilestoneFormDatesDto result = await _service.SaveMilestoneFormDatesAsync(dto);
            return Ok(_mapper.Map<MilestoneFormDatesRes>(result));
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateMilestone_PMD(
            [FromQuery] string? project,
            [FromQuery] string? number,
            [FromBody] MilestoneReq request)
        {
            MilestoneDto dto = _mapper.Map<MilestoneDto>(request);
            string? changedBy = ResolveUserEmail(User);
            MilestoneDto result = await _service.UpdateMilestoneAsync_PMD(
                project!,
                HttpUtility.UrlDecode(number!),
                dto.UnderSdReview,
                dto.OnTarget ?? 0,
                dto.DateCompleted,
                dto.ProjectLeaderComment,
                changedBy);

            return Ok(_mapper.Map<MilestoneRes>(result));
        }

        private static string ResolveUserEmail(ClaimsPrincipal? user)
        {
            string identityName = user?.Identity?.Name ?? string.Empty;
            if (IsEmailAddress(identityName))
                return identityName;

            string email = user?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(email))
                return email;

            return user?.Identity?.Name ?? string.Empty;
        }

        private static bool IsEmailAddress(string value)
            => !string.IsNullOrWhiteSpace(value)
                && MailAddress.TryCreate(value, out MailAddress? address)
                && string.Equals(address.Address, value, StringComparison.OrdinalIgnoreCase);

    }
}

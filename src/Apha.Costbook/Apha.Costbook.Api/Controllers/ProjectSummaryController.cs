using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.Costbook.Api.Controllers;

/// <summary>Project Summary — cost totals including profit.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projectsummary")]
[Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")]
public class ProjectSummaryController : ControllerBase
{
    private readonly IProjectSummaryService _service;
    private readonly IMapper _mapper;

    public ProjectSummaryController(IProjectSummaryService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet("{projectId}/years/{year}/profittotal")]
    public async Task<IActionResult> GetProfitIncludedTotal(string projectId, int year)
    {
        var total = await _service.GetProfitIncludedTotalAsync(projectId, year);
        return Ok(BuildOk(total));
    }

    /// <summary>Returns the CSG7 staff years pivot for a given project, grouped by grade and pivoted by financial year.</summary>
    [HttpGet("{id}/staff-years")]
    public async Task<IActionResult> GetStaffYearsPivot(string id, [FromQuery] QueryParameters<string>? query = null)
    {
        var result = await _service.GetStaffYearsPivotAsync(id, query);
        return Ok(_mapper.Map<StaffYearsPivotRes>(result));
    }

    /// <summary>Returns the SID3 staff effort pivot for a given project, grouped by WorkGroup, GradeCode and Name, pivoted by financial year.</summary>
    [HttpGet("{id}/staff-effort")]
    public async Task<IActionResult> GetStaffEffort(string id, [FromQuery] QueryParameters<string>? query = null)
    {
        var result = await _service.GetStaffEffortAsync(id, query);
        return Ok(_mapper.Map<StaffEffortPivotRes>(result));
    }

    /// <summary>Returns the CSG7 project costs pivot for a given project, grouped by category and pivoted by financial year.</summary>
    [HttpGet("{id}/project-costs")]
    public async Task<IActionResult> GetProjectCostsPivot(string id, [FromQuery] QueryParameters<string>? query = null)
    {
        var result = await _service.GetProjectCostsPivotAsync(id, query);
        return Ok(_mapper.Map<ProjectCostsPivotRes>(result));
    }

    private static ApiResponse<T> BuildOk<T>(T data) => new()
    {
        Success = true,
        Data = data,
        Errors = new List<ApiError>(),
        Meta = new ApiMeta()
    };
}
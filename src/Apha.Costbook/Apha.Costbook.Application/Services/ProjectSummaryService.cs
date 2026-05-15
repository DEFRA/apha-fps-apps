using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;

namespace Apha.Costbook.Application.Services;

public class ProjectSummaryService : IProjectSummaryService
{
    private readonly IProjectRepository _projectRepo;
    private readonly IMapper _mapper;

    public ProjectSummaryService(IProjectRepository projectRepo, IMapper mapper)
    {
        _projectRepo = projectRepo;
        _mapper = mapper;
    }

    public Task<double> GetProfitIncludedTotalAsync(string projectId, int year)
        => _projectRepo.GetProfitIncludedTotalAsync(projectId, year);

    public async Task<StaffYearsPivotDto> GetStaffYearsPivotAsync(string projectId, QueryParameters<string>? query = null)
    {
        var parameters = query != null
            ? _mapper.Map<PaginationParameters<string>>(query)
            : null;

        var data = await _projectRepo.GetStaffYearsPivotAsync(projectId, parameters);
        return new StaffYearsPivotDto
        {
            Years = data.Years,
            TotalCount = data.TotalCount,
            Rows = data.Rows.Select(r => new StaffYearsRowDto
            {
                Project = r.Project,
                Grade = r.Grade,
                Total = r.Total,
                YearlyAmounts = r.YearlyAmounts
            }).ToList()
        };
    }
    public async Task<StaffEffortPivotDto> GetStaffEffortAsync(string projectId, QueryParameters<string>? query = null)
    {
        var parameters = query != null
            ? _mapper.Map<PaginationParameters<string>>(query)
            : null;

        var data = await _projectRepo.GetStaffEffortAsync(projectId, parameters);
        return new StaffEffortPivotDto
        {
            Years = data.Years,
            TotalCount = data.TotalCount,
            Rows = data.Rows.Select(r => new StaffEffortRowDto
            {
                Project = r.Project,
                WorkGroup = r.WorkGroup,
                GradeCode = r.GradeCode,
                Name = r.Name,
                Total = r.Total,
                YearlyAmounts = r.YearlyAmounts
            }).ToList()
        };
    }

    public async Task<ProjectCostsPivotDto> GetProjectCostsPivotAsync(string projectId, QueryParameters<string>? query = null)
    {
        var parameters = query != null
            ? _mapper.Map<PaginationParameters<string>>(query)
            : null;

        var data = await _projectRepo.GetProjectCostsPivotAsync(projectId, parameters);
        return new ProjectCostsPivotDto
        {
            Years = data.Years,
            TotalCount = data.TotalCount,
            Rows = data.Rows.Select(r => new ProjectCostsRowDto
            {
                Project = r.Project,
                Category = r.Category,
                Total = r.Total,
                YearlyAmounts = r.YearlyAmounts
            }).ToList()
        };
    }
}
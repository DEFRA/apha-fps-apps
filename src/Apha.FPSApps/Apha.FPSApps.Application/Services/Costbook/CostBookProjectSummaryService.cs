using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.Costbook;

public class CostBookProjectSummaryService : ICostBookProjectSummaryService
{
    private readonly ICostBookApiClient _client;

    public CostBookProjectSummaryService(ICostBookApiClient client)
    {
        _client = client;
    }

    public Task<ApiResponseDto<double>> GetProfitIncludedTotalAsync(string projectId, int year)
        => _client.ProjectSummary.GetProfitIncludedTotalAsync(projectId, year);

    public Task<ApiResponseDto<StaffYearsPivotDto>> GetStaffYearsPivotAsync(string projectId, QueryParameters<string>? query = null)
    {
        return _client.ProjectSummary.GetStaffYearsPivotAsync(projectId, query);
    }

    public Task<ApiResponseDto<StaffEffortPivotDto>> GetStaffEffortAsync(string projectId, QueryParameters<string>? query = null)
    {
        return _client.ProjectSummary.GetStaffEffortAsync(projectId, query);
    }

    public Task<ApiResponseDto<ProjectCostsPivotDto>> GetProjectCostsPivotAsync(string projectId, QueryParameters<string>? query = null)
    {
        return _client.ProjectSummary.GetProjectCostsPivotAsync(projectId, query);
    }

    public Task<byte[]> ExportProjectSummaryToExcelAsync(string projectId)
    {
        return _client.ProjectSummary.ExportProjectSummaryToExcelAsync(projectId);
    }

    public Task<ApiResponseDto<ProjectYearCostSummaryDto>> GetProjectYearCostSummaryAsync(string projectId, int year)
        => _client.ProjectSummary.GetProjectYearCostSummaryAsync(projectId, year);
}
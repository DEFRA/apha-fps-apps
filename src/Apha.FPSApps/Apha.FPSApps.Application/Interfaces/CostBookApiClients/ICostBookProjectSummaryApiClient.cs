using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients;

public interface ICostBookProjectSummaryApiClient
{
    Task<ApiResponseDto<double>> GetProfitIncludedTotalAsync(string projectId, int year);
    Task<ApiResponseDto<StaffYearsPivotDto>> GetStaffYearsPivotAsync(string projectId, QueryParameters<string>? query = null);
    Task<ApiResponseDto<StaffEffortPivotDto>> GetStaffEffortAsync(string projectId, QueryParameters<string>? query = null);
    Task<ApiResponseDto<ProjectCostsPivotDto>> GetProjectCostsPivotAsync(string projectId, QueryParameters<string>? query = null);
}
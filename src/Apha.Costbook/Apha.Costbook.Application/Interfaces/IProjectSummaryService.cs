using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;

namespace Apha.Costbook.Application.Interfaces;

public interface IProjectSummaryService
{
    Task<double> GetProfitIncludedTotalAsync(string projectId, int year);
    Task<StaffYearsPivotDto> GetStaffYearsPivotAsync(string projectId, QueryParameters<string>? query = null);
    Task<StaffEffortPivotDto> GetStaffEffortAsync(string projectId, QueryParameters<string>? query = null);
    Task<ProjectCostsPivotDto> GetProjectCostsPivotAsync(string projectId, QueryParameters<string>? query = null);
    Task<byte[]> ExportProjectSummaryToExcelAsync(string projectId);
    Task<ProjectYearCostSummaryDto> GetProjectYearCostSummaryAsync(string projectId, int year);
    Task<ProjectAdditionalCostDto> GetProjectAdditionalCostsPagedAsync(QueryParameters<string>? query = null);
}
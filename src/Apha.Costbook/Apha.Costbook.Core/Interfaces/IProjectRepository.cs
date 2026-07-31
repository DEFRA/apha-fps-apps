using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;

namespace Apha.Costbook.Core.Interfaces
{
    public interface IProjectRepository
    {
        Task<PagedData<Project>> GetPaginatedProjectsAsync(PaginationParameters<string> queryFilter);

        Task<IEnumerable<Project>> GetProjectsAsync(string? contractFilter, string? submittedByFilter);
        Task<Project?> GetProjectByIdAsync(string id);
        Task<Project> AddProjectAsync(Project project);
        Task<Project> CopyProjectAsync(Project project, string sourceProjectId);
        Task<Project> UpdateProjectAsync(Project project);
        Task<bool> DeleteProjectAsync(string id);
        Task<string> GetNextProjectNumberAsync(string? baseNumber);
        Task<bool> RecostProjectAsync(string projectID);
        Task<double> GetProfitIncludedTotalAsync(string projectId, int year);
        Task<StaffYearsPivotData> GetStaffYearsPivotAsync(string projectId, PaginationParameters<string>? parameters = null);
        Task<StaffEffortPivotData> GetStaffEffortAsync(string projectId, PaginationParameters<string>? parameters = null);
        Task<ProjectCostsPivotData> GetProjectCostsPivotAsync(string projectId, PaginationParameters<string>? parameters = null);
        Task<ProjectSummaryExportData> GetProjectSummaryExportDataAsync(string projectId);
        Task<double> GetInflationFactorAsync(string infType, string projectId, int year, int currentYear);
        Task<ProjectYearCostSummary> GetProjectYearCostSummaryAsync(string projectId, int year);
        Task<ProjectAdditionalCostData> GetProjectExceptionalCostsPagedAsync(PaginationParameters<string> query);
    }
}

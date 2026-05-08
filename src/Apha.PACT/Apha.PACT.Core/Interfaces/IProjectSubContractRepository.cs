using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IProjectSubContractRepository
    {
        Task<PagedData<ProjectSubContract>> GetPagedProjectSubContractsAsync(PaginationParameters<string> query, string? project);
        Task<decimal> GetTotalAmountAsync(string? project);
        Task<PagedData<ProjectSubContract>> GetFpsProjectSubContractsAsync(PaginationParameters<string> query, string? project);
        Task<decimal> GetFpsProjectSubContractTotalAmountAsync(string? project);
        Task<ProjectSubContract?> GetByIdAsync(int subContCounter);
        Task<ProjectSubContract> CreateAsync(ProjectSubContract entity);
        Task<ProjectSubContract> UpdateAsync(ProjectSubContract entity);
        Task<bool> DeleteAsync(int subContCounter);
        Task<List<MonthlySubContractsSummary>> GetMonthlySubContractsSummaryAsync(PaginationParameters<string> parameters);
    }
}

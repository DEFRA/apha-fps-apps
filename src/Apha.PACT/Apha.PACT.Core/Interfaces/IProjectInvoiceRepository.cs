using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IProjectInvoiceRepository
    {
        Task<PagedData<ProjectInvoice>> GetPagedProjectInvoicesAsync(PaginationParameters<string> query, string? parentProject);
        Task<decimal> GetTotalAmountAsync(string? parentProject);
        Task<ProjectInvoice?> GetByIdAsync(int invoiceCounter);
        Task<ProjectInvoice> CreateAsync(ProjectInvoice entity);
        Task<ProjectInvoice> UpdateAsync(ProjectInvoice entity);
        Task<bool> DeleteAsync(int invoiceCounter);
        Task<List<MonthlyInvoicesSummary>> GetMonthlyInvoicesSummaryAsync();
    }
}

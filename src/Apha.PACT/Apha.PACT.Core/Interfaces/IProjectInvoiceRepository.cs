using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IProjectInvoiceRepository
    {
        Task<PagedData<ProjectInvoice>> GetPagedProjectInvoicesAsync(PaginationParameters<string> query, string? parentProject);
        Task<PagedData<ProjectInvoice>> GetPagedProjectInvoicesByMonthAsync(PaginationParameters<string> query, int? month);
        Task<decimal> GetTotalAmountAsync(string? parentProject);
        Task<ProjectInvoice?> GetByIdAsync(int invoiceCounter);
        Task<ProjectInvoice> CreateAsync(ProjectInvoice entity);
        Task<ProjectInvoice> UpdateAsync(ProjectInvoice entity);
        Task<bool> DeleteAsync(int invoiceCounter);
        Task<List<MonthlyInvoicesSummary>> GetMonthlyInvoicesSummaryAsync(PaginationParameters<string> parameters);
        Task<List<ProjectInvoice>> GetInvoicesByMonthAsync(int month);
        Task<List<ProjectInvoice>> GetInvoicesByIdsAsync(List<int> invoiceIds);
        Task<bool> HasInvoicesForMonthAsync(int month);
        Task<int> CreateBulkInvoiceAsync(IEnumerable<ProjectInvoice> entities);
        Task<int> CopyInvoicesByMonthAsync(int sourceMonth, int targetMonth, List<int>? specificInvoiceIds = null);
    }
}

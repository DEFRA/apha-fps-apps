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
        Task<List<MonthlyInvoicesSummary>> GetMonthlyInvoicesSummaryAsync(PaginationParameters<string> parameters);
        Task<HashSet<string>> GetValidProjectsAsync();
        int GetCurrentFpsYear();
        Task<PagedData<InvoiceImportRow>> GetFailedInvoiceImportAsync(PaginationParameters<string> query, string importedBy);
        Task<ProjectInvoiceStaging?> GetFailedInvoiceImportByIdAsync(int id, string importedBy);
        Task<bool> DeleteFailedInvoiceImportByIdAsync(int id, string importedBy);
        Task<int> DeleteFailedInvoiceImportByUserAsync(string importedBy);
        Task<InvoiceImportResult> ImportInvoiceAsync(List<ProjectInvoice> passedRows, List<ProjectInvoiceStaging> failedRows);
        Task UpdateFailedInvoiceImportRecordsAsync(List<ProjectInvoiceStaging> records);
        Task DeleteFailedInvoiceImportByIdsAsync(List<int> ids, string importedBy);
    }
}

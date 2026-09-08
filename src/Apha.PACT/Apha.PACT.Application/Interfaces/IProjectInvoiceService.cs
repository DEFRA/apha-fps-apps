using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IProjectInvoiceService
    {
        Task<PaginatedResult<ProjectInvoiceDto>> GetPagedProjectInvoicesAsync(QueryParameters<string> query, string? parentProject);
        Task<decimal> GetTotalAmountAsync(string? parentProject);
        Task<ProjectInvoiceDto?> GetByIdAsync(int invoiceCounter);
        Task<ProjectInvoiceDto> CreateAsync(ProjectInvoiceDto dto);
        Task<ProjectInvoiceDto> UpdateAsync(ProjectInvoiceDto dto);
        Task<bool> DeleteAsync(int invoiceCounter);
        Task<MonthlyInvoicesPivotDto> GetMonthlyInvoicesSummaryAsync(QueryParameters<string> query);
        Task<PaginatedResult<InvoiceImportRowDto>> GetFailedInvoiceImportAsync(QueryParameters<string> query, string importedBy);
        Task<int> DeleteFailedInvoiceImportByUserAsync(string importedBy);
        Task<InvoiceImportResultDto> ImportInvoiceAsync(InvoiceImportDto request, string importedBy);
        Task<InvoiceImportRowDto?> GetFailedInvoiceImportByIdAsync(int id, string importedBy);
        Task<bool> SaveFailedInvoiceImportAsync(int id, InvoiceImportRowDto dto, string importedBy);
        Task<bool> DeleteFailedInvoiceImportByIdAsync(int id, string importedBy);
    }
}

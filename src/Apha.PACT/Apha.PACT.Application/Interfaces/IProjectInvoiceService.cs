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
        Task<CopyInvoicesResultDto> CopyInvoicesAsync(CopyInvoicesDto copyDto);
    }
}

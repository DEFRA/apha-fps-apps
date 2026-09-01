using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Interfaces;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IRadTrackInvoiceService
    {
        Task<PaginatedResult<RadTrackInvoiceDto>> GetAllAsync(QueryParameters<RadTrackInvoiceFilter> parameters);
        Task<RadTrackInvoiceDto?> GetByIdAsync(int invoiceCounter);
        Task<RadTrackInvoiceDto> CreateAsync(RadTrackInvoiceDto dto);
        Task<RadTrackInvoiceDto> UpdateAsync(RadTrackInvoiceDto dto);
        Task<bool> DeleteAsync(int invoiceCounter);
        Task<RadTrackInvoiceTotalsDto> GetTotalsAsync(RadTrackInvoiceFilter? filter, string? search = null);
        Task<bool> ExistsAsync(string? project, string? contract, string? invoiceRef, int? excludeInvoiceCounter = null);

        // Lookup methods for filter dropdowns
        Task<List<string>> GetProjectsAsync();
        Task<List<int>> GetYearsAsync();
        Task<List<string>> GetContractsAsync();
        Task<List<string>> GetProgramsAsync();
    }
}

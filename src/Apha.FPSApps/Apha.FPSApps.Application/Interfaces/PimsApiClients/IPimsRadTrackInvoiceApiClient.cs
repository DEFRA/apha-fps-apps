using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsRadTrackInvoiceApiClient
    {
        Task<ApiResponseDto<List<RadTrackInvoiceDto>>> GetAllAsync(QueryParameters<string> query, string? project = null, string? contract = null, int? year = null, string? program = null);
        Task<ApiResponseDto<RadTrackInvoiceTotalsDto>> GetTotalsAsync(string? project = null, string? contract = null, int? year = null, string? program = null);
        Task<ApiResponseDto<RadTrackInvoiceDto>> GetByIdAsync(int id);
        Task<ApiResponseDto<RadTrackInvoiceDto>> CreateAsync(RadTrackInvoiceDto dto);
        Task<ApiResponseDto<RadTrackInvoiceDto>> UpdateAsync(int id, RadTrackInvoiceDto dto);
        Task<ApiResponseDto<object>> DeleteAsync(int id);

        // Lookup methods for filter/modal dropdowns
        Task<ApiResponseDto<List<string>>> GetProjectsAsync();
        Task<ApiResponseDto<List<int>>> GetYearsAsync();
        Task<ApiResponseDto<List<string>>> GetContractsAsync();
        Task<ApiResponseDto<List<string>>> GetProgramsAsync();
    }
}

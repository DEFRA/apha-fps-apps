using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IProjectInvoiceService
    {
        Task<ApiResponseDto<List<ProjectInvoiceDto>>> GetPagedProjectInvoicesAsync(QueryParameters<string> query, string? parentProject);
        Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? parentProject);
        Task<ApiResponseDto<ProjectInvoiceDto>> GetByIdAsync(int invoiceCounter);
        Task<ApiResponseDto<ProjectInvoiceDto>> CreateAsync(ProjectInvoiceDto dto);
        Task<ApiResponseDto<ProjectInvoiceDto>> UpdateAsync(int invoiceCounter, ProjectInvoiceDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(int invoiceCounter);
    }
}

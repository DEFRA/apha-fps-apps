using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class ProjectInvoiceService : IProjectInvoiceService
    {
        private readonly IPactApiClient _pactClient;

        public ProjectInvoiceService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<ProjectInvoiceDto>>> GetPagedProjectInvoicesAsync(QueryParameters<string> query, string? parentProject)
            => await _pactClient.PactProjectInvoice.GetPagedProjectInvoicesAsync(query, parentProject);
        public async Task<ApiResponseDto<List<ProjectInvoiceDto>>> GetPagedProjectInvoiceManualAsync(QueryParameters<string> query, string? parentProject)
            => await _pactClient.PactProjectInvoice.GetPagedProjectInvoiceManualAsync(query, parentProject);

        public async Task<ApiResponseDto<List<ProjectInvoiceDto>>> GetPagedProjectInvoicesByMonthAsync(QueryParameters<string> query, int? month)
            => await _pactClient.PactProjectInvoice.GetPagedProjectInvoicesByMonthAsync(query, month);

        public async Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? parentProject)
            => await _pactClient.PactProjectInvoice.GetTotalAmountAsync(parentProject);

        public async Task<ApiResponseDto<ProjectInvoiceDto>> GetByIdAsync(int invoiceCounter)
            => await _pactClient.PactProjectInvoice.GetByIdAsync(invoiceCounter);

        public async Task<ApiResponseDto<ProjectInvoiceDto>> CreateAsync(ProjectInvoiceDto dto)
            => await _pactClient.PactProjectInvoice.CreateAsync(dto);

        public async Task<ApiResponseDto<ProjectInvoiceDto>> UpdateAsync(int invoiceCounter, ProjectInvoiceDto dto)
            => await _pactClient.PactProjectInvoice.UpdateAsync(invoiceCounter, dto);

        public async Task<ApiResponseDto<bool>> DeleteAsync(int invoiceCounter)
            => await _pactClient.PactProjectInvoice.DeleteAsync(invoiceCounter);

        public async Task<ApiResponseDto<MonthlyInvoicesPivotDto>> GetMonthlyInvoicesSummaryAsync(QueryParameters<string> query)
            => await _pactClient.PactProjectInvoice.GetMonthlyInvoicesSummaryAsync(query);

        public async Task<ApiResponseDto<bool>> CopyInvoicesAsync(CopyInvoicesDto request)
            => await _pactClient.PactProjectInvoice.CopyInvoicesAsync(request);
    }
}

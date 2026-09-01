
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class RadTrackInvoiceService : IRadTrackInvoiceService
    {
       
        private readonly IPimsApiClient _client;

        public RadTrackInvoiceService(IPimsApiClient client)
        {
            _client = client;
        }
       
        public async Task<ApiResponseDto<List<RadTrackInvoiceDto>>> GetAllAsync(
            QueryParameters<string> query,
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null)
            => await _client.PimsRadTrackInvoice.GetAllAsync(query, project, contract, year, program);
       
        public async Task<ApiResponseDto<RadTrackInvoiceTotalsDto>> GetTotalsAsync(
            string? project = null,
            string? contract = null,
            int? year = null,
            string? program = null,
            string? search = null)
            => await _client.PimsRadTrackInvoice.GetTotalsAsync(project, contract, year, program, search);
        
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> GetByIdAsync(int id)
            => await _client.PimsRadTrackInvoice.GetByIdAsync(id);
        
        public async Task<ApiResponseDto<RadTrackInvoiceDto>> CreateAsync(RadTrackInvoiceDto dto)
            => await _client.PimsRadTrackInvoice.CreateAsync(dto);

        public async Task<ApiResponseDto<RadTrackInvoiceDto>> UpdateAsync(int id, RadTrackInvoiceDto dto)
            => await _client.PimsRadTrackInvoice.UpdateAsync(id, dto);

        public async Task<ApiResponseDto<object>> DeleteAsync(int id)
            => await _client.PimsRadTrackInvoice.DeleteAsync(id);

        public async Task<ApiResponseDto<List<string>>> GetProjectsAsync()
            => await _client.PimsRadTrackInvoice.GetProjectsAsync();

        public async Task<ApiResponseDto<List<int>>> GetYearsAsync()
            => await _client.PimsRadTrackInvoice.GetYearsAsync();

        public async Task<ApiResponseDto<List<string>>> GetContractsAsync()
            => await _client.PimsRadTrackInvoice.GetContractsAsync();

        public async Task<ApiResponseDto<List<string>>> GetProgramsAsync()
            => await _client.PimsRadTrackInvoice.GetProgramsAsync();
    }
}

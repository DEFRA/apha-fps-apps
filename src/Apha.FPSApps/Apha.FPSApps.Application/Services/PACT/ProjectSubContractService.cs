using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class ProjectSubContractService : IProjectSubContractService
    {
        private readonly IPactApiClient _pactClient;

        public ProjectSubContractService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project)
            => await _pactClient.PactProjectSubContract.GetPagedProjectSubContractsAsync(query, project);

        public async Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? project)
            => await _pactClient.PactProjectSubContract.GetTotalAmountAsync(project);

        public async Task<ApiResponseDto<ProjectSubContractDto>> GetByIdAsync(int subContCounter)
            => await _pactClient.PactProjectSubContract.GetByIdAsync(subContCounter);

        public async Task<ApiResponseDto<ProjectSubContractDto>> CreateAsync(ProjectSubContractDto dto)
            => await _pactClient.PactProjectSubContract.CreateAsync(dto);

        public async Task<ApiResponseDto<ProjectSubContractDto>> UpdateAsync(int subContCounter, ProjectSubContractDto dto)
            => await _pactClient.PactProjectSubContract.UpdateAsync(subContCounter, dto);

        public async Task<ApiResponseDto<bool>> DeleteAsync(int subContCounter)
            => await _pactClient.PactProjectSubContract.DeleteAsync(subContCounter);

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetFpsProjectSubContractsAsync(QueryParameters<string> query, string? project)
            => await _pactClient.PactProjectSubContract.GetFpsProjectSubContractsAsync(query, project);

        public async Task<ApiResponseDto<decimal>> GetFpsProjectSubContractTotalAmountAsync(string? project)
            => await _pactClient.PactProjectSubContract.GetFpsProjectSubContractTotalAmountAsync(project);

        public async Task<ApiResponseDto<MonthlySubContractsPivotDto>> GetMonthlySubContractsSummaryAsync(QueryParameters<string> query)
           => await _pactClient.PactProjectSubContract.GetMonthlySubContractsSummaryAsync(query);
    }
}

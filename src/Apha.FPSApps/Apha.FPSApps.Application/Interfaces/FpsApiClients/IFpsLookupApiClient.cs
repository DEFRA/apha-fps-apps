using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsLookupApiClient
    {
        Task<ApiResponseDto<List<StatusDto>>> GetAllStatusesAsync();
        Task<ApiResponseDto<List<DiseaseDto>>> GetAllDiseasesAsync();
        Task<ApiResponseDto<List<CustomerDto>>> GetAllCustomersAsync();
        Task<ApiResponseDto<List<ContractDto>>> GetAllContractsAsync();
        Task<ApiResponseDto<List<ContractDto>>> GetAllPactContractsAsync();
        Task<ApiResponseDto<List<ProjectGroupDto>>> GetAllProjectGroupsAsync();
    }
}

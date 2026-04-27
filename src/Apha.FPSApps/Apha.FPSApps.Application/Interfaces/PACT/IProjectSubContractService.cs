using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IProjectSubContractService
    {
        Task<ApiResponseDto<List<ProjectSubContractDto>>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project);
        Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? project);
        Task<ApiResponseDto<ProjectSubContractDto>> GetByIdAsync(int subContCounter);
        Task<ApiResponseDto<ProjectSubContractDto>> CreateAsync(ProjectSubContractDto dto);
        Task<ApiResponseDto<ProjectSubContractDto>> UpdateAsync(int subContCounter, ProjectSubContractDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(int subContCounter);
        Task<ApiResponseDto<List<ProjectSubContractDto>>> GetAnimalSubContractsAsync(QueryParameters<string> query, string? project);
        Task<ApiResponseDto<decimal>> GetAnimalTotalAmountAsync(string? project);
    }
}

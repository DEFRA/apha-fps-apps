using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IProjectSubContractService
    {
        Task<PaginatedResult<ProjectSubContractDto>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project);
        Task<decimal> GetTotalAmountAsync(string? project);
        Task<ProjectSubContractDto?> GetByIdAsync(int subContCounter);
        Task<ProjectSubContractDto> CreateAsync(ProjectSubContractDto dto);
        Task<ProjectSubContractDto> UpdateAsync(ProjectSubContractDto dto);
        Task<bool> DeleteAsync(int subContCounter);
    }
}

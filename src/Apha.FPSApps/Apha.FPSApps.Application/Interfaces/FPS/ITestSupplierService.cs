using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface ITestSupplierService
    {
        Task<ApiResponseDto<List<TestSupplierViewDto>>> GetPagedAsync(QueryParameters<string> query, string testCode, bool showRejected);
        Task<ApiResponseDto<TestSupplierViewDto>> GetViewByIdAsync(string testCode, string buyer);
        Task<ApiResponseDto<FpsTestRequirementDto>> GetByIdAsync(string testCode, string buyer);
        Task<ApiResponseDto<FpsTestRequirementDto>> CreateAsync(FpsTestRequirementDto dto);
        Task<ApiResponseDto<FpsTestRequirementDto>> UpdateAsync(FpsTestRequirementDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string buyer);
    }
}

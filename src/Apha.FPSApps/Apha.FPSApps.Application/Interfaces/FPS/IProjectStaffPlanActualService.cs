using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProjectStaffPlanActualService
    {
        Task<ApiResponseDto<List<TimeCostCalcsViewDto>>> GetTimeCostCalcsByProjectAsync(QueryParameters<string> query, string projectCode);
        Task<ApiResponseDto<TimeCostCalcsTotalsDto>> GetTotalActualByProjectAsync(string projectCode);
        Task<ApiResponseDto<bool>> DeleteTimeCostCalcsAsync(string workgroup, string jobCode, string project, double month, string staffId);
    }
}

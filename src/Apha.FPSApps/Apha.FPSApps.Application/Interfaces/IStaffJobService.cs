using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces
{
    public interface IStaffJobService
    {
        Task<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>> GetStaffWorkgroupLookupAsync();
        Task<PaginatedApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobsAsync(QueryParameters<string> staffJobReq);
        Task<ApiResponseDto<StaffJobDto>> GetStaffJobByIdAsync(string staffId);
        Task<ApiResponseDto<StaffJobDto>> CreateStaffJobAsync(StaffJobDto staffJob);
        Task<ApiResponseDto<StaffJobDto>> UpdateStaffJobAsync(string staffId, StaffJobDto staffJob);
        Task<ApiResponseDto<bool>> DeleteStaffJobAsync(string staffId, string jobCode);
    }
}

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsStaffJobApiClient
    {
        Task<ApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobAsync(QueryParameters<string> staffJob, string jobCode);
        Task<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>> GetStaffWorkgroupLookupAsync();
        Task<ApiResponseDto<decimal?>> GetStaffChargeRate(string staffId, string jobcode);
        Task<ApiResponseDto<decimal>> GetTotalStaffCostAsync(string jobCode);
        Task<ApiResponseDto<StaffJobDto>> GetStaffJobByIdAsync(string staffId, string jobCode);
        Task<ApiResponseDto<StaffJobViewDto?>> GetViewByStaffIdAsync(string staffId, string jobCode);
        Task<ApiResponseDto<StaffJobDto>> CreateStaffJobAsync(StaffJobDto staffJob); 
        Task<ApiResponseDto<StaffJobDto>> UpdateStaffJobAsync(StaffJobDto staffJob);
        Task<ApiResponseDto<bool>> DeleteStaffJobAsync(string staffId, string jobCode);      

    }
}

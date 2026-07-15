using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsResourceMain2ApiClient
    {
        Task<ApiResponseDto<List<ResourceStaffAllocationDto>>> GetStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade);
        Task<ApiResponseDto<List<ResourceStaffJobDto>>> GetStaffJobsByStaffIdAsync(int staffId);
    }
}

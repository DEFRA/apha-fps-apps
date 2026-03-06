using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces
{
    public interface IStaffJobService
    {
        Task<PaginatedApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobsAsync(QueryParameters<string> staffJobReq);
    }
}

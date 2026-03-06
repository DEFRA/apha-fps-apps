using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.DTOs;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services
{
    public class StaffJobService : IStaffJobService
    {
        private readonly IFpsApiClient _fpsClient;

        public StaffJobService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<PaginatedApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobsAsync(QueryParameters<string> staffJobReq)
        {
            var staffJobs = await _fpsClient.FpsStaffJob.GetAllStaffJobAsync(staffJobReq);
            return staffJobs;
        }
    }
}

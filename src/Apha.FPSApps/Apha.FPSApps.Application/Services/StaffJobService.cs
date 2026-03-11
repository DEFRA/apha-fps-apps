using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
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

        public async Task<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>> GetStaffWorkgroupLookupAsync()
        {
            var workgroups = await _fpsClient.FpsStaffJob.GetStaffWorkgroupLookupAsync();
            return workgroups;
        }

        public async Task<ApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobsAsync(QueryParameters<string> staffJobReq)
        {
            var staffJobs = await _fpsClient.FpsStaffJob.GetAllStaffJobAsync(staffJobReq);
            return staffJobs;
        }

        public async Task<ApiResponseDto<StaffJobDto>> GetStaffJobByIdAsync(string staffId)
        {
            var staffJob = await _fpsClient.FpsStaffJob.GetStaffJobByIdAsync(staffId);
            return staffJob;
        }

        public async Task<ApiResponseDto<StaffJobDto>> CreateStaffJobAsync(StaffJobDto staffJob)
        {
            var result = await _fpsClient.FpsStaffJob.CreateStaffJobAsync(staffJob);
            return result;
        }

        public async Task<ApiResponseDto<StaffJobDto>> UpdateStaffJobAsync(string staffId, StaffJobDto staffJob)
        {
            var result = await _fpsClient.FpsStaffJob.UpdateStaffJobAsync(staffJob);
            return result;
        }

        public async Task<ApiResponseDto<bool>> DeleteStaffJobAsync(string staffId, string jobCode)
        {
            var result = await _fpsClient.FpsStaffJob.DeleteStaffJobAsync(staffId, jobCode);
            return result;
        }
    }
}

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ResourceMain2Service : IResourceMain2Service
    {
        private readonly IFpsApiClient _fpsClient;

        public ResourceMain2Service(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ResourceStaffAllocationDto>>> GetStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade)
        {
            return await _fpsClient.FpsResourceMain2.GetStaffAllocationsByWorkGroupGradeAsync(workGroupGrade);
        }

        public async Task<ApiResponseDto<List<ResourceStaffJobDto>>> GetStaffJobsByStaffIdAsync(int staffId)
        {
            return await _fpsClient.FpsResourceMain2.GetStaffJobsByStaffIdAsync(staffId);
        }
    }
}

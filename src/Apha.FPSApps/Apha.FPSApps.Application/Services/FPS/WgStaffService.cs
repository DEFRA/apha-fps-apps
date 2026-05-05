using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class WgStaffService : IWgStaffService
    {
        private readonly IFpsApiClient _fpsClient;

        public WgStaffService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<WgEmployeeViewDto>>> GetWgStaffAsync(QueryParameters<string> query, string wgGrade)
        {
            return await _fpsClient.FpsWgStaff.GetWgStaffAsync(query, wgGrade);
        }

        public async Task<ApiResponseDto<WgEmployeeDto>> GetWgEmployeeByIdAsync(string pactId)
        {
            return await _fpsClient.FpsWgStaff.GetWgEmployeeByIdAsync(pactId);
        }

        public async Task<ApiResponseDto<WgEmployeeDto>> UpdateWgEmployeeAsync(WgEmployeeDto dto)
        {
            return await _fpsClient.FpsWgStaff.UpdateWgEmployeeAsync(dto);
        }

        public async Task<ApiResponseDto<bool>> DeleteWgEmployeeAsync(string pactId)
        {
            return await _fpsClient.FpsWgStaff.DeleteWgEmployeeAsync(pactId);
        }
    }
}

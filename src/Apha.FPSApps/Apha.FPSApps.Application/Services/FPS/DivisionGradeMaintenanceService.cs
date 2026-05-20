using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class DivisionGradeMaintenanceService : IDivisionGradeMaintenanceService
    {
        private readonly IFpsApiClient _fpsClient;

        public DivisionGradeMaintenanceService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<DivisionGradeMaintenanceDto>>> GetAllPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsMaintDG.GetAllPagedAsync(query);
        }

        public async Task<ApiResponseDto<DivisionGradeMaintenanceDto>> GetByIdAsync(string divisionGradeCode)
        {
            return await _fpsClient.FpsMaintDG.GetByIdAsync(divisionGradeCode);
        }

        public async Task<ApiResponseDto<DivisionGradeMaintenanceDto>> CreateAsync(DivisionGradeMaintenanceDto dto)
        {
            return await _fpsClient.FpsMaintDG.CreateAsync(dto);
        }

        public async Task<ApiResponseDto<DivisionGradeMaintenanceDto>> UpdateAsync(string originalCode, DivisionGradeMaintenanceDto dto)
        {
            return await _fpsClient.FpsMaintDG.UpdateAsync(originalCode, dto);
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string divisionGradeCode)
        {
            return await _fpsClient.FpsMaintDG.DeleteAsync(divisionGradeCode);
        }

        public async Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync()
        {
            return await _fpsClient.FpsMaintDG.GetAllGradeCodesAsync();
        }
    }
}

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class DivisionGradeService : IDivisionGradeService
    {
        private readonly IFpsApiClient _fpsClient;

        public DivisionGradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<DivisionGradeDto>>> GetAllPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsMaintDG.GetAllPagedAsync(query);
        }

        public async Task<ApiResponseDto<DivisionGradeDto>> GetByIdAsync(string divisionGradeCode)
        {
            return await _fpsClient.FpsMaintDG.GetByIdAsync(divisionGradeCode);
        }

        public async Task<ApiResponseDto<DivisionGradeDto>> CreateAsync(DivisionGradeDto dto)
        {
            return await _fpsClient.FpsMaintDG.CreateAsync(dto);
        }

        public async Task<ApiResponseDto<DivisionGradeDto>> UpdateAsync(string originalCode, DivisionGradeDto dto)
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

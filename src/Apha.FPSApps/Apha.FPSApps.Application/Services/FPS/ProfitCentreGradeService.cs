using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProfitCentreGradeService : IProfitCentreGradeService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProfitCentreGradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetProfitCentreGradesAsync(string profitCentre)
        {
            return await _fpsClient.FpsProfitCentreGrade.GetProfitCentreGradesAsync(new QueryParameters<string>(), profitCentre);
        }

        public async Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetAllPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsProfitCentreGrade.GetAllPagedAsync(query);
        }

        public async Task<ApiResponseDto<ProfitCentreGradeDto>> GetByIdAsync(string pcGrade)
        {
            return await _fpsClient.FpsProfitCentreGrade.GetByIdAsync(pcGrade);
        }

        public async Task<ApiResponseDto<ProfitCentreGradeDto>> CreateAsync(ProfitCentreGradeDto dto)
        {
            return await _fpsClient.FpsProfitCentreGrade.CreateAsync(dto);
        }

        public async Task<ApiResponseDto<ProfitCentreGradeDto>> UpdateAsync(string originalPcGrade, ProfitCentreGradeDto dto)
        {
            return await _fpsClient.FpsProfitCentreGrade.UpdateAsync(originalPcGrade, dto);
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string pcGrade)
        {
            return await _fpsClient.FpsProfitCentreGrade.DeleteAsync(pcGrade);
        }

        public async Task<ApiResponseDto<List<string>>> GetAllProfitCentreCodesAsync()
        {
            return await _fpsClient.FpsProfitCentreGrade.GetAllProfitCentreCodesAsync();
        }

        public async Task<ApiResponseDto<List<string>>> GetAllPcGradesAsync()
            => await _fpsClient.FpsProfitCentreGrade.GetAllPcGradesAsync();
    }
}

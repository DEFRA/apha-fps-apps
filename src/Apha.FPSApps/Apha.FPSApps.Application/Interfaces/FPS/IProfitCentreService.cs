using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProfitCentreService
    {
        Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync();
        Task<ApiResponseDto<IEnumerable<ProfitCentreDto>>> GetAllProfitCentresAsync();
        Task<ApiResponseDto<ProfitCentreDto>> GetProfitCentreByIdAsync(string profitCentre);
        Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetLayout);
    }
}

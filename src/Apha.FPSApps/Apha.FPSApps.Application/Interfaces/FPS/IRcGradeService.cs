using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IRcGradeService
    {
        Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetRcGradesAsync(string profitCentre);
    }
}

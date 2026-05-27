using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProfitCentreGradeService
    {
        Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetProfitCentreGradesAsync(string profitCentre);
    }
}

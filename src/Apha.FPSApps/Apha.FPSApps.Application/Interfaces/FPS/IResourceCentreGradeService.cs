using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IResourceCentreGradeService
    {
        Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetResourceCentreGradesAsync(string profitCentre);
    }
}

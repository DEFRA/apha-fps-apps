using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWgGradeService
    {
        Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWgGradesAsync(string pcGrade);
        Task<ApiResponseDto<bool>> DeleteWgGradeAsync(string wgGrade);
    }
}

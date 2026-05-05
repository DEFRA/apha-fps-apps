using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWorkGroupGradeService
    {
        Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkGroupGradeAsync(string pcGrade);
        Task<ApiResponseDto<bool>> DeleteWorkGroupGradeAsync(string wgGrade);
    }
}

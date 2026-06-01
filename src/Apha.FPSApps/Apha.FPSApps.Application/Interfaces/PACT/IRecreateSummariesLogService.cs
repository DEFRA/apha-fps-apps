using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IRecreateSummariesLogService
    {
        Task<ApiResponseDto<List<RecreateSummariesLogDto>>> GetAllLogsAsync();
    }
}

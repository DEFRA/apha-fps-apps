using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PACT;
public interface ISummarisedWgTimeViewService
{
    Task<ApiResponseDto<List<SummarisedWgTimeDto>>> GetSummarisedWorkgroupTimeAsync(string? workGroup); 
}

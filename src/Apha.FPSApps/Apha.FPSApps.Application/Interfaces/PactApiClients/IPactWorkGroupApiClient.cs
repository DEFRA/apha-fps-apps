using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactWorkGroupApiClient
    {
        Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync();
    }
}

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IPactApiClient _pactClient;

        public WorkGroupService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync()
            => await _pactClient.PactWorkGroup.GetAllWorkGroupsAsync();

        public async Task<ApiResponseDto<List<WorkGroupTimeCodeDto>>> GetPagedWorkGroupTimeCodesAsync(
            QueryParameters<string> query, string? workGroup, int? monthNumber)
            => await _pactClient.PactWorkGroup.GetPagedWorkGroupTimeCodesAsync(query, workGroup, monthNumber);
    }
}
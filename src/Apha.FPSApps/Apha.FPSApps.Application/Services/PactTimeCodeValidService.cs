using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services
{
    public class PactTimeCodeValidService : IPactTimeCodeValidService
    {
        private readonly IPactApiClient _pactClient;

        public PactTimeCodeValidService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetByJobCodeAsync(string jobCode, string parentProject)
            => await _pactClient.PactTimeCodeValid.GetByJobCodeAsync(jobCode, parentProject);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedTimeCodesAsync(QueryParameters<string> query, string? jobCode, string? parentProject)
            => await _pactClient.PactTimeCodeValid.GetPagedTimeCodesAsync(query, jobCode, parentProject);

        public async Task<ApiResponseDto<TimeCodeValidDto>> CreateTimeCodeValidAsync(TimeCodeValidDto item)
            => await _pactClient.PactTimeCodeValid.CreateTimeCodeValidAsync(item);

        public async Task<ApiResponseDto<TimeCodeValidDto>> UpdateTimeCodeValidAsync(TimeCodeValidDto item)
            => await _pactClient.PactTimeCodeValid.UpdateTimeCodeValidAsync(item);

        public async Task<ApiResponseDto<bool>> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
            => await _pactClient.PactTimeCodeValid.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);

        public async Task<ApiResponseDto<bool>> DeleteAllByJobCodeAsync(string jobCode, string parentProject)
            => await _pactClient.PactTimeCodeValid.DeleteAllByJobCodeAsync(jobCode, parentProject);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> CopyWorkGroupAsync(string sourceJobCode, string targetJobCode, string parentProject)
            => await _pactClient.PactTimeCodeValid.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);
    }
}

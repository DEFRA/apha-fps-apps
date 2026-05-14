using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
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

        public async Task<ApiResponseDto<TimeCodeValidDto>> GetTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
            => await _pactClient.PactTimeCodeValid.GetTimeCodeValidAsync(workGroup, timeCode, parentProject);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedTimeCodesAsync(QueryParameters<string> query, string? jobCode, string? parentProject)
            => await _pactClient.PactTimeCodeValid.GetPagedTimeCodesAsync(query, jobCode, parentProject);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedTimeCodesTestCodeAsync(QueryParameters<string> query, string? jobCode, string? testCode, string? parentProject)
            => await _pactClient.PactTimeCodeValid.GetPagedTimeCodesTestCodeAsync(query, jobCode, testCode, parentProject);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedByProjectAndTestCodeAsync(QueryParameters<string> query, string parentProject, string testCode)
            => await _pactClient.PactTimeCodeValid.GetPagedByProjectAndTestCodeAsync(query, parentProject, testCode);

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

        public async Task<ApiResponseDto<bool>> DeleteBulkAsync(BulkDeleteTimeCodeRequestDto request)
            => await _pactClient.PactTimeCodeValid.DeleteBulkAsync(request);

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> CopySelectedWorkGroupsAsync(BulkCopyWorkGroupRequestDto request)
            => await _pactClient.PactTimeCodeValid.CopySelectedWorkGroupsAsync(request);
    }
}

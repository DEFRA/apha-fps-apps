using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class ProjectJobCodeService : IProjectJobCodeService
    {
        private readonly IPactApiClient _pactClient;

        public ProjectJobCodeService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<JobCodeDto>>> GetJobCodesByProjectAsync(string parentProject)
            => await _pactClient.PactJobCode.GetJobCodesByProjectAsync(parentProject);

        public async Task<ApiResponseDto<List<JobCodeDto>>> GetPagedJobCodesAsync(QueryParameters<string> query, string? parentProject)
            => await _pactClient.PactJobCode.GetPagedJobCodesAsync(query, parentProject);

        public async Task<ApiResponseDto<JobCodeDto>> GetJobCodeByIdAsync(string jobCodeId)
            => await _pactClient.PactJobCode.GetJobCodeByIdAsync(jobCodeId);

        public async Task<ApiResponseDto<List<string>>> GetTypesAsync()
            => await _pactClient.PactJobCode.GetTypesAsync();

        public async Task<ApiResponseDto<JobCodeDto>> CreateJobCodeAsync(JobCodeDto jobCode)
            => await _pactClient.PactJobCode.CreateJobCodeAsync(jobCode);

        public async Task<ApiResponseDto<JobCodeDto>> UpdateJobCodeAsync(JobCodeDto jobCode)
            => await _pactClient.PactJobCode.UpdateJobCodeAsync(jobCode);

        public async Task<ApiResponseDto<bool>> DeleteJobCodeAsync(string jobCodeId)
            => await _pactClient.PactJobCode.DeleteJobCodeAsync(jobCodeId);

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync()
            => await _pactClient.PactWorkGroup.GetAllWorkGroupsAsync();
    }
}

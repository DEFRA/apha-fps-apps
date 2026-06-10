using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactJobCodeApiClient
    {
        Task<ApiResponseDto<List<JobCodeDto>>> GetJobCodesAsync();
        Task<ApiResponseDto<List<JobCodeDto>>> GetJobCodesByProjectAsync(string parentProject);
        Task<ApiResponseDto<List<JobCodeDto>>> GetPagedJobCodesAsync(QueryParameters<string> query, string? parentProject);
        Task<ApiResponseDto<JobCodeDto>> GetJobCodeByIdAsync(string jobCodeId);
        Task<ApiResponseDto<List<string>>> GetTypesAsync();
        Task<ApiResponseDto<JobCodeDto>> CreateJobCodeAsync(JobCodeDto jobCode);
        Task<ApiResponseDto<JobCodeDto>> UpdateJobCodeAsync(JobCodeDto jobCode);
        Task<ApiResponseDto<bool>> DeleteJobCodeAsync(string jobCodeId);
        Task<ApiResponseDto<IEnumerable<FpsZtJobCodeDto>>> GetZtJobCodesAsync();
    }
}

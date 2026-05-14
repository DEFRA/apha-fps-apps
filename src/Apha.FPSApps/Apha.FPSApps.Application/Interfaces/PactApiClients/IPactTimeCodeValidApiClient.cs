using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactTimeCodeValidApiClient
    {
        Task<ApiResponseDto<List<TimeCodeValidDto>>> GetByJobCodeAsync(string jobCode, string parentProject);
        Task<ApiResponseDto<TimeCodeValidDto>> GetTimeCodeValidAsync(string workGroup, string timeCode, string parentProject);
        Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedTimeCodesAsync(QueryParameters<string> query, string? jobCode, string? parentProject);
        Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedTimeCodesTestCodeAsync(QueryParameters<string> query, string? jobCode, string? testCode, string? parentProject);
        Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedByProjectAndTestCodeAsync(QueryParameters<string> query, string parentProject, string testCode);
        Task<ApiResponseDto<TimeCodeValidDto>> CreateTimeCodeValidAsync(TimeCodeValidDto item);
        Task<ApiResponseDto<TimeCodeValidDto>> UpdateTimeCodeValidAsync(TimeCodeValidDto item);
        Task<ApiResponseDto<bool>> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject);
        Task<ApiResponseDto<bool>> DeleteAllByJobCodeAsync(string jobCode, string parentProject);
        Task<ApiResponseDto<List<TimeCodeValidDto>>> CopyWorkGroupAsync(string sourceJobCode, string targetJobCode, string parentProject);
        Task<ApiResponseDto<bool>> DeleteBulkAsync(BulkDeleteTimeCodeRequestDto request);
        Task<ApiResponseDto<List<TimeCodeValidDto>>> CopySelectedWorkGroupsAsync(BulkCopyWorkGroupRequestDto request);
    }
}

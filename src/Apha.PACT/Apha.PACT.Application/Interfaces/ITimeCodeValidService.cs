using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface ITimeCodeValidService
    {
        Task<IEnumerable<TimeCodeValidDto>> GetByJobCodeAsync(string jobCode, string parentProject);
        Task<PaginatedResult<TimeCodeValidDto>> GetPagedTimeCodesAsync(QueryParameters<string> query, string? jobCode, string? parentProject);
        Task<TimeCodeValidDto?> GetTimeCodeValidAsync(string workGroup, string timeCode, string parentProject);
        Task<TimeCodeValidDto> CreateTimeCodeValidAsync(TimeCodeValidDto timeCodeValid);
        Task<TimeCodeValidDto> UpdateTimeCodeValidAsync(TimeCodeValidDto timeCodeValid);
        Task<bool> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject);
        Task<bool> DeleteAllByJobCodeAsync(string jobCode, string parentProject);
        Task<IEnumerable<TimeCodeValidDto>> CopyWorkGroupAsync(string sourceJobCode, string targetJobCode, string parentProject);
    }
}

using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IMonthlyTimeService
    {
        Task<PaginatedResult<MonthlyTimeLogDto>> SearchAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete);
    }
}

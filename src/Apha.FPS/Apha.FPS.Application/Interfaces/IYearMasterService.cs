using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IYearMasterService
    {
        Task<IEnumerable<YearMasterDto>> GetAllFpsYearsAsync();
        Task<PaginatedResult<YearMasterDto>> GetAllFpsYearsPagedAsync(QueryParameters<int> query);
        Task<YearMasterDto?> GetFpsYearByIdAsync(int fpsYear);
    }
}

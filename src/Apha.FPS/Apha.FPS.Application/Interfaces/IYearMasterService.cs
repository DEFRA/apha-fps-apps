using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IYearMasterService
    {
        Task<IEnumerable<YearMasterDto>> GetAllYearMastersAsync();
        Task<PaginatedResult<YearMasterDto>> GetAllYearMastersAsync(QueryParameters<int> query);
        Task<YearMasterDto?> GetYearMasterByIdAsync(int fpsYear);
    }
}

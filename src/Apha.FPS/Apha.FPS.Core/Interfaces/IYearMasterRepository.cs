using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IYearMasterRepository
    {
        Task<IEnumerable<YearMaster>> GetAllFpsYearsAsync();
        Task<PagedData<YearMaster>> GetAllFpsYearsPagedAsync(PaginationParameters<int> query);
        Task<YearMaster?> GetFpsYearByIdAsync(int fpsYear);
    }
}

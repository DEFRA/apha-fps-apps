using Apha.FPS.Core.Enities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IYearMasterRepository
    {
        Task<IEnumerable<YearMaster>> GetAllYearMastersAsync();
        Task<PagedData<YearMaster>> GetAllYearMastersAsync(PaginationParameters<int> query);
        Task<YearMaster?> GetYearMasterByIdAsync(int fpsYear);
    }
}

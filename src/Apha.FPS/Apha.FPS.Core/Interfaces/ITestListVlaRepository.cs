using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for TestOrProduct VLA list operations.
    /// Scoped to the frmTestList / fsubTest_MainList VLA use case.
    /// Composite PK on fps.testorproduct: ItemCode + FpsYear.
    /// No infrastructure-specific code — Core layer only.
    /// </summary>
    public interface ITestListVlaRepository
    {
        //   string filter used as search prefix across itemcode / itemdescription
        Task<PagedData<TestOrProduct>> GetPagedAsync(PaginationParameters<string> query);

        Task<IEnumerable<TestOrProduct>> GetAllByYearAsync();

        Task<TestOrProduct?> GetByKeyAsync(string itemCode);

        Task<bool> ExistsAsync(string itemCode);

        Task<TestOrProduct> AddAsync(TestOrProduct testOrProduct);

        Task<TestOrProduct> UpdateAsync(TestOrProduct testOrProduct);

        Task<bool> DeleteAsync(string itemCode);
    }
}

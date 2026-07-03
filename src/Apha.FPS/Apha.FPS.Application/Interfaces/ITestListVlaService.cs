using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for TestOrProduct VLA list business operations.
    /// Orchestrates repository calls and enforces business rules extracted from VBA/SP guards.
    /// Composite PK on fps.testorproduct: ItemCode + FpsYear.
    /// </summary>
    public interface ITestListVlaService
    {
        Task<PaginatedResult<TestListVlaDto>> GetAllAsync(QueryParameters<string> query, int fpsYear);

        Task<IEnumerable<TestListVlaDto>> GetAllByYearAsync(int fpsYear);

        Task<TestListVlaDto?> GetByKeyAsync(string itemCode, int fpsYear);

        Task<TestListVlaDto> CreateAsync(TestListVlaDto dto);

        Task<TestListVlaDto> UpdateAsync(string itemCode, int fpsYear, TestListVlaDto dto);

        Task<bool> DeleteAsync(string itemCode, int fpsYear);
    }
}

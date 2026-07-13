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
        Task<PaginatedResult<TestListVlaDto>> GetAllAsync(QueryParameters<string> query);

        Task<IEnumerable<TestListVlaDto>> GetAllByYearAsync();

        Task<TestListVlaDto?> GetByKeyAsync(string itemCode);

        Task<TestListVlaDto> CreateAsync(TestListVlaDto dto);

        Task<TestListVlaDto> UpdateAsync(string itemCode, TestListVlaDto dto);

        Task<bool> DeleteAsync(string itemCode);
    }
}

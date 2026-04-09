using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface ITestReqmtRepository
    {
        Task<PagedData<TestRequirement>> GetPagedByTestCodeAsync(PaginationParameters<string> query, string testCode);
        Task<PagedData<TestReqmtDetail>> GetPagedWithDetailsAsync(PaginationParameters<string> query, string testCode);
        Task<TestRequirement?> GetByIdAsync(string testCode, string buyer);
        Task<TestReqmtDetail?> GetDetailByIdAsync(string testCode, string buyer);
        Task<TestReqmtDetail?> GetPricingAsync(string testCode, string? projectCode);
        Task<IEnumerable<TestReqmtDetail>> GetAllForExportAsync(string testCode, string? filterJson);
        Task<bool> ExistsByTestBuyerCodeAsync(string testBuyerCode);
        Task<bool> ExistsByTestCodeAndBuyerInMonthlyOutputAsync(string testCode, string buyer);
        Task<TestRequirement> AddAsync(TestRequirement entity);
        Task<TestRequirement> UpdateAsync(TestRequirement entity);
        Task<bool> DeleteAsync(string testCode, string buyer);
    }
}

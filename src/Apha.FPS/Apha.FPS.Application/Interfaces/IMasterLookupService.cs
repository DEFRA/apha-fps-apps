using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IMasterLookupService
    {
        Task<IEnumerable<string>> GetAllMasterLookupsAsync();

        Task<IEnumerable<string>> GetLookupItemsAsync(string tableName);

        Task<PaginatedResult<string>> GetLookupItemsPagedAsync(string tableName, QueryParameters<string> query);

        Task<bool> CreateLookupItemAsync(string tableName, string value);

        Task<bool> UpdateLookupItemAsync(string tableName, string originalValue, string newValue);

        Task<bool> DeleteLookupItemAsync(string tableName, string value);
    }
}

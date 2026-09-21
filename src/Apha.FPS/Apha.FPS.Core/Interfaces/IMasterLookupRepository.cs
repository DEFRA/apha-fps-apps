using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IMasterLookupRepository
    {
        Task<IEnumerable<MasterLookup>> GetAllMasterLookupsAsync();

        /// <summary>
        /// Returns all values from the single-column lookup table identified by <paramref name="tableName"/>.
        /// The table name is validated against the registered master lookup tables before any SQL runs.
        /// </summary>
        Task<IEnumerable<string>> GetLookupItemsAsync(string tableName);

        /// <summary>
        /// Returns a paginated, filtered and sorted set of values from the single-column lookup table
        /// identified by <paramref name="tableName"/>. Filtering, searching, sorting and paging are all
        /// applied within the repository against the underlying table.
        /// </summary>
        Task<PagedData<string>> GetLookupItemsPagedAsync(string tableName, PaginationParameters<string> query);

        /// <summary>
        /// Inserts a new value into the specified lookup table.
        /// </summary>
        Task<bool> CreateLookupItemAsync(string tableName, string value);

        /// <summary>
        /// Updates an existing value in the specified lookup table.
        /// </summary>
        Task<bool> UpdateLookupItemAsync(string tableName, string originalValue, string newValue);

        /// <summary>
        /// Deletes a value from the specified lookup table.
        /// </summary>
        Task<bool> DeleteLookupItemAsync(string tableName, string value);
    }
}

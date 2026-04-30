using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for Division entity operations.
    /// </summary>
    public interface IDivisionRepository
    {
        /// <summary>
        /// Retrieves all divisions with their related agency information.
        /// </summary>
        /// <returns>Collection of all divisions.</returns>
        Task<List<Division>> GetAllDivisionsAsync();

        /// <summary>
        /// Retrieves a paginated list of divisions.
        /// </summary>
        /// <param name="query">Pagination and filtering parameters.</param>
        /// <returns>Paginated result set of divisions.</returns>
        Task<PagedData<Division>> GetAllDivisionsPagedAsync(PaginationParameters<string> query);

        /// <summary>
        /// Retrieves a single division by its name (primary key).
        /// </summary>
        /// <param name="divName">Division name (case-insensitive).</param>
        /// <returns>Division entity if found; null otherwise.</returns>
        Task<Division?> GetDivisionByNameAsync(string divName);

        /// <summary>
        /// Creates a new division record.
        /// </summary>
        /// <param name="division">Division entity to create.</param>
        /// <returns>Created division entity with generated identifier.</returns>
        Task<Division> CreateDivisionAsync(Division division);

        /// <summary>
        /// Updates an existing division record.
        /// </summary>
        /// <param name="originalDivName">Original division name to identify the record (primary key).</param>
        /// <param name="division">Division entity with updated values (may include new DivName).</param>
        /// <returns>Updated division entity.</returns>
        Task<Division> UpdateDivisionAsync(string originalDivName, Division division);

        /// <summary>
        /// Deletes a division record by name.
        /// </summary>
        /// <param name="divName">Division name to delete.</param>
        /// <returns>True if deleted; false if not found.</returns>
        Task<bool> DeleteDivisionAsync(string divName);

        /// <summary>
        /// Checks if a division with the given name already exists.
        /// </summary>
        /// <param name="divName">Division name to check.</param>
        /// <returns>True if exists; false otherwise.</returns>
        Task<bool> DivisionExistsAsync(string divName);

        /// <summary>
        /// Checks if a division name is referenced in other tables as a foreign key.
        /// </summary>
        /// <param name="divName">Division name to check for references.</param>
        /// <returns>List of table names where the division name is referenced. Empty list if no references.</returns>
        Task<List<string>> GetDivisionForeignKeyReferencesAsync(string divName);
    }
}

/*
 * TRANSFORMENGINE MIGRATION — IAccountGroupService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New service interface created for AccountGroup (CSG7) CRUD (Tab 3 of frmMaintainance)
 *   - Full async CRUD: GetAll, GetByCsg7Group, Add, Update, Delete
 *   - Service methods map to backend routes: GET /api/v1/AccountGroup, POST, PUT, DELETE
 *   - No pagination on AccountGroup — typically small dataset (all groups loaded for dropdown + grid)
 *
 * PRESERVED:
 *   - Async-only signatures per Application layer convention
 *   - No infrastructure-specific types (no DbContext, EF references)
 *   - Uses AccountGroupDto as the service contract — no entity types exposed
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Csg7Group max length (varchar 15) validation is enforced in service
 */

using Apha.Costbook.Application.Dtos;

namespace Apha.Costbook.Application.Interfaces
{
    // TRANSFORMENGINE: Service interface for mabarchive.tblcsg7_accountgroups CRUD — covers Tab 3 (CSG7 Inflation Options) maintenance operations
    public interface IAccountGroupService
    {
        /// <summary>Returns all CSG7 account groups ordered by Csg7Group key.</summary>
        Task<List<AccountGroupDto>> GetAllAsync();

        /// <summary>Returns a single AccountGroup by primary key, or null if not found.</summary>
        Task<AccountGroupDto?> GetByCsg7GroupAsync(string csg7Group);

        /// <summary>
        /// Adds a new AccountGroup.
        /// Throws <see cref="ArgumentException"/> if Csg7Group is null/empty or already exists.
        /// </summary>
        Task<AccountGroupDto> AddAsync(AccountGroupDto dto);

        /// <summary>
        /// Updates an existing AccountGroup.
        /// Throws <see cref="KeyNotFoundException"/> if no record with the given Csg7Group exists.
        /// </summary>
        Task<AccountGroupDto> UpdateAsync(string csg7Group, AccountGroupDto dto);

        /// <summary>
        /// Deletes the AccountGroup identified by Csg7Group.
        /// Throws <see cref="KeyNotFoundException"/> if no record with the given Csg7Group exists.
        /// </summary>
        Task DeleteAsync(string csg7Group);
    }
}

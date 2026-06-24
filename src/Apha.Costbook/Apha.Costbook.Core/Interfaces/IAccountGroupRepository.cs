/*
 * TRANSFORMENGINE MIGRATION — IAccountGroupRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New repository interface created for AccountGroup (CSG7) CRUD operations
 *   - Supports CSG7 Inflation Options Tab (Tab 3) of frmMaintainance maintenance screen
 *   - Full async CRUD: GetAll, GetById, Add, Update, Delete, ExistsAsync
 *   - Source: mabarchive.tblcsg7_accountgroups (Csg7group PK varchar 15, useinflation boolean)
 *
 * PRESERVED:
 *   - Async-only signatures per Core layer convention
 *   - No infrastructure-specific types (no DbContext, EF references)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify Csg7group PK is treated as string (varchar 15) in all callers
 */

using Apha.Costbook.Core.Entities;

namespace Apha.Costbook.Core.Interfaces
{
    // TRANSFORMENGINE: New interface — full CRUD for mabarchive.tblcsg7_accountgroups; Csg7group is string PK
    public interface IAccountGroupRepository
    {
        /// <summary>Returns all CSG7 account groups ordered by Csg7group key.</summary>
        Task<List<AccountGroup>> GetAllAsync();

        /// <summary>Returns a single AccountGroup by primary key, or null if not found.</summary>
        Task<AccountGroup?> GetByCsg7GroupAsync(string csg7Group);

        /// <summary>Returns true if an AccountGroup with the given Csg7group key exists.</summary>
        Task<bool> ExistsAsync(string csg7Group);

        /// <summary>Adds a new AccountGroup and returns the persisted entity.</summary>
        Task<AccountGroup> AddAsync(AccountGroup accountGroup);

        /// <summary>Updates an existing AccountGroup and returns the updated entity.</summary>
        Task<AccountGroup> UpdateAsync(AccountGroup accountGroup);

        /// <summary>Deletes the AccountGroup identified by Csg7group. Returns true if deleted.</summary>
        Task<bool> DeleteAsync(string csg7Group);
    }
}

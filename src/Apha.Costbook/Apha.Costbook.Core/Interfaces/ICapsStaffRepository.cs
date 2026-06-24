/*
 * TRANSFORMENGINE MIGRATION — ICapsStaffRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New repository interface created for CapsStaff CRUD operations
 *   - Supports CAPS Staff Tab (Tab 5) of frmMaintainance maintenance screen
 *   - Full async CRUD: GetAll, GetById, Add, Update, Delete, ExistsAsync
 *   - Source: mabarchive.tblcapsstaff (MNumber PK, Name, Dt2Number)
 *
 * PRESERVED:
 *   - Async-only signatures per Core layer convention
 *   - No infrastructure-specific types (no DbContext, EF references)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify MNumber is treated as a string key (varchar 50) in all callers
 */

using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;

namespace Apha.Costbook.Core.Interfaces
{
    // TRANSFORMENGINE: New interface — full CRUD for mabarchive.tblcapsstaff; MNumber is string PK
    public interface ICapsStaffRepository
    {
        /// <summary>Returns all CAPS staff members ordered by MNumber.</summary>
        Task<List<CapsStaff>> GetAllAsync();

        /// <summary>Returns a paginated list of CAPS staff members.</summary>
        Task<PagedData<CapsStaff>> GetPaginatedAsync(PaginationParameters<string> queryFilter);

        /// <summary>Returns a single CAPS staff member by primary key, or null if not found.</summary>
        Task<CapsStaff?> GetByMNumberAsync(string mNumber);

        /// <summary>Returns true if a CAPS staff member with the given MNumber exists.</summary>
        Task<bool> ExistsAsync(string mNumber);

        /// <summary>Adds a new CAPS staff member and returns the persisted entity.</summary>
        Task<CapsStaff> AddAsync(CapsStaff capsStaff);

        /// <summary>Updates an existing CAPS staff member and returns the updated entity.</summary>
        Task<CapsStaff> UpdateAsync(CapsStaff capsStaff);

        /// <summary>Deletes the CAPS staff member identified by MNumber. Returns true if deleted.</summary>
        Task<bool> DeleteAsync(string mNumber);
    }
}

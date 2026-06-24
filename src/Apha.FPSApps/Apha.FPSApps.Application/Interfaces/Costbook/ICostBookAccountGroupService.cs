/*
 * TRANSFORMENGINE MIGRATION — ICostBookAccountGroupService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend service interface created for frmMaintainance Tab 3 (CSG7 Inflation Options)
 *   - Mirrors ICostBookAccountGroupApiClient signatures exactly (thin delegate pattern)
 *   - GetAllAccountGroupsAsync()       → delegates to CostbookAccountGroup.GetAllAccountGroupsAsync()
 *   - GetAccountGroupAsync()           → delegates to CostbookAccountGroup.GetAccountGroupAsync()
 *   - AddAccountGroupAsync()           → delegates to CostbookAccountGroup.AddAccountGroupAsync()
 *   - UpdateAccountGroupAsync()        → delegates to CostbookAccountGroup.UpdateAccountGroupAsync()
 *   - DeleteAccountGroupAsync()        → delegates to CostbookAccountGroup.DeleteAccountGroupAsync()
 *   - GetAllAccountGroupsAsync() also drives the CSG7 group dropdown in the AccountCategory maintenance modal (Tab 2)
 *
 * PRESERVED:
 *   - All return types and parameter signatures match ICostBookAccountGroupApiClient exactly
 *   - Csg7Group string PK (varchar 15) used for route-based lookups, updates, and deletes
 *   - AccountGroupDto used for both CRUD and lookup (GET all) since the list is also used as a dropdown source
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Csg7Group max length (varchar 15) validation enforced at service/controller level
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Interfaces.Costbook
{
    // TRANSFORMENGINE: Service interface for frmMaintainance Tab 3 (CSG7 Inflation Options) full CRUD + Tab 2 dropdown lookup
    //   thin delegate pattern, mirrors ICostBookAccountGroupApiClient
    public interface ICostBookAccountGroupService
    {
        // TRANSFORMENGINE: GET /api/v1/accountgroup → returns full list for Tab 3 grid + AccCat modal dropdown
        /// <summary>Returns all CSG7 account groups ordered by Csg7Group key.</summary>
        Task<ApiResponseDto<List<AccountGroupDto>>> GetAllAccountGroupsAsync();

        // TRANSFORMENGINE: GET /api/v1/accountgroup/{csg7Group} → single record lookup
        /// <summary>Returns a single CSG7 account group by Csg7Group key.</summary>
        Task<ApiResponseDto<AccountGroupDto>> GetAccountGroupAsync(string csg7Group);

        // TRANSFORMENGINE: POST /api/v1/accountgroup → create from Tab 3 modal (formTblCsg7)
        /// <summary>Creates a new CSG7 account group. Csg7Group must be unique (varchar 15).</summary>
        Task<ApiResponseDto<AccountGroupDto>> AddAccountGroupAsync(AccountGroupDto dto);

        // TRANSFORMENGINE: PUT /api/v1/accountgroup/{csg7Group} → update from Tab 3 edit modal
        /// <summary>Updates an existing CSG7 account group.</summary>
        Task<ApiResponseDto<AccountGroupDto>> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/accountgroup/{csg7Group} → delete from Tab 3 confirm modal
        /// <summary>Deletes the CSG7 account group identified by Csg7Group.</summary>
        Task<ApiResponseDto<bool>> DeleteAccountGroupAsync(string csg7Group);
    }
}

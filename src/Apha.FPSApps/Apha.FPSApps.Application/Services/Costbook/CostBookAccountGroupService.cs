/*
 * TRANSFORMENGINE MIGRATION — CostBookAccountGroupService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend service implementation created for frmMaintainance Tab 3 (CSG7 Inflation Options)
 *   - Implements ICostBookAccountGroupService as a thin delegate to ICostBookApiClient.CostbookAccountGroup
 *   - GetAllAccountGroupsAsync()       → _costBookClient.CostbookAccountGroup.GetAllAccountGroupsAsync()
 *   - GetAccountGroupAsync()           → _costBookClient.CostbookAccountGroup.GetAccountGroupAsync()
 *   - AddAccountGroupAsync()           → _costBookClient.CostbookAccountGroup.AddAccountGroupAsync()
 *   - UpdateAccountGroupAsync()        → _costBookClient.CostbookAccountGroup.UpdateAccountGroupAsync()
 *   - DeleteAccountGroupAsync()        → _costBookClient.CostbookAccountGroup.DeleteAccountGroupAsync()
 *   - _costBookClient is private readonly (Sonar S2933 compliance)
 *   - GetAllAccountGroupsAsync() also serves as the CSG7 dropdown source for the AccountCategory modal (Tab 2)
 *
 * PRESERVED:
 *   - No business logic — all methods are single-line return delegates (Sonar S4144 intentional)
 *   - Csg7Group string PK (varchar 15) used for route-based lookups, updates, and deletes
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Csg7Group max length (varchar 15) validation enforced at service/controller level
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Services.Costbook
{
    // TRANSFORMENGINE: Thin delegate service for frmMaintainance Tab 3 (CSG7 Inflation Options) full CRUD + Tab 2 dropdown lookup
    //   forwards to ICostBookApiClient.CostbookAccountGroup
    public class CostBookAccountGroupService : ICostBookAccountGroupService
    {
        // TRANSFORMENGINE: private readonly — Sonar S2933 compliance
        private readonly ICostBookApiClient _costBookClient;

        public CostBookAccountGroupService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }

        // TRANSFORMENGINE: delegate → GET /api/v1/accountgroup (also used as CSG7 dropdown source for Tab 2 modal)
        public Task<ApiResponseDto<List<AccountGroupDto>>> GetAllAccountGroupsAsync()
        {
            return _costBookClient.CostbookAccountGroup.GetAllAccountGroupsAsync();
        }

        // TRANSFORMENGINE: delegate → GET /api/v1/accountgroup/{csg7Group}
        public Task<ApiResponseDto<AccountGroupDto>> GetAccountGroupAsync(string csg7Group)
        {
            return _costBookClient.CostbookAccountGroup.GetAccountGroupAsync(csg7Group);
        }

        // TRANSFORMENGINE: delegate → POST /api/v1/accountgroup
        public Task<ApiResponseDto<AccountGroupDto>> AddAccountGroupAsync(AccountGroupDto dto)
        {
            return _costBookClient.CostbookAccountGroup.AddAccountGroupAsync(dto);
        }

        // TRANSFORMENGINE: delegate → PUT /api/v1/accountgroup/{csg7Group}
        public Task<ApiResponseDto<AccountGroupDto>> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto)
        {
            return _costBookClient.CostbookAccountGroup.UpdateAccountGroupAsync(csg7Group, dto);
        }

        // TRANSFORMENGINE: delegate → DELETE /api/v1/accountgroup/{csg7Group}
        public Task<ApiResponseDto<bool>> DeleteAccountGroupAsync(string csg7Group)
        {
            return _costBookClient.CostbookAccountGroup.DeleteAccountGroupAsync(csg7Group);
        }
    }
}

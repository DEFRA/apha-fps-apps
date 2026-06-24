/*
 * TRANSFORMENGINE MIGRATION — AccountCategoryMaintenanceService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New service implementation created for AccountCategory maintenance (Tab 2 of frmMaintainance)
 *   - Orchestrates IFpsAccountCategoryRepository calls and maps results via AutoMapper
 *   - GetAllForMaintenanceAsync — fetches all account categories for the maintenance grid
 *   - UpdateCsg7GroupAsync — targeted CSG7 group update; preserves the saveTblAccCat JS handler flow:
 *       1. Validates accShortName input (non-null/empty)
 *       2. Fetches the current record via GetByAccShortNameAsync — throws KeyNotFoundException if missing
 *       3. Delegates targeted update to repository.UpdateCsg7GroupAsync
 *       4. Re-fetches the updated record and maps to DTO for response
 *   - Source: fps[year].tblkpaccountcategory; only Csg7Group field is maintained via this service
 *
 * PRESERVED:
 *   - All async-only patterns per Application layer convention
 *   - No direct DbContext usage — repository-only orchestration
 *   - ArgumentException for invalid input; KeyNotFoundException for missing records
 *   - saveTblAccCat targeted-field-update pattern from costbookmaintainance.js preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm FpsYear is derived server-side from CurrentFinancialYear setting or route param
 *   - TRANSFORMENGINE TODO: Confirm AccountType and ConstituentAccountCodes should remain read-only in this flow
 */

using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Interfaces;
using AutoMapper;

namespace Apha.Costbook.Application.Services
{
    // TRANSFORMENGINE: Service implementation for IAccountCategoryMaintenanceService — Tab 2 (Account Categories) maintenance
    public class AccountCategoryMaintenanceService : IAccountCategoryMaintenanceService
    {
        private readonly IFpsAccountCategoryRepository _repository;
        private readonly IMapper _mapper;

        public AccountCategoryMaintenanceService(IFpsAccountCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GetAllForMaintenanceAsync — returns all account categories; maps List<FpsAccountCategory> → List<AccountCategoryMaintenanceDto>
        public async Task<List<AccountCategoryMaintenanceDto>> GetAllForMaintenanceAsync()
        {
            var entities = await _repository.GetAllForMaintenanceAsync();
            return _mapper.Map<List<AccountCategoryMaintenanceDto>>(entities);
        }

        // TRANSFORMENGINE: UpdateCsg7GroupAsync — targeted CSG7 group update (preserves saveTblAccCat JS flow from costbookmaintainance.js)
        public async Task<AccountCategoryMaintenanceDto> UpdateCsg7GroupAsync(string accShortName, string? csg7Group)
        {
            // TRANSFORMENGINE: Input guard — mirrors JS null-check before POST to API
            if (string.IsNullOrWhiteSpace(accShortName))
                throw new ArgumentException("AccShortName must not be null or empty.", nameof(accShortName));

            // TRANSFORMENGINE: Existence guard — throws 404-mapping exception if record not found (no silent insert)
            var existing = await _repository.GetByAccShortNameAsync(accShortName);
            if (existing is null)
                throw new KeyNotFoundException($"Account category with AccShortName '{accShortName}' was not found.");

            // TRANSFORMENGINE: Delegate targeted update to repository.UpdateCsg7GroupAsync — only csg7Group column is modified
            var updated = await _repository.UpdateCsg7GroupAsync(accShortName, csg7Group);
            if (!updated)
                throw new InvalidOperationException($"Failed to update CSG7 group for account category '{accShortName}'.");

            // TRANSFORMENGINE: Re-fetch the updated record to return current state
            var refreshed = await _repository.GetByAccShortNameAsync(accShortName);
            return _mapper.Map<AccountCategoryMaintenanceDto>(refreshed);
        }
    }
}

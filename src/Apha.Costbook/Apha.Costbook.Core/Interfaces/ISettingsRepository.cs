/*
 * TRANSFORMENGINE MIGRATION — ISettingsRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Added GetAllUserUpdatableAsync() — returns all Settings where Userupdateable=true
 *   - Added UpdateMultipleAsync(Dictionary<string, string>) — bulk update settings values by Id
 *   - Both methods support Inflation (Tab 1) and Profit Margins (Tab 4) of frmMaintainance
 *
 * PRESERVED:
 *   - Existing GetSettingValueByIdAsync(string id) method signature unchanged
 *   - Namespace and interface name unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm mabarchive.tbl_settings Userupdateable column maps to bool? Userupdateable on Settings entity
 */

using Apha.Costbook.DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.Costbook.Core.Interfaces
{
    public interface ISettingsRepository
    {
        // TRANSFORMENGINE: Preserved — existing single-value lookup
        Task<string?> GetSettingValueByIdAsync(string id);

        // TRANSFORMENGINE: Added — bulk fetch for Inflation/Profit Margins tabs; returns all rows where Userupdateable=true
        Task<List<Settings>> GetAllUserUpdatableAsync();

        // TRANSFORMENGINE: Added — bulk update for Inflation/Profit Margins form submit (formInflation, formProfitMargins handlers in JS)
        Task<bool> UpdateMultipleAsync(Dictionary<string, string> settingsById);
    }
}

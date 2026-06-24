/*
 * TRANSFORMENGINE MIGRATION — SettingsRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Added GetAllUserUpdatableAsync() — returns all Settings rows where Userupdateable = true
 *     Supports Inflation (Tab 1) and Profit Margins (Tab 4) of frmMaintainance maintenance screen
 *   - Added UpdateMultipleAsync(Dictionary<string, string>) — bulk update of settings values by Id
 *     Implements the formInflation / formProfitMargins submit paths from costbookmaintainance.js
 *     Each dictionary entry triggers an ExecuteUpdateAsync targeted by setting Id for set-based efficiency
 *
 * PRESERVED:
 *   - Existing GetSettingValueByIdAsync(string id) method — signature and logic unchanged
 *   - Constructor injection of CostbookDbContext
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm mabarchive.tbl_settings userupdateable column drives which settings are editable via UI
 *   - TRANSFORMENGINE TODO: verify caller validation ensures settingsById values are within max 255-char limit (HasMaxLength in SettingsMap)
 */

using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apha.Costbook.DataAccess.Repositories
{
    // TRANSFORMENGINE: Updated SettingsRepository — adds bulk fetch + bulk update for maintenance tabs
    public class SettingsRepository : ISettingsRepository
    {
        private readonly CostbookDbContext _context;

        public SettingsRepository(CostbookDbContext context)
        {
            _context = context;
        }

        // TRANSFORMENGINE: Preserved — single setting lookup by Id (used by service layer for individual setting reads)
        public async Task<string?> GetSettingValueByIdAsync(string id)
        {
            var result = await _context.DatabaseSettings
                .Where(s => s.Id == id)
                .Select(s => s.Setting)
                .FirstOrDefaultAsync();

            return result;
        }

        // TRANSFORMENGINE: Added — returns all settings where userupdateable = true; supports Inflation + Profit Margins tabs
        public async Task<List<Settings>> GetAllUserUpdatableAsync()
        {
            return await _context.DatabaseSettings
                .AsNoTracking()
                .Where(s => s.Userupdateable == true)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        // TRANSFORMENGINE: Added — bulk update; each key in dict maps to a Settings.Id; ExecuteUpdateAsync for set-based efficiency
        public async Task<bool> UpdateMultipleAsync(Dictionary<string, string> settingsById)
        {
            if (settingsById == null || settingsById.Count == 0)
                return false;

            foreach (var kvp in settingsById)
            {
                var id = kvp.Key;
                var value = kvp.Value;

                // TRANSFORMENGINE: ExecuteUpdateAsync targets only the setting row matching this Id — no tracked load needed
                await _context.DatabaseSettings
                    .Where(s => s.Id == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Setting, value));
            }

            return true;
        }
    }
}

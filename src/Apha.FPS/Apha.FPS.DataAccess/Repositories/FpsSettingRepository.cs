using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Apha.FPS.DataAccess.Repositories
{
    public class FpsSettingRepository : IFpsSettingRepository
    {
        private readonly FpsDbContext _dbContext;

        public FpsSettingRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<FpsSetting>> GetAllAsync()
        {
            return await _dbContext.TblSettings.ToListAsync();
        }

        public async Task<FpsSetting?> GetByKeyAsync(string key)
        {
            return await _dbContext.TblSettings.FirstOrDefaultAsync(s => s.Id == key);
        }

        public async Task<FpsSetting> AddAsync(FpsSetting setting)
        {
            _dbContext.TblSettings.Add(setting);
            await _dbContext.SaveChangesAsync();
            return setting;
        }

        public async Task<FpsSetting> UpdateAsync(FpsSetting setting)
        {
            _dbContext.TblSettings.Update(setting);
            await _dbContext.SaveChangesAsync();
            return setting;
        }

        public async Task<List<FpsYearEndSetting>> GetYearEndSettingsAsync()
        {
            var result = new List<FpsYearEndSetting>();
            var settingIds = new[]
               {
                    "hoursinday",
                    "cap_approval_received_for_reset"
                };

            var OpenfpsYears = await _dbContext.YearMasters
               .AsNoTracking()
               .Where(y => y.Active && y.YearStatus.ToLower() == "open")
               .OrderByDescending(y => y.FpsYear)
               .ToListAsync();

            var openYear = OpenfpsYears.FirstOrDefault()?.FpsYear;

            var PlannedfpsYears = await _dbContext.YearMasters
               .AsNoTracking()
               .Where(y => y.Active && y.YearStatus.ToLower() == "planned")
               .OrderByDescending(y => y.FpsYear)
               .ToListAsync();

            var plannedYear = PlannedfpsYears.FirstOrDefault()?.FpsYear;

            var settings = await _dbContext.TblSettings
                .AsNoTracking()
                .Where(s => (s.FpsYear == openYear || s.FpsYear == plannedYear) &&
                            settingIds.Contains(s.Id.ToLower()))
                .ToListAsync();

            //Remove duplicates based on Id and prioritize planned year over open year
            settings = settings
                .GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.FirstOrDefault(x => x.FpsYear == plannedYear)
                ?? g.First(x => x.FpsYear == openYear)).ToList();

      
            foreach (var id in settingIds)
            {
                var setting = settings.FirstOrDefault(s =>
                    s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

                if (setting != null)
                {
                    result.Add(new FpsYearEndSetting
                    {
                        Id = setting.Id,
                        FpsYear = plannedYear.HasValue ? plannedYear.Value : openYear + 1,
                        Notes = setting.Notes,
                        UpdatedBy = setting.UpdatedBy,
                        UpdatedAt = setting.UpdatedAt,
                        FpsYearType = setting.FpsYear == plannedYear ? "planned" : "open"
                    });
                }
                else
                {
                    result.Add(new FpsYearEndSetting
                    {
                        Id = id,
                        FpsYear = plannedYear.HasValue ? plannedYear.Value : openYear + 1,
                        Notes = null,
                        UpdatedBy = null,
                        UpdatedAt = DateTime.MinValue,
                        FpsYearType = plannedYear.HasValue ? "planned" : "open"
                    });
                }
            }

            return result;
        }
    }
}
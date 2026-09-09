using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Apha.FPS.DataAccess.Repositories
{
    public class FpsSettingRepository : BaseRepository, IFpsSettingRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public FpsSettingRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
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

       // [ExcludeFromCodeCoverage]
        public async Task<FpsSetting> SaveAsync(FpsSetting setting)
        {
            var existing = await _dbContext.TblSettings
                .FirstOrDefaultAsync(m =>
                    m.Id == setting.Id &&
                    m.FpsYear == setting.FpsYear);

            setting.UpdatedBy = _requestContext.UserEmailId;
            setting.UpdatedAt = DateTime.UtcNow;

            if (existing is null)
            {
                _dbContext.TblSettings.Add(setting);
            }
            else
            {
                existing.Setting = setting.Setting;
                existing.Notes = setting.Notes;
                existing.UpdatedBy = setting.UpdatedBy;
                existing.UpdatedAt = setting.UpdatedAt;
                _dbContext.TblSettings.Update(existing);
            }

            await _dbContext.SaveChangesAsync();
            return existing ?? setting;
        }

        public async Task<FpsSetting> SaveYearEndSettingAsync(FpsSetting setting)
        {
            int? plannedYear = await GetPlannedYear();

            if (plannedYear is null)
            {
                return await SaveStagingAsync(setting);
            }
            else 
            {
                return await SaveAsync(setting);
            }
        }

       // [ExcludeFromCodeCoverage]
        public async Task<List<YearEndFpsSetting>> GetYearEndSettingsAsync()
        {
            List<FpsSetting> settings;

            var settingIds = new[]
               {
                    "HoursInDay",
                    "CapApprovalReceivedForReset"
                };

            int openYear = await GetOpenYear();

            int? plannedYear = await GetPlannedYear();

            var settingIdsLower = settingIds.Select(id => id.ToLowerInvariant()).ToArray();
           
            if (plannedYear is null)
            {
                plannedYear = openYear + 1;
                var openSettings = await _dbContext.TblSettings
                .AsNoTracking()
                .Where(s => (s.FpsYear == openYear ) &&
                            settingIdsLower.Contains(s.Id.ToLower()))
                .ToListAsync();

                var stagingSettings = await _dbContext.TblStagingSettings
               .AsNoTracking()
               .Where(s => (s.FpsYear == plannedYear) &&
                           settingIdsLower.Contains(s.Id.ToLower()))
               .ToListAsync();

                var plannedSettings = stagingSettings.Select(s => new FpsSetting
                {
                    Id = s.Id,
                    FpsYear = s.FpsYear,
                    Notes = s.Notes,
                    Setting=s.Setting,
                    UpdatedAt=s.UpdatedAt,
                    UpdatedBy=s.UpdatedBy
                }).ToList();

                settings = openSettings.Concat(plannedSettings).ToList();
            }
            else
            {
                 settings = await _dbContext.TblSettings
                               .AsNoTracking()
                               .Where(s => ( s.FpsYear == plannedYear) &&
                                           settingIdsLower.Contains(s.Id.ToLower()))
                               .ToListAsync();
            }
               

            //Remove duplicates based on Id and prioritize planned year over open year
            settings = settings
                .GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.FirstOrDefault(x => x.FpsYear == plannedYear)
                ?? g.First(x => x.FpsYear == openYear)).ToList();

            var result = GetSettingList(settingIds, openYear, plannedYear, settings);

            return result;
        }

        private async Task<FpsSetting> SaveStagingAsync(FpsSetting setting)
        {
            var stagingSetting = new FpsSettingStaging
            {
                Id = setting.Id,
                FpsYear = setting.FpsYear,
                Setting = setting.Setting,
                Notes = setting.Notes,
                UpdatedBy = _requestContext.UserEmailId,
                UpdatedAt = DateTime.UtcNow
            };

            var existing = await _dbContext.TblStagingSettings
                .FirstOrDefaultAsync(m =>
                    m.Id == stagingSetting.Id &&
                    m.FpsYear == stagingSetting.FpsYear);

            stagingSetting.UpdatedBy = _requestContext.UserEmailId;
            stagingSetting.UpdatedAt = DateTime.UtcNow;

            if (existing is null)
            {
                _dbContext.TblStagingSettings.Add(stagingSetting);
            }
            else
            {
                existing.Setting = stagingSetting.Setting;
                existing.Notes = stagingSetting.Notes;
                existing.UpdatedBy = stagingSetting.UpdatedBy;
                existing.UpdatedAt = stagingSetting.UpdatedAt;
                _dbContext.TblStagingSettings.Update(existing);
            }

            await _dbContext.SaveChangesAsync();

            return setting;
        }
        
        private static List<YearEndFpsSetting> GetSettingList( string[] settingIds, int openYear, int? plannedYear, List<FpsSetting> settings)
        {
            var result = new List<YearEndFpsSetting>();

            foreach (var id in settingIds)
            {
                var setting = settings.FirstOrDefault(s =>
                    s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

                if (setting != null)
                {
                    result.Add(new YearEndFpsSetting
                    {
                        Id = setting.Id,
                        FpsYear = plannedYear.HasValue ? plannedYear.Value : openYear + 1,
                        Setting = setting.Setting,
                        Notes = setting.Notes,
                        UpdatedBy = setting.UpdatedBy,
                        UpdatedAt = setting.UpdatedAt,
                        ExistsForPlannedYear = setting.FpsYear == plannedYear ? "Yes" : "No"
                    });
                }
                else
                {
                    result.Add(new YearEndFpsSetting
                    {
                        Id = id,
                        FpsYear = plannedYear.HasValue ? plannedYear.Value : openYear + 1,
                        Setting = id.ToLower() == "capapprovalreceivedforreset"? "No": null,
                        Notes = null,
                        UpdatedBy = null,
                        UpdatedAt = DateTime.MinValue,
                        ExistsForPlannedYear = "No"
                    });
                }
            }

            return result;
        }

        private async Task<int> GetOpenYear()
        {
            var openFpsYears = await _dbContext.YearMasters
                .AsNoTracking()
                .Where(y => y.Active && y.YearStatus.ToLower() == "open")
                .OrderByDescending(y => y.FpsYear)
                .Select(y => y.FpsYear)
                .FirstAsync();

            return openFpsYears;
        }

        private async Task<int?> GetPlannedYear()
        {
            var plannedFpsYears = await _dbContext.YearMasters
                .AsNoTracking()
                .Where(y => y.Active && y.YearStatus.ToLower() == "planned")
                .OrderByDescending(y => y.FpsYear)
                .ToListAsync();

            var plannedYear = plannedFpsYears.FirstOrDefault()?.FpsYear;
            return plannedYear;
        }
    }
}
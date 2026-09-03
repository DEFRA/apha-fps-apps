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

        [ExcludeFromCodeCoverage]
        public async Task<FpsSetting> SaveAsync(FpsSetting setting)
        {
            var existing = await _dbContext.TblSettings.IgnoreQueryFilters()
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

        private static readonly string[] YearEndSettingIds =
        {
            "HoursInDay",
            "CapApprovalReceivedForReset"
        };

        /// <summary>
        /// Grid read path (planned-year staging design, CR067): current/Open-year real values
        /// overlaid with staged rows for <paramref name="request"/>'s JobQueueId. Deliberately does
        /// not use YearMasters.Status == "Planned" — pre-Approval there is no such row.
        /// </summary>
        public async Task<List<YearEndFpsSetting>> GetYearEndSettingsAsync(YearEndRequestSummary request)
        {
            if (!request.TargetFpsYear.HasValue)
                throw new InvalidOperationException(
                    $"Year End request {request.JobQueueId} has no target_fpsyear set.");

            var settingIdsLower = YearEndSettingIds.Select(id => id.ToLowerInvariant()).ToArray();

            var currentYearSettings = await _dbContext.TblSettings.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(s => s.FpsYear == request.FpsYear && settingIdsLower.Contains(s.Id.ToLower()))
                .ToListAsync();

            var stagedSettings = await _dbContext.YearEndSettingStagings
                .AsNoTracking()
                .Where(s => s.JobQueueId == request.JobQueueId)
                .ToListAsync();

            var result = new List<YearEndFpsSetting>();

            foreach (var id in YearEndSettingIds)
            {
                var staged = stagedSettings.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                var current = currentYearSettings.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

                result.Add(new YearEndFpsSetting
                {
                    Id = id,
                    FpsYear = request.TargetFpsYear.Value,
                    Setting = staged?.Setting ?? current?.Setting,
                    Notes = staged?.Notes ?? current?.Notes,
                    UpdatedBy = current?.UpdatedBy,
                    UpdatedAt = current?.UpdatedAt ?? DateTime.MinValue,
                    ExistsForPlannedYear = staged is not null ? "Yes" : "No"
                });
            }

            return result;
        }

        [ExcludeFromCodeCoverage]
        public async Task<List<YearEndFpsSetting>> GetYearEndSettingsAsync()
        {
            var settingIds = YearEndSettingIds;

            int openYear = await GetOpenYear();

            int? plannedYear = await GetPlannedYear();

            var settingIdsLower = settingIds.Select(id => id.ToLowerInvariant()).ToArray();

            var settings = await _dbContext.TblSettings.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(s => (s.FpsYear == openYear || s.FpsYear == plannedYear) &&
                            settingIdsLower.Contains(s.Id.ToLower()))
                .ToListAsync();

            //Remove duplicates based on Id and prioritize planned year over open year
            settings = settings
                .GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.FirstOrDefault(x => x.FpsYear == plannedYear)
                ?? g.First(x => x.FpsYear == openYear)).ToList();

            var result = GetSettingList(settingIds, openYear, plannedYear, settings);

            return result;
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
                        Setting = null,
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
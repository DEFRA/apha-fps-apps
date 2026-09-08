using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    // Persistence primitives only, per the Year End planned-year staging design (CR067) — no
    // Initiated/Approved status enforcement here, that's the service/application layer's job.
    // Every write below uses ordinary tracked-entity Add/Update/Remove + SaveChangesAsync, never a
    // relational bulk-operation extension (ExecuteUpdateAsync/ExecuteDeleteAsync/ExecuteSqlRawAsync) —
    // this repository's existing unit test harness mocks FpsDbContext with no real EF provider
    // configured, and those throw InvalidOperationException against it (same constraint YearEndRepository
    // and FpsSettingRepository already follow).
    public class YearEndStagingRepository : BaseRepository, IYearEndStagingRepository
    {
        // Matches YearEndService's own YearEndDataSetupJobName constant. Staging is a
        // Data-Setup-only concept (CutOver never stages anything), so this repository is
        // inherently scoped to that one job — hardcoded here rather than threaded as a parameter.
        private const string YearEndDataSetupJobName = "YearEnd-DataSetup";

        public YearEndStagingRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<YearEndRequestSummary?> ResolveRequestAsync(Guid jobExecutionId)
        {
            // IgnoreQueryFilters: BatchJobQueue carries a global HasQueryFilter(e => e.FpsYear ==
            // FilterFpsYear) — a lookup by JobExecutionId is a unique-identifier resolution, not a
            // year-scoped listing, and must not depend on the caller's ambient X-FPS-Year header
            // matching whatever FpsYear this row happens to carry.
            //
            // job_master join: job_queue is a shared table across YearEnd/Bulk Rates/etc. Without
            // this filter, a JobExecutionId belonging to some other job type would resolve here as
            // if it were a valid Year End Data Setup request — silently wrong, not just unlikely.
            var result = await (
                from jq in _context.BatchJobQueues.IgnoreQueryFilters().AsNoTracking()
                join jm in _context.BatchJobs.AsNoTracking() on jq.JobId equals jm.JobId
                join js in _context.BatchJobStatuses.AsNoTracking()
                    on new { jq.StatusId, jq.JobId } equals new { js.StatusId, js.JobId }
                where jq.JobExecutionId == jobExecutionId
                   && jm.JobName == YearEndDataSetupJobName
                select new YearEndRequestSummary(jq.JobqueueId, jq.FpsYear, jq.TargetFpsYear, js.Status)
            ).FirstOrDefaultAsync();

            return result;
        }

        public async Task<List<FpsSettingStaging>> GetStagedSettingsAsync()
        {
            return await _context.FpsSettingStagings
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<MonthHourStaging>> GetStagedMonthHoursAsync()
        {
            return await _context.MonthHourStagings
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpsertStagedSettingAsync(FpsSettingStaging setting)
        {
            var existing = await _context.FpsSettingStagings
                .FirstOrDefaultAsync(s => s.Id == setting.Id && s.FpsYear == setting.FpsYear);

            if (existing is null)
            {
                _context.FpsSettingStagings.Add(setting);
            }
            else
            {
                existing.Setting = setting.Setting;
                existing.Notes = setting.Notes;
                existing.UpdatedBy = setting.UpdatedBy;
                existing.UpdatedAt = setting.UpdatedAt;
                _context.FpsSettingStagings.Update(existing);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpsertStagedMonthHourAsync(MonthHourStaging monthHour)
        {
            var existing = await _context.MonthHourStagings
                .FirstOrDefaultAsync(m =>
                    m.Year == monthHour.Year &&
                    m.Month == monthHour.Month &&
                    m.FpsYear == monthHour.FpsYear);

            if (existing is null)
            {
                _context.MonthHourStagings.Add(monthHour);
            }
            else
            {
                existing.Fmonth = monthHour.Fmonth;
                existing.Days = monthHour.Days;
                existing.CvlHours = monthHour.CvlHours;
                existing.VidHours = monthHour.VidHours;
                _context.MonthHourStagings.Update(existing);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteStagingAsync()
        {
            var settings = await _context.FpsSettingStagings.ToListAsync();
            var monthHours = await _context.MonthHourStagings.ToListAsync();

            if (settings.Count == 0 && monthHours.Count == 0)
                return;

            _context.FpsSettingStagings.RemoveRange(settings);
            _context.MonthHourStagings.RemoveRange(monthHours);
            await _context.SaveChangesAsync();
        }
    }
}

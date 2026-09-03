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

        public async Task<List<YearEndSettingStaging>> GetStagedSettingsAsync(Guid jobQueueId)
        {
            return await _context.YearEndSettingStagings
                .AsNoTracking()
                .Where(s => s.JobQueueId == jobQueueId)
                .ToListAsync();
        }

        public async Task<List<YearEndMonthHourStaging>> GetStagedMonthHoursAsync(Guid jobQueueId)
        {
            return await _context.YearEndMonthHourStagings
                .AsNoTracking()
                .Where(m => m.JobQueueId == jobQueueId)
                .ToListAsync();
        }

        public async Task UpsertStagedSettingAsync(YearEndSettingStaging setting)
        {
            var existing = await _context.YearEndSettingStagings
                .FirstOrDefaultAsync(s => s.JobQueueId == setting.JobQueueId && s.Id == setting.Id);

            if (existing is null)
            {
                _context.YearEndSettingStagings.Add(setting);
            }
            else
            {
                existing.Setting = setting.Setting;
                existing.Notes = setting.Notes;
                _context.YearEndSettingStagings.Update(existing);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpsertStagedMonthHourAsync(YearEndMonthHourStaging monthHour)
        {
            var existing = await _context.YearEndMonthHourStagings
                .FirstOrDefaultAsync(m =>
                    m.JobQueueId == monthHour.JobQueueId &&
                    m.Month == monthHour.Month &&
                    m.Fmonth == monthHour.Fmonth);

            if (existing is null)
            {
                _context.YearEndMonthHourStagings.Add(monthHour);
            }
            else
            {
                existing.MonthYear = monthHour.MonthYear;
                existing.Days = monthHour.Days;
                existing.CvlHours = monthHour.CvlHours;
                existing.VidHours = monthHour.VidHours;
                _context.YearEndMonthHourStagings.Update(existing);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteStagingAsync(Guid jobQueueId)
        {
            var settings = await _context.YearEndSettingStagings
                .Where(s => s.JobQueueId == jobQueueId)
                .ToListAsync();
            var monthHours = await _context.YearEndMonthHourStagings
                .Where(m => m.JobQueueId == jobQueueId)
                .ToListAsync();

            if (settings.Count == 0 && monthHours.Count == 0)
                return;

            _context.YearEndSettingStagings.RemoveRange(settings);
            _context.YearEndMonthHourStagings.RemoveRange(monthHours);
            await _context.SaveChangesAsync();
        }
    }
}

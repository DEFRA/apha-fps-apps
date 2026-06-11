using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class RecreateAndReleaseSummaryRepository : BaseRepository, IRecreateAndReleaseSummaryRepository
    {
        public RecreateAndReleaseSummaryRepository(FpsDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Retrieves recreate summaries logs with pagination and sorting.
        /// Implements the SQL query logic: LEFT JOIN tblUsers ON CONCAT('CVLNT', UserID) = UserName
        /// Returns enriched log data with user information.
        /// </summary>
        public async Task<PagedData<RecreateSummaryLogWithComment>> GetRecreateSummaryLogAsync(PaginationParameters<string> parameters)
        {
            // Build the query with LEFT JOIN - EF Core will translate string concatenation to SQL CONCAT
            var baseQuery = from log in _context.RecreateSummaryLogs.AsNoTracking()
                            join user in _context.Users.AsNoTracking()
                            on ("CVLNT" + log.UserId) equals user.UserName into userGroup
                            from user in userGroup.DefaultIfEmpty()
                            select new
                            {
                                Log = log,
                                UserName = user != null ? user.UserName : null,
                                UserComments = user != null ? user.Comments : null
                            };

            // Apply sorting at database level
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                var sortBy = parameters.SortBy.ToLower();
                baseQuery = (sortBy, parameters.Descending) switch
                {
                    ("id", true) => baseQuery.OrderByDescending(e => e.Log.Id),
                    ("id", false) => baseQuery.OrderBy(e => e.Log.Id),
                    ("datedone", true) => baseQuery.OrderByDescending(e => e.Log.DateDone),
                    ("datedone", false) => baseQuery.OrderBy(e => e.Log.DateDone),
                    ("userid", true) => baseQuery.OrderByDescending(e => e.Log.UserId),
                    ("userid", false) => baseQuery.OrderBy(e => e.Log.UserId),
                    ("user", true) => baseQuery.OrderByDescending(e => e.UserComments),
                    ("user", false) => baseQuery.OrderBy(e => e.UserComments),
                    ("period", true) => baseQuery.OrderByDescending(e => e.Log.Period),
                    ("period", false) => baseQuery.OrderBy(e => e.Log.Period),
                    _ => baseQuery.OrderByDescending(e => e.Log.DateDone)
                };
            }
            else
            {
                baseQuery = baseQuery.OrderByDescending(e => e.Log.DateDone);
            }

            // Map to the result model
            var mappedData = baseQuery.Select(r => new RecreateSummaryLogWithComment
            {
                Id = r.Log.Id,
                UserId = r.Log.UserId,
                Comments = r.UserComments ?? string.Empty,
                Period = r.Log.Period,
                DateDone = r.Log.DateDone,
            });

            return await base.ApplyPaging(mappedData, parameters.Page, parameters.PageSize);
        }


        public async Task<ReleaseSummary> GetReleaseSummariesAsync()
        {
            var releaseSummary = new ReleaseSummary()
            {
                ReleasePeriods = await GetReleasePeriodsAsync(),
                Setting = await GetSettingByIdAsync("SendEmail")
            };

            return releaseSummary;
        }

      
        private async Task<string?> GetSettingByIdAsync(string settingId)
        {
            var setting = await _context.Settings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == settingId);

            return setting?.Setting;
        }

        public async Task<IList<ReleasePeriod>> GetReleasePeriodsAsync()
        {
            return await _context.ReleasePeriods
                            .AsNoTracking()
                            .OrderBy(p => p.EndPeriod)
                            .ToListAsync();
        }
        public async Task<ReleasePeriod?> SetFinalSummaryRunAsync(string? periodName, short? finalSummariesRun, string? sendEmail)
        {
            if (!string.IsNullOrWhiteSpace(sendEmail))
            {
                await UpdateSettingsAsync(sendEmail);
                return new ReleasePeriod();
            }
            return await UpdateFinalSummaryRunAsync(periodName, finalSummariesRun ?? 0);
        }

        private async Task UpdateSettingsAsync(string sendEmail)
        {
            var settingValue = (sendEmail == "1" || sendEmail == "-1")
                ? "-1"
                : "0";

            var setting = await _context.Settings.FindAsync("SendEmail");
            setting?.Setting = settingValue;
            await _context.SaveChangesAsync();
        }

        private async Task<ReleasePeriod?> UpdateFinalSummaryRunAsync(string? periodName, short finalSummariesRun)
        {
            var fpsYear = _context.FilterFpsYear;
            var releasePeriod = await _context.ReleasePeriods.FindAsync(periodName, fpsYear);

            var finalSummariesRunValue = (finalSummariesRun == 1 || finalSummariesRun == -1)
                ? (short)-1
                : (short)0;

            if (releasePeriod is not null)
            {
                releasePeriod.FinalSummariesRun = finalSummariesRunValue;

                await _context.SaveChangesAsync();
            }

            return releasePeriod;
        }
    }
}
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.PACT.DataAccess.Repository
{
    public class RecreateAndReleaseSummaryRepository : BaseRepository, IRecreateAndReleaseSummaryRepository
    {
        public RecreateAndReleaseSummaryRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<PagedData<RecreateSummariesLog>> GetRecreateSummariesAllLogsAsync(PaginationParameters<string> parameters)
        {
            IQueryable<RecreateSummariesLog> query = _context.RecreateSummariesLogs
                .Include(r => r.User)
                .AsNoTracking();

            // Apply sorting
            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            // Get total count before pagination
            var totalRecords = await query.CountAsync();

            // Apply pagination at database level
            var data = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            // Build pagination metadata
            var paginationData = new PaginationData
            {
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / parameters.PageSize),
                TotalRecords = totalRecords
            };

            return new PagedData<RecreateSummariesLog>(data.AsReadOnly(), paginationData);
        }

        public async Task<ReleaseSummary> GetReleaseSummariesAsync()
        {
            var releaseSummary = new ReleaseSummary()
            {
                ReleasePeriods = await GetReleasePeriodAsync(),
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

        private async Task<IList<ReleasePeriod>> GetReleasePeriodAsync()
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

        private static IQueryable<RecreateSummariesLog> ApplySorting(IQueryable<RecreateSummariesLog> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderByDescending(e => e.DateDone);

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable<RecreateSummariesLog> ApplySortingByProperty(IQueryable<RecreateSummariesLog> query, string property, bool descending)
        {
            return property switch
            {
                "id" => ApplyOrder(query, r => r.Id, descending),
                "datedone" => ApplyOrder(query, r => r.DateDone, descending),
                "userid" => ApplyOrder(query, r => r.UserId, descending),
                "user" => ApplyOrder(query, r => r.User!.UserName, descending),
                "period" => ApplyOrder(query, r => r.Period, descending),
                _ => query.OrderByDescending(e => e.DateDone)
            };
        }

        private static IQueryable<RecreateSummariesLog> ApplyOrder<T>(IQueryable<RecreateSummariesLog> query, Expression<Func<RecreateSummariesLog, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}

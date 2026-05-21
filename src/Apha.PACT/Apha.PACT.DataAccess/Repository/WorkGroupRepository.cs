using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PACT.DataAccess.Repository
{
    public class WorkGroupRepository : BaseRepository, IWorkGroupRepository
    {
        public WorkGroupRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<WorkGroup>> GetAllWorkGroupsAsync()
        {
            return await _context.WorkGroups
                .AsNoTracking()
                .OrderBy(w => w.WorkGroupName)
                .ToListAsync();
        }

        public async Task<PagedData<WorkGroup>> GetWorkGroupsByProfitCentreAsync(
            PaginationParameters<string> query, string profitCentre)
        {
            var baseQuery = _context.WorkGroups
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre && w.FpsYear == _context.FilterFpsYear);

            baseQuery = ApplyWorkGroupFilter(baseQuery, query.Filter);

            // SendEmailYes / SendEmailNo are view-model-only computed properties that have no
            // corresponding column on the WorkGroup entity; fall back to WorkGroupName for those.
            var sortBy = query.SortBy is nameof(WorkGroup.WorkGroupName) or nameof(WorkGroup.EmailRecipient)
                ? query.SortBy
                : nameof(WorkGroup.WorkGroupName);

            baseQuery = query.Descending
                ? baseQuery.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : baseQuery.OrderBy(e => EF.Property<object>(e, sortBy));

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<bool> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            var fpsYear = _context.FilterFpsYear;
            await _context.WorkGroups
                .Where(wg => wg.FpsYear == fpsYear
                          && _context.ProfitCentres
                                .Any(pc => pc.ProfitCentreId == profitCentre
                                        && pc.ProfitCentreId == wg.ProfitCentre))
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.SendEmail, flag));
            return true;
        }

        public async Task<bool> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            var fpsYear = _context.FilterFpsYear;
            await _context.WorkGroups
                .Where(w => w.FpsYear == fpsYear)
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.SendEmail, flag));
            return true;
        }

        public async Task<bool> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient)
        {
            var fpsYear = _context.FilterFpsYear;
            await _context.WorkGroups
                .Where(w => w.WorkGroupName == workGroupName && w.FpsYear == fpsYear)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(w => w.SendEmail, sendEmail)
                    .SetProperty(w => w.EmailRecipient, emailRecipient));
            return true;
        }

        private static IQueryable<WorkGroup> ApplyWorkGroupFilter(
            IQueryable<WorkGroup> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("WorkGroupName", out var workGroupName) && !string.IsNullOrWhiteSpace(workGroupName))
                query = query.Where(w => EF.Functions.ILike(w.WorkGroupName, $"%{workGroupName}%"));

            if (filters.TryGetValue("EmailRecipient", out var emailRecipient) && !string.IsNullOrWhiteSpace(emailRecipient))
                query = query.Where(w => w.EmailRecipient != null &&
                                         EF.Functions.ILike(w.EmailRecipient, $"%{emailRecipient}%"));

            return query;
        }
    }
}

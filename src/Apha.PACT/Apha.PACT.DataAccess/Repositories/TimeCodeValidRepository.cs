using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repositories
{
    public class TimeCodeValidRepository : BaseRepository, ITimeCodeValidRepository
    {
        private readonly IFpsYearContext _fpsYearContext;

        public TimeCodeValidRepository(FpsDbContext context, IFpsYearContext fpsYearContext) : base(context)
        {
            _fpsYearContext = fpsYearContext;
        }

        public async Task<IEnumerable<TimeCodeValid>> GetByJobCodeAsync(string jobCode, string parentProject)
        {
            return await _context.TimeCodeValids
                .AsNoTracking()
                .Where(t => t.JobCode == jobCode && t.ParentProject == parentProject)
                .OrderBy(t => t.WorkGroup)
                .ToListAsync();
        }

        public async Task<PagedData<TimeCodeValid>> GetPagedTimeCodesAsync(
            PaginationParameters<string> query, string? jobCode, string? parentProject)
        {
            var queryable = _context.TimeCodeValids.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(jobCode))
                queryable = queryable.Where(t => t.JobCode == jobCode);

            if (!string.IsNullOrWhiteSpace(parentProject))
                queryable = queryable.Where(t => t.ParentProject == parentProject);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                queryable = queryable.Where(t =>
                    t.WorkGroup.ToLower().Contains(search) ||
                    (t.JobCode != null && t.JobCode.ToLower().Contains(search)));
            }

            queryable = query.SortBy?.ToLower() switch
            {
                "workgroup" => query.Descending
                    ? queryable.OrderByDescending(t => t.WorkGroup)
                    : queryable.OrderBy(t => t.WorkGroup),
                _ => queryable.OrderBy(t => t.WorkGroup)
            };

            var result = await queryable.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<TimeCodeValid?> GetTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
        {
            return await _context.TimeCodeValids
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.WorkGroup == workGroup &&
                    t.TimeCode == timeCode &&
                    t.ParentProject == parentProject);
        }

        public async Task<TimeCodeValid> CreateTimeCodeValidAsync(TimeCodeValid timeCodeValid)
        {
            timeCodeValid.FpsYear = _fpsYearContext.FPSYear;
            await _context.TimeCodeValids.AddAsync(timeCodeValid);
            await _context.SaveChangesAsync();
            return timeCodeValid;
        }

        public async Task<TimeCodeValid> UpdateTimeCodeValidAsync(TimeCodeValid timeCodeValid)
        {
            timeCodeValid.FpsYear = _fpsYearContext.FPSYear;
            _context.Entry(timeCodeValid).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return timeCodeValid;
        }

        public async Task<bool> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
        {
            var entity = await _context.TimeCodeValids
                .FirstOrDefaultAsync(t =>
                    t.WorkGroup == workGroup &&
                    t.TimeCode == timeCode &&
                    t.ParentProject == parentProject &&
                    t.FpsYear == _fpsYearContext.FPSYear);
            if (entity == null) return false;
            _context.TimeCodeValids.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllByJobCodeAsync(string jobCode, string parentProject)
        {
            var entities = await _context.TimeCodeValids
                .Where(t => t.JobCode == jobCode &&
                            t.ParentProject == parentProject &&
                            t.FpsYear == _fpsYearContext.FPSYear)
                .ToListAsync();
            if (entities.Any())
            {
                _context.TimeCodeValids.RemoveRange(entities);
                await _context.SaveChangesAsync();
            }            
            return true;
        }

        public async Task<IEnumerable<TimeCodeValid>> CopyWorkGroupAsync(
            string sourceJobCode, string targetJobCode, string parentProject)
        {
            var sourceEntries = await _context.TimeCodeValids
                .AsNoTracking()
                .Where(t => t.JobCode == sourceJobCode && t.ParentProject == parentProject)
                .ToListAsync();

            var copies = sourceEntries.Select(s => new TimeCodeValid
            {
                TimeCode = targetJobCode,
                WorkGroup = s.WorkGroup,
                ParentProject = parentProject,
                JobCode = targetJobCode,
                Active = s.Active,
                FpsYear = _fpsYearContext.FPSYear
            }).ToList();

            await _context.TimeCodeValids.AddRangeAsync(copies);
            await _context.SaveChangesAsync();
            return copies;
        }
    }
}

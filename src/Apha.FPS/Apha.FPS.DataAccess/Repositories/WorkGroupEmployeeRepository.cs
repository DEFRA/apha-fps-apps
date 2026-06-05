using System.Dynamic;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class WorkGroupEmployeeRepository : BaseRepository, IWorkGroupEmployeeRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public WorkGroupEmployeeRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }
       
        public async Task<WorkGroupEmployeeView?> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            return await _dbContext.WorkGroupEmployees
                .AsNoTracking()
                .Where(wg => wg.PactId == pactId)
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WorkGroupEmployeeView
                    {
                        PactId         = wg.PactId,
                        SpNumber       = wg.SpNumber,
                        WorkGroupGrade = wg.WorkGroupGrade,
                        Name           = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        PersonStatus   = wg.PersonStatus,
                        PersonClass    = wg.PersonClass,
                        HrsPaid        = wg.HrsPaid,
                        Leave          = wg.Leave,
                        SickSpecial    = wg.SickSpecial,
                        HrsAvail       = wg.HrsAvail,
                        MakeAvailable  = wg.MakeAvailable,
                    })
                .FirstOrDefaultAsync(default);
        }

        public async Task<WorkGroupEmployee> UpdateWorkGroupEmployeeAsync(WorkGroupEmployee entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var existing = await _dbContext.WorkGroupEmployees
                .FirstOrDefaultAsync(x => x.PactId == entity.PactId);
            if (existing == null)
                throw new KeyNotFoundException($"WorkGroupEmployee with PACTid '{entity.PactId}' was not found.");

            existing.HrsPaid       = entity.HrsPaid;
            existing.Leave         = entity.Leave;
            existing.SickSpecial   = entity.SickSpecial;
            existing.HrsAvail      = entity.HrsPaid - (entity.Leave + entity.SickSpecial);
            existing.PersonStatus  = entity.PersonStatus;
            existing.PersonClass   = entity.PersonClass;
            existing.MakeAvailable = entity.MakeAvailable;

            await _dbContext.SaveChangesAsync(default);
            return existing;
        }
      
        public async Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeAsync(
            PaginationParameters<string> query,
            string wgGrade)
        {
            var all = await _dbContext.WorkGroupEmployeeViews
                .AsNoTracking()
                .Where(x => x.WorkGroupGrade == wgGrade
                         && x.PersonStatus != "I"
                         && x.UserEmail != null && x.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WorkGroupEmployeeView
                    {
                        PactId         = wg.PactId,
                        SpNumber       = wg.SpNumber,
                        WorkGroupGrade = wg.WorkGroupGrade,
                        Name           = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        PersonStatus   = wg.PersonStatus,
                        PersonClass    = wg.PersonClass,
                        HrsPaid        = wg.HrsPaid,
                        Leave          = wg.Leave,
                        SickSpecial    = wg.SickSpecial,
                        HrsAvail       = wg.HrsAvail,
                        MakeAvailable  = wg.MakeAvailable,
                        TimeRecorder   = wg.TimeRecorder,
                        StartDate      = wg.StartDate,
                        EndDate        = wg.EndDate,
                        HoursPerWeek   = wg.HoursPerWeek,
                        FpsYear        = wg.FpsYear,
                        UserId         = wg.UserId,
                        Dt2Username    = wg.Dt2Username,
                        UserEmail      = wg.UserEmail,
                    })
                .ToListAsync();

            var filtered = ApplyFilter(all.AsQueryable(), query.Filter);
            var sorted   = ApplySorting(filtered, query.SortBy, query.Descending);

            return ApplyPaging(sorted, query.Page, query.PageSize);
        }

        public async Task<bool> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            var entity = await _dbContext.WorkGroupEmployees
                .FirstOrDefaultAsync(x => x.PactId == pactId);
            if (entity == null)
                return false;

            _dbContext.WorkGroupEmployees.Remove(entity);
            await _dbContext.SaveChangesAsync(default);
            return true;
        }

        public async Task<bool> HasAssociatedStaffAsync(string wgGrade)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return false;

            return await _dbContext.WorkGroupEmployees
                .AnyAsync(e => e.WorkGroupGrade == wgGrade);
        }

        private static IQueryable<WorkGroupEmployeeView> ApplyFilter(IQueryable<WorkGroupEmployeeView> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("SpNumber", out var spNumber) && spNumber != null)
                query = query.Where(x => x.SpNumber != null && x.SpNumber.Contains(spNumber.ToString()!));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => x.Name != null && x.Name.Contains(name.ToString()!));

            return query;
        }

        private static IQueryable<WorkGroupEmployeeView> ApplySorting(IQueryable<WorkGroupEmployeeView> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "spnumber" => descending ? query.OrderByDescending(x => x.SpNumber) : query.OrderBy(x => x.SpNumber),
                "name"     => descending ? query.OrderByDescending(x => x.Name)     : query.OrderBy(x => x.Name),
                _          => query.OrderBy(x => x.Name)
            };
        }
    }
}

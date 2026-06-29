using System.Dynamic;
using System.Linq.Expressions;
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
                        PactId = wg.PactId,
                        SpNumber = wg.SpNumber,
                        WorkGroupGrade = wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        PersonStatus = wg.PersonStatus,
                        PersonClass = wg.PersonClass,
                        HrsPaid = wg.HrsPaid,
                        Leave = wg.Leave,
                        SickSpecial = wg.SickSpecial,
                        HrsAvail = wg.HrsAvail,
                        MakeAvailable = wg.MakeAvailable,
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

            existing.HrsPaid = entity.HrsPaid;
            existing.Leave = entity.Leave;
            existing.SickSpecial = entity.SickSpecial;
            existing.HrsAvail = entity.HrsPaid - (entity.Leave + entity.SickSpecial);
            existing.PersonStatus = entity.PersonStatus;
            existing.PersonClass = entity.PersonClass;
            existing.MakeAvailable = entity.MakeAvailable;

            await _dbContext.SaveChangesAsync(default);
            return existing;
        }

        public async Task<WorkGroupEmployee> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployee entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var existing = await _dbContext.WorkGroupEmployees
                .FirstOrDefaultAsync(x => x.PactId == entity.PactId);
            if (existing == null)
                throw new KeyNotFoundException($"WorkGroupEmployee with PACTid '{entity.PactId}' was not found.");

            existing.HrsPaid = entity.HrsPaid;
            existing.Leave = entity.Leave;
            existing.SickSpecial = entity.SickSpecial;
            existing.HrsAvail = entity.HrsAvail;
            existing.PersonStatus = entity.PersonStatus;
            existing.PersonClass = entity.PersonClass;
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
                         && x.UserEmail != null && x.UserEmail.ToLower() == _requestContext.UserEmailId.ToLower())
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

        public async Task<WorkGroupEmployee> CreateWorkGroupEmployeeAsync(WorkGroupEmployee entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            entity.FpsYear = _requestContext.FpsYear;
            await _dbContext.WorkGroupEmployees.AddAsync(entity);
            await _dbContext.SaveChangesAsync(default);
            return entity;
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
                "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                _ => query.OrderBy(x => x.Name)
            };
        }

        public async Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeForStaffAsync(
            PaginationParameters<string> query,
            string wgGrade)
        {
            var workGroupEmployeeQuery = _dbContext.WorkGroupEmployeeViews
                .AsNoTracking()
                .Where(wg => (string.IsNullOrWhiteSpace(wgGrade) || wg.WorkGroupGrade == wgGrade)
                          && wg.UserEmail != null
                          && wg.UserEmail.ToLower() == _requestContext.UserEmailId.ToLower())
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WorkGroupEmployeeView
                    {
                        PactId = wg.PactId,
                        SpNumber = wg.SpNumber,
                        WorkGroupGrade = wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        PersonStatus = wg.PersonStatus,
                        PersonClass = wg.PersonClass,
                        HrsPaid = wg.HrsPaid,
                        Leave = wg.Leave,
                        SickSpecial = wg.SickSpecial,
                        HrsAvail = wg.HrsAvail,
                        MakeAvailable = wg.MakeAvailable,
                        TimeRecorder = wg.TimeRecorder,
                        StartDate = wg.StartDate,
                        EndDate = wg.EndDate,
                        HoursPerWeek = wg.HoursPerWeek,
                        FpsYear = wg.FpsYear,
                        UserId = wg.UserId,
                        Dt2Username = wg.Dt2Username,
                        UserEmail = wg.UserEmail
                    })
                .Distinct()
                .AsQueryable();

            workGroupEmployeeQuery = ApplyWorkGroupEmployeeFilter(workGroupEmployeeQuery, query.Filter);
            workGroupEmployeeQuery = (IQueryable<WorkGroupEmployeeView>)ApplySorting_Enhanced(workGroupEmployeeQuery, query.SortBy, query.Descending);

            var result = await workGroupEmployeeQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        private static IQueryable<WorkGroupEmployeeView> ApplyWorkGroupEmployeeFilter(IQueryable<WorkGroupEmployeeView> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("PactId", out var pactId) && pactId != null)
                query = query.Where(x => EF.Functions.ILike(x.PactId, $"%{pactId}%"));

            if (dict.TryGetValue("SpNumber", out var spNumber) && spNumber != null)
                query = query.Where(x => x.SpNumber != null && EF.Functions.ILike(x.SpNumber, $"%{spNumber}%"));

            if (TryGetFilterValue(dict, "Name", "StaffName", out var name) && name != null)
                query = query.Where(x => x.Name != null && EF.Functions.ILike(x.Name, $"%{name}%"));

            if (TryGetFilterValue(dict, "WorkGroupGrade", "WgGrade", out var workGroupGrade) && workGroupGrade != null)
                query = query.Where(x => x.WorkGroupGrade != null && EF.Functions.ILike(x.WorkGroupGrade, $"%{workGroupGrade}%"));

            return query;
        }

        private static bool TryGetFilterValue(IDictionary<string, object> dict, string primaryKey, string alternateKey, out object? value)
        {
            if (dict.TryGetValue(primaryKey, out value))
                return true;

            if (dict.TryGetValue(alternateKey, out value))
                return true;

            value = null;
            return false;
        }

        private static IQueryable ApplySorting_Enhanced(IQueryable<WorkGroupEmployeeView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<WorkGroupEmployeeView> query, string property, bool descending)
        {
            return property switch
            {
                "pactid" => ApplyOrder(query, i => i.PactId, descending),
                "spnumber" => ApplyOrder(query, i => i.SpNumber, descending),
                "name" or "staffname" => ApplyOrder(query, i => i.Name, descending),
                "workgroupgrade" or "wggrade" => ApplyOrder(query, i => i.WorkGroupGrade, descending),
                "personstatus" => ApplyOrder(query, i => i.PersonStatus, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<WorkGroupEmployeeView> query, Expression<Func<WorkGroupEmployeeView, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}

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

        public WorkGroupEmployeeRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns a paginated list of staff for the given WG grade, excluding inactive employees.
        /// </summary>
        public async Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeAsync(
            PaginationParameters<string> query,
            string wgGrade)
        {
            var raw = await _dbContext.WgEmployees
                .AsNoTracking()
                .Where(wg => wg.WorkGroupGrade == wgGrade && wg.PersonStatus != "I")
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
                .ToListAsync(default);

            var filtered = ApplyFilter(raw.AsQueryable(), query.Filter);
            var sorted   = ApplySorting(filtered, query.SortBy, query.Descending);

            return ApplyPaging(sorted, query.Page, query.PageSize);
        }

        /// <summary>
        /// Returns a single WG employee by PACTid, joined with Employee to include Name.
        /// </summary>
        public async Task<WorkGroupEmployeeView?> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            return await _dbContext.WgEmployees
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

        /// <summary>
        /// Updates WorkGroupEmployee; computes HrsAvail = HrsPaid - (Leave + SickSpecial).
        /// </summary>
        public async Task<WorkGroupEmployee> UpdateWorkGroupEmployeeAsync(WorkGroupEmployee entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var existing = await _dbContext.WgEmployees
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

        /// <summary>
        /// Deletes a WG employee by PACTid.
        /// </summary>
        public async Task DeleteWorkGroupEmployeeAsync(string pactId)
        {
            var entity = await _dbContext.WgEmployees
                .FirstOrDefaultAsync(x => x.PactId == pactId);
            if (entity == null)
                throw new KeyNotFoundException($"WorkGroupEmployee with PACTid '{pactId}' was not found.");

            _dbContext.WgEmployees.Remove(entity);
            await _dbContext.SaveChangesAsync(default);
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
                query = query.Where(x => x.SpNumber.Contains(spNumber.ToString()!));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => x.Name.Contains(name.ToString()!));

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

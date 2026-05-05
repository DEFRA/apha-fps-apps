using System.Dynamic;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class WgStaffRepository : BaseRepository, IWgStaffRepository
    {
        private readonly FpsDbContext _dbContext;

        public WgStaffRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns a paginated list of staff for the given WG grade, excluding inactive employees.
        /// </summary>
        public async Task<PagedData<WgEmployeeView>> GetWgStaffAsync(
            PaginationParameters<string> query,
            string wgGrade,
            CancellationToken cancellationToken = default)
        {
            var raw = await _dbContext.WgEmployees
                .AsNoTracking()
                .Where(wg => wg.WorkGroupGrade == wgGrade && wg.PersonStatus != "I")
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WgEmployeeView
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
                .ToListAsync(cancellationToken);

            var filtered = ApplyFilter(raw.AsQueryable(), query.Filter);
            var sorted   = ApplySorting(filtered, query.SortBy, query.Descending);

            return ApplyPaging(sorted, query.Page, query.PageSize);
        }

        /// <summary>
        /// Returns a single WG employee by PACTid, joined with Employee to include Name.
        /// </summary>
        public async Task<WgEmployeeView?> GetWgEmployeeByIdAsync(string pactId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WgEmployees
                .AsNoTracking()
                .Where(wg => wg.PactId == pactId)
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WgEmployeeView
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
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Updates WgEmployee; computes HrsAvail = HrsPaid - (Leave + SickSpecial).
        /// </summary>
        public async Task<WgEmployee> UpdateWgEmployeeAsync(WgEmployee entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var existing = await _dbContext.WgEmployees
                .FirstOrDefaultAsync(x => x.PactId == entity.PactId, cancellationToken);
            if (existing == null)
                throw new KeyNotFoundException($"WgEmployee with PACTid '{entity.PactId}' was not found.");

            existing.HrsPaid       = entity.HrsPaid;
            existing.Leave         = entity.Leave;
            existing.SickSpecial   = entity.SickSpecial;
            existing.HrsAvail      = entity.HrsPaid - (entity.Leave + entity.SickSpecial);
            existing.PersonStatus  = entity.PersonStatus;
            existing.PersonClass   = entity.PersonClass;
            existing.MakeAvailable = entity.MakeAvailable;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return existing;
        }

        /// <summary>
        /// Deletes a WG employee by PACTid.
        /// </summary>
        public async Task DeleteWgEmployeeAsync(string pactId, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.WgEmployees
                .FirstOrDefaultAsync(x => x.PactId == pactId, cancellationToken);
            if (entity == null)
                throw new KeyNotFoundException($"WgEmployee with PACTid '{pactId}' was not found.");

            _dbContext.WgEmployees.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static IQueryable<WgEmployeeView> ApplyFilter(IQueryable<WgEmployeeView> query, string? filter)
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

        private static IQueryable<WgEmployeeView> ApplySorting(IQueryable<WgEmployeeView> query, string? sortBy, bool descending)
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

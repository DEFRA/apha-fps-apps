using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class AccessUserLevelRepository : BaseRepository, IAccessUserLevelRepository
    {
        private readonly PimsDbContext _dbContext;

        public AccessUserLevelRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PagedData<AccessUserLevel>> GetPagedAccessUserLevelAllAsync(PaginationParameters<string> query)
        {
            var baseQuery = _dbContext.AccessUserLevels.AsNoTracking();
            baseQuery = ApplyAccessUserLevelFilters(baseQuery, query.Filter);
            baseQuery = ApplyAccessUserLevelSorting(baseQuery, query.SortBy, query.Descending);

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }

        private IQueryable<AccessUserLevel> ApplyAccessUserLevelFilters(IQueryable<AccessUserLevel> baseQuery, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return baseQuery;
            }

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filter)
                ?? new Dictionary<string, string>();

            if (filters.TryGetValue("UserName", out var userNameFilter)
                && !string.IsNullOrWhiteSpace(userNameFilter))
            {
                var value = userNameFilter.Trim();
                // Resolve matching NtLogins via subquery against AccessUsers
                var matchingLogins = _dbContext.AccessUsers
                    .Where(u => u.UserName != null && EF.Functions.ILike(u.UserName, $"%{value}%"))
                    .Select(u => u.NtLogin);
                baseQuery = baseQuery.Where(ul => matchingLogins.Contains(ul.NtLogin));
            }

            if (filters.TryGetValue("NtLogin", out var ntloginFilter)
                && !string.IsNullOrWhiteSpace(ntloginFilter))
            {
                var value = ntloginFilter.Trim();
                baseQuery = baseQuery.Where(ul => EF.Functions.ILike(ul.NtLogin, $"%{value}%"));
            }

            if (filters.TryGetValue("SystemId", out var systemIdFilter)
                && int.TryParse(systemIdFilter, out var systemIdVal))
            {
                baseQuery = baseQuery.Where(ul => ul.SystemId == systemIdVal);
            }

            return baseQuery;
        }

        private IQueryable<AccessUserLevel> ApplyAccessUserLevelSorting(IQueryable<AccessUserLevel> baseQuery, string? sortBy, bool descending)
        {
            return (sortBy, descending) switch
            {
                ("NtLogin", true)        => baseQuery.OrderByDescending(ul => ul.NtLogin),
                ("NtLogin", false)       => baseQuery.OrderBy(ul => ul.NtLogin),
                ("UserName", true)       => baseQuery
                    .OrderByDescending(ul => _dbContext.AccessUsers
                        .Where(u => u.SystemId == ul.SystemId && u.NtLogin == ul.NtLogin)
                        .Select(u => u.UserName)
                        .FirstOrDefault())
                    .ThenByDescending(ul => ul.NtLogin),
                ("UserName", false)      => baseQuery
                    .OrderBy(ul => _dbContext.AccessUsers
                        .Where(u => u.SystemId == ul.SystemId && u.NtLogin == ul.NtLogin)
                        .Select(u => u.UserName)
                        .FirstOrDefault())
                    .ThenBy(ul => ul.NtLogin),
                ("AccessLevelId", true)  => baseQuery.OrderByDescending(ul => ul.AccessLevelId),
                ("AccessLevelId", false) => baseQuery.OrderBy(ul => ul.AccessLevelId),
                ("SystemId", true)       => baseQuery.OrderByDescending(ul => ul.SystemId),
                ("SystemId", false)      => baseQuery.OrderBy(ul => ul.SystemId),
                (_, true)                 => baseQuery.OrderByDescending(ul => ul.SystemId).ThenByDescending(ul => ul.NtLogin),
                _                         => baseQuery.OrderBy(ul => ul.SystemId).ThenBy(ul => ul.NtLogin)
            };
        }
        public async Task<List<AccessUserLevel>> GetBySystemIdAsync(int systemId)
        {
            return await _dbContext.AccessUserLevels
                .AsNoTracking()
                .Where(ul => ul.SystemId == systemId)
                .OrderBy(ul => ul.NtLogin)
                .ThenBy(ul => ul.AccessLevelId)
                .ToListAsync();
        }
        public async Task<List<AccessUserLevel>> GetByUserAsync(int systemId, string ntLogin)
        {
            return await _dbContext.AccessUserLevels
                .AsNoTracking()
                .Where(ul => ul.SystemId == systemId && ul.NtLogin == ntLogin)
                .OrderBy(ul => ul.AccessLevelId)
                .ToListAsync();
        }
        public async Task<AccessUserLevel?> GetByIdAsync(int systemId, string ntLogin, int accessLevelId)
        {
            return await _dbContext.AccessUserLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(ul => ul.SystemId == systemId
                                        && ul.NtLogin == ntLogin
                                        && ul.AccessLevelId == accessLevelId);
        }
        public async Task<AccessUserLevel> AddAsync(AccessUserLevel entity)
        {
            _dbContext.AccessUserLevels.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int systemId, string ntLogin, int accessLevelId)
        {
            int rowsAffected = await _dbContext.AccessUserLevels
                .Where(ul => ul.SystemId == systemId
                          && ul.NtLogin == ntLogin
                          && ul.AccessLevelId == accessLevelId)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }
        public async Task<bool> ExistsAsync(int systemId, string ntLogin, int accessLevelId)
        {
            return await _dbContext.AccessUserLevels
                .AnyAsync(ul => ul.SystemId == systemId
                             && ul.NtLogin == ntLogin
                             && ul.AccessLevelId == accessLevelId);
        }
    }
}

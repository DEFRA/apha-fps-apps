using System.Diagnostics.CodeAnalysis;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Enums;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;

namespace Apha.FPS.DataAccess.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public UserRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users
                .AsNoTracking()
                .OrderBy(u => u.Comments)
                .ToListAsync();
        }

        public async Task<PagedData<User>> GetAllUsersPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var usersQuery = _dbContext.Users
                .AsNoTracking()
                .AsQueryable();

            usersQuery = ApplyUserFilter(usersQuery, query.Filter);

            usersQuery = ApplyUserSorting(usersQuery, query.SortBy, query.Descending);

            var users = await usersQuery.ToListAsync();
            return ApplyPaging(users, query.Page, query.PageSize);
        }

        public async Task<PagedData<User>> GetNonSuperUsersPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var superUserId = (int)Core.Enums.SuperUser.SuperUserId;

            var usersQuery = _dbContext.Users
                .AsNoTracking()
                .Where(u => u.UserId != superUserId)
                .AsQueryable();

            usersQuery = ApplyUserFilter(usersQuery, query.Filter);

            usersQuery = ApplyUserSorting(usersQuery, query.SortBy, query.Descending);

            var users = await usersQuery.ToListAsync();
            return ApplyPaging(users, query.Page, query.PageSize);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username != null
                    && EF.Functions.ILike(u.Username, username));
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserEmail != null
                    && EF.Functions.ILike(u.UserEmail, email));
        }

        public async Task<User> AddUserAsync(User entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbContext.Users.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<User> UpdateUserAsync(User entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var existing = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.UserId == entity.UserId)
                ?? throw new ArgumentException($"User with ID {entity.UserId} not found.");

            existing.Username = entity.Username;
            existing.Comments = entity.Comments;
            existing.UserEmail = entity.UserEmail;
            existing.Dt2Username = entity.Dt2Username;

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        [ExcludeFromCodeCoverage(Justification = "Uses ExecuteDeleteAsync with transactions which cannot be unit tested with mocked DbContext.")]
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await _dbContext.UserProfitcentres.IgnoreQueryFilters()
                        .Where(x => x.UserId == userId).ExecuteDeleteAsync();
                    await _dbContext.UserPrograms.IgnoreQueryFilters()
                        .Where(x => x.UserID == userId).ExecuteDeleteAsync();
                    await _dbContext.UserCategories.IgnoreQueryFilters()
                        .Where(x => x.UserId == userId).ExecuteDeleteAsync();
                    await _dbContext.UserTestOwners.IgnoreQueryFilters()
                        .Where(x => x.UserId == userId).ExecuteDeleteAsync();
                    await _dbContext.UserProjectGroups.IgnoreQueryFilters()
                        .Where(x => x.UserId == userId).ExecuteDeleteAsync();

                    var rowsAffected = await _dbContext.Users
                        .Where(u => u.UserId == userId).ExecuteDeleteAsync();

                    await transaction.CommitAsync();
                    return rowsAffected > 0;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<string>> GetUserProfitCentresAsync(int userId)
        {
            return await _dbContext.UserProfitcentres
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.FpsYear == _requestContext.FpsYear)
                .Select(x => x.ProfitCentre)
                .ToListAsync();
        }

        public async Task<List<string>> GetUserProgramsAsync(int userId)
        {
            return await _dbContext.UserPrograms
                .AsNoTracking()
                .Where(x => x.UserID == userId && x.FpsYear == _requestContext.FpsYear)
                .Select(x => x.ProgramNo)
                .ToListAsync();
        }

        public async Task<List<string>> GetUserCategoriesAsync(int userId)
        {
            return await _dbContext.UserCategories
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.FpsYear == _requestContext.FpsYear)
                .Select(x => x.Category)
                .ToListAsync();
        }

        public async Task<List<string>> GetUserTestOwnersAsync(int userId)
        {
            return await _dbContext.UserTestOwners
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.FpsYear == _requestContext.FpsYear)
                .Select(x => x.TestOwner)
                .ToListAsync();
        }

        public async Task<List<string>> GetUserProjectGroupsAsync(int userId)
        {
            return await _dbContext.UserProjectGroups
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.FpsYear == _requestContext.FpsYear)
                .Select(x => x.ProjectGroup)
                .ToListAsync();
        }

        [ExcludeFromCodeCoverage(Justification = "Uses ExecuteDeleteAsync with transactions which cannot be unit tested with mocked DbContext.")]
        public async Task SaveUserPermissionsAsync(int userId, List<string> profitCentres, List<string> programs,
            List<string> categories, List<string> testOwners, List<string> projectGroups)
        {
            var fpsYear = _requestContext.FpsYear;
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await _dbContext.UserProfitcentres
                        .Where(x => x.UserId == userId && x.FpsYear == fpsYear).ExecuteDeleteAsync();
                    await _dbContext.UserPrograms
                        .Where(x => x.UserID == userId && x.FpsYear == fpsYear).ExecuteDeleteAsync();
                    await _dbContext.UserCategories
                        .Where(x => x.UserId == userId && x.FpsYear == fpsYear).ExecuteDeleteAsync();
                    await _dbContext.UserTestOwners
                        .Where(x => x.UserId == userId && x.FpsYear == fpsYear).ExecuteDeleteAsync();
                    await _dbContext.UserProjectGroups
                        .Where(x => x.UserId == userId && x.FpsYear == fpsYear).ExecuteDeleteAsync();

                    if (profitCentres.Count > 0)
                    {
                        _dbContext.UserProfitcentres.AddRange(profitCentres.Select(pc => new UserProfitcentre
                        {
                            UserId = userId,
                            ProfitCentre = pc,
                            FpsYear = fpsYear
                        }));
                    }

                    if (programs.Count > 0)
                    {
                        _dbContext.UserPrograms.AddRange(programs.Select(p => new UserProgram
                        {
                            UserID = userId,
                            ProgramNo = p,
                            FpsYear = fpsYear
                        }));
                    }

                    if (categories.Count > 0)
                    {
                        _dbContext.UserCategories.AddRange(categories.Select(c => new UserCategory
                        {
                            UserId = userId,
                            Category = c,
                            FpsYear = fpsYear
                        }));
                    }

                    if (testOwners.Count > 0)
                    {
                        _dbContext.UserTestOwners.AddRange(testOwners.Select(t => new UserTestOwner
                        {
                            UserId = userId,
                            TestOwner = t,
                            FpsYear = fpsYear
                        }));
                    }

                    if (projectGroups.Count > 0)
                    {
                        _dbContext.UserProjectGroups.AddRange(projectGroups.Select(pg => new UserProjectGroup
                        {
                            UserId = userId,
                            ProjectGroup = pg,
                            FpsYear = fpsYear
                        }));
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<string>> GetAllProfitCentreOptionsAsync()
        {
            var currentUserId = await GetCurrentUserIdAsync();

            return await _dbContext.UserProfitcentres
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(p => p.UserId == currentUserId && p.FpsYear == _requestContext.FpsYear)
                .Select(p => p.ProfitCentre)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllProgramOptionsAsync()
        {
            var currentUserId = await GetCurrentUserIdAsync();

            return await _dbContext.UserPrograms
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(p => p.UserID == currentUserId && p.FpsYear == _requestContext.FpsYear)
                .Select(p => p.ProgramNo)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllCategoryOptionsAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Select(c => c.CategoryName)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllTestOwnerOptionsAsync()
        {
            return await _dbContext.UserTestOwners
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(p => p.FpsYear == _requestContext.FpsYear)
                .Select(t => t.TestOwner)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllProjectGroupOptionsAsync()
        {
            return await _dbContext.ProjectGroups
                .AsNoTracking()
                .Where(p => p.FpsYear == _requestContext.FpsYear)
                .Select(pg => pg.ProjectGroupName)
                .Distinct()
                .OrderBy(pg => pg)
                .ToListAsync();
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var currentUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserEmail != null
                    && EF.Functions.ILike(u.UserEmail, _requestContext.UserEmailId));

            return currentUser?.UserId ?? (int)SuperUser.SuperUserId;
        }

        private static IQueryable<User> ApplyUserSorting(IQueryable<User> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(u => u.Comments);

            return sortBy.ToLower() switch
            {
                "dt2username" => descending ? query.OrderByDescending(u => u.Dt2Username) : query.OrderBy(u => u.Dt2Username),
                "username" => descending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
                "comments" => descending ? query.OrderByDescending(u => u.Comments) : query.OrderBy(u => u.Comments),
                "useremail" => descending ? query.OrderByDescending(u => u.UserEmail) : query.OrderBy(u => u.UserEmail),
                "userid" => descending ? query.OrderByDescending(u => u.UserId) : query.OrderBy(u => u.UserId),
                _ => query.OrderBy(u => u.Comments)
            };
        }

        private static IQueryable<User> ApplyUserFilter(IQueryable<User> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Dt2Username", out var Dt2Username) && Dt2Username != null)
                query = query.Where(u => EF.Functions.ILike(u.Dt2Username!, $"%{Dt2Username}%"));

            if (dict.TryGetValue("Username", out var Username) && Username != null)
                query = query.Where(u => EF.Functions.ILike(u.Username!, $"%{Username}%"));

            if (dict.TryGetValue("UserEmail", out var UserEmail) && UserEmail != null)
                query = query.Where(u => EF.Functions.ILike(u.UserEmail!, $"%{UserEmail}%"));

            if (dict.TryGetValue("Comments", out var Comments) && Comments != null)
                query = query.Where(u => EF.Functions.ILike(u.Comments!, $"%{Comments}%"));

            return query;
        }
    }
}

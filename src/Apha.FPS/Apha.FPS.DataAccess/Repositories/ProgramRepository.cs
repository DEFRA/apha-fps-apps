using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;


namespace Apha.FPS.DataAccess.Repositories
{
    public class ProgramRepository : BaseRepository, IProgramRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsYearContext _yearContext;
        private readonly int userId = 42;
        public ProgramRepository(FpsDbContext dbContext, IFpsYearContext yearContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _yearContext = yearContext;
        }
              
        public async Task<IEnumerable<Program>> GetAllProgramsAsync()
        {
            return await _dbContext.ProgramViews.Where(p => p.UserId == userId)
                .Select(p => new Program
                {
                    ProgramNo = p.ProgramNo ?? "",
                    ProgramName = p.ProgramName,
                    Directorate = p.Directorate,
                    Target = p.Target,
                    Manager = p.Manager
                }).ToListAsync();
        }

        public async Task<PagedData<Program>> GetAllProgramsAsync(PaginationParameters<string> query)
        {

            var programQuery =  _dbContext.ProgramViews.Where(p => p.UserId == userId)
                                .Select(p => new Program
                                {
                                    ProgramNo = p.ProgramNo ?? "",
                                    ProgramName = p.ProgramName,
                                    Directorate = p.Directorate,
                                    Target = p.Target,
                                    Manager = p.Manager
                                }).AsQueryable();
        

            programQuery = ApplyProgramFilter(programQuery, query.Filter);
            programQuery = (IQueryable<Program>)ApplySorting(programQuery, query.SortBy, query.Descending);

            var result = await programQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<Program?> GetProgramByIdAsync(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);

            return await _dbContext.Programs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProgramNo == id);
        }

        public async Task<Program> AddProgramAsync(Program entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            entity.FpsCalYear = _yearContext.FPSYear;

            _dbContext.Programs.Add(entity);

            var dboUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "dbo");
            if (dboUser != null)
            {
                var userProgram = new UserProgram
                {
                    ProgramNo = entity.ProgramNo,
                    UserID = dboUser.UserId,
                    FpsCalYear = _yearContext.FPSYear
                };
                _dbContext.UserPrograms.Add(userProgram);
            }

            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Program> UpdateProgramAsync(Program entity)
        {

            ArgumentNullException.ThrowIfNull(entity);
            entity.FpsCalYear = _yearContext.FPSYear;

            _dbContext.Programs.Update(entity);
            var dboUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == "dbo");

            if (dboUser != null)
            {
                var userProgramExists = await _dbContext.UserPrograms
                    .AnyAsync(up => up.ProgramNo == entity.ProgramNo && up.UserID == dboUser.UserId);

                if (!userProgramExists)
                {
                    var userProgram = new UserProgram
                    {
                        ProgramNo = entity.ProgramNo,
                        UserID = dboUser.UserId,
                        FpsCalYear = _yearContext.FPSYear
                    };
                    _dbContext.UserPrograms.Add(userProgram);
                }
            }

            await _dbContext.SaveChangesAsync();
            return entity;

        }

        public async Task<bool> DeleteProgramAsync(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);

            await _dbContext.UserPrograms
                .Where(up => up.ProgramNo == id)
                .ExecuteDeleteAsync();

            var rowsAffected = await _dbContext.Programs
                .Where(p => p.ProgramNo == id)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;


        }

        private static IQueryable ApplySorting(IQueryable<Program> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<Program> query, string property, bool descending)
        {
            return property switch
            {
                "programno" => ApplyOrder(query, i => i.ProgramNo, descending),
                "programname" => ApplyOrder(query, i => i.ProgramName, descending),
                "directorate" => ApplyOrder(query, i => i.Directorate, descending),
                "target" => ApplyOrder(query, i => i.Target, descending),
                "manager" => ApplyOrder(query, i => i.Manager, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<Program> query, Expression<Func<Program, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<Program> ApplyProgramFilter(IQueryable<Program> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ProgramNo", out var programNo) && programNo != null)
                query = query.Where(x => x.ProgramNo.Contains(programNo.ToString()!));

            if (dict.TryGetValue("ProgramName", out var programName) && programName != null)
                query = query.Where(x => x.ProgramName!.Contains(programName.ToString()!));

            if (dict.TryGetValue("Directorate", out var directorate) && directorate != null)
                query = query.Where(x => x.Directorate!.Contains(directorate.ToString()!));

            if (dict.TryGetValue("Manager", out var manager) && manager != null)
                query = query.Where(x => x.Manager!.Contains(manager.ToString()!));

            return query;
        }

    }
}

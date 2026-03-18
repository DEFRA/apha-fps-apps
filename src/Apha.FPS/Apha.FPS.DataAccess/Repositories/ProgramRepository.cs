using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Apha.FPS.DataAccess.Repositories
{
    public class ProgramRepository : BaseRepository, IProgramRepository
    {       
        private readonly FpsDbContext _dbContext;
        private readonly IFpsYearContext _yearContext;
        public ProgramRepository(FpsDbContext dbContext, IFpsYearContext yearContext) : base(dbContext)
        {          
            _dbContext = dbContext;
            _yearContext = yearContext;
        }

        public IQueryable<Program> Get()
        {           
            var data = (from p in _dbContext.Programs
                    join up in _dbContext.UserPrograms
                        on p.ProgramNo equals up.ProgramNo                       
                    join u in _dbContext.Users
                        on up.UserID equals u.UserId
                    where u.Username == "dbo"
                        select p).AsQueryable();

            return data;
        }

        public async Task<IEnumerable<Program>> GetAllProgramsAsync()
        {
            return await Get().ToListAsync();
        }

        public async Task<PagedData<Program>> GetAllProgramsAsync(PaginationParameters<string> query)
        {

            var programQuery = Get();
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

        public async Task<List<string?>> GetAllDirectoratesAsync()
        {
            var hardcoded = new List<string> { "CSG", "Surveillance", "Lab Services" };

            var dbDirectorates =  Get().AsQueryable();
            var directorates = await dbDirectorates.Select(p => p.Directorate)
                .Where(d => !string.IsNullOrEmpty(d))               
                .ToListAsync();
           
            var allDirectorates = hardcoded
                .Union(directorates, StringComparer.OrdinalIgnoreCase)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .ToList();

            return allDirectorates;
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

    }
}

using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class AnimalRepository : BaseRepository, IAnimalRepository 
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public AnimalRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<List<Animal>> GetAnimalLookup() => await _dbContext.Animals.ToListAsync();

        public async Task<PagedData<AnimalCostView>> GetAnimalCostAsync(PaginationParameters<string> query, string jobCode)
        {
            var queryAnimalCost = BuildAnimalCostQuery(jobCode);

            queryAnimalCost = (IQueryable<AnimalCostView>)ApplySorting(queryAnimalCost, query.SortBy, query.Descending);

            var result = await queryAnimalCost.ToListAsync();

            var animalCostViews = result.Select(e => {
                e.AnimalCost = (decimal)e.NumberOfDays * (decimal)e.NumberOfAnimals * (e.DailyRate ?? 0m);
                return e;
            }).ToList();

            return base.ApplyPaging(animalCostViews, query.Page, query.PageSize);
        }

        public async Task<decimal> GetTotalAnimalCostAsync(string jobCode)
        {
            var result = await BuildAnimalCostQuery(jobCode).ToListAsync();
            return result.Sum(e => (decimal)e.NumberOfDays * (decimal)e.NumberOfAnimals * (e.DailyRate ?? 0m));
        }

        public async Task<AnimalCostView?> GetAnimalCostViewByIdAsync(int indCounter, string jobCode)
        {
            var record = await BuildAnimalCostQuery(jobCode)
                .Where(e => e.IndCounter == indCounter)
                .FirstOrDefaultAsync();

            if (record == null) return null;
            record.AnimalCost = (decimal)record.NumberOfDays * (decimal)record.NumberOfAnimals * (record.DailyRate ?? 0m);
            return record;
        }

        public async Task<decimal?> GetAnimalRateByIdAsync(string animalType)
        {
            var queryAnimalCost = from animalReq in _dbContext.AnimalRequestViews
                                  join animal in _dbContext.Animals on animalReq.AnimalType equals animal.AnimalType
                                  join project in _dbContext.ProjectViews on
                                         new { animalReq.JobCode, animalReq.UserId } equals new { JobCode = project.ParentProject, project.UserId }
                                  let dailyRate = (project.IsDefraProject == -1 ? animal.DefraDailyRate : animal.DailyRate)
                                  where animal.AnimalType == animalType
                                      && animalReq.UserEmail != null
                                      && animalReq.UserEmail.ToLower() == _requestContext.UserEmailId
                                  select dailyRate;

            return await queryAnimalCost.FirstOrDefaultAsync();
        }

        public async Task<AnimalRequest> AddAnimalCostAsync(AnimalRequest animalReq)
        {
            ArgumentNullException.ThrowIfNull(animalReq);
            animalReq.FpsYear = _requestContext.FpsYear;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var logEntry = CreateAnimalRequestLogEntry(animalReq, "I");

                    _dbContext.AnimalRequests.Add(animalReq);
                    _dbContext.AnimalRequestLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return animalReq;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<AnimalRequest> UpdateAnimalCostAsync(AnimalRequest animalReq)
        {
            ArgumentNullException.ThrowIfNull(animalReq);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existingEntity = await _dbContext.AnimalRequests.FindAsync(animalReq.IndCounter);

                    if (existingEntity == null)
                        throw new InvalidOperationException(
                            $"Animal cost with AnimalType {animalReq.AnimalType} not found");

                    existingEntity.JobCode = animalReq.JobCode;
                    existingEntity.AnimalType = animalReq.AnimalType;
                    existingEntity.NumberOfDays = animalReq.NumberOfDays;
                    existingEntity.NumberOfAnimals = animalReq.NumberOfAnimals;
                    existingEntity.FpsYear = _requestContext.FpsYear;

                    var logEntry = CreateAnimalRequestLogEntry(existingEntity, "U");

                    _dbContext.AnimalRequestLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return existingEntity;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> DeleteJobAnimalCostAsync(int indCounter)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(indCounter);

            var entity = await _dbContext.AnimalRequests.FindAsync(indCounter);
            if (entity == null)
                return false;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var logEntry = CreateAnimalRequestLogEntry(entity, "D");

                    _dbContext.AnimalRequests.Remove(entity);
                    _dbContext.AnimalRequestLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private AnimalRequestLog CreateAnimalRequestLogEntry(AnimalRequest animalReq, string insertDelete)
        {
            return new AnimalRequestLog
            {
                JobCode = animalReq.JobCode,
                AnimalType = animalReq.AnimalType,
                NumberOfDays = animalReq.NumberOfDays,
                NumberOfAnimals = animalReq.NumberOfAnimals,
                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId = _requestContext.UserEmailId?.Length > 20
                    ? _requestContext.UserEmailId[..20]
                    : _requestContext.UserEmailId,
                InsertDelete = insertDelete,
                FpsYear = _requestContext.FpsYear
            };
        }

        private IQueryable<AnimalCostView> BuildAnimalCostQuery(string jobCode)
        {
            return from animalReq in _dbContext.AnimalRequestViews
                   join animal in _dbContext.Animals on animalReq.AnimalType equals animal.AnimalType
                   join project in _dbContext.ProjectViews on
                          new { animalReq.JobCode, animalReq.UserId } equals new { JobCode = project.ParentProject, project.UserId }
                   let dailyRate = (project.IsDefraProject == -1 ? animal.DefraDailyRate : animal.DailyRate)
                   where animalReq.JobCode == jobCode
                       && animalReq.UserEmail != null
                       && animalReq.UserEmail.ToLower() == _requestContext.UserEmailId
                   select new AnimalCostView
                   {
                       IndCounter = animalReq.IndCounter,
                       Programme = project.Program,
                       AnimalType = animalReq.AnimalType,
                       JobCode = animalReq.JobCode,
                       NumberOfDays = animalReq.NumberOfDays,
                       NumberOfAnimals = animalReq.NumberOfAnimals,
                       DailyRate = dailyRate,
                       TotalDays = animalReq.NumberOfAnimals * animalReq.NumberOfDays
                   };
        }

        private static IQueryable ApplySorting(IQueryable<AnimalCostView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<AnimalCostView> query, string property, bool descending)
        {
            return property switch
            {
                "animaltype" => ApplyOrder(query, i => i.AnimalType, descending),
                "animalcost" => ApplyOrder(query, i => i.AnimalCost, descending),
                "dailyrate" => ApplyOrder(query, i => i.DailyRate, descending),
                "numberofdays" => ApplyOrder(query, i => i.NumberOfDays, descending),
                "numberofanimals" => ApplyOrder(query, i => i.NumberOfAnimals, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<AnimalCostView> query, Expression<Func<AnimalCostView, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }       

    }
}

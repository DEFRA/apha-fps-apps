using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPS.DataAccess.Repositories
{
    public class AnimalRepository : BaseRepository, IAnimalRepository 
    {
        private readonly FpsDbContext _dbContext;      
        private readonly IProjectRepository _projectRepository;
        public AnimalRepository(FpsDbContext dbContext,
            IProjectRepository projectRepository) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public IQueryable<AnimalRequest> Get()
        {
            var projects = _projectRepository.Get();

            return (from p in _dbContext.AnimalRequests
                    join ap in projects
                        on p.JobCode equals ap.ParentProject
                    select p).AsQueryable();
        }

        public async Task<List<Animal>> GetAnimalLookup() => await _dbContext.Animals.ToListAsync();

        public async Task<PagedData<AnimalCostView>> GetAnimalCostAsync(PaginationParameters<string> query, string jobCode)
        {           
            var projects = _projectRepository.Get();
            var animalRequest = Get();            

            var queryAnimalCost = from animalReq in animalRequest
                                    join animal in _dbContext.Animals on animalReq.AnimalType equals animal.AnimalType
                                    join project in projects on animalReq.JobCode equals project.ParentProject
                                    let dailyRate = (project.IsDefraProject == -1 ? animal.DefraDailyRate : animal.DailyRate)
                                    where animalReq.JobCode == jobCode
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
           

            queryAnimalCost = (IQueryable<AnimalCostView>)ApplySorting(queryAnimalCost, query.SortBy, query.Descending);

            var result = await queryAnimalCost.ToListAsync();

            var animalCostViews = result.Select(e => {
                e.AnimalCost = (decimal)e.NumberOfDays * (decimal)e.NumberOfAnimals * (e.DailyRate ?? 0m);
                return e;
            }).ToList();

            return base.ApplyPaging(animalCostViews, query.Page, query.PageSize);
        }

        public async Task<decimal?> GetAnimalRateByIdAsync(string animalType)
        {
            var projects = _projectRepository.Get();
            var animalRequest = Get();

            var queryAnimalCost = from animalReq in animalRequest
                                  join animal in _dbContext.Animals on animalReq.AnimalType equals animal.AnimalType
                                  join project in projects on animalReq.JobCode equals project.ParentProject
                                  let dailyRate = project.IsDefraProject == -1 ? animal.DefraDailyRate : animal.DailyRate
                                  where animal.AnimalType == animalType
                                  select dailyRate;

            return await queryAnimalCost.FirstOrDefaultAsync();
        }

        public async Task<AnimalRequest> AddAnimalCostAsync(AnimalRequest animalReq)
        {
            ArgumentNullException.ThrowIfNull(animalReq);
            _dbContext.AnimalRequests.Add(animalReq);
            await _dbContext.SaveChangesAsync();
            return animalReq;
        }

        public async Task<AnimalRequest> UpdateAnimalCostAsync(AnimalRequest animalReq)
        {
            ArgumentNullException.ThrowIfNull(animalReq);
            
            var existingEntity = await _dbContext.AnimalRequests.FindAsync(animalReq.IndCounter);

            if (existingEntity == null)
            {
                throw new InvalidOperationException(
                    $"Animal cost with AnimalType {animalReq.AnimalType} not found");
            }

            existingEntity.JobCode = animalReq.JobCode;
            existingEntity.AnimalType = animalReq.AnimalType;
            existingEntity.NumberOfDays = animalReq.NumberOfDays;
            existingEntity.NumberOfAnimals = animalReq.NumberOfAnimals;
        
            await _dbContext.SaveChangesAsync();

            return existingEntity;
        }

        public async Task<bool> DeleteJobAnimalCostAsync(int indCounter)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(indCounter);
           
            var entity = await _dbContext.AnimalRequests.FindAsync(indCounter);
            if (entity == null)
            {
                return false;
            }

            _dbContext.AnimalRequests.Remove(entity);
            await _dbContext.SaveChangesAsync();

            return true;
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

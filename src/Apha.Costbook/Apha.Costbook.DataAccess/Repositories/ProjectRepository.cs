using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly CostbookDbContext _context;

        public ProjectRepository(CostbookDbContext context)
        {
            _context = context;
        }

        public async Task<PagedData<Project>> GetPaginatedProjectsAsync(PaginationParameters<string> queryFilter)
        {
            var queryProjects = _context.Projects
                .AsNoTracking()
                .AsQueryable();

           
            // Apply general filtering
            queryProjects = ApplyProjectFilter(queryProjects, queryFilter.Filter);            

            // Apply sorting
            queryProjects = (IQueryable<Project>)ApplySorting(queryProjects, queryFilter.SortBy, queryFilter.Descending);

            // Execute query
            var result = await queryProjects.ToListAsync();

            // Apply paging
            return ApplyPaging(result, queryFilter.Page, queryFilter.PageSize);
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync(string? contractFilter, string? submittedByFilter)
        {
            var query = _context.Projects.AsQueryable();
            if (!string.IsNullOrEmpty(contractFilter))
                query = query.Where(p => p.ContractNumber == contractFilter);
            if (!string.IsNullOrEmpty(submittedByFilter))
                query = query.Where(p => (p.SubmittedByFName + ", " + p.SubmittedByFName) == submittedByFilter);
            return await query.OrderByDescending(p => p.ProjectId).ToListAsync();
        }
    public async Task<Project?> GetProjectByIdAsync(string id)
    {
            var decodedId = HttpUtility.UrlDecode(id);
          
        return await _context.Set<Project>().FirstOrDefaultAsync(p => p.ProjectId == decodedId);
    }


    public async Task<Project> AddProjectAsync(Project project)
        {
            
            var dbSet = _context.Set<Project>(); 
            dbSet.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {            
            var dbSet = _context.Set<Project>();
            dbSet.Update(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> DeleteProjectAsync(string id)
        {
            var decodedId = HttpUtility.UrlDecode(id);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Delete in correct order (children first)
                await _context.Set<AnimalRequirement>()
                    .Where(ar => ar.Project == decodedId)
                    .ExecuteDeleteAsync();

                await _context.Set<AdditionalCost>()
                    .Where(ac => ac.Project == decodedId)
                    .ExecuteDeleteAsync();

                await _context.Set<TestRequirement>()
                    .Where(t => t.Project == decodedId)
                    .ExecuteDeleteAsync();

                await _context.Set<StaffRequirement>()
                    .Where(sr => sr.Project == decodedId)
                    .ExecuteDeleteAsync();

                await _context.Set<ProjectYear>()
                    .Where(py => py.Project == decodedId)
                    .ExecuteDeleteAsync();

                var project = await _context.Set<Project>()
                    .FirstOrDefaultAsync(p => p.ProjectId == decodedId);

                if (project == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                _context.Set<Project>().Remove(project);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            });
        }
        public async Task<Project> CopyProjectAsync(Project newProject, string sourceProjectId)
        {
            var decodedSourceId = HttpUtility.UrlDecode(sourceProjectId);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // 1. Insert the new project record
                _context.Set<Project>().Add(newProject);
                await _context.SaveChangesAsync();

                // 2. Copy ProjectYear records
                var sourceProjectYears = await _context.Set<ProjectYear>()
                    .AsNoTracking()
                    .Where(py => py.Project == decodedSourceId)
                    .ToListAsync();

                foreach (var sourcePY in sourceProjectYears)
                {
                    var newPY = new ProjectYear
                    {
                        Project = newProject.ProjectId,
                        YearValue = sourcePY.YearValue

                    };
                    _context.Set<ProjectYear>().Add(newPY);
                }

                // 3. Copy AnimalRequirement records
                var sourceAnimalReqs = await _context.Set<AnimalRequirement>()
                    .AsNoTracking()
                    .Where(ar => ar.Project == decodedSourceId)
                    .ToListAsync();

                foreach (var sourceAR in sourceAnimalReqs)
                {
                    var newAR = new AnimalRequirement
                    {
                        Project = newProject.ProjectId,
                        Year = sourceAR.Year,
                        AnimalType = sourceAR.AnimalType,
                        NumberOfDays = sourceAR.NumberOfDays,
                        NumberOfAnimals = sourceAR.NumberOfAnimals,
                        DailyRate = sourceAR.DailyRate
                    };
                    _context.Set<AnimalRequirement>().Add(newAR);
                }

                // 4. Copy AdditionalCost records
                var sourceAdditionalCosts = await _context.Set<AdditionalCost>()
                    .AsNoTracking()
                    .Where(ac => ac.Project == decodedSourceId)
                    .ToListAsync();

                foreach (var sourceAC in sourceAdditionalCosts)
                {
                    var newAC = new AdditionalCost
                    {
                        Project = newProject.ProjectId,
                        Year = sourceAC.Year,
                        AccountCat = sourceAC.AccountCat,
                        Description = sourceAC.Description,
                        ItemCost = sourceAC.ItemCost,
                        CostEntered = sourceAC.CostEntered,
                        Freq = sourceAC.Freq
                    };
                    _context.Set<AdditionalCost>().Add(newAC);
                }

                // 5. Copy StaffRequirement records
                var sourceStaffReqs = await _context.Set<StaffRequirement>()
                    .AsNoTracking()
                    .Where(sr => sr.Project == decodedSourceId)
                    .ToListAsync();

                foreach (var sourceSR in sourceStaffReqs)
                {
                    var newSR = new StaffRequirement
                    {
                        Project = newProject.ProjectId,
                        Year = sourceSR.Year,
                        WgGrade = sourceSR.WgGrade,
                        Name = sourceSR.Name,
                        Nohours = sourceSR.Nohours,
                        Nodays = sourceSR.Nodays,
                        Chargerate = sourceSR.Chargerate,
                        Payrate = sourceSR.Payrate,
                        Npr = sourceSR.Npr,
                        Ohr = sourceSR.Ohr
                    };
                    _context.Set<StaffRequirement>().Add(newSR);
                }

                // 6. Copy TestRequirement records
                var sourceTestReqs = await _context.Set<TestRequirement>()
                    .AsNoTracking()
                    .Where(tr => tr.Project == decodedSourceId)
                    .ToListAsync();

                foreach (var sourceTR in sourceTestReqs)
                {
                    var newTR = new TestRequirement
                    {
                        Project = newProject.ProjectId,
                        Year = sourceTR.Year,
                        TestCode = sourceTR.TestCode,
                        NumberOfTests = sourceTR.NumberOfTests,
                        UnitPrice = sourceTR.UnitPrice
                    };
                    _context.Set<TestRequirement>().Add(newTR);
                }

                // Save all copied records
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return newProject;
            });
        }
        public async Task<string> GetNextProjectNumberAsync(string? baseNumber)
        {
            if (!string.IsNullOrEmpty(baseNumber))
            {
                baseNumber = HttpUtility.UrlDecode(baseNumber);
            }

            var currentYear = GetCurrentFinancialYear();

            var dbSet = _context.Set<Project>();

            if (string.IsNullOrEmpty(baseNumber))
            {
                // Get only the highest ProjectId for the year (NO full list)
                var maxProjectId = await dbSet
                    .Where(p => p.ProjectId.StartsWith($"{currentYear}/"))
                    .OrderByDescending(p => p.ProjectId)
                    .Select(p => p.ProjectId)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(maxProjectId))
                {
                    return $"{currentYear}/001";
                }

                // Extract numeric part safely
                var parts = maxProjectId.Split('/');
                if (parts.Length == 2 && parts[1].Length >= 3 &&
                    int.TryParse(parts[1].Substring(0, 3), out int num))
                {
                    return $"{currentYear}/{(num + 1):D3}";
                }

                return $"{currentYear}/001";
            }
            else
            {
                // CASE 1: 2024/001a → increment letter
                if (baseNumber.Length == 9 && char.IsLetter(baseNumber[8]))
                {
                    var basePattern = baseNumber.Substring(0, 8);

                    var maxProject = await dbSet
                        .Where(p => p.ProjectId.StartsWith(basePattern))
                        .OrderByDescending(p => p.ProjectId)
                        .Select(p => p.ProjectId)
                        .FirstOrDefaultAsync();

                    if (string.IsNullOrEmpty(maxProject))
                    {
                        return baseNumber;
                    }

                    if (maxProject.Length == 9 && char.IsLetter(maxProject[8]))
                    {
                        var nextChar = (char)(maxProject[8] + 1);
                        return $"{basePattern}{nextChar}";
                    }

                    return $"{basePattern}a";
                }

                // CASE 2: 2024/001 → find next suffix
                if (baseNumber.Length == 8)
                {
                    var maxProject = await dbSet
                        .Where(p => p.ProjectId.StartsWith(baseNumber))
                        .OrderByDescending(p => p.ProjectId)
                        .Select(p => p.ProjectId)
                        .FirstOrDefaultAsync();

                    if (string.IsNullOrEmpty(maxProject))
                    {
                        return baseNumber;
                    }

                    if (maxProject.Length == 9 && char.IsLetter(maxProject[8]))
                    {
                        var nextChar = (char)(maxProject[8] + 1);
                        return $"{baseNumber}{nextChar}";
                    }

                    return $"{baseNumber}a";
                }

                // CASE 3: fallback
                var similarProject = await dbSet
                    .Where(p => p.ProjectId.StartsWith(baseNumber))
                    .OrderByDescending(p => p.ProjectId)
                    .Select(p => p.ProjectId)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(similarProject))
                {
                    return baseNumber;
                }

                return $"{similarProject}a";
            }
        }

        private static int GetCurrentFinancialYear()
        {
            var now = DateTime.Now;
            // MS Access logic: if month <= 3 (Jan-Mar), use previous year, otherwise current year
            return now.Month <= 3 ? now.Year - 1 : now.Year;
        }

        // Filtering logic similar to FPS ApplyEmployeeFilter
        private static IQueryable<Project> ApplyProjectFilter(IQueryable<Project> queryProjects, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return queryProjects;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return queryProjects;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ProjectId", out var projectId) && projectId != null)
            {
                queryProjects = queryProjects.Where(x => x.ProjectId.Contains(projectId.ToString()!));
            }           

            return queryProjects;
        }

        // Sorting logic similar to FPS ApplySorting
        private static IQueryable ApplySorting(IQueryable<Project> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderByDescending(p => p.ProjectId);
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        // Property-based sorting similar to FPS ApplySortingByProperty
        private static IQueryable ApplySortingByProperty(IQueryable<Project> query, string property, bool descending)
        {
            return property switch
            {
                "projectid" => ApplyOrder(query, p => p.ProjectId, descending),
                "projecttitle" => ApplyOrder(query, p => p.ProjectTitle, descending),
                "programme" => ApplyOrder(query, p => p.Programme, descending),
                "contractnumber" => ApplyOrder(query, p => p.ContractNumber, descending),
                "customername" => ApplyOrder(query, p => p.CustomerName, descending),
                "disease" => ApplyOrder(query, p => p.Disease, descending),
                "startdate" => ApplyOrder(query, p => p.StartDate, descending),
                "contractprice" => ApplyOrder(query, p => p.ContractPrice, descending),
                "preparedby" => ApplyOrder(query, p => p.PreparedBy, descending),
                "dateofsubmission" => ApplyOrder(query, p => p.DateOfSubmission, descending),
                _ => query.OrderByDescending(p => p.ProjectId)
            };
        }

        // Order application helper similar to FPS ApplyOrder
        private static IQueryable ApplyOrder<T>(IQueryable<Project> query, Expression<Func<Project, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        // Paging logic similar to FPS ApplyPaging (assuming it exists in base repository)
        private static PagedData<Project> ApplyPaging(List<Project> data, int page, int pageSize)
        {
            var totalCount = data.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var pagedItems = data
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paginationData = new PaginationData
            {
                TotalRecords = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return new PagedData<Project>(pagedItems, paginationData);
        }
    }
}

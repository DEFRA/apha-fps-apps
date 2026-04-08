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
                query = query.Where(p => (p.Submittedbylname + ", " + p.Submittedbyfname) == submittedByFilter);
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
            var project = await _context.Set<Project>().FirstOrDefaultAsync(p => p.ProjectId == decodedId);
            if (project == null) return false;
            var dbSet = _context.Set<Project>();
            dbSet.Remove(project);
            await _context.SaveChangesAsync();
            return true;
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
                "projecttitle" => ApplyOrder(query, p => p.Projecttitle, descending),
                "programme" => ApplyOrder(query, p => p.Programme, descending),
                "contractnumber" => ApplyOrder(query, p => p.ContractNumber, descending),
                "customername" => ApplyOrder(query, p => p.CustomerName, descending),
                "disease" => ApplyOrder(query, p => p.Disease, descending),
                "startdate" => ApplyOrder(query, p => p.Startdate, descending),
                "contractprice" => ApplyOrder(query, p => p.Contractprice, descending),
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

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
        private readonly ISettingsRepository _settingsRepository;

        public ProjectRepository(CostbookDbContext context, ISettingsRepository settingsRepository)
        {
            _context = context;
            _settingsRepository = settingsRepository;
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
        public async Task<Project> CopyProjectAsync(Project project, string sourceProjectId)
        {
            var decodedSourceId = HttpUtility.UrlDecode(sourceProjectId);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // 1. Insert the new project record
                _context.Set<Project>().Add(project);
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
                        Project = project.ProjectId,
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
                        Project = project.ProjectId,
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
                        Project = project.ProjectId,
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
                        Project = project.ProjectId,
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
                        Project = project.ProjectId,
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

                return project;
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
        public async Task<bool> RecostProjectAsync(string projectID)
        {
            bool result = false;

            var decodedId = HttpUtility.UrlDecode(projectID);
            var currentYearSetting = await _settingsRepository.GetSettingValueByIdAsync("CurrentYear");
            

            if (string.IsNullOrEmpty(currentYearSetting) || !int.TryParse(currentYearSetting, out int fyear))
            {
                throw new InvalidOperationException("CurrentYear setting not found or invalid in settings table.");
            }
            bool isDefraProject = (await _context.Projects
                .Where(p => p.ProjectId == decodedId)
                .Select(p => (int?)p.IsDefraProject)
                .FirstOrDefaultAsync() ?? 0) != 0;
            #region TestorProducts
            // Step 2: Get all relevant records
            var records = await _context.TestRequirements
                .Where(r => r.Project ==decodedId && r.Year >= fyear)
                .ToListAsync();

            // Step 3: Preload test data (avoid repeated DB calls like DLookup)
            var testData = await _context.FpsTestorProducts
                .ToDictionaryAsync(t => t.ItemCode);

            // Step 4: Loop and update
            foreach (var rec in records)
            {
                if (!testData.TryGetValue(rec.TestCode, out var test))
                    continue;

                decimal basePriceDecimal = isDefraProject
                    ? test.DefraUnitPrice
                    : test.UnitPriceVla.GetValueOrDefault(0);

                double basePrice = (double)basePriceDecimal;
                double inflationFactor = await fnInflation("InflationTests", decodedId, rec.Year, fyear);

                rec.UnitPrice = basePrice * inflationFactor;
            }

            // Step 5: Save changes
            await _context.SaveChangesAsync();
            #endregion

            #region AnimalRequirements
            // Get all animal requirement records
            var animalRecords = await _context.AnimalRequirements
                .Where(ar => ar.Project == decodedId && ar.Year >= fyear)
                .ToListAsync();

            // Preload animal data (avoid repeated DB calls like DLookup)
            var animalData = await _context.FpsAnimals
                .ToDictionaryAsync(a => a.AnimalType);

            // Loop and update
            foreach (var rec in animalRecords)
            {
                if (!animalData.TryGetValue(rec.AnimalType, out var animal))
                    continue;

                decimal? baseRateDecimal = isDefraProject
                    ? animal.DefraDailyRate
                    : animal.DailyRate;

                double baseRate = (double)(baseRateDecimal ?? 0);

                double inflationFactor = await fnInflation("InflationAnimals", decodedId, rec.Year ?? 0, fyear);

                rec.DailyRate = baseRate * inflationFactor;
            }

            // Save changes
            await _context.SaveChangesAsync();
            #endregion

            #region AdditionalCosts
            // Get all additional cost records
            var additionalCostRecords = await _context.AdditionalCosts
                .Where(ac => ac.Project == decodedId && ac.Year >= fyear)
                .ToListAsync();

            // Loop and update
            foreach (var rec in additionalCostRecords)
            {
                double inflatedCost;

                if (await fnUseInflation(rec.AccountCat))
                {
                    double inflationFactor = await fnInflation("InflationExceptional", decodedId, rec.Year ?? 0, fyear);
                    inflatedCost = rec.CostEntered * inflationFactor;
                }
                else
                {
                    inflatedCost = rec.CostEntered;
                }

                rec.ItemCost = inflatedCost;
            }

            // Save changes
            await _context.SaveChangesAsync();
            #endregion

            #region StaffRequirements
            // Get all staff requirement records
            var staffRecords = await _context.StaffRequirements
                .Where(sr => sr.Project == decodedId && sr.Year >= fyear)
                .ToListAsync();

            // Preload pay rates data based on project type
            // Equivalent to qrypayRates_defra or qrypayRates_nondefra
            var payRatesQuery = from wg in _context.WorkGroupGrades
                                join pc in _context.ProfitCentreGrades
                                on wg.ProfitCentreGrade equals pc.PcGrade
                                select new
                                {
                                    wg.WgGrade,
                                    ChargeRate = isDefraProject 
                                        ? pc.DefraChargeRate 
                                        : pc.ChargeRate,
                                    pc.PayRate,
                                    pc.Npr,
                                    Ohr = isDefraProject ? 0 : pc.Ohr
                                };

            // Filter out zero charge rates (applies to both DEFRA and non-DEFRA)
            payRatesQuery = payRatesQuery.Where(x => x.ChargeRate != null && x.ChargeRate != 0);

            var payRatesData = await payRatesQuery.ToDictionaryAsync(x => x.WgGrade);

            // Loop and update
            foreach (var rec in staffRecords)
            {
                if (!payRatesData.TryGetValue(rec.WgGrade, out var payRate))
                    continue;

                double inflationFactor = await fnInflation("InflationStaff", decodedId, rec.Year ?? 0, fyear);

                rec.Chargerate = (double)(payRate.ChargeRate ?? 0) * inflationFactor;
                rec.Payrate = (double)(payRate.PayRate ?? 0) * inflationFactor;
                rec.Npr = (double)(payRate.Npr ?? 0) * inflationFactor;
                rec.Ohr = (double)(payRate.Ohr ?? 0) * inflationFactor;
            }

            // Save changes
            await _context.SaveChangesAsync();
            #endregion

            result = true;
            return result;
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

        private static int fnYearGapSign(int yearGap)
        {
            if (yearGap == 0) return 0;
            if (yearGap > 0) return 1;
            return -1;
        }

        private async Task<bool> fnUseInflation(string accountCat)
        {
            
            var useInflation = await (from ac in _context.FpsAccountCategories
                                       join ag in _context.AccountGroups 
                                       on ac.Csg7Group equals ag.Csg7group
                                       where ac.AccShortName == accountCat
                                       select ag.Useinflation)
                                      .FirstOrDefaultAsync();

            // Return false if null (no match found), otherwise return the value
            return useInflation ?? false;
        }

        private async Task<double> fnInflation(string infType, string proj, int year,int currentYear)
        {
            // Get project data
            var project = await _context.Projects
                .Where(p => p.ProjectId == proj)
                .Select(p => new
                {
                    p.Inflation,
                    p.StartDate,
                    p.StartFYear,
                    p.FinancialYears
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                throw new InvalidOperationException($"Project {proj} not found.");
            }

            // If inflation is disabled, return 1 (no inflation)
            if (project.Inflation != -1)
            {
                return 1.0;
            }

            // Get inflation rate from settings
            var inflationRateSetting = await _settingsRepository.GetSettingValueByIdAsync(infType);
            
            if (string.IsNullOrEmpty(inflationRateSetting) || !double.TryParse(inflationRateSetting, out double inflationRate))
            {
                throw new InvalidOperationException($"Inflation setting '{infType}' not found or invalid.");
            }

            if (project.FinancialYears == -1)
            {
                // Simple compound inflation based on financial years
              

                int yearGap = year - currentYear;
                if (yearGap < 0) yearGap = 0;

                return Math.Pow(1 + inflationRate / 100, yearGap);
            }
            else
            {
                // Complex calculation with partial year logic
                if (!project.StartFYear.HasValue || !project.StartDate.HasValue)
                {
                    throw new InvalidOperationException($"Project {proj} missing StartFYear or StartDate.");
                }

                var fyearStart = new DateTime((int)project.StartFYear.Value, 4, 1);
                var startDate = project.StartDate.Value.ToDateTime(TimeOnly.MinValue);               

                int yearGap = year - currentYear;
                double percentOfYear = Math.Abs((fyearStart - startDate).TotalDays) / 364.0;

                double inflation;
                double inflation2;

                if (startDate < fyearStart)
                {
                    double inflationAsNumber = 1 + (fnYearGapSign(yearGap - 1) * inflationRate) / 100;
                    inflation = percentOfYear * Math.Pow(inflationAsNumber, Math.Abs(yearGap - 1));

                    double inflationAsNumber2 = 1 + (fnYearGapSign(yearGap) * inflationRate) / 100;
                    inflation2 = (1 - percentOfYear) * Math.Pow(inflationAsNumber2, Math.Abs(yearGap));
                }
                else
                {
                    double inflationAsNumber = 1 + (fnYearGapSign(yearGap) * inflationRate) / 100;
                    inflation = (1 - percentOfYear) * Math.Pow(inflationAsNumber, Math.Abs(yearGap));

                    double inflationAsNumber2 = 1 + (fnYearGapSign(yearGap + 1) * inflationRate) / 100;
                    inflation2 = percentOfYear * Math.Pow(inflationAsNumber2, Math.Abs(yearGap + 1));
                }

                return inflation + inflation2;
            }
        }
    }
}

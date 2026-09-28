using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class ProjectYearRepository : IProjectYearRepository
{
    private readonly CostbookDbContext _context;
    private readonly ISettingsRepository _settingsRepo;
    private readonly IProjectRepository _projectRepo;


    public ProjectYearRepository(CostbookDbContext context, ISettingsRepository settingsRepo, IProjectRepository projectRepo)
    {
        _context = context;
        _settingsRepo = settingsRepo;
        _projectRepo = projectRepo;
    }

    public async Task<IEnumerable<ProjectYear>> GetByProjectAsync(string project)
    {
        var decodedProject = HttpUtility.UrlDecode(project);
        return await _context.ProjectYears
            .AsNoTracking()
            .Where(py => py.Project == decodedProject)
            .OrderBy(py => py.YearValue)
            .ToListAsync();
    }

    public async Task<int?> GetMaxProjectYearAsync(string project)
    {
        var decodedProject = HttpUtility.UrlDecode(project);
        return await _context.ProjectYears
            .AsNoTracking()
            .Where(py => py.Project == decodedProject)
            .MaxAsync(py => (int?)py.YearValue);
    }

    public async Task<ProjectYear> AddProjectYearAsync(string project, int year, ProjectYear yearData)
    {
        var decodedProject = HttpUtility.UrlDecode(project);

        var projectEntity = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == decodedProject);

        var isCommercial = projectEntity?.Programme == "Comm";

        ProjectYear newYear;

        // If rate data is provided from the form, use it directly
        var hasRateData = yearData.MarkupTime.HasValue || yearData.MarkupTests.HasValue
                       || yearData.MarkupAnimals.HasValue || yearData.MarkupAdditional.HasValue
                       || yearData.ProfitTime.HasValue || yearData.ProfitTests.HasValue
                       || yearData.ProfitAnimals.HasValue || yearData.ProfitAdditional.HasValue;

        if (hasRateData)
        {
            newYear = new ProjectYear
            {
                Project = decodedProject,
                YearValue = year,
                MarkupTime = yearData.MarkupTime,
                MarkupTests = yearData.MarkupTests,
                MarkupAnimals = yearData.MarkupAnimals,
                MarkupAdditional = yearData.MarkupAdditional,
                ProfitTime = yearData.ProfitTime,
                ProfitTests = yearData.ProfitTests,
                ProfitAnimals = yearData.ProfitAnimals,
                ProfitAdditional = yearData.ProfitAdditional
            };
        }
        else if (!isCommercial)
        {
            newYear = new ProjectYear { Project = decodedProject, YearValue = year };
        }
        else
        {
            var previousYear = await _context.ProjectYears
                .AsNoTracking()
                .FirstOrDefaultAsync(py => py.Project == decodedProject && py.YearValue == year - 1);

            if (previousYear is null)
            {
                newYear = new ProjectYear
                {
                    Project = decodedProject,
                    YearValue = year,
                    ProfitTime = await GetSettingDoubleAsync("Profitstaff"),
                    ProfitTests = await GetSettingDoubleAsync("Profittests"),
                    ProfitAnimals = await GetSettingDoubleAsync("ProfitAnimals"),
                    ProfitAdditional = await GetSettingDoubleAsync("ProfitExceptional"),
                    MarkupTime = await GetSettingDoubleAsync("Markupstaff"),
                    MarkupTests = await GetSettingDoubleAsync("Markuptests"),
                    MarkupAnimals = await GetSettingDoubleAsync("MarkupAnimals"),
                    MarkupAdditional = await GetSettingDoubleAsync("MarkupExceptional")
                };
            }
            else
            {
                newYear = new ProjectYear
                {
                    Project = decodedProject,
                    YearValue = year,
                    ProfitTime = previousYear.ProfitTime ?? await GetSettingDoubleAsync("Profitstaff"),
                    ProfitTests = previousYear.ProfitTests ?? await GetSettingDoubleAsync("Profittests"),
                    ProfitAnimals = previousYear.ProfitAnimals ?? await GetSettingDoubleAsync("ProfitAnimals"),
                    ProfitAdditional = previousYear.ProfitAdditional ?? await GetSettingDoubleAsync("ProfitExceptional"),
                    MarkupTime = previousYear.MarkupTime ?? await GetSettingDoubleAsync("Markupstaff"),
                    MarkupTests = previousYear.MarkupTests ?? await GetSettingDoubleAsync("Markuptests"),
                    MarkupAnimals = previousYear.MarkupAnimals ?? await GetSettingDoubleAsync("MarkupAnimals"),
                    MarkupAdditional = previousYear.MarkupAdditional ?? await GetSettingDoubleAsync("MarkupExceptional")
                };
            }
        }

        _context.ProjectYears.Add(newYear);
        await _context.SaveChangesAsync();
        return newYear;
    }

    public async Task<ProjectYear> UpdateProjectYearAsync(ProjectYear projectYear)
    {
        projectYear.Project = HttpUtility.UrlDecode(projectYear.Project);
        _context.ProjectYears.Update(projectYear);
        await _context.SaveChangesAsync();
        return projectYear;
    }

    public async Task<(bool Deleted, IReadOnlyList<string> Errors)> DeleteProjectYearAsync(string project, int year)
    {
        var decodedProject = HttpUtility.UrlDecode(project);

        var validationErrors = await GetChildValidationErrorsAsync(decodedProject, year);
        if (validationErrors.Count > 0)
            return (false, validationErrors);

        var entity = await _context.ProjectYears
            .FirstOrDefaultAsync(py => py.Project == decodedProject && py.YearValue == year);
        if (entity is null)
            return (false, Array.Empty<string>());
        _context.ProjectYears.Remove(entity);
        await _context.SaveChangesAsync();
        return (true, Array.Empty<string>());
    }

    public async Task<(bool Copied, IReadOnlyList<string> Errors)> CopyYearDataAsync(string project, int sourceYear, int targetYear)
    {
       
        var decodedProject = HttpUtility.UrlDecode(project);

        if (sourceYear <= 0 || targetYear <= 0)
            return (false, new List<string> { "Source year and target year must be valid." });

        if (sourceYear == targetYear)
            return (false, new List<string> { "Source year and target year cannot be the same." });

        var sourceProjectYear = await _context.ProjectYears
            .AsNoTracking()
            .FirstOrDefaultAsync(py => py.Project == decodedProject && py.YearValue == sourceYear);
        if (sourceProjectYear is null)
            return (false, new List<string> { "Source year does not exist for the selected project." });

        var targetProjectYearExists = await _context.ProjectYears
            .AsNoTracking()
            .AnyAsync(py => py.Project == decodedProject && py.YearValue == targetYear);
        if (!targetProjectYearExists)
            return (false, new List<string> { "Target year does not exist for the selected project." });

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var targetStaff = await _context.StaffRequirements
                    .Where(s => s.Project == decodedProject && s.Year == targetYear)
                    .ToListAsync();
                if (targetStaff.Count > 0)
                    _context.StaffRequirements.RemoveRange(targetStaff);

                var targetTests = await _context.TestRequirements
                    .Where(t => t.Project == decodedProject && t.Year == targetYear)
                    .ToListAsync();
                if (targetTests.Count > 0)
                    _context.TestRequirements.RemoveRange(targetTests);

                var targetAnimals = await _context.AnimalRequirements
                    .Where(a => a.Project == decodedProject && a.Year == targetYear)
                    .ToListAsync();
                if (targetAnimals.Count > 0)
                    _context.AnimalRequirements.RemoveRange(targetAnimals);

                var targetAdditionalCosts = await _context.AdditionalCosts
                    .Where(a => a.Project == decodedProject && a.Year == targetYear)
                    .ToListAsync();
                if (targetAdditionalCosts.Count > 0)
                    _context.AdditionalCosts.RemoveRange(targetAdditionalCosts);

                var sourceStaff = await _context.StaffRequirements
                    .AsNoTracking()
                    .Where(s => s.Project == decodedProject && s.Year == sourceYear)
                    .ToListAsync();
                foreach (var staff in sourceStaff)
                {
                    _context.StaffRequirements.Add(new StaffRequirement
                    {
                        Project = decodedProject,
                        Year = targetYear,
                        WgGrade = staff.WgGrade,
                        Name = staff.Name,
                        Nohours = staff.Nohours,
                        Nodays = staff.Nodays,
                        Chargerate = staff.Chargerate,
                        Payrate = staff.Payrate,
                        Npr = staff.Npr,
                        Ohr = staff.Ohr
                    });
                }

                var sourceTests = await _context.TestRequirements
                    .AsNoTracking()
                    .Where(t => t.Project == decodedProject && t.Year == sourceYear)
                    .ToListAsync();
                foreach (var test in sourceTests)
                {
                    _context.TestRequirements.Add(new TestRequirement
                    {
                        Project = decodedProject,
                        Year = targetYear,
                        TestCode = test.TestCode,
                        NumberOfTests = test.NumberOfTests,
                        UnitPrice = test.UnitPrice
                    });
                }

                var sourceAnimals = await _context.AnimalRequirements
                    .AsNoTracking()
                    .Where(a => a.Project == decodedProject && a.Year == sourceYear)
                    .ToListAsync();
                foreach (var animal in sourceAnimals)
                {
                    _context.AnimalRequirements.Add(new AnimalRequirement
                    {
                        Project = decodedProject,
                        Year = targetYear,
                        AnimalType = animal.AnimalType,
                        NumberOfDays = animal.NumberOfDays,
                        NumberOfAnimals = animal.NumberOfAnimals,
                        DailyRate = animal.DailyRate
                    });
                }

                var sourceAdditionalCosts = await _context.AdditionalCosts
                    .AsNoTracking()
                    .Where(a => a.Project == decodedProject && a.Year == sourceYear)
                    .ToListAsync();
                foreach (var additionalCost in sourceAdditionalCosts)
                {
                    _context.AdditionalCosts.Add(new AdditionalCost
                    {
                        Project = decodedProject,
                        Year = targetYear,
                        AccountCat = additionalCost.AccountCat,
                        Description = additionalCost.Description,
                        ItemCost = additionalCost.ItemCost,
                        CostEntered = additionalCost.CostEntered,
                        Freq = additionalCost.Freq
                    });
                }

                // Persist copied target-year rows first, then recost from tracked DB rows
                await _context.SaveChangesAsync();

                // Get CurrentYear setting for inflation calculations
                var currentYearSetting = await _settingsRepo.GetSettingValueByIdAsync("CurrentYear");
                if (string.IsNullOrEmpty(currentYearSetting) || !int.TryParse(currentYearSetting, out int fyear))
                {
                    throw new InvalidOperationException("CurrentYear setting not found or invalid in settings table.");
                }

                // Determine if project is DEFRA
                bool isDefraProject = (await _context.Projects
                    .Where(p => p.ProjectId == decodedProject)
                    .Select(p => (int?)p.IsDefraProject)
                    .FirstOrDefaultAsync() ?? 0) != 0;

                // Recost target-year data
                var projectRepo = (ProjectRepository)_projectRepo;

                var testInflationFactor = await projectRepo.fnInflation("InflationTests", decodedProject, targetYear, fyear);
                var animalInflationFactor = await projectRepo.fnInflation("InflationAnimals", decodedProject, targetYear, fyear);
                var additionalInflationFactor = await projectRepo.fnInflation("InflationExceptional", decodedProject, targetYear, fyear);
                var staffInflationFactor = await projectRepo.fnInflation("InflationStaff", decodedProject, targetYear, fyear);

                #region RecostTests
                var testRecords = await _context.TestRequirements
                    .Where(r => r.Project == decodedProject && r.Year == targetYear)
                    .ToListAsync();

                var testData = await _context.FpsTestorProducts
                    .ToDictionaryAsync(t => t.ItemCode);

                foreach (var rec in testRecords)
                {
                    if (!testData.TryGetValue(rec.TestCode, out var test))
                        continue;

                    decimal basePriceDecimal = isDefraProject
                        ? test.DefraUnitPrice
                        : test.UnitPriceVla.GetValueOrDefault(0);

                    double basePrice = (double)basePriceDecimal;
                    rec.UnitPrice = basePrice * testInflationFactor;
                }

                _context.TestRequirements.UpdateRange(testRecords);
                #endregion

                #region RecostAnimals
                var animalRecords = await _context.AnimalRequirements
                    .Where(ar => ar.Project == decodedProject && ar.Year == targetYear)
                    .ToListAsync();

                var animalData = await _context.FpsAnimals
                    .ToDictionaryAsync(a => a.AnimalType);

                foreach (var rec in animalRecords)
                {
                    if (!animalData.TryGetValue(rec.AnimalType, out var animal))
                        continue;

                    decimal? baseRateDecimal = isDefraProject
                        ? animal.DefraDailyRate
                        : animal.DailyRate;

                    double baseRate = (double)(baseRateDecimal ?? 0);

                    rec.DailyRate = baseRate * animalInflationFactor;
                }

                _context.AnimalRequirements.UpdateRange(animalRecords);
                #endregion

                #region RecostAdditionalCosts
                var additionalCostRecords = await _context.AdditionalCosts
                    .Where(ac => ac.Project == decodedProject && ac.Year == targetYear)
                    .ToListAsync();

                foreach (var rec in additionalCostRecords)
                {
                    double inflatedCost;

                    if (await projectRepo.fnUseInflation(rec.AccountCat))
                    {
                        inflatedCost = rec.CostEntered * additionalInflationFactor;
                    }
                    else
                    {
                        inflatedCost = rec.CostEntered;
                    }

                    rec.ItemCost = inflatedCost;
                }

                _context.AdditionalCosts.UpdateRange(additionalCostRecords);
                #endregion

                #region RecostStaff
                var staffRecords = await _context.StaffRequirements
                    .Where(sr => sr.Project == decodedProject && sr.Year == targetYear)
                    .ToListAsync();

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

                payRatesQuery = payRatesQuery.Where(x => x.ChargeRate != null && x.ChargeRate != 0);

                var payRatesData = await payRatesQuery.ToDictionaryAsync(x => x.WgGrade);

                foreach (var rec in staffRecords)
                {
                    if (!payRatesData.TryGetValue(rec.WgGrade, out var payRate))
                        continue;

                    rec.Chargerate = (double)(payRate.ChargeRate ?? 0) * staffInflationFactor;
                    rec.Payrate = (double)(payRate.PayRate ?? 0) * staffInflationFactor;
                    rec.Npr = (double)(payRate.Npr ?? 0) * staffInflationFactor;
                    rec.Ohr = (double)(payRate.Ohr ?? 0) * staffInflationFactor;
                }

                _context.StaffRequirements.UpdateRange(staffRecords);
                #endregion

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, (IReadOnlyList<string>)Array.Empty<string>());
        });
    }

    private async Task<List<string>> GetChildValidationErrorsAsync(string project, int year)
    {
        var errors = new List<string>();

        await CheckChildRecordsAsync(_context.StaffRequirements, s => s.Project == project && s.Year == year, "staff requirements", errors);
        await CheckChildRecordsAsync(_context.TestRequirements, t => t.Project == project && t.Year == year, "test requirements", errors);
        await CheckChildRecordsAsync(_context.AnimalRequirements, a => a.Project == project && a.Year == year, "animal requirements", errors);
        await CheckChildRecordsAsync(_context.AdditionalCosts, ac => ac.Project == project && ac.Year == year, "additional costs", errors);

        return errors;
    }

    private static async Task CheckChildRecordsAsync<T>(
        IQueryable<T> dbSet,
        Expression<Func<T, bool>> predicate,
        string label,
        List<string> errors) where T : class
    {
        var count = await dbSet.AsNoTracking().CountAsync(predicate);
        if (count > 0)
            errors.Add($"Year has {count} {label}. Remove them first.");
    }

    private async Task<double?> GetSettingDoubleAsync(string key)
    {
        var val = await _settingsRepo.GetSettingValueByIdAsync(key);
        return double.TryParse(val, out var d) ? d : null;
    }
}

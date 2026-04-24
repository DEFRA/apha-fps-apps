using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class ProjectYearRepository : IProjectYearRepository
{
    private readonly CostbookDbContext _context;
    private readonly ISettingsRepository _settingsRepo;

    public ProjectYearRepository(CostbookDbContext context, ISettingsRepository settingsRepo)
    {
        _context = context;
        _settingsRepo = settingsRepo;
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

    public async Task<ProjectYear> AddProjectYearAsync(string project, int year)
    {
        var decodedProject = HttpUtility.UrlDecode(project);

        var projectEntity = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == decodedProject);

        var isCommercial = projectEntity?.Programme == "Comm";

        ProjectYear newYear;

        if (!isCommercial)
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
        _context.ProjectYears.Update(projectYear);
        await _context.SaveChangesAsync();
        return projectYear;
    }

    public async Task<IEnumerable<PayRateLookup>> GetPayRatesAsync(bool isDefra)
    {
        // qryPayRates_NonDefra / qryPayRates_Defra:
        // SELECT WorkGroupGrade.WGGrade, tblPCGrades.ChargeRate, tblPCGrades.PayRate, tblPCGrades.NPR, tblPCGrades.OHR
        // FROM tblPCGrades INNER JOIN WorkGroupGrade ON tblPCGrades.PCGrade = WorkGroupGrade.ProfitCentreGrade
        // WHERE tblPCGrades.ChargeRate <> 0
        // Defra variant uses DefraChargeRate; NonDefra uses ChargeRate
        var rows = await _context.WorkGroupGrades
            .AsNoTracking()
            .Join(
                _context.ProfitCentreGrades.AsNoTracking(),
                wg => new { wg.ProfitCentreGrade, wg.FpsYear },
                pc => new { ProfitCentreGrade = pc.PcGrade, pc.FpsYear },
                (wg, pc) => new { wg.WgGrade, pc.ChargeRate, pc.DefraChargeRate, pc.PayRate, pc.Npr, pc.Ohr })
            .Where(x => isDefra ? x.DefraChargeRate != 0 : x.ChargeRate != 0)
            .ToListAsync();

        return rows.Select(x => new PayRateLookup(
            x.WgGrade,
            isDefra ? (double?)x.DefraChargeRate : (double?)x.ChargeRate,
            (double?)x.PayRate,
            (double?)x.Npr,
            (double?)x.Ohr));
    }

    private async Task<double?> GetSettingDoubleAsync(string key)
    {
        var val = await _settingsRepo.GetSettingValueByIdAsync(key);
        return double.TryParse(val, out var d) ? d : null;
    }
}

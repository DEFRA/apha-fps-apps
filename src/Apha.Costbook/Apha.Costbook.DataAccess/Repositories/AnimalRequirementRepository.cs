using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class AnimalRequirementRepository : RepositoryBase, IAnimalRequirementRepository
{
    private readonly ISettingsRepository _settingsRepo;
    private readonly IProjectRepository _projectRepo;

    public AnimalRequirementRepository(CostbookDbContext context, ISettingsRepository settingsRepo, IProjectRepository projectRepo)
        : base(context)
    {
        _settingsRepo = settingsRepo;
        _projectRepo = projectRepo;
    }

    public async Task<PagedData<AnimalRequirementDetailView>> GetAnimalRequirementsByProjectYearAsync(
        string project, int year, PaginationParameters<string> query)
    {
        var decodedProject = HttpUtility.UrlDecode(project);

        var baseQuery = _context.AnimalRequirements
            .AsNoTracking()
            .Where(ar => ar.Project == decodedProject && ar.Year == year)
            .GroupJoin(
                _context.Projects.AsNoTracking(),
                ar => ar.Project,
                p => p.ProjectId,
                (ar, projJoin) => new { ar, projJoin })
            .SelectMany(
                x => x.projJoin.DefaultIfEmpty(),
                (x, p) => new AnimalRequirementDetailView
                {
                    ArIdentity = x.ar.ArIdentity,
                    Project = x.ar.Project,
                    Year = x.ar.Year,
                    AnimalType = x.ar.AnimalType,
                    NumberOfDays = x.ar.NumberOfDays,
                    NumberOfAnimals = x.ar.NumberOfAnimals,
                    DailyRate = x.ar.DailyRate,
                    AnimalCost = x.ar.NumberOfDays * x.ar.NumberOfAnimals * x.ar.DailyRate,
                    Programme = p != null ? p.Programme : null,
                    EuroConvRate = p != null ? p.Euroconvrate : null
                })
            .Distinct();

        baseQuery = ApplySorting(baseQuery, query.SortBy, query.Descending);        
        return await ApplyPaging(baseQuery, query.Page, query.PageSize);
    }

    public async Task<AnimalRequirement> AddAnimalRequirementAsync(AnimalRequirement animalRequirement)
    {
        animalRequirement.Project = HttpUtility.UrlDecode(animalRequirement.Project);
        _context.AnimalRequirements.Add(animalRequirement);
        await _context.SaveChangesAsync();

        return animalRequirement;
    }

    public async Task<AnimalRequirement> UpdateAnimalRequirementAsync(AnimalRequirement animalRequirement)
    {
        animalRequirement.Project = HttpUtility.UrlDecode(animalRequirement.Project);

        _context.AnimalRequirements.Update(animalRequirement);
        await _context.SaveChangesAsync();

        return animalRequirement;
    }

    public async Task<bool> DeleteAnimalRequirementAsync(int arIdentity)
    {
        var deleted = await _context.AnimalRequirements
            .Where(a => a.ArIdentity == arIdentity)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }

    public async Task<IEnumerable<AnimalRateLookup>> GetAnimalRatesAsync(string projectId, int year, bool isDefra)
    {
        var decodedId = HttpUtility.UrlDecode(projectId);

        var currentYearSetting = await _settingsRepo.GetSettingValueByIdAsync("CurrentYear");

        if (string.IsNullOrEmpty(currentYearSetting) || !int.TryParse(currentYearSetting, out int fyear))
        {
            throw new InvalidOperationException("CurrentYear setting not found or invalid in settings table.");
        }

        var results = await _context.FpsAnimals
            .AsNoTracking()
            .OrderBy(a => a.AnimalType)
            .Select(a => new
            {
                a.AnimalType,
                Rate = isDefra ? a.DefraDailyRate : a.DailyRate
            })
            .ToListAsync();

        double inflationFactor = await _projectRepo.GetInflationFactorAsync("InflationAnimals", decodedId, year, fyear);

        return results.Select(a =>
        {
            var baseRate = (double?)Convert.ToDouble(a.Rate);
            return new AnimalRateLookup
            {
                AnimalType = a.AnimalType,
                DailyRate = a.Rate,
                DailyRateWithInflamation = a.Rate.HasValue
                    ? (decimal?)(Convert.ToDouble(a.Rate.Value) * inflationFactor)
                    : null
            };
        });
    }

    public async Task<IEnumerable<FpsAnimals>> GetAllAnimalsAsync()
    {
        var result = await _context.FpsAnimals
                .AsNoTracking()
                .OrderBy(a => a.AnimalType)
                .ToListAsync();

        return result;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static IQueryable<AnimalRequirementDetailView> ApplySorting(
        IQueryable<AnimalRequirementDetailView> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderBy(a => a.AnimalType);

        return sortBy.ToLower() switch
        {
            "aridentity"     => ApplyOrder(query, a => a.ArIdentity, descending),
            "project"        => ApplyOrder(query, a => a.Project, descending),
            "year"           => ApplyOrder(query, a => a.Year, descending),
            "animaltype"     => ApplyOrder(query, a => a.AnimalType, descending),
            "numberofdays"   => ApplyOrder(query, a => a.NumberOfDays, descending),
            "numberofanimals"=> ApplyOrder(query, a => a.NumberOfAnimals, descending),
            "dailyrate"      => ApplyOrder(query, a => a.DailyRate, descending),
            "animalcost"     => ApplyOrder(query, a => a.AnimalCost, descending),
            _                => query.OrderBy(a => a.AnimalType)
        };
    }

    private static IQueryable<AnimalRequirementDetailView> ApplyOrder<TKey>(
        IQueryable<AnimalRequirementDetailView> query,
        System.Linq.Expressions.Expression<Func<AnimalRequirementDetailView, TKey>> keySelector,
        bool descending)
    {
        return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}

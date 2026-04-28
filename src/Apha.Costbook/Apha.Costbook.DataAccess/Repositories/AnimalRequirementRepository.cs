using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class AnimalRequirementRepository : IAnimalRequirementRepository
{
    private readonly CostbookDbContext _context;

    public AnimalRequirementRepository(CostbookDbContext context) => _context = context;

    public async Task<IEnumerable<AnimalRequirementDetailView>> GetAnimalRequirementsByProjectYearAsync(string project, int year)
    {
        var decodedProject = HttpUtility.UrlDecode(project);

        return await _context.AnimalRequirements
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
            .Distinct()
            .OrderBy(a => a.AnimalType)
            .ToListAsync();
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

    public async Task<IEnumerable<AnimalRateLookup>> GetAnimalRatesAsync(bool isDefra)
    {
        var results = await _context.FpsAnimals
            .AsNoTracking()
            .OrderBy(a => a.AnimalType)
            .Select(a => new
            {
                a.AnimalType,
                Rate = isDefra ? a.DefraDailyRate : a.DailyRate
            })
            .ToListAsync();

        return results.Select(a => new AnimalRateLookup(a.AnimalType, (double?)a.Rate));
    }

    public async Task<IEnumerable<FpsAnimals>> GetAllAnimalsAsync()
    {
        var result = await _context.FpsAnimals
                .AsNoTracking()
                .OrderBy(a => a.AnimalType)
                .ToListAsync();

        return result;
    }
}

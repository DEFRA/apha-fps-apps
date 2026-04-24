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

    public async Task<IEnumerable<AnimalRequirement>> GetByProjectYearAsync(string project, int year)
    {
        var decodedProject = HttpUtility.UrlDecode(project);
        return await _context.AnimalRequirements
            .AsNoTracking()
            .Where(a => a.Project == decodedProject && a.Year == year)
            .OrderBy(a => a.AnimalType)
            .ToListAsync();
    }

    public async Task<AnimalRequirement> AddAsync(AnimalRequirement animalRequirement)
    {
        _context.AnimalRequirements.Add(animalRequirement);
        await _context.SaveChangesAsync();
        return animalRequirement;
    }

    public async Task<AnimalRequirement> UpdateAsync(AnimalRequirement animalRequirement)
    {
        _context.AnimalRequirements.Update(animalRequirement);
        await _context.SaveChangesAsync();
        return animalRequirement;
    }

    public async Task<bool> DeleteAsync(int arIdentity)
    {
        var deleted = await _context.AnimalRequirements
            .Where(a => a.ArIdentity == arIdentity)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }

    public async Task<IEnumerable<AnimalRateLookup>> GetAnimalRatesAsync(bool isDefra)
        => await _context.FpsAnimals
            .AsNoTracking()
            .OrderBy(a => a.AnimalType)
            .Select(a => new AnimalRateLookup(
                a.AnimalType,
                isDefra ? (double?)a.DefraDailyRate : (double?)a.DailyRate))
            .ToListAsync();
}

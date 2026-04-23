using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Apha.Costbook.DataAccess.Repositories;

public class StaffRequirementRepository : IStaffRequirementRepository
{
    private readonly CostbookDbContext _context;

    public StaffRequirementRepository(CostbookDbContext context) => _context = context;

    public async Task<IEnumerable<StaffRequirement>> GetByProjectYearAsync(string project, int year)
        => await _context.StaffRequirements
            .AsNoTracking()
            .Where(s => s.Project == project && s.Year == year)
            .OrderBy(s => s.WgGrade)
            .ToListAsync();

    public async Task<StaffRequirement> AddAsync(StaffRequirement staffRequirement)
    {
        _context.StaffRequirements.Add(staffRequirement);
        await _context.SaveChangesAsync();
        return staffRequirement;
    }

    public async Task<StaffRequirement> UpdateAsync(StaffRequirement staffRequirement)
    {
        _context.StaffRequirements.Update(staffRequirement);
        await _context.SaveChangesAsync();
        return staffRequirement;
    }

    public async Task<bool> DeleteAsync(int srIdentity)
    {
        var deleted = await _context.StaffRequirements
            .Where(s => s.SrIdentity == srIdentity)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }
}

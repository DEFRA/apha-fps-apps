using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ProposedProjectRepository : IProposedProjectRepository
    {
        private readonly PimsDbContext _context;

        public ProposedProjectRepository(PimsDbContext context)
        {
            _context = context;
        }

        public async Task<Project?> GetFpsProjectByIdAsync(string parentproject)
        {
            return await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Parentproject == parentproject);
        }

        public async Task<ProposedProject?> GetProposedProjectByIdAsync(string parentproject)
        {
            return await _context.ProposedProjects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Parentproject == parentproject);
        }

        public async Task<ProposedProject> AddProposedProjectAsync(ProposedProject entity)
        {
            _context.ProposedProjects.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<List<string>> GetProjectProgramsAsync()
        {
            return await _context.RadtrackProgs
                .AsNoTracking()
                .Select(p => p.Program)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task<List<string>> GetProjectCustomersAsync()
        {
            return await _context.ProjectLatestDetails
                .AsNoTracking()
                .Where(p => p.Customer != null)
                .GroupBy(p => p.Customer)
                .Select(g => g.Key!)
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<ProjectStatus>> GetProjectStatusesAsync()
        {
            return await _context.ProjectStatuses
                .AsNoTracking()
                .Where(s => s.IsPims && s.Projectstatus != "Completed")
                .ToListAsync();
        }
    }
}

using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class DiseaseRepository : IDiseaseRepository
    {
        private readonly FpsDbContext _dbContext;

        public DiseaseRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Disease>> GetAllDiseasesAsync()
        {
            return await _dbContext.Diseases
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Disease> AddAsync(Disease disease)
        {
            _dbContext.Diseases.Add(disease);
            await _dbContext.SaveChangesAsync();
            return disease;
        }

        public async Task<bool> DeleteAsync(string diseaseName)
        {
            var entity = await _dbContext.Diseases.FirstOrDefaultAsync(d => d.DiseaseName == diseaseName);
            if (entity == null)
            {
                return false;
            }

            _dbContext.Diseases.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string diseaseName)
        {
            return await _dbContext.Diseases.AnyAsync(d => d.DiseaseName == diseaseName);
        }
    }
}

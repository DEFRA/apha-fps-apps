using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.Core.Entities;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProjectRepository : IProjectRepository
    {       
        private readonly FpsDbContext _dbContext;
        private readonly IProgramRepository _programRepository;
        public ProjectRepository(FpsDbContext dbContext, IProgramRepository programRepository)
        {           
            _dbContext = dbContext;
            _programRepository = programRepository;
        }

        public IQueryable<Project> Get()
        {
            var programs = _programRepository.Get();           

            return (from p in _dbContext.Projects
                          join ap in programs
                            on p.Program equals ap.ProgramNo                          
                          select p).AsQueryable();

        }
    }
}

using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Entities;
using Apha.FPS.DataAccess.Data;


namespace Apha.FPS.DataAccess.Repositories
{
    public class ProgramRepository : IProgramRepository
    {       
        private readonly FpsDbContext _dbContext;
        public ProgramRepository(FpsDbContext dbContext)
        {          
            _dbContext = dbContext;
        }

        public IQueryable<Program> Get()
        {           
            var data = (from p in _dbContext.Programs
                    join up in _dbContext.UserPrograms
                        on p.ProgramNo equals up.ProgramNo                       
                    join u in _dbContext.Users
                        on up.UserID equals u.UserId
                    where u.Username == "dbo"
                        select p).AsQueryable();

            return data;
        }
    }
}

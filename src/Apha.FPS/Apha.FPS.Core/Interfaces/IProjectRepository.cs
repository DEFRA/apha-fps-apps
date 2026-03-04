using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectRepository
    {       
        IQueryable<Project> Get();
    }
}

using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProgramRepository
    {
        IQueryable<Program> Get();
        Task<IEnumerable<Program>> GetAllProgramsAsync();
    }
}

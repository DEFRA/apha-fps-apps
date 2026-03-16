using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProgramRepository
    {
        IQueryable<Program> Get();
        Task<IEnumerable<Program>> GetAllProgramsAsync();
        Task<PagedData<Program>> GetAllProgramsAsync(PaginationParameters<string> query);
        Task<List<string?>> GetAllDirectoratesAsync();
        Task<Program?> GetProgramByIdAsync(string id);       
        Task<Program> AddProgramAsync(Program entity);
        Task<Program> UpdateProgramAsync(Program entity);
        Task<bool> DeleteProgramAsync(string id);
    }
}

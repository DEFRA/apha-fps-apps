using Apha.Costbook.Application.Dtos;

namespace Apha.Costbook.Application.Interfaces
{
    public interface IProgramService
    {
        Task<List<ProgramDto>> GetAllProgramsAsync();
    }
}

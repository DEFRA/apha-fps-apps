using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProgramService
    {
        Task<IEnumerable<ProgramDto>> GetAllProgramsAsync();
    }
}

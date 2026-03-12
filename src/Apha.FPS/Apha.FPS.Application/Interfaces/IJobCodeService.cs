using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IJobCodeService
    {
        Task<IEnumerable<JobCodeDto>> GetJobCodeListAsync();
    }
}

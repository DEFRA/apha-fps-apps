using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IJobCodeRepository
    {
        Task<IEnumerable<JobCode>> GetAllJobCodesAsync();
        Task<IEnumerable<JobCode>> GetZtJobCodesAsync();
    }
}

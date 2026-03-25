using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IStoredProcRepository
    {
        Task<IEnumerable<CostCentreWorkgroup>> GetAllCostCentreWorkgroupAsync();
    }
}

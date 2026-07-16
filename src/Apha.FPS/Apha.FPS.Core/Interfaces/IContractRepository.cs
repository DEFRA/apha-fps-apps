using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllContractsAsync();
        Task<IEnumerable<Contract>> GetAllContractsByUserAsync();
        Task<IEnumerable<Contract>> GetAllPactContractsAsync();
    }
}

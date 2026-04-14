using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces
{
    public interface ITestorProductRepository
    {
        Task<IEnumerable<TestorProduct>> GetAllAsync();
    }
}

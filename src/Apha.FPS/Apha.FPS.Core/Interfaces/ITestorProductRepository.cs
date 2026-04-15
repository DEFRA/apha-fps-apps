using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface ITestorProductRepository
    {
        Task<IEnumerable<TestorProduct>> GetAllTestorProductsAsync();
    }
}

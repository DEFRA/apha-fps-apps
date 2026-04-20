using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface ITestorProductService
    {
        Task<IEnumerable<TestorProductDto>> GetAllTestorProductsAsync();
    }
}

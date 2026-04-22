using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class TestorProductService : ITestorProductService
    {
        private readonly ITestorProductRepository _testorProductRepository;
        private readonly IMapper _mapper;
        public TestorProductService(ITestorProductRepository testorProductRepository, IMapper mapper)
        {
            _testorProductRepository = testorProductRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TestorProductDto>> GetAllTestorProductsAsync()
        {
            var items = await _testorProductRepository.GetAllTestorProductsAsync();
            return _mapper.Map<IEnumerable<TestorProductDto>>(items);

        
        }
    }
}

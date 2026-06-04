using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class TestSupplierService : ITestSupplierService
    {
        private readonly ITestSupplierRepository _repository;
        private readonly IMapper _mapper;

        public TestSupplierService(ITestSupplierRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<TestSupplierViewDto>> GetPagedAsync(
            QueryParameters<string> query, string testCode, bool showRejected)
        {
            ArgumentNullException.ThrowIfNull(query);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetPagedByTestCodeAsync(parameters, testCode, showRejected);
            return _mapper.Map<PaginatedResult<TestSupplierViewDto>>(result);
        }
    }
}

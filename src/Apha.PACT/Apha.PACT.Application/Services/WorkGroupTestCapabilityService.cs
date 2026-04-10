using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class WorkGroupTestCapabilityService : IWorkGroupTestCapabilityService
    {
        private readonly ITestCapabilityRepository _testCapabilityRepository;
        private readonly ITestRequirementRepository _testReqmtRepository;
        private readonly ITestorProductRepository _testorProductRepository;
        private readonly IMapper _mapper;

        public WorkGroupTestCapabilityService(
            ITestCapabilityRepository testCapabilityRepository,
            ITestRequirementRepository testReqmtRepository,
            ITestorProductRepository testorProductRepository,
            IMapper mapper)
        {
            _testCapabilityRepository = testCapabilityRepository;
            _testReqmtRepository = testReqmtRepository;
            _testorProductRepository = testorProductRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<TestCapabilityDto>> GetPagedByWorkGroupAsync(QueryParameters<string> query, string? workGroup)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testCapabilityRepository.GetPagedByWorkGroupAsync(parameters, workGroup);
            return _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData);
        }

        public async Task<PaginatedResult<TestCapabilityDto>> GetPagedByTestCodeAsync(QueryParameters<string> query, string? testCode)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testCapabilityRepository.GetPagedByTestCodeAsync(parameters, testCode);
            return _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData);
        }

        public async Task<TestCapabilityDto?> GetTestCapabilityByIdAsync(string testCode, string workGroup)
        {
            var entity = await _testCapabilityRepository.GetByIdAsync(testCode, workGroup);
            return entity is null ? null : _mapper.Map<TestCapabilityDto>(entity);
        }

        public async Task<TestCapabilityDto> AddTestCapabilityAsync(TestCapabilityDto dto)
        {
            var existing = await _testCapabilityRepository.GetByIdAsync(dto.TestCode, dto.WorkGroup);
            if (existing is not null)
                throw new InvalidOperationException(
                    $"A Test Capability record with TestCode '{dto.TestCode}' and WorkGroup '{dto.WorkGroup}' already exists.");

            var entity = _mapper.Map<TestCapability>(dto);
            var created = await _testCapabilityRepository.AddAsync(entity);
            return _mapper.Map<TestCapabilityDto>(created);
        }

        public async Task<TestCapabilityDto> UpdateTestCapabilityAsync(TestCapabilityDto dto)
        {
            var existing = await _testCapabilityRepository.GetByIdAsync(dto.TestCode, dto.WorkGroup);
            if (existing is null)
                throw new KeyNotFoundException(
                    $"A Test Capability record with TestCode '{dto.TestCode}' and WorkGroup '{dto.WorkGroup}' was not found.");

            var hasReqmts = await _testReqmtRepository.ExistsByTestBuyerCodeAsync(dto.TestCode + dto.WorkGroup);
            if (hasReqmts)
                throw new InvalidOperationException("Cannot update, test requirements are dependant on this.");

            var entity = _mapper.Map<TestCapability>(dto);
            var updated = await _testCapabilityRepository.UpdateAsync(entity);
            return _mapper.Map<TestCapabilityDto>(updated);
        }

        public async Task<bool> DeleteTestCapabilityAsync(string testCode, string workGroup)
        {
            var hasReqmts = await _testReqmtRepository.ExistsByTestBuyerCodeAsync(testCode + workGroup);
            if (hasReqmts)
                throw new InvalidOperationException("Cannot delete, test requirements are dependant on this.");

            return await _testCapabilityRepository.DeleteAsync(testCode, workGroup);
        }

        public async Task<IEnumerable<TestorProductDto>> GetAllTestorProductsAsync()
        {
            var items = await _testorProductRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TestorProductDto>>(items);
        }
    }
}

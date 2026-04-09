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
        private readonly ITestReqmtRepository _testReqmtRepository;
        private readonly ITestorProductRepository _testorProductRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public WorkGroupTestCapabilityService(
            ITestCapabilityRepository testCapabilityRepository,
            ITestReqmtRepository testReqmtRepository,
            ITestorProductRepository testorProductRepository,
            IProjectRepository projectRepository,
            IMapper mapper)
        {
            _testCapabilityRepository = testCapabilityRepository;
            _testReqmtRepository = testReqmtRepository;
            _testorProductRepository = testorProductRepository;
            _projectRepository = projectRepository;
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

        public async Task<PaginatedResult<TestReqmtDto>> GetPagedTestReqmtAsync(QueryParameters<string> query, string testCode)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testReqmtRepository.GetPagedWithDetailsAsync(parameters, testCode);
            var dtos = _mapper.Map<List<TestReqmtDto>>(pagedData.Data);
            var paginationDto = _mapper.Map<PaginationDto>(pagedData.PaginationData);
            return new PaginatedResult<TestReqmtDto>(dtos, paginationDto);
        }

        public async Task<IEnumerable<TestReqmtDto>> GetAllTestReqmtForExportAsync(string testCode, string? filterJson)
        {
            var items = await _testReqmtRepository.GetAllForExportAsync(testCode, filterJson);
            return _mapper.Map<IEnumerable<TestReqmtDto>>(items);
        }

        public async Task<TestReqmtDto?> GetTestReqmtByIdAsync(string testCode, string buyer)
        {
            var detail = await _testReqmtRepository.GetDetailByIdAsync(testCode, buyer);
            return detail is null ? null : _mapper.Map<TestReqmtDto>(detail);
        }

        public async Task<TestReqmtDto?> GetTestReqmtPricingAsync(string testCode, string? projectCode = null)
        {
            var detail = await _testReqmtRepository.GetPricingAsync(testCode, projectCode);
            return detail is null ? null : _mapper.Map<TestReqmtDto>(detail);
        }

        public async Task<TestReqmtDto> AddTestReqmtAsync(TestReqmtDto dto)
        {
            // ITrig: both fields null
            if (string.IsNullOrWhiteSpace(dto.ProjectBuyerCode) && string.IsNullOrWhiteSpace(dto.TestBuyerCode))
                throw new InvalidOperationException("Must fill in Project Buyer or Test Buyer");

            // ITrig: project must exist when ProjectBuyerCode is provided
            if (!string.IsNullOrWhiteSpace(dto.ProjectBuyerCode))
            {
                var projectExists = await _projectRepository.ExistsAsync(dto.ProjectBuyerCode);
                if (!projectExists)
                    throw new InvalidOperationException("Not a valid project.");
            }

            // ITrig: TestCapability must exist when TestBuyerCode is provided
            if (!string.IsNullOrWhiteSpace(dto.TestBuyerCode))
            {
                var capabilityExists = await _testReqmtRepository.ExistsByTestBuyerCodeAsync(dto.TestBuyerCode);
                if (!capabilityExists)
                    throw new InvalidOperationException("This workgroup is not setup to do this test.");
            }

            var entity = _mapper.Map<TestReqmt>(dto);
            var created = await _testReqmtRepository.AddAsync(entity);
            return _mapper.Map<TestReqmtDto>(created);
        }

        public async Task<TestReqmtDto> UpdateTestReqmtAsync(TestReqmtDto dto)
        {
            // UTrig: both fields null
            if (string.IsNullOrWhiteSpace(dto.ProjectBuyerCode) && string.IsNullOrWhiteSpace(dto.TestBuyerCode))
                throw new InvalidOperationException("Cannot update, you must fill in project buyer or test buyer.");

            // UTrig: TestCapability must exist when TestBuyerCode is provided
            if (!string.IsNullOrWhiteSpace(dto.TestBuyerCode))
            {
                var capabilityExists = await _testReqmtRepository.ExistsByTestBuyerCodeAsync(dto.TestBuyerCode);
                if (!capabilityExists)
                    throw new InvalidOperationException("Cannot update, test buyers workgroup is not setup to do this test.");
            }

            // UTrig: no MonthlyOutput records may exist for this TestCode + Buyer
            var hasMonthlyOutput = await _testReqmtRepository.ExistsByTestCodeAndBuyerInMonthlyOutputAsync(dto.TestCode, dto.Buyer);
            if (hasMonthlyOutput)
                throw new InvalidOperationException("Cannot update, existing data in Monthly Output.");

            // UTrig: project must exist when ProjectBuyerCode is provided
            if (!string.IsNullOrWhiteSpace(dto.ProjectBuyerCode))
            {
                var projectExists = await _projectRepository.ExistsAsync(dto.ProjectBuyerCode);
                if (!projectExists)
                    throw new InvalidOperationException("Cannot update, project does not exist.");
            }

            var entity = _mapper.Map<TestReqmt>(dto);
            var updated = await _testReqmtRepository.UpdateAsync(entity);
            return _mapper.Map<TestReqmtDto>(updated);
        }

        public async Task<bool> DeleteTestReqmtAsync(string testCode, string buyer)
        {
            // DTrig: no MonthlyOutput records may exist for this TestCode + Buyer
            var hasMonthlyOutput = await _testReqmtRepository.ExistsByTestCodeAndBuyerInMonthlyOutputAsync(testCode, buyer);
            if (hasMonthlyOutput)
                throw new InvalidOperationException("Cannot delete, existing data in MonthlyOutput.");

            return await _testReqmtRepository.DeleteAsync(testCode, buyer);
        }

        public async Task<IEnumerable<TestorProductDto>> GetAllTestorProductsAsync()
        {
            var items = await _testorProductRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TestorProductDto>>(items);
        }
    }
}

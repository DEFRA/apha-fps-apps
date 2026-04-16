using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class TimeCodeValidService : ITimeCodeValidService
    {
        private readonly ITimeCodeValidRepository _repository;
        private readonly IJobCodeRepository _jobCodeRepository;
        private readonly ITestCapabilityRepository _testCapabilityRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public TimeCodeValidService(
            ITimeCodeValidRepository repository,
            IJobCodeRepository jobCodeRepository,
            ITestCapabilityRepository testCapabilityRepository,
            IProjectRepository projectRepository,
            IMapper mapper)
        {
            _repository = repository;
            _jobCodeRepository = jobCodeRepository;
            _testCapabilityRepository = testCapabilityRepository;
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TimeCodeValidDto>> GetByJobCodeAsync(string jobCode, string parentProject)
        {
            var items = await _repository.GetByJobCodeAsync(jobCode, parentProject);
            return _mapper.Map<IEnumerable<TimeCodeValidDto>>(items);
        }

        public async Task<PaginatedResult<TimeCodeValidDto>> GetPagedTimeCodesAsync(QueryParameters<string> query, string? jobCode, string? parentProject)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedTimeCodesAsync(parameters, jobCode, parentProject);
            return _mapper.Map<PaginatedResult<TimeCodeValidDto>>(pagedData);
        }

        public async Task<TimeCodeValidDto?> GetTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
        {
            var item = await _repository.GetTimeCodeValidAsync(workGroup, timeCode, parentProject);
            return item == null ? null : _mapper.Map<TimeCodeValidDto>(item);
        }

        public async Task<TimeCodeValidDto> CreateTimeCodeValidAsync(TimeCodeValidDto timeCodeValid)
        {
            await ValidateTimeCodeFieldsAsync(timeCodeValid, null);
            var entity = _mapper.Map<TimeCodeValid>(timeCodeValid);
            var created = await _repository.CreateTimeCodeValidAsync(entity);
            return _mapper.Map<TimeCodeValidDto>(created);
        }

        public async Task<TimeCodeValidDto> UpdateTimeCodeValidAsync(TimeCodeValidDto timeCodeValid)
        {
            var existing = await _repository.GetTimeCodeValidAsync(
                timeCodeValid.WorkGroup, timeCodeValid.TimeCode, timeCodeValid.ParentProject);
            await ValidateTimeCodeFieldsAsync(timeCodeValid, existing);
            var entity = _mapper.Map<TimeCodeValid>(timeCodeValid);
            var updated = await _repository.UpdateTimeCodeValidAsync(entity);
            return _mapper.Map<TimeCodeValidDto>(updated);
        }

        public async Task<bool> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
        {
            return await _repository.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);
        }

        public async Task<bool> DeleteAllByJobCodeAsync(string jobCode, string parentProject)
        {
            return await _repository.DeleteAllByJobCodeAsync(jobCode, parentProject);
        }

        public async Task<IEnumerable<TimeCodeValidDto>> CopyWorkGroupAsync(string sourceJobCode, string targetJobCode, string parentProject)
        {
            var items = await _repository.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);
            return _mapper.Map<IEnumerable<TimeCodeValidDto>>(items);
        }

        public async Task<bool> DeleteBulkAsync(IEnumerable<(string WorkGroup, string TimeCode)> items, string parentProject)
        {
            return await _repository.DeleteBulkAsync(items, parentProject);
        }

        public async Task<IEnumerable<TimeCodeValidDto>> CopySelectedWorkGroupsAsync(IEnumerable<string> workGroups, string sourceJobCode, string targetJobCode, string parentProject)
        {
            var result = await _repository.CopySelectedWorkGroupsAsync(workGroups, sourceJobCode, targetJobCode, parentProject);
            return _mapper.Map<IEnumerable<TimeCodeValidDto>>(result);
        }

        /// <summary>
        /// Validates FK combinations mirroring TimeCodeValid_ITrig (insert) and TimeCodeValid_UTrig (update).
        /// Pass <paramref name="existing"/> as null for insert validation; supply existing entity for update validation.
        /// Note: the MonthlyTime dependency check from UTrig is not implemented here as that entity is not available.
        /// </summary>
        private async Task ValidateTimeCodeFieldsAsync(TimeCodeValidDto dto, TimeCodeValid? existing)
        {
            bool hasTestCode = !string.IsNullOrEmpty(dto.TestCode);
            bool hasPortfolio = !string.IsNullOrEmpty(dto.Portfolio);
            bool hasJobCode = !string.IsNullOrEmpty(dto.JobCode);

            // Rule: must fill in (TestCode + Portfolio) or JobCode — not all null
            if (!hasTestCode && !hasPortfolio && !hasJobCode)
                throw new InvalidOperationException("Must fill in Testcode and Portfolio, or Jobcode");

            // Rule: TestCode and Portfolio must both be supplied together
            if (hasTestCode && !hasPortfolio)
                throw new InvalidOperationException("Must fill in Testcode and Portfolio, or Jobcode");
            if (hasPortfolio && !hasTestCode)
                throw new InvalidOperationException("Must fill in Testcode and Portfolio, or Jobcode");

            bool isInsert = existing == null;

            // Rule: validate JobCode FK
            if (hasJobCode)
            {
                bool jobCodeChanged = isInsert || existing!.JobCode != dto.JobCode;
                if (jobCodeChanged)
                {
                    var jobCode = await _jobCodeRepository.GetJobCodeByIdAsync(dto.JobCode!);
                    if (jobCode == null)
                        throw new InvalidOperationException("Not a valid jobcode.");
                }
            }

            // Rule: validate TestCode + Portfolio combination in tlkpTestCapability
            if (hasTestCode && hasPortfolio)
            {
                bool comboChanged = isInsert || existing!.TestCode != dto.TestCode || existing.Portfolio != dto.Portfolio;
                if (comboChanged)
                {
                    var comboExists = await _testCapabilityRepository.ExistsAsync(dto.TestCode!, dto.Portfolio!);
                    if (!comboExists)
                        throw new InvalidOperationException("Cannot update, this testcode is not in this portfolio.");
                }
            }

            // Rule: validate ParentProject FK
            if (!string.IsNullOrEmpty(dto.ParentProject))
            {
                bool projectChanged = isInsert || existing!.ParentProject != dto.ParentProject;
                if (projectChanged)
                {
                    var projectExists = await _projectRepository.ExistsAsync(dto.ParentProject);
                    if (!projectExists)
                        throw new InvalidOperationException("Not a valid project");
                }
            }
        }
    }
}

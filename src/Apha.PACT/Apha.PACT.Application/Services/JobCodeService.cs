using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class JobCodeService : IJobCodeService
    {
        private readonly IJobCodeRepository _repository;
        private readonly ITimeCodeValidRepository _timeCodeValidRepository;
        private readonly IMapper _mapper;

        public JobCodeService(IJobCodeRepository repository, ITimeCodeValidRepository timeCodeValidRepository, IMapper mapper)
        {
            _repository = repository;
            _timeCodeValidRepository = timeCodeValidRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<JobCodeDto>> GetJobCodesByProjectAsync(string parentProject)
        {
            var items = await _repository.GetJobCodesByProjectAsync(parentProject);
            return _mapper.Map<IEnumerable<JobCodeDto>>(items);
        }

        public async Task<PaginatedResult<JobCodeDto>> GetPagedJobCodesAsync(QueryParameters<string> query, string? parentProject)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedJobCodesAsync(parameters, parentProject);
            return _mapper.Map<PaginatedResult<JobCodeDto>>(pagedData);
        }

        public async Task<JobCodeDto?> GetJobCodeByIdAsync(string jobCodeId)
        {
            var item = await _repository.GetJobCodeByIdAsync(jobCodeId);
            return item == null ? null : _mapper.Map<JobCodeDto>(item);
        }

        public async Task<IEnumerable<string>> GetTypesAsync()
        {
            return await _repository.GetTypesAsync();
        }

        public async Task<JobCodeDto> CreateJobCodeAsync(JobCodeDto jobCode)
        {
            var existing = await _repository.GetJobCodeByIdAsync(jobCode.JobCodeId);
            if (existing != null)
                throw new InvalidOperationException($"A JobCode with ID '{jobCode.JobCodeId}' already exists.");

            var entity = _mapper.Map<JobCode>(jobCode);
            var created = await _repository.CreateJobCodeAsync(entity);
            return _mapper.Map<JobCodeDto>(created);
        }

        public async Task<JobCodeDto> UpdateJobCodeAsync(JobCodeDto jobCode)
        {
            var existing = await _repository.GetJobCodeByIdAsync(jobCode.JobCodeId);
            if (existing != null && existing.JobCodeId != jobCode.JobCodeId && 
                await _timeCodeValidRepository.HasRelatedTimeCodeValidRecordsAsync(existing.JobCodeId))
            {
                throw new InvalidOperationException($"This JobCode has related records in TimeCodeValid and cannot be updated.");
            }

            var entity = _mapper.Map<JobCode>(jobCode);
            var updated = await _repository.UpdateJobCodeAsync(entity);
            return _mapper.Map<JobCodeDto>(updated);
        }

        public async Task<bool> DeleteJobCodeAsync(string jobCodeId)
        {
            if (await _timeCodeValidRepository.HasRelatedTimeCodeValidRecordsAsync(jobCodeId))
                throw new InvalidOperationException($"This JobCode has related records in TimeCodeValid and cannot be deleted.");

            return await _repository.DeleteJobCodeAsync(jobCodeId);
        }
    }
}

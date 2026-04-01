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
        private readonly IMapper _mapper;

        public TimeCodeValidService(ITimeCodeValidRepository repository, IMapper mapper)
        {
            _repository = repository;
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
            var entity = _mapper.Map<TimeCodeValid>(timeCodeValid);
            var created = await _repository.CreateTimeCodeValidAsync(entity);
            return _mapper.Map<TimeCodeValidDto>(created);
        }

        public async Task<TimeCodeValidDto> UpdateTimeCodeValidAsync(TimeCodeValidDto timeCodeValid)
        {
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
    }
}

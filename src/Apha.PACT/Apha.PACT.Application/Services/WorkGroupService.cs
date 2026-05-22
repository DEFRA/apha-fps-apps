using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IWorkGroupRepository _repository;
        private readonly IMapper _mapper;

        public WorkGroupService(IWorkGroupRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkGroupDto>> GetAllWorkGroupsAsync()
        {
            var items = await _repository.GetAllWorkGroupsAsync();
            return _mapper.Map<IEnumerable<WorkGroupDto>>(items);
        }

        public async Task<PaginatedResult<WorkGroupTimeCodeDto>> GetWorkGroupTimeCodeAsync(QueryParameters<string> query, string workGroup, int monthNumber)
        {
            ValidateWorkGroup(workGroup);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupTimeCodeAsync(parameters, workGroup, monthNumber);
            return _mapper.Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData);
        }

        public async Task<PaginatedResult<WorkGroupValidTimeCodeDto>> GetWorkGroupValidTimeCodeAsync(
            QueryParameters<string> query, string workGroup)
        {
            ValidateWorkGroup(workGroup);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupValidTimeCodeAsync(parameters, workGroup);
            return _mapper.Map<PaginatedResult<WorkGroupValidTimeCodeDto>>(pagedData);
        }

        public async Task<PaginatedResult<WorkGroupDto>> GetWorkGroupsByProfitCentreAsync(
            QueryParameters<string> query, string profitCentre)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupsByProfitCentreAsync(parameters, profitCentre);
            return _mapper.Map<PaginatedResult<WorkGroupDto>>(pagedData);
        }

        public async Task<bool> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            return await _repository.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag);
        }

        public async Task<bool> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            return await _repository.SetSendEmailForAllWorkGroupsAsync(flag);
        }

        public async Task<bool> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient)
        {
            return await _repository.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient);
        }

        private static void ValidateWorkGroup(string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("WorkGroup is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }
    }
}

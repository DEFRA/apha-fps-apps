using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class ProjectSubContractService : IProjectSubContractService
    {
        private readonly IProjectSubContractRepository _repository;
        private readonly IMapper _mapper;

        public ProjectSubContractService(IProjectSubContractRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProjectSubContractDto>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project)
        {
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<ProjectSubContract> pagedData = await _repository.GetPagedProjectSubContractsAsync(parameters, project);
            return _mapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData);
        }

        public async Task<decimal> GetTotalAmountAsync(string? project)
            => await _repository.GetTotalAmountAsync(project);

        public async Task<ProjectSubContractDto?> GetByIdAsync(int subContCounter)
        {
            ProjectSubContract? entity = await _repository.GetByIdAsync(subContCounter);
            return entity == null ? null : _mapper.Map<ProjectSubContractDto>(entity);
        }

        public async Task<ProjectSubContractDto> CreateAsync(ProjectSubContractDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.Month is null)
                errors.Add(new BusinessValidationError("Month is required", "MONTH_REQUIRED"));
            if (dto.Amount is null)
                errors.Add(new BusinessValidationError("Amount is required", "AMOUNT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectSubContract entity = _mapper.Map<ProjectSubContract>(dto);
            ProjectSubContract created = await _repository.CreateAsync(entity);
            return _mapper.Map<ProjectSubContractDto>(created);
        }

        public async Task<ProjectSubContractDto> UpdateAsync(ProjectSubContractDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.Month is null)
                errors.Add(new BusinessValidationError("Month is required", "MONTH_REQUIRED"));
            if (dto.Amount is null)
                errors.Add(new BusinessValidationError("Amount is required", "AMOUNT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectSubContract entity = _mapper.Map<ProjectSubContract>(dto);
            ProjectSubContract updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<ProjectSubContractDto>(updated);
        }

        public async Task<bool> DeleteAsync(int subContCounter)
        {
            return await _repository.DeleteAsync(subContCounter);
        }
    }
}

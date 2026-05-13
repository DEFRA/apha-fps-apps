using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class ProjectMonthService : IProjectMonthService
    {
        private readonly IProjectMonthRepository _repository;
        private readonly IMapper _mapper;

        public ProjectMonthService(IProjectMonthRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IList<ProjectMonthDto>> GetProjectMonthByProjectAsync(string project)
        {
            IList<ProjectMonth> entities = await _repository.GetProjectMonthByProjectAsync(project);
            return _mapper.Map<IList<ProjectMonthDto>>(entities);
        }

        public async Task<ProjectMonthDto?> GetProjectMonthAsync(string project, int monthNo)
        {
            ProjectMonth? entity = await _repository.GetProjectMonthAsync(project, monthNo);
            return entity == null ? null : _mapper.Map<ProjectMonthDto>(entity);
        }

        public async Task<ProjectMonthDto> CreateProjectMonthAsync(ProjectMonthDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.MonthNo <= 0)
                errors.Add(new BusinessValidationError("Month number must be greater than zero", "MONTHNO_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectMonth entity = _mapper.Map<ProjectMonth>(dto);
            ProjectMonth created = await _repository.CreateProjectMonthAsync(entity);
            return _mapper.Map<ProjectMonthDto>(created);
        }

        public async Task<ProjectMonthDto> UpdateProjectMonthAsync(ProjectMonthDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.MonthNo <= 0)
                errors.Add(new BusinessValidationError("Month number must be greater than zero", "MONTHNO_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectMonth entity = _mapper.Map<ProjectMonth>(dto);
            ProjectMonth updated = await _repository.UpdateProjectMonthAsync(entity);
            return _mapper.Map<ProjectMonthDto>(updated);
        }

        public async Task<bool> DeleteProjectMonthAsync(string project, int monthNo)
        {
            return await _repository.DeleteProjectMonthAsync(project, monthNo);
        }
    }
}
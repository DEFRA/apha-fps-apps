using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class WorkGroupEmployeeService : IWorkGroupEmployeeService
    {
        private readonly IWorkGroupEmployeeRepository _repository;
        private readonly IMapper _mapper;

        public WorkGroupEmployeeService(IWorkGroupEmployeeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<WorkGroupEmployeeDto>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade)
        {
            ArgumentNullException.ThrowIfNull(query);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupEmployeeAsync(filter, wgGrade);
            return _mapper.Map<PaginatedResult<WorkGroupEmployeeDto>>(pagedData);
        }

        public async Task<WorkGroupEmployeeDto?> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            if (string.IsNullOrWhiteSpace(pactId))
            {
                throw new ArgumentException("PACT Id is required.");
            }

            var entity = await _repository.GetWorkGroupEmployeeByIdAsync(pactId);
            return _mapper.Map<WorkGroupEmployeeDto?>(entity);
        }

        public async Task<WorkGroupEmployeeDto> CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.PactId))
            {
                throw new ArgumentException("PACT Id is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.WorkGroupGrade))
            {
                throw new ArgumentException("Work Group Grade is required.");
            }

            var existing = await _repository.GetWorkGroupEmployeeByIdAsync(dto.PactId);
            if (existing != null)
            {
                throw new ArgumentException($"WorkGroupEmployee with PACT Id '{dto.PactId}' already exists.");
            }

            var entity = _mapper.Map<WorkGroupEmployee>(dto);
            var created = await _repository.CreateWorkGroupEmployeeAsync(entity);
            return _mapper.Map<WorkGroupEmployeeDto>(created);
        }

        public async Task<WorkGroupEmployeeDto> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.PactId))
            {
                throw new ArgumentException("PACT Id is required.");
            }

            var existing = await _repository.GetWorkGroupEmployeeByIdAsync(dto.PactId);
            if (existing == null)
            {
                throw new KeyNotFoundException($"WorkGroupEmployee with PACT Id '{dto.PactId}' not found.");
            }

            var entity = _mapper.Map<WorkGroupEmployee>(dto);
            var updated = await _repository.UpdateWorkGroupEmployeeAsync(entity);
            return _mapper.Map<WorkGroupEmployeeDto>(updated);
        }

        public async Task<bool> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            if (string.IsNullOrWhiteSpace(pactId))
            {
                throw new ArgumentException("PACT Id is required.");
            }

            var existing = await _repository.GetWorkGroupEmployeeByIdAsync(pactId);
            if (existing == null)
            {
                throw new KeyNotFoundException($"WorkGroupEmployee with PACT Id '{pactId}' was not found.");
            }

            return await _repository.DeleteWorkGroupEmployeeAsync(pactId);
        }

    }
}

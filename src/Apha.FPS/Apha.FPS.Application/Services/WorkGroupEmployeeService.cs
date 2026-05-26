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
            ArgumentException.ThrowIfNullOrWhiteSpace(wgGrade);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupEmployeeAsync(filter, wgGrade);
            return _mapper.Map<PaginatedResult<WorkGroupEmployeeDto>>(pagedData);
        }

        public async Task<WorkGroupEmployeeDto?> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pactId);
            var entity = await _repository.GetWorkGroupEmployeeByIdAsync(pactId);
            return _mapper.Map<WorkGroupEmployeeDto>(entity);
        }

        public async Task<WorkGroupEmployeeDto> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            var entity = _mapper.Map<WorkGroupEmployee>(dto);
            var updated = await _repository.UpdateWorkGroupEmployeeAsync(entity);
            return _mapper.Map<WorkGroupEmployeeDto>(updated);
        }

        public async Task<bool> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pactId);
            return await _repository.DeleteWorkGroupEmployeeAsync(pactId);
        }
    }
}

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

        public async Task<PaginatedResult<WgEmployeeViewDto>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(wgGrade);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupEmployeeAsync(filter, wgGrade);
            return _mapper.Map<PaginatedResult<WgEmployeeViewDto>>(pagedData);
        }

        public async Task<WgEmployeeViewDto?> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pactId);
            var entity = await _repository.GetWorkGroupEmployeeByIdAsync(pactId);
            return _mapper.Map<WgEmployeeViewDto>(entity);
        }

        public async Task<WgEmployeeDto> UpdateWorkGroupEmployeeAsync(WgEmployeeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            var entity = _mapper.Map<WgEmployee>(dto);
            var updated = await _repository.UpdateWorkGroupEmployeeAsync(entity);
            return _mapper.Map<WgEmployeeDto>(updated);
        }

        public async Task DeleteWorkGroupEmployeeAsync(string pactId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pactId);
            await _repository.DeleteWorkGroupEmployeeAsync(pactId);
        }
    }
}

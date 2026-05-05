using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class WgStaffService : IWgStaffService
    {
        private readonly IWgStaffRepository _repository;
        private readonly IMapper _mapper;

        public WgStaffService(IWgStaffRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<WgEmployeeViewDto>> GetWgStaffAsync(QueryParameters<string> query, string wgGrade, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(wgGrade);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWgStaffAsync(filter, wgGrade, cancellationToken);
            return _mapper.Map<PaginatedResult<WgEmployeeViewDto>>(pagedData);
        }

        public async Task<WgEmployeeViewDto?> GetWgEmployeeByIdAsync(string pactId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pactId);
            var entity = await _repository.GetWgEmployeeByIdAsync(pactId, cancellationToken);
            return _mapper.Map<WgEmployeeViewDto>(entity);
        }

        public async Task<WgEmployeeDto> UpdateWgEmployeeAsync(WgEmployeeDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            var entity = _mapper.Map<WgEmployee>(dto);
            var updated = await _repository.UpdateWgEmployeeAsync(entity, cancellationToken);
            return _mapper.Map<WgEmployeeDto>(updated);
        }

        public async Task DeleteWgEmployeeAsync(string pactId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pactId);
            await _repository.DeleteWgEmployeeAsync(pactId, cancellationToken);
        }
    }
}

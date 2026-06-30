using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class TotalBusinessOverheadsService : ITotalBusinessOverheadsService
    {
        private readonly ITotalBusinessOverheadsRepository _repository;
        private readonly IFpsRequestContext _requestContext;
        private readonly IMapper _mapper;

        public TotalBusinessOverheadsService(
            ITotalBusinessOverheadsRepository repository,
            IFpsRequestContext requestContext,
            IMapper mapper)
        {
            _repository = repository;
            _requestContext = requestContext;
            _mapper = mapper;
        }

        public async Task<TotalBusinessOverheadsDto?> GetAsync()
        {
            var entity = await _repository.GetByYearAsync(_requestContext.FpsYear);
            return entity == null ? null : _mapper.Map<TotalBusinessOverheadsDto>(entity);
        }

        public async Task<TotalBusinessOverheadsDto> UpdateAsync(TotalBusinessOverheadsDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var existing = await _repository.GetByYearAsync(_requestContext.FpsYear);

            if (existing == null)
                throw new InvalidOperationException(
                    $"Total Business Overheads record for year '{_requestContext.FpsYear}' was not found.");

            existing.BusinessOverheads = dto.TotalBusinessOverheads;
            var result = await _repository.UpdateAsync(existing);
            return _mapper.Map<TotalBusinessOverheadsDto>(result);
        }
    }
}

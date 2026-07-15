using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class CostCentreService : ICostCentreService
    {
        private readonly ICostCentreRepository _repository;
        private readonly IProfitCentreRepository _profitCentreRepository;
        private readonly IMapper _mapper;

        public CostCentreService(
            ICostCentreRepository repository,
            IProfitCentreRepository profitCentreRepository,
            IMapper mapper)
        {
            _repository = repository;
            _profitCentreRepository = profitCentreRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<CostCentreDto>> GetAllCostCentresPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var queryParams = _mapper.Map<PaginationParameters<string>>(query);
            var pagedResult = await _repository.GetAllPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<CostCentreDto>>(pagedResult);
        }

        public async Task<CostCentreDto?> GetCostCentreByIdAsync(double costCentreNo, int fpsYear)
        {
            var entity = await _repository.GetByIdAsync(costCentreNo, fpsYear);
            return entity == null ? null : _mapper.Map<CostCentreDto>(entity);
        }

        //   Guard 1: ArgumentNullException if dto null (fail fast before any I/O)
        //   Guard 2: InvalidOperationException if composite key already exists (duplicate prevention from VBA analysis)
        //   Guard 3: InvalidOperationException if ProfitCentre FK does not exist in tblkpprofitcentre (transform-plan.md item 1)
        public async Task<CostCentreDto> CreateCostCentreAsync(CostCentreDto costCentreDto)
        {
            ArgumentNullException.ThrowIfNull(costCentreDto);
            ArgumentException.ThrowIfNullOrWhiteSpace(costCentreDto.ProfitCentre);

            if (await _repository.ExistsAsync(costCentreDto.CostCentreNo, costCentreDto.FpsYear))
                throw new InvalidOperationException(
                    $"A cost centre with number '{costCentreDto.CostCentreNo}' already exists for FPS year '{costCentreDto.FpsYear}'.");

            //   Validates that the supplied ProfitCentre code exists in fps.tblkpprofitcentre before inserting
            var profitCentreExists = await _profitCentreRepository.ProfitCentreExistsAsync(costCentreDto.ProfitCentre);
            if (!profitCentreExists)
                throw new InvalidOperationException(
                    $"Profit centre '{costCentreDto.ProfitCentre}' does not exist. Select a valid profit centre.");

            var entity = _mapper.Map<CostCentre>(costCentreDto);
            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<CostCentreDto>(created);
        }

        //   Guard 1: ArgumentNullException if dto null
        //   Guard 2: KeyNotFoundException if original record does not exist
        //   Guard 3: InvalidOperationException if new ProfitCentre FK does not exist in tblkpprofitcentre (transform-plan.md item 1)
        public async Task<CostCentreDto> UpdateCostCentreAsync(double originalCostCentreNo, int fpsYear, CostCentreDto costCentreDto)
        {
            ArgumentNullException.ThrowIfNull(costCentreDto);
            ArgumentException.ThrowIfNullOrWhiteSpace(costCentreDto.ProfitCentre);

            if (!await _repository.ExistsAsync(originalCostCentreNo, fpsYear))
                throw new KeyNotFoundException(
                    $"Cost centre '{originalCostCentreNo}' for FPS year '{fpsYear}' was not found.");

            var profitCentreExists = await _profitCentreRepository.ProfitCentreExistsAsync(costCentreDto.ProfitCentre);
            if (!profitCentreExists)
                throw new InvalidOperationException(
                    $"Profit centre '{costCentreDto.ProfitCentre}' does not exist. Select a valid profit centre.");

            var entity = _mapper.Map<CostCentre>(costCentreDto);
            var updated = await _repository.UpdateAsync(originalCostCentreNo, fpsYear, entity);
            return _mapper.Map<CostCentreDto>(updated);
        }

        //   Guard: KeyNotFoundException if record does not exist before attempting delete
        public async Task<bool> DeleteCostCentreAsync(double costCentreNo, int fpsYear)
        {
            if (!await _repository.ExistsAsync(costCentreNo, fpsYear))
                throw new KeyNotFoundException(
                    $"Cost centre '{costCentreNo}' for FPS year '{fpsYear}' was not found.");

            return await _repository.DeleteAsync(costCentreNo, fpsYear);
        }
    }
}

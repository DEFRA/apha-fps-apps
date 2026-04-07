using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class YearMasterService : IYearMasterService
    {
        private readonly IYearMasterRepository _yearMasterRepository;
        private readonly IMapper _mapper;

        public YearMasterService(IYearMasterRepository yearMasterRepository, IMapper mapper)
        {
            _yearMasterRepository = yearMasterRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<YearMasterDto>> GetAllFpsYearsAsync()
        {
            var yearMasters = await _yearMasterRepository.GetAllFpsYearsAsync();
            return _mapper.Map<IEnumerable<YearMasterDto>>(yearMasters);
        }

        public async Task<PaginatedResult<YearMasterDto>> GetAllFpsYearsPagedAsync(QueryParameters<int> query)
        {
            var filter = _mapper.Map<PaginationParameters<int>>(query);
            var yearMasters = await _yearMasterRepository.GetAllFpsYearsPagedAsync(filter);
            return _mapper.Map<PaginatedResult<YearMasterDto>>(yearMasters);
        }

        public async Task<YearMasterDto?> GetFpsYearByIdAsync(int fpsYear)
        {
            var yearMaster = await _yearMasterRepository.GetFpsYearByIdAsync(fpsYear);
            return _mapper.Map<YearMasterDto?>(yearMaster);
        }
    }
}

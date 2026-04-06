using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class YearMasterService : IYearMasterService
    {
        private readonly IFpsApiClient _fpsClient;

        public YearMasterService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<IEnumerable<YearMasterDto>>> GetAllYearMastersAsync()
        {
            var yearMasters = await _fpsClient.FpsYearMaster.GetAllYearMastersAsync();
            return yearMasters;
        }

        public async Task<ApiResponseDto<List<YearMasterDto>>> GetAllYearMastersPagedAsync(QueryParameters<int> query)
        {
            var yearMasters = await _fpsClient.FpsYearMaster.GetAllYearMastersPagedAsync(query);
            return yearMasters;
        }

        public async Task<ApiResponseDto<YearMasterDto>> GetYearMasterByIdAsync(int fpsYear)
        {
            var yearMaster = await _fpsClient.FpsYearMaster.GetYearMasterByIdAsync(fpsYear);
            return yearMaster;
        }
    }
}

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service implementation for Division operations.
    /// </summary>
    public class DivisionService : IDivisionService
    {
        private readonly IFpsApiClient _fpsClient;

        public DivisionService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<IEnumerable<DivisionDto>>> GetAllDivisionsAsync()
        {
            return await _fpsClient.FpsDivision.GetAllDivisionsAsync();
        }

        public async Task<ApiResponseDto<List<DivisionDto>>> GetAllDivisionsPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsDivision.GetAllDivisionsPagedAsync(query);
        }

        public async Task<ApiResponseDto<DivisionDto>> GetDivisionByNameAsync(string divName)
        {
            return await _fpsClient.FpsDivision.GetDivisionByNameAsync(divName);
        }

        public async Task<ApiResponseDto<DivisionDto>> CreateDivisionAsync(DivisionDto divisionDto)
        {
            return await _fpsClient.FpsDivision.CreateDivisionAsync(divisionDto);
        }

        public async Task<ApiResponseDto<DivisionDto>> UpdateDivisionAsync(string divName, DivisionDto divisionDto)
        {
            return await _fpsClient.FpsDivision.UpdateDivisionAsync(divName, divisionDto);
        }

        public async Task<ApiResponseDto<bool>> DeleteDivisionAsync(string divName)
        {
            return await _fpsClient.FpsDivision.DeleteDivisionAsync(divName);
        }

        public async Task<ApiResponseDto<IEnumerable<AgencyDto>>> GetAllAgenciesAsync()
        {
            return await _fpsClient.FpsAgency.GetAllAgenciesAsync();
        }
    }
}

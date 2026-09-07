using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service implementation for Master Lookup operations.
    /// </summary>
    public class MasterLookupService : IMasterLookupService
    {
        private readonly IFpsApiClient _fpsClient;

        public MasterLookupService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<IEnumerable<MasterLookupDto>>> GetAllMasterLookupsAsync()
        {
            return await _fpsClient.FpsMasterLookup.GetAllMasterLookupsAsync();
        }

        public async Task<ApiResponseDto<IEnumerable<LookupItemDto>>> GetLookupItemsAsync(string tableName)
        {
            return await _fpsClient.FpsMasterLookup.GetLookupItemsAsync(tableName);
        }

        public async Task<ApiResponseDto<PaginatedResult<LookupItemDto>>> GetLookupItemsPagedAsync(string tableName, QueryParameters<string> query)
        {
            return await _fpsClient.FpsMasterLookup.GetLookupItemsPagedAsync(tableName, query);
        }

        public async Task<ApiResponseDto<bool>> CreateLookupItemAsync(string tableName, string value)
        {
            return await _fpsClient.FpsMasterLookup.CreateLookupItemAsync(tableName, value);
        }

        public async Task<ApiResponseDto<bool>> UpdateLookupItemAsync(string tableName, string originalValue, string newValue)
        {
            return await _fpsClient.FpsMasterLookup.UpdateLookupItemAsync(tableName, originalValue, newValue);
        }

        public async Task<ApiResponseDto<bool>> DeleteLookupItemAsync(string tableName, string value)
        {
            return await _fpsClient.FpsMasterLookup.DeleteLookupItemAsync(tableName, value);
        }
    }
}

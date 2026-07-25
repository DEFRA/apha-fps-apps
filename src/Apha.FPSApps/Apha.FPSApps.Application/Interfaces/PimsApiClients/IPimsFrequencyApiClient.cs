using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors FrequencyController — integer PK (frequencyid); full CRUD
    public interface IPimsFrequencyApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/frequency — full list
        Task<ApiResponseDto<List<FrequencyDto>>> GetAllFrequenciesAsync();

        // TRANSFORMENGINE: GET /api/v1/frequency/paged — paged/sorted/filterable list
        Task<ApiResponseDto<PaginatedResult<FrequencyDto>>> GetPagedFrequenciesAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: GET /api/v1/frequency/{frequencyId:int}
        Task<ApiResponseDto<FrequencyDto>> GetFrequencyByIdAsync(int frequencyId);

        // TRANSFORMENGINE: POST /api/v1/frequency
        Task<ApiResponseDto<FrequencyDto>> CreateFrequencyAsync(FrequencyDto dto);

        // TRANSFORMENGINE: PUT /api/v1/frequency/{frequencyId:int} — route PK is authoritative
        Task<ApiResponseDto<FrequencyDto>> UpdateFrequencyAsync(int frequencyId, FrequencyDto dto);

        Task<ApiResponseDto<bool>> DeleteFrequencyAsync(int frequencyId);
    }
}

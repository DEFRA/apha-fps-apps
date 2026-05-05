using Apha.Common.Constants;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookDiseaseApiClient : ICostBookDiseaseApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public CostBookDiseaseApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<DiseaseDto>>> GetAllDiseasesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<DiseaseRes>>(CostBookApiEndpoints.GetAllDiseases);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DiseaseDto>>>(response);
                return ApiResponseDto<List<DiseaseDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception ex)
            {
                return ApiResponseDto<List<DiseaseDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve diseases", Code = InternalCodeError, Details = ex.Message }],
                    new ApiMetaDto());
            }
        }
    }
}

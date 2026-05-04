using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    /// <summary>
    /// HTTP client implementation for Agency API operations.
    /// </summary>
    public class FpsAgencyApiClient : IFpsAgencyApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsAgencyApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<IEnumerable<AgencyDto>>> GetAllAgenciesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AgencyRes>>("api/agency");

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<IEnumerable<AgencyDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<AgencyDto>>>(response);
                    return ApiResponseDto<IEnumerable<AgencyDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = "Failed to retrieve agency data",
                        Code = InternalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<IEnumerable<AgencyDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }
    }
}

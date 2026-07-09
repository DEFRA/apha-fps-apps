using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsTotalBusinessOverheadsApiClient : IFpsTotalBusinessOverheadsApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsTotalBusinessOverheadsApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<TotalBusinessOverheadsDto>> GetAsync()
        {
            try
            {
                var response = await _http.GetAsync<TotalBusinessOverheadsRes>(FpsApiEndpoints.GetTotalBusinessOverheads);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(response);
                return ApiResponseDto<TotalBusinessOverheadsDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TotalBusinessOverheadsDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Total Business Overheads", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TotalBusinessOverheadsDto>> UpdateAsync(TotalBusinessOverheadsDto dto)
        {
            try
            {
                var request = _mapper.Map<TotalBusinessOverheadsReq>(dto);
                var response = await _http.PutAsync<TotalBusinessOverheadsReq, TotalBusinessOverheadsRes>(
                    FpsApiEndpoints.UpdateTotalBusinessOverheads, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<TotalBusinessOverheadsDto>>(response);
                return ApiResponseDto<TotalBusinessOverheadsDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TotalBusinessOverheadsDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update Total Business Overheads", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}

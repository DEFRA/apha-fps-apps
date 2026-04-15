using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsTestorProductApiClient : IFpsTestorProductApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsTestorProductApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<TestorProductRes>>(FpsApiEndpoints.GetAllTestorProducts);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(response);
                return ApiResponseDto<List<TestorProductDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestorProductDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve testor products", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

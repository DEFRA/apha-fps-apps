using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsTimeSellerPcApiClient : IFpsTimeSellerPcApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsTimeSellerPcApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TimeSellerPcRowDto>>> GetRowsAsync(string sellingPc)
        {
            var url = string.Format(FpsApiEndpoints.GetTimeSellerPcRows, sellingPc);
            var response = await _http.GetAsync<List<TimeSellerPcRowRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TimeSellerPcRowDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TimeSellerPcRowDto>>>(response);
            return ApiResponseDto<List<TimeSellerPcRowDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TimeSellerPcTotalsDto>> GetTotalsAsync(string sellingPc)
        {
            var url = string.Format(FpsApiEndpoints.GetTimeSellerPcTotals, sellingPc);
            var response = await _http.GetAsync<TimeSellerPcTotalsRes>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<TimeSellerPcTotalsDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<TimeSellerPcTotalsDto>>(response);
            return ApiResponseDto<TimeSellerPcTotalsDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsTimeSellerPcApiClient : IFpsContributionSummaryApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsTimeSellerPcApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ContributionSummaryRowDto>>> GetRowsAsync(string sellingPc)
        {
            var url = string.Format(FpsApiEndpoints.GetTimeSellerPcRows, sellingPc);
            var response = await _http.GetAsync<List<ContributionSummaryRowRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ContributionSummaryRowDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ContributionSummaryRowDto>>>(response);
            return ApiResponseDto<List<ContributionSummaryRowDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ContributionSummaryTotalsDto>> GetTotalsAsync(string sellingPc)
        {
            var url = string.Format(FpsApiEndpoints.GetTimeSellerPcTotals, sellingPc);
            var response = await _http.GetAsync<ContributionSummaryTotalsRes>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<ContributionSummaryTotalsDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ContributionSummaryTotalsDto>>(response);
            return ApiResponseDto<ContributionSummaryTotalsDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

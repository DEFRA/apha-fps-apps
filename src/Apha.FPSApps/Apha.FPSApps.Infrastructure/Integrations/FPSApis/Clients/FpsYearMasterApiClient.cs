using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsYearMasterApiClient : IFpsYearMasterApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsYearMasterApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<IEnumerable<YearMasterDto>>> GetAllFpsYearsAsync()
        {
            var response = await _http.GetAsync<List<YearMasterRes>>("api/yearmaster");

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<IEnumerable<YearMasterDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<YearMasterDto>>>(response);
                return ApiResponseDto<IEnumerable<YearMasterDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<YearMasterDto>>> GetAllFpsYearsPagedAsync(QueryParameters<int> query)
        {
            var url = QueryStringHelper.AddQueryString("api/yearmaster/paged", query);
            var response = await _http.GetAsync<List<YearMasterRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<YearMasterDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<YearMasterDto>>>(response);
                return ApiResponseDto<List<YearMasterDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<YearMasterDto>> GetFpsYearByIdAsync(int fpsYear)
        {
            var response = await _http.GetAsync<YearMasterRes>($"api/yearmaster/{fpsYear}");

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<YearMasterDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<YearMasterDto>>(response);
                return ApiResponseDto<YearMasterDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }
    }
}

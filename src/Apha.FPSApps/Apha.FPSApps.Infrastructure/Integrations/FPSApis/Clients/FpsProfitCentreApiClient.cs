using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProfitCentreApiClient : IFpsProfitCentreApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsProfitCentreApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync()
        {
            var response = await _http.GetAsync<List<ProfitCentreRes>>(FpsApiEndpoints.GetProfitCentres);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);
                return ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetAllProfitCentresPagedAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedProfitCentres, query);
            var response = await _http.GetAsync<List<ProfitCentreRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);
                return ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> GetProfitCentreByIdAsync(string profitCentreId)
        {
            var response = await _http.GetAsync<ProfitCentreRes>(string.Format(FpsApiEndpoints.GetProfitCentreById, profitCentreId));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);
                return ApiResponseDto<ProfitCentreDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> CreateProfitCentreAsync(ProfitCentreDto profitCentreDto)
        {
            var request = _mapper.Map<ProfitCentreReq>(profitCentreDto);
            var response = await _http.PostAsync<ProfitCentreReq, ProfitCentreRes>(FpsApiEndpoints.CreateProfitCentre, request);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);
                return ApiResponseDto<ProfitCentreDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> UpdateProfitCentreAsync(string profitCentreId, ProfitCentreDto profitCentreDto)
        {
            var request = _mapper.Map<ProfitCentreReq>(profitCentreDto);
            var response = await _http.PutAsync<ProfitCentreReq, ProfitCentreRes>(string.Format(FpsApiEndpoints.UpdateProfitCentre, profitCentreId), request);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);
                return ApiResponseDto<ProfitCentreDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteProfitCentreAsync(string profitCentreId)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteProfitCentre, profitCentreId));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<bool>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }
    }
}

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

        public async Task<ApiResponseDto<IEnumerable<ProfitCentreDto>>> GetAllProfitCentresAsync()
        {
            var response = await _http.GetAsync<IEnumerable<ProfitCentreRes>>(FpsApiEndpoints.GetAllProfitCentres);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreDto>>>(response);
            return ApiResponseDto<IEnumerable<ProfitCentreDto>>.FailureResponse(dto.Errors, dto.Meta);
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

        public async Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(
            string profitCentre, int timesheet, int outputsheet, short timesheetLayout)
        {
            var request = new UpdateProfitCentreSettingsReq
            {
                ProfitCentre = profitCentre,
                Timesheet = timesheet,
                Outputsheet = outputsheet,
                TimesheetLayout = timesheetLayout
            };
            var response = await _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(
                FpsApiEndpoints.PatchProfitCentreSettings, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var failureDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failureDto.Errors, failureDto.Meta);
        }

        public async Task<ApiResponseDto<IEnumerable<ProfitCentreCostDto>>> GetProfitCenterCostSummaryAsync(double monthNumber)
        {
            var url = $"{FpsApiEndpoints.GetProfitCenterCostSummary}?monthNumber={monthNumber}";

            var response = await _http.GetAsync<IEnumerable<ProfitCentreCostRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreCostDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreCostDto>>>(response);
                return ApiResponseDto<IEnumerable<ProfitCentreCostDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<PaginatedResult<ProfitCentreCostDto>>> GetPagedProfitCenterCostSummaryAsync(
            QueryParameters<string> query, double monthNumber)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedProfitCenterCostSummary, query);
            url = $"{url}&monthNumber={monthNumber}";

            var response = await _http.GetAsync<List<ProfitCentreCostRes>>(url);

            if (response.Success)
            {
                var dto = _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(response);
                var pagination = response.Pagination;
                var result = new PaginatedResult<ProfitCentreCostDto>(
                    dto.Data ?? new List<ProfitCentreCostDto>(),
                    pagination?.TotalRecords ?? 0,
                    pagination?.PageNumber ?? query.Page,
                    pagination?.PageSize ?? query.PageSize);
                return ApiResponseDto<PaginatedResult<ProfitCentreCostDto>>.SuccessResponse(result);
            }

            var failDto = _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(response);
            return ApiResponseDto<PaginatedResult<ProfitCentreCostDto>>.FailureResponse(failDto.Errors, failDto.Meta);
        }
    }
}

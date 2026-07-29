using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsDepartmentIncomeApiClient : IFpsDepartmentIncomeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string InternalCodeError = "INTERNAL_ERROR";

        private const string BaseUrl = "api/v1/department-income";

        public FpsDepartmentIncomeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            try
            {
                var url = BuildIncomeUrl($"{BaseUrl}/time", project, monthFrom, monthTo);
                var response = await _http.GetAsync<List<DepartmentIncomeTimeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(response);
                return ApiResponseDto<List<DepartmentIncomeTimeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<DepartmentIncomeTimeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income time data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            try
            {
                var url = BuildIncomeUrl($"{BaseUrl}/tests", project, monthFrom, monthTo);
                var response = await _http.GetAsync<List<DepartmentIncomeTestRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(response);
                return ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income test data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            try
            {
                var url = BuildIncomeUrl($"{BaseUrl}/animals", project, monthFrom, monthTo);
                var response = await _http.GetAsync<List<DepartmentIncomeAnimalRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(response);
                return ApiResponseDto<List<DepartmentIncomeAnimalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<DepartmentIncomeAnimalDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income animal data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            try
            {
                var url = BuildIncomeUrl($"{BaseUrl}/additional", project, monthFrom, monthTo);
                var response = await _http.GetAsync<List<DepartmentIncomeAdditionalRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(response);
                return ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income additional data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            try
            {
                var url = BuildIncomeUrl($"{BaseUrl}/totals", project, monthFrom, monthTo);
                var response = await _http.GetAsync<List<DepartmentIncomeTotalsRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(response);
                return ApiResponseDto<List<DepartmentIncomeTotalsDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<DepartmentIncomeTotalsDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income totals data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<PeriodLookupRes>>($"{BaseUrl}/periods");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<PeriodLookupDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<PeriodLookupDto>>>(response);
                return ApiResponseDto<List<PeriodLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<PeriodLookupDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Department Income period lookup data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        private static string BuildIncomeUrl(string endpoint, string? project, int? monthFrom, int? monthTo)
        {
            var queryParams = new Dictionary<string, string?>();

            if (project is not null)
                queryParams["project"] = project;
            if (monthFrom.HasValue)
                queryParams["monthFrom"] = monthFrom.Value.ToString();
            if (monthTo.HasValue)
                queryParams["monthTo"] = monthTo.Value.ToString();

            return queryParams.Count > 0
                ? QueryHelpers.AddQueryString(endpoint, queryParams)
                : endpoint;
        }
    }
}

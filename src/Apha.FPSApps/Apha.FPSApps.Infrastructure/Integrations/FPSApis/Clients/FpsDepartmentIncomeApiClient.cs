using Apha.Common.Constants;
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

        public FpsDepartmentIncomeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http   = http   ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var url = BuildIncomeUrl(FpsApiEndpoints.GetDepartmentIncomeTime, project, monthFrom, monthTo);
            var response = await _http.GetAsync<List<DepartmentIncomeTimeRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(response);
            return ApiResponseDto<List<DepartmentIncomeTimeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var url = BuildIncomeUrl(FpsApiEndpoints.GetDepartmentIncomeTests, project, monthFrom, monthTo);
            var response = await _http.GetAsync<List<DepartmentIncomeTestRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(response);
            return ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var url = BuildIncomeUrl(FpsApiEndpoints.GetDepartmentIncomeAnimals, project, monthFrom, monthTo);
            var response = await _http.GetAsync<List<DepartmentIncomeAnimalRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(response);
            return ApiResponseDto<List<DepartmentIncomeAnimalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var url = BuildIncomeUrl(FpsApiEndpoints.GetDepartmentIncomeAdditional, project, monthFrom, monthTo);
            var response = await _http.GetAsync<List<DepartmentIncomeAdditionalRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(response);
            return ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var url = BuildIncomeUrl(FpsApiEndpoints.GetDepartmentIncomeTotals, project, monthFrom, monthTo);
            var response = await _http.GetAsync<List<DepartmentIncomeTotalsRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(response);
            return ApiResponseDto<List<DepartmentIncomeTotalsDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync(double? accntsPeriod = null)
        {
            var url = accntsPeriod.HasValue
                ? QueryHelpers.AddQueryString(FpsApiEndpoints.GetDepartmentIncomePeriods,
                    new Dictionary<string, string?> { ["accntsPeriod"] = accntsPeriod.Value.ToString() })
                : FpsApiEndpoints.GetDepartmentIncomePeriods;

            var response = await _http.GetAsync<List<PeriodLookupRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<PeriodLookupDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<PeriodLookupDto>>>(response);
            return ApiResponseDto<List<PeriodLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<PeriodSnapshotDto>>> GetSnapshotPeriodsAsync()
        {
            var response = await _http.GetAsync<List<PeriodSnapshotRes>>(FpsApiEndpoints.GetDepartmentIncomeSnapshotPeriods);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<PeriodSnapshotDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<PeriodSnapshotDto>>>(response);
            return ApiResponseDto<List<PeriodSnapshotDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> UpdatePeriodLockedAsync(string periodName, bool periodLocked)
        {
            var url = QueryHelpers.AddQueryString(FpsApiEndpoints.UpdateDepartmentIncomeSnapshotPeriodLock, "periodName", periodName);
            var response = await _http.PutAsync<bool, bool>(url, periodLocked);
            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(true);
            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeCurrentAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var url = BuildIncomeUrl(FpsApiEndpoints.GetDepartmentIncomeCurrentTime, project, monthFrom, monthTo);
            var response = await _http.GetAsync<List<DepartmentIncomeTimeRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTimeDto>>>(response);
            return ApiResponseDto<List<DepartmentIncomeTimeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeCurrentAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var url = BuildIncomeUrl(FpsApiEndpoints.GetDepartmentIncomeCurrentTests, project, monthFrom, monthTo);
            var response = await _http.GetAsync<List<DepartmentIncomeTestRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTestDto>>>(response);
            return ApiResponseDto<List<DepartmentIncomeTestDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeCurrentAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var url = BuildIncomeUrl(FpsApiEndpoints.GetDepartmentIncomeCurrentAnimals, project, monthFrom, monthTo);
            var response = await _http.GetAsync<List<DepartmentIncomeAnimalRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeAnimalDto>>>(response);
            return ApiResponseDto<List<DepartmentIncomeAnimalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeCurrentAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var url = BuildIncomeUrl(FpsApiEndpoints.GetDepartmentIncomeCurrentAdditional, project, monthFrom, monthTo);
            var response = await _http.GetAsync<List<DepartmentIncomeAdditionalRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>>(response);
            return ApiResponseDto<List<DepartmentIncomeAdditionalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsCurrentAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
        {
            var url = BuildIncomeUrl(FpsApiEndpoints.GetDepartmentIncomeCurrentTotals, project, monthFrom, monthTo);
            var response = await _http.GetAsync<List<DepartmentIncomeTotalsRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<DepartmentIncomeTotalsDto>>>(response);
            return ApiResponseDto<List<DepartmentIncomeTotalsDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
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

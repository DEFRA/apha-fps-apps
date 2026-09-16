using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProjectYearCostsApiClient : IPimsProjectYearCostsApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsProjectYearCostsApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalActualsAsync(
            string project, short year, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetAdditionalActuals, project, year), query);
            var response = await _http.GetAsync<List<AdditionalCostRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);
            return ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalPlansAsync(
            string project, short year, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetAdditionalPlans, project, year), query);
            var response = await _http.GetAsync<List<AdditionalCostRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);
            return ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<AnimalCostDto>>> GetAnimalActualsAsync(
            string project, short year, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetAnimalActuals, project, year), query);
            var response = await _http.GetAsync<List<AnimalCostRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(response);
            return ApiResponseDto<List<AnimalCostDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<AnimalCostDto>>> GetAnimalPlansAsync(
            string project, short year, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetAnimalPlans, project, year), query);
            var response = await _http.GetAsync<List<AnimalCostRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(response);
            return ApiResponseDto<List<AnimalCostDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<TestCostDto>>> GetTestPlansAsync(
            string project, short year, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetTestPlans, project, year), query);
            var response = await _http.GetAsync<List<TestCostRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestCostDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestCostDto>>>(response);
            return ApiResponseDto<List<TestCostDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<TestCostDto>>> GetTestActualsAsync(
            string project, short year, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetTestActuals, project, year), query);
            var response = await _http.GetAsync<List<TestCostRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestCostDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestCostDto>>>(response);
            return ApiResponseDto<List<TestCostDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<StaffCostDto>>> GetStaffPlansAsync(
            string project, short year, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetStaffPlans, project, year), query);
            var response = await _http.GetAsync<List<StaffCostRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(response);
            return ApiResponseDto<List<StaffCostDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<StaffCostDto>>> GetStaffActualsAsync(
            string project, short year, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetStaffActuals, project, year), query);
            var response = await _http.GetAsync<List<StaffCostRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(response);
            return ApiResponseDto<List<StaffCostDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectYearDetailsDto>> GetProjectYearDetailsAsync(
            string project, short year)
        {
            string url = string.Format(PimsApiEndpoints.GetProjectYearDetails, project, year);
            var response = await _http.GetAsync<ProjectYearDetailsRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectYearDetailsDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectYearDetailsDto>>(response);
            return ApiResponseDto<ProjectYearDetailsDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<PactPayDto>>> GetPactPayAsync(
            string project, short year, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetPactPay, project, year), query);
            var response = await _http.GetAsync<List<PactPayRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<PactPayDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<PactPayDto>>>(response);
            return ApiResponseDto<List<PactPayDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<MonthlyPactDto>>> GetMonthlyPactDataAsync(
            string project, short year, QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetMonthlyPactData, project, year), query);
            var response = await _http.GetAsync<List<MonthlyPactRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyPactDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthlyPactDto>>>(response);
            return ApiResponseDto<List<MonthlyPactDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<FpsYearTotalsDto>> GetFpsYearTotalsAsync(string project, short year)
        {
            string url = string.Format(PimsApiEndpoints.GetFpsYearTotals, project, year);
            var response = await _http.GetAsync<FpsYearTotalsRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<FpsYearTotalsDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<FpsYearTotalsDto>>(response);
            return ApiResponseDto<FpsYearTotalsDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public Task<byte[]> ExportProjectYearCostsToExcelAsync(string project, short year)
        {
            string url = string.Format(PimsApiEndpoints.ExportProjectYearCostsToExcel, project, year);
            return _http.GetFileAsync(url);
        }
    }
}

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
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsProjectYearCostsApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalActualsAsync(
            string project, short year, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(
                    string.Format(PimsApiEndpoints.GetAdditionalActuals, project, year), query);
                var response = await _http.GetAsync<List<AdditionalCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);
                return ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve additional actuals", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalPlansAsync(
            string project, short year, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(
                    string.Format(PimsApiEndpoints.GetAdditionalPlans, project, year), query);
                var response = await _http.GetAsync<List<AdditionalCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);
                return ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve additional plans", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<AnimalCostDto>>> GetAnimalActualsAsync(
            string project, short year, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(
                    string.Format(PimsApiEndpoints.GetAnimalActuals, project, year), query);
                var response = await _http.GetAsync<List<AnimalCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(response);
                return ApiResponseDto<List<AnimalCostDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AnimalCostDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve animal actuals", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<AnimalCostDto>>> GetAnimalPlansAsync(
            string project, short year, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(
                    string.Format(PimsApiEndpoints.GetAnimalPlans, project, year), query);
                var response = await _http.GetAsync<List<AnimalCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<AnimalCostDto>>>(response);
                return ApiResponseDto<List<AnimalCostDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AnimalCostDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve animal plans", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TestCostDto>>> GetTestPlansAsync(
            string project, short year, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(
                    string.Format(PimsApiEndpoints.GetTestPlans, project, year), query);
                var response = await _http.GetAsync<List<TestCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestCostDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TestCostDto>>>(response);
                return ApiResponseDto<List<TestCostDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestCostDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test plans", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TestCostDto>>> GetTestActualsAsync(
            string project, short year, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(
                    string.Format(PimsApiEndpoints.GetTestActuals, project, year), query);
                var response = await _http.GetAsync<List<TestCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestCostDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TestCostDto>>>(response);
                return ApiResponseDto<List<TestCostDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestCostDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve test actuals", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<StaffCostDto>>> GetStaffPlansAsync(
            string project, short year, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(
                    string.Format(PimsApiEndpoints.GetStaffPlans, project, year), query);
                var response = await _http.GetAsync<List<StaffCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(response);
                return ApiResponseDto<List<StaffCostDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<StaffCostDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve staff plans", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<StaffCostDto>>> GetStaffActualsAsync(
            string project, short year, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(
                    string.Format(PimsApiEndpoints.GetStaffActuals, project, year), query);
                var response = await _http.GetAsync<List<StaffCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<StaffCostDto>>>(response);
                return ApiResponseDto<List<StaffCostDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<StaffCostDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve staff actuals", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectYearDetailsDto>> GetProjectYearDetailsAsync(
            string project, short year)
        {
            try
            {
                string url = string.Format(PimsApiEndpoints.GetProjectYearDetails, project, year);
                var response = await _http.GetAsync<ProjectYearDetailsRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectYearDetailsDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProjectYearDetailsDto>>(response);
                return ApiResponseDto<ProjectYearDetailsDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectYearDetailsDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve project year details", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<PactPayDto>>> GetPactPayAsync(
            string project, short year, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(
                    string.Format(PimsApiEndpoints.GetPactPay, project, year), query);
                var response = await _http.GetAsync<List<PactPayRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<PactPayDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<PactPayDto>>>(response);
                return ApiResponseDto<List<PactPayDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<PactPayDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve pact pay data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

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
    }
}

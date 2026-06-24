using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactMonthHourApiClient : IPactMonthHourApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactMonthHourApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<MonthHourDto>>> GetAllAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetAllMonthHours, query);
            var response = await _http.GetAsync<List<MonthHourRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(response);
            return ApiResponseDto<List<MonthHourDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<MonthHourDto>>> GetByYearAsync(short year)
        {
            var url = string.Format(PactApiEndpoints.GetMonthHoursByYear, year);
            var response = await _http.GetAsync<List<MonthHourRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(response);
            return ApiResponseDto<List<MonthHourDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<short>>> GetDistinctYearsAsync()
        {
            var response = await _http.GetAsync<List<short>>(PactApiEndpoints.GetDistinctMonthHourYears);
            if (response.Success)
                return new ApiResponseDto<List<short>> { Data = response.Data, Success = true };

            return ApiResponseDto<List<short>>.FailureResponse([], new ApiMetaDto
            {
                CorrelationId = response.Meta?.CorrelationId ?? Guid.NewGuid().ToString(),
                TimestampUtc = DateTime.UtcNow
            });
        }
    }
}

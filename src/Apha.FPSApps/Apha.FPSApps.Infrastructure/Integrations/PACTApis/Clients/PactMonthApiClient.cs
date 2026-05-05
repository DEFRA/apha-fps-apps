using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactMonthApiClient : IPactMonthApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactMonthApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<MonthDto>>> GetAllMonthsAsync()
        {
            var response = await _http.GetAsync<List<MonthRes>>(PactApiEndpoints.GetAllMonths);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthDto>>>(response);
            return ApiResponseDto<List<MonthDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

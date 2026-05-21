using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactCalenderMonthApiClient : IPactCalenderMonthApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactCalenderMonthApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<CalenderMonthDto>>> GetAllCalenderMonthsAsync()
        {
            var response = await _http.GetAsync<List<CalenderMonthRes>>(PactApiEndpoints.GetAllCalenderMonths);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<CalenderMonthDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<CalenderMonthDto>>>(response);
            return ApiResponseDto<List<CalenderMonthDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

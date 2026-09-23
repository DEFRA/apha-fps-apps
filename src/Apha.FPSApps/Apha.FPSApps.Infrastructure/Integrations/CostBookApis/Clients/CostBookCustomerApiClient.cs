using Apha.Common.Constants;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using MapsterMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookCustomerApiClient : ICostBookCustomerApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;       

        public CostBookCustomerApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<CustomerDto>>> GetAllCustomersAsync()
        {
           
                var response = await _http.GetAsync<List<CustomerRes>>(CostBookApiEndpoints.GetAllCustomers);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<CustomerDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<CustomerDto>>>(response);
                return ApiResponseDto<List<CustomerDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            
        }
    }
}

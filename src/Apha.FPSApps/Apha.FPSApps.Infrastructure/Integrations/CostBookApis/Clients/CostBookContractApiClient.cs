using Apha.Common.Constants;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using MapsterMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookContractApiClient : ICostBookContractApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;

        public CostBookContractApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ContractDto>>> GetAllContractNumbersAsync()
        {
            var response = await _http.GetAsync<List<ContractRes>>(CostBookApiEndpoints.GetAllContracts);

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<ContractDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ContractDto>>>(response);
            return ApiResponseDto<List<ContractDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}

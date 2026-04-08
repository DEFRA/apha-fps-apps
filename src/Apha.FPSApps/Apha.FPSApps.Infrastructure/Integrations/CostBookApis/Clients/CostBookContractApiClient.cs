using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            try
            {
                var response = await _http.GetAsync<List<ContractRes>>("api/projects/contracts");

                if (response.Success && response.Data != null)
                {
                    return _mapper.Map<ApiResponseDto<List<ContractDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<ContractDto>>>(response);
                    return ApiResponseDto<List<ContractDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve contracts",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<List<ContractDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }
    }
}

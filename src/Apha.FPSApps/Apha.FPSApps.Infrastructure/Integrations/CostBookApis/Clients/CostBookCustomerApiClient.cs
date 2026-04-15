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
            try
            {
                var response = await _http.GetAsync<List<CustomerRes>>("api/projects/customers");

                if (response.Success && response.Data != null)
                {
                    return _mapper.Map<ApiResponseDto<List<CustomerDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<CustomerDto>>>(response);
                    return ApiResponseDto<List<CustomerDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve customers",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<List<CustomerDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }
    }
}

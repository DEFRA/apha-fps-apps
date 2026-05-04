using Apha.Common.Constants;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookStaffApiClient : ICostBookStaffApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public CostBookStaffApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<StaffDto>>> GetAllStaffAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<StaffRes>>(CostBookApiEndpoints.GetAllStaff);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<StaffDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<StaffDto>>>(response);
                return ApiResponseDto<List<StaffDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception ex)
            {
                return ApiResponseDto<List<StaffDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve staff", Code = InternalCodeError, Details = ex.Message }],
                    new ApiMetaDto());
            }
        }
    }
}

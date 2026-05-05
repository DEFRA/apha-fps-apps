using Apha.Common.Constants;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookProgramApiClient : ICostBookProgramApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public CostBookProgramApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProgramDto>>> GetAllProgramsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProgramRes>>(CostBookApiEndpoints.GetAllPrograms);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<ProgramDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProgramDto>>>(response);
                return ApiResponseDto<List<ProgramDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception ex)
            {
                return ApiResponseDto<List<ProgramDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve programs", Code = InternalCodeError, Details = ex.Message }],
                    new ApiMetaDto());
            }
        }
    }
}

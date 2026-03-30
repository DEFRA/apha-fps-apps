using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProjectApiClient : IFpsProjectApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsProjectApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProgramAsync(
            QueryParameters<string> query, string programNo)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(
                    $"api/project/paged?programNo={Uri.EscapeDataString(programNo)}", query);

                var response = await _http.GetAsync<List<ProjectRes>>(url);

                if (response.Success)
                {
                    return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
                }

                var responseDto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
                return ApiResponseDto<List<ProjectDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                var apiErrorsDto = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Message = "Failed to retrieve projects",
                        Code = InternalCodeError,
                        Details = null
                    }
                };
                return ApiResponseDto<List<ProjectDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }
    }
}

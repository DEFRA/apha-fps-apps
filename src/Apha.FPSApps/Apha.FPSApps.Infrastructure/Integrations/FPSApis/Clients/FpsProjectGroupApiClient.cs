using Apha.Common.Constants;
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
    public class FpsProjectGroupApiClient : IFpsProjectGroupApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsProjectGroupApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectGroupDto>>> GetAllProjectGroupsAsync()
        {
            var response = await _http.GetAsync<List<ProjectGroupRes>>(FpsApiEndpoints.GetAllProjectGroups);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(response);
            return ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsByUserAsync()
        {
            var response = await _http.GetAsync<List<ProjectGroupRes>>(FpsApiEndpoints.GetProjectGroupsByUser);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(response);
            return ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProjectGroupAsync(
            QueryParameters<string> query, string projectGroup)
        {
            var url = QueryStringHelper.AddQueryString(
                string.Format(FpsApiEndpoints.GetProjectsByProjectGroup, Uri.EscapeDataString(projectGroup)), query);

            var response = await _http.GetAsync<List<ProjectRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
            return ApiResponseDto<List<ProjectDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}

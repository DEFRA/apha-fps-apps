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
        private const string BaseEndpoint = "api/project";

        public FpsProjectApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetAllProjectsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProjectRes>>(BaseEndpoint);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
                return ApiResponseDto<List<ProjectDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve projects", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetPagedProjectsAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"{BaseEndpoint}/paged", query);
                var response = await _http.GetAsync<List<ProjectRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
                return ApiResponseDto<List<ProjectDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged projects", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetPagedPactProjectsAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"{BaseEndpoint}/pactview", query);
                var response = await _http.GetAsync<List<ProjectRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
                return ApiResponseDto<List<ProjectDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged projects", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string parentProject)
        {
            try
            {
                var response = await _http.GetAsync<ProjectRes>($"{BaseEndpoint}/{Uri.EscapeDataString(parentProject)}");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectDto>> CreateProjectAsync(ProjectDto project)
        {
            try
            {
                var request = _mapper.Map<ProjectReq>(project);
                var response = await _http.PostAsync<ProjectReq, ProjectRes>(BaseEndpoint, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(ProjectDto project)
        {
            try
            {
                var request = _mapper.Map<ProjectReq>(project);
                var response = await _http.PutAsync<ProjectReq, ProjectRes>(BaseEndpoint, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectDto>> UpdatePactProjectAsync(ProjectDto project)
        {
            try
            {
                var request = _mapper.Map<ProjectReq>(project);
                var response = await _http.PatchAsync<ProjectReq, ProjectRes>($"{BaseEndpoint}/external/pact", request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteProjectAsync(string parentProject)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>($"{BaseEndpoint}/{Uri.EscapeDataString(parentProject)}");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete project", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
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


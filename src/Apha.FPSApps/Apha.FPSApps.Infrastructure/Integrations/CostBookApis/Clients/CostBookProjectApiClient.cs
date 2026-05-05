using Apha.Common.Constants;
using Apha.Common.Contracts.Costbook;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookProjectApiClient : ICostBookProjectApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public CostBookProjectApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetFilteredProjectsAsync(QueryParameters<string> criteria)
        {
            var url = QueryStringHelper.AddQueryString(CostBookApiEndpoints.GetFilteredProjects, criteria);
            var response = await _http.GetAsync<List<ProjectRes>>(url);

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
            return ApiResponseDto<List<ProjectDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string id)
        {
            var response = await _http.GetAsync<ProjectRes>(
                string.Format(CostBookApiEndpoints.GetProjectById, HttpUtility.UrlEncode(id)));

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> AddProjectAsync(ProjectDto project)
        {
          
                var request = _mapper.Map<ProjectReq>(project);
                var response = await _http.PostAsync<ProjectReq, ProjectRes>(CostBookApiEndpoints.AddProject, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                return ApiResponseDto<ProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
           
        }

        public async Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(string id, ProjectDto project)
        {
            var request = _mapper.Map<ProjectReq>(project);
            var response = await _http.PutAsync<ProjectReq, ProjectRes>(
                string.Format(CostBookApiEndpoints.UpdateProject, HttpUtility.UrlEncode(id)), request);

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteProjectAsync(string id)
        {
            var response = await _http.DeleteAsync<bool?>(
                string.Format(CostBookApiEndpoints.DeleteProject, HttpUtility.UrlEncode(id)));

            if (response.Success && response.Data.HasValue)
                return ApiResponseDto<bool>.SuccessResponse(response.Data.Value);

            if (response.Success && !response.Data.HasValue)
                return ApiResponseDto<bool>.SuccessResponse(true);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> CopyProjectAsync(string id, string newId)
        {
            var response = await _http.PostAsync<string, ProjectRes>(
                string.Format(CostBookApiEndpoints.CopyProject, HttpUtility.UrlEncode(id)), newId);

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> RecostProjectAsync(string id)
        {
            var response = await _http.PostAsync<object, bool>(
                string.Format(CostBookApiEndpoints.RecostProject, HttpUtility.UrlEncode(id)), new { });

            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(response.Data);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<string>> GetNextProjectNumberAsync(string? baseNumber)
        {
            var query = !string.IsNullOrEmpty(baseNumber)
                ? $"{CostBookApiEndpoints.GetNextProjectNumber}?baseNumber={HttpUtility.UrlEncode(baseNumber)}"
                : CostBookApiEndpoints.GetNextProjectNumber;

            var response = await _http.GetAsync<string>(query);

            if (response.Success && response.Data != null)
                return ApiResponseDto<string>.SuccessResponse(response.Data);

            var responseDto = _mapper.Map<ApiResponseDto<string>>(response);
            return ApiResponseDto<string>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}

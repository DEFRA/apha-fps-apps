using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookProjectApiClient : ICostBookProjectApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;

        public CostBookProjectApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetFilteredProjectsAsync(QueryParameters<string> criteria)
        {
           
                // Build query string using FPS pattern
                var baseUrl = "api/projects/paginated";
                var url = QueryStringHelper.AddQueryString(baseUrl, criteria);

                // Add additional filter parameters
                var query = HttpUtility.ParseQueryString(url.Split('?').Length > 1 ? url.Split('?')[1] : string.Empty);
               
                var finalUrl = $"{baseUrl.Split('?')[0]}?{query}";
                var response = await _http.GetAsync<List<ProjectRes>>(finalUrl);

                if (response.Success && response.Data != null)
                {
                    return _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<ProjectDto>>>(response);
                    return ApiResponseDto<List<ProjectDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            
          
        }
        public async Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string id)
        {
           
                //var response = await _http.GetAsync<ProjectRes>($"api/projects/{id}");
                // URL encode the ID to handle special characters like forward slashes
                var encodedId = HttpUtility.UrlEncode(id);
                var response = await _http.GetAsync<ProjectRes>($"api/projects/{encodedId}");

                if (response.Success && response.Data != null)
                {
                    return _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                    return ApiResponseDto<ProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            
            
        }

        public async Task<ApiResponseDto<ProjectDto>> AddProjectAsync(ProjectDto project)
        {
            try
            {
                var request = _mapper.Map<ProjectReq>(project);
                var response = await _http.PostAsync<ProjectReq, ProjectRes>("api/projects", request);

                if (response.Success && response.Data != null)
                {
                    return _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                    return ApiResponseDto<ProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to add project",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<ProjectDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(string id, ProjectDto project)
        {
           
               
                var encodedId = HttpUtility.UrlEncode(id);
                var request = _mapper.Map<ProjectReq>(project);
                var response = await _http.PutAsync<ProjectReq, ProjectRes>($"api/projects/{encodedId}", request);

                if (response.Success && response.Data != null)
                {
                    return _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                    return ApiResponseDto<ProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            
            
        }

        

        public async Task<ApiResponseDto<bool>> DeleteProjectAsync(string id)
        {           
                // URL encode the ID to handle special characters like forward slashes
                var encodedId = HttpUtility.UrlEncode(id);
                var response = await _http.DeleteAsync<bool?>($"api/projects/{encodedId}/delete");


                if (response.Success && response.Data.HasValue)
                {
                    return ApiResponseDto<bool>.SuccessResponse(response.Data.Value);
                }
                else if (response.Success && !response.Data.HasValue)
                {
                    // Handle null Data in successful response
                    return ApiResponseDto<bool>.SuccessResponse(true);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                    return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }            
          
        }

        public async Task<ApiResponseDto<ProjectDto>> CopyProjectAsync(string id, string newId)
        {
            
                // URL encode the ID to handle special characters like forward slashes
                var encodedId = HttpUtility.UrlEncode(id);
                var response = await _http.PostAsync<string, ProjectRes>($"api/projects/{encodedId}/copy", newId);

                if (response.Success && response.Data != null)
                {
                    return _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
                    return ApiResponseDto<ProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            
           
        }

        public async Task<ApiResponseDto<bool>> RecostProjectAsync(string id)
        {
             
                var encodedId = HttpUtility.UrlEncode(id);
                var response = await _http.PostAsync<object, bool>($"api/projects/{encodedId}/recost", new { });

                if (response.Success)
                {
                    return ApiResponseDto<bool>.SuccessResponse(response.Data);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                    return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            
           
        }

        public async Task<ApiResponseDto<string>> GetNextProjectNumberAsync(string? baseNumber)
        {
           
                //var query = !string.IsNullOrEmpty(baseNumber) ? $"?baseNumber={baseNumber}" : "";
                var query = !string.IsNullOrEmpty(baseNumber) ? $"?baseNumber={HttpUtility.UrlEncode(baseNumber)}" : "";
                var response = await _http.GetAsync<string>($"api/projects/number{query}");

                if (response.Success && response.Data != null)
                {
                    return ApiResponseDto<string>.SuccessResponse(response.Data);
                }
                //var response = await _http.GetAsync<ApiResponse<string>>($"api/projects/number{query}"); // ✅ Changed type

                //if (response.Success && response.Data != null && response.Data.Success)
                //{
                //    return ApiResponseDto<string>.SuccessResponse(response.Data.Data); // ✅ Extract nested data
                //}
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<string>>(response);
                    return ApiResponseDto<string>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }
            
           
        }
    }
}

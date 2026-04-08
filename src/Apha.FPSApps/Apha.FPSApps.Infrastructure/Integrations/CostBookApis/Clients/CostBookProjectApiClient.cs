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

        //public async Task<ApiResponseDto<ProjectPagedResultDto>> GetProjectsAsync(ProjectFilterDto filter)
        //{
        //    try
        //    {
        //        var query = HttpUtility.ParseQueryString(string.Empty);

        //        if (!string.IsNullOrEmpty(filter.ContractFilter))
        //            query["contractFilter"] = filter.ContractFilter;
        //        if (!string.IsNullOrEmpty(filter.SubmittedByFilter))
        //            query["submittedByFilter"] = filter.SubmittedByFilter;
        //        if (!string.IsNullOrEmpty(filter.SearchTerm))
        //            query["searchTerm"] = filter.SearchTerm;
        //        if (filter.Year > 0)
        //            query["year"] = filter.Year.ToString();

        //        query["page"] = filter.Page.ToString();
        //        query["pageSize"] = filter.PageSize.ToString();

        //        // Use GetPaginatedAsync with enhanced error handling
        //        //var response = await _http.GetPaginatedAsync<List<ProjectDto>>($"api/projects?{query}");

        //        //if (response.Success && response.Data != null)
        //        //{
        //        //    // Filter out any projects with null data that might cause issues
        //        //    var validProjects = response.Data.Where(p => p != null).ToList();

        //        //    var pagedResult = new ProjectPagedResultDto
        //        //    {
        //        //        Projects = validProjects,
        //        //        TotalRecords = response.Pagination?.TotalRecords ?? 0,
        //        //        TotalPages = response.Pagination?.TotalPages ?? 0,
        //        //        CurrentPage = response.Pagination?.PageNumber ?? 1,
        //        //        PageSize = response.Pagination?.PageSize ?? filter.PageSize
        //        //    };

        //        //    return ApiResponseDto<ProjectPagedResultDto>.SuccessResponse(pagedResult);
        //        //}

        //        // FIX: Change from List<ProjectDto> to List<ProjectRes>
        //        var response = await _http.GetPaginatedAsync<List<ProjectRes>>($"api/projects?{query}");

        //        if (response.Success && response.Data != null)
        //        {
        //            // Map ProjectRes to ProjectDto using AutoMapper
        //            var projectDtos = _mapper.Map<List<ProjectDto>>(response.Data);

        //            var pagedResult = new ProjectPagedResultDto
        //            {
        //                Projects = projectDtos,
        //                TotalRecords = response.Pagination?.TotalRecords ?? projectDtos.Count,
        //                TotalPages = response.Pagination?.TotalPages ?? 1,
        //                CurrentPage = response.Pagination?.PageNumber ?? 1,
        //                PageSize = response.Pagination?.PageSize ?? filter.PageSize
        //            };

        //            return ApiResponseDto<ProjectPagedResultDto>.SuccessResponse(pagedResult);
        //        }
        //        else
        //        {
        //            //var apiErrors = response.Errors?.Select(e => new ApiErrorDto
        //            //{
        //            //    Message = e.Message,
        //            //    Code = e.Code
        //            //}).ToList() ?? new List<ApiErrorDto>();

        //            // Return empty result instead of failure
        //            var emptyResult = new ProjectPagedResultDto
        //            {
        //                Projects = new List<ProjectDto>(),
        //                TotalRecords = 0,
        //                TotalPages = 1,
        //                CurrentPage = filter.Page,
        //                PageSize = filter.PageSize
        //            };

        //            //return ApiResponseDto<ProjectPagedResultDto>.FailureResponse(apiErrors, new ApiMetaDto());
        //            return ApiResponseDto<ProjectPagedResultDto>.SuccessResponse(emptyResult);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Return empty result with proper error handling
        //        var emptyResult = new ProjectPagedResultDto
        //        {
        //            Projects = new List<ProjectDto>(),
        //            TotalRecords = 0,
        //            TotalPages = 1,
        //            CurrentPage = filter.Page,
        //            PageSize = filter.PageSize
        //        };

        //        var apiErrors = new List<ApiErrorDto>
        //{
        //    new ApiErrorDto
        //    {
        //        Message = "Failed to retrieve projects",
        //        Code = "INTERNAL_ERROR",
        //        Details = ex.Message
        //    }
        //};

        //        return ApiResponseDto<ProjectPagedResultDto>.FailureResponse(apiErrors, new ApiMetaDto());
        //    }
        //}
        public async Task<ApiResponseDto<List<ProjectDto>>> GetFilteredProjectsAsync(QueryParameters<string> criteria)
        {
            try
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
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                    new ApiErrorDto {
                        Message = "Failed to retrieve projects",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<List<ProjectDto>>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }
        public async Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string id)
        {
            try
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
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve project",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<ProjectDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
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
            try
            {
                //var request = _mapper.Map<ProjectReq>(project);
                //var response = await _http.PutAsync<ProjectReq, ProjectRes>($"api/projects/{id}", request);
                // URL encode the ID to handle special characters like forward slashes
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
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to update project",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<ProjectDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        

        public async Task<ApiResponseDto<bool>> DeleteProjectAsync(string id)
        {
            try
            {
                // URL encode the ID to handle special characters like forward slashes
                var encodedId = HttpUtility.UrlEncode(id);
                var response = await _http.DeleteAsync<bool>($"api/projects/{encodedId}/delete");

               
                if (response.Success)
                {
                    return ApiResponseDto<bool>.SuccessResponse(true);
                }
                else
                {
                    var errors = response.Errors != null
                        ? _mapper.Map<List<ApiErrorDto>>(response.Errors)
                        : new List<ApiErrorDto> { new ApiErrorDto { Message = "Delete operation failed", Code = "DELETE_FAILED" } };

                    var meta = response.Meta != null
                        ? _mapper.Map<ApiMetaDto>(response.Meta)
                        : new ApiMetaDto();

                    return ApiResponseDto<bool>.FailureResponse(errors, meta);
                }
            }
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to delete project",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<bool>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectDto>> CopyProjectAsync(string id, string newId)
        {
            try
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
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to copy project",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<ProjectDto>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> RecostProjectAsync(string id)
        {
            try
            {
                //var response = await _http.PostAsync<object, bool>($"api/projects/{id}/recost", new { });
                // URL encode the ID to handle special characters like forward slashes
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
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to recost project",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<bool>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<string>> GetNextProjectNumberAsync(string? baseNumber)
        {
            try
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
            catch (Exception ex)
            {
                var apiErrorsDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to get next project number",
                        Code = "INTERNAL_ERROR",
                        Details = ex.Message
                    }
                };
                return ApiResponseDto<string>.FailureResponse(apiErrorsDto, new ApiMetaDto());
            }
        }
    }
}

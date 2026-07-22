/*
 * TRANSFORMENGINE MIGRATION — PimsProjectCommentApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - string? topic parameter added to GetCommentsByProjectAsync — forwarded as optional
 *     query string parameter to backend GET /api/v1/projectcomment?project&year&topic
 *   - topic appended via QueryStringHelper.AddQueryString(url, new { project, year, topic })
 *     so null/empty values are omitted automatically
 *   - All HTTP calls wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - private readonly fields _http and _mapper (S2933)
 *   - private const string InternalCodeError (S1192)
 *
 * PRESERVED:
 *   - All 5 CRUD + 1 lookup methods: GetCommentsByProjectAsync, GetByIdAsync, CreateCommentAsync,
 *     UpdateCommentAsync, DeleteCommentAsync, GetCommentTopicsAsync
 *   - URL constants delegated to PimsApiEndpoints static class (GetCommentsByProject,
 *     GetCommentById, CreateComment, UpdateComment, DeleteComment, GetCommentTopics)
 *   - Mapper used for all success response mappings (not manual construction)
 *   - Namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProjectCommentApiClient : IPimsProjectCommentApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsProjectCommentApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: topic parameter added — forwarded as optional query string parameter to backend GET /api/v1/projectcomment
        public async Task<ApiResponseDto<List<CommentDto>>> GetCommentsByProjectAsync(string project, int? year, string? topic, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentsByProject, query);
                url = QueryStringHelper.AddQueryString(url, new { project, year, topic });
                var response = await _http.GetAsync<List<CommentRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<CommentDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<CommentDto>>>(response);
                return ApiResponseDto<List<CommentDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<CommentDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve comments", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<CommentDto>> GetByIdAsync(int commentno)
        {
            try
            {
                var response = await _http.GetAsync<CommentRes>(string.Format(PimsApiEndpoints.GetCommentById, commentno));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<CommentDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<CommentDto>>(response);
                return ApiResponseDto<CommentDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CommentDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve comment", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<CommentDto>> CreateCommentAsync(CommentDto dto)
        {
            try
            {
                CommentReq request = _mapper.Map<CommentReq>(dto);
                var response = await _http.PostAsync<CommentReq, CommentRes>(PimsApiEndpoints.CreateComment, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<CommentDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<CommentDto>>(response);
                return ApiResponseDto<CommentDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CommentDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create comment", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<CommentDto>> UpdateCommentAsync(int commentno, CommentDto dto)
        {
            try
            {
                CommentReq request = _mapper.Map<CommentReq>(dto);
                var response = await _http.PutAsync<CommentReq, CommentRes>(string.Format(PimsApiEndpoints.UpdateComment, commentno), request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<CommentDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<CommentDto>>(response);
                return ApiResponseDto<CommentDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CommentDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update comment", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteCommentAsync(int commentno)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>(string.Format(PimsApiEndpoints.DeleteComment, commentno));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete comment", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<CommentTopicDto>>> GetCommentTopicsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<CommentTopicRes>>(PimsApiEndpoints.GetCommentTopics);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<CommentTopicDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<CommentTopicDto>>>(response);
                return ApiResponseDto<List<CommentTopicDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<CommentTopicDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve comment topics", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}

/*
 * TRANSFORMENGINE MIGRATION — IPimsProjectCommentApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - Added string? topic parameter to GetCommentsByProjectAsync to match updated backend
 *     GET /api/v1/projectcomment?project&year&topic endpoint (Phase 5 update)
 *   - Added TransformEngine migration annotation header
 *
 * PRESERVED:
 *   - All existing method signatures: GetByIdAsync, CreateCommentAsync, UpdateCommentAsync,
 *     DeleteCommentAsync, GetCommentTopicsAsync
 *   - Return types wrapped in ApiResponseDto<T>
 *   - CommentTopicDto used as dedicated lookup DTO for GetCommentTopicsAsync (not reusing CommentDto)
 *   - QueryParameters<string> for paginated list method
 *   - Namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsProjectCommentApiClient
    {
        // TRANSFORMENGINE: topic parameter added — matches GET /api/v1/projectcomment?project&year&topic
        //   backend route updated in Phase 5; topic is optional filter sourced from filterTopic dropdown
        Task<ApiResponseDto<List<CommentDto>>> GetCommentsByProjectAsync(string project, int? year, string? topic, QueryParameters<string> query);

        // TRANSFORMENGINE: GET /api/v1/projectcomment/{commentno} — retrieve single comment by PK
        Task<ApiResponseDto<CommentDto>> GetByIdAsync(int commentno);

        // TRANSFORMENGINE: POST /api/v1/projectcomment — create new comment
        Task<ApiResponseDto<CommentDto>> CreateCommentAsync(CommentDto dto);

        // TRANSFORMENGINE: PUT /api/v1/projectcomment/{commentno} — update existing comment
        Task<ApiResponseDto<CommentDto>> UpdateCommentAsync(int commentno, CommentDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/projectcomment/{commentno} — delete comment by PK
        Task<ApiResponseDto<bool>> DeleteCommentAsync(int commentno);

        // TRANSFORMENGINE: GET /api/v1/projectcomment/commenttopics — lookup endpoint for filterTopic dropdown
        Task<ApiResponseDto<List<CommentTopicDto>>> GetCommentTopicsAsync();
    }
}

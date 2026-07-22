/*
 * TRANSFORMENGINE MIGRATION — ProjectCommentService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access DAO/ADODB project comment queries → thin frontend service delegate
 *   - Injects IPimsApiClient; all methods forward to _client.PimsProjectComment
 *   - GetCommentsByProjectAsync: string? topic parameter forwarded to API client to
 *     match updated backend GET /api/v1/projectcomment?project&year&topic route
 *   - GetCommentTopicsAsync: lookup delegate added for topic dropdown population
 *   - _client field is private readonly (S2933 compliance)
 *
 * PRESERVED:
 *   - No business logic — pure thin-delegate pattern enforced
 *   - All IProjectCommentService contract methods: GetCommentsByProjectAsync,
 *     GetByIdAsync, CreateCommentAsync, UpdateCommentAsync, DeleteCommentAsync,
 *     GetCommentTopicsAsync
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class ProjectCommentService : IProjectCommentService
    {
        private readonly IPimsApiClient _client;

        public ProjectCommentService(IPimsApiClient client)
        {
            _client = client;
        }

        // TRANSFORMENGINE: topic parameter added — forwarded to IPimsProjectCommentApiClient to match updated backend route
        public async Task<ApiResponseDto<List<CommentDto>>> GetCommentsByProjectAsync(string project, int? year, string? topic, QueryParameters<string> query)
            => await _client.PimsProjectComment.GetCommentsByProjectAsync(project, year, topic, query);

        public async Task<ApiResponseDto<CommentDto>> GetByIdAsync(int commentno)
            => await _client.PimsProjectComment.GetByIdAsync(commentno);

        public async Task<ApiResponseDto<CommentDto>> CreateCommentAsync(CommentDto dto)
            => await _client.PimsProjectComment.CreateCommentAsync(dto);

        public async Task<ApiResponseDto<CommentDto>> UpdateCommentAsync(int commentno, CommentDto dto)
            => await _client.PimsProjectComment.UpdateCommentAsync(commentno, dto);

        public async Task<ApiResponseDto<bool>> DeleteCommentAsync(int commentno)
            => await _client.PimsProjectComment.DeleteCommentAsync(commentno);

        public async Task<ApiResponseDto<List<CommentTopicDto>>> GetCommentTopicsAsync()
            => await _client.PimsProjectComment.GetCommentTopicsAsync();
    }
}

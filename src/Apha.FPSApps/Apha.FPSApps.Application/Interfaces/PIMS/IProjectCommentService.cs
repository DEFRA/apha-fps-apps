/*
 * TRANSFORMENGINE MIGRATION — IProjectCommentService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access DAO/ADODB project comment queries → thin frontend service interface
 *   - Interface mirrors IPimsProjectCommentApiClient signatures for MVC controller injection
 *   - GetCommentsByProjectAsync: string? topic parameter added to match updated backend
 *     GET /api/v1/projectcomment?project&year&topic route
 *   - GetCommentTopicsAsync: lookup method added for topic dropdown population
 *
 * PRESERVED:
 *   - All method signatures visible to the MVC controller (IProjectCommentService contract)
 *   - commentno (int) as primary key type for GetById, Update, Delete
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify CommentDto and CommentTopicDto field names match
 *     the final backend DTO definitions in Apha.PIMS if those are ever modified
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PIMS
{
    public interface IProjectCommentService
    {
        // TRANSFORMENGINE: topic parameter added — matches updated backend GET /api/v1/projectcomment?project&year&topic
        Task<ApiResponseDto<List<CommentDto>>> GetCommentsByProjectAsync(string project, int? year, string? topic, QueryParameters<string> query);
        Task<ApiResponseDto<CommentDto>> GetByIdAsync(int commentno);
        Task<ApiResponseDto<CommentDto>> CreateCommentAsync(CommentDto dto);
        Task<ApiResponseDto<CommentDto>> UpdateCommentAsync(int commentno, CommentDto dto);
        Task<ApiResponseDto<bool>> DeleteCommentAsync(int commentno);
        Task<ApiResponseDto<List<CommentTopicDto>>> GetCommentTopicsAsync();
    }
}

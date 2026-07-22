/*
 * TRANSFORMENGINE MIGRATION — CommentTopicDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - Added TransformEngine migration annotation header
 *   - DTO verified to mirror Apha.PIMS.Application.Dtos.CommentTopicDto and CommentTopicRes shape
 *
 * PRESERVED:
 *   - Single Topic property (required string) matching tlkpCommentTopics PK VARCHAR 25 NOT NULL
 *   - required modifier matching NOT NULL constraint
 *   - Namespace Apha.FPSApps.Application.Dtos.PIMS
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
namespace Apha.FPSApps.Application.Dtos.PIMS
{
    // TRANSFORMENGINE: Lookup DTO for tlkpCommentTopics — mirrors CommentTopicRes from backend
    //   used by IPimsProjectCommentApiClient.GetCommentTopicsAsync() → filterTopic dropdown
    public class CommentTopicDto
    {
        public required string Topic { get; set; }
    }
}

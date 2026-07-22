/*
 * TRANSFORMENGINE MIGRATION — CommentTopicRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - tlkpCommentTopics lookup table → dedicated lookup response contract
 *   - Added TransformEngine migration annotation header and inline field comment
 *
 * PRESERVED:
 *   - Topic required string property; required annotation matches NOT NULL PK constraint
 *   - Namespace Apha.Common.Contracts.PIMS
 *
 * DEFERRED: none — fully automated.
 */

namespace Apha.Common.Contracts.PIMS
{
    public class CommentTopicRes
    {
        // TRANSFORMENGINE: Topic — tlkpCommentTopics.Topic (VARCHAR 25 NOT NULL, PK);
        //   drives the filterTopic dropdown in the standalone Comments page and the
        //   GET /api/v1/projectcomment/commenttopics lookup endpoint.
        public required string Topic { get; set; }
    }
}

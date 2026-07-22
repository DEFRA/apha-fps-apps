/*
 * TRANSFORMENGINE MIGRATION — Comment.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access / SQL Server table `mabarchive.tblcomments` → C# entity class
 *   - Column `comment` (text) renamed to property `CommentText` (EF column mapping in CommentMap.cs preserves DB name)
 *   - Column `madeby` (char(20)) exposed as nullable string; max-length enforced in EF map
 *   - `commentno` uses auto-generated sequence (configured in CommentMap.cs)
 *
 * PRESERVED:
 *   - All 7 DDL column nullability semantics (project/year/topic NOT NULL; dateentered/comment/madeby nullable)
 *   - `year` typed as `short` to match PostgreSQL smallint
 *   - Unique constraint (project, year, topic) captured in ExistsAsync on ICommentRepository
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
using System;
using System.Collections.Generic;

namespace Apha.PIMS.Core.Entities
{

    public partial class Comment
    {
        public int CommentNo { get; set; }

        public string Project { get; set; } = null!;

        public short Year { get; set; }

        public DateTime? DateEntered { get; set; }

        public string Topic { get; set; } = null!;

        public string? CommentText { get; set; }
        public string? MadeBy { get; set; }
    }
}

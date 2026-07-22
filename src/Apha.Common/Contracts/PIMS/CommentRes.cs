/*
 * TRANSFORMENGINE MIGRATION — CommentRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - tblComments RecordSource columns → typed API response contract
 *   - Added TransformEngine migration annotation header and inline field comments
 *
 * PRESERVED:
 *   - All existing property names and types (CommentNo, Project, Year, Topic, Comment,
 *     MadeBy, DateEntered, CommentText)
 *   - Nullable annotations aligning with SQL-nullable source columns
 *   - Namespace Apha.Common.Contracts.PIMS
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: CommentText appears to duplicate the Comment property; no
 *     CommentText column exists in tblComments. Confirm which property the EntityMapper
 *     populates and remove the redundant alias once validated.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Contracts.PIMS
{
    public class CommentRes
    {
        // TRANSFORMENGINE: CommentNo — tblComments.CommentNo IDENTITY PK (INT NOT NULL);
        //   used by frontend grid for row-level edit/delete operations.
        public int CommentNo { get; set; }

        // TRANSFORMENGINE: Project — tblComments.Project (VARCHAR 20 NOT NULL).
        public string? Project { get; set; }

        // TRANSFORMENGINE: Year — tblComments.Year (SMALLINT NOT NULL).
        public int? Year { get; set; }

        // TRANSFORMENGINE: Topic — tblComments.Topic (VARCHAR 25 NOT NULL); FK to
        //   tlkpCommentTopics. Exposed here for filterTopic display and grid column.
        public string? Topic { get; set; }

        // TRANSFORMENGINE: Comment — tblComments.Comment (TEXT); primary comment body.
        public string? Comment { get; set; }

        // TRANSFORMENGINE: MadeBy — tblComments.MadeBy (CHAR 20); set by SQL trigger
        //   UI_tblComments (suser_sname()); modal-commentMadeBy display.
        public string? MadeBy { get; set; }

        // TRANSFORMENGINE: DateEntered — tblComments.DateEntered (DATETIME); set by SQL
        //   trigger UI_tblComments (GetDate()); read-only in the response.
        public DateTime? DateEntered { get; set; }

        // TRANSFORMENGINE TODO: CommentText has no corresponding column in tblComments.
        //   Likely an alias populated from Comment by EntityMapper. Confirm the mapping
        //   and remove the redundant alias once validated.
        public string? CommentText { get; set; }
    }
}

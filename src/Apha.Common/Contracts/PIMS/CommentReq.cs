/*
 * TRANSFORMENGINE MIGRATION — CommentReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access frmtblComments modal input fields → writable backend request contract
 *   - tblComments table columns mapped to typed C# nullable properties
 *   - Added TransformEngine migration annotation header and inline field comments
 *
 * PRESERVED:
 *   - All existing property names and types (Project, Year, Topic, Comment, MadeBy,
 *     CommentNo, CommentText, DateEntered)
 *   - Nullable annotations matching source form field optionality
 *   - Namespace Apha.Common.Contracts.PIMS
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: CommentNo (PK identity) is present in the Req — verify the
 *     update endpoint uses route id; remove this field if RequestMapper does not need it
 *   - TRANSFORMENGINE TODO: CommentText appears to duplicate the Comment property; confirm
 *     which one the RequestMapper reads and drop the redundant alias
 *   - TRANSFORMENGINE TODO: DateEntered is trigger-managed (UI_tblComments sets
 *     DateEntered=GetDate() on INSERT/UPDATE) — client-supplied value is discarded on the
 *     server; verify the frontend does not rely on round-tripping this field
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Contracts.PIMS
{
    public class CommentReq
    {
        // TRANSFORMENGINE: Project — tblComments.Project (VARCHAR 20 NOT NULL); driven by
        //   the top-level commentProject dropdown in the standalone Comments page.
        public string? Project { get; set; }

        // TRANSFORMENGINE: Year — tblComments.Year (SMALLINT NOT NULL); modal-commentYear
        //   form field (name="year"), mandatory per HTML prototype.
        public int? Year { get; set; }

        // TRANSFORMENGINE: Topic — tblComments.Topic (VARCHAR 25 NOT NULL); FK to
        //   tlkpCommentTopics; selected via filterTopic dropdown and modal-commentYearTopic
        //   form field (name="yearTopic").
        public string? Topic { get; set; }

        // TRANSFORMENGINE: Comment — tblComments.Comment (TEXT); modal-commentText textarea
        //   (name="comment"), mandatory per HTML prototype.
        public string? Comment { get; set; }

        // TRANSFORMENGINE: MadeBy — tblComments.MadeBy (CHAR 20); modal-commentMadeBy
        //   form field (name="madeChangedBy"). Note: SQL trigger UI_tblComments also sets
        //   MadeBy = suser_sname() on INSERT/UPDATE, overriding any client value.
        public string? MadeBy { get; set; }

        // TRANSFORMENGINE TODO: CommentNo is the IDENTITY PK and should not be a writable
        //   Req field per Phase 1 contract rules. Retained to avoid breaking the existing
        //   RequestMapper update path. Verify the update endpoint uses the route id and
        //   remove if the mapper does not require a body id.
        public int CommentNo { get; set; }

        // TRANSFORMENGINE TODO: CommentText appears to be a redundant alias for the Comment
        //   property (no CommentText column in tblComments). Confirm which property the
        //   RequestMapper serialises and remove the duplicate.
        public string? CommentText { get; set; }

        // TRANSFORMENGINE TODO: DateEntered is auto-managed by SQL trigger UI_tblComments
        //   (SET DateEntered = GetDate()). Any client-supplied value is overwritten server-side.
        //   Verify the frontend does not depend on sending or receiving this field via Req.
        public DateTime? DateEntered { get; set; }
    }
}

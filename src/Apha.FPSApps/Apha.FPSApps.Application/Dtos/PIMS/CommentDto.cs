/*
 * TRANSFORMENGINE MIGRATION — CommentDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - Added TransformEngine migration annotation header
 *   - DTO verified to mirror Apha.PIMS.Application.Dtos.CommentDto + CommentRes shape
 *   - [Required] validation annotations present for mandatory modal fields (Project, Year, Topic)
 *
 * PRESERVED:
 *   - All properties: CommentNo, Project, Year, Topic, Comment, CommentText, MadeBy, DateEntered
 *   - Nullable reference types matching backend nullability
 *   - [Required] annotations on Project, Year, Topic (frontend validation)
 *   - Comment and CommentText both retained to match CommentRes two-alias shape for ApiDtoMapper compatibility
 *   - Namespace Apha.FPSApps.Application.Dtos.PIMS
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: CommentText duplicates the Comment alias (no tblComments.CommentText column).
 *     Once the backend CommentRes/EntityMapper alias is resolved, align this DTO to keep only
 *     the canonical property name.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class CommentDto
    {
        // TRANSFORMENGINE: CommentNo — PK from tblComments.CommentNo IDENTITY INT; used for edit/delete routing
        public int CommentNo { get; set; }

        // TRANSFORMENGINE: Project — tblComments.Project VARCHAR 20; sourced from commentProject dropdown on page
        [Required(ErrorMessage = "Project is required")]
        public string? Project { get; set; }

        // TRANSFORMENGINE: Year — tblComments.Year SMALLINT; modal-commentYear input (mandatory)
        [Required(ErrorMessage = "Year is required")]
        public int? Year { get; set; }

        // TRANSFORMENGINE: Topic — tblComments.Topic VARCHAR 25 FK to tlkpCommentTopics; modal-commentYearTopic
        [Required(ErrorMessage = "Topic is required")]
        public string? Topic { get; set; }

        // TRANSFORMENGINE: Comment — tblComments.Comment TEXT; modal-commentText textarea (primary comment body)
        public string? Comment { get; set; }

        // TRANSFORMENGINE TODO: CommentText is a redundant alias for Comment (no tblComments.CommentText column);
        //   retained to match CommentRes shape used by ApiDtoMapper — remove once backend alias is resolved
        public string? CommentText { get; set; }

        // TRANSFORMENGINE: MadeBy — tblComments.MadeBy CHAR 20; trigger-managed (suser_sname()) — display only
        public string? MadeBy { get; set; }

        // TRANSFORMENGINE: DateEntered — tblComments.DateEntered DATETIME; trigger-managed (GetDate()) — display only
        public DateTime? DateEntered { get; set; }
    }
}

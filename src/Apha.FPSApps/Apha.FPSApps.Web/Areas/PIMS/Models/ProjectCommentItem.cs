/*
 * TRANSFORMENGINE MIGRATION — ProjectCommentItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - Verified against HTML prototype frmtblComments.html grid container (#gridContainer_comments)
 *   - JS DataGridComponent columns: Year (number 80px), Topic (text 200px), Comments/Comment (text 400px)
 *   - CommentNo: hidden PK (IsVisible = false, ReadOnly) — NOT a visible JS column; used as KeyProperty for CRUD ops
 *   - Property names match CommentDto exactly: CommentNo, Year, Topic, Comment
 *
 * PRESERVED:
 *   - All 4 properties: CommentNo, Year, Topic, Comment
 *   - GridColumn attributes (Width, Type, IsVisible) matching HTML prototype column definitions
 *   - [Display(Name)] labels matching column header text from prototype
 *   - Namespace Apha.FPSApps.Web.Areas.PIMS.Models
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ProjectCommentItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int CommentNo { get; set; }

        [Display(Name = "Year")]
        [GridColumn(Width = 80, Type = GridColumnType.Number)]
        public int? Year { get; set; }

        [Display(Name = "Topic")]
        [GridColumn(Width = 200, Type = GridColumnType.Text)]
        public string? Topic { get; set; }

        [Display(Name = "Comments")]
        [GridColumn(Width = 400, Type = GridColumnType.Text)]
        public string? Comment { get; set; }
    }
}

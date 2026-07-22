/*
 * TRANSFORMENGINE MIGRATION — AddEditCommentViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - Verified against frmtblComments.html Add/Edit modal fields:
 *     modal-commentYear → Year (required int?),
 *     modal-commentYearTopic → Topic (required string?),
 *     modal-commentText textarea → CommentText (required string?),
 *     Project (hidden, set from page context), IsAddingNew (hidden, controls modal title/button)
 *   - [Required] annotations on Project, Year, Topic, CommentText matching * Mandatory Fields in prototype
 *   - YearOptions + TopicOptions for modal select dropdowns
 *   - TransformEngine migration header added
 *
 * PRESERVED:
 *   - All 7 properties: CommentNo, Project, Year, Topic, CommentText, IsAddingNew, YearOptions, TopicOptions
 *   - [Required] validation annotations
 *   - Namespace Apha.FPSApps.Web.Areas.PIMS.Models
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: CommentText is an alias for CommentDto.Comment (no tblComments.CommentText column);
 *     retained to match AddEditCommentViewModel ↔ CommentDto AutoMapper binding; resolve once CommentDto alias is cleaned up.
 */
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models;

public class AddEditCommentViewModel
{
    public int CommentNo { get; set; }
    [Required(ErrorMessage = "Project is required")]
    [Display(Name = "Project")]
    public string? Project { get; set; }
    [Required(ErrorMessage = "Year is required")]
    [Display(Name = "Year")]
    public int? Year { get; set; }
    [Required(ErrorMessage = "Topic is required")]
    [Display(Name = "Topic")]
    public string? Topic { get; set; } 
    [Required]
    public string? CommentText { get; set; }
    public bool IsAddingNew { get; set; }

    public List<SelectListItem> YearOptions { get; set; } = [];
    public List<SelectListItem> TopicOptions { get; set; } = [];
}

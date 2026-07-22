/*
 * TRANSFORMENGINE MIGRATION — CommentViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access frmtblComments form controls → ASP.NET Core MVC ViewModel for standalone Comments page
 *   - commentProject <select id="commentProject"> (outside grid container) → SelectedProject + ProjectOptions
 *   - forecastSpend <input> (read-only display field from sf_PCForecastSpend subform) → ForecastSpend double?
 *   - filterTopic <select id="filterTopic"> (outside grid, filter panel) → SelectedTopic + TopicOptions
 *   - filterYear <input type="text" id="filterYear"> (outside grid, filter panel) → SelectedYear string?
 *   - gridContainer_comments → CommentsGrid DataGridConfig<ProjectCommentItem>
 *   - YearOptions included for modal Year select (populated for GetAddEditCommentPartial)
 *
 * PRESERVED:
 *   - Property names align with CommentDto field names: Project → SelectedProject, Topic → SelectedTopic
 *   - Nullable types match optional filter semantics (user may not select all filters before loading)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ForecastSpend sourced from g_tlkpproject_radtrackdata.pcforecastspend
 *     via sf_PCForecastSpend MS Access subform. No frontend IProjectRadTrackDataService currently exists.
 *     Requires a new backend GET /api/v1/projectcomment/forecastspend?project={project} endpoint plus
 *     a corresponding frontend service method. Until resolved, ForecastSpend is null on page load.
 */
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class CommentViewModel
    {
        // TRANSFORMENGINE: SelectedProject — bound to commentProject <select id="commentProject"> outside grid container;
        //   page-level project selector; required for LoadCommentsGrid to call backend GET /api/v1/projectcomment?project=...
        public string? SelectedProject { get; set; }

        // TRANSFORMENGINE TODO STUB: ForecastSpend — from g_tlkpproject_radtrackdata.pcforecastspend via sf_PCForecastSpend subform.
        //   No frontend service exposes this field. Field renders null until IProjectRadTrackDataService is wired up.
        //   replace stub with real implementation when service is available
        public double? ForecastSpend { get; set; }

        // TRANSFORMENGINE: SelectedTopic — bound to filterTopic <select id="filterTopic"> in filter panel outside grid;
        //   optional topic filter passed to LoadCommentsGrid as query param
        public string? SelectedTopic { get; set; }

        // TRANSFORMENGINE: SelectedYear — bound to filterYear <input type="text" id="filterYear"> in filter panel;
        //   text input (not a dropdown); parsed to int? in LoadCommentsGrid for backend year filter
        public string? SelectedYear { get; set; }

        // TRANSFORMENGINE: ProjectOptions — commentProject dropdown; populated from IProjectListService.GetAllProjectsListAsync()
        //   Value = Parentproject, Text = Parentproject; includes "-- Select --" placeholder
        public List<SelectListItem> ProjectOptions { get; set; } = [];

        // TRANSFORMENGINE: TopicOptions — filterTopic dropdown; populated from IProjectCommentService.GetCommentTopicsAsync()
        //   (lookup DTO: CommentTopicDto.Topic used for both Value and Text)
        public List<SelectListItem> TopicOptions { get; set; } = [];

        // TRANSFORMENGINE: YearOptions — for modal Year select in _AddEditComment partial;
        //   populated from IProjectDetailsService.GetAllYearAsync() (same source as ProjectDetails embedded grid)
        public List<SelectListItem> YearOptions { get; set; } = [];

        // TRANSFORMENGINE: CommentsGrid — binds to /PIMS/Comment/LoadCommentsGrid via DataGrid component;
        //   explicitly built in CommentController.Index() — never left as default new()
        public DataGridConfig<ProjectCommentItem> CommentsGrid { get; set; } = new();
    }
}

/*
 * TRANSFORMENGINE MIGRATION — CommentController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access frmtblComments form → standalone ASP.NET Core MVC [Area("PIMS")] controller
 *   - Index(): builds CommentViewModel with Project dropdown (IProjectListService.GetAllProjectsListAsync),
 *     Topic dropdown (IProjectCommentService.GetCommentTopicsAsync), Year dropdown (IProjectDetailsService.GetAllYearAsync),
 *     and explicit DataGridConfig<ProjectCommentItem> — never left as new()
 *   - LoadCommentsGrid (POST): AJAX DataGrid reload with project + year + topic filters sourced from page controls;
 *     project is the required business context — guards against empty project with early-return empty grid
 *   - GetAddEditCommentPartial (GET): modal partial for add/edit; loads CommentTopics + YearOptions for selects;
 *     pre-populates on edit via IProjectCommentService.GetByIdAsync
 *   - CreateComment (POST): [FromBody] CommentDto, sets MadeBy from User.Identity.Name;
 *     [ValidateAntiForgeryToken] added (Phase 14 security fix — aligns with ProjectDetailsController convention)
 *   - UpdateComment (POST): [FromBody] CommentDto, sets MadeBy from User.Identity.Name;
 *     [ValidateAntiForgeryToken] added (Phase 14 security fix)
 *   - DeleteComment (DELETE): delete by CommentNo;
 *     [ValidateAntiForgeryToken] added (Phase 14 security fix — aligns with Invoice/ProjectDetails DELETE pattern)
 *   - GetComment (GET): returns single comment for modal pre-population
 *   - ForecastSpend stubbed — no frontend service currently exposes pcforecastspend from
 *     g_tlkpproject_radtrackdata; endpoint deferred (TRANSFORMENGINE TODO)
 *   - PopulateDropdownsAsync: uses CommentTopicDto.Topic (lookup DTO) for Topic dropdown — NOT CommentDto.Topic
 *   - AllowAdd = true, AllowEdit = true, AllowDelete = true: derived from HTML prototype modal Save/Update/Delete buttons
 *   - ExtraFilterMethod = "getCommentsExtraFilters" for project + year + topic filter passing from JS
 *
 * PRESERVED:
 *   - All CRUD action signatures mirroring ProjectDetailsController comment logic (GetCurrentUser, comment
 *     CommentText / Comment synchronisation, ModelState validation pattern)
 *   - Consistent error response shape: { success, errors } / { success, data, message }
 *   - [Authorize(Roles = "PIMSAdmin,PIMSUser")] + [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
 *   - Route area "PIMS"
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ForecastSpend (pcforecastspend from g_tlkpproject_radtrackdata) requires a new
 *     backend GET endpoint and frontend service method. Stub returns null until resolved.
 *   - TRANSFORMENGINE TODO: Verify pimscomments.js grid column definitions once JS prototype file is
 *     available — current column config derived from frmtblComments.html #gridContainer_comments structure.
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    // TRANSFORMENGINE: Standalone Comments page controller — separate from ProjectDetailsController.
    //   Matches frmtblComments.html prototype with Project selector, ForecastSpend display,
    //   Topic/Year filter panel, and full Add/Edit/Delete DataGrid.
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class CommentController : Controller
    {
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: IProjectCommentService — primary CRUD service for tblComments resource family.
        //   GetCommentsByProjectAsync (project + year + topic), GetByIdAsync, CreateCommentAsync,
        //   UpdateCommentAsync, DeleteCommentAsync, GetCommentTopicsAsync (lookup)
        private readonly IProjectCommentService _commentService;

        // TRANSFORMENGINE: IProjectListService — lookup only for Project dropdown population.
        //   GetAllProjectsListAsync() → ProjectOptions select list.
        private readonly IProjectListService _projectListService;

        // TRANSFORMENGINE: IProjectDetailsService — lookup only for Year dropdown population.
        //   GetAllYearAsync() → YearOptions select list (same source used in ProjectDetailsController).
        private readonly IProjectDetailsService _projectDetailsService;

        public CommentController(
            IMapper mapper,
            IProjectCommentService commentService,
            IProjectListService projectListService,
            IProjectDetailsService projectDetailsService)
        {
            _mapper = mapper;
            _commentService = commentService;
            _projectListService = projectListService;
            _projectDetailsService = projectDetailsService;
        }

        // ── Index ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            CommentViewModel viewModel = new();
            await PopulateDropdownsAsync(viewModel);

            // TRANSFORMENGINE: DataGridConfig<ProjectCommentItem> built EXPLICITLY — never left as new().
            //   AllowAdd = true: HTML prototype has tblCommentsModal Save button (showAddButton: true)
            //   AllowEdit = true: prototype has tblCommentsUpdateBtn (edit button in action column)
            //   AllowDelete = true: prototype has tblCommentsDeleteModal (delete confirm modal)
            viewModel.CommentsGrid = new DataGridConfig<ProjectCommentItem>
            {
                GridId             = "commentsGrid",
                Title              = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "CommentNo",       // PK: CommentNo (hidden column, IsVisible = false)
                AllowAdd           = true,              // from HTML prototype Save button
                AddFunction        = "addComment",
                AllowEdit          = true,              // from HTML prototype Update button
                EditFunction       = "editComment",
                AllowDelete        = true,              // from HTML prototype Delete confirm modal
                DeleteFunction     = "deleteComment",
                ExtraFilterMethod  = "getCommentsExtraFilters",  // passes selectedProject, selectedTopic, selectedYear from JS
                BindGridUrl        = "/PIMS/Comment/LoadCommentsGrid",
                Data               = new List<ProjectCommentItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<ProjectCommentItem>(null),
                Pagination         = new PaginationModel()
            };

            return View(viewModel);
        }

        // ── Dropdown Population ───────────────────────────────────────────────

        private async Task PopulateDropdownsAsync(CommentViewModel model)
        {
            // TRANSFORMENGINE: Project options — from IProjectListService (lookup flow, NOT CRUD flow);
            //   commentProject <select id="commentProject"> is OUTSIDE the grid container (per frmtblComments.html line 55-62)
            Task<ApiResponseDto<List<ProjectListViewDto>>> projectsTask =
                _projectListService.GetAllProjectsListAsync();

            // TRANSFORMENGINE: Topic options — from IProjectCommentService.GetCommentTopicsAsync (lookup DTO CommentTopicDto);
            //   filterTopic <select id="filterTopic"> is OUTSIDE the grid container (filter panel, line 84-96)
            Task<ApiResponseDto<List<CommentTopicDto>>> topicsTask =
                _commentService.GetCommentTopicsAsync();

            // TRANSFORMENGINE: Year options — from IProjectDetailsService.GetAllYearAsync (same source as ProjectDetails);
            //   used for modal Year select (YearOptions) — not a standalone page-level year dropdown
            Task<ApiResponseDto<List<YearDto>>> yearsTask =
                _projectDetailsService.GetAllYearAsync();

            await Task.WhenAll(projectsTask, topicsTask, yearsTask);

            // Project selector
            List<SelectListItem> projectOptions = [new SelectListItem("-- Select --", "")];
            if (projectsTask.Result is { Success: true, Data: not null })
            {
                projectOptions.AddRange(projectsTask.Result.Data
                    .Select(p => new SelectListItem(p.Parentproject, p.Parentproject)));
            }
            model.ProjectOptions = projectOptions;

            // Topic filter dropdown — uses lookup DTO property Topic for both Value and Text
            List<SelectListItem> topicOptions = [new SelectListItem("-- All topics --", "")];
            if (topicsTask.Result is { Success: true, Data: not null })
            {
                topicOptions.AddRange(topicsTask.Result.Data
                    .Select(t => new SelectListItem(t.Topic, t.Topic)));
            }
            model.TopicOptions = topicOptions;

            // Year options for modal select — ordered descending
            model.YearOptions = yearsTask.Result?.Data?
                .OrderByDescending(y => y.Value)
                .Select(y => new SelectListItem(y.Value.ToString(), y.Value.ToString()))
                .ToList() ?? [];
        }

        // ── DataGrid AJAX Reload ───────────────────────────────────────────────

        // TRANSFORMENGINE: LoadCommentsGrid — called by DataGrid ExtraFilterMethod "getCommentsExtraFilters"
        //   passing project (required page context), year (optional filter), and topic (optional filter)
        //   from the three explicit HTML controls outside the grid container.
        //   project MUST NOT be empty/null — backend GET /api/v1/projectcomment requires it.
        //   If project is empty, returns an empty DataGridConfig immediately (no backend call).
        [HttpPost]
        public async Task<IActionResult> LoadCommentsGrid(
            PaginationFilter<string> request,
            string? project = null,
            string? topic = null,
            string? year = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            DataGridConfig<ProjectCommentItem> gridConfig =
                await BuildCommentsGridAsync(request, project, topic, year);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProjectCommentItem>> BuildCommentsGridAsync(
            PaginationFilter<string> request,
            string? project,
            string? topic,
            string? year)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            // TRANSFORMENGINE: project is required business context — sourced from commentProject <select>
            //   outside grid. Guard: if project is empty, return empty grid (do NOT call backend with null project).
            if (string.IsNullOrWhiteSpace(project))
            {
                return new DataGridConfig<ProjectCommentItem>
                {
                    GridId             = "commentsGrid",
                    Title              = string.Empty,
                    ShowCheckboxColumn = false,
                    ShowPagination     = true,
                    KeyProperty        = "CommentNo",
                    AllowAdd           = true,
                    AddFunction        = "addComment",
                    AllowEdit          = true,
                    EditFunction       = "editComment",
                    AllowDelete        = true,
                    DeleteFunction     = "deleteComment",
                    ExtraFilterMethod  = "getCommentsExtraFilters",
                    BindGridUrl        = "/PIMS/Comment/LoadCommentsGrid",
                    Data               = new List<ProjectCommentItem>(),
                    Columns            = GridDataProvider.GetColumnsDefination<ProjectCommentItem>(null),
                    Pagination         = new PaginationModel(),
                    CurrentFilters     = filterDict
                };
            }

            // TRANSFORMENGINE: year filter — filterYear is a free-text input in the HTML prototype;
            //   parse to int? for backend year filter parameter
            int? parsedYear = int.TryParse(year, out int yr) ? yr : null;

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);

            // TRANSFORMENGINE: topic=null when SelectedTopic is empty string — backend accepts null as "no filter"
            string? topicFilter = string.IsNullOrWhiteSpace(topic) ? null : topic;

            ApiResponseDto<List<CommentDto>> pagedData =
                await _commentService.GetCommentsByProjectAsync(project, parsedYear, topicFilter, queryParameters);

            List<ProjectCommentItem> items = pagedData.Data is not null
                ? _mapper.Map<List<ProjectCommentItem>>(pagedData.Data)
                : new List<ProjectCommentItem>();

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ProjectCommentItem>
            {
                GridId             = "commentsGrid",
                Title              = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "CommentNo",
                AllowAdd           = true,
                AddFunction        = "addComment",
                AllowEdit          = true,
                EditFunction       = "editComment",
                AllowDelete        = true,
                DeleteFunction     = "deleteComment",
                ExtraFilterMethod  = "getCommentsExtraFilters",
                BindGridUrl        = "/PIMS/Comment/LoadCommentsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<ProjectCommentItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        // ── CRUD Endpoints ────────────────────────────────────────────────────

        // TRANSFORMENGINE: GetAddEditCommentPartial — GET, loads modal partial for add or edit.
        //   parentproject sourced from the page commentProject selector (passed as query param from JS).
        //   selectedYear pre-populates Year select on add. CommentNo pre-populates all fields on edit.
        [HttpGet]
        public async Task<IActionResult> GetAddEditCommentPartial(
            string? parentproject, int? commentNo, int? selectedYear)
        {
            AddEditCommentViewModel model = await LoadAddEditCommentViewModelAsync(
                parentproject, commentNo, selectedYear);

            if (commentNo is not null and not 0)
            {
                ApiResponseDto<CommentDto> result = await _commentService.GetByIdAsync(commentNo.Value);
                if (result is { Success: true, Data: not null })
                {
                    model.CommentNo   = result.Data.CommentNo;
                    model.Year        = result.Data.Year;
                    model.Topic       = result.Data.Topic;
                    model.CommentText = result.Data.CommentText;
                }
            }

            return PartialView("_AddEditComment", model);
        }

        private async Task<AddEditCommentViewModel> LoadAddEditCommentViewModelAsync(
            string? parentproject, int? commentNo, int? selectedYear)
        {
            // TRANSFORMENGINE: GetCommentTopicsAsync — uses lookup DTO CommentTopicDto (NOT CommentDto)
            //   for Topic dropdown; Topic property on CommentTopicDto is the lookup display text + value
            ApiResponseDto<List<CommentTopicDto>> topicsResult = await _commentService.GetCommentTopicsAsync();

            List<SelectListItem> topicOptions = [new SelectListItem("Select a topic", "")];
            if (topicsResult is { Success: true, Data: not null })
            {
                topicOptions.AddRange(topicsResult.Data
                    .Select(t => new SelectListItem(t.Topic, t.Topic)));
            }

            List<SelectListItem> yearOptions = await GetYearOptionsAsync();

            return new AddEditCommentViewModel
            {
                Project      = parentproject ?? string.Empty,
                IsAddingNew  = commentNo is null or 0,
                Year         = selectedYear,
                YearOptions  = yearOptions,
                TopicOptions = topicOptions
            };
        }

        private async Task<List<SelectListItem>> GetYearOptionsAsync()
        {
            ApiResponseDto<List<YearDto>> years = await _projectDetailsService.GetAllYearAsync();
            return years?.Data?
                .OrderByDescending(y => y.Value)
                .Select(y => new SelectListItem(y.Value.ToString(), y.Value.ToString()))
                .ToList() ?? [];
        }

        // TRANSFORMENGINE: GetComment — GET, returns single comment JSON for any JS modal pre-population
        [HttpGet]
        public async Task<IActionResult> GetComment(int commentNo)
        {
            ApiResponseDto<CommentDto> result = await _commentService.GetByIdAsync(commentNo);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment retrieved successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE: CreateComment — POST [FromBody] JSON; [ValidateAntiForgeryToken] added (Phase 14);
        //   token is read from the _AddEditComment partial's @Html.AntiForgeryToken() hidden field and sent
        //   as RequestVerificationToken header in saveComment() JS — aligns with ProjectDetailsController convention.
        //   MadeBy set server-side from User.Identity.Name (mirrors UI_tblComments trigger suser_sname())
        //   CommentText synchronized to Comment (tblComments.Comment is the primary column)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([FromBody] CommentDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .Select(ms => new { field = ms.Key, message = ms.Value!.Errors.First().ErrorMessage })
                    .ToList();
                return Json(new { success = false, errors });
            }

            dto.MadeBy      = GetCurrentUser();
            dto.CommentText = dto.Comment?.Trim();
            ApiResponseDto<CommentDto> result = await _commentService.CreateCommentAsync(dto);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment added successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE: UpdateComment — POST [FromBody] JSON; [ValidateAntiForgeryToken] added (Phase 14);
        //   MadeBy updated server-side on every save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment([FromBody] CommentDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .Select(ms => new { field = ms.Key, message = ms.Value!.Errors.First().ErrorMessage })
                    .ToList();
                return Json(new { success = false, errors });
            }

            dto.MadeBy = GetCurrentUser();
            ApiResponseDto<CommentDto> result = await _commentService.UpdateCommentAsync(dto.CommentNo, dto);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment updated successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE: DeleteComment — [HttpDelete]; [ValidateAntiForgeryToken] added (Phase 14);
        //   token sent as RequestVerificationToken header by deleteComment() JS in Index.cshtml;
        //   aligns with Invoice + ProjectDetails DELETE pattern.
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentNo)
        {
            ApiResponseDto<bool> result = await _commentService.DeleteCommentAsync(commentNo);
            return result.Success
                ? Json(new { success = true, message = "Comment deleted successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        // TRANSFORMENGINE TODO STUB: GetForecastSpend — reads pcforecastspend from g_tlkpproject_radtrackdata.
        //   No frontend IProjectRadTrackDataService currently exists. Returns null until backend endpoint + service are added.
        //   replace stub with real implementation
        [HttpGet]
        public IActionResult GetForecastSpend(string project)
        {
            // TRANSFORMENGINE TODO STUB: Requires new backend endpoint GET /api/v1/projectcomment/forecastspend?project={project}
            //   and matching frontend service method. Return null until wired up.
            return Json(new { success = true, forecastSpend = (double?)null });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string GetCurrentUser()
        {
            return User?.Identity?.Name ?? string.Empty;
        }
    }
}

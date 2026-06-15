using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectGroupStaffPlanController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectGroupStaffPlanService _staffPlanService;
        private readonly IAppStateService _appStateService;

        public ProjectGroupStaffPlanController(
            IMapper mapper,
            IProjectGroupStaffPlanService staffPlanService,
            IAppStateService appStateService)
        {
            _mapper = mapper;
            _staffPlanService = staffPlanService;
            _appStateService = appStateService;
        }

        /// <summary>
        /// Displays the project group staff plan summary page (fps.vpvtprojectgroupmgrplan).
        /// Pre-populates the ProjectGroup column filter from session and filters data accordingly.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var projectGroup = await _appStateService.GetSessionAsync<string>(SessionKeys.SelectedProjectGroup)
                ?? string.Empty;

            var grid = await BuildGridAsync(new PaginationFilter<string>(), projectGroup);

            return View(new ProjectGroupStaffPlanViewModel { Grid = grid });
        }

        /// <summary>
        /// Reloads the grid partial view based on pagination, sort, and filter parameters.
        /// The ProjectGroup column filter value is carried naturally in request.Filter.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<ProjectGroupStaffPlanViewItem>> BuildGridAsync(
            PaginationFilter<string> request, string? projectGroup = null)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            // Seed the ProjectGroup column filter from session on initial page load.
            // On subsequent reloads the value is carried inside request.Filter by the grid manager.
            if (!string.IsNullOrWhiteSpace(projectGroup) && !filterDict.ContainsKey("ProjectGroup"))
                filterDict["ProjectGroup"] = projectGroup;

            var query = _mapper.Map<QueryParameters<string>>(request);
            query.Filter = filterDict.Count > 0 ? JsonConvert.SerializeObject(filterDict) : null;

            var response = await _staffPlanService.GetPagedAsync(query);

            var rows = new List<ProjectGroupStaffPlanViewItem>();
            PaginationModel pagination = new();

            if (response.Success && response.Data != null)
            {
                rows = _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(response.Data);

                if (response.Pagination != null)
                {
                    pagination.PageNumber   = response.Pagination.PageNumber;
                    pagination.PageSize     = response.Pagination.PageSize;
                    pagination.TotalRecords = response.Pagination.TotalRecords;
                }
            }

            pagination.SortColumn    = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<ProjectGroupStaffPlanViewItem>
            {
                GridId         = "projectGroupStaffPlanGrid",
                KeyProperty    = "ParentProject",
                AllowAdd       = false,
                AllowEdit      = false,
                AllowDelete    = false,
                ShowPagination = true,
                BindGridUrl    = "/FPS/ProjectGroupStaffPlan/LoadGrid",
                Columns        = GridDataProvider.GetColumnsDefination<ProjectGroupStaffPlanViewItem>(),
                Data           = rows,
                CurrentFilters = filterDict,
                Pagination     = pagination
            };
        }
    }
}

using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
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

        public ProjectGroupStaffPlanController(IMapper mapper, IProjectGroupStaffPlanService staffPlanService)
        {
            _mapper = mapper;
            _staffPlanService = staffPlanService;
        }

        /// <summary>
        /// Displays the project group staff plan summary page (fps.vpvtprojectgroupmgrplan).
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var grid = await BuildGridAsync(new PaginationFilter<string>());
            return View(new ProjectGroupStaffPlanViewModel { Grid = grid });
        }

        /// <summary>
        /// Reloads the grid partial view based on pagination, sort, and filter parameters.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<ProjectGroupStaffPlanViewItem>> BuildGridAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
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

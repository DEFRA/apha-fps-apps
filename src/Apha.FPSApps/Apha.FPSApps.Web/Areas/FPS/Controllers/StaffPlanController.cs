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
    public class StaffPlanController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectStaffPlanService _staffPlanService;

        public StaffPlanController(IMapper mapper, IProjectStaffPlanService staffPlanService)
        {
            _mapper = mapper;
            _staffPlanService = staffPlanService;
        }

        /// <summary>
        /// Displays the staff plan summary page showing planned staff costs from fps.vprojectstaffplan.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var grid = await BuildGridAsync(new PaginationFilter<string>());
            return View(new StaffPlanViewModel { Grid = grid });
        }

        /// <summary>
        /// Reloads the staff plan grid partial view based on the supplied pagination, sort, and filter parameters.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<StaffPlanViewItem>> BuildGridAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _staffPlanService.GetPagedAsync(query);

            var rows = new List<StaffPlanViewItem>();
            PaginationModel pagination = new();

            if (response.Success && response.Data != null)
            {
                rows = _mapper.Map<List<StaffPlanViewItem>>(response.Data);

                if (response.Pagination != null)
                {
                    pagination.PageNumber   = response.Pagination.PageNumber;
                    pagination.PageSize     = response.Pagination.PageSize;
                    pagination.TotalRecords = response.Pagination.TotalRecords;
                }
            }

            pagination.SortColumn    = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<StaffPlanViewItem>
            {
                GridId         = "staffPlanGrid",
                KeyProperty    = "ParentProject",
                AllowAdd       = false,
                AllowEdit      = false,
                AllowDelete    = false,
                ShowPagination = true,
                BindGridUrl    = "/FPS/StaffPlan/LoadGrid",
                Columns        = GridDataProvider.GetColumnsDefination<StaffPlanViewItem>(),
                Data           = rows,
                CurrentFilters = filterDict,
                Pagination     = pagination
            };
        }
    }
}

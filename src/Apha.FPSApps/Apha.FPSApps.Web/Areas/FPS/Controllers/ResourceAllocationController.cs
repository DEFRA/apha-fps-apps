using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    /// <summary>
    /// MVC controller for Stage 2 Check Resource Allocation (frmResourceAllocation).
    /// Read-only view showing staff allocation and jobs grids for a selected workgroup grade.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ResourceAllocationController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IResourceAllocationService _ResourceAllocationService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupGradeService _workGroupGradeService;

        public ResourceAllocationController(
            IMapper mapper,
            IResourceAllocationService ResourceAllocationService,
            IProfitCentreService profitCentreService,
            IWorkGroupGradeService workGroupGradeService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _ResourceAllocationService = ResourceAllocationService ?? throw new ArgumentNullException(nameof(ResourceAllocationService));
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
            _workGroupGradeService = workGroupGradeService ?? throw new ArgumentNullException(nameof(workGroupGradeService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new ResourceAllocationViewModel
            {
                ResourceCentres = await PopulateResourceCentresAsync(),
                StaffAllocationGrid = BuildStaffAllocationGridConfig(new List<ResourceStaffAllocationItem>()),
                StaffJobsGrid = BuildStaffJobsGridConfig(new List<ResourceStaffJobItem>())
            };

            return View(viewModel);
        }

        /// <summary>
        /// Returns workgroup grades as JSON for a selected resource centre (used by the grade dropdown).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGradesByResourceCentre(string resourceCentre)
        {
            if (string.IsNullOrWhiteSpace(resourceCentre))
                return Json(new { success = false, message = "Resource Centre is required." });

            var response = await _workGroupGradeService.GetWorkgroupGradesByWorkGroupAsync(resourceCentre);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load grades." });

            var grades = (response.Data ?? new List<WorkgroupGradeDto>())
                .Select(g => new { value = g.WgGrade, text = g.WgGrade })
                .ToList();

            return Json(new { success = true, data = grades });
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupByResourceCentre(string resourceCentre)
        {
            if (string.IsNullOrWhiteSpace(resourceCentre))
                return Json(new { success = false, message = "Resource Centre is required." });

            var response = await _workGroupGradeService.GetWorkgroupGradesByWorkGroupAsync(resourceCentre);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load grades." });

            var grades = (response.Data ?? new List<WorkgroupGradeDto>())
                .Select(g => new { value = g.WgGrade, text = g.WgGrade })
                .ToList();

            return Json(new { success = true, data = grades });
        }

        /// <summary>
        /// Loads the staff allocation DataGrid for a given workgroup grade (supports pagination, sorting, filtering).
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadStaffAllocationGrid(PaginationFilter<string> request, [FromForm] string? workGroupGrade)
        {
            if (string.IsNullOrWhiteSpace(workGroupGrade))
                return PartialView("_DataGrid", BuildStaffAllocationGridConfig(new List<ResourceStaffAllocationItem>()));

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _ResourceAllocationService.GetPagedStaffAllocationsByWorkGroupGradeAsync(workGroupGrade, query);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staff allocations." });

            var items = (response.Data ?? new List<ResourceStaffAllocationDto>())
                .Select(d => _mapper.Map<ResourceStaffAllocationItem>(d))
                .ToList();

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return PartialView("_DataGrid", BuildStaffAllocationGridConfig(items, paginationModel, filterDict));
        }

        /// <summary>
        /// Returns column totals for all staff in a workgroup grade for the "Overall Position for the Grade" panel.
        /// Formulas mirror fsubResourceTotals2 in the original Access form.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStaffAllocationTotals(string workGroupGrade)
        {
            if (string.IsNullOrWhiteSpace(workGroupGrade))
                return Json(new { success = false, message = "WorkGroup Grade is required." });

            var query = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };
            var response = await _ResourceAllocationService.GetPagedStaffAllocationsByWorkGroupGradeAsync(workGroupGrade, query);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staff allocations." });

            var items = response.Data ?? new List<ResourceStaffAllocationDto>();

            double totalHrsAvail = items.Sum(i => i.HrsAvail ?? 0);
            double totalPlannedHours = items.Sum(i => i.PlannedHours);
            double totalAppChargeHours = items.Sum(i => i.AppChargeHours);
            double totalChargeHours = items.Sum(i => i.ChargeHours);

            string allocationPct = totalHrsAvail == 0 ? "" : FormatPct(totalPlannedHours / totalHrsAvail);
            string assuredUtilPct = totalHrsAvail == 0 ? "" : FormatPct(totalAppChargeHours / totalHrsAvail);
            string totalUtilPct = totalHrsAvail == 0 ? "" : FormatPct(totalChargeHours / totalHrsAvail);

            return Json(new
            {
                success = true,
                hrsAvail = totalHrsAvail,
                plannedHrs = totalPlannedHours,
                allocationPct,
                assuredChargeHrs = totalAppChargeHours,
                assuredUtilPct,
                totalChargeHrs = totalChargeHours,
                totalUtilPct
            });
        }

        private static string FormatPct(double value) =>
            (value * 100).ToString("0.##") + "%";

        /// <summary>
        /// Loads the jobs DataGrid for a given staff member (supports pagination, sorting, filtering).
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadStaffJobsGrid(PaginationFilter<string> request, [FromForm] string? staffId)
        {
            if (string.IsNullOrWhiteSpace(staffId))
                return PartialView("_DataGrid", BuildStaffJobsGridConfig(new List<ResourceStaffJobItem>()));

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _ResourceAllocationService.GetPagedStaffJobDetailsByStaffIdAsync(staffId, query);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staff jobs." });

            var items = (response.Data ?? new List<ResourceStaffJobDetailDto>())
                .Select(d => _mapper.Map<ResourceStaffJobItem>(d))
                .ToList();

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return PartialView("_DataGrid", BuildStaffJobsGridConfig(items, paginationModel, filterDict));
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private static DataGridConfig<ResourceStaffAllocationItem> BuildStaffAllocationGridConfig(
            List<ResourceStaffAllocationItem> data,
            PaginationModel? pagination = null,
            Dictionary<string, string>? filters = null) =>
            new()
            {
                GridId = "StaffAllocationGrid",
                Title = "",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffId",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "OnStaffRowSelect",
                BindGridUrl = "/FPS/ResourceAllocation/LoadStaffAllocationGrid",
                ExtraFilterMethod = "GetStaffAllocationExtraFilters",
                Data = data,
                Columns = GridDataProvider.GetColumnsDefination<ResourceStaffAllocationItem>(),
                Pagination = pagination ?? new PaginationModel(),
                CurrentFilters = filters ?? new Dictionary<string, string>()
            };

        private static DataGridConfig<ResourceStaffJobItem> BuildStaffJobsGridConfig(
            List<ResourceStaffJobItem> data,
            PaginationModel? pagination = null,
            Dictionary<string, string>? filters = null) =>
            new()
            {
                GridId = "StaffJobsGrid",
                Title = "Jobs for Staff",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffId",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = false,
                BindGridUrl = "/FPS/ResourceAllocation/LoadStaffJobsGrid",
                ExtraFilterMethod = "GetStaffJobsExtraFilters",
                Data = data,
                Columns = GridDataProvider.GetColumnsDefination<ResourceStaffJobItem>(),
                Pagination = pagination ?? new PaginationModel(),
                CurrentFilters = filters ?? new Dictionary<string, string>()
            };

        private async Task<List<SelectListItem>> PopulateResourceCentresAsync()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProfitCentreId,
                        Text = p.ProfitCentreId
                    })
                    .ToList();
            }
            return new List<SelectListItem>();
        }
    }
}

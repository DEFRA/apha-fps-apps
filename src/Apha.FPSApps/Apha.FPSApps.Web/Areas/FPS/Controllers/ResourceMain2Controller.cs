using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    /// <summary>
    /// MVC controller for Stage 2 Check Resource Allocation (frmResourceMain2).
    /// Read-only view showing staff allocation and jobs grids for a selected workgroup grade.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ResourceMain2Controller : Controller
    {
        private readonly IMapper _mapper;
        private readonly IResourceMain2Service _resourceMain2Service;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupGradeService _workGroupGradeService;

        public ResourceMain2Controller(
            IMapper mapper,
            IResourceMain2Service resourceMain2Service,
            IProfitCentreService profitCentreService,
            IWorkGroupGradeService workGroupGradeService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _resourceMain2Service = resourceMain2Service ?? throw new ArgumentNullException(nameof(resourceMain2Service));
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
            _workGroupGradeService = workGroupGradeService ?? throw new ArgumentNullException(nameof(workGroupGradeService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new ResourceMain2ViewModel
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

        /// <summary>
        /// Loads the staff allocation DataGrid for a given workgroup grade.
        /// </summary>
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadStaffAllocationGrid(string workGroupGrade)
        {
            if (string.IsNullOrWhiteSpace(workGroupGrade))
                return Json(new { success = false, message = "Workgroup Grade is required." });

            var response = await _resourceMain2Service.GetStaffAllocationsByWorkGroupGradeAsync(workGroupGrade);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staff allocations." });

            var items = (response.Data ?? new List<ResourceStaffAllocationDto>())
                .Select(d => _mapper.Map<ResourceStaffAllocationItem>(d))
                .ToList();

            return PartialView("_DataGrid", BuildStaffAllocationGridConfig(items));
        }

        /// <summary>
        /// Loads the jobs DataGrid for a given staff member.
        /// </summary>
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadStaffJobsGrid(int staffId)
        {
            var response = await _resourceMain2Service.GetStaffJobsByStaffIdAsync(staffId);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staff jobs." });

            var items = (response.Data ?? new List<ResourceStaffJobDto>())
                .Select(d => _mapper.Map<ResourceStaffJobItem>(d))
                .ToList();

            return PartialView("_DataGrid", BuildStaffJobsGridConfig(items));
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private static DataGridConfig<ResourceStaffAllocationItem> BuildStaffAllocationGridConfig(
            List<ResourceStaffAllocationItem> data) =>
            new()
            {
                GridId = "rm2StaffAllocationGrid",
                Title = "Staff of this Grade",
                ShowCheckboxColumn = false,
                ShowPagination = false,
                KeyProperty = "StaffId",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "rm2OnStaffRowSelect",
                BindGridUrl = "/FPS/ResourceMain2/LoadStaffAllocationGrid",
                Data = data,
                Columns = GridDataProvider.GetColumnsDefination<ResourceStaffAllocationItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = new Dictionary<string, string>()
            };

        private static DataGridConfig<ResourceStaffJobItem> BuildStaffJobsGridConfig(
            List<ResourceStaffJobItem> data) =>
            new()
            {
                GridId = "rm2StaffJobsGrid",
                Title = "Jobs for Staff",
                ShowCheckboxColumn = false,
                ShowPagination = false,
                KeyProperty = "StaffId",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = false,
                BindGridUrl = "/FPS/ResourceMain2/LoadStaffJobsGrid",
                Data = data,
                Columns = GridDataProvider.GetColumnsDefination<ResourceStaffJobItem>(),
                Pagination = new PaginationModel(),
                CurrentFilters = new Dictionary<string, string>()
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

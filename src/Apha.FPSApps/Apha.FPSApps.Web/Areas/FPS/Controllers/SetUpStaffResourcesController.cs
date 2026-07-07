using Apha.FPSApps.Application.Dtos;
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

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class SetUpStaffResourcesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupEmployeeService _workGroupEmployeeService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupGradeService _workGroupGradeService;

        public SetUpStaffResourcesController(
            IMapper mapper,
            IWorkGroupEmployeeService workGroupEmployeeService,
            IProfitCentreService profitCentreService,
            IWorkGroupGradeService workGroupGradeService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _workGroupEmployeeService = workGroupEmployeeService ?? throw new ArgumentNullException(nameof(workGroupEmployeeService));
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
            _workGroupGradeService = workGroupGradeService ?? throw new ArgumentNullException(nameof(workGroupGradeService));
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? resourceCentre = null)
        {
            var resourceCentres = await PopulateResourceCentresAsync();
            var selectedRc = resourceCentre ?? string.Empty;

            var viewModel = new SetUpStaffResourcesViewModel
            {
                ResourceCentres = resourceCentres,
                SelectedResourceCentre = selectedRc
            };

            if (!string.IsNullOrWhiteSpace(selectedRc))
            {
                var gradesResponse = await _workGroupGradeService.GetWorkGroupGradeAsync(selectedRc);
                if (gradesResponse.Success && gradesResponse.Data != null)
                {
                    viewModel.GradeList = gradesResponse.Data.Select(g => g.WgGrade).ToList();
                }
            }

            viewModel.StaffGrid = new DataGridConfig<SetUpStaffResourcesItem>
            {
                GridId = "ssrStaffGrid",
                Title = "Staff",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "PactId",
                AllowAdd = false,
                AllowEdit = true,
                EditFunction = "editSsrStaff",
                AllowDelete = false,
                BindGridUrl = "/FPS/SetUpStaffResources/LoadStaffGrid",
                Data = new List<SetUpStaffResourcesItem>(),
                Columns = GridDataProvider.GetColumnsDefination<SetUpStaffResourcesItem>(),
                Pagination = new PaginationModel()
            };

            return View(viewModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadStaffGrid(PaginationFilter<string> request, string wgGrade)
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

            if (string.IsNullOrWhiteSpace(wgGrade))
            {
                return Json(new { success = false, message = "WG Grade is required." });
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _workGroupEmployeeService.GetWorkGroupEmployeeForStaffAsync(queryParameters, wgGrade);

            if (!response.Success)
            {
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staff data." });
            }

            var rawData = response.Data ?? new List<WorkGroupEmployeeStaffDto>();
            var staffItems = rawData.Select(d => _mapper.Map<SetUpStaffResourcesItem>(d)).ToList();

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = new DataGridConfig<SetUpStaffResourcesItem>
            {
                GridId = "ssrStaffGrid",
                Title = "Staff",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "PactId",
                AllowAdd = false,
                AllowEdit = true,
                EditFunction = "editSsrStaff",
                AllowDelete = false,
                BindGridUrl = "/FPS/SetUpStaffResources/LoadStaffGrid",
                Data = staffItems,
                Columns = GridDataProvider.GetColumnsDefination<SetUpStaffResourcesItem>(),
                Pagination = paginationModel
            };

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> GetGradesByResourceCentre(string resourceCentre)
        {
            if (string.IsNullOrWhiteSpace(resourceCentre))
            {
                return Json(new { success = false, message = "Resource Centre is required." });
            }

            var response = await _workGroupGradeService.GetWorkGroupGradeAsync(resourceCentre);
            if (!response.Success)
            {
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load grades." });
            }

            var grades = (response.Data ?? new List<WorkgroupGradeDto>())
                .Select(g => new { value = g.WgGrade, text = $"{g.WgGrade} - {g.Workgroup}" })
                .ToList();

            return Json(new { success = true, data = grades });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string pactId)
        {
            if (string.IsNullOrWhiteSpace(pactId))
            {
                return Json(new { success = false, message = "PACT ID is required." });
            }

            var response = await _workGroupEmployeeService.GetWorkGroupEmployeeByIdForStaffAsync(pactId);
            if (!response.Success || response.Data == null)
            {
                return NotFound();
            }

            var item = _mapper.Map<SetUpStaffResourcesItem>(response.Data);
            return PartialView("_EditStaffModal", item);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] WorkGroupEmployeeStaffDto model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Staff data is required." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var response = await _workGroupEmployeeService.UpdateWorkGroupEmployeeForStaffAsync(model);
            if (response.Success)
            {
                return Json(new { success = true, data = response.Data, message = "Staff record updated successfully." });
            }

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to update staff record.",
                errors = (response.Errors ?? new List<ApiErrorDto>())
                    .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
            });
        }

        private async Task<List<SelectListItem>> PopulateResourceCentresAsync()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProfitCentreId,
                        Text = $"{p.ProfitCentreId} - {p.ProfitCentreName}"
                    })
                    .ToList();
            }
            return new List<SelectListItem>();
        }
    }
}

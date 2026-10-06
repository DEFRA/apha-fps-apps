using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Globalization;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class CostCentreMaintenanceController : Controller
    {
        private readonly IMapper _mapper;

        private readonly ICostCentreService _costCentreService;
        private readonly IProfitCentreService _profitCentreService;

        public CostCentreMaintenanceController(IMapper mapper, ICostCentreService costCentreService, IProfitCentreService profitCentreService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _costCentreService = costCentreService ?? throw new ArgumentNullException(nameof(costCentreService));
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
        }

        // ── Index ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var viewModel = new CostCentreMaintenanceViewModel();
            
            var defaultRequest = new PaginationFilter<string>
            {
                Filter = "{}"
            };
            viewModel.CostCentreGrid = await GetCostCentreGridConfigAsync(defaultRequest);

            await PopulateDropdownsAsync(viewModel);

            return View(viewModel);
        }

        private async Task PopulateDropdownsAsync(CostCentreMaintenanceViewModel model)
        {
            model.ProfitCentreList = await GetProfitCentreListAsync();
        }

        private async Task<List<SelectListItem>> GetProfitCentreListAsync()
        {
            var lookupResult = await _profitCentreService.GetAllProfitCentresAsync();
            if (!lookupResult.Success || lookupResult.Data == null)
            {
                return new List<SelectListItem>();
            }

            return lookupResult.Data
                .Where(item => !string.IsNullOrWhiteSpace(item.ProfitCentreId))
                .Select(item => item.ProfitCentreId!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p)
                .Select(p => new SelectListItem { Value = p, Text = p })
                .ToList();
        }

        // ── DataGrid AJAX Reload ──────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadCostCentreGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message  = "Invalid request data",
                    errors   = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetCostCentreGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<CostCentreItem>> GetCostCentreGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var pagedData = await _costCentreService.GetAllCostCentresPagedAsync(queryParameters);

            var items = new List<CostCentreItem>();
            if (pagedData.Data != null)
            {
                items = _mapper.Map<List<CostCentreItem>>(pagedData.Data);
            }

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<CostCentreItem>
            {
                GridId             = "costcenterGrid",
                Title              = "Cost Centres Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "CostCentreNo",
                AllowAdd           = true,
                AddFunction        = "addCostCentre",
                AllowEdit          = true,
                EditFunction       = "editCostCentre",
                AllowDelete        = true,
                DeleteFunction     = "deleteCostCentre",
                BindGridUrl        = "/FPS/CostCentreMaintenance/LoadCostCentreGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<CostCentreItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        // ── CRUD — Create ─────────────────────────────────────────────────────

        // Create GET returns the _AddEditCostCentre partial with a populated ProfitCentre dropdown
        // sourced from the strongly-typed model (CostCentreItem.ProfitCentreList).
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CostCentreItem
            {
                ProfitCentreList = await GetProfitCentreListAsync()
            };
            return PartialView("_AddEditCostCentre", model);
        }

        // No [ValidateAntiForgeryToken] — endpoint receives JSON body, not form post
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CostCentreDto dto)
        {
            if (dto is null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message  = "Please correct the errors below.",
                    errors   = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field   = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            var result = await _costCentreService.CreateCostCentreAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Cost Centre created successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create cost centre.";
            return Json(new
            {
                success = false,
                message  = errorMessage,
                errors   = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field   = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD — Edit ───────────────────────────────────────────────────────

        // id supplied as query string; culture-invariant parse prevents decimal separator issues
        // Edit GET returns the _AddEditCostCentre partial with a populated ProfitCentre dropdown
        // sourced from the strongly-typed model (CostCentreItem.ProfitCentreList).
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Cost Centre number is required" });
            }

            if (!double.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out var costCentreNo))
            {
                return Json(new { success = false, message = $"Invalid Cost Centre number: '{id}'" });
            }

            var result = await _costCentreService.GetCostCentreByIdAsync(costCentreNo);

            if (result.Success && result.Data != null)
            {
                var item = _mapper.Map<CostCentreItem>(result.Data);
                item.ProfitCentreList = await GetProfitCentreListAsync();
                return PartialView("_AddEditCostCentre", item);
            }

            return Json(new { success = false, message = $"Cost Centre '{id}' not found." });
        }

        // ICostCentreService.UpdateCostCentreAsync(double costCentreNo, CostCentreDto dto) signature
        [HttpPost]
        public async Task<IActionResult> Edit(string id, [FromBody] CostCentreDto dto)
        {
            if (dto is null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            if (!double.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out var costCentreNo))
            {
                return Json(new { success = false, message = $"Invalid Cost Centre number: '{id}'" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message  = "Please correct the errors below.",
                    errors   = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field   = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            // dto.CostCentreNo may differ if user edits the number (matches UpdateCostCentreAsync signature)
            var result = await _costCentreService.UpdateCostCentreAsync(costCentreNo, dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Cost Centre updated successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update cost centre.";
            return Json(new
            {
                success = false,
                message  = errorMessage,
                errors   = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field   = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD — Delete ─────────────────────────────────────────────────────

        // id supplied as query string from DataGrid delete button; culture-invariant parse
        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Cost Centre number is required" });
            }

            if (!double.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out var costCentreNo))
            {
                return Json(new { success = false, message = $"Invalid Cost Centre number: '{id}'" });
            }

            var result = await _costCentreService.DeleteCostCentreAsync(costCentreNo);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "Cost Centre deleted successfully" });
            }

            var firstError = result.Errors?.FirstOrDefault();
            var errorMessage = firstError?.Code == "DB_POSTGRES_ERROR"
                ? "This cost centre cannot be deleted because it is referenced by other records."
                : firstError?.Message ?? "Unable to delete the cost centre as it may be in use.";

            return Json(new { success = false, message = errorMessage });
        }
    }
}

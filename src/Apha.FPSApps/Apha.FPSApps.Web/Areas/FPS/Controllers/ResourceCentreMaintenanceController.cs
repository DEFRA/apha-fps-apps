using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Dtos;
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
    /// <summary>
    /// MVC controller for Resource Centre (Profit Centre) maintenance operations.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ResourceCentreMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IDivisionService _divisionService;
        private readonly IEmployeeService _employeeService;

        public ResourceCentreMaintenanceController(
            IMapper mapper,
            IProfitCentreService profitCentreService,
            IDivisionService divisionService,
            IEmployeeService employeeService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
            _divisionService = divisionService ?? throw new ArgumentNullException(nameof(divisionService));
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        }

        /// <summary>
        /// Displays the Resource Centre maintenance page with DataGrid.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var defaultRequest = new PaginationFilter<string>
            {
                Filter = "{}"
            };

            var gridConfig = await GetResourceCentreGridConfigAsync(defaultRequest);

            var viewModel = new ResourceCentreMaintenanceViewModel
            {
                ResourceCentreGrid = gridConfig
            };

            return View(viewModel);
        }

        /// <summary>
        /// Loads the Resource Centre grid via AJAX for pagination and filtering.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadResourceCentreGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetResourceCentreGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ResourceCentreMaintenanceItem>> GetResourceCentreGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _profitCentreService.GetAllProfitCentresPagedAsync(queryParameters);

            var items = new List<ResourceCentreMaintenanceItem>();
            if (pagedData.Data != null)
            {
                items = _mapper.Map<List<ResourceCentreMaintenanceItem>>(pagedData.Data);
            }

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ResourceCentreMaintenanceItem>
            {
                GridId = "resourceCentreGrid",
                Title = "Resource Centre Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "ProfitCentreId",
                AddFunction = "addResourceCentre",
                EditFunction = "editResourceCentre",
                DeleteFunction = "deleteResourceCentre",
                BindGridUrl = "/FPS/ResourceCentreMaintenance/LoadResourceCentreGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ResourceCentreMaintenanceItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        /// <summary>
        /// Displays the create resource centre modal.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            var model = new ResourceCentreMaintenanceItem
            {
                ProfitCentreId = string.Empty,
                ProfitCentreName = string.Empty,
                Division = string.Empty
            };
            return PartialView("_AddEditMaintResourceCentre", model);
        }

        /// <summary>
        /// Creates a new resource centre.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResourceCentreMaintenanceItem viewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            var dto = _mapper.Map<ProfitCentreDto>(viewModel);
            var result = await _profitCentreService.CreateProfitCentreAsync(dto);

            if (result.Success)
            {
                return Json(new { success =  true, data = result.Data, message = "Resource centre created successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create resource centre.";
            return Json(new
            {
                success = false,
                message = errorMessage,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Displays the edit resource centre modal.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string profitCentreId)
        {
            if (string.IsNullOrWhiteSpace(profitCentreId))
            {
                return Json(new { success = false, message = "Resource centre ID is required" });
            }

            var result = await _profitCentreService.GetProfitCentreByIdAsync(profitCentreId);
            if (result.Success && result.Data != null)
            {
                var itemViewModel = _mapper.Map<ResourceCentreMaintenanceItem>(result.Data);
                return PartialView("_AddEditMaintResourceCentre", itemViewModel);
            }

            return Json(new { success = false, message = $"Resource centre '{profitCentreId}' not found." });
        }

        /// <summary>
        /// Updates an existing resource centre.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ResourceCentreMaintenanceItem viewModel, [FromQuery] string? originalProfitCentreId = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            var identifyingId = !string.IsNullOrWhiteSpace(originalProfitCentreId) ? originalProfitCentreId : viewModel.ProfitCentreId;

            var dto = _mapper.Map<ProfitCentreDto>(viewModel);
            var result = await _profitCentreService.UpdateProfitCentreAsync(identifyingId, dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Resource centre updated successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update resource centre.";
            return Json(new
            {
                success = false,
                message = errorMessage,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Deletes a resource centre.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(string profitCentreId)
        {
            if (string.IsNullOrWhiteSpace(profitCentreId))
            {
                return Json(new { success = false, message = "Resource centre ID is required" });
            }

            var result = await _profitCentreService.DeleteProfitCentreAsync(profitCentreId);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "Resource centre deleted successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Unable to delete the resource centre as it is already in use.";
            return Json(new { success = false, message = errorMessage });
        }

        /// <summary>
        /// Gets distinct divisions for the dropdown.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDistinctDivisions()
        {
            try
            {
                var result = await _divisionService.GetAllDivisionsAsync();

                if (result.Success && result.Data != null)
                {
                    var divisions = result.Data
                        .Select(d => new { divisionId = d.DivisionId, divisionName = d.DivName ?? string.Empty })
                        .OrderBy(d => d.divisionName)
                        .ToList();

                    return Json(new { success = true, data = divisions });
                }

                return Json(new { success = false, message = "Failed to load divisions" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading divisions: {ex.Message}" });
            }
        }

        /// <summary>
        /// Gets managers for the RC Head dropdown.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetManagers()
        {
            var result = await _employeeService.GetAllManagersAsync();

            if (result.Success && result.Data != null)
            {
                var managers = result.Data
                    .Select(m => new { name = m.Name })
                    .OrderBy(m => m.name)
                    .ToList();
                return Json(new { success = true, data = managers });
            }

            return Json(new { success = false, message = "Failed to load managers" });
        }
    }
}

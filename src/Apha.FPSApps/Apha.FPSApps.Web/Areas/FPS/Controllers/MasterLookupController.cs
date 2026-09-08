using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    /// <summary>
    /// MVC controller for generic master lookup maintenance. Lists the registered lookup
    /// tables on the left and provides an inline add/edit/delete DataGrid on the right for
    /// the selected table. A single, table-agnostic pipeline serves every registered table.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class MasterLookupController : Controller
    {
        private readonly IMasterLookupService _masterLookupService;

        public MasterLookupController(IMasterLookupService masterLookupService)
        {
            _masterLookupService = masterLookupService ?? throw new ArgumentNullException(nameof(masterLookupService));
        }

        /// <summary>
        /// Displays the maintenance shell: left-side table list and right-side grid area.
        /// </summary>
        /// <param name="tableName">Optional table to pre-select and load.</param>
        public async Task<IActionResult> Index(string? tableName = null)
        {
            var tableNames = await GetTableNamesAsync();

            var selectedTable = !string.IsNullOrWhiteSpace(tableName)
                && tableNames.Contains(tableName, StringComparer.OrdinalIgnoreCase)
                ? tableName
                : null;

            var viewModel = new MasterLookupMaintenanceViewModel
            {
                TableNames = tableNames,
                SelectedTable = selectedTable,
                ItemGrid = selectedTable != null
                    ? await BuildItemGridAsync(selectedTable)
                    : null
            };

            return View(viewModel);
        }

        /// <summary>
        /// Loads the item grid for the selected table via AJAX, honouring the grid's
        /// pagination, search, sort and column-filter state.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadItemsGrid(PaginationFilter<string> request, string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Json(new { success = false, message = "Table name is required" });
            }

            if (!ModelState.IsValid)
            {
                return ValidationFailureJson();
            }

            var gridConfig = await BuildItemGridAsync(tableName, request);
            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Displays the create item modal.
        /// </summary>
        [HttpGet]
        public IActionResult Create(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Json(new { success = false, message = "Table name is required" });
            }

            ViewData["TableName"] = tableName;
            var model = new LookupItemViewModel();
            return PartialView("_AddEditLookupItem", model);
        }

        /// <summary>
        /// Displays the edit item modal.
        /// </summary>
        [HttpGet]
        public IActionResult Edit(string tableName, string value)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(value))
            {
                return Json(new { success = false, message = "Table name and value are required" });
            }

            ViewData["TableName"] = tableName;
            var model = new LookupItemViewModel { Value = value };
            return PartialView("_AddEditLookupItem", model);
        }

        /// <summary>
        /// Creates a new value in the selected lookup table.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(string tableName, [FromBody] LookupItemViewModel model)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return Json(new { success = false, message = "Table name is required" });
            }

            if (!ModelState.IsValid)
            {
                return ValidationFailureJson();
            }

            var result = await _masterLookupService.CreateLookupItemAsync(tableName, model.Value);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "Item created successfully" });
            }

            return FailureJson(result.Errors, "Failed to create item.");
        }

        /// <summary>
        /// Updates an existing value in the selected lookup table.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(string tableName, [FromBody] LookupItemViewModel model, [FromQuery] string originalValue)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(originalValue))
            {
                return Json(new { success = false, message = "Table name and original value are required" });
            }

            if (!ModelState.IsValid)
            {
                return ValidationFailureJson();
            }

            var result = await _masterLookupService.UpdateLookupItemAsync(tableName, originalValue, model.Value);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "Item updated successfully" });
            }

            return FailureJson(result.Errors, "Failed to update item.");
        }

        /// <summary>
        /// Deletes a value from the selected lookup table.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(string tableName, string value)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(value))
            {
                return Json(new { success = false, message = "Table name and value are required" });
            }

            var result = await _masterLookupService.DeleteLookupItemAsync(tableName, value);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "Item deleted successfully" });
            }

            return FailureJson(result.Errors, "Failed to delete item.");
        }

        /// <summary>
        /// Checks whether a value already exists in the selected lookup table (case-insensitive).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckItemExists(string tableName, string value, string? originalValue = null)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(value))
            {
                return Json(new { exists = false });
            }

            if (!string.IsNullOrWhiteSpace(originalValue)
                && value.Equals(originalValue, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { exists = false });
            }

            var result = await _masterLookupService.GetLookupItemsAsync(tableName);
            var exists = result.Success
                && result.Data != null
                && result.Data.Any(i => i.Value.Equals(value, StringComparison.OrdinalIgnoreCase));

            return Json(new { exists });
        }

        private async Task<List<string>> GetTableNamesAsync()
        {
            var response = await _masterLookupService.GetAllMasterLookupsAsync();
            return response.Success && response.Data != null
                ? response.Data.Select(m => m.MasterTableName).ToList()
                : new List<string>();
        }

        private async Task<DataGridConfig<LookupItemViewModel>> BuildItemGridAsync(
            string tableName, PaginationFilter<string>? request = null)
        {
            request ??= new PaginationFilter<string>();

            var query = new Application.Pagination.QueryParameters<string>
            {
                Search = request.Search,
                SortBy = request.SortBy,
                Descending = request.Descending,
                Page = request.Page,
                PageSize = request.PageSize,
                Filter = request.Filter
            };

            var response = await _masterLookupService.GetLookupItemsPagedAsync(tableName, query);

            var data = response.Success && response.Data != null
                ? response.Data
                : new Application.Pagination.PaginatedResult<Application.Dtos.FPS.LookupItemDto>();

            var items = data.data
                .Select(i => new LookupItemViewModel { Value = i.Value })
                .ToList();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<LookupItemViewModel>
            {
                GridId = "lookupItemGrid",
                Title = tableName,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Value",
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                AddFunction = "addLookupItem",
                EditFunction = "editLookupItem",
                DeleteFunction = "deleteLookupItem",
                ExtraFilterMethod = "getLookupExtraFilters",
                BindGridUrl = "/FPS/MasterLookup/LoadItemsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<LookupItemViewModel>(null),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentSearch = request.Search,
                CurrentFilters = filterDict
            };
        }

        private JsonResult ValidationFailureJson()
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

        private static string FirstError(IEnumerable<Application.Dtos.ApiErrorDto>? errors, string fallback)
        {
            return errors?.FirstOrDefault()?.Message ?? fallback;
        }

        private JsonResult FailureJson(IReadOnlyCollection<Application.Dtos.ApiErrorDto>? errors, string fallback)
        {
            return Json(new
            {
                success = false,
                message = FirstError(errors, fallback),
                errors = (errors ?? new List<Application.Dtos.ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }
    }
}

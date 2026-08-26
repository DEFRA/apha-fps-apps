using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class DivisionGradeMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IDivisionGradeService _maintDGService;
        private readonly IDivisionService _divisionService;
        private readonly IFpsYearContext _fpsYearContext;

        public DivisionGradeMaintenanceController(IMapper mapper, IDivisionGradeService maintDGService, IDivisionService divisionService, IFpsYearContext fpsYearContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _maintDGService = maintDGService ?? throw new ArgumentNullException(nameof(maintDGService));
            _divisionService = divisionService ?? throw new ArgumentNullException(nameof(divisionService));
            _fpsYearContext = fpsYearContext ?? throw new ArgumentNullException(nameof(fpsYearContext));
        }

        public async Task<IActionResult> Index(int? year)
        {
            var viewModel = new DivisionGradeViewModel();
            viewModel.SelectedYear = year;
            // Build the initial grid with no default sort applied.
            // Sorting is only applied after the user explicitly clicks a column header.
            var defaultRequest = new PaginationFilter<string>
            {
                Filter = "{}"
            };
            viewModel.DivisionGradeGrid = await GetDivisionGradeGridConfigAsync(defaultRequest);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadDivisionGradeGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetDivisionGradeGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<DivisionGradeItem>> GetDivisionGradeGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _maintDGService.GetAllPagedAsync(queryParameters);

            var items = new List<DivisionGradeItem>();
            if (pagedData.Data != null)
            {
                items = _mapper.Map<List<DivisionGradeItem>>(pagedData.Data);
            }

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<DivisionGradeItem>
            {
                GridId = "divisionGradeGrid",
                Title = "Division Grade Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "DivisionGradeCode",
                AllowAdd = true,
                AddFunction = "addDivisionGrade",
                AllowEdit = true,
                EditFunction = "editDivisionGrade",
                AllowDelete = true,
                DeleteFunction = "deleteDivisionGrade",
                BindGridUrl = $"/FPS/DivisionGradeMaintenance/LoadDivisionGradeGrid?year={_fpsYearContext.Year}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<DivisionGradeItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new DivisionGradeItem();
            return PartialView("_AddEditDivisionGrade", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DivisionGradeDto dto)
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

            // Ensure rate fields are always stored as positive values
            if (dto.ChargeRate.HasValue) dto.ChargeRate = Math.Abs(dto.ChargeRate.Value);
            if (dto.DirectRate.HasValue) dto.DirectRate = Math.Abs(dto.DirectRate.Value);
            if (dto.PayRate.HasValue) dto.PayRate = Math.Abs(dto.PayRate.Value);
            if (dto.Npr.HasValue) dto.Npr = Math.Abs(dto.Npr.Value);
            if (dto.Ohr.HasValue) dto.Ohr = Math.Abs(dto.Ohr.Value);

            var result = await _maintDGService.CreateAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Division grade created successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create division grade.";
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

        [HttpGet]
        public async Task<IActionResult> CheckDivisionGradeCodeExists(string code, string? originalCode = null)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { exists = false });
            }

            // When editing, re-casing the record's own code is allowed and must not count as a conflict.
            if (!string.IsNullOrWhiteSpace(originalCode)
                && code.Equals(originalCode, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { exists = false });
            }

            var result = await _maintDGService.GetAllDivisionGradeCodesAsync();
            var exists = result.Success
                && result.Data != null
                && result.Data.Any(c =>
                    !string.IsNullOrWhiteSpace(c)
                    && c.Equals(code, StringComparison.OrdinalIgnoreCase));

            return Json(new { exists });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Division grade code is required" });
            }

            var result = await _maintDGService.GetByIdAsync(id);

            if (result.Success && result.Data != null)
            {
                var item = _mapper.Map<DivisionGradeItem>(result.Data);
                return PartialView("_AddEditDivisionGrade", item);
            }

            return Json(new { success = false, message = $"Division grade '{id}' not found." });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, [FromBody] DivisionGradeDto dto)
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

            // Ensure rate fields are always stored as positive values
            if (dto.ChargeRate.HasValue) dto.ChargeRate = Math.Abs(dto.ChargeRate.Value);
            if (dto.DirectRate.HasValue) dto.DirectRate = Math.Abs(dto.DirectRate.Value);
            if (dto.PayRate.HasValue) dto.PayRate = Math.Abs(dto.PayRate.Value);
            if (dto.Npr.HasValue) dto.Npr = Math.Abs(dto.Npr.Value);
            if (dto.Ohr.HasValue) dto.Ohr = Math.Abs(dto.Ohr.Value);

            var result = await _maintDGService.UpdateAsync(id, dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Division grade updated successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update division grade.";
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

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Division grade code is required" });
            }

            var result = await _maintDGService.DeleteAsync(id);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "Division grade deleted successfully" });
            }

            return Json(new { success = false, message = "Unable to delete the division grade as it may be in use." });
        }

        [HttpGet]
        public async Task<IActionResult> GetDistinctGradeCodes()
        {
            try
            {
                var result = await _maintDGService.GetAllGradeCodesAsync();

                if (result.Success && result.Data != null)
                {
                    var gradeCodes = result.Data.Select(g => new { gradeCode = g }).ToList();
                    return Json(new { success = true, data = gradeCodes });
                }

                return Json(new { success = false, message = "Failed to load grade codes" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading grade codes: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDistinctDivisions()
        {
            try
            {
                var result = await _divisionService.GetAllDivisionsAsync();

                if (result.Success && result.Data != null)
                {
                    var divisions = result.Data
                        .Select(d => new { divName = d.DivName })
                        .OrderBy(d => d.divName)
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

       
    }
}

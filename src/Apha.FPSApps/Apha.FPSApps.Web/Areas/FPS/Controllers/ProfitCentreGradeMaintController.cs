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
    public class ProfitCentreGradeMaintController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProfitCentreGradeService _profitCentreGradeService;
        private readonly IDivisionGradeService _divisionGradeService;
        private readonly IFpsYearContext _fpsYearContext;

        public ProfitCentreGradeMaintController(IMapper mapper, IProfitCentreGradeService profitCentreGradeService, IDivisionGradeService divisionGradeService, IFpsYearContext fpsYearContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _profitCentreGradeService = profitCentreGradeService ?? throw new ArgumentNullException(nameof(profitCentreGradeService));
            _divisionGradeService = divisionGradeService ?? throw new ArgumentNullException(nameof(divisionGradeService));
            _fpsYearContext = fpsYearContext ?? throw new ArgumentNullException(nameof(fpsYearContext));
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new ProfitCentreGradeMaintViewModel();
            var defaultRequest = new PaginationFilter<string> { Filter = "{}", SortBy = "PcGrade", Descending = false };
            viewModel.RcGradeMaintenanceGrid = await GetProfitCentreGradeMaintGridConfigAsync(defaultRequest);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadProfitCentreGradeMaintGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetProfitCentreGradeMaintGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProfitCentreGradeMaintItem>> GetProfitCentreGradeMaintGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _profitCentreGradeService.GetAllPagedAsync(queryParameters);

            var items = new List<ProfitCentreGradeMaintItem>();
            if (pagedData.Data != null)
            {
                items = _mapper.Map<List<ProfitCentreGradeMaintItem>>(pagedData.Data);
            }

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ProfitCentreGradeMaintItem>
            {
                GridId = "rcGradeMaintenanceGrid",
                Title = "Resource Centre Grade Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "PcGrade",
                AllowAdd = true,
                AddFunction = "addMaintPCGrade",
                AllowEdit = true,
                EditFunction = "editMaintPCGrade",
                AllowDelete = true,
                DeleteFunction = "deleteMaintPCGrade",
                BindGridUrl = $"/FPS/ProfitCentreGradeMaint/LoadProfitCentreGradeMaintGrid?year={_fpsYearContext.Year}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProfitCentreGradeMaintItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new ProfitCentreGradeMaintItem();
            return PartialView("_AddEditMaintPCGrade", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProfitCentreGradeDto dto)
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

            if (dto.ChargeRate.HasValue) dto.ChargeRate = Math.Abs(dto.ChargeRate.Value);
            if (dto.DirectRate.HasValue) dto.DirectRate = Math.Abs(dto.DirectRate.Value);
            if (dto.PayRate.HasValue) dto.PayRate = Math.Abs(dto.PayRate.Value);
            if (dto.NPR.HasValue) dto.NPR = Math.Abs(dto.NPR.Value);
            if (dto.OHR.HasValue) dto.OHR = Math.Abs(dto.OHR.Value);

            var result = await _profitCentreGradeService.CreateAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "RC grade created successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create RC grade.";
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
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "RC grade code is required" });
            }

            var result = await _profitCentreGradeService.GetByIdAsync(id);

            if (result.Success && result.Data != null)
            {
                var item = _mapper.Map<ProfitCentreGradeMaintItem>(result.Data);
                return PartialView("_AddEditMaintPCGrade", item);
            }

            return Json(new { success = false, message = $"RC grade '{id}' not found." });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, [FromBody] ProfitCentreGradeDto dto)
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

            if (dto.ChargeRate.HasValue) dto.ChargeRate = Math.Abs(dto.ChargeRate.Value);
            if (dto.DirectRate.HasValue) dto.DirectRate = Math.Abs(dto.DirectRate.Value);
            if (dto.PayRate.HasValue) dto.PayRate = Math.Abs(dto.PayRate.Value);
            if (dto.NPR.HasValue) dto.NPR = Math.Abs(dto.NPR.Value);
            if (dto.OHR.HasValue) dto.OHR = Math.Abs(dto.OHR.Value);

            var result = await _profitCentreGradeService.UpdateAsync(id, dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "RC grade updated successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update RC grade.";
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
                return Json(new { success = false, message = "RC grade code is required" });
            }

            var result = await _profitCentreGradeService.DeleteAsync(id);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "RC grade deleted successfully" });
            }

            return Json(new { success = false, message = "Unable to delete the RC grade as it may be in use." });
        }

        [HttpGet]
        public async Task<IActionResult> GetDistinctDivisionGrades()
        {
            try
            {
                var result = await _divisionGradeService.GetAllDivisionGradeCodesAsync();

                if (result.Success && result.Data != null)
                {
                    var items = result.Data.Select(g => new { divisionGrade = g }).ToList();
                    return Json(new { success = true, data = items });
                }

                return Json(new { success = false, message = "Failed to load division grades" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading division grades: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDistinctGradeCodes()
        {
            try
            {
                var result = await _divisionGradeService.GetAllGradeCodesAsync();

                if (result.Success && result.Data != null)
                {
                    var items = result.Data.Select(g => new { gradeCode = g }).ToList();
                    return Json(new { success = true, data = items });
                }

                return Json(new { success = false, message = "Failed to load grade codes" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading grade codes: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDistinctProfitCentres()
        {
            try
            {
                var result = await _profitCentreGradeService.GetAllProfitCentreCodesAsync();

                if (result.Success && result.Data != null)
                {
                    var items = result.Data.Select(p => new { profitCentre = p }).ToList();
                    return Json(new { success = true, data = items });
                }

                return Json(new { success = false, message = "Failed to load profit centres" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading profit centres: {ex.Message}" });
            }
        }
    }
}

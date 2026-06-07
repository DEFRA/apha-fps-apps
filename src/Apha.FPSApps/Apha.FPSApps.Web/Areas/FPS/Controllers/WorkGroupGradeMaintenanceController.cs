using Apha.FPSApps.Application.Dtos;
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
    /// <summary>
    /// MVC controller for WorkgroupGrade maintenance operations.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class WorkGroupGradeMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupGradeService _wgGradeService;
        private readonly IProfitCentreGradeService _pcGradeService;
        private readonly IWorkGroupService _workgroupService;

        public WorkGroupGradeMaintenanceController(IMapper mapper, IWorkGroupGradeService wgGradeService, IProfitCentreGradeService pcGradeService, IWorkGroupService workgroupService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _wgGradeService = wgGradeService ?? throw new ArgumentNullException(nameof(wgGradeService));
            _pcGradeService = pcGradeService ?? throw new ArgumentNullException(nameof(pcGradeService));
            _workgroupService = workgroupService ?? throw new ArgumentNullException(nameof(workgroupService));
        }

        /// <summary>Displays the WorkgroupGrade maintenance page with DataGrid.</summary>
        public async Task<IActionResult> Index()
        {
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var gridConfig = await GetWGGradeGridConfigAsync(defaultRequest);

            var viewModel = new MaintWGGradeViewModel
            {
                WGGradeGrid = gridConfig
            };

            return View(viewModel);
        }

        /// <summary>Loads the WorkgroupGrade grid via AJAX for pagination and filtering.</summary>
        [HttpPost]
        public async Task<IActionResult> LoadMaintWGGradeGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetWGGradeGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<MaintWGGradeItem>> GetWGGradeGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _wgGradeService.GetAllWorkgroupGradesPagedAsync(queryParameters);

            var items = pagedData.Data != null
                ? _mapper.Map<List<MaintWGGradeItem>>(pagedData.Data)
                : new List<MaintWGGradeItem>();

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<MaintWGGradeItem>
            {
                GridId = "wgGradeGrid",
                Title = "WorkGroup Grades",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "WgGrade",
                AllowAdd = true,
                AddFunction = "addWGGrade",
                AllowEdit = true,
                EditFunction = "editWGGrade",
                AllowDelete = true,
                DeleteFunction = "deleteWGGrade",
                ExtraFilterMethod = "getWGGradeExtraFilters",
                BindGridUrl = "/FPS/WorkGroupGradeMaintenance/LoadMaintWGGradeGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<MaintWGGradeItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        /// <summary>Displays the Add WGGrade modal partial.</summary>
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEditMaintWGGrade", new MaintWGGradeItem());
        }

        /// <summary>Creates a new WorkgroupGrade record.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MaintWGGradeItem item)
        {
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

            var dto = _mapper.Map<WorkgroupGradeDto>(item);
            var result = await _wgGradeService.CreateAsync(dto);

            if (result.Success)
                return Json(new { success = true, message = "WorkgroupGrade created successfully" });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create WorkgroupGrade.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>Displays the Edit WGGrade modal partial.</summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string wgGrade)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return Json(new { success = false, message = "WgGrade is required" });

            var result = await _wgGradeService.GetByWgGradeAsync(wgGrade);

            if (!result.Success || result.Data is null)
                return Json(new { success = false, message = $"WorkgroupGrade '{wgGrade}' not found." });

            var item = _mapper.Map<MaintWGGradeItem>(result.Data);
            return PartialView("_AddEditMaintWGGrade", item);
        }

        /// <summary>Updates an existing WorkgroupGrade record.</summary>
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] MaintWGGradeItem item)
        {
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

            var dto = _mapper.Map<WorkgroupGradeDto>(item);
            var result = await _wgGradeService.UpdateAsync(item.WgGrade, dto);

            if (result.Success)
                return Json(new { success = true, message = "WorkgroupGrade updated successfully" });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update WorkgroupGrade.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>Deletes a WorkgroupGrade record by WgGrade code.</summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(string wgGrade)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return Json(new { success = false, message = "WgGrade is required" });

            var result = await _wgGradeService.DeleteAsync(wgGrade);

            if (result.Success)
                return Json(new { success = true, message = "WorkgroupGrade deleted successfully" });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete WorkgroupGrade.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>Returns all PC Grade codes for dropdown population in the modal.</summary>
        [HttpGet]
        public async Task<IActionResult> GetPcGrades()
        {
            var result = await _pcGradeService.GetAllPcGradesAsync();
            return result.Success
                ? Json(new { success = true, data = result.Data })
                : Json(new { success = false, message = "Failed to load PC Grades" });
        }

        /// <summary>Returns all Grade codes for dropdown population in the modal.</summary>
        [HttpGet]
        public async Task<IActionResult> GetGradeCodes()
        {
            var result = await _wgGradeService.GetAllGradeCodesAsync();
            return result.Success
                ? Json(new { success = true, data = result.Data })
                : Json(new { success = false, message = "Failed to load Grade codes" });
        }

        /// <summary>Returns all Workgroup names for dropdown population in the modal.</summary>
        [HttpGet]
        public async Task<IActionResult> GetWorkgroups()
        {
            var result = await _workgroupService.GetAllWorkGroupNamesAsync();
            return result.Success
                ? Json(new { success = true, data = result.Data })
                : Json(new { success = false, message = "Failed to load Workgroups" });
        }
    }
}

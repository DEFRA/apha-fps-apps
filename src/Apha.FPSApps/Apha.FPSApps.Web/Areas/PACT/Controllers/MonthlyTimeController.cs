using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class MonthlyTimeController : Controller
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IMapper _mapper;
        private readonly IPactMonthlyTimeService _monthlyTimeService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IEmployeeService _employeeService;
        private readonly IPactTimeCodeValidService _timeCodeValidService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;
        private readonly IExcelExportService _excelExportService;

        public MonthlyTimeController(
            IMapper mapper,
            IPactMonthlyTimeService monthlyTimeService,
            IWorkGroupService workGroupService,
            IEmployeeService employeeService,
            IPactTimeCodeValidService timeCodeValidService,
            IProjectService projectService,
            IMonthService monthService,
            IExcelExportService excelExportService)
        {
            _mapper = mapper;
            _monthlyTimeService = monthlyTimeService;
            _workGroupService = workGroupService;
            _employeeService = employeeService;
            _timeCodeValidService = timeCodeValidService;
            _projectService = projectService;
            _monthService = monthService;
            _excelExportService = excelExportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            DataGridConfig<MonthlyTimeLiveItem> liveGrid = await BuildLiveGridAsync(new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, null, null, null, null, null);
            DataGridConfig<StagingMonthlyTimeItem> stagingGrid = await BuildStagingGridAsync(new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, null);
            var viewModel = new MonthlyTimeViewModel
            {
                WorkGroupOptions = await GetWorkGroupOptionsAsync(),
                StaffOptions = new List<SelectListItem>(),
                TimeCodeOptions = new List<SelectListItem>(),
                ProjectOptions = new List<SelectListItem>(),
                MonthOptions = await GetMonthOptionsAsync(),
                LiveGrid = liveGrid,
                StagingGrid = stagingGrid,
                LiveTotalHours = liveGrid.Total,
                StagingTotalHours = stagingGrid.Total
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadLiveGrid(
            PaginationFilter<string> request,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            double? month)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildLiveGridAsync(request, workGroup, timeCode, pactStaffId, parentProject, month);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadStagingGrid(PaginationFilter<string> request, bool? passed)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildStagingGridAsync(request, passed);
            return PartialView("_DataGrid", grid);
        }

        [HttpGet]
        public async Task<IActionResult> GetLiveRecord(string pactStaffId, string timeCode, double month, string parentProject)
        {
            await PopulateViewBagsAsync();
            var response = await _monthlyTimeService.GetLiveByKeyAsync(pactStaffId, timeCode, month, parentProject);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<MonthlyTimeLiveItem>(response.Data);
            return PartialView("_EditMonthlyTimeLive", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffByWorkGroup(string? workGroup)
        {
            var response = await _employeeService.GetPactWorkGroupStaffAsync(workGroup);
            if (!response.Success || response.Data == null)
                return Json(Array.Empty<object>());

            var staff = response.Data
                .Where(x => !string.IsNullOrWhiteSpace(x.PactId))
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    pactId = x.PactId ?? string.Empty,
                    spNumber = x.SpNumber ?? string.Empty,
                    name = x.Name ?? string.Empty,
                    workGroupGrade = x.WorkGroupGrade ?? string.Empty
                })
                .ToList();

            return Json(staff);
        }

        [HttpGet]
        public async Task<IActionResult> GetTimeCodesByWorkGroup(string? workGroup)
        {
            if (string.IsNullOrWhiteSpace(workGroup))
                return Json(Array.Empty<object>());

            var options = await GetTimeCodeOptionsAsync(workGroup);
            var result = options.Select(x => new { value = x.Value, text = x.Text }).ToList();
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectsByWorkGroupAndTimeCode(string? workGroup, string? timeCode)
        {
            if (string.IsNullOrWhiteSpace(workGroup) || string.IsNullOrWhiteSpace(timeCode))
                return Json(Array.Empty<object>());

            var options = await GetProjectOptionsAsync(workGroup, timeCode);
            var result = options.Select(x => new { value = x.Value, text = x.Text }).ToList();
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTimeCodes()
        {
            var response = await _timeCodeValidService.GetAllDistinctTimeCodesAsync();
            if (!response.Success || response.Data == null)
                return Json(Array.Empty<object>());

            var result = response.Data.Select(x => new { value = x, text = x }).ToList();
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            var response = await _timeCodeValidService.GetAllDistinctProjectsAsync();
            if (!response.Success || response.Data == null)
                return Json(Array.Empty<object>());

            var result = response.Data.Select(x => new { value = x, text = x }).ToList();
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLiveRecord([FromBody] MonthlyTimeLiveItem model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data." });

            var validationErrors = await ValidateLiveRecordAsync(model);
            if (validationErrors.Count > 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = validationErrors
                });
            }

            var dto = _mapper.Map<MonthlyTimeDto>(model);

            var keyParts = (model.CompositeKey ?? string.Empty).Split('|');
            dto.OriginalPactStaffId = keyParts.ElementAtOrDefault(0);

            var response = await _monthlyTimeService.UpdateLiveAsync(dto);
            if (response.Success)
                return Json(new { success = true, message = "Monthly time record updated successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to update monthly time record.",
                errors = response.Errors?.Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "Validation error" })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetStagingRecord(int id)
        {
            await PopulateViewBagsAsync();
            var response = await _monthlyTimeService.GetStagingByIdAsync(id);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<StagingMonthlyTimeItem>(response.Data);

            // Pre-populate TimeCode and ParentProject options from the existing record's WG/TC
            if (!string.IsNullOrWhiteSpace(model.WorkGroup))
            {
                ViewBag.TimeCodeOptions = await GetTimeCodeOptionsAsync(model.WorkGroup);

                if (!string.IsNullOrWhiteSpace(model.TimeCode))
                    ViewBag.ProjectOptions = await GetProjectOptionsAsync(model.WorkGroup, model.TimeCode);
            }

            return PartialView("_AddEditStagingMonthlyTime", model);
        }

        [HttpGet]
        public async Task<IActionResult> AddStagingRecord()
        {
            await PopulateViewBagsAsync();
            return PartialView("_AddEditStagingMonthlyTime", new StagingMonthlyTimeItem());
        }

        [HttpPost]
        public async Task<IActionResult> SaveStagingRecord([FromBody] StagingMonthlyTimeItem model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data." });

            StagingMonthlyTimeDto? existingRecord = null;
            if (model.Id != 0)
            {
                var existingResponse = await _monthlyTimeService.GetStagingByIdAsync(model.Id);
                if (existingResponse.Success)
                    existingRecord = existingResponse.Data;
            }

            var dto = _mapper.Map<StagingMonthlyTimeDto>(model);
            ApiResponseDto<StagingMonthlyTimeDto> response = model.Id == 0
                ? await _monthlyTimeService.CreateStagingAsync(dto)
                : await _monthlyTimeService.UpdateStagingAsync(model.Id, dto);

            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to save staging record." });

            var shouldApplyNameUpdating = model.Id != 0
                && model.NameUpdating
                && existingRecord != null
                && !string.IsNullOrWhiteSpace(existingRecord.WorkGroup)
                && !string.IsNullOrWhiteSpace(existingRecord.PactStaffId)
                && (!string.Equals(existingRecord.Name, model.Name, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(existingRecord.PactStaffId, model.PactStaffId, StringComparison.OrdinalIgnoreCase));

            if (!shouldApplyNameUpdating)
                return Json(new { success = true, message = model.Id == 0 ? "Staging record added successfully." : "Staging record updated successfully." });

            var bulkUpdateResponse = await _monthlyTimeService.BulkUpdateStagingNamesAsync(new BulkUpdateStagingMonthlyTimeNamesDto
            {
                ExcludeId = model.Id,
                OriginalWorkGroup = existingRecord.WorkGroup,
                OriginalPactStaffId = existingRecord.PactStaffId,
                NewName = model.Name,
                NewPactStaffId = model.PactStaffId,
                NewPactId = model.PactId
            });

            if (!bulkUpdateResponse.Success)
                return Json(new { success = false, message = bulkUpdateResponse.Errors?.FirstOrDefault()?.Message ?? "Failed to apply name updates to related records." });

            var updatedCount = bulkUpdateResponse.Data?.UpdatedCount ?? 0;
            return Json(new
            {
                success = true,
                message = updatedCount > 0
                    ? $"Staging record updated successfully. Name updates applied to {updatedCount} related record(s)."
                    : "Staging record updated successfully."
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteStagingRecord(int id)
        {
            var response = await _monthlyTimeService.DeleteStagingAsync(id);
            return Json(new { success = response.Success && response.Data });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllStagingRecords()
        {
            var response = await _monthlyTimeService.DeleteAllStagingByUserAsync();
            return Json(new { success = response.Success && response.Data });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFailedStagingRecords()
        {
            var response = await _monthlyTimeService.DeleteFailedStagingByUserAsync();
            return Json(new
            {
                success = response.Success && response.Data,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to delete failed imported records."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file, short importType)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select an Excel file to import." });

            var response = await _monthlyTimeService.ImportMonthlyTimeAsync(file, importType);
            if (response.Success && response.Data != null)
            {
                return Json(new
                {
                    success = true,
                    importedCount = response.Data.ImportedCount,
                    passedCount = response.Data.PassedCount,
                    failedCount = response.Data.FailedCount,
                    message = response.Data.Message
                });
            }

            return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Import failed." });
        }

        [HttpPost]
        public async Task<IActionResult> Validate()
        {
            var response = await _monthlyTimeService.ValidateStagingAsync();
            if (response.Success && response.Data != null)
            {
                return Json(new
                {
                    success = true,
                    passedCount = response.Data.PassedCount,
                    failedCount = response.Data.FailedCount,
                    message = response.Data.Message
                });
            }

            return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Validation failed." });
        }

        [HttpPost]
        public async Task<IActionResult> MakeLive()
        {
            var response = await _monthlyTimeService.MakeLiveAsync();
            if (response.Success && response.Data != null)
            {
                return Json(new
                {
                    success = true,
                    processedCount = response.Data.ProcessedCount,
                    importedCount = response.Data.ImportedCount,
                    failedCount = response.Data.FailedCount,
                    message = response.Data.Message
                });
            }

            return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Make live failed.", errors = response.Errors });
        }

        [HttpGet]
        public async Task<IActionResult> ExportStaging(bool? passed)
        {
            var response = await _monthlyTimeService.GetStagingAsync(new QueryParameters<string> { Page = -1 }, passed);
            if (!response.Success || response.Data == null)
                return NotFound();

            var rows = _mapper.Map<List<StagingMonthlyTimeExportItem>>(response.Data);
            var excelBytes = _excelExportService.ExportToExcel(rows, "MonthlyTime");

            var fileName = rows.FirstOrDefault()?.Filename;
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = $"MonthlyTime_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(excelBytes, ExcelContentType, fileName);
        }

        private async Task<DataGridConfig<MonthlyTimeLiveItem>> BuildLiveGridAsync(
            PaginationFilter<string> request,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            double? month)
        {
            var hasAnyFilter = !string.IsNullOrWhiteSpace(workGroup)
                || !string.IsNullOrWhiteSpace(timeCode)
                || !string.IsNullOrWhiteSpace(pactStaffId)
                || !string.IsNullOrWhiteSpace(parentProject)
                || month.HasValue;

            var currentFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? [];

            List<MonthlyTimeLiveItem> items;
            decimal total;
            PaginationModel pagination;

            if (!hasAnyFilter)
            {
                items = [];
                total = 0;
                pagination = new PaginationModel
                {
                    TotalRecords = 0,
                    PageNumber = request.Page > 0 ? request.Page : 1,
                    PageSize = request.PageSize > 0 ? request.PageSize : 10
                };
            }
            else
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _monthlyTimeService.GetLiveAsync(query, workGroup, timeCode, pactStaffId, parentProject, month);

                items = response.Success && response.Data != null
                    ? _mapper.Map<List<MonthlyTimeLiveItem>>(response.Data)
                    : [];
                total = response.Total;
                pagination = response.Pagination != null
                    ? _mapper.Map<PaginationModel>(response.Pagination)
                    : new PaginationModel();
            }

            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<MonthlyTimeLiveItem>
            {
                GridId = "monthlyTimeLiveGrid",
                Title = "Monthly Time",
                AllowAdd = false,
                AllowDelete = false,
                ShowCheckboxColumn = false,
                KeyProperty = "CompositeKey",
                EditFunction = "editMonthlyTimeLive",
                BindGridUrl = "/PACT/MonthlyTime/LoadLiveGrid",
                ExtraFilterMethod = "getMonthlyTimeLiveFilters",
                Data = items,
                Total = total,
                Columns = GridDataProvider.GetColumnsDefination<MonthlyTimeLiveItem>(null),
                Pagination = pagination,
                CurrentFilters = currentFilters
            };
        }

        private async Task<DataGridConfig<StagingMonthlyTimeItem>> BuildStagingGridAsync(PaginationFilter<string> request, bool? passed)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _monthlyTimeService.GetStagingAsync(query, passed);
            var items = response.Success && response.Data != null ? _mapper.Map<List<StagingMonthlyTimeItem>>(response.Data) : [];
            var total = response.Total;
            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<StagingMonthlyTimeItem>
            {
                GridId = "monthlyTimeStagingGrid",
                Title = "Imported Time Records",
                AllowExport = false,
                ShowCheckboxColumn = false,
                KeyProperty = "Id", 
                AddFunction = "addStagingMonthlyTime",
                EditFunction = "editStagingMonthlyTime",
                DeleteFunction = "deleteStagingMonthlyTime",
                BindGridUrl = "/PACT/MonthlyTime/LoadStagingGrid",
                ExtraFilterMethod = "getMonthlyTimeStagingFilters",
                Data = items,
                Total = total,
                Columns = GridDataProvider.GetColumnsDefination<StagingMonthlyTimeItem>(null),
                Pagination = pagination,
                CurrentFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? []
            };
        }

        private async Task PopulateViewBagsAsync()
        {
            ViewBag.WorkGroups = await GetWorkGroupOptionsAsync();
            ViewBag.StaffOptions = new List<SelectListItem>();
            ViewBag.TimeCodeOptions = new List<SelectListItem>();
            ViewBag.ProjectOptions = new List<SelectListItem>();
            ViewBag.MonthOptions = await GetMonthOptionsAsync();
        }

        private async Task<List<SelectListItem>> GetWorkGroupOptionsAsync()
        {
            var response = await _workGroupService.GetAllWorkGroupsAsync();
            return response.Success && response.Data != null
                ? response.Data.OrderBy(x => x.WorkGroupName).Select(x => new SelectListItem(x.WorkGroupName, x.WorkGroupName)).ToList()
                : [];
        }

        private async Task<List<SelectListItem>> GetTimeCodeOptionsAsync(string workGroup)
        {
            var response = await _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync(workGroup);
            return response.Success && response.Data != null
                ? response.Data.Select(x => x.TimeCode).Distinct().OrderBy(x => x).Select(x => new SelectListItem(x, x)).ToList()
                : [];
        }

        private async Task<List<SelectListItem>> GetProjectOptionsAsync(string workGroup, string timeCode)
        {
            var response = await _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync(workGroup, timeCode);
            return response.Success && response.Data != null
                ? response.Data.OrderBy(x => x).Select(x => new SelectListItem(x, x)).ToList()
                : [];
        }

        private async Task<List<SelectListItem>> GetMonthOptionsAsync()
        {
            var response = await _monthService.GetAllMonthsAsync();
            return response.Success && response.Data != null
                ? response.Data.OrderBy(x => x.Monthnumber).Select(x => new SelectListItem(x.Monthname, x.Monthnumber.ToString())).ToList()
                : [];
        }

        private async Task<List<object>> ValidateLiveRecordAsync(MonthlyTimeLiveItem model)
        {
            var errors = new List<object>();

            var workGroup = model.WorkGroup?.Trim();
            var staffId = model.PactStaffId?.Trim();
            var timeCode = model.TimeCode?.Trim();
            var parentProject = model.ParentProject?.Trim();

            if (model.Hours is null || model.Hours <= 0)
                errors.Add(new { field = "Hours", message = "The hours field must be greater than zero." });

            if (string.IsNullOrWhiteSpace(workGroup))
            {
                errors.Add(new { field = "WorkGroup", message = "The work group name is blank." });
            }
            else
            {
                var workGroups = await GetWorkGroupOptionsAsync();
                if (!workGroups.Any(x => string.Equals(x.Value, workGroup, StringComparison.OrdinalIgnoreCase)))
                    errors.Add(new { field = "WorkGroup", message = $"The work group name is invalid: {workGroup}" });
            }

            if (string.IsNullOrWhiteSpace(staffId))
                errors.Add(new { field = "PactStaffId", message = "Staff ID blank." });

            if (string.IsNullOrWhiteSpace(timeCode))
            {
                errors.Add(new { field = "TimeCode", message = "The Timecode is blank." });
            }
            else if (!string.IsNullOrWhiteSpace(workGroup))
            {
                var validTimeCodes = await GetTimeCodeOptionsAsync(workGroup);
                if (!validTimeCodes.Any(x => string.Equals(x.Value, timeCode, StringComparison.OrdinalIgnoreCase)))
                    errors.Add(new { field = "TimeCode", message = $"Timecode not valid for this WG or invalid timecode: {timeCode}, {workGroup}" });
            }

            if (string.IsNullOrWhiteSpace(parentProject))
            {
                errors.Add(new { field = "ParentProject", message = "The Project is blank." });
            }
            else if (!string.IsNullOrWhiteSpace(workGroup) && !string.IsNullOrWhiteSpace(timeCode))
            {
                var validProjects = await GetProjectOptionsAsync(workGroup, timeCode);
                if (!validProjects.Any(x => string.Equals(x.Value, parentProject, StringComparison.OrdinalIgnoreCase)))
                    errors.Add(new { field = "ParentProject", message = $"Not valid timecode/Project/WG combination: {parentProject}, {timeCode}, {workGroup}" });
            }

            var validMonths = await GetMonthOptionsAsync();
            var monthValue = model.Month.ToString("0");
            if (!validMonths.Any(x => string.Equals(x.Value, monthValue, StringComparison.OrdinalIgnoreCase)))
                errors.Add(new { field = "Month", message = $"The month No. invalid: {model.Month}" });

            return errors;
        }
    }
}

using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
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
            var viewModel = new MonthlyTimeViewModel
            {
                WorkGroupOptions = await GetWorkGroupOptionsAsync(),
                StaffOptions = new List<SelectListItem>(),
                TimeCodeOptions = new List<SelectListItem>(),
                ProjectOptions = new List<SelectListItem>(),
                MonthOptions = await GetMonthOptionsAsync(),
                LiveGrid = await BuildLiveGridAsync(new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, null, null, null, null, null),
                StagingGrid = await BuildStagingGridAsync(new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 }, null)
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
            if (string.IsNullOrWhiteSpace(workGroup))
                return Json(Array.Empty<object>());

            var response = await _employeeService.GetPactWorkGroupStaffAsync(workGroup);
            if (!response.Success || response.Data == null)
                return Json(Array.Empty<object>());

            var staff = response.Data
                .Where(x => !string.IsNullOrWhiteSpace(x.PactId))
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    pactId = x.PactId ?? string.Empty,
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

        [HttpPost]
        public async Task<IActionResult> SaveLiveRecord([FromBody] MonthlyTimeLiveItem model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data." });

            var dto = _mapper.Map<MonthlyTimeDto>(model);
            var response = await _monthlyTimeService.UpdateLiveAsync(dto);
            if (response.Success)
                return Json(new { success = true, message = "Monthly time record updated successfully." });

            return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to update monthly time record." });
        }

        [HttpGet]
        public async Task<IActionResult> GetStagingRecord(int id)
        {
            await PopulateViewBagsAsync();
            var response = await _monthlyTimeService.GetStagingByIdAsync(id);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<StagingMonthlyTimeItem>(response.Data);
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

            var dto = _mapper.Map<StagingMonthlyTimeDto>(model);
            ApiResponseDto<StagingMonthlyTimeDto> response = model.Id == 0
                ? await _monthlyTimeService.CreateStagingAsync(dto)
                : await _monthlyTimeService.UpdateStagingAsync(model.Id, dto);

            if (response.Success)
                return Json(new { success = true, message = model.Id == 0 ? "Staging record added successfully." : "Staging record updated successfully." });

            return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to save staging record." });
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
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _monthlyTimeService.GetLiveAsync(query, workGroup, timeCode, pactStaffId, parentProject, month);
            var items = response.Success && response.Data != null ? _mapper.Map<List<MonthlyTimeLiveItem>>(response.Data) : [];
            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
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
                Columns = GridDataProvider.GetColumnsDefination<MonthlyTimeLiveItem>(null),
                Pagination = pagination,
                CurrentFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? []
            };
        }

        private async Task<DataGridConfig<StagingMonthlyTimeItem>> BuildStagingGridAsync(PaginationFilter<string> request, bool? passed)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _monthlyTimeService.GetStagingAsync(query, passed);
            var items = response.Success && response.Data != null ? _mapper.Map<List<StagingMonthlyTimeItem>>(response.Data) : [];
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

        private async Task<List<SelectListItem>> GetPactWorkGroupStaffOptionsAsync(string workGroup)
        {
            var response = await _employeeService.GetPactWorkGroupStaffAsync(workGroup);
            return response.Success && response.Data != null
                ? response.Data.Where(x => !string.IsNullOrWhiteSpace(x.PactId)).OrderBy(x => x.Name).Select(x => new SelectListItem($"{x.Name} ({x.PactId})", x.PactId ?? string.Empty)).ToList()
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
    }
}

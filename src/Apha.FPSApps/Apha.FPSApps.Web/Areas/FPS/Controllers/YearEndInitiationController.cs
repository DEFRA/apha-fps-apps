using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class YearEndInitiationController : Controller
    {
        private const string YearEndInitiationJobName = "YearEndInitiation";

        private readonly IMapper _mapper;
        private readonly IYearMasterService _yearMasterService;
        private readonly ISettingService _settingService;
        private readonly IMonthHourService _monthHourService;
        private readonly IYearEndService _yearEndService;
        private readonly ILogger<YearEndInitiationController> _logger;

        public YearEndInitiationController(
            IMapper mapper,
            IYearMasterService yearMasterService,
            ISettingService settingService,
            IMonthHourService monthHourService,
            IYearEndService yearEndService,
            ILogger<YearEndInitiationController> logger)
        {
            _mapper = mapper;
            _yearMasterService = yearMasterService;
            _settingService = settingService;
            _monthHourService = monthHourService;
            _yearEndService = yearEndService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var plannedYear = await GetPlannedYearAsync();
            var configValues = await GetConfigValuesAsync();
            var monthHours = await GetMonthWorkingHoursAsync();
            var canRun = await GetCanRunJobAsync();
            var grid = await BuildHistoryGridAsync(new PaginationFilter<string> { Filter = "{}" });

            return View(new YearEndInitiationViewModel
            {
                PlannedYear = plannedYear,
                ConfigValues = configValues,
                MonthWorkingHours = monthHours,
                CanRunJob = canRun,
                HistoryGrid = grid
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadHistoryGrid(PaginationFilter<string> request)
        {
            var grid = await BuildHistoryGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSetting([FromBody] SettingDto dto)
        {
            var result = await _settingService.SaveSettingAsync(dto);
            if (result.Success)
                return Json(new { success = true });

            var errors = result.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? [new { field = string.Empty, message = "Failed to save setting." }];
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> SaveMonthHour([FromBody] MonthHourDto dto)
        {
            var result = await _monthHourService.SaveMonthHourAsync(dto);
            if (result.Success)
                return Json(new { success = true });

            var errors = result.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? [new { field = string.Empty, message = "Failed to save month hour." }];
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> TriggerInitiate()
        {
            var correlationId = Guid.NewGuid().ToString();
            var result = await _yearEndService.TriggerYearEndInitiationJobAsync(0, correlationId);
            if (result.Success)
            {
                _logger.LogInformation("Year End Initiation job triggered. EventId: {EventId}", result?.Data?.EventId);
                return Json(new { success = true });
            }

            var errors = result?.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? [new { field = string.Empty, message = "Failed to trigger Year End Initiation job." }];
            return Json(new { success = false, errors });
        }

        private async Task<int> GetPlannedYearAsync()
        {
            var result = await _yearMasterService.GetFpsPlannedYearAsync();
            return result.Success ? result.Data : 0;
        }

        private async Task<bool> GetCanRunJobAsync()
        {
            var result = await _yearEndService.CanRunYearEndInitiationBatchJobAsync(YearEndInitiationJobName);
            return result.Success && result.Data;
        }

        private async Task<List<YearEndConfigValueItem>> GetConfigValuesAsync()
        {
            var result = await _settingService.GetYearEndSettingsAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data.Select(s => new YearEndConfigValueItem
                {
                    Id = s.Id,
                    Label = s.Setting ?? s.Id,
                    Value = s.Notes,
                    FpsYearType = s.FpsYearType
                }).ToList();
            }
            return [];
        }

        private async Task<List<YearEndMonthWorkingItem>> GetMonthWorkingHoursAsync()
        {
            var result = await _monthHourService.GetYearEndMonthHoursAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data.Select(m => new YearEndMonthWorkingItem
                {
                    Year = m.Year,
                    Month = m.Month,
                    MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Month),
                    Days = m.Days,
                    CvlHours = m.CvlHours,
                    VidHours = m.VidHours,
                    Fmonth = m.Fmonth,
                    FpsYear = m.FpsYear,
                    FpsYearType = m.FpsYearType
                }).ToList();
            }
            return [];
        }

        private async Task<DataGridConfig<YearEndHistoryItem>> BuildHistoryGridAsync(PaginationFilter<string> request)
        {
            var grid = HistoryGridConfig();
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _yearEndService.GetYearEndInitiationBatchJobHistoryAsync(query, YearEndInitiationJobName);

            grid.Data = response.Data?.data != null
                ? response.Data.data.Select(d => new YearEndHistoryItem
                {
                    JobName = d.JobName,
                    RequestedBy = d.RequestedBy,
                    StartDateTime = d.StartDateTime,
                    EndDateTime = d.EndDateTime,
                    Status = d.Status,
                    ErrorMessage = d.ErrorMessage
                }).ToList()
                : [];

            grid.Pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();

            grid.Pagination.SortColumn = request.SortBy;
            grid.Pagination.SortDirection = request.Descending;

            return grid;
        }

        private static DataGridConfig<YearEndHistoryItem> HistoryGridConfig() => new()
        {
            GridId = "yearEndInitiationHistoryGrid",
            Title = string.Empty,
            BindGridUrl = "/FPS/YearEndInitiation/LoadHistoryGrid",
            ShowCheckboxColumn = false,
            AllowAdd = false,
            AllowEdit = false,
            AllowDelete = false,
            Columns = GridDataProvider.GetColumnsDefination<YearEndHistoryItem>()
        };
    }
}

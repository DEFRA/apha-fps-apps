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
    [Authorize(Roles = "FPSAdmin,FPSUser,PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class WorkGroupCos90sController : Controller
    {
        private readonly IWorkGroupService _workGroupService;
        private readonly IWorkGroupReportService _workGroupReportService;
        private readonly IMonthHourService _monthHourService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly ICalenderMonthService _calenderMonthService;
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;

        public WorkGroupCos90sController(
            IWorkGroupService workGroupService,
            IWorkGroupReportService workGroupReportService,
            IMonthHourService monthHourService,
            IProfitCentreService profitCentreService,
            ICalenderMonthService calenderMonthService,
            IEmployeeService employeeService,
            IMapper mapper)
        {
            _workGroupService = workGroupService;
            _workGroupReportService = workGroupReportService;
            _monthHourService = monthHourService;
            _profitCentreService = profitCentreService;
            _calenderMonthService = calenderMonthService;
            _employeeService = employeeService;
            _mapper = mapper;
        }

        /// <summary>
        /// Renders the COS90 page, pre-loading the first available profit centre's
        /// work groups and populating all dropdowns.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var profitCentreOptions = await GetProfitCentreSelectListAsync();
            var firstPc = profitCentreOptions.FirstOrDefault()?.Value;

            var individualOptions = await GetIndividualSelectListAsync();
            var calenderMonthItems = await GetCalenderMonthItemsAsync();
            var yearOptions = await GetYearOptionsAsync();

            var vm = new WorkGroupCos90sViewModel
            {
                ProfitCentreOptions = profitCentreOptions,
                IndividualOptions   = individualOptions,
                CalenderMonthItems  = calenderMonthItems,
                YearOptions         = yearOptions,
                SelectedProfitCentre = firstPc,
                SelectedYear = yearOptions.Contains((short)DateTime.Now.Year)
                    ? (short)DateTime.Now.Year
                    : yearOptions.FirstOrDefault()
            };

            if (!string.IsNullOrWhiteSpace(firstPc))
            {
                var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
                vm.WorkGroupGrid = await GetWorkGroupGridConfigAsync(defaultRequest, firstPc);
            }
            else
            {
                vm.WorkGroupGrid = BuildEmptyWorkGroupGrid();
            }

            return View(vm);
        }

        /// <summary>
        /// AJAX endpoint – reloads the work-group grid with updated pagination/filter/sort and
        /// selected profit centre. Returns the shared <c>_DataGrid</c> partial.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadWorkGroupGrid(
            PaginationFilter<string> request, string? profitCentre)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(profitCentre))
                return PartialView("_DataGrid", BuildEmptyWorkGroupGrid());

            var grid = await GetWorkGroupGridConfigAsync(request, profitCentre);
            return PartialView("_DataGrid", grid);
        }

        /// <summary>
        /// Sets the COS90 flag to <c>1</c> for all work groups in <paramref name="profitCentre"/>.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectPCWorkGroups(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                return BadRequest(new { error = "Profit Centre is required." });

            var result = await _workGroupService.SetCos90ForProfitCentreWorkGroupsAsync(profitCentre, 1);
            return result.Success
                ? Ok(new { success = true, message = $"All work groups for '{profitCentre}' flagged for COS90." })
                : StatusCode(500, new { error = "Failed to flag work groups for COS90." });
        }

        /// <summary>
        /// Clears the COS90 flag (sets to <c>0</c>) for all work groups in <paramref name="profitCentre"/>.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearPCWorkGroups(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                return BadRequest(new { error = "Profit Centre is required." });

            var result = await _workGroupService.SetCos90ForProfitCentreWorkGroupsAsync(profitCentre, 0);
            return result.Success
                ? Ok(new { success = true, message = $"COS90 flags cleared for all work groups in '{profitCentre}'." })
                : StatusCode(500, new { error = "Failed to clear COS90 flags." });
        }

        /// <summary>
        /// Clears the COS90 flag (sets to <c>0</c>) for every work group across all profit centres.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAllWorkGroups()
        {
            var result = await _workGroupService.SetCos90ForAllWorkGroupsAsync(0);
            return result.Success
                ? Ok(new { success = true })
                : StatusCode(500, new { error = "Failed to clear COS90 flags for all work groups." });
        }

        /// <summary>
        /// Returns the list of work groups currently flagged for COS90 (used for generation preview).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFlaggedWorkGroups()
        {
            var result = await _workGroupService.GetWorkGroupsFlaggedForCos90Async();
            if (!result.Success)
                return StatusCode(500, new { error = "Failed to retrieve flagged work groups." });

            return Ok(result.Data ?? new());
        }

        /// <summary>
        /// AJAX endpoint – returns the read-only Maintain Working Hours &amp; Days partial
        /// rendered inside a modal on the COS90 page.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMonthHourGrid(short? year)
        {
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = nameof(MonthHourRowItem.Year),
                Descending = false,
                Filter = "{}"
            };

            var grid = await GetMonthHourGridConfigAsync(request, year);
            return PartialView("_MonthHourGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadMonthHourGrid(PaginationFilter<string> request, short? year)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await GetMonthHourGridConfigAsync(request, year);
            return PartialView("_DataGrid", grid);
        }

        /// <summary>
        /// Returns distinct years available for the MonthHour year filter dropdown.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMonthHourYears()
        {
            var response = await _monthHourService.GetDistinctYearsAsync();
            var years = response.Success && response.Data != null ? response.Data : new List<short>();
            return Ok(years);
        }

        [HttpGet]
        public async Task<IActionResult> GetPeriods()
        {
            var months = await GetCalenderMonthItemsAsync();
            var periods = months
                .OrderBy(m => m.AccntsPeriod)
                .Select(m => new
                {
                    period = m.AccntsPeriod,
                    monthName = m.MonthName,
                    monthNumber = m.MonthNumber
                })
                .ToList();

            return Ok(periods);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWorkGroupCos90(string workGroupName, string profitCentre, short flag)
        {
            if (string.IsNullOrWhiteSpace(profitCentre) || string.IsNullOrWhiteSpace(workGroupName))
                return BadRequest(new { error = "Work Group and Profit Centre are required." });

            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 500,
                SortBy = nameof(WorkGroupCos90sWorkGroupItem.WorkGroupName),
                Descending = false,
                Filter = JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    [nameof(WorkGroupCos90sWorkGroupItem.WorkGroupName)] = workGroupName
                })
            };

            var lookup = await _workGroupService.GetWorkGroupsByProfitCentreAsync(query, profitCentre);
            if (!lookup.Success || lookup.Data == null)
                return StatusCode(500, new { error = "Failed to validate work group." });

            var existing = lookup.Data.FirstOrDefault(w =>
                string.Equals(w.WorkGroupName, workGroupName, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
                return NotFound(new { error = "Work Group not found for selected Profit Centre." });

            var setResult = await _workGroupService.SetCos90ForWorkGroupAsync(profitCentre, workGroupName, flag);
            return setResult.Success
                ? Ok(new { success = true })
                : StatusCode(500, new { error = "Failed to update COS90 flag." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportCos90s(string? selectedProfitCentre, short? selectedMonthNumber, short? selectedYear, string? pactId)
        {
            if (string.IsNullOrWhiteSpace(selectedProfitCentre))
                ModelState.AddModelError(nameof(WorkGroupCos90sViewModel.SelectedProfitCentre), "Profit Centre is required.");

            if (!selectedMonthNumber.HasValue || selectedMonthNumber.Value <= 0)
                ModelState.AddModelError(nameof(WorkGroupCos90sViewModel.SelectedMonthNumber), "For Period is required.");

            if (!selectedYear.HasValue || selectedYear.Value <= 0)
                ModelState.AddModelError(nameof(WorkGroupCos90sViewModel.SelectedYear), "In Year is required.");

            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var response = await _workGroupReportService.ExportCos90sAsync(
                selectedProfitCentre!,
                selectedMonthNumber!.Value,
                selectedYear!.Value,
                pactId);

            if (!response.Success || response.Data == null || response.Data.Content == null || response.Data.Content.Length == 0)
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to generate COS90 Excel.",
                    errors = new[]
                    {
                        new { field = string.Empty, message = "Failed to generate COS90 Excel." }
                    }
                });

            var contentType = string.IsNullOrWhiteSpace(response.Data.ContentType)
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : response.Data.ContentType;

            var fileName = string.IsNullOrWhiteSpace(response.Data.FileName)
                ? $"COS90_{selectedProfitCentre}_{selectedYear}_{selectedMonthNumber:D2}.xlsx"
                : response.Data.FileName;

            return File(response.Data.Content, contentType, fileName);
        }

        // ── Private helpers ─────────────────────────────────────────────────

        private async Task<DataGridConfig<WorkGroupCos90sWorkGroupItem>> GetWorkGroupGridConfigAsync(
            PaginationFilter<string> request, string profitCentre)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);

            var response = await _workGroupService.GetWorkGroupsByProfitCentreAsync(query, profitCentre);

            var items = response.Success && response.Data != null
                ? response.Data
                    .Select(w => new WorkGroupCos90sWorkGroupItem
                    {
                        WorkGroupName = w.WorkGroupName,
                        ProfitCentre  = w.ProfitCentre,
                        Cos90Flagged  = w.Cos90 == 1,
                        FpsYear       = w.FpsYear
                    })
                    .ToList()
                : new List<WorkGroupCos90sWorkGroupItem>();

            var pagination = new PaginationModel
            {
                TotalRecords  = response.Pagination?.TotalRecords ?? 0,
                PageNumber    = request.Page,
                PageSize      = request.PageSize,
                SortColumn    = request.SortBy    ?? nameof(WorkGroupCos90sWorkGroupItem.WorkGroupName),
                SortDirection = request.Descending
            };

            return BuildWorkGroupGrid(items, pagination, filterDict);
        }

        private static DataGridConfig<WorkGroupCos90sWorkGroupItem> BuildEmptyWorkGroupGrid() =>
            BuildWorkGroupGrid(new List<WorkGroupCos90sWorkGroupItem>(), new PaginationModel(), new Dictionary<string, string>());

        private async Task<DataGridConfig<MonthHourRowItem>> GetMonthHourGridConfigAsync(
            PaginationFilter<string> request, short? year)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            if (year is > 0)
                filterDict[nameof(MonthHourRowItem.Year)] = year.Value.ToString();

            var query = _mapper.Map<QueryParameters<string>>(request);
            query.Page = query.Page < 1 ? 1 : query.Page;
            query.PageSize = query.PageSize < 1 ? 10 : query.PageSize;
            query.SortBy = string.IsNullOrWhiteSpace(query.SortBy) ? nameof(MonthHourRowItem.Year) : query.SortBy;
            query.Filter = JsonConvert.SerializeObject(filterDict);

            var response = await _monthHourService.GetAllAsync(query);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<MonthHourRowItem>>(response.Data)
                : new List<MonthHourRowItem>();

            return new DataGridConfig<MonthHourRowItem>
            {
                GridId = "cos90MonthHourGrid",
                Title = string.Empty,
                KeyProperty = nameof(MonthHourRowItem.Month),
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowCopy = false,
                AllowExport = false,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                BindGridUrl = "/PACT/WorkGroupCos90s/LoadMonthHourGrid",
                ExtraFilterMethod = "getMonthHourGridExtraFilters",
                Data = items,
                CurrentFilters = filterDict,
                Pagination = new PaginationModel
                {
                    TotalRecords = response.Pagination?.TotalRecords ?? 0,
                    PageNumber = response.Pagination?.PageNumber ?? query.Page,
                    PageSize = response.Pagination?.PageSize ?? query.PageSize,
                    SortColumn = query.SortBy,
                    SortDirection = query.Descending
                },
                Columns = GridDataProvider.GetColumnsDefination<MonthHourRowItem>()
            };
        }

        private static DataGridConfig<WorkGroupCos90sWorkGroupItem> BuildWorkGroupGrid(
            List<WorkGroupCos90sWorkGroupItem> items,
            PaginationModel pagination,
            Dictionary<string, string> filterDict)
        {
            return new DataGridConfig<WorkGroupCos90sWorkGroupItem>
            {
                GridId             = "cos90WorkGroupGrid",
                Title              = "Work Groups",
                KeyProperty        = nameof(WorkGroupCos90sWorkGroupItem.WorkGroupName),
                AllowAdd           = false,
                AllowEdit          = true,
                EditFunction       = "editCos90WorkGroup",
                AllowDelete        = false,
                AllowCopy          = false,
                AllowExport        = false,
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                ExtraFilterMethod  = "getCos90GridExtraFilters",
                BindGridUrl        = "/PACT/WorkGroupCos90s/LoadWorkGroupGrid",
                Data               = items,
                Pagination         = pagination,
                CurrentFilters     = filterDict,
                Columns            = GridDataProvider.GetColumnsDefination<WorkGroupCos90sWorkGroupItem>()
            };
        }

        private async Task<List<SelectListItem>> GetProfitCentreSelectListAsync()
        {
            var response = await _profitCentreService.GetAllProfitCentresAsync();
            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .Where(pc => !string.IsNullOrWhiteSpace(pc.ProfitCentreId))
                .Select(pc => new SelectListItem(pc.ProfitCentreId, pc.ProfitCentreId))
                .ToList();
        }

        private async Task<List<SelectListItem>> GetIndividualSelectListAsync()
        {
            var response = await _employeeService.GetActiveStaffAsync();
            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .Where(s => !string.IsNullOrWhiteSpace(s.PactId) && !string.IsNullOrWhiteSpace(s.Name))
                .Select(s => new SelectListItem($"{s.Name} ({s.WorkGroupGrade})", s.PactId))
                .ToList();
        }

        private async Task<List<CalenderMonthDto>> GetCalenderMonthItemsAsync()
        {
            var response = await _calenderMonthService.GetCalenderMonthsAsync();
            if (!response.Success || response.Data == null)
                return new List<CalenderMonthDto>();

            return response.Data
                .OrderBy(m => m.AccntsPeriod)
                .ThenBy(m => m.MonthNumber)
                .ToList();
        }

        private async Task<List<short>> GetYearOptionsAsync()
        {
            var response = await _monthHourService.GetDistinctYearsAsync();
            if (!response.Success || response.Data == null)
                return new List<short>();

            return response.Data
                .OrderBy(y => y)
                .ToList();
        }
    }
}

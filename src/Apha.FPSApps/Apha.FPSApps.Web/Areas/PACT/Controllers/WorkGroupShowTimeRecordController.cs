using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class WorkGroupShowTimeRecordController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;
        private readonly ICalenderMonthService _calenderMonthService;

        public WorkGroupShowTimeRecordController(
            IMapper mapper,
            IWorkGroupService workGroupService,
            ICalenderMonthService calenderMonthService)
        {
            _mapper = mapper;
            _workGroupService = workGroupService;
            _calenderMonthService = calenderMonthService;
        }

        /// <summary>
        /// Renders the Work Group Show Time Records index page with work group and calendar month dropdowns.
        /// An optional <paramref name="workGroup"/> query parameter pre-selects the work group dropdown.
        /// </summary>
        public async Task<IActionResult> Index(string? workGroup = null)
        {
            var workGroupOptions = await GetWorkGroupsAsync();
            var calenderMonthOptions = await GetCalenderMonthsAsync();

            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var timeRecordsGrid = await BuildTimeRecordsGridAsync(defaultRequest, workGroup, null);

            var viewModel = new WorkGroupShowTimeRecordViewModel
            {
                SelectedWorkGroup = workGroup,
                WorkGroupOptions = workGroupOptions,
                CalenderMonthOptions = calenderMonthOptions,
                TimeRecordsGrid = timeRecordsGrid
            };

            return View(viewModel);
        }

        // ── GRID LOAD ─────────────────────────────────────────────────────────

        /// <summary>
        /// Loads the time records data grid filtered by the selected work group and calendar month.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadTimeRecordsGrid(
            PaginationFilter<string> request, string? workGroup, int? monthNumber)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTimeRecordsGridAsync(request, workGroup, monthNumber);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        /// <summary>
        /// Builds and returns a <see cref="DataGridConfig{WorkGroupTimeCodeItem}"/> populated with
        /// records filtered by work group and optional month number.
        /// </summary>
        private async Task<DataGridConfig<WorkGroupTimeCodeItem>> BuildTimeRecordsGridAsync(
            PaginationFilter<string> request, string? workGroup, int? monthNumber)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? [];

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _workGroupService.GetPagedWorkGroupTimeCodesAsync(query, workGroup, monthNumber);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<WorkGroupTimeCodeItem>>(response.Data)
                : [];

            var pagination = response.Pagination != null
                ? new PaginationModel
                {
                    TotalRecords = response.Pagination.TotalRecords,
                    PageNumber = response.Pagination.PageNumber,
                    PageSize = response.Pagination.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                }
                : new PaginationModel { SortColumn = request.SortBy, SortDirection = request.Descending };

            return new DataGridConfig<WorkGroupTimeCodeItem>
            {
                GridId = "timeRecordsGrid",
                Title = "Time Records",
                ShowCheckboxColumn = false,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ShowPagination = true,
                AllowRowSelection = false,
                KeyProperty = "PACTStaffID",
                ExtraFilterMethod = "getTimeRecordsExtraFilters",
                BindGridUrl = "/PACT/WorkGroupShowTimeRecord/LoadTimeRecordsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<WorkGroupTimeCodeItem>(null),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        ///
        /// Retrieves and maps the list of available work groups for the work group selection dropdown.
        /// </summary>
        private async Task<List<WorkGroup>> GetWorkGroupsAsync()
        {
            var response = await _workGroupService.GetAllWorkGroupsAsync();
            if (!response.Success || response.Data == null)
                return [];

            return _mapper.Map<List<WorkGroup>>(response.Data);
        }

        /// <summary>
        /// Retrieves and maps the list of calendar months for the month selection dropdown.
        /// </summary>
        private async Task<List<CalenderMonth>> GetCalenderMonthsAsync()
        {
            var response = await _calenderMonthService.GetCalenderMonthsAsync();
            if (!response.Success || response.Data == null)
                return [];

            return _mapper.Map<List<CalenderMonth>>(response.Data);
        }
    }
}
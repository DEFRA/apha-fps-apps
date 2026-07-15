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
    public class WorkGroupValidTimeCodeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;

        /// <summary>
        /// Initialises a new instance of <see cref="WorkGroupValidTimeCodeController"/> with the required
        /// mapper and work group service dependencies.
        /// </summary>
        /// <param name="mapper">AutoMapper instance used to project DTOs to view models.</param>
        /// <param name="workGroupService">Service for retrieving work group and valid time code data.</param>
        public WorkGroupValidTimeCodeController(IMapper mapper, IWorkGroupService workGroupService)
        {
            _mapper = mapper;
            _workGroupService = workGroupService;
        }

        /// <summary>
        /// Renders the Work Group Valid Time Code index page with the work group dropdown.
        /// An optional <paramref name="workGroup"/> query parameter pre-selects the work group dropdown.
        /// </summary>
        /// <param name="workGroup">Optional work group name used to pre-select the work group dropdown on page load.</param>
        /// <returns>A <see cref="ViewResult"/> containing a <see cref="WorkGroupValidTimeCodeViewModel"/> with dropdown options and the initial grid.</returns>
        public async Task<IActionResult> Index(string workGroup = "")
        {
            TempData["NavigationSource"] = "WorkGroupValidTimeCode";
            TempData["SelectedWorkGroup"] = workGroup;
            var workGroupOptions = await GetWorkGroupsAsync();
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var validTimeCodesGrid = await BuildValidTimeCodesGridAsync(defaultRequest, workGroup);

            var viewModel = new WorkGroupValidTimeCodeViewModel
            {
                SelectedWorkGroup = workGroup,
                WorkGroupOptions = workGroupOptions,
                ValidTimeCodesGrid = validTimeCodesGrid
            };

            return View(viewModel);
        }

        // ── GRID LOAD ─────────────────────────────────────────────────────────

        /// <summary>
        /// Loads the valid time codes data grid filtered by the selected work group.
        /// Returns a JSON error object when the request model state is invalid.
        /// </summary>
        /// <param name="request">Pagination, sorting, and filter parameters for the data grid request.</param>
        /// <param name="workGroup">Optional work group name to filter valid time codes by.</param>
        /// <returns>
        /// A <see cref="PartialViewResult"/> rendering the <c>_DataGrid</c> partial view when the request is valid;
        /// otherwise a <see cref="JsonResult"/> containing validation error details.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> LoadValidTimeCodesGrid(
            PaginationFilter<string> request, string workGroup = "")
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildValidTimeCodesGridAsync(request, workGroup);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private async Task<DataGridConfig<WorkGroupValidTimeCodeItem>> BuildValidTimeCodesGridAsync(
            PaginationFilter<string> request, string workGroup)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? [];

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _workGroupService.GetPagedWorkGroupValidTimeCodesAsync(query, workGroup);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<WorkGroupValidTimeCodeItem>>(response.Data)
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

            return new DataGridConfig<WorkGroupValidTimeCodeItem>
            {
                GridId = "validTimeCodesGrid",
                Title = "Valid Time Codes",
                ShowCheckboxColumn = false,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ShowPagination = true,
                AllowRowSelection = true,
                KeyProperty = "TimeCode",
                RowSelectFunction = "onValidTimeCodeRowSelect",
                ExtraFilterMethod = "getValidTimeCodesExtraFilters",
                BindGridUrl = "/PACT/WorkGroupValidTimeCode/LoadValidTimeCodesGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<WorkGroupValidTimeCodeItem>(null),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task<List<WorkGroup>> GetWorkGroupsAsync()
        {
            var response = await _workGroupService.GetAllWorkGroupsAsync();
            if (!response.Success || response.Data == null)
                return [];

            return _mapper.Map<List<WorkGroup>>(response.Data);
        }
    }
}
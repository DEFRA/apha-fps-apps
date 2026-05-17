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
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class MonthlyOutputLogController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IPactMonthlyOutputService _logService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly IProjectService _projectService;

        public MonthlyOutputLogController(
            IMapper mapper,
            IPactMonthlyOutputService logService,
            ITestCapabilityService testCapabilityService,
            IProjectService projectService)
        {
            _mapper = mapper;
            _logService = logService;
            _testCapabilityService = testCapabilityService;
            _projectService = projectService;
        }

        public async Task<IActionResult> Index()
        {
            var workGroupsResponse = await _testCapabilityService.GetAllWorkGroupsAsync();
            var testsResponse = await _testCapabilityService.GetPagedByWorkGroupAsync(
                new QueryParameters<string> { Page = 1, PageSize = 9999 }, null);
            var projectsResponse = await _projectService.GetAllPactProjectsAsync();

            var viewModel = new MonthlyOutputLogViewModel
            {
                LogGrid = BuildEmptyGrid(),
                WorkGroupOptions = workGroupsResponse.Success && workGroupsResponse.Data != null
                    ? workGroupsResponse.Data
                        .Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName))
                        .OrderBy(x => x.Text)
                        .ToList()
                    : new List<SelectListItem>(),
                TestCodeOptions = testsResponse.Success && testsResponse.Data != null
                    ? testsResponse.Data
                        .Select(t => new SelectListItem(t.TestCode, t.TestCode))
                        .DistinctBy(x => x.Value)
                        .OrderBy(x => x.Text)
                        .ToList()
                    : new List<SelectListItem>(),
                ProjectOptions = projectsResponse.Success && projectsResponse.Data != null
                    ? projectsResponse.Data
                        .Select(p => new SelectListItem(
                            $"{p.ParentProject} — {p.ProjectTitle}", p.ParentProject))
                        .OrderBy(x => x.Value)
                        .ToList()
                    : new List<SelectListItem>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Search(
            PaginationFilter<string> request,
            string? workGroup,
            string? testCode,
            string? buyer,
            string? buyingTest,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var effectiveBuyer = !string.IsNullOrWhiteSpace(buyingTest) ? buyingTest : buyer;

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _logService.SearchAsync(
                query, workGroup, testCode, effectiveBuyer,
                dateImported, month, userId, insertDelete);

            if (!response.Success || response.Data == null)
            {
                var emptyGrid = BuildEmptyGrid();
                return PartialView("_DataGrid", emptyGrid);
            }

            var items = response.Data.Select(d => _mapper.Map<MonthlyOutputLogItem>(d)).ToList();
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var gridConfig = new DataGridConfig<MonthlyOutputLogItem>
            {
                GridId = "moLogGrid",
                Title = "Monthly Output Log",
                ShowCheckboxColumn = false,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ShowPagination = true,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<MonthlyOutputLogItem>(null),
                Pagination = new PaginationModel
                {
                    PageNumber = response.Pagination?.PageNumber ?? 1,
                    PageSize = response.Pagination?.PageSize ?? 20,
                    //TotalPages = response.Pagination?.TotalPages ?? 1,
                    TotalRecords = response.Pagination?.TotalRecords ?? items.Count,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict,
                BindGridUrl = "/PACT/MonthlyOutputLog/Search"
            };

            return PartialView("_DataGrid", gridConfig);
        }

        private static DataGridConfig<MonthlyOutputLogItem> BuildEmptyGrid() =>
            new()
            {
                GridId = "moLogGrid",
                Title = "Monthly Output Log",
                ShowCheckboxColumn = false,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ShowPagination = false,
                Data = new List<MonthlyOutputLogItem>(),
                Columns = GridDataProvider.GetColumnsDefination<MonthlyOutputLogItem>(null),
                Pagination = new PaginationModel(),
                BindGridUrl = "/PACT/MonthlyOutputLog/Search"
            };
    }
}

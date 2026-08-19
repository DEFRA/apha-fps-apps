using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class QueriesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IQueriesService _queriesService;
        private readonly IRadTrackInvoiceService _radTrackInvoiceService;
        private readonly IProjectDetailsService _projectDetailsService;

        public QueriesController(
            IMapper mapper,
            IQueriesService queriesService,
            IRadTrackInvoiceService radTrackInvoiceService,
            IProjectDetailsService projectDetailsService)
        {
            _mapper = mapper;
            _queriesService = queriesService;
            _radTrackInvoiceService = radTrackInvoiceService;
            _projectDetailsService = projectDetailsService;
        }

        public async Task<IActionResult> Index()
        {
            DateTime now = DateTime.Now;

            QueriesViewModel viewModel = new()
            {
                QueryResultsGrid = BuildQueryResultsGrid(
                    items: [],
                    filterDict: new Dictionary<string, string>(),
                    pagination: new PaginationModel(),
                    exportQueryType: null),
                SelectedMonth = now.Month,
                SelectedYear = now.Year
            };

            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadQueryResultsGrid(
            PaginationFilter<string> request,
            string? month = null,
            string? year = null,
            string? contract = null,
            string? exportQuery = null)
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

            if (string.Equals(exportQuery, "programMonitoring", StringComparison.OrdinalIgnoreCase))
            {
                DataGridConfig<ProgramCustomerMonitoringResultItem> programMonitoringGridConfig =
                    await BuildProgramCustomerMonitoringGridAsync(request, month, year);
                return PartialView("_DataGrid", programMonitoringGridConfig);
            }

            DataGridConfig<QueryResultItem> gridConfig = await BuildQueryResultsGridAsync(request, month, year, contract, exportQuery);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<QueryResultItem>> BuildQueryResultsGridAsync(
            PaginationFilter<string> request,
            string? month,
            string? year,
            string? contract,
            string? exportQuery)
        {
            Dictionary<string, string> filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            List<QueryResultItem> items = [];
            PaginationModel paginationModel = new()
            {
                PageNumber = request.Page,
                PageSize = request.PageSize,
                SortColumn = request.SortBy,
                SortDirection = request.Descending
            };

            string selectedQuery = exportQuery ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(selectedQuery)
                && short.TryParse(year, out short reportYear)
                && short.TryParse(month, out short reportMonth)
                && reportMonth >= 1
                && reportMonth <= 12)
            {
                QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
                queryParameters.Filter = request.Filter;

                Dictionary<string, string> serviceFilterDict =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(queryParameters.Filter ?? "{}") ?? new();
                serviceFilterDict["ExportQueryType"] = selectedQuery;
                queryParameters.Filter = JsonConvert.SerializeObject(serviceFilterDict);

                bool shouldApplyContractFilter = string.Equals(selectedQuery, "contractMonitoring", StringComparison.OrdinalIgnoreCase);
                string contractFilter = shouldApplyContractFilter && !string.IsNullOrWhiteSpace(contract)
                    ? contract
                    : "*";

                var response = await _queriesService.GetMonitoringReportDataAsync(
                    queryParameters,
                    reportYear,
                    reportMonth,
                    contractFilter);

                if (response.Success && response.Data != null)
                {
                    items = _mapper.Map<List<QueryResultItem>>(response.Data);
                }

                if (response.Pagination != null)
                {
                    paginationModel = _mapper.Map<PaginationModel>(response.Pagination);
                    paginationModel.SortColumn = request.SortBy;
                    paginationModel.SortDirection = request.Descending;
                }
            }

            return BuildQueryResultsGrid(
                items,
                filterDict,
                paginationModel,
                selectedQuery);
        }

        private async Task<DataGridConfig<ProgramCustomerMonitoringResultItem>> BuildProgramCustomerMonitoringGridAsync(
            PaginationFilter<string> request,
            string? month,
            string? year)
        {
            Dictionary<string, string> filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            List<ProgramCustomerMonitoringResultItem> items = [];
            PaginationModel paginationModel = new()
            {
                PageNumber = request.Page,
                PageSize = request.PageSize,
                SortColumn = request.SortBy,
                SortDirection = request.Descending
            };

            if (short.TryParse(year, out short reportYear)
                && short.TryParse(month, out short reportMonth)
                && reportMonth >= 1
                && reportMonth <= 12)
            {
                QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
                queryParameters.Filter = request.Filter;

                var response = await _queriesService.GetProgramCustomerMonitoringReportDataAsync(
                    queryParameters,
                    reportYear,
                    reportMonth);

                if (response.Success && response.Data != null)
                {
                    items = _mapper.Map<List<ProgramCustomerMonitoringResultItem>>(response.Data);
                }

                if (response.Pagination != null)
                {
                    paginationModel = _mapper.Map<PaginationModel>(response.Pagination);
                    paginationModel.SortColumn = request.SortBy;
                    paginationModel.SortDirection = request.Descending;
                }
            }

            return BuildProgramCustomerMonitoringGrid(items, filterDict, paginationModel);
        }

        private async Task PopulateDropdownsAsync(QueriesViewModel model)
        {
            List<SelectListItem> contractOptions = [new SelectListItem("Select Contract", "")];

            Task<ApiResponseDto<List<string>>> contractsTask = _radTrackInvoiceService.GetContractsAsync();
            Task<ApiResponseDto<List<YearDto>>> yearsTask = _projectDetailsService.GetAllYearAsync();

            await Task.WhenAll(contractsTask, yearsTask);

            if (contractsTask.Result is { Success: true, Data: not null })
            {
                contractOptions.AddRange(contractsTask.Result.Data
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c)
                    .Select(c => new SelectListItem(c, c)));
            }

            model.ContractOptions = contractOptions;
            model.YearOptions = yearsTask.Result?.Data?
                .OrderBy(y => y.Value)
                .Select(y => new SelectListItem(y.Value.ToString(), y.Value.ToString(), y.Value == model.SelectedYear))
                .ToList() ?? [];

            if (model.YearOptions.Count > 0 && model.YearOptions.All(y => !y.Selected))
            {
                model.YearOptions[0].Selected = true;
                if (int.TryParse(model.YearOptions[0].Value, out int fallbackYear))
                {
                    model.SelectedYear = fallbackYear;
                }
            }
        }

        private static DataGridConfig<QueryResultItem> BuildQueryResultsGrid(
            List<QueryResultItem> items,
            Dictionary<string, string> filterDict,
            PaginationModel pagination,
            string? exportQueryType)
        {
            List<DataGridColumn> columns = GridDataProvider.GetColumnsDefination<QueryResultItem>();

            DataGridColumn? yearColumn = columns.FirstOrDefault(c => c.PropertyName == nameof(QueryResultItem.Year));
            DataGridColumn? projectColumn = columns.FirstOrDefault(c => c.PropertyName == nameof(QueryResultItem.Project));
            DataGridColumn? parentProjectColumn = columns.FirstOrDefault(c => c.PropertyName == nameof(QueryResultItem.ParentProject));

            if (yearColumn != null)
                yearColumn.IsVisible = false;

            if (projectColumn != null)
                projectColumn.IsVisible = false;

            if (parentProjectColumn != null)
            {
                parentProjectColumn.IsVisible = true;
                parentProjectColumn.DisplayName = "Project";
            }

            string[] filterableColumns =
            [
                nameof(QueryResultItem.Program),
                nameof(QueryResultItem.Project),
                nameof(QueryResultItem.ParentProject),
                nameof(QueryResultItem.ProjectTitle),
                nameof(QueryResultItem.Manager),
                nameof(QueryResultItem.ProjectStatus),
                nameof(QueryResultItem.Contract)
            ];

            foreach (DataGridColumn column in columns.Where(c => filterableColumns.Contains(c.PropertyName)))
            {
                column.IsFilterable = true;
            }

            return new DataGridConfig<QueryResultItem>
            {
                GridId = "queryResultsGrid",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ExtraFilterMethod = "getQueryExtraFilters",
                BindGridUrl = "/PIMS/Queries/LoadQueryResultsGrid",
                Data = items,
                Columns = columns,
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private static DataGridConfig<ProgramCustomerMonitoringResultItem> BuildProgramCustomerMonitoringGrid(
            List<ProgramCustomerMonitoringResultItem> items,
            Dictionary<string, string> filterDict,
            PaginationModel pagination)
        {
            List<DataGridColumn> columns = GridDataProvider.GetColumnsDefination<ProgramCustomerMonitoringResultItem>();

            string[] filterableColumns =
            [
                nameof(ProgramCustomerMonitoringResultItem.Program),
                nameof(ProgramCustomerMonitoringResultItem.Project),
                nameof(ProgramCustomerMonitoringResultItem.ParentProject),
                nameof(ProgramCustomerMonitoringResultItem.ProjectTitle),
                nameof(ProgramCustomerMonitoringResultItem.Manager),
                nameof(ProgramCustomerMonitoringResultItem.ProjectStatus),
                nameof(ProgramCustomerMonitoringResultItem.Customer),
                nameof(ProgramCustomerMonitoringResultItem.Contract)
            ];

            foreach (DataGridColumn column in columns.Where(c => filterableColumns.Contains(c.PropertyName)))
            {
                column.IsFilterable = true;
            }

            return new DataGridConfig<ProgramCustomerMonitoringResultItem>
            {
                GridId = "queryResultsGrid",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ExtraFilterMethod = "getQueryExtraFilters",
                BindGridUrl = "/PIMS/Queries/LoadQueryResultsGrid",
                Data = items,
                Columns = columns,
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }
    }
}

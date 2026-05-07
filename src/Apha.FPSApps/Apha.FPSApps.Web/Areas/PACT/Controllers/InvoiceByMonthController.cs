using Apha.FPSApps.Application.Dtos.PACT;
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
    public class InvoiceByMonthController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;

        public InvoiceByMonthController(IMapper mapper, IProjectInvoiceService invoiceService)
        {
            _mapper = mapper;
            _invoiceService = invoiceService;
        }        

        /// <summary>
        /// Displays the invoice-by-month pivot summary page with a dynamically columned data grid
        /// showing monthly invoice amounts grouped by program and parent project.
        /// </summary>
        /// <returns>The InvoiceByMonth Index view populated with the initial pivot grid configuration.</returns>
        public async Task<IActionResult> Index()
        {
            var grid = await BuildGridAsync(new PaginationFilter<string>());
            return View(new InvoiceByMonthViewModel { Grid = grid });
        }       

        /// <summary>
        /// Reloads the invoice-by-month pivot grid partial view based on the supplied pagination, sort, and filter parameters.
        /// </summary>
        /// <param name="request">Pagination and filter parameters submitted from the data grid.</param>
        /// <returns>A partial view containing the refreshed pivot grid, or <see cref="BadRequestResult"/> if the model state is invalid.</returns>
        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildGridAsync(request);
            return PartialView("_DataGrid", grid);
        }        

        /// <summary>
        /// Builds the monthly invoice pivot grid configuration by fetching summary data from the service,
        /// mapping flat rows into pivot row models, and generating dynamic month columns based on the
        /// months present in the returned data.
        /// </summary>
        /// <remarks>
        /// Month columns are derived from the financial year period where period 1 = April through period 12 = March.
        /// Each column is labelled in the format "period-MonthAbbr" (e.g. "1-Apr").
        /// </remarks>
        /// <param name="request">Pagination and filter parameters for the grid query.</param>
        /// <returns>A fully configured <see cref="DataGridConfig{T}"/> with dynamic month columns ready for rendering.</returns>
        private async Task<DataGridConfig<MonthlyInvoicePivotRow>> BuildGridAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _invoiceService.GetMonthlyInvoicesSummaryAsync(query);
            var pivot = response.Success && response.Data != null
                ? response.Data
                : new MonthlyInvoicesPivotDto();

            var rows = pivot.Rows.Select(r =>
            {
                var row = new MonthlyInvoicePivotRow
                {
                    Program = r.Program,
                    ParentProject = r.ParentProject
                };
                foreach (var kvp in r.MonthlyAmounts)
                {
                    switch (kvp.Key)
                    {
                        case 1:  row.M1  = kvp.Value; break;
                        case 2:  row.M2  = kvp.Value; break;
                        case 3:  row.M3  = kvp.Value; break;
                        case 4:  row.M4  = kvp.Value; break;
                        case 5:  row.M5  = kvp.Value; break;
                        case 6:  row.M6  = kvp.Value; break;
                        case 7:  row.M7  = kvp.Value; break;
                        case 8:  row.M8  = kvp.Value; break;
                        case 9:  row.M9  = kvp.Value; break;
                        case 10: row.M10 = kvp.Value; break;
                        case 11: row.M11 = kvp.Value; break;
                        case 12: row.M12 = kvp.Value; break;
                    }
                }
                return row;
            }).ToList();

            var columns = new List<DataGridColumn>
            {
                new() { PropertyName = "Program",       DisplayName = "Program",  ColumnType = GridColumnType.Text, IsFilterable = true, Width = 120 },
                new() { PropertyName = "ParentProject", DisplayName = "Project",  ColumnType = GridColumnType.Text, IsFilterable = true, Width = 150 }
            };

            foreach (int month in pivot.Months)
            {
                // Financial year: period 1 = Apr, 2 = May, ... 9 = Dec, 10 = Jan, 11 = Feb, 12 = Mar
                int calendarMonth = ((month + 2) % 12) + 1;
                string monthAbbr = new DateTime(2000, calendarMonth, 1, 0, 0, 0, DateTimeKind.Unspecified).ToString("MMM");

                columns.Add(new DataGridColumn
                {
                    PropertyName = $"M{month}",
                    DisplayName  = $"{month}-{monthAbbr}",
                    ColumnType   = GridColumnType.GbpValue,
                    IsFilterable = false,
                    Width        = 90
                });
            }

            var pagination = pivot.Pagination != null
                ? _mapper.Map<PaginationModel>(pivot.Pagination)
                : new PaginationModel();
            pagination.SortColumn    = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<MonthlyInvoicePivotRow>
            {
                GridId         = "invoiceByMonthGrid",
                KeyProperty    = "ParentProject",
                AllowAdd       = false,
                AllowEdit      = false,
                AllowDelete    = false,
                ShowPagination = true,
                BindGridUrl    = "/PACT/InvoiceByMonth/LoadGrid",
                Columns        = columns,
                Data           = rows,
                CurrentFilters = filterDict,
                Pagination     = pagination
            };
        }
    }
}

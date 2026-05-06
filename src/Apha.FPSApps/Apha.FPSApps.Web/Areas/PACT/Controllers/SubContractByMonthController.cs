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
    public class SubContractByMonthController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectSubContractService _subContractService;

        public SubContractByMonthController(IMapper mapper, IProjectSubContractService subContractService)
        {
            _mapper = mapper;
            _subContractService = subContractService;
        }

        // ── INDEX ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var grid = await BuildGridAsync(new PaginationFilter<string>());
            return View(new SubContractByMonthViewModel { Grid = grid });
        }

        // ── GRID RELOAD (called by DataGrid JS on sort / filter) ─────────────

        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        // ── PRIVATE ──────────────────────────────────────────────────────────

        private async Task<DataGridConfig<SubContractByMonthPivotRow>> BuildGridAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _subContractService.GetMonthlySubContractsSummaryAsync(query);
            var pivot = response.Success && response.Data != null
                ? response.Data
                : new MonthlySubContractsPivotDto();

            var rows = pivot.Rows.Select(r =>
            {
                var row = new SubContractByMonthPivotRow
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

            return new DataGridConfig<SubContractByMonthPivotRow>
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

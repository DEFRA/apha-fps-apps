using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class BBQueryController : Controller
    {
        private readonly IWorkGroupService _workGroupService;
        private readonly IBudgetBidsService _budgetBidsService;
        private readonly IProfitCentreService _profitCentreService;

        public BBQueryController(
            IWorkGroupService workGroupService,
            IBudgetBidsService budgetBidsService,
            IProfitCentreService profitCentreService)
        {
            _workGroupService = workGroupService;
            _budgetBidsService = budgetBidsService;
            _profitCentreService = profitCentreService;
        }

        /// <summary>
        /// Displays the Budget Bids cross-tab (BBQuery) page. The grid stays empty until a
        /// Resource Centre is selected. Data is built from the selected Resource Centre and the
        /// current FPS year, mirroring the Budget Bids cross-tab Excel report.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var profitCentreOptions = await GetProfitCentreSelectListAsync();
            var year = GetSelectedFpsYear();

            var grid = await BuildGridAsync(null);

            return View(new BBQueryViewModel
            {
                Grid = grid,
                ProfitCentreOptions = profitCentreOptions,
                SelectedProfitCentre = null,
                FpsYear = year
            });
        }

        /// <summary>
        /// Reloads the BBQuery cross-tab grid partial for the selected Resource Centre.
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadGrid(string? profitCentre)
        {
            var grid = await BuildGridAsync(profitCentre);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<BBQueryCrosstabRow>> BuildGridAsync(string? profitCentre)
        {
            var rows = new List<BBQueryCrosstabRow>();
            var columns = new List<DataGridColumn>
            {
                new() { PropertyName = "AccShortName", DisplayName = "AccShortName", ColumnType = GridColumnType.ReadOnly, IsFilterable = true,  Width = 160 },
                new() { PropertyName = "RowSummary",   DisplayName = "Row Summary",  ColumnType = GridColumnType.GbpValue, IsFilterable = false, Width = 120 }
            };

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                // Reuse the same query flow as the Budget Bids cross-tab export:
                // Resource Centre -> workgroups -> bids -> [account][workgroup] = GenBid lookup.
                var wgResponse = await _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(profitCentre);
                var workgroups = wgResponse.Success && wgResponse.Data != null
                    ? wgResponse.Data.Select(w => w.WorkGroupName).OrderBy(w => w).ToList()
                    : new List<string>();

                var allBids = new List<BidViewDto>();
                foreach (var wg in workgroups)
                {
                    var bidResponse = await _budgetBidsService.GetBidViewAsync(wg);
                    if (!bidResponse.Success || bidResponse.Data == null) continue;
                    allBids.AddRange(bidResponse.Data);
                }

                var bidLookup = allBids
                    .GroupBy(b => b.Account)
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToDictionary(b => b.WorkGroupName, b => b.GenBid));

                var categoriesResponse = await _budgetBidsService.GetAccountCategoriesAsync();
                var accounts = categoriesResponse.Success && categoriesResponse.Data?.Count > 0
                    ? categoriesResponse.Data.Select(a => a.AccShortName).OrderBy(a => a).ToList()
                    : allBids.Select(b => b.Account).Distinct().OrderBy(a => a).ToList();

                foreach (var wg in workgroups)
                {
                    columns.Add(new DataGridColumn
                    {
                        PropertyName = wg,
                        DisplayName  = wg,
                        ColumnType   = GridColumnType.GbpValue,
                        IsFilterable = false,
                        Width        = 110
                    });
                }

                foreach (var account in accounts)
                {
                    var row = new BBQueryCrosstabRow { AccShortName = account };
                    decimal rowTotal = 0;

                    foreach (var wg in workgroups)
                    {
                        decimal amount = 0;
                        if (bidLookup.TryGetValue(account, out var wgBids) &&
                            wgBids.TryGetValue(wg, out var value))
                        {
                            amount = value;
                        }

                        row.Values[wg] = amount;
                        rowTotal += amount;
                    }

                    row.RowSummary = rowTotal;
                    rows.Add(row);
                }
            }

            return new DataGridConfig<BBQueryCrosstabRow>
            {
                GridId            = "bbQueryGrid",
                KeyProperty       = "AccShortName",
                AllowAdd          = false,
                AllowEdit         = false,
                AllowDelete       = false,
                ShowPagination    = false,
                ExtraFilterMethod = "getBBQueryExtraFilters",
                BindGridUrl       = "/FPS/BBQuery/LoadGrid",
                Columns           = columns,
                Data              = rows,
                Pagination        = new PaginationModel()
            };
        }

        private int GetSelectedFpsYear()
        {
            if (HttpContext.Items.TryGetValue("SelectedFPSYear", out var yearObj) &&
                yearObj != null &&
                int.TryParse(yearObj.ToString(), out var year))
            {
                return year;
            }

            return DateTime.Now.Year;
        }

        private async Task<List<SelectListItem>> GetProfitCentreSelectListAsync()
        {
            var response = await _profitCentreService.GetProfitCentresAsync();
            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .Where(pc => !string.IsNullOrWhiteSpace(pc.ProfitCentreId))
                .Select(pc => new SelectListItem(pc.ProfitCentreId, pc.ProfitCentreId))
                .ToList();
        }
    }
}

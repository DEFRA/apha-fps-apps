using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using System.Text.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class MiscReportsController : Controller
    {
        private readonly IProfitCentreService _profitCentreService;

        public MiscReportsController(IProfitCentreService profitCentreService)
        {
            _profitCentreService = profitCentreService;
        }

        /// <summary>
        /// Displays the Misc Reports page. The grid stays empty until a Resource Centre is
        /// selected, mirroring the Budget Bids Query page behaviour.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var profitCentreOptions = await GetProfitCentreSelectListAsync();
            var year = GetSelectedFpsYear();

            var grid = BuildGrid(null);

            return View(new MiscReportsViewModel
            {
                Grid = grid,
                ProfitCentreOptions = profitCentreOptions,
                SelectedProfitCentre = null,
                FpsYear = year
            });
        }

        /// <summary>
        /// Reloads the Misc Reports grid partial for the selected Resource Centre.
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult LoadGrid(string? profitCentre, string? sortBy = null, bool descending = false, string? filter = null, int page = 1, int pageSize = 20)
        {
            var grid = BuildGrid(profitCentre, sortBy, descending, filter, page, pageSize);
            return PartialView("_DataGrid", grid);
        }

        private static DataGridConfig<Dictionary<string, string?>> BuildGrid(string? profitCentre, string? sortBy = null, bool descending = false, string? filter = null, int page = 1, int pageSize = 20)
        {
            var rows = new List<Dictionary<string, string?>>();
            var columns = new List<DataGridColumn>
            {
                new() { PropertyName = "ProfitCentre", DisplayName = "Resource Centre", ColumnType = GridColumnType.ReadOnly, IsFilterable = true, Width = 200 },
                new() { PropertyName = "Report",       DisplayName = "Report",          ColumnType = GridColumnType.ReadOnly, IsFilterable = true, Width = 240 }
            };

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                // TODO: Populate report rows for the selected Resource Centre once the
                // Misc Reports data source is available. Left intentionally empty so the
                // page renders the grid shell consistently with the Budget Bids Query page.
            }

            var filters = ParseFilters(filter);
            rows = ApplyFilters(rows, filters);
            rows = ApplySorting(rows, sortBy, descending);

            var pageNumber = page > 0 ? page : 1;
            var itemsPerPage = pageSize > 0 ? pageSize : 20;
            var totalRecords = rows.Count;

            var pagedRows = rows
                .Skip((pageNumber - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            return new DataGridConfig<Dictionary<string, string?>>
            {
                GridId            = "miscReportsGrid",
                KeyProperty       = "ProfitCentre",
                AllowAdd          = false,
                AllowEdit         = false,
                AllowDelete       = false,
                ShowPagination    = true,
                ExtraFilterMethod = "getMiscReportsExtraFilters",
                BindGridUrl       = "/FPS/MiscReports/LoadGrid",
                Columns           = columns,
                Data              = pagedRows,
                CurrentFilters    = filters,
                Pagination        = new PaginationModel
                {
                    TotalRecords  = totalRecords,
                    PageNumber    = pageNumber,
                    PageSize      = itemsPerPage,
                    SortColumn    = sortBy,
                    SortDirection = descending
                }
            };
        }

        /// <summary>
        /// Parses the JSON filter payload posted by the DataGrid into a column/value map.
        /// Returns null when there is nothing to filter on.
        /// </summary>
        private static Dictionary<string, string>? ParseFilters(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return null;

            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(filter);
                if (parsed == null || parsed.Count == 0)
                    return null;

                var cleaned = parsed
                    .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return cleaned.Count > 0 ? cleaned : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static List<Dictionary<string, string?>> ApplyFilters(List<Dictionary<string, string?>> rows, Dictionary<string, string>? filters)
        {
            if (filters == null || filters.Count == 0 || rows.Count == 0)
                return rows;

            return rows.Where(row => filters.All(f => RowMatchesFilter(row, f.Key, f.Value))).ToList();
        }

        private static bool RowMatchesFilter(Dictionary<string, string?> row, string column, string value)
        {
            var cellValue = row.TryGetValue(column, out var v) ? v : null;

            return cellValue != null &&
                   cellValue.Contains(value, StringComparison.OrdinalIgnoreCase);
        }

        private static List<Dictionary<string, string?>> ApplySorting(List<Dictionary<string, string?>> rows, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy) || rows.Count == 0)
                return rows;

            Func<Dictionary<string, string?>, string?> keySelector =
                r => r.TryGetValue(sortBy, out var v) ? v : null;

            return descending
                ? rows.OrderByDescending(keySelector, StringComparer.OrdinalIgnoreCase).ToList()
                : rows.OrderBy(keySelector, StringComparer.OrdinalIgnoreCase).ToList();
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

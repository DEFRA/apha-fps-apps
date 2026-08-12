using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class QueriesController : Controller
    {
        public IActionResult Index()
        {
            QueriesViewModel viewModel = new()
            {
                QueryResultsGrid = BuildQueryResultsGrid(null, null, null)
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult LoadQueryResultsGrid(
            string? month = null, string? year = null, string? contract = null,
            string? sortBy = null, bool descending = false)
        {
            DataGridConfig<QueryResultItem> gridConfig = BuildQueryResultsGrid(month, year, contract, sortBy, descending);
            return PartialView("_DataGrid", gridConfig);
        }

        private static DataGridConfig<QueryResultItem> BuildQueryResultsGrid(
            string? month, string? year, string? contract, string? sortBy = null, bool descending = false)
        {
            List<QueryResultItem> allItems =
            [
                new() { Project = "PRJ-001", Contract = "NZ001", Manager = "John Smith", Status = "Active", PlanCosts = 50000.00m, YtdCosts = 35250.00m, Comments = "On track" },
                new() { Project = "PRJ-002", Contract = "NZ001", Manager = "Jane Doe", Status = "Active", PlanCosts = 75000.00m, YtdCosts = 45600.00m, Comments = "Delayed" },
                new() { Project = "PRJ-003", Contract = "NZ002", Manager = "Mike Johnson", Status = "Active", PlanCosts = 60000.00m, YtdCosts = 60000.00m, Comments = "Completed" }
            ];

            List<QueryResultItem> items = string.IsNullOrEmpty(contract)
                ? []
                : allItems.Where(i => i.Contract == contract).ToList();

            if (!string.IsNullOrEmpty(sortBy))
            {
                var property = typeof(QueryResultItem).GetProperty(sortBy,
                    System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property != null)
                {
                    items = descending
                        ? items.OrderByDescending(i => property.GetValue(i, null)).ToList()
                        : items.OrderBy(i => property.GetValue(i, null)).ToList();
                }
            }

            return new DataGridConfig<QueryResultItem>
            {
                GridId = "queryResultsGrid",
                ShowCheckboxColumn = false,
                ShowPagination = false,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ExtraFilterMethod = "getQueryExtraFilters",
                BindGridUrl = "/PIMS/Queries/LoadQueryResultsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<QueryResultItem>(),
                Pagination = new PaginationModel
                {
                    SortColumn = sortBy,
                    SortDirection = descending
                }
            };
        }
    }
}


using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class RecreateSummariesLogController : Controller
    {
        private readonly IRecreateSummariesLogService _logService;

        public RecreateSummariesLogController(
            IRecreateSummariesLogService logService)
        {
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _logService.GetAllLogsAsync();
            var viewModel = new RecreateSummariesLogViewModel
            {
                LogsGrid = BuildLogsGrid(response.Data?.ToList() ?? new List<RecreateSummariesLogDto>())
            };
            return View(viewModel);
        }

        private DataGridConfig<RecreateSummariesLogItem> BuildLogsGrid(List<RecreateSummariesLogDto> logs)
        {
            var items = logs.Select(log => new RecreateSummariesLogItem
            {
                Id = log.Id,
                DateDone = log.DateDone?.ToString("yyyy-MM-dd HH:mm:ss"),
                UserId = log.UserId,
                User = log.UserName ?? string.Empty,
                Period = log.Period
            }).ToList();

            var pagination = new PaginationModel
            {
                PageNumber = 1,
                PageSize = 20,
                TotalRecords = items.Count
            };

            return new DataGridConfig<RecreateSummariesLogItem>
            {
                GridId = "releaseLogsGrid",
                Title = string.Empty,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<RecreateSummariesLogItem>(),
                Pagination = pagination,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowExport = false
            };
        }
    }
}

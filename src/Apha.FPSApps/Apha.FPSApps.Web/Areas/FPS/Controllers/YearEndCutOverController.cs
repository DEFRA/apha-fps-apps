using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class YearEndCutOverController : Controller
    {
        private const string YearEndCutOverJobName = "YearEnd-CutOver";

        private readonly IMapper _mapper;
        private readonly IYearMasterService _yearMasterService;
        private readonly IYearEndService _yearEndService;
        private readonly ILogger<YearEndCutOverController> _logger;

        public YearEndCutOverController(
            IMapper mapper,
            IYearMasterService yearMasterService,
            IYearEndService yearEndService,
            ILogger<YearEndCutOverController> logger)
        {
            _mapper = mapper;
            _yearMasterService = yearMasterService;
            _yearEndService = yearEndService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var plannedYear = await GetPlannedYearAsync();
            var canInitiate = await CanInitiateCutOverRequestAsync();
            var canApprove = await CanApproveOrRejectCutOverRequestAsync();
            var currentJobExecutionId = await GetInitiatedCutOverJobExecutionIdAsync();
            var historyGrid = await BuildHistoryGridAsync(new PaginationFilter<string> { Filter = "{}" });

            return View(new YearEndCutOverViewModel
            {
                PlannedYear = plannedYear,
                CanInitiate = canInitiate,
                CanApprove = canApprove,
                CurrentJobExecutionId = currentJobExecutionId,
                HistoryGrid = historyGrid
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadHistoryGrid(PaginationFilter<string> request)
        {
            var grid = await BuildHistoryGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> TriggerInitiate(int plannedYear)
        {
            var result = await _yearEndService.EnqueueYearEndCutOverInitiationJobAsync(plannedYear);
            if (result.Success)
            {
                _logger.LogInformation("Year End CutOver initiation job enqueued for year {PlannedYear}.", plannedYear);
                return Json(new { success = true, jobExecutionId = result.Data?.JobExecutionId });
            }

            var errors = result?.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? [new { field = string.Empty, message = "Failed to enqueue Year End CutOver initiation job." }];
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> TriggerApprove(int plannedYear, Guid? jobExecutionId)
        {
            if (!jobExecutionId.HasValue || jobExecutionId.Value == Guid.Empty)
                return Json(new { success = false, errors = new[] { new { field = "", message = "No active Year End CutOver request." } } });

            var result = await _yearEndService.TriggerYearEndCutOverApprovalJobAsync(plannedYear, jobExecutionId.Value);
            if (result.Success)
            {
                var eventId = result?.Data?.EventId;
                _logger.LogInformation("Year End CutOver approval job triggered. EventId: {EventId}", eventId);
                return Json(new { success = true });
            }

            var errors = result?.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? new[] { new { field = string.Empty, message = "Failed to trigger Year End CutOver approval job." } };
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> TriggerReject(int plannedYear, Guid? jobExecutionId)
        {
            if (!jobExecutionId.HasValue || jobExecutionId.Value == Guid.Empty)
                return Json(new { success = false, errors = new[] { new { field = "", message = "No active Year End CutOver request." } } });

            var result = await _yearEndService.EnqueueYearEndCutOverRejectJobAsync(plannedYear, jobExecutionId.Value);
            if (result.Success)
            {
                _logger.LogInformation("Year End CutOver reject job enqueued for year {PlannedYear}.", plannedYear);
                return Json(new { success = true });
            }

            var errors = result?.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? new[] { new { field = string.Empty, message = "Failed to enqueue Year End CutOver reject job." } };
            return Json(new { success = false, errors });
        }
        private async Task<int> GetPlannedYearAsync()
        {
            var result = await _yearMasterService.GetFpsPlannedYearAsync();
            return result.Success ? result.Data : 0;
        }

        private async Task<bool> CanInitiateCutOverRequestAsync()
        {
            var result = await _yearEndService.CanInitiateCutOverRequestAsync(YearEndCutOverJobName);
            return result.Success && result.Data;
        }

        private async Task<bool> CanApproveOrRejectCutOverRequestAsync()
        {
            var result = await _yearEndService.CanApproveOrRejectCutOverRequestAsync(YearEndCutOverJobName);
            return result.Success && result.Data;
        }

        private async Task<Guid?> GetInitiatedCutOverJobExecutionIdAsync()
        {
            var result = await _yearEndService.GetInitiatedCutOverJobExecutionIdAsync();
            return result.Success ? result.Data : null;
        }

        private async Task<DataGridConfig<YearEndHistoryItem>> BuildHistoryGridAsync(PaginationFilter<string> request)
        {
            var grid = HistoryGridConfig();
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _yearEndService.GetYearEndCutOverBatchJobHistoryAsync(query, YearEndCutOverJobName);

            grid.Data = response.Data?.data != null
                ? response.Data.data.Select(d => new YearEndHistoryItem
                {
                    JobName = d.JobName,
                    RequestedBy = d.RequestedBy,
                    StartDateTime = d.StartDateTime,
                    EndDateTime = d.EndDateTime,
                    Status = d.Status,
                    ErrorMessage = d.ErrorMessage
                }).ToList()
                : [];

            grid.Pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();

            grid.Pagination.SortColumn = request.SortBy;
            grid.Pagination.SortDirection = request.Descending;

            return grid;
        }

        private static DataGridConfig<YearEndHistoryItem> HistoryGridConfig() => new()
        {
            GridId = "yearEndCutOverHistoryGrid",
            Title = "Request History",
            BindGridUrl = "/FPS/YearEndCutOver/LoadHistoryGrid",
            ShowCheckboxColumn = false,
            AllowAdd = false,
            AllowEdit = false,
            AllowDelete = false,
            Columns = GridDataProvider.GetColumnsDefination<YearEndHistoryItem>()
        };
    }
}

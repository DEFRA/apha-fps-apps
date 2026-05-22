using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "FPSAdmin,FPSUser,PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class WorkGroupReportController : Controller
    {
        private readonly IWorkGroupReportEmailService _emailSendService;
        private readonly IWorkGroupService _workGroupService;
        private readonly ICalenderMonthService _calenderMonthService;
        private readonly IProfitCentreService _profitCentreService;

        public WorkGroupReportController(
            IWorkGroupReportEmailService emailSendService,
            IWorkGroupService workGroupService,
            ICalenderMonthService calenderMonthService,
            IProfitCentreService profitCentreService)
        {
            _emailSendService = emailSendService;
            _workGroupService = workGroupService;
            _calenderMonthService = calenderMonthService;
            _profitCentreService = profitCentreService;
        }
        
        /// <summary>
        /// Renders the WorkGroup Report page, pre-loading the first available profit centre's
        /// work groups, period selector months, and profit centre settings.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var profitCentreOptions = await GetProfitCentreSelectListAsync();
            var firstPc = profitCentreOptions.FirstOrDefault()?.Value;

            var calenderMonths = await GetCalenderMonthsAsync();

            var vm = new WorkGroupReportEmailViewModel
            {
                CalenderMonthItems   = calenderMonths,
                ProfitCentreOptions  = profitCentreOptions,
                SelectedProfitCentre = firstPc
            };

            if (!string.IsNullOrWhiteSpace(firstPc))
            {
                var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
                vm.WorkGroupGrid = await GetWorkGroupGridConfigAsync(defaultRequest, firstPc);
                await ApplyProfitCentreSettingsAsync(vm);
            }
            else
            {
                vm.WorkGroupGrid = BuildEmptyWorkGroupGrid();
            }

            return View(vm);
        }
       
        /// <summary>
        /// AJAX endpoint called by the DataGrid client to reload the work-group grid with the
        /// supplied pagination, sort, column-filter, and profit-centre parameters.
        /// Returns a <c>_DataGrid</c> partial view.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadWorkGroupGrid(
            PaginationFilter<string> request, string? profitCentre)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(profitCentre))
                return PartialView("_DataGrid", BuildEmptyWorkGroupGrid());

            var grid = await GetWorkGroupGridConfigAsync(request, profitCentre);
            return PartialView("_DataGrid", grid);
        }

        /// <summary>
        /// Flags all work groups belonging to <paramref name="profitCentre"/> for email sending
        /// by setting their <c>SendEmail</c> flag to <c>1</c>.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SelectPCWorkGroups(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                return BadRequest(new { error = "Profit Centre is required." });

            var result = await _workGroupService.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, 1);
            return result.Success
                ? Ok(new { success = true, message = $"All work groups for '{profitCentre}' flagged for email." })
                : StatusCode(500, new { error = "Failed to flag work groups for email." });
        }

        /// <summary>
        /// Clears the <c>SendEmail</c> flag (sets to <c>0</c>) for all work groups belonging to
        /// <paramref name="profitCentre"/>.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ClearPCWorkGroups(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                return BadRequest(new { error = "Profit Centre is required." });

            var result = await _workGroupService.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, 0);
            return result.Success
                ? Ok(new { success = true, message = $"Email flags cleared for all work groups in '{profitCentre}'." })
                : StatusCode(500, new { error = "Failed to clear email flags." });
        }

        /// <summary>
        /// Clears the <c>SendEmail</c> flag (sets to <c>0</c>) for every work group across all
        /// profit centres.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ClearAllWorkGroups()
        {
            var result = await _workGroupService.SetSendEmailForAllWorkGroupsAsync(0);
            return result.Success
                ? Ok(new { success = true })
                : StatusCode(500, new { error = "Failed to clear email flags for all work groups." });
        }

        /// <summary>
        /// Triggers the email-send process for the given <paramref name="profitCentre"/> and
        /// calendar <paramref name="monthNumber"/>. Returns a JSON array of per-work-group send
        /// results (work group name, recipient, status, and failure reason if applicable).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string profitCentre, short monthNumber)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _emailSendService.SendEmailsAsync(profitCentre, monthNumber);

            if (!response.Success)
                return StatusCode(500, new { error = "An error occurred while sending emails. Please try again." });

            var results = (response.Data ?? new())
                .Select(r => new
                {
                    r.WorkGroupName,
                    r.EmailRecipient,
                    r.Status,
                    r.Reason
                })
                .ToList();

            return Ok(new { success = true, results });
        }

        /// <summary>
        /// Returns the <c>_WorkGroupEditModal</c> partial pre-populated with the current
        /// send-email flag and email recipient for the specified work group.
        /// </summary>
        [HttpGet]
        public IActionResult GetWorkGroupEdit(
            string workGroupName, bool flaggedForEmail, string? emailRecipient)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return PartialView("_WorkGroupEditModal", new WorkGroupEmailItem
            {
                WorkGroupName  = workGroupName,
                FlaggedForEmail = flaggedForEmail,
                EmailRecipient = emailRecipient
            });
        }

        /// <summary>
        /// Updates the <c>SendEmail</c> flag and <c>EmailRecipient</c> for a single work group.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateWorkGroupEmail(
            string workGroupName, short sendEmail, string? emailRecipient)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(workGroupName))
                return BadRequest(new { error = "WorkGroupName is required." });

            var result = await _workGroupService.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient);

            return result.Success
                ? Ok(new { success = true })
                : StatusCode(500, new { error = "Failed to update work group." });
        }

        /// <summary>
        /// Fetches a paged, filtered, and sorted list of work groups for <paramref name="profitCentre"/>
        /// then assembles and returns the complete <see cref="DataGridConfig{T}"/> ready for the
        /// <c>_DataGrid</c> partial. Pagination and sort state are read directly from
        /// <paramref name="request"/> and the API response, avoiding a separate tuple return.
        /// </summary>
        private async Task<DataGridConfig<WorkGroupEmailItem>> GetWorkGroupGridConfigAsync(
            PaginationFilter<string> request, string profitCentre)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var query = new QueryParameters<string>
            {
                Page       = request.Page,
                PageSize   = request.PageSize,
                SortBy     = request.SortBy,
                Descending = request.Descending,
                Filter     = request.Filter
            };

            var response = await _workGroupService.GetWorkGroupsByProfitCentreAsync(query, profitCentre);

            var items = response.Success && response.Data != null
                ? response.Data
                    .Select(w => new WorkGroupEmailItem
                    {
                        WorkGroupName   = w.WorkGroupName,
                        EmailRecipient  = w.EmailRecipient,
                        FlaggedForEmail = w.SendEmail == 1
                    })
                    .ToList()
                : new List<WorkGroupEmailItem>();

            var pagination = new PaginationModel
            {
                TotalRecords  = response.Pagination?.TotalRecords ?? 0,
                PageNumber    = request.Page,
                PageSize      = request.PageSize,
                SortColumn    = request.SortBy    ?? nameof(WorkGroupEmailItem.WorkGroupName),
                SortDirection = request.Descending
            };

            return BuildWorkGroupGrid(items, pagination, filterDict);
        }

        /// <summary>
        /// Returns an empty <see cref="DataGridConfig{T}"/> with default pagination, used when
        /// no profit centre is selected.
        /// </summary>
        private static DataGridConfig<WorkGroupEmailItem> BuildEmptyWorkGroupGrid() =>
            BuildWorkGroupGrid(new List<WorkGroupEmailItem>(), new PaginationModel(), new Dictionary<string, string>());

        private static DataGridConfig<WorkGroupEmailItem> BuildWorkGroupGrid(
            List<WorkGroupEmailItem> items,
            PaginationModel pagination,
            Dictionary<string, string> filterDict)
        {

            return new DataGridConfig<WorkGroupEmailItem>
            {
                GridId = "workGroupGrid",
                Title = "Workgroups and email recipients",
                KeyProperty = nameof(WorkGroupEmailItem.WorkGroupName),
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = false,
                AllowCopy = false,
                AllowExport = false,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                EditFunction = "openWorkGroupEditModal",
                ExtraFilterMethod = "getWorkGroupGridExtraFilters",
                BindGridUrl = "/PACT/WorkGroupReport/LoadWorkGroupGrid",
                Data = items,
                Pagination = pagination,
                CurrentFilters = filterDict,
                Columns = new List<DataGridColumn>
                {
                    new() { PropertyName = nameof(WorkGroupEmailItem.WorkGroupName), DisplayName = "Workgroup",       ColumnType = GridColumnType.Text,     IsFilterable = true,  Width = 200 },
                    new() { PropertyName = nameof(WorkGroupEmailItem.SendEmailYes),  DisplayName = "SendEmail? Yes",  ColumnType = GridColumnType.Checkbox, IsFilterable = false, Width = 80  },
                    new() { PropertyName = nameof(WorkGroupEmailItem.SendEmailNo),   DisplayName = "SendEmail? No",   ColumnType = GridColumnType.Checkbox, IsFilterable = false, Width = 60  },
                    new() { PropertyName = nameof(WorkGroupEmailItem.EmailRecipient),DisplayName = "Email Recipient", ColumnType = GridColumnType.Text,     IsFilterable = true,  Width = 250 }
                }
            };
        }

        /// <summary>
        /// Retrieves all calendar months from the CalenderMonth service for use in the period
        /// selector dropdown.
        /// </summary>
        private async Task<List<CalenderMonthDto>> GetCalenderMonthsAsync()
        {
            var response = await _calenderMonthService.GetCalenderMonthsAsync();
            return response.Success && response.Data != null
                ? response.Data
                : new List<CalenderMonthDto>();
        }

        /// <summary>
        /// Builds the profit-centre <see cref="SelectListItem"/> collection for the page
        /// dropdown from the ProfitCentre service.
        /// </summary>
        private async Task<List<SelectListItem>> GetProfitCentreSelectListAsync()
        {
            var response = await _profitCentreService.GetAllProfitCentresAsync();
            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .Where(pc => !string.IsNullOrWhiteSpace(pc.ProfitCentreId))
                .Select(pc => new SelectListItem(
                    pc.ProfitCentreId,
                    pc.ProfitCentreId))
                .ToList();
        }

        /// <summary>
        /// Returns the timesheet, output-sheet, and timesheet-layout settings for the given
        /// <paramref name="profitCentre"/> as JSON, used by the page to pre-populate the
        /// settings checkboxes without a full page reload.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfitCentreSettings(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                return Json(new { timesheet = false, outputsheet = false, timesheetLayout = 1 });

            var response = await _profitCentreService.GetProfitCentreByIdAsync(profitCentre);
            if (!response.Success || response.Data == null)
                return Json(new { timesheet = false, outputsheet = false, timesheetLayout = 1 });

            var d = response.Data;
            return Json(new
            {
                timesheet       = d.Timesheet == -1,
                outputsheet     = d.Outputsheet == -1,
                timesheetLayout = d.TimesheetLayout ?? 1
            });
        }

        /// <summary>
        /// Persists the timesheet, output-sheet, and layout preferences for
        /// <paramref name="profitCentre"/>. Boolean flags are converted to the Access-style
        /// numeric semantics: <c>-1</c> = true, <c>0</c> = false; layout <c>1</c> = Flat-file,
        /// <c>2</c> = Cross-tab.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PatchProfitCentreSettings(
            string profitCentre, bool sendTimeSheet, bool sendOutputSheet, bool timesheetLayoutFlat)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(profitCentre))
                return BadRequest(new { error = "ProfitCentre is required." });

            // Convert booleans back to Access semantics: -1 = true, 0 = false
            var timesheet       = sendTimeSheet   ? -1 : 0;
            var outputsheet     = sendOutputSheet ? -1 : 0;
            // timesheetLayoutFlat true → 1 (Flat-file), false → 2 (Cross-tab)
            var timesheetLayout = (short)(timesheetLayoutFlat ? 1 : 2);

            var result = await _profitCentreService.UpdateProfitCentreSettingsAsync(
                profitCentre, timesheet, outputsheet, timesheetLayout);

            return result.Success
                ? Ok(new { success = true })
                : StatusCode(500, new { error = "Failed to update profit centre settings." });
        }

        /// <summary>
        /// Reads profit-centre settings from the service and applies them to the view model so
        /// the timesheet, output-sheet, and layout checkboxes reflect the persisted values on
        /// initial page load.
        /// </summary>
        private async Task ApplyProfitCentreSettingsAsync(WorkGroupReportEmailViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SelectedProfitCentre))
                return;

            var settings = await _profitCentreService.GetProfitCentreByIdAsync(model.SelectedProfitCentre);
            if (!settings.Success || settings.Data == null)
                return;

            var d = settings.Data;
            model.SendTimeSheet = d.Timesheet == -1;
            model.SendOutputSheet = d.Outputsheet == -1;

            // TimesheetLayout: 1 = Flat-file, 2 = Cross-tab (matches Access OptionValue)
            model.TimesheetLayoutFlat = d.TimesheetLayout == 1 || d.TimesheetLayout == null;
            model.TimesheetLayoutCrossTab = d.TimesheetLayout == 2;
        }
    }
}

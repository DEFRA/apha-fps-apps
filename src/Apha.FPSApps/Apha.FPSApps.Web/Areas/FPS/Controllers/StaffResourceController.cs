using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class StaffResourceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IStaffJobService _staffJobService;
        private readonly IAppStateService _appStateService;

        public StaffResourceController(
            IMapper mapper,
            IProfitCentreService profitCentreService,
            IWorkGroupService workGroupService,
            IStaffJobService staffJobService,
            IAppStateService appStateService)
        {
            _mapper = mapper;
            _profitCentreService = profitCentreService;
            _workGroupService = workGroupService;
            _staffJobService = staffJobService;
            _appStateService = appStateService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? profitCentre = null)
        {
            var viewModel = new StaffResourceViewModel();

            await PopulateProfitCentresAsync(viewModel);

            // Fall back to session only when no param was supplied (arriving from another screen).
            // An explicitly supplied empty value clears the session.
            if (profitCentre == null)
                profitCentre = await _appStateService.GetSessionAsync<string>(SessionKeys.SelectedProfitCentre);

            var selected = !string.IsNullOrWhiteSpace(profitCentre)
                && viewModel.ProfitCentreList.Any(p => p.Value == profitCentre)
                ? profitCentre
                : string.Empty;

            await _appStateService.SetSessionAsync(SessionKeys.SelectedProfitCentre, selected);

            viewModel.SelectedProfitCentre = selected;
            foreach (var item in viewModel.ProfitCentreList)
                item.Selected = string.Equals(item.Value, selected, StringComparison.OrdinalIgnoreCase);

            viewModel.WorkgroupGrid = await GetWorkgroupGridConfigAsync(new QueryParameters<string>(), null, selected);
            viewModel.StaffGrid = GetStaffGridConfig();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadWorkgroupGrid(
            PaginationFilter<string> request, string? profitCentre = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var gridConfig = await GetWorkgroupGridConfigAsync(queryParameters, filterDict, profitCentre);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadStaffGrid(PaginationFilter<string> request, string? workgroup = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var gridConfig = await GetStaffGridConfigAsync(queryParameters, filterDict, workgroup);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> LoadStaffTotals(string? workgroup = null)
        {
            var totals = await GetStaffTotalsAsync(workgroup);
            return Json(totals);
        }

        private async Task<StaffResourceTotals> GetStaffTotalsAsync(string? workgroup)
        {
            var totals = new StaffResourceTotals();

            if (string.IsNullOrWhiteSpace(workgroup))
                return totals;

            // Totals must always reflect the complete underlying dataset for the
            // workgroup, regardless of pagination, filtering, sorting, or the
            // number of rows currently displayed. Request all rows with no filter.
            var fullQuery = new QueryParameters<string>
            {
                Page = 1,
                PageSize = int.MaxValue,
                Filter = null,
                SortBy = null,
                Descending = false
            };

            var response = await _staffJobService.GetStaffResourceUtilisationAsync(fullQuery, workgroup);
            if (!response.Success || response.Data == null || response.Data.Count == 0)
                return totals;

            var data = response.Data;

            totals.TotalH = Round2(data.Sum(d => d.HrsAvail));
            totals.Ztw = Round2(data.Sum(d => d.PlannedZt));
            totals.Avail = Round2(data.Sum(d => d.AvailSoct));
            totals.Left = Round2(data.Sum(d => d.Left));
            totals.ApprovedPlan = Round2(data.Sum(d => d.ApprovedSoct));
            totals.NotApprovedPlan = Round2(data.Sum(d => d.NotApprovedSoct));
            totals.TotalPlan = Round2(data.Sum(d => d.ApprovedSoct + d.NotApprovedSoct));

            totals.ApprovedUtil = AverageOrNull(data.Where(d => d.ApprovedUtilPct.HasValue).Select(d => d.ApprovedUtilPct!.Value));
            totals.NotApprovedUtil = AverageOrNull(data.Where(d => d.NotApprovedUtilPct.HasValue).Select(d => d.NotApprovedUtilPct!.Value));
            totals.TotalUtil = AverageOrNull(data.Where(d => d.TotalUtilPct.HasValue).Select(d => d.TotalUtilPct!.Value));

            return totals;
        }

        private static double Round2(double value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        private static double? AverageOrNull(IEnumerable<double> values)
        {
            var list = values.ToList();
            return list.Count == 0 ? null : Math.Round(list.Average(), 2, MidpointRounding.AwayFromZero);
        }

        private async Task<DataGridConfig<StaffResourceWorkgroupItem>> GetWorkgroupGridConfigAsync(
            QueryParameters<string> queryParameters, Dictionary<string, string>? filterDict, string? profitCentre)
        {
            var items = new List<StaffResourceWorkgroupItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                var response = await _workGroupService.GetWorkGroupsByProfitCentreAsync(queryParameters, profitCentre);
                if (response.Success && response.Data != null)
                {
                    items = response.Data.Select(d => new StaffResourceWorkgroupItem
                    {
                        WorkGroupName = d.WorkGroupName
                    }).ToList();

                    paginationModel = response.Pagination == null
                        ? new PaginationModel()
                        : _mapper.Map<PaginationModel>(response.Pagination);
                }
            }

            paginationModel.SortColumn = queryParameters.SortBy;
            paginationModel.SortDirection = queryParameters.Descending;

            return new DataGridConfig<StaffResourceWorkgroupItem>
            {
                GridId = "ruvWorkgroupGrid",
                Title = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination = false,
                KeyProperty = "WorkGroupName",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                ExtraFilterMethod = "getRuvWorkgroupExtraFilters",
                BindGridUrl = Url.Action(nameof(LoadWorkgroupGrid), "StaffResource", new { area = "FPS" })!,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<StaffResourceWorkgroupItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private DataGridConfig<StaffResourceStaffItem> GetStaffGridConfig()
        {
            return new DataGridConfig<StaffResourceStaffItem>
            {
                GridId = "ruvStaffGrid",
                Title = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffName",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ExtraFilterMethod = "getRuvStaffExtraFilters",
                BindGridUrl = Url.Action(nameof(LoadStaffGrid), "StaffResource", new { area = "FPS" })!,
                Data = new List<StaffResourceStaffItem>(),
                Columns = GridDataProvider.GetColumnsDefination<StaffResourceStaffItem>(null),
                Pagination = new PaginationModel()
            };
        }

        private async Task<DataGridConfig<StaffResourceStaffItem>> GetStaffGridConfigAsync(
            QueryParameters<string> queryParameters, Dictionary<string, string>? filterDict, string? workgroup)
        {
            var items = new List<StaffResourceStaffItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(workgroup))
            {
                var response = await _staffJobService.GetStaffResourceUtilisationAsync(queryParameters, workgroup);
                if (response.Success && response.Data != null)
                {
                    items = response.Data.Select(d => new StaffResourceStaffItem
                    {
                        WgGrade = d.WgGrade,
                        Name = d.Name,
                        TotalH = d.HrsAvail,
                        Ztw = d.PlannedZt,
                        Avail = d.AvailSoct,
                        Left = d.Left,
                        ApprovedPlan = d.ApprovedSoct,
                        ApprovedUtil = d.ApprovedUtilPct,
                        NotApprovedPlan = d.NotApprovedSoct,
                        NotApprovedUtil = d.NotApprovedUtilPct,
                        TotalPlan = d.ApprovedSoct + d.NotApprovedSoct,
                        TotalUtil = d.TotalUtilPct
                    }).ToList();

                    paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
                }
            }

            paginationModel.SortColumn = queryParameters.SortBy;
            paginationModel.SortDirection = queryParameters.Descending;

            return new DataGridConfig<StaffResourceStaffItem>
            {
                GridId = "ruvStaffGrid",
                Title = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffName",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ExtraFilterMethod = "getRuvStaffExtraFilters",
                BindGridUrl = Url.Action(nameof(LoadStaffGrid), "StaffResource", new { area = "FPS" })!,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<StaffResourceStaffItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private async Task PopulateProfitCentresAsync(StaffResourceViewModel model)
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            model.ProfitCentreList = result.Data == null
                ? new List<SelectListItem>()
                : result.Data.Select(p => new SelectListItem
                {
                    Value = p.ProfitCentreId,
                    Text = p.ProfitCentreId,
                    Selected = string.Equals(model.SelectedProfitCentre, p.ProfitCentreId, StringComparison.OrdinalIgnoreCase)
                }).ToList();
        }
    }
}

using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Interfaces.FPS;
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
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class StaffPlanDetailsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectStaffPlanDetailsService _staffPlanDetailsService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IAppStateService _appStateService;

        public StaffPlanDetailsController(
            IMapper mapper,
            IProjectStaffPlanDetailsService staffPlanDetailsService,
            IProfitCentreService profitCentreService,
            IAppStateService appStateService)
        {
            _mapper = mapper;
            _staffPlanDetailsService = staffPlanDetailsService;
            _profitCentreService = profitCentreService;
            _appStateService = appStateService;
        }

        public async Task<IActionResult> Index(string? profitCentre = null)
        {
            var profitCentreOptions = await GetProfitCentreSelectListAsync();

            if (profitCentre == null)
                profitCentre = await _appStateService.GetSessionAsync<string>(SessionKeys.SelectedProfitCentre);

            var selected = !string.IsNullOrWhiteSpace(profitCentre)
                && profitCentreOptions.Any(p => p.Value == profitCentre) ? profitCentre : null;

            await _appStateService.SetSessionAsync(SessionKeys.SelectedProfitCentre, selected ?? string.Empty);

            var grid = await BuildGridAsync(new PaginationFilter<string>(), selected);

            return View(new StaffPlanDetailsViewModel
            {
                Grid = grid,
                ProfitCentreOptions = profitCentreOptions,
                SelectedProfitCentre = selected
            });
        }

        /// <summary>
        /// Reloads the staff plan details grid partial view for the supplied pagination, sort,
        /// filter, and profit-centre parameters.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request, string? profitCentre)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _appStateService.SetSessionAsync(SessionKeys.SelectedProfitCentre, profitCentre ?? string.Empty);

            var grid = await BuildGridAsync(request, profitCentre);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<StaffPlanDetailsViewItem>> BuildGridAsync(PaginationFilter<string> request, string? profitCentre)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var rows = new List<StaffPlanDetailsViewItem>();
            PaginationModel pagination = new();

            // Only query when a profit centre has been selected; otherwise show an empty grid.
            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                filterDict["ProfitCentre"] = profitCentre;
                request.Filter = JsonConvert.SerializeObject(filterDict);

                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _staffPlanDetailsService.GetPagedAsync(query);

                if (response.Success && response.Data != null)
                {
                    rows = _mapper.Map<List<StaffPlanDetailsViewItem>>(response.Data);

                    if (response.Pagination != null)
                    {
                        pagination.PageNumber   = response.Pagination.PageNumber;
                        pagination.PageSize     = response.Pagination.PageSize;
                        pagination.TotalRecords = response.Pagination.TotalRecords;
                    }
                }
            }

            pagination.SortColumn    = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<StaffPlanDetailsViewItem>
            {
                GridId            = "staffPlanDetailsGrid",
                KeyProperty       = "Program",
                AllowAdd          = false,
                AllowEdit         = false,
                AllowDelete       = false,
                ShowPagination    = true,
                ExtraFilterMethod = "getStaffPlanDetailsExtraFilters",
                BindGridUrl       = "/FPS/StaffPlanDetails/LoadGrid",
                Columns           = GridDataProvider.GetColumnsDefination<StaffPlanDetailsViewItem>(),
                Data              = rows,
                CurrentFilters    = filterDict,
                Pagination        = pagination
            };
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

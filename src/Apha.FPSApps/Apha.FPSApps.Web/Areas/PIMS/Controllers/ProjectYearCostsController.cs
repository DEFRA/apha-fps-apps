using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class ProjectYearCostsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectYearCostsService _yearCostsService;
        private readonly IProjectListService _projectListService;
        private readonly IProjectDetailsService _projectDetailsService;

        public ProjectYearCostsController(
            IMapper mapper,
            IProjectYearCostsService yearCostsService,
            IProjectListService projectListService,
            IProjectDetailsService projectDetailsService)
        {
            _mapper = mapper;
            _yearCostsService = yearCostsService;
            _projectListService = projectListService;
            _projectDetailsService = projectDetailsService;
        }

        public async Task<IActionResult> Index(string? parentproject, short? year)
        {
            Task<ApiResponseDto<List<ProjectListViewDto>>> projectsTask = _projectListService.GetAllProjectsListAsync();
            Task<ApiResponseDto<List<YearDto>>> yearsTask = _projectDetailsService.GetAllYearAsync();
            await Task.WhenAll(projectsTask, yearsTask);

            List<SelectListItem> projectOptions = (projectsTask.Result.Data ?? [])
                .Select(p => new SelectListItem(p.Parentproject, p.Parentproject))
                .ToList();

            List<SelectListItem> yearOptions = (yearsTask.Result.Data ?? [])
                .OrderBy(y => y.Value)
                .Select(y => new SelectListItem(y.Value.ToString(), y.Value.ToString()))
                .ToList();

            short resolvedYear = year ?? (short)(yearsTask.Result.Data?.Max(y => y.Value) ?? DateTime.Now.Year);
            string resolvedProject = parentproject ?? projectOptions.FirstOrDefault()?.Value ?? string.Empty;

            PaginationFilter<string> defaultRequest = new() { Filter = "{}" };

            Task<DataGridConfig<AdditionalCostPlanItem>> plansGridTask =
                BuildAdditionalPlansGridAsync(resolvedProject, resolvedYear, defaultRequest);
            Task<DataGridConfig<AdditionalCostActualItem>> actualsGridTask =
                BuildAdditionalActualsGridAsync(resolvedProject, resolvedYear, defaultRequest);

            await Task.WhenAll(plansGridTask, actualsGridTask);

            return View(new ProjectYearCostsViewModel
            {
                Parentproject = resolvedProject,
                SelectedYear = resolvedYear,
                ProjectOptions = projectOptions,
                YearOptions = yearOptions,
                AdditionalPlansGrid = plansGridTask.Result,
                AdditionalActualsGrid = actualsGridTask.Result
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadAdditionalPlansGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<AdditionalCostPlanItem> grid =
                await BuildAdditionalPlansGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAdditionalActualsGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<AdditionalCostActualItem> grid =
                await BuildAdditionalActualsGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        // ── grid builders ────────────────────────────────────────────────

        private async Task<DataGridConfig<AdditionalCostPlanItem>> BuildAdditionalPlansGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<AdditionalCostDto>> response =
                await _yearCostsService.GetAdditionalPlansAsync(project, year, queryParameters);

            List<AdditionalCostPlanItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<AdditionalCostPlanItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<AdditionalCostPlanItem>
            {
                GridId = "additionalPlansGrid",
                Title = "Additional Cost",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "AcctCode",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadAdditionalPlansGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AdditionalCostPlanItem>(null),
                Pagination = pagination
            };
        }

        private async Task<DataGridConfig<AdditionalCostActualItem>> BuildAdditionalActualsGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<AdditionalCostDto>> response =
                await _yearCostsService.GetAdditionalActualsAsync(project, year, queryParameters);

            List<AdditionalCostActualItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<AdditionalCostActualItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<AdditionalCostActualItem>
            {
                GridId = "additionalActualsGrid",
                Title = "Additional Actuals",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "AcctCode",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadAdditionalActualsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AdditionalCostActualItem>(null),
                Pagination = pagination
            };
        }
    }
}

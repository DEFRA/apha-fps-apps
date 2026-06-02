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
                Task<DataGridConfig<AnimalCostPlanItem>> animalPlansGridTask =
                    BuildAnimalPlansGridAsync(resolvedProject, resolvedYear, defaultRequest);
                Task<DataGridConfig<AnimalCostActualItem>> animalActualsGridTask =
                    BuildAnimalActualsGridAsync(resolvedProject, resolvedYear, defaultRequest);
                Task<DataGridConfig<TestCostPlanItem>> testPlansGridTask =
                    BuildTestPlansGridAsync(resolvedProject, resolvedYear, defaultRequest);
                Task<DataGridConfig<TestCostActualItem>> testActualsGridTask =
                    BuildTestActualsGridAsync(resolvedProject, resolvedYear, defaultRequest);
                Task<DataGridConfig<StaffCostPlanItem>> staffPlansGridTask =
                    BuildStaffPlansGridAsync(resolvedProject, resolvedYear, defaultRequest);
                Task<DataGridConfig<StaffCostActualItem>> staffActualsGridTask =
                    BuildStaffActualsGridAsync(resolvedProject, resolvedYear, defaultRequest);

                // Plan tab grids — served with empty data; lazy-loaded on first tab click
                DataGridConfig<StaffCostPlanItem> planStaffGrid =
                    BuildEmptyGrid<StaffCostPlanItem>("planStaffGrid", "Staff Plan", "WgGrade", "/PIMS/ProjectYearCosts/LoadPlanStaffGrid");
                DataGridConfig<TestCostPlanItem> planTestGrid =
                    BuildEmptyGrid<TestCostPlanItem>("planTestGrid", "Test Plan", "TestCode", "/PIMS/ProjectYearCosts/LoadPlanTestGrid");
                DataGridConfig<AnimalCostPlanItem> planAnimalGrid =
                    BuildEmptyGrid<AnimalCostPlanItem>("planAnimalGrid", "Animal Plan", "AnimalType", "/PIMS/ProjectYearCosts/LoadPlanAnimalGrid");
                DataGridConfig<AdditionalCostPlanItem> planAdditionalGrid =
                    BuildEmptyGrid<AdditionalCostPlanItem>("planAdditionalGrid", "Additional Cost Plan", "Account", "/PIMS/ProjectYearCosts/LoadPlanAdditionalGrid");

                // Pact Pay tab grid — lazy-loaded on first tab click
                DataGridConfig<PactPayItem> pactPayGrid =
                    BuildEmptyGrid<PactPayItem>("pactPayGrid", "Pact Pay", "Month", "/PIMS/ProjectYearCosts/LoadPactPayGrid");

                await Task.WhenAll(plansGridTask, actualsGridTask, animalPlansGridTask, animalActualsGridTask,
                    testPlansGridTask, testActualsGridTask, staffPlansGridTask, staffActualsGridTask);

            return View(new ProjectYearCostsViewModel
            {
                Parentproject = resolvedProject,
                SelectedYear = resolvedYear,
                ProjectOptions = projectOptions,
                YearOptions = yearOptions,
                AdditionalPlansGrid = plansGridTask.Result,
                AdditionalActualsGrid = actualsGridTask.Result,
                AnimalPlansGrid = animalPlansGridTask.Result,
                AnimalActualsGrid = animalActualsGridTask.Result,
                TestPlansGrid = testPlansGridTask.Result,
                TestActualsGrid = testActualsGridTask.Result,
                StaffPlansGrid = staffPlansGridTask.Result,
                StaffActualsGrid = staffActualsGridTask.Result,
                PlanStaffGrid = planStaffGrid,
                PlanTestGrid = planTestGrid,
                PlanAnimalGrid = planAnimalGrid,
                PlanAdditionalGrid = planAdditionalGrid,
                PactPayGrid = pactPayGrid
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

        private static DataGridConfig<T> BuildEmptyGrid<T>(
            string gridId, string title, string keyProperty, string bindUrl) where T : class, new()
        {
            return new DataGridConfig<T>
            {
                GridId = gridId,
                Title = title,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = keyProperty,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = bindUrl,
                Data = [],
                Columns = GridDataProvider.GetColumnsDefination<T>(null),
                Pagination = new PaginationModel()
            };
        }

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

        [HttpPost]
        public async Task<IActionResult> LoadAnimalPlansGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<AnimalCostPlanItem> grid =
                await BuildAnimalPlansGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAnimalActualsGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<AnimalCostActualItem> grid =
                await BuildAnimalActualsGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<AnimalCostPlanItem>> BuildAnimalPlansGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<AnimalCostDto>> response =
                await _yearCostsService.GetAnimalPlansAsync(project, year, queryParameters);

            List<AnimalCostPlanItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<AnimalCostPlanItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<AnimalCostPlanItem>
            {
                GridId = "animalPlansGrid",
                Title = "Animal Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "AnimalType",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadAnimalPlansGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AnimalCostPlanItem>(null),
                Pagination = pagination
            };
        }

        private async Task<DataGridConfig<AnimalCostActualItem>> BuildAnimalActualsGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<AnimalCostDto>> response =
                await _yearCostsService.GetAnimalActualsAsync(project, year, queryParameters);

            List<AnimalCostActualItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<AnimalCostActualItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<AnimalCostActualItem>
            {
                GridId = "animalActualsGrid",
                Title = "Animal Actuals",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "AcctCode",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadAnimalActualsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AnimalCostActualItem>(null),
                Pagination = pagination
            };
        }

        [HttpPost]
        public async Task<IActionResult> LoadTestPlansGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<TestCostPlanItem> grid =
                await BuildTestPlansGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTestActualsGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<TestCostActualItem> grid =
                await BuildTestActualsGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<TestCostPlanItem>> BuildTestPlansGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<TestCostDto>> response =
                await _yearCostsService.GetTestPlansAsync(project, year, queryParameters);

            List<TestCostPlanItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<TestCostPlanItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<TestCostPlanItem>
            {
                GridId = "testPlansGrid",
                Title = "Test Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "TestCode",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadTestPlansGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestCostPlanItem>(null),
                Pagination = pagination
            };
        }

        private async Task<DataGridConfig<TestCostActualItem>> BuildTestActualsGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<TestCostDto>> response =
                await _yearCostsService.GetTestActualsAsync(project, year, queryParameters);

            List<TestCostActualItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<TestCostActualItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<TestCostActualItem>
            {
                GridId = "testActualsGrid",
                Title = "Test Actuals",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "TestCode",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadTestActualsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestCostActualItem>(null),
                Pagination = pagination
            };
        }

        [HttpPost]
        public async Task<IActionResult> LoadStaffPlansGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<StaffCostPlanItem> grid =
                await BuildStaffPlansGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadStaffActualsGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<StaffCostActualItem> grid =
                await BuildStaffActualsGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<StaffCostPlanItem>> BuildStaffPlansGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<StaffCostDto>> response =
                await _yearCostsService.GetStaffPlansAsync(project, year, queryParameters);

            List<StaffCostPlanItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<StaffCostPlanItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<StaffCostPlanItem>
            {
                GridId = "staffPlansGrid",
                Title = "Staff Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "WgGrade",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadStaffPlansGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<StaffCostPlanItem>(null),
                Pagination = pagination
            };
        }

        private async Task<DataGridConfig<StaffCostActualItem>> BuildStaffActualsGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<StaffCostDto>> response =
                await _yearCostsService.GetStaffActualsAsync(project, year, queryParameters);

            List<StaffCostActualItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<StaffCostActualItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<StaffCostActualItem>
            {
                GridId = "staffActualsGrid",
                Title = "Staff Actuals",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "JobCode",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadStaffActualsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<StaffCostActualItem>(null),
                Pagination = pagination
            };
        }

        // ── Plan tab ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetPlanTotals(string project, short year)
        {
            QueryParameters<string> allRecords = new() { Page = 1, PageSize = int.MaxValue };

            Task<ApiResponseDto<List<StaffCostDto>>> staffTask =
                _yearCostsService.GetStaffPlansAsync(project, year, allRecords);
            Task<ApiResponseDto<List<TestCostDto>>> testTask =
                _yearCostsService.GetTestPlansAsync(project, year, allRecords);
            Task<ApiResponseDto<List<AnimalCostDto>>> animalTask =
                _yearCostsService.GetAnimalPlansAsync(project, year, allRecords);
            Task<ApiResponseDto<List<AdditionalCostDto>>> additionalTask =
                _yearCostsService.GetAdditionalPlansAsync(project, year, allRecords);

            await Task.WhenAll(staffTask, testTask, animalTask, additionalTask);

            decimal staffTotal      = (staffTask.Result.Data      ?? []).Sum(x => x.Cost      ?? 0m);
            decimal testTotal       = (testTask.Result.Data       ?? []).Sum(x => x.Cost      ?? 0m);
            decimal animalTotal     = (animalTask.Result.Data     ?? []).Sum(x => (decimal)(x.Cost ?? 0d));
            decimal additionalTotal = (additionalTask.Result.Data ?? []).Sum(x => x.ItemCost  ?? 0m);

            return Json(new
            {
                staffTotal      = staffTotal.ToString("C"),
                testTotal       = testTotal.ToString("C"),
                animalTotal     = animalTotal.ToString("C"),
                additionalTotal = additionalTotal.ToString("C")
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadPlanStaffGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<StaffCostPlanItem> grid =
                await BuildPlanStaffGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadPlanTestGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<TestCostPlanItem> grid =
                await BuildPlanTestGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadPlanAnimalGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<AnimalCostPlanItem> grid =
                await BuildPlanAnimalGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadPlanAdditionalGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<AdditionalCostPlanItem> grid =
                await BuildPlanAdditionalGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<StaffCostPlanItem>> BuildPlanStaffGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<StaffCostDto>> response =
                await _yearCostsService.GetStaffPlansAsync(project, year, queryParameters);

            List<StaffCostPlanItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<StaffCostPlanItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<StaffCostPlanItem>
            {
                GridId = "planStaffGrid",
                Title = "Staff Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "WgGrade",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadPlanStaffGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<StaffCostPlanItem>(null),
                Pagination = pagination
            };
        }

        private async Task<DataGridConfig<TestCostPlanItem>> BuildPlanTestGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<TestCostDto>> response =
                await _yearCostsService.GetTestPlansAsync(project, year, queryParameters);

            List<TestCostPlanItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<TestCostPlanItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<TestCostPlanItem>
            {
                GridId = "planTestGrid",
                Title = "Test Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "TestCode",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadPlanTestGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestCostPlanItem>(null),
                Pagination = pagination
            };
        }

        private async Task<DataGridConfig<AnimalCostPlanItem>> BuildPlanAnimalGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<AnimalCostDto>> response =
                await _yearCostsService.GetAnimalPlansAsync(project, year, queryParameters);

            List<AnimalCostPlanItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<AnimalCostPlanItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<AnimalCostPlanItem>
            {
                GridId = "planAnimalGrid",
                Title = "Animal Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "AnimalType",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadPlanAnimalGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AnimalCostPlanItem>(null),
                Pagination = pagination
            };
        }

        private async Task<DataGridConfig<AdditionalCostPlanItem>> BuildPlanAdditionalGridAsync(
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
                GridId = "planAdditionalGrid",
                Title = "Additional Cost Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Account",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadPlanAdditionalGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AdditionalCostPlanItem>(null),
                Pagination = pagination
            };
        }

        [HttpPost]
        public async Task<IActionResult> LoadPactPayGrid(
            PaginationFilter<string> request, string project, short year)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            DataGridConfig<PactPayItem> grid = await BuildPactPayGridAsync(project, year, request);
            return PartialView("_DataGrid", grid);
        }

        [HttpGet]
        public async Task<IActionResult> GetPactPayTotals(string project, short year)
        {
            QueryParameters<string> allRecords = new() { Page = 1, PageSize = int.MaxValue };
            ApiResponseDto<List<PactPayDto>> response =
                await _yearCostsService.GetPactPayAsync(project, year, allRecords);

            List<PactPayDto> data = response.Data ?? [];
            return Json(new
            {
                payTotal        = data.Sum(x => x.Pay).ToString("C"),
                nonPayTotal     = data.Sum(x => x.NonPay).ToString("C"),
                overheadTotal   = data.Sum(x => x.Overhead).ToString("C"),
                staffCostsTotal = data.Sum(x => x.StaffCosts).ToString("C")
            });
        }

        private async Task<DataGridConfig<PactPayItem>> BuildPactPayGridAsync(
            string project, short year, PaginationFilter<string> request)
        {
            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<PactPayDto>> response =
                await _yearCostsService.GetPactPayAsync(project, year, queryParameters);

            List<PactPayItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<PactPayItem>>(response.Data)
                : [];

            PaginationModel pagination = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<PactPayItem>
            {
                GridId = "pactPayGrid",
                Title = "Pact Pay",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Month",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                ExtraFilterMethod = "getYearCostsExtraFilters",
                BindGridUrl = "/PIMS/ProjectYearCosts/LoadPactPayGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<PactPayItem>(null),
                Pagination = pagination
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectYearDetails(string project, short year)
        {
            ApiResponseDto<ProjectYearDetailsDto> response =
                await _yearCostsService.GetProjectYearDetailsAsync(project, year);

            ProjectYearDetailsDto d = response.Data ?? new ProjectYearDetailsDto();

            return Json(new
            {
                year              = d.Year,
                parentproject     = d.Parentproject,
                manager           = d.Manager,
                disease           = d.Disease,
                contract          = d.Contract,
                finished          = d.Finished,
                transferincome    = d.Transferincome?.ToString("C"),
                custincome        = d.Custincome?.ToString("C"),
                feccost           = d.Feccost?.ToString("C"),
                profit            = d.Profit?.ToString("C"),
                carryover         = d.Carryover?.ToString("C"),
                budgetcvl         = d.BudgetCvl?.ToString("C"),
                caseworksub       = d.Caseworksub?.ToString("C"),
                costcentre        = d.Costcentre.HasValue ? ((long)d.Costcentre.Value).ToString() : null,
                pvsincome         = d.Pvsincome?.ToString("C"),
                oracleprojectcode = d.Oracleprojectcode,
                plancaseworkdebit = d.Plancaseworkdebit?.ToString("C"),
                subaccountcode    = d.Subaccountcode,
                source            = d.Source,
                projectgroup      = d.Projectgroup,
                isdefraproject    = d.Isdefraproject,
                comments          = d.Comments
            });
        }
    }
}

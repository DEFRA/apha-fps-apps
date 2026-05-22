using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
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
    public class ProjectPlanningController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IStaffJobService _staffJobService;
        private readonly IAnimalPlanService _animalPlanService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly IAdditionalCostService _additionalCostService;
        private readonly IProjectService _projectService;

        public ProjectPlanningController(
            IMapper mapper,
            IStaffJobService staffJobService,
            IAnimalPlanService animalPlanService,
            ITestRequirementService testRequirementService,
            IAdditionalCostService additionalCostService,
            IProjectService projectService)
        {
            _mapper = mapper;
            _staffJobService = staffJobService;
            _animalPlanService = animalPlanService;
            _testRequirementService = testRequirementService;
            _additionalCostService = additionalCostService;
            _projectService = projectService;
        }

        public async Task<IActionResult> Index(string projectCode, string? selectedYear = null)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                throw new InvalidOperationException("Project code is required.");

            var projectResult = await _projectService.GetProjectByIdAsync(projectCode);
            if (!projectResult.Success || projectResult.Data == null)
            {
                TempData["ErrorMessage"] = projectResult.Errors?.FirstOrDefault()?.Message
                    ?? "Project not found.";
                return RedirectToAction("Index", "Home");
            }

            var project = projectResult.Data;

            var model = new Models.ProjectPlanningViewModel
            {
                ProjectCode        = project.ParentProject,
                ProjectDescription = project.ProjectTitle,
                SelectedProgramme  = project.Program ?? string.Empty,
                SelectedYear       = selectedYear ?? string.Empty,
                UserName           = User.Identity?.Name ?? string.Empty,
                BudgetCVL          = project.BudgetCvl ?? 0m,
                TransferIncome     = project.TransferIncome,
                ExternalIncome     = project.BudgetExt ?? 0m,
                StaffBookedGrid       = await GetStaffBookedDataGrid(projectCode),
                AnimalsBookedGrid     = await GetAnimalsBookedDataGrid(projectCode),
                TestsBookedGrid       = await GetTestsBookedDataGrid(projectCode),
                ExceptionalCostsGrid  = await GetExceptionalCostsDataGrid(projectCode)
            };

            return View(model);
        }

        private async Task<DataGridConfig<StaffJobItemViewModel>> GetStaffBookedDataGrid(string jobcode)
        {
            var staffJobPagedData = await _staffJobService.GetAllStaffJobsAsync(new QueryParameters<string>(), jobcode);
            List<StaffJobItemViewModel> staffJobItems = new List<StaffJobItemViewModel>();
            if (staffJobPagedData.Data != null)
            {
                staffJobItems = _mapper.Map<List<StaffJobItemViewModel>>(staffJobPagedData.Data.ToList());
            }
            PaginationModel paginationModel = _mapper.Map<PaginationModel>(staffJobPagedData.Pagination) ?? new PaginationModel();
     
            var staffJobGridConfig = new DataGridConfig<StaffJobItemViewModel>
            {
                GridId = "staffBookedGrid",
                Title = "Staff Booked",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffID",
                AddFunction = "addStaffJob",
                EditFunction = "editStaffJob",
                DeleteFunction = "deleteStaffJob",
                ExtraFilterMethod = "getStaffJobExtraFilters",
                BindGridUrl = $"/FPS/StaffJob/LoadStaffJobGrid?title={Uri.EscapeDataString("Staff Booked")}",
                Data = staffJobItems,
                Columns = GridDataProvider.GetColumnsDefination<StaffJobItemViewModel>(null),
                Pagination = paginationModel
            };

            return staffJobGridConfig;
        }

        private async Task<DataGridConfig<AnimalPlanItem>> GetAnimalsBookedDataGrid(string jobcode)
        {
            var pagedData = await _animalPlanService.GetAllAnimalCostAsync(new QueryParameters<string>(), jobcode);
            List<AnimalPlanItem> items = pagedData.Data != null
                ? _mapper.Map<List<AnimalPlanItem>>(pagedData.Data.ToList())
                : new List<AnimalPlanItem>();
            PaginationModel paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();

            return new DataGridConfig<AnimalPlanItem>
            {
                GridId = "animalBookedGrid",
                Title = "Animals Booked",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "IndCounter",
                AddFunction = "addAnimalPlan",
                EditFunction = "editAnimalPlan",
                DeleteFunction = "deleteAnimalPlan",
                ExtraFilterMethod = "getAnimalPlanExtraFilters",
                BindGridUrl = $"/FPS/AnimalJob/LoadAnimalPlanGrid?title={Uri.EscapeDataString("Animals Booked")}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AnimalPlanItem>(null),
                Pagination = paginationModel
            };
        }

        private async Task<DataGridConfig<TestPlanItem>> GetTestsBookedDataGrid(string jobcode)
        {
            var query = new QueryParameters<string>();
            var response = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(query, jobcode);
            List<TestPlanItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<TestPlanItem>>(response.Data)
                : new List<TestPlanItem>();
            PaginationModel paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);

            return new DataGridConfig<TestPlanItem>
            {
                GridId = "testPlanGrid",
                Title = "Tests Booked",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = true,
                AllowDelete = true,
                KeyProperty = "TestCode",
                AddFunction = "addTestPlan",
                EditFunction = "editTestPlan",
                DeleteFunction = "deleteTestPlan",
                ExtraFilterMethod = "getTestPlanExtraFilters",
                BindGridUrl = $"/FPS/TestPlanJob/LoadTestPlanGrid?title={Uri.EscapeDataString("Tests Booked")}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestPlanItem>(null),
                Pagination = paginationModel
            };
        }

        private async Task<DataGridConfig<AdditionalCostItemViewModel>> GetExceptionalCostsDataGrid(string jobcode)
        {
            var pagedData = await _additionalCostService.GetAdditionalCostsAsync(new QueryParameters<string>(), jobcode);
            List<AdditionalCostItemViewModel> items = pagedData.Data != null
                ? _mapper.Map<List<AdditionalCostItemViewModel>>(pagedData.Data)
                : new List<AdditionalCostItemViewModel>();
            PaginationModel paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();

            return new DataGridConfig<AdditionalCostItemViewModel>
            {
                GridId = "additionalCostGrid",
                Title = "Exceptional Costs",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                KeyProperty = "Description",
                AddFunction = "addAdditionalCost",
                EditFunction = "editAdditionalCost",
                DeleteFunction = "deleteAdditionalCost",
                ExtraFilterMethod = "getAdditionalCostExtraFilters",
                BindGridUrl = $"/FPS/AdditionalCostJob/LoadAdditionalCostGrid?title={Uri.EscapeDataString("Exceptional Costs")}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AdditionalCostItemViewModel>(null),
                Pagination = paginationModel
            };
        }
    }
}

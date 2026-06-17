using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.Common.Utilities.StateManagement;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectPlanViewerControllerTest
{
    public class ProjectPlanViewerControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProgramService _programService;
        private readonly IStaffJobService _staffJobService;
        private readonly IAnimalPlanService _animalPlanService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly IAdditionalCostService _additionalCostService;
        private readonly ITimeCostCalcsService _timeCostCalcsService;
        private readonly IMonthlyOutputService _monthlyOutputService;
        private readonly IProjectSubContractService _projectSubContractService;
        private readonly IAppStateService _appStateService;
        private readonly ProjectPlanViewerController _controller;

        public ProjectPlanViewerControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _programService = Substitute.For<IProgramService>();
            _staffJobService = Substitute.For<IStaffJobService>();
            _animalPlanService = Substitute.For<IAnimalPlanService>();
            _testRequirementService = Substitute.For<ITestRequirementService>();
            _additionalCostService = Substitute.For<IAdditionalCostService>();
            _timeCostCalcsService = Substitute.For<ITimeCostCalcsService>();
            _monthlyOutputService = Substitute.For<IMonthlyOutputService>();
            _projectSubContractService = Substitute.For<IProjectSubContractService>();
            _appStateService = Substitute.For<IAppStateService>();

            _controller = new ProjectPlanViewerController(
                _mapper,
                _projectService,
                _programService,
                _staffJobService,
                _animalPlanService,
                _testRequirementService,
                _additionalCostService,
                _timeCostCalcsService,
                _monthlyOutputService,
                _projectSubContractService,
                _appStateService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private record JsonResponse(bool success, string? message);

        private static ProjectDto BuildProjectDto(string projectCode = "AH0001") => new()
        {
            ParentProject = projectCode,
            ProjectTitle = "Test Project",
            ShortTitle = "TP",
            Customer = "DEFRA",
            Program = "P01",
            Manager = "John",
            Disease = "TB",
            CustIncome = 1000m,
            TransferIncome = 500m,
            Profit = 100m,
            ProjectStatus = "Active",
            CostBookNo = "CB01",
            Contract = "C001",
            IsDefraProject = 1,
            CostCentre = 100,
            OwningRc = "RC1",
            ProjectGroup = "G1",
            IncomeAccountCode = "IAC01",
            SubAccountCode = "SAC01",
            BudgetCvl = 5000m,
            PvsIncome = 200m,
            PlanCaseWorkDebit = 50m,
            CarryOver = 300m,
            CarryOverSeed = 10m,
            Comments = "Test comments"
        };

        private static TestRequirementDto BuildTestRequirementDto(string testCode = "T01", string buyer = "B01") => new()
        {
            TestCode = testCode,
            Buyer = buyer,
            UnitPrice = 25m,
            NoRequired = 10
        };

        private void SetupDefaultServices(string projectCode = "AH0001")
        {
            // Programs
            _programService.GetAllProgramsUnfilteredAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(
                    new List<ProgramDto> { new() { ProgramNo = "P01" } }));

            // Project groups
            _projectService.GetAllProjectGroupsAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(
                    new List<ProjectGroupDto> { new() { ProjectGroupName = "G1", ProjectGroup = "G1" } }));

            // Projects
            _projectService.GetAllProjectsUnfilteredAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                    new List<ProjectDto> { BuildProjectDto(projectCode) }));

            // Session state
            _appStateService.GetSessionAsync<string>(Arg.Any<string>())
                .Returns((string?)null);

            // Mapper for pagination
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupProjectDetailsServices(string projectCode = "AH0001")
        {
            SetupDefaultServices(projectCode);

            // Project details
            _projectService.GetProjectByIdAsync(projectCode)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(BuildProjectDto(projectCode)));

            // Staff cost
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(1000m));

            // Animal cost
            _animalPlanService.GetTotalAnimalCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(500m));

            // Additional cost
            _additionalCostService.GetTotalItemCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(300m));

            // Test requirements
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                    new List<TestRequirementDto> { BuildTestRequirementDto() }));

            // Time cost calcs
            _timeCostCalcsService.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(
                    new TimeCostCalcsTotalsDto { TotalHours = 50, TotalCost = 800 }));

            // Monthly output
            _monthlyOutputService.GetTotalActualByProjectAsync(projectCode, Arg.Any<Dictionary<(string, string), decimal>>())
                .Returns(ApiResponseDto<double>.SuccessResponse(200.0));

            // Project sub contracts - animal
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: true)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(400m));

            // Project sub contracts - additional
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: false)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(250m));
        }

        #region Access / Authorization Attribute Tests

        [Fact]
        public void Controller_HasAreaAttribute_FPS()
        {
            var attrs = typeof(ProjectPlanViewerController)
                .GetCustomAttributes(typeof(AreaAttribute), true);
            Assert.NotEmpty(attrs);
            var area = (AreaAttribute)attrs[0];
            Assert.Equal("FPS", area.RouteValue);
        }

        [Fact]
        public void Controller_HasAuthorizeAttribute_WithExpectedRoles()
        {
            var attrs = typeof(ProjectPlanViewerController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true);
            Assert.NotEmpty(attrs);
            var auth = (AuthorizeAttribute)attrs[0];
            Assert.Contains("FPSAdmin", auth.Roles);
            Assert.Contains("FPSUser", auth.Roles);
        }

        [Fact]
        public void Controller_HasAuthorizeForScopesAttribute()
        {
            var attrs = typeof(ProjectPlanViewerController)
                .GetCustomAttributes(typeof(AuthorizeForScopesAttribute), true);
            Assert.NotEmpty(attrs);
        }

        [Fact]
        public void Index_HasHttpGetAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethods()
                .First(m => m.Name == nameof(ProjectPlanViewerController.Index));
            var attr = method.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void GetProjectDetails_HasHttpGetAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.GetProjectDetails));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void GetPlanSummaryTotals_HasHttpGetAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.GetPlanSummaryTotals));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void GetStaffPlanVsActualTotals_HasHttpGetAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.GetStaffPlanVsActualTotals));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void GetTestPlanVsActualTotals_HasHttpGetAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.GetTestPlanVsActualTotals));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void GetAnimalPlanVsActualTotals_HasHttpGetAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.GetAnimalPlanVsActualTotals));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void GetAdditionalPlanVsActualTotals_HasHttpGetAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.GetAdditionalPlanVsActualTotals));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void LoadProjectDetailsGrid_HasHttpPostAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.LoadProjectDetailsGrid));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPostAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void LoadStaffPlanGrid_HasHttpPostAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.LoadStaffPlanGrid));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPostAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void LoadTestPlanGrid_HasHttpPostAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.LoadTestPlanGrid));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPostAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void LoadAnimalPlanGrid_HasHttpPostAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.LoadAnimalPlanGrid));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPostAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void LoadAdditionalCostGrid_HasHttpPostAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.LoadAdditionalCostGrid));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPostAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void LoadStaffActualGrid_HasHttpPostAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.LoadStaffActualGrid));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPostAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void LoadTestActualGrid_HasHttpPostAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.LoadTestActualGrid));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPostAttribute), true);
            Assert.NotEmpty(attr);
        }

        [Fact]
        public void LoadActualCostGrid_HasHttpPostAttribute()
        {
            var method = typeof(ProjectPlanViewerController).GetMethod(nameof(ProjectPlanViewerController.LoadActualCostGrid));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpPostAttribute), true);
            Assert.NotEmpty(attr);
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_WithNoParameters_ReturnsViewResult()
        {
            SetupDefaultServices();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_WithNoParameters_PopulatesProgramList()
        {
            SetupDefaultServices();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.NotEmpty(model.ProgramList);
        }

        [Fact]
        public async Task Index_WithNoParameters_PopulatesProjectGroupList()
        {
            SetupDefaultServices();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.NotEmpty(model.ProjectGroupList);
        }

        [Fact]
        public async Task Index_WithNoParameters_PopulatesProjectList()
        {
            SetupDefaultServices();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.NotEmpty(model.ProjectList);
        }

        [Fact]
        public async Task Index_WithProjectCode_SetsSelectedProjectCode()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal("AH0001", model.SelectedProjectCode);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesProjectDetails()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal("AH0001", model.ProjectDetails.SelectedProjectCode);
        }

        [Fact]
        public async Task Index_WithProgram_SetsSelectedProgramInModel()
        {
            SetupDefaultServices();

            var result = await _controller.Index(program: "P01");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal("P01", model.SelectedProgram);
        }

        [Fact]
        public async Task Index_WithProjectGroup_SetsSelectedProjectGroupInModel()
        {
            SetupDefaultServices();

            var result = await _controller.Index(projectGroup: "G1");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal("G1", model.SelectedProjectGroup);
        }

        [Fact]
        public async Task Index_WithNullProjectCode_FallsBackToSession()
        {
            SetupDefaultServices();
            _appStateService.GetSessionAsync<string>(Arg.Any<string>())
                .Returns("AH0001");
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index();

            await _appStateService.Received(1).GetSessionAsync<string>(Arg.Any<string>());
        }

        [Fact]
        public async Task Index_WithProjectCodeNotInList_SetsSelectedProjectCodeToEmpty()
        {
            SetupDefaultServices();

            var result = await _controller.Index(projectCode: "NOTEXIST");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProjectCode);
        }

        [Fact]
        public async Task Index_WithEmptyProjectCode_DoesNotPopulateProjectDetails()
        {
            SetupDefaultServices();

            var result = await _controller.Index(projectCode: "");

            await _projectService.DidNotReceive().GetProjectByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task Index_SetsSelectedProgramOnModel()
        {
            SetupDefaultServices();

            var result = await _controller.Index(program: "P01");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal("P01", model.SelectedProgram);
        }

        [Fact]
        public async Task Index_SetsSelectedProjectGroupOnModel()
        {
            SetupDefaultServices();

            var result = await _controller.Index(projectGroup: "G1");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal("G1", model.SelectedProjectGroup);
        }

        [Fact]
        public async Task Index_WhenProgramServiceReturnsEmpty_ProgramListIsEmpty()
        {
            _programService.GetAllProgramsUnfilteredAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(new List<ProgramDto>()));
            _projectService.GetAllProjectGroupsAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(new List<ProjectGroupDto>()));
            _projectService.GetAllProjectsUnfilteredAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>()));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>()).Returns((string?)null);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Empty(model.ProgramList);
        }

        [Fact]
        public async Task Index_WhenProgramServiceFails_ProgramListIsEmpty()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "500" } };
            _programService.GetAllProgramsUnfilteredAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(errors, new ApiMetaDto()));
            _projectService.GetAllProjectGroupsAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(new List<ProjectGroupDto>()));
            _projectService.GetAllProjectsUnfilteredAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>()));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>()).Returns((string?)null);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Empty(model.ProgramList);
        }

        #endregion

        #region GetProjectDetails Tests

        [Fact]
        public async Task GetProjectDetails_WithEmptyProjectCode_ReturnsJsonError()
        {
            var result = await _controller.GetProjectDetails("");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Project code is required.", value.message);
        }

        [Fact]
        public async Task GetProjectDetails_WithNullProjectCode_ReturnsJsonError()
        {
            var result = await _controller.GetProjectDetails(null!);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task GetProjectDetails_WithWhitespace_ReturnsJsonError()
        {
            var result = await _controller.GetProjectDetails("   ");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task GetProjectDetails_WhenProjectNotFound_ReturnsJsonError()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            _projectService.GetProjectByIdAsync("NOTEXIST")
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            var result = await _controller.GetProjectDetails("NOTEXIST");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task GetProjectDetails_WhenDataIsNull_ReturnsJsonError()
        {
            var response = new ApiResponseDto<ProjectDto> { Success = true, Data = null };
            _projectService.GetProjectByIdAsync("AH0001").Returns(response);

            var result = await _controller.GetProjectDetails("AH0001");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task GetProjectDetails_WithValidProject_ReturnsSuccessJson()
        {
            _projectService.GetProjectByIdAsync("AH0001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(BuildProjectDto()));

            var result = await _controller.GetProjectDetails("AH0001");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.GetProperty("success").GetBoolean());
            Assert.Equal("AH0001", root.GetProperty("projectCode").GetString());
            Assert.Equal("Test Project", root.GetProperty("projectTitle").GetString());
        }

        [Fact]
        public async Task GetProjectDetails_WithValidProject_ReturnsAllExpectedFields()
        {
            _projectService.GetProjectByIdAsync("AH0001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(BuildProjectDto()));

            var result = await _controller.GetProjectDetails("AH0001");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal("TP", root.GetProperty("shortTitle").GetString());
            Assert.Equal("DEFRA", root.GetProperty("customer").GetString());
            Assert.Equal("P01", root.GetProperty("program").GetString());
            Assert.Equal("John", root.GetProperty("manager").GetString());
            Assert.Equal("TB", root.GetProperty("disease").GetString());
            Assert.Equal("Active", root.GetProperty("projectStatus").GetString());
            Assert.Equal("CB01", root.GetProperty("costBookNo").GetString());
            Assert.Equal("C001", root.GetProperty("contract").GetString());
            Assert.Equal(1, root.GetProperty("isDefraProject").GetInt32());
        }

        [Fact]
        public async Task GetProjectDetails_WhenServiceReturnsErrorMessage_ReturnsFirstErrorMessage()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Custom error", Code = "500" } };
            _projectService.GetProjectByIdAsync("AH0001")
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            var result = await _controller.GetProjectDetails("AH0001");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.Equal("Custom error", value.message);
        }

        #endregion

        #region GetPlanSummaryTotals Tests

        [Fact]
        public async Task GetPlanSummaryTotals_WithEmptyProjectCode_ReturnsJsonError()
        {
            var result = await _controller.GetPlanSummaryTotals("");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Project code is required.", value.message);
        }

        [Fact]
        public async Task GetPlanSummaryTotals_WithNullProjectCode_ReturnsJsonError()
        {
            var result = await _controller.GetPlanSummaryTotals(null!);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task GetPlanSummaryTotals_WithValidProject_ReturnsSuccessJson()
        {
            const string projectCode = "AH0001";
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(1000m));
            _animalPlanService.GetTotalAnimalCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(500m));
            _additionalCostService.GetTotalItemCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(300m));
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                    new List<TestRequirementDto> { BuildTestRequirementDto() }));

            var result = await _controller.GetPlanSummaryTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.GetProperty("success").GetBoolean());
            Assert.Equal(1000m, root.GetProperty("totalStaffCost").GetDecimal());
            Assert.Equal(500m, root.GetProperty("totalAnimalCost").GetDecimal());
            Assert.Equal(300m, root.GetProperty("totalAdditionalCost").GetDecimal());
        }

        [Fact]
        public async Task GetPlanSummaryTotals_CalculatesTestCostFromRequirements()
        {
            const string projectCode = "AH0001";
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _animalPlanService.GetTotalAnimalCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _additionalCostService.GetTotalItemCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                    new List<TestRequirementDto>
                    {
                        new() { TestCode = "T01", Buyer = "B01", UnitPrice = 10m, NoRequired = 5 },
                        new() { TestCode = "T02", Buyer = "B01", UnitPrice = 20m, NoRequired = 3 }
                    }));

            var result = await _controller.GetPlanSummaryTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            // 10*5 + 20*3 = 110
            Assert.Equal(110m, root.GetProperty("totalTestCost").GetDecimal());
        }

        [Fact]
        public async Task GetPlanSummaryTotals_WithNoTestRequirements_TestCostIsZero()
        {
            const string projectCode = "AH0001";
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _animalPlanService.GetTotalAnimalCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _additionalCostService.GetTotalItemCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(new List<TestRequirementDto>()));

            var result = await _controller.GetPlanSummaryTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal(0m, root.GetProperty("totalTestCost").GetDecimal());
        }

        [Fact]
        public async Task GetPlanSummaryTotals_WithNullTestData_TestCostIsZero()
        {
            const string projectCode = "AH0001";
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _animalPlanService.GetTotalAnimalCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _additionalCostService.GetTotalItemCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            var failedResponse = ApiResponseDto<List<TestRequirementDto>>.FailureResponse(
                new List<ApiErrorDto>(), new ApiMetaDto());
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(failedResponse);

            var result = await _controller.GetPlanSummaryTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal(0m, root.GetProperty("totalTestCost").GetDecimal());
        }

        #endregion

        #region GetStaffPlanVsActualTotals Tests

        [Fact]
        public async Task GetStaffPlanVsActualTotals_WithEmptyProjectCode_ReturnsJsonError()
        {
            var result = await _controller.GetStaffPlanVsActualTotals("");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task GetStaffPlanVsActualTotals_WithValidProject_ReturnsSuccess()
        {
            const string projectCode = "AH0001";
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(1000m));
            _timeCostCalcsService.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(
                    new TimeCostCalcsTotalsDto { TotalHours = 50, TotalCost = 800 }));

            var result = await _controller.GetStaffPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.GetProperty("success").GetBoolean());
            Assert.Equal(1000m, root.GetProperty("totalPlannedCost").GetDecimal());
            Assert.Equal(50, root.GetProperty("totalActualHrs").GetDouble());
            Assert.Equal(800, root.GetProperty("totalActualCost").GetDouble());
        }

        [Fact]
        public async Task GetStaffPlanVsActualTotals_CalculatesPercentOfPlan()
        {
            const string projectCode = "AH0001";
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(1000m));
            _timeCostCalcsService.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(
                    new TimeCostCalcsTotalsDto { TotalHours = 50, TotalCost = 500 }));

            var result = await _controller.GetStaffPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            // 500 / 1000 * 100 = 50
            Assert.Equal(50, root.GetProperty("percentOfPlan").GetDouble());
        }

        [Fact]
        public async Task GetStaffPlanVsActualTotals_WithZeroPlannedCost_PercentIsZero()
        {
            const string projectCode = "AH0001";
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _timeCostCalcsService.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(
                    new TimeCostCalcsTotalsDto { TotalHours = 10, TotalCost = 100 }));

            var result = await _controller.GetStaffPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal(0, root.GetProperty("percentOfPlan").GetDouble());
        }

        [Fact]
        public async Task GetStaffPlanVsActualTotals_WithNullActualData_DefaultsToZero()
        {
            const string projectCode = "AH0001";
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(1000m));
            _timeCostCalcsService.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(null!));

            var result = await _controller.GetStaffPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal(0, root.GetProperty("totalActualHrs").GetDouble());
            Assert.Equal(0, root.GetProperty("totalActualCost").GetDouble());
        }

        #endregion

        #region GetTestPlanVsActualTotals Tests

        [Fact]
        public async Task GetTestPlanVsActualTotals_WithEmptyProjectCode_ReturnsJsonError()
        {
            var result = await _controller.GetTestPlanVsActualTotals("");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task GetTestPlanVsActualTotals_WithValidProject_ReturnsSuccess()
        {
            const string projectCode = "AH0001";
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                    new List<TestRequirementDto> { BuildTestRequirementDto() }));
            _monthlyOutputService.GetTotalActualByProjectAsync(projectCode, Arg.Any<Dictionary<(string, string), decimal>>())
                .Returns(ApiResponseDto<double>.SuccessResponse(150.0));

            var result = await _controller.GetTestPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.GetProperty("success").GetBoolean());
            // UnitPrice(25) * NoRequired(10) = 250
            Assert.Equal(250m, root.GetProperty("totalPlannedCost").GetDecimal());
            Assert.Equal(150.0, root.GetProperty("totalActualCost").GetDouble());
        }

        [Fact]
        public async Task GetTestPlanVsActualTotals_CalculatesPercentOfPlan()
        {
            const string projectCode = "AH0001";
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                    new List<TestRequirementDto>
                    {
                        new() { TestCode = "T01", Buyer = "B01", UnitPrice = 50m, NoRequired = 4 }
                    }));
            _monthlyOutputService.GetTotalActualByProjectAsync(projectCode, Arg.Any<Dictionary<(string, string), decimal>>())
                .Returns(ApiResponseDto<double>.SuccessResponse(100.0));

            var result = await _controller.GetTestPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            // planned = 50*4 = 200, actual = 100, percent = 100/200*100 = 50
            Assert.Equal(50, root.GetProperty("percentOfPlan").GetDouble());
        }

        [Fact]
        public async Task GetTestPlanVsActualTotals_WithZeroPlannedCost_PercentIsZero()
        {
            const string projectCode = "AH0001";
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(new List<TestRequirementDto>()));
            _monthlyOutputService.GetTotalActualByProjectAsync(projectCode, Arg.Any<Dictionary<(string, string), decimal>>())
                .Returns(ApiResponseDto<double>.SuccessResponse(0));

            var result = await _controller.GetTestPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal(0, root.GetProperty("percentOfPlan").GetDouble());
        }

        [Fact]
        public async Task GetTestPlanVsActualTotals_WithFailedActualResult_ActualCostIsZero()
        {
            const string projectCode = "AH0001";
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                    new List<TestRequirementDto> { BuildTestRequirementDto() }));
            _monthlyOutputService.GetTotalActualByProjectAsync(projectCode, Arg.Any<Dictionary<(string, string), decimal>>())
                .Returns(ApiResponseDto<double>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto()));

            var result = await _controller.GetTestPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal(0, root.GetProperty("totalActualCost").GetDouble());
        }

        #endregion

        #region GetAnimalPlanVsActualTotals Tests

        [Fact]
        public async Task GetAnimalPlanVsActualTotals_WithEmptyProjectCode_ReturnsJsonError()
        {
            var result = await _controller.GetAnimalPlanVsActualTotals("");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task GetAnimalPlanVsActualTotals_WithValidProject_ReturnsSuccess()
        {
            const string projectCode = "AH0001";
            _animalPlanService.GetTotalAnimalCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(500m));
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: true)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(300m));

            var result = await _controller.GetAnimalPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.GetProperty("success").GetBoolean());
            Assert.Equal(500m, root.GetProperty("totalPlannedCost").GetDecimal());
            Assert.Equal(300m, root.GetProperty("totalActualCost").GetDecimal());
        }

        [Fact]
        public async Task GetAnimalPlanVsActualTotals_CalculatesPercentOfPlan()
        {
            const string projectCode = "AH0001";
            _animalPlanService.GetTotalAnimalCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(1000m));
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: true)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(250m));

            var result = await _controller.GetAnimalPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            // 250 / 1000 * 100 = 25
            Assert.Equal(25, root.GetProperty("percentOfPlan").GetDouble());
        }

        [Fact]
        public async Task GetAnimalPlanVsActualTotals_WithZeroPlannedCost_PercentIsZero()
        {
            const string projectCode = "AH0001";
            _animalPlanService.GetTotalAnimalCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: true)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(100m));

            var result = await _controller.GetAnimalPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal(0, root.GetProperty("percentOfPlan").GetDouble());
        }

        #endregion

        #region GetAdditionalPlanVsActualTotals Tests

        [Fact]
        public async Task GetAdditionalPlanVsActualTotals_WithEmptyProjectCode_ReturnsJsonError()
        {
            var result = await _controller.GetAdditionalPlanVsActualTotals("");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task GetAdditionalPlanVsActualTotals_WithValidProject_ReturnsSuccess()
        {
            const string projectCode = "AH0001";
            _additionalCostService.GetTotalItemCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(600m));
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: false)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(200m));

            var result = await _controller.GetAdditionalPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.GetProperty("success").GetBoolean());
            Assert.Equal(600m, root.GetProperty("totalPlannedCost").GetDecimal());
            Assert.Equal(200m, root.GetProperty("totalActualCost").GetDecimal());
        }

        [Fact]
        public async Task GetAdditionalPlanVsActualTotals_CalculatesPercentOfPlan()
        {
            const string projectCode = "AH0001";
            _additionalCostService.GetTotalItemCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(400m));
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: false)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(100m));

            var result = await _controller.GetAdditionalPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            // 100 / 400 * 100 = 25
            Assert.Equal(25, root.GetProperty("percentOfPlan").GetDouble());
        }

        [Fact]
        public async Task GetAdditionalPlanVsActualTotals_WithZeroPlannedCost_PercentIsZero()
        {
            const string projectCode = "AH0001";
            _additionalCostService.GetTotalItemCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: false)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(50m));

            var result = await _controller.GetAdditionalPlanVsActualTotals(projectCode);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.Equal(0, root.GetProperty("percentOfPlan").GetDouble());
        }

        #endregion

        #region LoadProjectDetailsGrid Tests

        [Fact]
        public async Task LoadProjectDetailsGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string>();

            var result = await _controller.LoadProjectDetailsGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadProjectDetailsGrid_WithParentProject_FetchesSingleProject()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
            _projectService.GetProjectByIdAsync("AH0001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(BuildProjectDto()));

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadProjectDetailsGrid(request, parentProject: "AH0001");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadProjectDetailsGrid_WithProgram_FiltersProjectsByProgram()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
            _projectService.GetProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), "P01")
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                    new List<ProjectDto> { BuildProjectDto() },
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }));

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadProjectDetailsGrid(request, program: "P01");

            await _projectService.Received(1).GetProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), "P01");
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadProjectDetailsGrid_WithProjectGroup_FiltersProjectsByGroup()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
            _projectService.GetProjectsByProjectGroupAsync(Arg.Any<QueryParameters<string>>(), "G1")
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                    new List<ProjectDto> { BuildProjectDto() },
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }));

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadProjectDetailsGrid(request, projectGroup: "G1");

            await _projectService.Received(1).GetProjectsByProjectGroupAsync(Arg.Any<QueryParameters<string>>(), "G1");
        }

        [Fact]
        public async Task LoadProjectDetailsGrid_WithNoFilters_CallsGetPagedProjects()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
            _projectService.GetPagedProjectsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                    new List<ProjectDto> { BuildProjectDto() },
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }));

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadProjectDetailsGrid(request);

            await _projectService.Received(1).GetPagedProjectsAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadProjectDetailsGrid_WithParentProjectNotFound_ReturnsEmptyGrid()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            _projectService.GetProjectByIdAsync("NOTEXIST")
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadProjectDetailsGrid(request, parentProject: "NOTEXIST");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectDetailsGridItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadProjectDetailsGrid_WithFilter_DeserializesFilter()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
            _projectService.GetPagedProjectsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                    new List<ProjectDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));

            var request = new PaginationFilter<string> { Filter = "{\"ParentProject\":\"AH0001\"}" };
            var result = await _controller.LoadProjectDetailsGrid(request);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectDetailsGridItem>>(partialResult.Model);
            Assert.NotNull(gridConfig.CurrentFilters);
        }

        #endregion

        #region LoadStaffPlanGrid Tests

        [Fact]
        public async Task LoadStaffPlanGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string>();

            var result = await _controller.LoadStaffPlanGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadStaffPlanGrid_WithValidRequest_ReturnsPartialView()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _staffJobService.GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(
                    new List<StaffJobViewDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<StaffJobItemViewModel>>(Arg.Any<List<StaffJobViewDto>>())
                .Returns(new List<StaffJobItemViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadStaffPlanGrid(request, parentProject: "AH0001");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadStaffPlanGrid_WithGridId_UsesProvidedGridId()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _staffJobService.GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(
                    new List<StaffJobViewDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<StaffJobItemViewModel>>(Arg.Any<List<StaffJobViewDto>>())
                .Returns(new List<StaffJobItemViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadStaffPlanGrid(request, parentProject: "AH0001", gridId: "staffPlanGrid");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobItemViewModel>>(partialResult.Model);
            Assert.Equal("staffPlanGrid", gridConfig.GridId);
        }

        [Fact]
        public async Task LoadStaffPlanGrid_WithNullParentProject_ReturnsEmptyGrid()
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadStaffPlanGrid(request);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobItemViewModel>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            await _staffJobService.DidNotReceive().GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        #endregion

        #region LoadTestPlanGrid Tests

        [Fact]
        public async Task LoadTestPlanGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string>();

            var result = await _controller.LoadTestPlanGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadTestPlanGrid_WithValidRequest_ReturnsPartialView()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                    new List<TestRequirementDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<TestPlanActualItem>>(Arg.Any<List<TestRequirementDto>>())
                .Returns(new List<TestPlanActualItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadTestPlanGrid(request, parentProject: "AH0001");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadTestPlanGrid_WithGridId_UsesProvidedGridId()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                    new List<TestRequirementDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<TestPlanActualItem>>(Arg.Any<List<TestRequirementDto>>())
                .Returns(new List<TestPlanActualItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadTestPlanGrid(request, parentProject: "AH0001", gridId: "testPlanGrid");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<TestPlanActualItem>>(partialResult.Model);
            Assert.Equal("testPlanGrid", gridConfig.GridId);
        }

        #endregion

        #region LoadAnimalPlanGrid Tests

        [Fact]
        public async Task LoadAnimalPlanGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string>();

            var result = await _controller.LoadAnimalPlanGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadAnimalPlanGrid_WithValidRequest_ReturnsPartialView()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _animalPlanService.GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                    new List<AnimalCostViewDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<AnimalPlanItem>>(Arg.Any<List<AnimalCostViewDto>>())
                .Returns(new List<AnimalPlanItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadAnimalPlanGrid(request, parentProject: "AH0001");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadAnimalPlanGrid_WithGridId_UsesProvidedGridId()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _animalPlanService.GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                    new List<AnimalCostViewDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<AnimalPlanItem>>(Arg.Any<List<AnimalCostViewDto>>())
                .Returns(new List<AnimalPlanItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadAnimalPlanGrid(request, parentProject: "AH0001", gridId: "animalPlanGrid");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalPlanItem>>(partialResult.Model);
            Assert.Equal("animalPlanGrid", gridConfig.GridId);
        }

        #endregion

        #region LoadAdditionalCostGrid Tests

        [Fact]
        public async Task LoadAdditionalCostGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string>();

            var result = await _controller.LoadAdditionalCostGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadAdditionalCostGrid_WithValidRequest_ReturnsPartialView()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _additionalCostService.GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(
                    new List<AdditionalCostDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<AdditionalCostItemViewModel>>(Arg.Any<List<AdditionalCostDto>>())
                .Returns(new List<AdditionalCostItemViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadAdditionalCostGrid(request, parentProject: "AH0001");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadAdditionalCostGrid_WithGridId_UsesProvidedGridId()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _additionalCostService.GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(
                    new List<AdditionalCostDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<AdditionalCostItemViewModel>>(Arg.Any<List<AdditionalCostDto>>())
                .Returns(new List<AdditionalCostItemViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadAdditionalCostGrid(request, parentProject: "AH0001", gridId: "additionalCostPlanGrid");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AdditionalCostItemViewModel>>(partialResult.Model);
            Assert.Equal("additionalCostPlanGrid", gridConfig.GridId);
        }

        #endregion

        #region LoadStaffActualGrid Tests

        [Fact]
        public async Task LoadStaffActualGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string>();

            var result = await _controller.LoadStaffActualGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadStaffActualGrid_WithValidRequest_ReturnsPartialView()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _timeCostCalcsService.GetTimeCostCalcsByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(
                    new List<TimeCostCalcsViewDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<CompareStaff2Item>>(Arg.Any<List<TimeCostCalcsViewDto>>())
                .Returns(new List<CompareStaff2Item>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadStaffActualGrid(request, parentProject: "AH0001");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadStaffActualGrid_WithNullParentProject_ReturnsEmptyGrid()
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadStaffActualGrid(request);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<CompareStaff2Item>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            await _timeCostCalcsService.DidNotReceive().GetTimeCostCalcsByProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        #endregion

        #region LoadTestActualGrid Tests

        [Fact]
        public async Task LoadTestActualGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string>();

            var result = await _controller.LoadTestActualGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadTestActualGrid_WithValidRequest_ReturnsPartialView()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                    new List<TestRequirementDto> { BuildTestRequirementDto() }));
            _monthlyOutputService.GetMonthlyOutputByProjectAsync(
                    Arg.Any<QueryParameters<string>>(), "AH0001", Arg.Any<Dictionary<(string, string), decimal>>())
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(
                    new List<MonthlyOutputDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<ActualTestOutputItem>>(Arg.Any<List<MonthlyOutputDto>>())
                .Returns(new List<ActualTestOutputItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadTestActualGrid(request, parentProject: "AH0001");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadTestActualGrid_BuildsPriceLookupFromTestRequirements()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                    new List<TestRequirementDto>
                    {
                        new() { TestCode = "T01", Buyer = "B01", UnitPrice = 10m, NoRequired = 5 }
                    }));
            _monthlyOutputService.GetMonthlyOutputByProjectAsync(
                    Arg.Any<QueryParameters<string>>(), "AH0001", Arg.Any<Dictionary<(string, string), decimal>>())
                .Returns(ApiResponseDto<List<MonthlyOutputDto>>.SuccessResponse(
                    new List<MonthlyOutputDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<ActualTestOutputItem>>(Arg.Any<List<MonthlyOutputDto>>())
                .Returns(new List<ActualTestOutputItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            await _controller.LoadTestActualGrid(request, parentProject: "AH0001");

            var expectedKey = ("T01", "B01");
            await _monthlyOutputService.Received(1).GetMonthlyOutputByProjectAsync(
                Arg.Any<QueryParameters<string>>(),
                "AH0001",
                Arg.Is<Dictionary<(string, string), decimal>>(d => d.ContainsKey(expectedKey)));
        }

        #endregion

        #region LoadActualCostGrid Tests

        [Fact]
        public async Task LoadActualCostGrid_WithInvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string>();

            var result = await _controller.LoadActualCostGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task LoadActualCostGrid_WithAnimalOnly_PassesFilterFlag()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _projectSubContractService.GetFpsProjectSubContractsAsync(
                    Arg.Any<QueryParameters<string>>(), "AH0001", filterByAnimalAcctCodes: true)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(
                    new List<ProjectSubContractDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<ActualProjectCostItem>>(Arg.Any<List<ProjectSubContractDto>>())
                .Returns(new List<ActualProjectCostItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadActualCostGrid(request, parentProject: "AH0001", animalOnly: true);

            await _projectSubContractService.Received(1)
                .GetFpsProjectSubContractsAsync(Arg.Any<QueryParameters<string>>(), "AH0001", filterByAnimalAcctCodes: true);
        }

        [Fact]
        public async Task LoadActualCostGrid_WithoutAnimalOnly_PassesFalseFlag()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _projectSubContractService.GetFpsProjectSubContractsAsync(
                    Arg.Any<QueryParameters<string>>(), "AH0001", filterByAnimalAcctCodes: false)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(
                    new List<ProjectSubContractDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<ActualProjectCostItem>>(Arg.Any<List<ProjectSubContractDto>>())
                .Returns(new List<ActualProjectCostItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadActualCostGrid(request, parentProject: "AH0001", animalOnly: false);

            await _projectSubContractService.Received(1)
                .GetFpsProjectSubContractsAsync(Arg.Any<QueryParameters<string>>(), "AH0001", filterByAnimalAcctCodes: false);
        }

        [Fact]
        public async Task LoadActualCostGrid_WithGridId_UsesProvidedGridId()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _projectSubContractService.GetFpsProjectSubContractsAsync(
                    Arg.Any<QueryParameters<string>>(), "AH0001", filterByAnimalAcctCodes: false)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(
                    new List<ProjectSubContractDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<ActualProjectCostItem>>(Arg.Any<List<ProjectSubContractDto>>())
                .Returns(new List<ActualProjectCostItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadActualCostGrid(request, parentProject: "AH0001", gridId: "actualAdditionalCostGrid");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialResult.Model);
            Assert.Equal("actualAdditionalCostGrid", gridConfig.GridId);
        }

        [Fact]
        public async Task LoadActualCostGrid_WithAnimalOnly_SetsCorrectTitle()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _projectSubContractService.GetFpsProjectSubContractsAsync(
                    Arg.Any<QueryParameters<string>>(), "AH0001", filterByAnimalAcctCodes: true)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(
                    new List<ProjectSubContractDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<ActualProjectCostItem>>(Arg.Any<List<ProjectSubContractDto>>())
                .Returns(new List<ActualProjectCostItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadActualCostGrid(request, parentProject: "AH0001", animalOnly: true);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialResult.Model);
            Assert.Equal("Actual Animal Costs (PACT)", gridConfig.Title);
        }

        [Fact]
        public async Task LoadActualCostGrid_WithoutAnimalOnly_SetsAdditionalTitle()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _projectSubContractService.GetFpsProjectSubContractsAsync(
                    Arg.Any<QueryParameters<string>>(), "AH0001", filterByAnimalAcctCodes: false)
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(
                    new List<ProjectSubContractDto>(),
                    new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }));
            _mapper.Map<List<ActualProjectCostItem>>(Arg.Any<List<ProjectSubContractDto>>())
                .Returns(new List<ActualProjectCostItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var request = new PaginationFilter<string>();
            var result = await _controller.LoadActualCostGrid(request, parentProject: "AH0001", animalOnly: false);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialResult.Model);
            Assert.Equal("Actual Additional Costs (PACT)", gridConfig.Title);
        }

        #endregion

        #region Edge Case / Exception Tests

        [Fact]
        public async Task GetPlanSummaryTotals_WhenServiceThrows_PropagatesException()
        {
            _staffJobService.GetTotalStaffCostAsync("AH0001").ThrowsAsync(new Exception("Service error"));
            _animalPlanService.GetTotalAnimalCostAsync("AH0001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));
            _additionalCostService.GetTotalItemCostAsync("AH0001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPlanSummaryTotals("AH0001"));
        }

        [Fact]
        public async Task GetStaffPlanVsActualTotals_WhenServiceThrows_PropagatesException()
        {
            _staffJobService.GetTotalStaffCostAsync("AH0001").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetStaffPlanVsActualTotals("AH0001"));
        }

        [Fact]
        public async Task GetTestPlanVsActualTotals_WhenServiceThrows_PropagatesException()
        {
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetTestPlanVsActualTotals("AH0001"));
        }

        [Fact]
        public async Task GetAnimalPlanVsActualTotals_WhenServiceThrows_PropagatesException()
        {
            _animalPlanService.GetTotalAnimalCostAsync("AH0001").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalPlanVsActualTotals("AH0001"));
        }

        [Fact]
        public async Task GetAdditionalPlanVsActualTotals_WhenServiceThrows_PropagatesException()
        {
            _additionalCostService.GetTotalItemCostAsync("AH0001").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAdditionalPlanVsActualTotals("AH0001"));
        }

        [Fact]
        public async Task LoadStaffPlanGrid_WhenServiceThrows_PropagatesException()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _staffJobService.GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .ThrowsAsync(new Exception("Service error"));

            var request = new PaginationFilter<string>();
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadStaffPlanGrid(request, parentProject: "AH0001"));
        }

        [Fact]
        public async Task LoadTestPlanGrid_WhenServiceThrows_PropagatesException()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .ThrowsAsync(new Exception("Service error"));

            var request = new PaginationFilter<string>();
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadTestPlanGrid(request, parentProject: "AH0001"));
        }

        [Fact]
        public async Task LoadAnimalPlanGrid_WhenServiceThrows_PropagatesException()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _animalPlanService.GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .ThrowsAsync(new Exception("Service error"));

            var request = new PaginationFilter<string>();
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadAnimalPlanGrid(request, parentProject: "AH0001"));
        }

        [Fact]
        public async Task LoadAdditionalCostGrid_WhenServiceThrows_PropagatesException()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _additionalCostService.GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .ThrowsAsync(new Exception("Service error"));

            var request = new PaginationFilter<string>();
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadAdditionalCostGrid(request, parentProject: "AH0001"));
        }

        [Fact]
        public async Task LoadStaffActualGrid_WhenServiceThrows_PropagatesException()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _timeCostCalcsService.GetTimeCostCalcsByProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .ThrowsAsync(new Exception("Service error"));

            var request = new PaginationFilter<string>();
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadStaffActualGrid(request, parentProject: "AH0001"));
        }

        [Fact]
        public async Task LoadTestActualGrid_WhenServiceThrows_PropagatesException()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .ThrowsAsync(new Exception("Service error"));

            var request = new PaginationFilter<string>();
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadTestActualGrid(request, parentProject: "AH0001"));
        }

        [Fact]
        public async Task LoadActualCostGrid_WhenServiceThrows_PropagatesException()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _projectSubContractService.GetFpsProjectSubContractsAsync(
                    Arg.Any<QueryParameters<string>>(), "AH0001", Arg.Any<bool>())
                .ThrowsAsync(new Exception("Service error"));

            var request = new PaginationFilter<string>();
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadActualCostGrid(request, parentProject: "AH0001"));
        }

        #endregion

        #region Empty ParentProject Returns Empty Grid Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadStaffPlanGrid_WithEmptyParentProject_ReturnsEmptyGridAndDoesNotCallService(string? parentProject)
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadStaffPlanGrid(request, parentProject: parentProject);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobItemViewModel>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            await _staffJobService.DidNotReceive().GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadTestPlanGrid_WithEmptyParentProject_ReturnsEmptyGridAndDoesNotCallService(string? parentProject)
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadTestPlanGrid(request, parentProject: parentProject);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<TestPlanActualItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            await _testRequirementService.DidNotReceive().GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadAnimalPlanGrid_WithEmptyParentProject_ReturnsEmptyGridAndDoesNotCallService(string? parentProject)
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadAnimalPlanGrid(request, parentProject: parentProject);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalPlanItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            await _animalPlanService.DidNotReceive().GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadAdditionalCostGrid_WithEmptyParentProject_ReturnsEmptyGridAndDoesNotCallService(string? parentProject)
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadAdditionalCostGrid(request, parentProject: parentProject);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<AdditionalCostItemViewModel>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            await _additionalCostService.DidNotReceive().GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadStaffActualGrid_WithEmptyParentProject_ReturnsEmptyGridAndDoesNotCallService(string? parentProject)
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadStaffActualGrid(request, parentProject: parentProject);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<CompareStaff2Item>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            await _timeCostCalcsService.DidNotReceive().GetTimeCostCalcsByProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadTestActualGrid_WithEmptyParentProject_ReturnsEmptyGridAndDoesNotCallService(string? parentProject)
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadTestActualGrid(request, parentProject: parentProject);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ActualTestOutputItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            await _testRequirementService.DidNotReceive().GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
            await _monthlyOutputService.DidNotReceive().GetMonthlyOutputByProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<Dictionary<(string, string), decimal>>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadActualCostGrid_WithEmptyParentProject_ReturnsEmptyGridAndDoesNotCallService(string? parentProject)
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadActualCostGrid(request, parentProject: parentProject);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            await _projectSubContractService.DidNotReceive().GetFpsProjectSubContractsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<bool>());
        }

        [Fact]
        public async Task LoadStaffPlanGrid_WithEmptyParentProject_UsesProvidedGridId()
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadStaffPlanGrid(request, parentProject: null, gridId: "staffPlanGrid");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobItemViewModel>>(partialResult.Model);
            Assert.Equal("staffPlanGrid", gridConfig.GridId);
        }

        [Fact]
        public async Task LoadTestPlanGrid_WithEmptyParentProject_UsesProvidedGridId()
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadTestPlanGrid(request, parentProject: null, gridId: "testPlanGrid");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<TestPlanActualItem>>(partialResult.Model);
            Assert.Equal("testPlanGrid", gridConfig.GridId);
        }

        [Fact]
        public async Task LoadAnimalPlanGrid_WithEmptyParentProject_UsesProvidedGridId()
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadAnimalPlanGrid(request, parentProject: null, gridId: "animalPlanGrid");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalPlanItem>>(partialResult.Model);
            Assert.Equal("animalPlanGrid", gridConfig.GridId);
        }

        [Fact]
        public async Task LoadAdditionalCostGrid_WithEmptyParentProject_UsesProvidedGridId()
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadAdditionalCostGrid(request, parentProject: null, gridId: "additionalCostPlanGrid");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AdditionalCostItemViewModel>>(partialResult.Model);
            Assert.Equal("additionalCostPlanGrid", gridConfig.GridId);
        }

        [Fact]
        public async Task LoadActualCostGrid_WithEmptyParentProject_UsesProvidedGridId()
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadActualCostGrid(request, parentProject: null, gridId: "actualAdditionalCostGrid");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialResult.Model);
            Assert.Equal("actualAdditionalCostGrid", gridConfig.GridId);
        }

        [Fact]
        public async Task LoadActualCostGrid_WithEmptyParentProject_AnimalOnlyTrue_SetsAnimalTitle()
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadActualCostGrid(request, parentProject: null, animalOnly: true);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialResult.Model);
            Assert.Equal("Actual Animal Costs (PACT)", gridConfig.Title);
        }

        [Fact]
        public async Task LoadActualCostGrid_WithEmptyParentProject_AnimalOnlyFalse_SetsAdditionalTitle()
        {
            var request = new PaginationFilter<string>();
            var result = await _controller.LoadActualCostGrid(request, parentProject: null, animalOnly: false);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ActualProjectCostItem>>(partialResult.Model);
            Assert.Equal("Actual Additional Costs (PACT)", gridConfig.Title);
        }

        #endregion

        #region ProjectDetails Partial ViewModel Population Tests

        [Fact]
        public async Task Index_WithProjectCode_PopulatesProjectDetailsProgramFromDto()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal("P01", model.ProjectDetails.Program);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesProjectDetailsFullFields()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            var details = model.ProjectDetails.ProjectDetails;
            Assert.Equal("AH0001", details.ProjectCode);
            Assert.Equal("Test Project", details.Description);
            Assert.Equal("TP", details.ShortTitle);
            Assert.Equal("DEFRA", details.Customer);
            Assert.Equal("John", details.Manager);
            Assert.Equal("TB", details.Disease);
            Assert.Equal("Active", details.ProjectStatus);
            Assert.Equal("CB01", details.CostBookNo);
            Assert.Equal("C001", details.Contract);
            Assert.Equal(1, details.IsDefraProject);
            Assert.Equal("RC1", details.OwningRc);
            Assert.Equal("G1", details.ProjectGroup);
            Assert.Equal("IAC01", details.IncomeAccountCode);
            Assert.Equal("SAC01", details.SubAccountCode);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesProjectDetailsCurrencyFields()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            var details = model.ProjectDetails.ProjectDetails;
            Assert.Equal(1000m, details.CustIncome);
            Assert.Equal(500m, details.TransferIncome);
            Assert.Equal(100m, details.TargetProfit);
            Assert.Equal(5000m, details.BudgetCvl);
            Assert.Equal(200m, details.PvsIncome);
            Assert.Equal(50m, details.PlanCaseWorkDebit);
            Assert.Equal(300m, details.CarryOver);
            Assert.Equal(10m, details.CarryOverSeed);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesCommentsField()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal("Test comments", model.ProjectDetails.ProjectDetails.Comments);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesTotalStaffPlanCost()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(1000m, model.ProjectDetails.TotalStaffPlanCost);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesTotalAnimalPlanCost()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(500m, model.ProjectDetails.TotalAnimalPlanCost);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesTotalAdditionalPlanCost()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(300m, model.ProjectDetails.TotalAdditionalPlanCost);
        }

        [Fact]
        public async Task Index_WithProjectCode_CalculatesTotalTestPlanCost()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            // BuildTestRequirementDto: UnitPrice = 25, NoRequired = 10 => 250
            Assert.Equal(250m, model.ProjectDetails.TotalTestPlanCost);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesStaffPlanVsActualTotals()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(1000m, model.ProjectDetails.StaffTotalPlannedCost);
            Assert.Equal(50, model.ProjectDetails.StaffTotalActualHrs);
            Assert.Equal(800, model.ProjectDetails.StaffTotalActualCost);
            // 800 / 1000 * 100 = 80
            Assert.Equal(80, model.ProjectDetails.StaffPercentOfPlan);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesTestPlanVsActualTotals()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            // planned = 25 * 10 = 250
            Assert.Equal(250m, model.ProjectDetails.TestTotalPlannedCost);
            // actual = 200
            Assert.Equal(200.0, model.ProjectDetails.TestTotalActualCost);
            // 200 / 250 * 100 = 80
            Assert.Equal(80, model.ProjectDetails.TestPercentOfPlan);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesAnimalPlanVsActualTotals()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(500m, model.ProjectDetails.AnimalTotalPlannedCost);
            Assert.Equal(400m, model.ProjectDetails.AnimalTotalActualCost);
            // 400 / 500 * 100 = 80
            Assert.Equal(80, model.ProjectDetails.AnimalPercentOfPlan);
        }

        [Fact]
        public async Task Index_WithProjectCode_PopulatesAdditionalPlanVsActualTotals()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(300m, model.ProjectDetails.AdditionalTotalPlannedCost);
            Assert.Equal(250m, model.ProjectDetails.AdditionalTotalActualCost);
            // 250 / 300 * 100 = 83.33...
            Assert.True(model.ProjectDetails.AdditionalPercentOfPlan > 83 && model.ProjectDetails.AdditionalPercentOfPlan < 84);
        }

        [Fact]
        public async Task Index_WithProjectCode_WhenStaffActualIsNull_DefaultsToZero()
        {
            SetupProjectDetailsServices("AH0001");
            _timeCostCalcsService.GetTotalActualByProjectAsync("AH0001")
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(null!));

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(0, model.ProjectDetails.StaffTotalActualHrs);
            Assert.Equal(0, model.ProjectDetails.StaffTotalActualCost);
            Assert.Equal(0, model.ProjectDetails.StaffPercentOfPlan);
        }

        [Fact]
        public async Task Index_WithProjectCode_WhenZeroStaffPlannedCost_PercentIsZero()
        {
            SetupProjectDetailsServices("AH0001");
            _staffJobService.GetTotalStaffCostAsync("AH0001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(0, model.ProjectDetails.StaffPercentOfPlan);
        }

        [Fact]
        public async Task Index_WithProjectCode_WhenTestResultFails_TestTotalsAreZero()
        {
            SetupProjectDetailsServices("AH0001");
            var failedResponse = ApiResponseDto<List<TestRequirementDto>>.FailureResponse(
                new List<ApiErrorDto>(), new ApiMetaDto());
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "AH0001")
                .Returns(failedResponse);

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(0m, model.ProjectDetails.TotalTestPlanCost);
            Assert.Equal(0m, model.ProjectDetails.TestTotalPlannedCost);
        }

        [Fact]
        public async Task Index_WithProjectCode_WhenZeroAnimalPlannedCost_PercentIsZero()
        {
            SetupProjectDetailsServices("AH0001");
            _animalPlanService.GetTotalAnimalCostAsync("AH0001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(0, model.ProjectDetails.AnimalPercentOfPlan);
        }

        [Fact]
        public async Task Index_WithProjectCode_WhenZeroAdditionalPlannedCost_PercentIsZero()
        {
            SetupProjectDetailsServices("AH0001");
            _additionalCostService.GetTotalItemCostAsync("AH0001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(0, model.ProjectDetails.AdditionalPercentOfPlan);
        }

        [Fact]
        public async Task Index_WithNoProjectCode_ProjectDetailsHasEmptySelectedCode()
        {
            SetupDefaultServices();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ProjectDetails.SelectedProjectCode);
        }

        [Fact]
        public async Task Index_WithNoProjectCode_ProjectDetailsTotalsAreZero()
        {
            SetupDefaultServices();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(0m, model.ProjectDetails.TotalStaffPlanCost);
            Assert.Equal(0m, model.ProjectDetails.TotalTestPlanCost);
            Assert.Equal(0m, model.ProjectDetails.TotalAnimalPlanCost);
            Assert.Equal(0m, model.ProjectDetails.TotalAdditionalPlanCost);
        }

        [Fact]
        public async Task Index_WithProjectCode_ProjectDetailsGridConfigsAreInitialized()
        {
            SetupProjectDetailsServices("AH0001");

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal("planSummaryStaffGrid", model.ProjectDetails.PlanSummaryStaffGrid.GridId);
            Assert.Equal("planSummaryTestGrid", model.ProjectDetails.PlanSummaryTestGrid.GridId);
            Assert.Equal("planSummaryAnimalGrid", model.ProjectDetails.PlanSummaryAnimalGrid.GridId);
            Assert.Equal("planSummaryAdditionalGrid", model.ProjectDetails.PlanSummaryAdditionalGrid.GridId);
            Assert.Equal("staffPlanGrid", model.ProjectDetails.StaffPlanGrid.GridId);
            Assert.Equal("testPlanGrid", model.ProjectDetails.TestPlanGrid.GridId);
            Assert.Equal("animalPlanGrid", model.ProjectDetails.AnimalPlanGrid.GridId);
            Assert.Equal("additionalCostPlanGrid", model.ProjectDetails.AdditionalPlanGrid.GridId);
            Assert.Equal("actualAnimalCostGrid", model.ProjectDetails.AnimalActualGrid.GridId);
            Assert.Equal("actualAdditionalCostGrid", model.ProjectDetails.AdditionalActualGrid.GridId);
        }

        [Fact]
        public async Task Index_WithProjectCode_WhenProjectNotFound_DoesNotPopulateDetails()
        {
            SetupDefaultServices();
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            _projectService.GetProjectByIdAsync("AH0001")
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            var result = await _controller.Index(projectCode: "AH0001");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanViewerViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ProjectDetails.Program);
            Assert.Equal(0m, model.ProjectDetails.TotalStaffPlanCost);
        }

        [Fact]
        public async Task Index_WithProjectCode_SkipsSessionLookup()
        {
            SetupProjectDetailsServices("AH0001");

            await _controller.Index(projectCode: "AH0001");

            await _appStateService.DidNotReceive().GetSessionAsync<string>(Arg.Any<string>());
        }

        #endregion
    }
}

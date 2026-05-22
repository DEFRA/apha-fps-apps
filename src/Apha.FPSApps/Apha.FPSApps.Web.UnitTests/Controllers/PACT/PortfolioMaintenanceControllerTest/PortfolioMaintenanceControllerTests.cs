using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.PortfolioMaintenanceControllerTest
{
    public class PortfolioMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly ITestorProductService _testorProductService;
        private readonly IPactTimeCodeValidService _timeCodeService;
        private readonly IProgramService _programService;
        private readonly IEmployeeService _employeeService;
        private readonly PortfolioMaintenanceController _controller;

        public PortfolioMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _testCapabilityService = Substitute.For<ITestCapabilityService>();
            _testorProductService = Substitute.For<ITestorProductService>();
            _timeCodeService = Substitute.For<IPactTimeCodeValidService>();
            _programService = Substitute.For<IProgramService>();
            _employeeService = Substitute.For<IEmployeeService>();

            _controller = new PortfolioMaintenanceController(
                _mapper,
                _projectService,
                _testCapabilityService,
                _testorProductService,
                _timeCodeService,
                _programService,
                _employeeService);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Substitute.For<ITempDataProvider>());
        }

        private static JsonElement GetJsonElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupIndexDefaults()
        {
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                [
                    new ProjectDto { ParentProject = "PP1", ProjectTitle = "Portfolio One" }
                ]));
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(
                [
                    new ProgramDto { ProgramNo = "P001", ProgramName = "Programme A" }
                ]));
            _employeeService.GetAllPactManagersAsync()
                .Returns(ApiResponseDto<List<ManagerDto>>.SuccessResponse(
                [
                    new ManagerDto { Name = "Jane Smith" }
                ]));
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                [
                    new WorkGroupDto { WorkGroupName = "WG1" }
                ]));
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                [
                    new TestorProductDto { ItemCode = "TC001" }
                ]));

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ConstituentTestItem>>(Arg.Any<List<TestCapabilityDto>>()).Returns([]);
            _mapper.Map<List<PortfolioTimeCodeViewModel>>(Arg.Any<List<TimeCodeValidDto>>()).Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
        }

        // ── INDEX ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_WithData_ReturnsViewWithPopulatedDropdowns()
        {
            SetupIndexDefaults();

            var result = await _controller.Index(null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PortfolioMaintenanceViewModel>(viewResult.Model);
            Assert.Single(model.PortfolioOptions);
            Assert.Single(model.Programs);
            Assert.Single(model.Managers);
            Assert.Single(model.WorkGroups);
            Assert.Single(model.TestorProducts);
        }

        [Fact]
        public async Task Index_WhenServicesReturnNullData_ReturnsViewWithEmptyCollections()
        {
            _projectService.GetAllPactProjectsAsync()
                .Returns(new ApiResponseDto<List<ProjectDto>> { Success = true });
            _programService.GetAllProgramsAsync()
                .Returns(new ApiResponseDto<IEnumerable<ProgramDto>> { Success = true });
            _employeeService.GetAllPactManagersAsync()
                .Returns(new ApiResponseDto<List<ManagerDto>> { Success = true });
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(new ApiResponseDto<List<WorkGroupDto>> { Success = true });
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(new ApiResponseDto<List<TestorProductDto>> { Success = true });
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = await _controller.Index(null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PortfolioMaintenanceViewModel>(viewResult.Model);
            Assert.Empty(model.PortfolioOptions);
            Assert.Empty(model.Programs);
        }

        [Fact]
        public async Task Index_WithPortfolioParameter_SetsViewBagSelectedPortfolio()
        {
            SetupIndexDefaults();

            var result = await _controller.Index("PP1", null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("PP1", _controller.ViewBag.SelectedPortfolio);
        }

        [Fact]
        public async Task Index_WithWorkGroupParameter_SetsViewBagSourceWorkGroup()
        {
            SetupIndexDefaults();

            var result = await _controller.Index(null, "WG1");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("WG1", _controller.ViewBag.SourceWorkGroup);
        }

        [Fact]
        public async Task Index_WithBothParameters_SetsBothViewBagValues()
        {
            SetupIndexDefaults();

            var result = await _controller.Index("PP1", "WG1");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("PP1", _controller.ViewBag.SelectedPortfolio);
            Assert.Equal("WG1", _controller.ViewBag.SourceWorkGroup);
        }

        [Fact]
        public async Task Index_WithNullParameters_SetsViewBagValuesToNull()
        {
            SetupIndexDefaults();

            var result = await _controller.Index(null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(_controller.ViewBag.SelectedPortfolio);
            Assert.Null(_controller.ViewBag.SourceWorkGroup);
        }

        // ── LOAD CONSTITUENT TEST GRID ─────────────────────────────────────────

        [Fact]
        public async Task LoadConstituentTestGrid_WithNullParentProject_ReturnsBadRequest()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadConstituentTestGrid(request, null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadConstituentTestGrid_WithEmptyParentProject_ReturnsBadRequest()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadConstituentTestGrid(request, string.Empty);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadConstituentTestGrid_WithValidProject_ReturnsPartialViewWithGrid()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };
            var query = new QueryParameters<string>();
            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _testCapabilityService.GetPagedTestCapabilityByPortfolioAsync(query, "PP1")
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(
                    [new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" }],
                    new PaginationDto()));
            _mapper.Map<List<ConstituentTestItem>>(Arg.Any<List<TestCapabilityDto>>())
                .Returns([new ConstituentTestItem { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" }]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = await _controller.LoadConstituentTestGrid(request, "PP1");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        // ── GET PORTFOLIO ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetPortfolio_WhenFound_ReturnsSuccessJson()
        {
            var dto = new ProjectDto { ParentProject = "PP1", ProjectTitle = "Portfolio One" };
            _projectService.GetProjectByIdAsync("PP1")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(dto));
            _mapper.Map<PactProjectViewModel>(dto)
                .Returns(new PactProjectViewModel { ParentProject = "PP1", ProjectTitle = "Portfolio One" });

            var result = await _controller.GetPortfolio("PP1");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetPortfolio_WhenNotFound_ReturnsFailureJson()
        {
            _projectService.GetProjectByIdAsync("MISSING")
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Not found" }], new ApiMetaDto()));

            var result = await _controller.GetPortfolio("MISSING");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetPortfolio_WhenSuccessButNullData_ReturnsFailureJson()
        {
            _projectService.GetProjectByIdAsync("PP1")
                .Returns(new ApiResponseDto<ProjectDto> { Success = true });

            var result = await _controller.GetPortfolio("PP1");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        // ── EDIT PORTFOLIO ────────────────────────────────────────────────────

        [Fact]
        public async Task Edit_WithValidModel_ReturnsSuccessJson()
        {
            var model = new PortfolioDetailModel
            {
                ParentProject = "PP1",
                ProjectTitle = "Updated Title",
                Program = "P001",
                Finished = false,
                TransferIncome = 100m
            };
            _projectService.UpdatePactPortfolioAsync(Arg.Any<ProjectDto>())
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto { ParentProject = "PP1" }));

            var result = await _controller.Edit(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_WhenUpdateFails_ReturnsFailureJson()
        {
            var model = new PortfolioDetailModel
            {
                ParentProject = "PP1",
                ProjectTitle = "Title",
                Program = "P001",
                TransferIncome = 0m
            };
            _projectService.UpdatePactPortfolioAsync(Arg.Any<ProjectDto>())
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "Update failed" }], new ApiMetaDto()));

            var result = await _controller.Edit(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_WithFinishedTrue_MapsToMinusOne()
        {
            var model = new PortfolioDetailModel
            {
                ParentProject = "PP1",
                ProjectTitle = "Title",
                Program = "P001",
                Finished = true,
                TransferIncome = 0m
            };
            ProjectDto? captured = null;
            _projectService.UpdatePactPortfolioAsync(Arg.Do<ProjectDto>(dto => captured = dto))
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto { ParentProject = "PP1" }));

            await _controller.Edit(model);

            Assert.NotNull(captured);
            Assert.Equal((short)-1, captured!.Finished);
        }

        // ── CREATE CONSTITUENT TEST (GET) ─────────────────────────────────────

        [Fact]
        public async Task CreateConstituentTest_Get_ReturnsPartialViewWithModel()
        {
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                    [new WorkGroupDto { WorkGroupName = "WG1" }]));
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                    [new TestorProductDto { ItemCode = "TC001" }]));

            var result = await _controller.CreateConstituentTest("PP1");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddConstituentTest", partialResult.ViewName);
            var model = Assert.IsType<ConstituentTestItem>(partialResult.Model);
            Assert.Equal("PP1", model.PlanPortfolio);
        }

        // ── CREATE CONSTITUENT TEST (POST) ────────────────────────────────────

        [Fact]
        public async Task CreateConstituentTest_Post_WithValidModel_ReturnsSuccessJson()
        {
            var model = new ConstituentTestItem { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            _mapper.Map<TestCapabilityDto>(model).Returns(new TestCapabilityDto { TestCode = "TC1" });
            _testCapabilityService.CreateTestCapabilityAsync(Arg.Any<TestCapabilityDto>())
                .Returns(ApiResponseDto<TestCapabilityDto>.SuccessResponse(new TestCapabilityDto { TestCode = "TC1" }));

            var result = await _controller.CreateConstituentTest(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateConstituentTest_Post_WhenServiceFails_ReturnsFailureJson()
        {
            var model = new ConstituentTestItem { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            _mapper.Map<TestCapabilityDto>(model).Returns(new TestCapabilityDto { TestCode = "TC1" });
            _testCapabilityService.CreateTestCapabilityAsync(Arg.Any<TestCapabilityDto>())
                .Returns(ApiResponseDto<TestCapabilityDto>.FailureResponse(
                    [new ApiErrorDto { Code = "CONFLICT", Message = "Already exists" }], new ApiMetaDto()));

            var result = await _controller.CreateConstituentTest(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        // ── DELETE CONSTITUENT TEST ───────────────────────────────────────────

        [Fact]
        public async Task DeleteConstituentTest_WhenSuccess_ReturnsSuccessJson()
        {
            _testCapabilityService.DeleteTestCapabilityAsync("TC1", "WG1")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            var result = await _controller.DeleteConstituentTest("TC1", "WG1");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteConstituentTest_WhenServiceFails_ReturnsFailureJson()
        {
            _testCapabilityService.DeleteTestCapabilityAsync("TC1", "WG1")
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "Delete failed" }], new ApiMetaDto()));

            var result = await _controller.DeleteConstituentTest("TC1", "WG1");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        // ── LOAD TIME CODE GRID ───────────────────────────────────────────────

        [Fact]
        public async Task LoadTimeCodeGrid_WithNullParentProject_ReturnsBadRequest()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadTimeCodeGrid(request, null!, null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadTimeCodeGrid_WithValidProjectAndTestCode_ReturnsPartialViewWithGrid()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };
            var query = new QueryParameters<string>();
            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _timeCodeService.GetPagedByProjectAndTestCodeAsync(query, "PP1", "TC1")
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                    [new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PP1" }],
                    new PaginationDto()));
            _mapper.Map<List<PortfolioTimeCodeViewModel>>(Arg.Any<List<TimeCodeValidDto>>())
                .Returns([new PortfolioTimeCodeViewModel { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PP1" }]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = await _controller.LoadTimeCodeGrid(request, "PP1", "TC1");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadTimeCodeGrid_WithParentProjectButNullTestCode_ReturnsEmptyGrid()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadTimeCodeGrid(request, "PP1", null);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        // ── CREATE TIME CODE (GET) ────────────────────────────────────────────

        [Fact]
        public async Task CreatePortfolioTimeCode_Get_ReturnsPartialViewWithPrefilledModel()
        {
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));

            var result = await _controller.CreatePortfolioTimeCode("PP1", "TC1", "PORTFOLIO1");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditPortfolioTimeCode", partialResult.ViewName);
            var model = Assert.IsType<PortfolioTimeCodeViewModel>(partialResult.Model);
            Assert.Equal("PP1", model.ParentProject);
            Assert.Equal("TC1", model.TestCode);
            Assert.Equal("PORTFOLIO1", model.Portfolio);
        }

        // ── CREATE TIME CODE (POST) ───────────────────────────────────────────

        [Fact]
        public async Task CreatePortfolioTimeCode_Post_WithValidModel_ReturnsSuccessJson()
        {
            var model = new PortfolioTimeCodeViewModel
            {
                WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1"
            };
            _mapper.Map<TimeCodeValidDto>(model).Returns(new TimeCodeValidDto { TimeCode = "TC1" });
            _timeCodeService.CreateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>())
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(new TimeCodeValidDto { TimeCode = "TC1" }));

            var result = await _controller.CreatePortfolioTimeCode(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreatePortfolioTimeCode_Post_WhenServiceFails_ReturnsFailureJson()
        {
            var model = new PortfolioTimeCodeViewModel
            {
                WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1"
            };
            _mapper.Map<TimeCodeValidDto>(model).Returns(new TimeCodeValidDto { TimeCode = "TC1" });
            _timeCodeService.CreateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>())
                .Returns(ApiResponseDto<TimeCodeValidDto>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "Duplicate entry" }], new ApiMetaDto()));

            var result = await _controller.CreatePortfolioTimeCode(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        // ── EDIT TIME CODE (GET) ──────────────────────────────────────────────

        [Fact]
        public async Task EditPortfolioTimeCode_Get_WhenFound_ReturnsPartialViewWithModel()
        {
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));
            _timeCodeService.GetTimeCodeValidAsync("WG1", "TC1", "PP1")
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(
                    new TimeCodeValidDto { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }));

            var result = await _controller.EditPortfolioTimeCode("WG1", "TC1", "PP1", "TST1");

            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditPortfolioTimeCode", partialResult.ViewName);
            var model = Assert.IsType<PortfolioTimeCodeViewModel>(partialResult.Model);
            Assert.True(model.IsEdit);
            Assert.Equal("TST1", model.TestCode);
        }

        [Fact]
        public async Task EditPortfolioTimeCode_Get_WhenNotFound_ReturnsPartialViewWithDefaultModel()
        {
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));
            _timeCodeService.GetTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PP1")
                .Returns(ApiResponseDto<TimeCodeValidDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Not found" }], new ApiMetaDto()));

            var result = await _controller.EditPortfolioTimeCode("WG_MISSING", "TC_MISSING", "PP1", null);

            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<PortfolioTimeCodeViewModel>(partialResult.Model);
            Assert.True(model.IsEdit);
            Assert.False(model.Active);
        }

        // ── EDIT TIME CODE (POST) ─────────────────────────────────────────────

        [Fact]
        public async Task EditPortfolioTimeCode_Post_WithValidModel_ReturnsSuccessJson()
        {
            var model = new PortfolioTimeCodeViewModel
            {
                WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", IsEdit = true
            };
            _mapper.Map<TimeCodeValidDto>(model).Returns(new TimeCodeValidDto { TimeCode = "TC1" });
            _timeCodeService.UpdateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>())
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(new TimeCodeValidDto { TimeCode = "TC1" }));

            var result = await _controller.EditPortfolioTimeCode(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditPortfolioTimeCode_Post_WhenServiceFails_ReturnsFailureJson()
        {
            var model = new PortfolioTimeCodeViewModel
            {
                WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", IsEdit = true
            };
            _mapper.Map<TimeCodeValidDto>(model).Returns(new TimeCodeValidDto { TimeCode = "TC1" });
            _timeCodeService.UpdateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>())
                .Returns(ApiResponseDto<TimeCodeValidDto>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "Update failed" }], new ApiMetaDto()));

            var result = await _controller.EditPortfolioTimeCode(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        // ── DELETE TIME CODE ──────────────────────────────────────────────────

        [Fact]
        public async Task DeletePortfolioTimeCode_WhenSuccess_ReturnsSuccessJson()
        {
            _timeCodeService.DeleteTimeCodeValidAsync("WG1", "TC1", "PP1")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            var result = await _controller.DeletePortfolioTimeCode("WG1", "TC1", "PP1");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeletePortfolioTimeCode_WhenServiceFails_ReturnsFailureJson()
        {
            _timeCodeService.DeleteTimeCodeValidAsync("WG1", "TC1", "PP1")
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Code = "ERR", Message = "Delete failed" }], new ApiMetaDto()));

            var result = await _controller.DeletePortfolioTimeCode("WG1", "TC1", "PP1");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }
    }
}

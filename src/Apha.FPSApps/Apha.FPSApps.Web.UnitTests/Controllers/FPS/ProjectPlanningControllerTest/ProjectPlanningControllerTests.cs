using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Security.Claims;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectPlanningControllerTest
{
    public class ProjectPlanningControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IStaffJobService _staffJobService;
        private readonly IAnimalPlanService _animalPlanService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly IAdditionalCostService _additionalCostService;
        private readonly IProjectService _projectService;
        private readonly IExcelExportService _excelExportService;
        private readonly ProjectPlanningController _controller;
        private readonly ITempDataDictionary _tempData;

        public ProjectPlanningControllerTests()
        {
            _mapper               = Substitute.For<IMapper>();
            _staffJobService      = Substitute.For<IStaffJobService>();
            _animalPlanService    = Substitute.For<IAnimalPlanService>();
            _testRequirementService = Substitute.For<ITestRequirementService>();
            _additionalCostService  = Substitute.For<IAdditionalCostService>();
            _projectService       = Substitute.For<IProjectService>();
            _excelExportService   = Substitute.For<IExcelExportService>();

            _controller = new ProjectPlanningController(
                _mapper,
                _staffJobService,
                _animalPlanService,
                _testRequirementService,
                _additionalCostService,
                _projectService,
                _excelExportService);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "TestUser") }, "TestAuth"));

            _tempData = Substitute.For<ITempDataDictionary>();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            _controller.TempData = _tempData;
        }

        /// <summary>
        /// Configures all four grid services to return empty success responses so that
        /// tests focused on the Index action's project-lookup logic do not fail on grid helpers.
        /// </summary>
        private void SetupGridServiceDefaults()
        {
            var emptyStaffResponse = ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(
                new List<StaffJobViewDto>(), new PaginationDto());
            _staffJobService
                .GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(emptyStaffResponse);

            var emptyAnimalResponse = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                new List<AnimalCostViewDto>(), new PaginationDto());
            _animalPlanService
                .GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(emptyAnimalResponse);

            var emptyTestResponse = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                new List<TestRequirementDto>(), new PaginationDto());
            _testRequirementService
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(emptyTestResponse);

            var emptyAdditionalResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(
                new List<AdditionalCostDto>(), new PaginationDto());
            _additionalCostService
                .GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(emptyAdditionalResponse);

            _mapper.Map<List<StaffJobItemViewModel>>(Arg.Any<List<StaffJobViewDto>>())
                   .Returns(new List<StaffJobItemViewModel>());
            _mapper.Map<List<AnimalPlanItem>>(Arg.Any<List<AnimalCostViewDto>>())
                   .Returns(new List<AnimalPlanItem>());
            _mapper.Map<List<TestPlanItem>>(Arg.Any<List<TestRequirementDto>>())
                   .Returns(new List<TestPlanItem>());
            _mapper.Map<List<AdditionalCostItemViewModel>>(Arg.Any<List<AdditionalCostDto>>())
                   .Returns(new List<AdditionalCostItemViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                   .Returns(new PaginationModel());
        }

        /// <summary>
        /// Configures all four grid services to return empty success responses and
        /// the excel export service to return a dummy byte array.
        /// </summary>
        private void SetupExportServiceDefaults()
        {
            _staffJobService
                .GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(new List<StaffJobViewDto>(), new PaginationDto()));
            _animalPlanService
                .GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(new List<AnimalCostViewDto>(), new PaginationDto()));
            _testRequirementService
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(new List<TestRequirementDto>(), new PaginationDto()));
            _additionalCostService
                .GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>(), new PaginationDto()));

            _mapper.Map<List<StaffJobItemViewModel>>(Arg.Any<List<StaffJobViewDto>>())
                   .Returns(new List<StaffJobItemViewModel>());
            _mapper.Map<List<AnimalPlanItem>>(Arg.Any<List<AnimalCostViewDto>>())
                   .Returns(new List<AnimalPlanItem>());
            _mapper.Map<List<TestPlanItem>>(Arg.Any<List<TestRequirementDto>>())
                   .Returns(new List<TestPlanItem>());
            _mapper.Map<List<AdditionalCostItemViewModel>>(Arg.Any<List<AdditionalCostDto>>())
                   .Returns(new List<AdditionalCostItemViewModel>());

            _excelExportService
                .ExportToExcelMultiSheet(Arg.Any<List<ExcelSheetDefinition>>())
                .Returns(new byte[] { 1, 2, 3 });
        }

        private static ProjectDto BuildProjectDto(string projectCode = "PROJ001") => new()
        {
            ParentProject  = projectCode,
            ProjectTitle   = "Test Project Title",
            Program        = "TestProg",
            BudgetCvl      = 10000m,
            TransferIncome = 500m,
            BudgetExt      = 2000m
        };

        #region Index Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Index_WithNullOrWhitespaceProjectCode_ThrowsInvalidOperationException(string? projectCode)
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Index(projectCode!));
        }

        [Fact]
        public async Task Index_WhenProjectServiceReturnsFailure_SetsTempDataAndRedirectsToHome()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Project not found.", Code = "NOT_FOUND" } };
            var serviceResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _projectService.GetProjectByIdAsync("PROJ001").Returns(serviceResponse);

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home",  redirect.ControllerName);
            _tempData.Received()["ErrorMessage"] = "Project not found.";
        }

        [Fact]
        public async Task Index_WhenProjectServiceReturnsNullData_SetsTempDataAndRedirectsToHome()
        {
            // Arrange
            var serviceResponse = ApiResponseDto<ProjectDto>.SuccessResponse(null!);
            _projectService.GetProjectByIdAsync("PROJ001").Returns(serviceResponse);

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home",  redirect.ControllerName);
        }

        [Fact]
        public async Task Index_WithValidProjectCode_ReturnsViewResult()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_WithValidProjectCode_MapsProjectDtoFieldsToViewModel()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanningViewModel>(viewResult.Model);
            Assert.Equal(projectDto.ParentProject,    model.ProjectCode);
            Assert.Equal(projectDto.ProjectTitle,     model.ProjectDescription);
            Assert.Equal(projectDto.Program,          model.SelectedProgramme);
            Assert.Equal(projectDto.BudgetCvl ?? 0m,  model.BudgetCVL);
            Assert.Equal(projectDto.TransferIncome,   model.TransferIncome);
            Assert.Equal(projectDto.BudgetExt ?? 0m,  model.ExternalIncome);
        }

        [Fact]
        public async Task Index_WithSelectedYear_PopulatesModelSelectedYear()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            var result = await _controller.Index("PROJ001", selectedYear: "2025");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanningViewModel>(viewResult.Model);
            Assert.Equal("2025", model.SelectedYear);
        }

        [Fact]
        public async Task Index_WithNullSelectedYear_ModelSelectedYearIsEmpty()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            var result = await _controller.Index("PROJ001", selectedYear: null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanningViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedYear);
        }

        [Fact]
        public async Task Index_WithValidProjectCode_SetsUserNameFromIdentity()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanningViewModel>(viewResult.Model);
            Assert.Equal("TestUser", model.UserName);
        }

        [Fact]
        public async Task Index_WithValidProjectCode_CallsAllFourGridServices()
        {
            // Arrange
            var projectCode = "PROJ001";
            var projectDto = BuildProjectDto(projectCode);
            _projectService.GetProjectByIdAsync(projectCode)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            await _controller.Index(projectCode);

            // Assert
            await _staffJobService.Received(1)
                .GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), projectCode);
            await _animalPlanService.Received(1)
                .GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), projectCode);
            await _testRequirementService.Received(1)
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode);
            await _additionalCostService.Received(1)
                .GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), projectCode);
        }

        [Fact]
        public async Task Index_WithValidProjectCode_PopulatesAllFourGridConfigs()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanningViewModel>(viewResult.Model);
            Assert.NotNull(model.StaffBookedGrid);
            Assert.NotNull(model.AnimalsBookedGrid);
            Assert.NotNull(model.TestsBookedGrid);
            Assert.NotNull(model.ExceptionalCostsGrid);
        }

        [Fact]
        public async Task Index_WithValidProjectCode_StaffGridHasCorrectConfig()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var model = Assert.IsType<ProjectPlanningViewModel>(
                Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("staffBookedGrid", model.StaffBookedGrid.GridId);
            Assert.Equal("Staff Booked",   model.StaffBookedGrid.Title);
            Assert.Equal("StaffID",        model.StaffBookedGrid.KeyProperty);
        }

        [Fact]
        public async Task Index_WithValidProjectCode_AnimalGridHasCorrectConfig()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var model = Assert.IsType<ProjectPlanningViewModel>(
                Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("animalBookedGrid", model.AnimalsBookedGrid.GridId);
            Assert.Equal("Animals Booked",  model.AnimalsBookedGrid.Title);
            Assert.Equal("IndCounter",       model.AnimalsBookedGrid.KeyProperty);
        }

        [Fact]
        public async Task Index_WithValidProjectCode_TestsGridHasCorrectConfig()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var model = Assert.IsType<ProjectPlanningViewModel>(
                Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("testPlanGrid",  model.TestsBookedGrid.GridId);
            Assert.Equal("Tests Booked",  model.TestsBookedGrid.Title);
            Assert.Equal("TestCode",      model.TestsBookedGrid.KeyProperty);
        }

        [Fact]
        public async Task Index_WithValidProjectCode_ExceptionalCostsGridHasCorrectConfig()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var model = Assert.IsType<ProjectPlanningViewModel>(
                Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("additionalCostGrid", model.ExceptionalCostsGrid.GridId);
            Assert.Equal("Exceptional Costs",  model.ExceptionalCostsGrid.Title);
            Assert.Equal("Description",        model.ExceptionalCostsGrid.KeyProperty);
        }

        [Fact]
        public async Task Index_WhenProjectServiceReturnsFailureWithNoErrors_UsesFallbackMessage()
        {
            // Arrange
            var serviceResponse = ApiResponseDto<ProjectDto>.FailureResponse(
                new List<ApiErrorDto>(), new ApiMetaDto());
            _projectService.GetProjectByIdAsync("PROJ001").Returns(serviceResponse);

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home",  redirect.ControllerName);
            _tempData.Received()["ErrorMessage"] = "Project not found.";
        }

        [Fact]
        public async Task Index_WithValidProjectCode_GridDataIsEmpty_WhenServiceReturnsNullData()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));

            // Staff returns null data
            _staffJobService
                .GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(null!, new PaginationDto()));

            // Other grids use defaults
            _animalPlanService
                .GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(new List<AnimalCostViewDto>(), new PaginationDto()));
            _testRequirementService
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(new List<TestRequirementDto>(), new PaginationDto()));
            _additionalCostService
                .GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>(), new PaginationDto()));

            _mapper.Map<List<AnimalPlanItem>>(Arg.Any<List<AnimalCostViewDto>>()).Returns(new List<AnimalPlanItem>());
            _mapper.Map<List<TestPlanItem>>(Arg.Any<List<TestRequirementDto>>()).Returns(new List<TestPlanItem>());
            _mapper.Map<List<AdditionalCostItemViewModel>>(Arg.Any<List<AdditionalCostDto>>()).Returns(new List<AdditionalCostItemViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index("PROJ001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectPlanningViewModel>(viewResult.Model);
            Assert.Empty(model.StaffBookedGrid.Data);
            _mapper.DidNotReceive().Map<List<StaffJobItemViewModel>>(Arg.Any<List<StaffJobViewDto>>());
        }

        [Fact]
        public async Task Index_WhenProjectFound_DoesNotSetErrorMessageTempData()
        {
            // Arrange
            var projectDto = BuildProjectDto();
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            await _controller.Index("PROJ001");

            // Assert
            _tempData.Received(0)["ErrorMessage"] = Arg.Any<object?>();
        }

        [Fact]
        public async Task Index_WithValidProjectCode_ProjectCodePassedToAllGridHelpers()
        {
            // Arrange
            var projectCode = "XYZ999";
            var projectDto = BuildProjectDto(projectCode);
            _projectService.GetProjectByIdAsync(projectCode)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            SetupGridServiceDefaults();

            // Act
            await _controller.Index(projectCode);

            // Assert
            await _staffJobService.Received(1)
                .GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), projectCode);
            await _animalPlanService.Received(1)
                .GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), projectCode);
            await _testRequirementService.Received(1)
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode);
            await _additionalCostService.Received(1)
                .GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), projectCode);
        }

        [Fact]
        public async Task Index_WhenProjectServiceFails_DoesNotCallGridServices()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            await _controller.Index("PROJ001");

            // Assert
            await _staffJobService.DidNotReceive()
                .GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
            await _animalPlanService.DidNotReceive()
                .GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
            await _testRequirementService.DidNotReceive()
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
            await _additionalCostService.DidNotReceive()
                .GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Index_WithValidProjectCode_GridsReturnDataFromService()
        {
            // Arrange
            var projectCode = "PROJ001";
            var projectDto = BuildProjectDto(projectCode);
            _projectService.GetProjectByIdAsync(projectCode)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));

            var staffJobs   = new List<StaffJobViewDto>   { new() { StaffID = "S1", JobCode = projectCode } };
            var animalCosts = new List<AnimalCostViewDto>  { new() { IndCounter = 1, AnimalType = "Cattle" } };
            var testReqs    = new List<TestRequirementDto> { new() { TestCode = "T001" } };
            var addCosts    = new List<AdditionalCostDto>  { new() { JobCode = projectCode, Account = "ACC1" } };

            _staffJobService
                .GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(staffJobs, new PaginationDto { TotalRecords = 1 }));
            _animalPlanService
                .GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(animalCosts, new PaginationDto { TotalRecords = 1 }));
            _testRequirementService
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(testReqs, new PaginationDto { TotalRecords = 1 }));
            _additionalCostService
                .GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), projectCode)
                .Returns(ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(addCosts, new PaginationDto { TotalRecords = 1 }));

            _mapper.Map<List<StaffJobItemViewModel>>(Arg.Any<List<StaffJobViewDto>>())
                   .Returns(new List<StaffJobItemViewModel> { new() { StaffID = "S1" } });
            _mapper.Map<List<AnimalPlanItem>>(Arg.Any<List<AnimalCostViewDto>>())
                   .Returns(new List<AnimalPlanItem> { new() { AnimalType = "Cattle" } });
            _mapper.Map<List<TestPlanItem>>(Arg.Any<List<TestRequirementDto>>())
                   .Returns(new List<TestPlanItem> { new() { TestCode = "T001" } });
            _mapper.Map<List<AdditionalCostItemViewModel>>(Arg.Any<List<AdditionalCostDto>>())
                   .Returns(new List<AdditionalCostItemViewModel> { new() { Description = "Extra" } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                   .Returns(new PaginationModel { TotalRecords = 1 });

            // Act
            var result = await _controller.Index(projectCode);

            // Assert
            var model = Assert.IsType<ProjectPlanningViewModel>(
                Assert.IsType<ViewResult>(result).Model);
            Assert.Single(model.StaffBookedGrid.Data);
            Assert.Single(model.AnimalsBookedGrid.Data);
            Assert.Single(model.TestsBookedGrid.Data);
            Assert.Single(model.ExceptionalCostsGrid.Data);
        }

        #endregion

        #region ExportToExcel Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExportToExcel_WithNullOrWhitespaceProjectCode_ThrowsInvalidOperationException(string? projectCode)
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.ExportToExcel(projectCode!));
        }

        [Fact]
        public async Task ExportToExcel_WithValidProjectCode_ReturnsFileResult()
        {
            // Arrange
            SetupExportServiceDefaults();

            // Act
            var result = await _controller.ExportToExcel("PROJ001");

            // Assert
            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task ExportToExcel_WithValidProjectCode_ReturnsCorrectContentType()
        {
            // Arrange
            SetupExportServiceDefaults();

            // Act
            var result = await _controller.ExportToExcel("PROJ001");

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileResult.ContentType);
        }

        [Fact]
        public async Task ExportToExcel_WithValidProjectCode_FileNameContainsProjectCode()
        {
            // Arrange
            SetupExportServiceDefaults();

            // Act
            var result = await _controller.ExportToExcel("PROJ001");

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Contains("PROJ001", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportToExcel_WithValidProjectCode_FileNameHasXlsxExtension()
        {
            // Arrange
            SetupExportServiceDefaults();

            // Act
            var result = await _controller.ExportToExcel("PROJ001");

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.EndsWith(".xlsx", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportToExcel_WithValidProjectCode_ReturnsBytesFromExportService()
        {
            // Arrange
            var expectedBytes = new byte[] { 10, 20, 30 };
            SetupExportServiceDefaults();
            _excelExportService
                .ExportToExcelMultiSheet(Arg.Any<List<ExcelSheetDefinition>>())
                .Returns(expectedBytes);

            // Act
            var result = await _controller.ExportToExcel("PROJ001");

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(expectedBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportToExcel_WithValidProjectCode_CallsAllFourServices()
        {
            // Arrange
            const string projectCode = "PROJ001";
            SetupExportServiceDefaults();

            // Act
            await _controller.ExportToExcel(projectCode);

            // Assert
            await _staffJobService.Received(1)
                .GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), projectCode);
            await _animalPlanService.Received(1)
                .GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), projectCode);
            await _testRequirementService.Received(1)
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), projectCode);
            await _additionalCostService.Received(1)
                .GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), projectCode);
        }

        [Fact]
        public async Task ExportToExcel_WithValidProjectCode_CallsExportServiceWithFourSheets()
        {
            // Arrange
            SetupExportServiceDefaults();

            // Act
            await _controller.ExportToExcel("PROJ001");

            // Assert
            _excelExportService.Received(1)
                .ExportToExcelMultiSheet(Arg.Is<List<ExcelSheetDefinition>>(
                    sheets => sheets.Count == 4));
        }

        [Fact]
        public async Task ExportToExcel_WithValidProjectCode_SheetsHaveCorrectNames()
        {
            // Arrange
            SetupExportServiceDefaults();

            // Act
            await _controller.ExportToExcel("PROJ001");

            // Assert
            _excelExportService.Received(1)
                .ExportToExcelMultiSheet(Arg.Is<List<ExcelSheetDefinition>>(sheets =>
                    sheets[0].SheetName == "Staff Booked"      &&
                    sheets[1].SheetName == "Animals Booked"    &&
                    sheets[2].SheetName == "Tests Booked"      &&
                    sheets[3].SheetName == "Exceptional Costs"));
        }

        [Fact]
        public async Task ExportToExcel_UsesMaxIntPageSize_ToFetchAllRecords()
        {
            // Arrange
            SetupExportServiceDefaults();

            // Act
            await _controller.ExportToExcel("PROJ001");

            // Assert — all four services must be called with PageSize == int.MaxValue
            await _staffJobService.Received(1)
                .GetAllStaffJobsAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == int.MaxValue),
                    Arg.Any<string>());
            await _animalPlanService.Received(1)
                .GetAllAnimalCostAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == int.MaxValue),
                    Arg.Any<string>());
            await _testRequirementService.Received(1)
                .GetPagedTestReqmtbyProjectAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == int.MaxValue),
                    Arg.Any<string>());
            await _additionalCostService.Received(1)
                .GetAdditionalCostsAsync(
                    Arg.Is<QueryParameters<string>>(q => q.PageSize == int.MaxValue),
                    Arg.Any<string>());
        }

        [Fact]
        public async Task ExportToExcel_WhenServiceReturnsNullData_MapsToEmptyLists()
        {
            // Arrange
            _staffJobService
                .GetAllStaffJobsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(null!, new PaginationDto()));
            _animalPlanService
                .GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(null!, new PaginationDto()));
            _testRequirementService
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(null!, new PaginationDto()));
            _additionalCostService
                .GetAdditionalCostsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(null!, new PaginationDto()));

            _excelExportService
                .ExportToExcelMultiSheet(Arg.Any<List<ExcelSheetDefinition>>())
                .Returns(new byte[] { 1 });

            // Act
            var result = await _controller.ExportToExcel("PROJ001");

            // Assert — export service is still called (with empty sheet data) and a file is returned
            Assert.IsType<FileContentResult>(result);
            _excelExportService.Received(1)
                .ExportToExcelMultiSheet(Arg.Is<List<ExcelSheetDefinition>>(
                    sheets => sheets.All(s => !s.Data.Any())));
        }

        #endregion
    }
}

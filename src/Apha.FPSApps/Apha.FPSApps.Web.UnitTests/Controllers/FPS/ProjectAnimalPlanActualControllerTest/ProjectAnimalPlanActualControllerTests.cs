using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectAnimalPlanActualControllerTest
{
    public class ProjectAnimalPlanActualControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IAnimalPlanService _animalPlanService;
        private readonly IProjectService _projectService;
        private readonly ProjectAnimalPlanActualController _controller;

        public ProjectAnimalPlanActualControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _animalPlanService = Substitute.For<IAnimalPlanService>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ProjectAnimalPlanActualController(_mapper, _animalPlanService, _projectService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        #region Create (GET) Tests

        [Fact]
        public async Task Create_Get_ReturnsPartialViewWithPopulatedAnimalDropdown()
        {
            // Arrange
            var animals = new List<AnimalDto>
            {
                new() { AnimalType = "Cattle", DailyRate = 25m },
                new() { AnimalType = "Sheep",  DailyRate = 15m }
            };
            _animalPlanService.GetAnimalLookupAsync()
                .Returns(ApiResponseDto<List<AnimalDto>>.SuccessResponse(animals));

            // Act
            var result = await _controller.Create();

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAnimalPlan", partialView.ViewName);
            var model = Assert.IsType<AnimalPlanItem>(partialView.Model);
            Assert.Equal(2, model.AnimalTypeList.Count);
        }

        [Fact]
        public async Task Create_Get_WhenAnimalLookupFails_ReturnsEmptyDropdown()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            _animalPlanService.GetAnimalLookupAsync()
                .Returns(ApiResponseDto<List<AnimalDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Create();

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AnimalPlanItem>(partialView.Model);
            Assert.Empty(model.AnimalTypeList);
        }

        #endregion

        #region Edit (GET) Tests

        [Fact]
        public async Task Edit_Get_WithValidId_ReturnsPartialViewWithModel()
        {
            // Arrange
            var indCounter = 1;
            var jobCode = "JOB001";
            var dto = new AnimalCostViewDto { IndCounter = indCounter, JobCode = jobCode, AnimalType = "Cattle" };
            var serviceResponse = ApiResponseDto<AnimalCostViewDto?>.SuccessResponse(dto);
            var model = new AnimalPlanItem { IndCounter = indCounter, JobCode = jobCode, AnimalType = "Cattle" };

            _animalPlanService.GetAnimalCostViewByIdAsync(indCounter, jobCode).Returns(serviceResponse);
            _mapper.Map<AnimalPlanItem>(dto).Returns(model);
            _animalPlanService.GetAnimalLookupAsync()
                .Returns(ApiResponseDto<List<AnimalDto>>.SuccessResponse(new List<AnimalDto>()));

            // Act
            var result = await _controller.Edit(indCounter, jobCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAnimalPlan", partialView.ViewName);
            var resultModel = Assert.IsType<AnimalPlanItem>(partialView.Model);
            Assert.Equal(indCounter, resultModel.IndCounter);
        }

        [Fact]
        public async Task Edit_Get_WhenNotFound_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            _animalPlanService.GetAnimalCostViewByIdAsync(99, string.Empty)
                .Returns(ApiResponseDto<AnimalCostViewDto?>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit(99);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_WithNoProjectCode_SelectsFirstProjectAndReturnsView()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PROJ001" },
                new() { ParentProject = "PROJ002" }
            };
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto { ParentProject = "PROJ001", ProjectTitle = "Test Project", Program = "Prog1", Contract = "C1" }));
            _animalPlanService.GetTotalAnimalCostAsync("PROJ001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(500m));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectAnimalPlanActualViewModel>(viewResult.Model);
            Assert.Equal("PROJ001", model.SelectedProjectCode);
            Assert.Equal("Test Project", model.ProjectTitle);
            Assert.Equal(500m, model.TotalPlannedCost);
            Assert.Equal(2, model.ProjectList.Count);
        }

        [Fact]
        public async Task Index_WithMatchingProjectCode_SelectsSpecifiedProject()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PROJ001" },
                new() { ParentProject = "PROJ002" }
            };
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));
            _projectService.GetProjectByIdAsync("PROJ002")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto { ParentProject = "PROJ002", ProjectTitle = "Project Two", Program = "P2", Contract = "C2" }));
            _animalPlanService.GetTotalAnimalCostAsync("PROJ002")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(200m));

            // Act
            var result = await _controller.Index("PROJ002");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectAnimalPlanActualViewModel>(viewResult.Model);
            Assert.Equal("PROJ002", model.SelectedProjectCode);
            Assert.Equal("Project Two", model.ProjectTitle);
        }

        [Fact]
        public async Task Index_WithNonMatchingProjectCode_FallsBackToFirstProject()
        {
            // Arrange
            var projects = new List<ProjectDto> { new() { ParentProject = "PROJ001" } };
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));
            _projectService.GetProjectByIdAsync("PROJ001")
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(new ProjectDto { ParentProject = "PROJ001", ProjectTitle = "Project One" }));
            _animalPlanService.GetTotalAnimalCostAsync("PROJ001")
                .Returns(ApiResponseDto<decimal>.SuccessResponse(100m));

            // Act
            var result = await _controller.Index("UNKNOWN");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectAnimalPlanActualViewModel>(viewResult.Model);
            Assert.Equal("PROJ001", model.SelectedProjectCode);
        }

        [Fact]
        public async Task Index_WhenProjectListIsEmpty_ReturnsViewWithEmptyState()
        {
            // Arrange
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectAnimalPlanActualViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProjectCode);
            Assert.Empty(model.ProjectList);
            Assert.Equal(0m, model.TotalPlannedCost);
        }

        [Fact]
        public async Task Index_WhenProjectServiceFails_ReturnsViewWithEmptyProjectList()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Service error", Code = "ERR" } };
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectAnimalPlanActualViewModel>(viewResult.Model);
            Assert.Empty(model.ProjectList);
        }

        #endregion

        #region GetTotalPlannedCost Tests

        [Fact]
        public async Task GetTotalPlannedCost_WithValidJobCode_ReturnsTotalCost()
        {
            // Arrange
            var jobCode = "JOB001";
            _animalPlanService.GetTotalAnimalCostAsync(jobCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(750.00m));

            // Act
            var result = await _controller.GetTotalPlannedCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(750.00m, value.GetProperty("totalPlannedCost").GetDecimal());
        }

        [Fact]
        public async Task GetTotalPlannedCost_WithEmptyJobCode_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetTotalPlannedCost(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal(0, value.GetProperty("totalPlannedCost").GetInt32());
            await _animalPlanService.DidNotReceive().GetTotalAnimalCostAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalPlannedCost_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var jobCode = "JOB001";
            var errors = new List<ApiErrorDto> { new() { Message = "Service error", Code = "ERR" } };
            _animalPlanService.GetTotalAnimalCostAsync(jobCode)
                .Returns(ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetTotalPlannedCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Service error", value.GetProperty("message").GetString());
        }

        #endregion
    }
}

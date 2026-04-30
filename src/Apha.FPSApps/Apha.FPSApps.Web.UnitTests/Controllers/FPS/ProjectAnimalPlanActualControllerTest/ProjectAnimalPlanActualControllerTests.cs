using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectAnimalPlanActualControllerTest
{
    public class ProjectAnimalPlanActualControllerTests
    {
        private readonly IAnimalPlanService _animalPlanService;
        private readonly IProjectService _projectService;
        private readonly ProjectAnimalPlanActualController _controller;

        public ProjectAnimalPlanActualControllerTests()
        {
            _animalPlanService = Substitute.For<IAnimalPlanService>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ProjectAnimalPlanActualController(_animalPlanService, _projectService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

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

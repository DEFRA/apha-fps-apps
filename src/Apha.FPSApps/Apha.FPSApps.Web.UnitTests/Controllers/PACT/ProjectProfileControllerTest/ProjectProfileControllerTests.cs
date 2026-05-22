using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.ProjectProfileControllerTest
{
    public class ProjectProfileControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectMonthService _projectMonthService;
        private readonly IProjectProfileService _projectProfileService;
        private readonly IProjectService _projectService;
        private readonly ProjectProfileController _controller;

        public ProjectProfileControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectMonthService = Substitute.For<IProjectMonthService>();
            _projectProfileService = Substitute.For<IProjectProfileService>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ProjectProfileController(
                _mapper,
                _projectMonthService,
                _projectProfileService,
                _projectService);
        }

        private static JsonElement GetJsonElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupProjectsViewBag()
        {
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));
        }

        private void SetupCostProfileGridMapper()
        {
            _mapper.Map<List<ProjectMonthItem>>(Arg.Any<List<ProjectMonthDto>>())
                .Returns([]);
        }

        #region Index

        [Fact]
        public async Task Index_WithParentProject_ReturnsViewWithPopulatedViewModel()
        {
            // Arrange
            const string parentProject = "PRJ1";
            var projectDto = new ProjectDto
            {
                ParentProject = parentProject,
                ProjectTitle  = "Test Project",
                BudgetCvl     = 1000m
            };

            _projectService.GetProjectByIdAsync(parentProject)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.Index(parentProject);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfileViewModel>(viewResult.Model);
            Assert.Equal(parentProject, model.ParentProject);
            Assert.Equal("Test Project", model.ProjectTitle);
            Assert.Equal(1000m, model.BudgetCvl);
        }

        [Fact]
        public async Task Index_WithNullParentProject_ReturnsViewWithEmptyProjectFields()
        {
            // Arrange
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfileViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ParentProject);
            Assert.Equal(string.Empty, model.ProjectTitle);
        }

        [Fact]
        public async Task Index_WithParentProject_ProjectFetchFails_ReturnsViewWithEmptyProjectFields()
        {
            // Arrange
            const string parentProject = "PRJ_NONE";
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };

            _projectService.GetProjectByIdAsync(parentProject)
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.Index(parentProject);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfileViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ProjectTitle);
            Assert.Null(model.BudgetCvl);
        }

        [Fact]
        public async Task Index_Projects_ServiceReturnsProjects_PopulatesProjectsOnViewModel()
        {
            // Arrange
            var projectList = new List<ProjectDto>
            {
                new() { ParentProject = "AAA", ProjectTitle = "Alpha" },
                new() { ParentProject = "BBB", ProjectTitle = "Beta" },
                new() { ParentProject = "CCC", ProjectTitle = "Gamma" }
            };
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projectList));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfileViewModel>(viewResult.Model);
            Assert.Equal(3, model.Projects.Count);
            Assert.Contains(model.Projects, p => p.Value == "AAA" && p.Text == "AAA");
            Assert.Contains(model.Projects, p => p.Value == "BBB" && p.Text == "BBB");
            Assert.Contains(model.Projects, p => p.Value == "CCC" && p.Text == "CCC");
        }

        [Fact]
        public async Task Index_Projects_SelectedProjectIsMarkedSelected()
        {
            // Arrange
            const string parentProject = "BBB";
            var projectList = new List<ProjectDto>
            {
                new() { ParentProject = "AAA", ProjectTitle = "Alpha" },
                new() { ParentProject = "BBB", ProjectTitle = "Beta" },
                new() { ParentProject = "CCC", ProjectTitle = "Gamma" }
            };
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projectList));
            _projectService.GetProjectByIdAsync(parentProject)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectList[1]));

            // Act
            var result = await _controller.Index(parentProject);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfileViewModel>(viewResult.Model);
            var selected = model.Projects.Where(p => p.Selected).ToList();
            Assert.Single(selected);
            Assert.Equal("BBB", selected[0].Value);
            Assert.All(model.Projects.Where(p => p.Value != "BBB"), p => Assert.False(p.Selected));
        }

        [Fact]
        public async Task Index_Projects_ServiceFails_ProjectsIsEmpty()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR", Message = "Service error" } };
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfileViewModel>(viewResult.Model);
            Assert.Empty(model.Projects);
        }

        [Fact]
        public async Task Index_Projects_ReturnedInAlphabeticalOrder()
        {
            // Arrange
            var projectList = new List<ProjectDto>
            {
                new() { ParentProject = "CCC", ProjectTitle = "Gamma" },
                new() { ParentProject = "AAA", ProjectTitle = "Alpha" },
                new() { ParentProject = "BBB", ProjectTitle = "Beta" }
            };
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projectList));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfileViewModel>(viewResult.Model);
            var values = model.Projects.Select(p => p.Value).ToList();
            Assert.Equal(["AAA", "BBB", "CCC"], values);
        }

        #endregion

        #region LoadCostProfileGrid

        [Fact]
        public async Task LoadCostProfileGrid_ValidRequest_ReturnsPartialViewWithGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _projectMonthService.GetProjectMonthByProjectAsync("PRJ1")
                .Returns(ApiResponseDto<List<ProjectMonthDto>>.SuccessResponse([]));
            SetupCostProfileGridMapper();

            // Act
            var result = await _controller.LoadCostProfileGrid(request, "PRJ1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ProjectMonthItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadCostProfileGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "error");

            // Act
            var result = await _controller.LoadCostProfileGrid(new PaginationFilter<string>(), null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region GetProjectDetailsAsync

        [Fact]
        public async Task GetProjectDetailsAsync_ProjectFound_ReturnsJsonWithDetails()
        {
            // Arrange
            const string parentProject = "PRJ1";
            var projectDto = new ProjectDto
            {
                ParentProject = parentProject,
                ProjectTitle = "Test Project",
                BudgetCvl = 5000m
            };
            _projectService.GetProjectByIdAsync(parentProject)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));

            // Act
            var result = await _controller.GetProjectDetailsAsync(parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal("Test Project", element.GetProperty("projectTitle").GetString());
            Assert.Equal(5000m, element.GetProperty("budgetCvl").GetDecimal());
        }

        [Fact]
        public async Task GetProjectDetailsAsync_ProjectNotFound_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _projectService.GetProjectByIdAsync("PRJ_NONE")
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetProjectDetailsAsync("PRJ_NONE");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        #region GetTotalCostProfile

        [Fact]
        public async Task GetTotalCostProfile_ServiceSuccess_ReturnsTotalSum()
        {
            // Arrange
            const string parentProject = "PRJ1";
            var months = new List<ProjectMonthDto>
            {
                new() { MonthNo = 1, CostProfile = 100m },
                new() { MonthNo = 2, CostProfile = 250m }
            };
            _projectMonthService.GetProjectMonthByProjectAsync(parentProject)
                .Returns(ApiResponseDto<List<ProjectMonthDto>>.SuccessResponse(months));

            // Act
            var result = await _controller.GetTotalCostProfile(parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(350m, element.GetProperty("data").GetDecimal());
        }

        [Fact]
        public async Task GetTotalCostProfile_NullCostProfileValues_ReturnsTotalAsZero()
        {
            // Arrange
            var months = new List<ProjectMonthDto>
            {
                new() { MonthNo = 1, CostProfile = null },
                new() { MonthNo = 2, CostProfile = null }
            };
            _projectMonthService.GetProjectMonthByProjectAsync("PRJ1")
                .Returns(ApiResponseDto<List<ProjectMonthDto>>.SuccessResponse(months));

            // Act
            var result = await _controller.GetTotalCostProfile("PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(0m, element.GetProperty("data").GetDecimal());
        }

        [Fact]
        public async Task GetTotalCostProfile_ServiceFails_ReturnsSuccessFalse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR", Message = "Error" } };
            _projectMonthService.GetProjectMonthByProjectAsync("PRJ1")
                .Returns(ApiResponseDto<List<ProjectMonthDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetTotalCostProfile("PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        #region GetProfileData

        [Fact]
        public async Task GetProfileData_ServiceSuccess_ReturnsJsonWithData()
        {
            // Arrange
            var profileData = new List<ProjectProfileDto>
            {
                new() { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new() { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            };
            _projectProfileService.GetProfileDataAsync("PRJ1")
                .Returns(ApiResponseDto<List<ProjectProfileDto>>.SuccessResponse(profileData));

            // Act
            var result = await _controller.GetProfileData("PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            var data = element.GetProperty("data");
            Assert.Equal(2, data.GetArrayLength());
            Assert.Equal(1, data[0].GetProperty("monthNo").GetInt32());
            Assert.Equal(100m, data[0].GetProperty("profile").GetDecimal());
            Assert.Equal(200m, data[0].GetProperty("totalCost").GetDecimal());
        }

        [Fact]
        public async Task GetProfileData_ServiceFails_ReturnsSuccessFalse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR", Message = "Error" } };
            _projectProfileService.GetProfileDataAsync("PRJ1")
                .Returns(ApiResponseDto<List<ProjectProfileDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetProfileData("PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetProfileData_EmptyData_ReturnsSuccessWithEmptyArray()
        {
            // Arrange
            _projectProfileService.GetProfileDataAsync("PRJ1")
                .Returns(ApiResponseDto<List<ProjectProfileDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.GetProfileData("PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(0, element.GetProperty("data").GetArrayLength());
        }

        #endregion

        #region GetCumulativeData

        [Fact]
        public async Task GetCumulativeData_ServiceSuccess_ReturnsJsonWithData()
        {
            // Arrange
            var cumulativeData = new List<ProjectProfileCumulativeDto>
            {
                new() { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new() { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            };
            _projectProfileService.GetCumulativeDataAsync("PRJ1")
                .Returns(ApiResponseDto<List<ProjectProfileCumulativeDto>>.SuccessResponse(cumulativeData));

            // Act
            var result = await _controller.GetCumulativeData("PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            var data = element.GetProperty("data");
            Assert.Equal(2, data.GetArrayLength());
            Assert.Equal(1, data[0].GetProperty("monthNo").GetInt32());
            Assert.Equal(100m, data[0].GetProperty("cumulativeProfile").GetDecimal());
            Assert.Equal(200m, data[0].GetProperty("cumulativeCost").GetDecimal());
        }

        [Fact]
        public async Task GetCumulativeData_ServiceFails_ReturnsSuccessFalse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR", Message = "Error" } };
            _projectProfileService.GetCumulativeDataAsync("PRJ1")
                .Returns(ApiResponseDto<List<ProjectProfileCumulativeDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetCumulativeData("PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetCumulativeData_EmptyData_ReturnsSuccessWithEmptyArray()
        {
            // Arrange
            _projectProfileService.GetCumulativeDataAsync("PRJ1")
                .Returns(ApiResponseDto<List<ProjectProfileCumulativeDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.GetCumulativeData("PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(0, element.GetProperty("data").GetArrayLength());
        }

        #endregion

        #region GetProjectMonth

        [Fact]
        public async Task GetProjectMonth_MonthNoIsZero_ReturnsPartialViewWithNewItem()
        {
            // Arrange & Act
            var result = await _controller.GetProjectMonth("PRJ1", 0);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditProjectMonth", partial.ViewName);
            var model = Assert.IsType<ProjectMonthItem>(partial.Model);
            Assert.Equal("PRJ1", model.Project);
            Assert.Equal(0, model.MonthNo);
        }

        [Fact]
        public async Task GetProjectMonth_MonthNoNonZero_RecordFound_ReturnsPartialViewWithMappedItem()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 3, CostProfile = 250m };
            var viewModel = new ProjectMonthItem { Project = "PRJ1", MonthNo = 3, CostProfile = 250m };

            _projectMonthService.GetProjectMonthAsync("PRJ1", 3)
                .Returns(ApiResponseDto<ProjectMonthDto>.SuccessResponse(dto));
            _mapper.Map<ProjectMonthItem>(dto).Returns(viewModel);

            // Act
            var result = await _controller.GetProjectMonth("PRJ1", 3);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditProjectMonth", partial.ViewName);
            var model = Assert.IsType<ProjectMonthItem>(partial.Model);
            Assert.Equal(3, model.MonthNo);
            Assert.Equal(250m, model.CostProfile);
        }

        [Fact]
        public async Task GetProjectMonth_MonthNoNonZero_ServiceFails_ReturnsNotFound()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _projectMonthService.GetProjectMonthAsync("PRJ1", 99)
                .Returns(ApiResponseDto<ProjectMonthDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetProjectMonth("PRJ1", 99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetProjectMonth_MonthNoNonZero_DataIsNull_ReturnsNotFound()
        {
            // Arrange
            _projectMonthService.GetProjectMonthAsync("PRJ1", 5)
                .Returns(ApiResponseDto<ProjectMonthDto>.SuccessResponse(null!));

            // Act
            var result = await _controller.GetProjectMonth("PRJ1", 5);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region SaveProjectMonth

        [Fact]
        public async Task SaveProjectMonth_NewRecord_CreateSuccess_ReturnsSuccessJson()
        {
            // Arrange
            var model = new ProjectMonthItem { Project = "PRJ1", MonthNo = 0, CostProfile = 100m };
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 0, CostProfile = 100m };

            _mapper.Map<ProjectMonthDto>(model).Returns(dto);
            _projectMonthService.CreateProjectMonthAsync(dto)
                .Returns(ApiResponseDto<ProjectMonthDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveProjectMonth(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal("Cost profile month saved successfully.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveProjectMonth_ExistingRecord_UpdateSuccess_ReturnsSuccessJson()
        {
            // Arrange
            var model = new ProjectMonthItem { Project = "PRJ1", MonthNo = 2, CostProfile = 300m };
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2, CostProfile = 300m };

            _mapper.Map<ProjectMonthDto>(model).Returns(dto);
            _projectMonthService.UpdateProjectMonthAsync(dto)
                .Returns(ApiResponseDto<ProjectMonthDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveProjectMonth(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal("Cost profile month updated successfully.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveProjectMonth_InvalidModelState_ReturnsValidationErrorJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("MonthNo", "MonthNo is required");

            // Act
            var result = await _controller.SaveProjectMonth(new ProjectMonthItem { Project = "PRJ1" });

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveProjectMonth_NewRecord_ServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var model = new ProjectMonthItem { Project = "PRJ1", MonthNo = 0, CostProfile = 100m };
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 0, CostProfile = 100m };
            var errors = new List<ApiErrorDto> { new() { Code = "CREATE_ERROR", Message = "Create failed" } };

            _mapper.Map<ProjectMonthDto>(model).Returns(dto);
            _projectMonthService.CreateProjectMonthAsync(dto)
                .Returns(ApiResponseDto<ProjectMonthDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveProjectMonth(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to save cost profile month.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveProjectMonth_ExistingRecord_ServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var model = new ProjectMonthItem { Project = "PRJ1", MonthNo = 3, CostProfile = 500m };
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 3, CostProfile = 500m };
            var errors = new List<ApiErrorDto> { new() { Code = "UPDATE_ERROR", Message = "Update failed" } };

            _mapper.Map<ProjectMonthDto>(model).Returns(dto);
            _projectMonthService.UpdateProjectMonthAsync(dto)
                .Returns(ApiResponseDto<ProjectMonthDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveProjectMonth(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to save cost profile month.", element.GetProperty("message").GetString());
        }

        #endregion

        #region DeleteProjectMonth

        [Fact]
        public async Task DeleteProjectMonth_ServiceSuccess_ReturnsSuccessJson()
        {
            // Arrange
            _projectMonthService.DeleteProjectMonthAsync("PRJ1", 1)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteProjectMonth("PRJ1", 1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteProjectMonth_ServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "DELETE_ERROR", Message = "Delete failed" } };
            _projectMonthService.DeleteProjectMonthAsync("PRJ1", 1)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteProjectMonth("PRJ1", 1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to delete cost profile month.", element.GetProperty("message").GetString());
        }

        #endregion
    }
}

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectMiscControllerTest
{
    public class ProjectMiscControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly ProjectMiscController _controller;

        public ProjectMiscControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ProjectMiscController(_mapper, _projectService);
        }

        // Helper to round-trip an anonymous JsonResult value through JSON serialisation
        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        // Shared setup for GetPagedProjectsAsync
        private void SetupPagedProjects(List<ProjectDto>? projects = null, PaginationDto? pagination = null)
        {
            projects ??= new List<ProjectDto>();
            pagination ??= new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = projects.Count };

            var apiResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects, pagination);
            _projectService.GetPagedProjectsAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<List<ProjectMiscItem>>(Arg.Any<List<ProjectDto>>())
                   .Returns(projects.Select(p => new ProjectMiscItem { ParentProject = p.ParentProject ?? string.Empty }).ToList());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithProjectMiscGrid()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new ProjectDto { ParentProject = "P001" },
                new ProjectDto { ParentProject = "P002" }
            };
            SetupPagedProjects(projects);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectMiscViewModel>(viewResult.Model);
            Assert.Equal("projectMiscGrid", model.ProjectMiscGrid.GridId);
            Assert.Equal("Misc Project Data", model.ProjectMiscGrid.Title);
        }

        [Fact]
        public async Task Index_CallsGetPagedProjectsAsync_WithDefaultParameters()
        {
            // Arrange
            SetupPagedProjects();

            // Act
            await _controller.Index();

            // Assert
            await _projectService.Received(1).GetPagedProjectsAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task Index_GridConfig_HasCorrectProperties()
        {
            // Arrange
            SetupPagedProjects();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectMiscViewModel>(viewResult.Model);
            var grid = model.ProjectMiscGrid;

            Assert.False(grid.ShowCheckboxColumn);
            Assert.True(grid.ShowPagination);
            Assert.False(grid.AllowAdd);
            Assert.True(grid.AllowEdit);
            Assert.True(grid.AllowDelete);
            Assert.Equal("ParentProject", grid.KeyProperty);
            Assert.Equal("editProjectMisc", grid.EditFunction);
            Assert.Equal("deleteProjectMisc", grid.DeleteFunction);
            Assert.Equal("/FPS/ProjectMisc/LoadProjectMiscGrid", grid.BindGridUrl);
        }

        #endregion

        #region LoadProjectMiscGrid Tests

        [Fact]
        public async Task LoadProjectMiscGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            SetupPagedProjects();
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());

            // Act
            var result = await _controller.LoadProjectMiscGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<ProjectMiscItem>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadProjectMiscGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadProjectMiscGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadProjectMiscGrid_WithNullFilter_HandlesGracefully()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null };
            SetupPagedProjects();
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());

            // Act
            var result = await _controller.LoadProjectMiscGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partialViewResult);
        }

        [Fact]
        public async Task LoadProjectMiscGrid_WithEmptyDataResponse_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            SetupPagedProjects();
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());

            // Act
            var result = await _controller.LoadProjectMiscGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectMiscItem>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadProjectMiscGrid_ConfiguresGridCorrectly()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = "{}",
                SortBy = "ParentProject",
                Descending = true,
                PageSize = 20
            };
            SetupPagedProjects();
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());

            // Act
            var result = await _controller.LoadProjectMiscGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectMiscItem>>(partialViewResult.Model);

            Assert.Equal("projectMiscGrid", gridConfig.GridId);
            Assert.Equal("Misc Project Data", gridConfig.Title);
            Assert.False(gridConfig.ShowCheckboxColumn);
            Assert.True(gridConfig.ShowPagination);
            Assert.Equal("ParentProject", gridConfig.KeyProperty);
            Assert.Equal("editProjectMisc", gridConfig.EditFunction);
            Assert.Equal("deleteProjectMisc", gridConfig.DeleteFunction);
            Assert.Equal("/FPS/ProjectMisc/LoadProjectMiscGrid", gridConfig.BindGridUrl);
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task Edit_Get_WithEmptyParentProject_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Edit(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Project is required.", value.message);
        }

        [Fact]
        public async Task Edit_Get_WithNullParentProject_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Edit((string)null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Edit_Get_WhenProjectNotFound_ReturnsJsonError()
        {
            // Arrange
            var parentProject = "NONEXISTENT";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _projectService.GetProjectByIdAsync(parentProject).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Project is not found.", value.message);
        }

        [Fact]
        public async Task Edit_Get_WithValidProject_ReturnsPartialViewWithModel()
        {
            // Arrange
            var parentProject = "P001";
            var projectDto = new ProjectDto { ParentProject = parentProject, SubAccountCode = "1020400" };
            var projectMiscItem = new ProjectMiscItem { ParentProject = parentProject, SubAccountCode = "1020400" };
            var subAccounts = new List<SubAccountDto>
            {
                new SubAccountDto { SubAccountCode = "1020400", SubAccount = "Defra (England)" },
                new SubAccountDto { SubAccountCode = "1020402", SubAccount = "Scottish Gov" }
            };

            var projectResponse = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);
            var subAccountResponse = ApiResponseDto<List<SubAccountDto>>.SuccessResponse(subAccounts);

            _projectService.GetProjectByIdAsync(parentProject).Returns(projectResponse);
            _projectService.GetSubAccountsAsync().Returns(subAccountResponse);
            _mapper.Map<ProjectMiscItem>(projectDto).Returns(projectMiscItem);

            // Act
            var result = await _controller.Edit(parentProject);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditProjectMisc", partialViewResult.ViewName);
            var model = Assert.IsType<ProjectMiscItem>(partialViewResult.Model);
            Assert.Equal(parentProject, model.ParentProject);
        }

        [Fact]
        public async Task Edit_Get_PopulatesSubAccountCodeList_FromApi()
        {
            // Arrange
            var parentProject = "P001";
            var projectDto = new ProjectDto { ParentProject = parentProject, SubAccountCode = "1020400" };
            var projectMiscItem = new ProjectMiscItem { ParentProject = parentProject, SubAccountCode = "1020400" };
            var subAccounts = new List<SubAccountDto>
            {
                new SubAccountDto { SubAccountCode = "1020400", SubAccount = "Defra (England)" },
                new SubAccountDto { SubAccountCode = "1020402", SubAccount = "Scottish Gov" },
                new SubAccountDto { SubAccountCode = "1020403", SubAccount = "Defra (GB)" }
            };

            _projectService.GetProjectByIdAsync(parentProject).Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _projectService.GetSubAccountsAsync().Returns(ApiResponseDto<List<SubAccountDto>>.SuccessResponse(subAccounts));
            _mapper.Map<ProjectMiscItem>(projectDto).Returns(projectMiscItem);

            // Act
            var result = await _controller.Edit(parentProject);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<ProjectMiscItem>(partialViewResult.Model);

            // 3 sub accounts + 1 blank "-- Select --" option
            Assert.Equal(4, model.SubAccountCodeList.Count);
            Assert.Equal("", model.SubAccountCodeList.First().Value);
            Assert.Equal("-- Select --", model.SubAccountCodeList.First().Text);
        }

        [Fact]
        public async Task Edit_Get_SubAccountCodeList_PreSelectsCurrentValue()
        {
            // Arrange
            var parentProject = "P001";
            var projectDto = new ProjectDto { ParentProject = parentProject, SubAccountCode = "1020402" };
            var projectMiscItem = new ProjectMiscItem { ParentProject = parentProject, SubAccountCode = "1020402" };
            var subAccounts = new List<SubAccountDto>
            {
                new SubAccountDto { SubAccountCode = "1020400", SubAccount = "Defra (England)" },
                new SubAccountDto { SubAccountCode = "1020402", SubAccount = "Scottish Gov" }
            };

            _projectService.GetProjectByIdAsync(parentProject).Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _projectService.GetSubAccountsAsync().Returns(ApiResponseDto<List<SubAccountDto>>.SuccessResponse(subAccounts));
            _mapper.Map<ProjectMiscItem>(projectDto).Returns(projectMiscItem);

            // Act
            var result = await _controller.Edit(parentProject);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<ProjectMiscItem>(partialViewResult.Model);
            var selected = model.SubAccountCodeList.Where(s => s.Selected).ToList();
            Assert.Single(selected);
            Assert.Equal("1020402", selected[0].Value);
        }

        [Fact]
        public async Task Edit_Get_WhenSubAccountApiReturnsNull_StillReturnsPartialView()
        {
            // Arrange
            var parentProject = "P001";
            var projectDto = new ProjectDto { ParentProject = parentProject };
            var projectMiscItem = new ProjectMiscItem { ParentProject = parentProject };

            _projectService.GetProjectByIdAsync(parentProject).Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _projectService.GetSubAccountsAsync().Returns(ApiResponseDto<List<SubAccountDto>>.SuccessResponse(null!));
            _mapper.Map<ProjectMiscItem>(projectDto).Returns(projectMiscItem);

            // Act
            var result = await _controller.Edit(parentProject);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<ProjectMiscItem>(partialViewResult.Model);
            // Only the blank "-- Select --" prepend item
            Assert.Single(model.SubAccountCodeList);
        }

        [Fact]
        public async Task Edit_Get_CallsGetSubAccountsAsync()
        {
            // Arrange
            var parentProject = "P001";
            var projectDto = new ProjectDto { ParentProject = parentProject };
            var projectMiscItem = new ProjectMiscItem { ParentProject = parentProject };

            _projectService.GetProjectByIdAsync(parentProject).Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _projectService.GetSubAccountsAsync().Returns(ApiResponseDto<List<SubAccountDto>>.SuccessResponse(new List<SubAccountDto>()));
            _mapper.Map<ProjectMiscItem>(projectDto).Returns(projectMiscItem);

            // Act
            await _controller.Edit(parentProject);

            // Assert
            await _projectService.Received(1).GetSubAccountsAsync();
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_Post_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var item = new ProjectMiscItem { ParentProject = "P001" };
            _controller.ModelState.AddModelError("ParentProject", "Project is required");

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please correct the errors below.", value.message);
        }

        [Fact]
        public async Task Edit_Post_WhenProjectNotFound_ReturnsJsonError()
        {
            // Arrange
            var item = new ProjectMiscItem { ParentProject = "NONEXISTENT" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());

            _projectService.GetProjectByIdAsync(item.ParentProject).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Edit_Post_WithValidItem_ReturnsSuccessJson()
        {
            // Arrange
            var item = new ProjectMiscItem
            {
                ParentProject = "P001",
                Program = "PROG1",
                CostCentre = 1020400,
                OracleProjectCode = "OPC001",
                SubAccountCode = "1020400"
            };
            var projectDto = new ProjectDto { ParentProject = "P001" };
            var updatedDto = new ProjectDto { ParentProject = "P001", SubAccountCode = "1020400" };

            _projectService.GetProjectByIdAsync(item.ParentProject).Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _projectService.UpdateProjectAsync(item.ParentProject, Arg.Any<ProjectDto>())
                           .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(updatedDto));

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Project data updated successfully.", value.message);
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var item = new ProjectMiscItem { ParentProject = "P001" };
            var projectDto = new ProjectDto { ParentProject = "P001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" } };

            _projectService.GetProjectByIdAsync(item.ParentProject).Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _projectService.UpdateProjectAsync(item.ParentProject, Arg.Any<ProjectDto>())
                           .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Update failed", value.message);
        }

        [Fact]
        public async Task Edit_Post_PatchesCorrectFields_OnProjectDto()
        {
            // Arrange
            var item = new ProjectMiscItem
            {
                ParentProject = "P001",
                Program = "PROG99",
                CostCentre = 9999.5,
                OracleProjectCode = "OPC-NEW",
                SubAccountCode = "1020403"
            };
            var projectDto = new ProjectDto { ParentProject = "P001", Program = "OLD_PROG" };

            ProjectDto? capturedDto = null;

            _projectService.GetProjectByIdAsync(item.ParentProject).Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _projectService.UpdateProjectAsync(item.ParentProject, Arg.Do<ProjectDto>(dto => capturedDto = dto))
                           .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));

            // Act
            await _controller.Edit(item);

            // Assert
            Assert.NotNull(capturedDto);
            Assert.Equal("PROG99", capturedDto!.Program);
            Assert.Equal(9999.5, capturedDto.CostCentre);
            Assert.Equal("OPC-NEW", capturedDto.OracleProjectCode);
            Assert.Equal("1020403", capturedDto.SubAccountCode);
        }

        [Fact]
        public async Task Edit_Post_CallsUpdateProjectAsync_WithParentProject()
        {
            // Arrange
            var item = new ProjectMiscItem { ParentProject = "P001" };
            var projectDto = new ProjectDto { ParentProject = "P001" };

            _projectService.GetProjectByIdAsync(item.ParentProject).Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _projectService.UpdateProjectAsync(item.ParentProject, Arg.Any<ProjectDto>())
                           .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));

            // Act
            await _controller.Edit(item);

            // Assert
            await _projectService.Received(1).UpdateProjectAsync("P001", Arg.Any<ProjectDto>());
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithEmptyParentProject_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Delete(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Project code is required.", value.message);
        }

        [Fact]
        public async Task Delete_WithValidParentProject_ReturnsSuccessJson()
        {
            // Arrange
            var parentProject = "P001";
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _projectService.DeleteProjectAndChildrenAsync(parentProject).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Project deleted successfully.", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var parentProject = "P001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _projectService.DeleteProjectAndChildrenAsync(parentProject).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Delete failed", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceFailsWithNoErrors_ReturnsFallbackMessage()
        {
            // Arrange
            var parentProject = "P001";
            var apiResponse = ApiResponseDto<bool>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto());

            _projectService.DeleteProjectAndChildrenAsync(parentProject).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Unable to delete this project as it may be in use.", value.message);
        }

        [Theory]
        [InlineData("P001")]
        [InlineData("PROJ123")]
        [InlineData("TEST-99")]
        public async Task Delete_WithVariousParentProjects_CallsService(string parentProject)
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _projectService.DeleteProjectAndChildrenAsync(parentProject).Returns(apiResponse);

            // Act
            await _controller.Delete(parentProject);

            // Assert
            await _projectService.Received(1).DeleteProjectAndChildrenAsync(parentProject);
        }

        #endregion

        // Helper class to deserialise JSON responses
        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
        }
    }
}

using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectGroupSelectionControllerTest
{
    public class ProjectGroupSelectionControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IAppStateService _appStateService;
        private readonly ProjectGroupSelectionController _controller;

        public ProjectGroupSelectionControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _appStateService = Substitute.For<IAppStateService>();
            _controller = new ProjectGroupSelectionController(_mapper, _projectService, _appStateService);
        }

        private static List<ProjectGroupDto> BuildProjectGroupList() =>
        [
            new() { ProjectGroupName = "GRP1", ProjectGroup = "GRP1" },
            new() { ProjectGroupName = "GRP2", ProjectGroup = "GRP2" }
        ];

        private static List<ProjectDto> BuildProjectList(string projectGroup = "GRP1") =>
        [
            new() { ParentProject = "PP001", ProjectGroup = projectGroup },
            new() { ParentProject = "PP002", ProjectGroup = projectGroup }
        ];

        private void SetupProjectGroupList(List<ProjectGroupDto>? groups = null)
        {
            groups ??= BuildProjectGroupList();
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(groups));
        }

        private void SetupProjectsByProjectGroup(List<ProjectDto>? projects = null)
        {
            projects ??= BuildProjectList();
            _projectService.GetProjectsByProjectGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects, new PaginationDto()));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        #region Index Tests

        [Fact]
        public async Task Index_WithValidProjectGroup_ReturnsViewWithSelectedProjectGroup()
        {
            // Arrange
            SetupProjectGroupList();
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.Index("GRP1");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupSelectionViewModel>(viewResult.Model);
            Assert.Equal("GRP1", model.SelectedProjectGroup);
        }

        [Fact]
        public async Task Index_WithValidProjectGroup_SavesProjectGroupToSession()
        {
            // Arrange
            SetupProjectGroupList();
            SetupProjectsByProjectGroup();

            // Act
            await _controller.Index("GRP1");

            // Assert
            await _appStateService.Received(1).SetSessionAsync(Arg.Any<string>(), "GRP1");
        }

        [Fact]
        public async Task Index_WithInvalidProjectGroup_DoesNotSaveToSession()
        {
            // Arrange
            SetupProjectGroupList();
            SetupProjectsByProjectGroup();

            // Act
            await _controller.Index("INVALID_GROUP");

            // Assert
            await _appStateService.DidNotReceive().SetSessionAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Index_WithNullProjectGroup_ReturnsEmptySelectedProjectGroup()
        {
            // Arrange
            SetupProjectGroupList();
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupSelectionViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProjectGroup);
        }

        [Fact]
        public async Task Index_WithNullProjectGroup_DoesNotSaveToSession()
        {
            // Arrange
            SetupProjectGroupList();
            SetupProjectsByProjectGroup();

            // Act
            await _controller.Index(null);

            // Assert
            await _appStateService.DidNotReceive().SetSessionAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Index_PopulatesProjectGroupList_WithOrderedItems()
        {
            // Arrange — return out-of-order list to verify ordering
            var groups = new List<ProjectGroupDto>
            {
                new() { ProjectGroupName = "GRP2", ProjectGroup = "GRP2" },
                new() { ProjectGroupName = "GRP1", ProjectGroup = "GRP1" }
            };
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(groups));
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupSelectionViewModel>(viewResult.Model);
            Assert.Equal(2, model.ProjectGroupList.Count);
            Assert.Equal("GRP1", model.ProjectGroupList[0].Value);
            Assert.Equal("GRP2", model.ProjectGroupList[1].Value);
        }

        [Fact]
        public async Task Index_WhenProjectGroupServiceFails_ReturnsEmptyProjectGroupList()
        {
            // Arrange
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto()));
            SetupProjectsByProjectGroup(new List<ProjectDto>());

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupSelectionViewModel>(viewResult.Model);
            Assert.Empty(model.ProjectGroupList);
        }

        [Fact]
        public async Task Index_ProjectsGrid_HasCorrectConfiguration()
        {
            // Arrange
            SetupProjectGroupList();
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.Index("GRP1");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupSelectionViewModel>(viewResult.Model);
            Assert.NotNull(model.ProjectsGrid);
            Assert.Equal("projectsGrid", model.ProjectsGrid!.GridId);
            Assert.Equal("Projects", model.ProjectsGrid.Title);
            Assert.Equal("ParentProject", model.ProjectsGrid.KeyProperty);
            Assert.False(model.ProjectsGrid.AllowAdd);
            Assert.True(model.ProjectsGrid.AllowEdit);
            Assert.False(model.ProjectsGrid.AllowDelete);
            Assert.True(model.ProjectsGrid.AllowView);
            Assert.Equal("editProject", model.ProjectsGrid.EditFunction);
            Assert.Equal("planProject", model.ProjectsGrid.ViewFunction);
        }

        [Fact]
        public async Task Index_WithValidProjectGroup_ProjectsGridBindUrlContainsProjectGroup()
        {
            // Arrange
            SetupProjectGroupList();
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.Index("GRP1");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupSelectionViewModel>(viewResult.Model);
            Assert.Contains("GRP1", model.ProjectsGrid!.BindGridUrl);
        }

        [Fact]
        public async Task Index_WithEmptyProjectGroup_DoesNotCallGetProjectsByProjectGroup()
        {
            // Arrange
            SetupProjectGroupList();
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            // Act
            await _controller.Index(null);

            // Assert
            await _projectService.DidNotReceive()
                .GetProjectsByProjectGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Index_WithValidProjectGroup_CallsGetProjectsByProjectGroup()
        {
            // Arrange
            SetupProjectGroupList();
            SetupProjectsByProjectGroup();

            // Act
            await _controller.Index("GRP1");

            // Assert
            await _projectService.Received(1)
                .GetProjectsByProjectGroupAsync(Arg.Any<QueryParameters<string>>(), "GRP1");
        }

        [Fact]
        public async Task Index_ProjectsGrid_ContainsCorrectProjectItems()
        {
            // Arrange
            SetupProjectGroupList();
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.Index("GRP1");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupSelectionViewModel>(viewResult.Model);
            Assert.Equal(2, model.ProjectsGrid!.Data.Count);
            Assert.Equal("PP001", model.ProjectsGrid.Data[0].ParentProject);
            Assert.Equal("PP002", model.ProjectsGrid.Data[1].ParentProject);
        }

        [Fact]
        public async Task Index_WhenNoProjectGroup_ProjectsGridHasNoData()
        {
            // Arrange
            SetupProjectGroupList();
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupSelectionViewModel>(viewResult.Model);
            Assert.Empty(model.ProjectsGrid!.Data);
        }

        #endregion

        #region SaveProjectGroupSession Tests

        [Fact]
        public async Task SaveProjectGroupSession_SavesProjectGroupToSession()
        {
            // Act
            var result = await _controller.SaveProjectGroupSession("GRP1");

            // Assert
            await _appStateService.Received(1).SetSessionAsync(Arg.Any<string>(), "GRP1");
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SaveProjectGroupSession_ReturnsOk()
        {
            // Act
            var result = await _controller.SaveProjectGroupSession("GRP1");

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SaveProjectGroupSession_WithEmptyString_StillCallsSession()
        {
            // Act
            await _controller.SaveProjectGroupSession(string.Empty);

            // Assert
            await _appStateService.Received(1).SetSessionAsync(Arg.Any<string>(), string.Empty);
        }

        #endregion

        #region LoadProjectsGrid Tests

        [Fact]
        public async Task LoadProjectsGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.LoadProjectsGrid(request, "GRP1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            Assert.IsType<DataGridConfig<ProjectGroupSelectionProjectItem>>(partialView.Model);
        }

        [Fact]
        public async Task LoadProjectsGrid_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadProjectsGrid(request, "GRP1");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadProjectsGrid_WithEmptyProjectGroup_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadProjectsGrid(request, string.Empty);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadProjectsGrid_ProjectsGridHasCorrectGridId()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.LoadProjectsGrid(request, "GRP1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupSelectionProjectItem>>(partialView.Model);
            Assert.Equal("projectsGrid", grid.GridId);
        }

        [Fact]
        public async Task LoadProjectsGrid_WithProjectSearch_FiltersResultsServerSide()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            var filteredProjects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectGroup = "GRP1" }
            };
            _projectService.GetProjectsByProjectGroupAsync(Arg.Any<QueryParameters<string>>(), "GRP1")
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(filteredProjects, new PaginationDto()));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectsGrid(request, "GRP1", "PP001");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupSelectionProjectItem>>(partialView.Model);
            Assert.Single(grid.Data);
            Assert.Equal("PP001", grid.Data[0].ParentProject);
        }

        [Fact]
        public async Task LoadProjectsGrid_WithNullProjectSearch_ReturnsAllProjects()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.LoadProjectsGrid(request, "GRP1", null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupSelectionProjectItem>>(partialView.Model);
            Assert.Equal(2, grid.Data.Count);
        }

        [Fact]
        public async Task LoadProjectsGrid_SetsCorrectPaginationSortProperties()
        {
            // Arrange
            var request = new PaginationFilter<string> { SortBy = "ParentProject", Descending = true };
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.LoadProjectsGrid(request, "GRP1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupSelectionProjectItem>>(partialView.Model);
            Assert.Equal("ParentProject", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadProjectsGrid_WithProjectSearchInFilterJson_ExtractsAndUsesSearch()
        {
            // Arrange — projectSearch embedded in request.Filter JSON, not passed directly
            var request = new PaginationFilter<string>
            {
                Filter = "{\"projectSearch\":\"PP001\"}"
            };
            var filteredProjects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectGroup = "GRP1" }
            };
            _projectService.GetProjectsByProjectGroupAsync(Arg.Any<QueryParameters<string>>(), "GRP1")
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(filteredProjects, new PaginationDto()));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectsGrid(request, "GRP1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupSelectionProjectItem>>(partialView.Model);
            Assert.Single(grid.Data);
        }

        [Fact]
        public async Task LoadProjectsGrid_ProjectsGridBindUrlContainsProjectGroup()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupProjectsByProjectGroup();

            // Act
            var result = await _controller.LoadProjectsGrid(request, "GRP1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupSelectionProjectItem>>(partialView.Model);
            Assert.Contains("GRP1", grid.BindGridUrl);
        }

        #endregion
    }
}

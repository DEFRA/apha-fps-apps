using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.ProjectListControllerTest
{
    public class ProjectListControllerAdditionalTests
    {
        private readonly IProjectListService _projectListServiceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectListController _controller;

        public ProjectListControllerAdditionalTests()
        {
            _projectListServiceMock = Substitute.For<IProjectListService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectListController(_mapperMock, _projectListServiceMock);
        }

        /// <summary>
        /// Sets up the common mocks required for BuildProjectListGridAsync to complete successfully.
        /// </summary>
        private void SetupSuccessfulGridMocks(
            List<ProjectListViewDto>? dtoData = null,
            PaginationDto? pagination = null)
        {
            dtoData ??= new List<ProjectListViewDto>();

            var apiResponse = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = true,
                Data = dtoData,
                Pagination = pagination
            };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            _projectListServiceMock.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);

            _mapperMock.Map<List<ProjectListItem>>(Arg.Any<List<ProjectListViewDto>>())
                .Returns(new List<ProjectListItem>());

            if (pagination != null)
            {
                _mapperMock.Map<PaginationModel>(Arg.Any<PaginationDto>())
                    .Returns(new PaginationModel());
            }
        }

        #region Index - AllowView / ViewFunction / ExtraFilterMethod

        [Fact]
        public async Task Index_ProjectGridAllowViewIsTrue()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.True(model.ProjectGrid.AllowView);
        }

        [Fact]
        public async Task Index_ProjectGridViewFunctionIsViewProject()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal("viewProject", model.ProjectGrid.ViewFunction);
        }

        [Fact]
        public async Task Index_ProjectGridExtraFilterMethodIsCorrect()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal("getProjectExtraFilters", model.ProjectGrid.ExtraFilterMethod);
        }

        [Fact]
        public async Task Index_FilterOptionIsOne_PassedToService()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            await _controller.Index();

            // Assert
            await _projectListServiceMock.Received(1)
                .GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), 1);
        }

        [Fact]
        public async Task Index_ProjectGridFilterOptionIsOne()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal(1, model.FilterOption);
        }

        #endregion

        #region LoadProjectListGrid - AllowView / ViewFunction / ExtraFilterMethod

        [Fact]
        public async Task LoadProjectListGrid_AllowViewIsTrue()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.True(model.AllowView);
        }

        [Fact]
        public async Task LoadProjectListGrid_ViewFunctionIsViewProject()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Equal("viewProject", model.ViewFunction);
        }

        [Fact]
        public async Task LoadProjectListGrid_ExtraFilterMethodIsCorrect()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Equal("getProjectExtraFilters", model.ExtraFilterMethod);
        }

        #endregion

        #region LoadProjectListGrid - filterOption parameter

        [Fact]
        public async Task LoadProjectListGrid_DefaultFilterOption_PassesFilterOptionTwoToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            await _controller.LoadProjectListGrid(request);

            // Assert
            await _projectListServiceMock.Received(1)
                .GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), 2);
        }

        [Fact]
        public async Task LoadProjectListGrid_CustomFilterOption_PassesCorrectFilterOptionToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            await _controller.LoadProjectListGrid(request, filterOption: 3);

            // Assert
            await _projectListServiceMock.Received(1)
                .GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), 3);
        }

        [Fact]
        public async Task LoadProjectListGrid_FilterOptionOne_PassesFilterOptionOneToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            await _controller.LoadProjectListGrid(request, filterOption: 1);

            // Assert
            await _projectListServiceMock.Received(1)
                .GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), 1);
        }

        #endregion

        #region LoadProjectListGrid - ModelState error message content

        [Fact]
        public async Task LoadProjectListGrid_WithInvalidModelState_JsonContainsErrorMessage()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string errorMessage = "Required field missing";
            _controller.ModelState.AddModelError("Filter", errorMessage);

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains(errorMessage, json);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithMultipleModelStateErrors_JsonContainsAllErrors()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Filter", "Filter is invalid");
            _controller.ModelState.AddModelError("Page", "Page must be positive");

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("Filter is invalid", json);
            Assert.Contains("Page must be positive", json);
        }

        #endregion

        #region LoadProjectListGrid - BindGridUrl

        [Fact]
        public async Task LoadProjectListGrid_BindGridUrlIsCorrect()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Equal("/PIMS/ProjectList/LoadProjectListGrid", model.BindGridUrl);
        }

        #endregion

        #region LoadProjectListGrid - KeyProperty / EditFunction

        [Fact]
        public async Task LoadProjectListGrid_KeyPropertyIsParentproject()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Equal("Parentproject", model.KeyProperty);
        }

        [Fact]
        public async Task LoadProjectListGrid_EditFunctionIsEditProject()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Equal("editProject", model.EditFunction);
        }

        [Fact]
        public async Task LoadProjectListGrid_ShowPaginationIsTrue()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.True(model.ShowPagination);
        }

        [Fact]
        public async Task LoadProjectListGrid_ShowCheckboxColumnIsFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.False(model.ShowCheckboxColumn);
        }

        #endregion

        #region LoadProjectListGrid - Columns

        [Fact]
        public async Task LoadProjectListGrid_ColumnsArePopulated()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.NotNull(model.Columns);
            Assert.NotEmpty(model.Columns);
        }

        #endregion

        #region LoadProjectListGrid - SortColumn / SortDirection defaults

        [Fact]
        public async Task LoadProjectListGrid_DefaultSortColumnIsNull()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Null(model.Pagination.SortColumn);
        }

        [Fact]
        public async Task LoadProjectListGrid_DefaultSortDirectionIsFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.False(model.Pagination.SortDirection);
        }

        #endregion
    }
}
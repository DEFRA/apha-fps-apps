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
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectGroupStaffPlanControllerTest
{
    public class ProjectGroupStaffPlanControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectGroupStaffPlanService _staffPlanService;
        private readonly IAppStateService _appStateService;
        private readonly ProjectGroupStaffPlanController _controller;

        public ProjectGroupStaffPlanControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _staffPlanService = Substitute.For<IProjectGroupStaffPlanService>();
            _appStateService = Substitute.For<IAppStateService>();
            _controller = new ProjectGroupStaffPlanController(_mapper, _staffPlanService, _appStateService);
        }

        private static ApiResponseDto<List<ProjectGroupStaffPlanViewDto>> SuccessResponse(int count = 2) =>
            ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(
                Enumerable.Range(1, count)
                    .Select(i => new ProjectGroupStaffPlanViewDto
                    {
                        ProjectGroup  = $"GROUP_{i}",
                        Manager       = $"Manager_{i}",
                        ResourceCentre = $"RC{i}",
                        Name          = $"Staff {i}",
                        Hrs           = i * 10.0,
                        ChargeRate    = i * 100m,
                        Fee           = i * 50m
                    })
                    .ToList(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = count });

        private static ApiResponseDto<List<ProjectGroupStaffPlanViewDto>> FailureResponse() =>
            ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Service error", Code = "SVC_ERR" } },
                new ApiMetaDto());

        private List<ProjectGroupStaffPlanViewItem> SampleViewItems(int count = 2) =>
            Enumerable.Range(1, count)
                .Select(i => new ProjectGroupStaffPlanViewItem
                {
                    ProjectGroup = $"GROUP_{i}",
                    Manager      = $"Manager_{i}"
                })
                .ToList();

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithViewModel()
        {
            // Arrange
            _appStateService.GetSessionAsync<string>("SelectedProjectGroup")
                .Returns("GROUP_A");
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(SampleViewItems());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProjectGroupStaffPlanViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_ReadsProjectGroupFromSession()
        {
            // Arrange
            _appStateService.GetSessionAsync<string>("SelectedProjectGroup")
                .Returns("GROUP_A");
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(SampleViewItems());

            // Act
            await _controller.Index();

            // Assert
            await _appStateService.Received(1).GetSessionAsync<string>("SelectedProjectGroup");
        }

        [Fact]
        public async Task Index_WhenSessionIsEmpty_StillCallsService()
        {
            // Arrange — no session value set
            _appStateService.GetSessionAsync<string>("SelectedProjectGroup")
                .Returns((string?)null);
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(0));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(new List<ProjectGroupStaffPlanViewItem>());

            // Act
            var result = await _controller.Index();

            // Assert
            await _staffPlanService.Received(1).GetPagedAsync(Arg.Any<QueryParameters<string>>());
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_SeedsProjectGroupFilterFromSession()
        {
            // Arrange
            const string sessionGroup = "GROUP_A";
            QueryParameters<string>? capturedQuery = null;

            _appStateService.GetSessionAsync<string>("SelectedProjectGroup")
                .Returns(sessionGroup);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _staffPlanService.GetPagedAsync(Arg.Do<QueryParameters<string>>(q => capturedQuery = q))
                .Returns(SuccessResponse());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(SampleViewItems());

            // Act
            await _controller.Index();

            // Assert — the filter JSON sent to the service must contain the session group value
            Assert.NotNull(capturedQuery);
            Assert.NotNull(capturedQuery!.Filter);
            Assert.Contains(sessionGroup, capturedQuery.Filter);
        }

        [Fact]
        public async Task Index_GridViewModel_HasCorrectGridId()
        {
            // Arrange
            _appStateService.GetSessionAsync<string>("SelectedProjectGroup")
                .Returns(string.Empty);
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(SampleViewItems());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupStaffPlanViewModel>(viewResult.Model);
            Assert.Equal("projectGroupStaffPlanGrid", model.Grid.GridId);
        }

        [Fact]
        public async Task Index_WhenServiceFails_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            _appStateService.GetSessionAsync<string>("SelectedProjectGroup")
                .Returns(string.Empty);
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(FailureResponse());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupStaffPlanViewModel>(viewResult.Model);
            Assert.Empty(model.Grid.Data);
        }

        #endregion

        #region LoadGrid Tests

        [Fact]
        public async Task LoadGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(SampleViewItems());

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            Assert.IsType<DataGridConfig<ProjectGroupStaffPlanViewItem>>(partialView.Model);
        }

        [Fact]
        public async Task LoadGrid_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Page", "Page must be greater than 0.");

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadGrid_WithNullFilter_HandlesGracefully()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(0));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(new List<ProjectGroupStaffPlanViewItem>());

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partialView);
        }

        [Fact]
        public async Task LoadGrid_WithEmptyDataResponse_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(0));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(new List<ProjectGroupStaffPlanViewItem>());

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupStaffPlanViewItem>>(partialView.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_WhenServiceFails_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(FailureResponse());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupStaffPlanViewItem>>(partialView.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_CallsServiceOnce()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(SampleViewItems());

            // Act
            await _controller.LoadGrid(request);

            // Assert
            await _staffPlanService.Received(1).GetPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion

        #region Grid Configuration Tests

        [Fact]
        public async Task Index_GridConfig_HasCorrectBindGridUrl()
        {
            // Arrange
            _appStateService.GetSessionAsync<string>("SelectedProjectGroup")
                .Returns(string.Empty);
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(SampleViewItems());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectGroupStaffPlanViewModel>(viewResult.Model);
            Assert.Equal("/FPS/ProjectGroupStaffPlan/LoadGrid", model.Grid.BindGridUrl);
        }

        [Fact]
        public async Task LoadGrid_GridConfig_HasCorrectProperties()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter     = "{}",
                SortBy     = "ProjectGroup",
                Descending = true,
                PageSize   = 20
            };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(SampleViewItems());

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupStaffPlanViewItem>>(partialView.Model);

            Assert.Equal("projectGroupStaffPlanGrid",              grid.GridId);
            Assert.Equal("ParentProject",                          grid.KeyProperty);
            Assert.Equal("/FPS/ProjectGroupStaffPlan/LoadGrid",    grid.BindGridUrl);
            Assert.True(grid.ShowPagination);
            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.False(grid.ShowCheckboxColumn);
        }

        [Fact]
        public async Task LoadGrid_GridConfig_SortColumnAndDirection_AreSetFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter     = "{}",
                SortBy     = "Manager",
                Descending = true
            };
            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(SampleViewItems());

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupStaffPlanViewItem>>(partialView.Model);
            Assert.Equal("Manager", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadGrid_GridConfig_PaginationMetadata_IsSetFromServiceResponse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var response = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(
                new List<ProjectGroupStaffPlanViewDto> { new() { ProjectGroup = "GROUP_A" } },
                new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 20 });

            _staffPlanService.GetPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(new List<ProjectGroupStaffPlanViewItem> { new() { ProjectGroup = "GROUP_A" } });

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProjectGroupStaffPlanViewItem>>(partialView.Model);
            Assert.Equal(2,  grid.Pagination.PageNumber);
            Assert.Equal(5,  grid.Pagination.PageSize);
            Assert.Equal(20, grid.Pagination.TotalRecords);
        }

        [Fact]
        public async Task LoadGrid_ExistingProjectGroupFilter_IsNotOverwritten()
        {
            // Arrange — filter already contains ProjectGroup; the controller must not overwrite it
            var request = new PaginationFilter<string>
            {
                Filter = "{\"ProjectGroup\":\"GROUP_B\"}"
            };
            QueryParameters<string>? capturedQuery = null;

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _staffPlanService.GetPagedAsync(Arg.Do<QueryParameters<string>>(q => capturedQuery = q))
                .Returns(SuccessResponse());
            _mapper.Map<List<ProjectGroupStaffPlanViewItem>>(Arg.Any<List<ProjectGroupStaffPlanViewDto>>())
                .Returns(SampleViewItems());

            // Act
            await _controller.LoadGrid(request);

            // Assert — the original filter value must be preserved
            Assert.NotNull(capturedQuery?.Filter);
            Assert.Contains("GROUP_B", capturedQuery!.Filter);
        }

        #endregion
    }
}

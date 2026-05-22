using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.WorkGroupValidTimeCodeControllerTest
{
    public class WorkGroupValidTimeCodeControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;
        private readonly WorkGroupValidTimeCodeController _controller;

        public WorkGroupValidTimeCodeControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _controller = new WorkGroupValidTimeCodeController(_mapper, _workGroupService);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void SetupDefaultValidTimeCodesMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupValidTimeCodeItem>>(Arg.Any<List<WorkGroupValidTimeCodeDto>>())
                .Returns([]);
        }

        private void SetupDefaultWorkGroupsResponse()
        {
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _mapper.Map<List<WorkGroup>>(Arg.Any<List<WorkGroupDto>>())
                .Returns([]);
        }

        private void SetupDefaultValidTimeCodesResponse()
        {
            _workGroupService.GetPagedWorkGroupValidTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.SuccessResponse([]));
        }

        // ── Index ──────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_Always_ReturnsViewResultWithViewModel()
        {
            // Arrange
            SetupDefaultWorkGroupsResponse();
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<WorkGroupValidTimeCodeViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_WithWorkGroup_SetsSelectedWorkGroup()
        {
            // Arrange
            SetupDefaultWorkGroupsResponse();
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.Index("WG1");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupValidTimeCodeViewModel>(viewResult.Model);
            Assert.Equal("WG1", model.SelectedWorkGroup);
        }

        [Fact]
        public async Task Index_WithoutWorkGroup_SelectedWorkGroupIsEmpty()
        {
            // Arrange
            SetupDefaultWorkGroupsResponse();
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupValidTimeCodeViewModel>(viewResult.Model);
            Assert.Equal("", model.SelectedWorkGroup);
        }

        [Fact]
        public async Task Index_PopulatesWorkGroupOptions_WhenServiceReturnsData()
        {
            // Arrange
            var workGroupDtos = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1" },
                new() { WorkGroupName = "WG2", ProfitCentre = "PC2" }
            };
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(workGroupDtos));
            _mapper.Map<List<WorkGroup>>(Arg.Any<List<WorkGroupDto>>())
                .Returns(workGroupDtos.Select(w => new WorkGroup { WorkGroupName = w.WorkGroupName }).ToList());
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupValidTimeCodeViewModel>(viewResult.Model);
            Assert.Equal(2, model.WorkGroupOptions.Count);
        }

        [Fact]
        public async Task Index_WorkGroupServiceFails_ReturnsEmptyWorkGroupOptions()
        {
            // Arrange
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse([], new ApiMetaDto()));
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupValidTimeCodeViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_WorkGroupServiceReturnsNull_ReturnsEmptyWorkGroupOptions()
        {
            // Arrange
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(null!));
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupValidTimeCodeViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_ValidTimeCodesGrid_HasCorrectConfiguration()
        {
            // Arrange
            SetupDefaultWorkGroupsResponse();
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupValidTimeCodeViewModel>(viewResult.Model);
            Assert.Equal("validTimeCodesGrid", model.ValidTimeCodesGrid.GridId);
            Assert.Equal("Valid Time Codes", model.ValidTimeCodesGrid.Title);
            Assert.Equal("/PACT/WorkGroupValidTimeCode/LoadValidTimeCodesGrid", model.ValidTimeCodesGrid.BindGridUrl);
            Assert.Equal("getValidTimeCodesExtraFilters", model.ValidTimeCodesGrid.ExtraFilterMethod);
            Assert.False(model.ValidTimeCodesGrid.AllowAdd);
            Assert.False(model.ValidTimeCodesGrid.AllowEdit);
            Assert.False(model.ValidTimeCodesGrid.AllowDelete);
            Assert.True(model.ValidTimeCodesGrid.ShowPagination);
            Assert.True(model.ValidTimeCodesGrid.AllowRowSelection);
            Assert.Equal("onValidTimeCodeRowSelect", model.ValidTimeCodesGrid.RowSelectFunction);
            Assert.Equal("TimeCode", model.ValidTimeCodesGrid.KeyProperty);
        }

        #endregion

        // ── LoadValidTimeCodesGrid ─────────────────────────────────────────────

        #region LoadValidTimeCodesGrid

        [Fact]
        public async Task LoadValidTimeCodesGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.LoadValidTimeCodesGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadValidTimeCodesGrid_InvalidModelState_ReturnsJsonResult()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            _controller.ModelState.AddModelError("Page", "Invalid page");

            // Act
            var result = await _controller.LoadValidTimeCodesGrid(request);

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadValidTimeCodesGrid_WithWorkGroup_PassesWorkGroupToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            await _controller.LoadValidTimeCodesGrid(request, "WG1");

            // Assert
            await _workGroupService.Received(1).GetPagedWorkGroupValidTimeCodesAsync(
                Arg.Any<QueryParameters<string>>(), "WG1");
        }

        [Fact]
        public async Task LoadValidTimeCodesGrid_ServiceFails_ReturnsPartialWithEmptyItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _workGroupService.GetPagedWorkGroupValidTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.FailureResponse([], new ApiMetaDto()));
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.LoadValidTimeCodesGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<WorkGroupValidTimeCodeItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadValidTimeCodesGrid_ServiceReturnsNullData_ReturnsPartialWithEmptyItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _workGroupService.GetPagedWorkGroupValidTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.SuccessResponse(null!));
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.LoadValidTimeCodesGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupValidTimeCodeItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadValidTimeCodesGrid_WithNullFilter_UsesEmptyFilterDictionary()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = null };
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.LoadValidTimeCodesGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupValidTimeCodeItem>>(partial.Model);
            Assert.NotNull(grid.CurrentFilters);
            Assert.Empty(grid.CurrentFilters);
        }

        [Fact]
        public async Task LoadValidTimeCodesGrid_SuccessWithData_MapsItemsCorrectly()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var dtos = new List<WorkGroupValidTimeCodeDto>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Manager = "John", Active = true }
            };
            var mappedItems = new List<WorkGroupValidTimeCodeItem>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1" }
            };
            _workGroupService.GetPagedWorkGroupValidTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.SuccessResponse(
                    dtos, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupValidTimeCodeItem>>(dtos)
                .Returns(mappedItems);

            // Act
            var result = await _controller.LoadValidTimeCodesGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupValidTimeCodeItem>>(partial.Model);
            Assert.Single(grid.Data);
            Assert.Equal("WG1", grid.Data.First().WorkGroup);
            Assert.Equal("TC1", grid.Data.First().TimeCode);
        }

        [Fact]
        public async Task LoadValidTimeCodesGrid_SuccessWithPagination_BuildsPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 2, PageSize = 5, Filter = "{}" };
            _workGroupService.GetPagedWorkGroupValidTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.SuccessResponse(
                    [], new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 30 }));
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.LoadValidTimeCodesGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupValidTimeCodeItem>>(partial.Model);
            Assert.Equal(30, grid.Pagination.TotalRecords);
            Assert.Equal(2, grid.Pagination.PageNumber);
            Assert.Equal(5, grid.Pagination.PageSize);
        }

        [Fact]
        public async Task LoadValidTimeCodesGrid_NullPagination_BuildsDefaultPaginationWithSortValues()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 10, Filter = "{}",
                SortBy = "TimeCode", Descending = true
            };
            _workGroupService.GetPagedWorkGroupValidTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(new ApiResponseDto<List<WorkGroupValidTimeCodeDto>>
                    { Success = true, Data = [], Pagination = null });
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.LoadValidTimeCodesGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupValidTimeCodeItem>>(partial.Model);
            Assert.Equal("TimeCode", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadValidTimeCodesGrid_GridId_IsCorrect()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultValidTimeCodesResponse();
            SetupDefaultValidTimeCodesMapper();

            // Act
            var result = await _controller.LoadValidTimeCodesGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupValidTimeCodeItem>>(partial.Model);
            Assert.Equal("validTimeCodesGrid", grid.GridId);
        }

        #endregion
    }
}

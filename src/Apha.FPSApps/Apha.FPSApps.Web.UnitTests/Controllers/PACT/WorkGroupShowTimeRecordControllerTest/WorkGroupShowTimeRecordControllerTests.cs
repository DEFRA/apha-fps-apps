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

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.WorkGroupShowTimeRecordControllerTest
{
    public class WorkGroupShowTimeRecordControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;
        private readonly ICalenderMonthService _calenderMonthService;
        private readonly WorkGroupShowTimeRecordController _controller;

        public WorkGroupShowTimeRecordControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _calenderMonthService = Substitute.For<ICalenderMonthService>();
            _controller = new WorkGroupShowTimeRecordController(_mapper, _workGroupService, _calenderMonthService);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void SetupDefaultTimeRecordsMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupTimeCodeItem>>(Arg.Any<List<WorkGroupTimeCodeDto>>())
                .Returns([]);
        }

        private void SetupDefaultWorkGroupsResponse()
        {
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _mapper.Map<List<WorkGroup>>(Arg.Any<List<WorkGroupDto>>())
                .Returns([]);
        }

        private void SetupDefaultCalenderMonthsResponse()
        {
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse([]));
            _mapper.Map<List<CalenderMonth>>(Arg.Any<List<CalenderMonthDto>>())
                .Returns([]);
        }

        private void SetupDefaultTimeRecordsResponse()
        {
            _workGroupService.GetPagedWorkGroupTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<WorkGroupTimeCodeDto>>.SuccessResponse([]));
        }

        // ── Index ──────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_Always_ReturnsViewResultWithViewModel()
        {
            // Arrange
            SetupDefaultWorkGroupsResponse();
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_WithWorkGroup_SetsSelectedWorkGroup()
        {
            // Arrange
            SetupDefaultWorkGroupsResponse();
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index("WG1");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Equal("WG1", model.SelectedWorkGroup);
        }

        [Fact]
        public async Task Index_WithoutWorkGroup_SelectedWorkGroupIsNull()
        {
            // Arrange
            SetupDefaultWorkGroupsResponse();
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Null(model.SelectedWorkGroup);
        }

        [Fact]
        public async Task Index_WithMonthNumber_SetsSelectedMonthNumber()
        {
            // Arrange
            SetupDefaultWorkGroupsResponse();
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index("WG1", 3);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Equal(3, model.SelectedMonthNumber);
        }

        [Fact]
        public async Task Index_WithoutMonthNumber_SelectedMonthNumberDefaultsToOne()
        {
            // Arrange
            SetupDefaultWorkGroupsResponse();
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Equal(1, model.SelectedMonthNumber);
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
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Equal(2, model.WorkGroupOptions.Count);
        }

        [Fact]
        public async Task Index_PopulatesCalenderMonthOptions_WhenServiceReturnsData()
        {
            // Arrange
            var monthDtos = new List<CalenderMonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(monthDtos));
            _mapper.Map<List<CalenderMonth>>(Arg.Any<List<CalenderMonthDto>>())
                .Returns(monthDtos.Select(m => new CalenderMonth { MonthNumber = m.MonthNumber, MonthName = m.MonthName }).ToList());
            SetupDefaultWorkGroupsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Equal(2, model.CalenderMonthOptions.Count);
        }

        [Fact]
        public async Task Index_WorkGroupServiceFails_ReturnsEmptyWorkGroupOptions()
        {
            // Arrange
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse([], new ApiMetaDto()));
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_WorkGroupServiceReturnsNull_ReturnsEmptyWorkGroupOptions()
        {
            // Arrange
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(null!));
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_CalenderMonthServiceFails_ReturnsEmptyCalenderMonthOptions()
        {
            // Arrange
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.FailureResponse([], new ApiMetaDto()));
            SetupDefaultWorkGroupsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Empty(model.CalenderMonthOptions);
        }

        [Fact]
        public async Task Index_CalenderMonthServiceReturnsNull_ReturnsEmptyCalenderMonthOptions()
        {
            // Arrange
            _calenderMonthService.GetCalenderMonthsAsync()
                .Returns(ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(null!));
            SetupDefaultWorkGroupsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Empty(model.CalenderMonthOptions);
        }

        [Fact]
        public async Task Index_TimeRecordsGrid_HasCorrectConfiguration()
        {
            // Arrange
            SetupDefaultWorkGroupsResponse();
            SetupDefaultCalenderMonthsResponse();
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupShowTimeRecordViewModel>(viewResult.Model);
            Assert.Equal("timeRecordsGrid", model.TimeRecordsGrid.GridId);
            Assert.Equal("Time Records", model.TimeRecordsGrid.Title);
            Assert.Equal("/PACT/WorkGroupShowTimeRecord/LoadTimeRecordsGrid", model.TimeRecordsGrid.BindGridUrl);
            Assert.Equal("getTimeRecordsExtraFilters", model.TimeRecordsGrid.ExtraFilterMethod);
            Assert.False(model.TimeRecordsGrid.AllowAdd);
            Assert.False(model.TimeRecordsGrid.AllowEdit);
            Assert.False(model.TimeRecordsGrid.AllowDelete);
            Assert.True(model.TimeRecordsGrid.ShowPagination);
        }

        #endregion

        // ── LoadTimeRecordsGrid ────────────────────────────────────────────────

        #region LoadTimeRecordsGrid

        [Fact]
        public async Task LoadTimeRecordsGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.LoadTimeRecordsGrid(request, "WG1", 1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadTimeRecordsGrid_InvalidModelState_ReturnsJsonResult()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            _controller.ModelState.AddModelError("Page", "Invalid page");

            // Act
            var result = await _controller.LoadTimeRecordsGrid(request, "WG1", 1);

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadTimeRecordsGrid_WithWorkGroupAndMonth_PassesParamsToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            await _controller.LoadTimeRecordsGrid(request, "WG1", 3);

            // Assert
            await _workGroupService.Received(1).GetPagedWorkGroupTimeCodesAsync(
                Arg.Any<QueryParameters<string>>(), "WG1", 3);
        }

        [Fact]
        public async Task LoadTimeRecordsGrid_WithNullWorkGroupAndMonth_PassesNullsToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            await _controller.LoadTimeRecordsGrid(request, "WG1", 1);

            // Assert
            await _workGroupService.Received(1).GetPagedWorkGroupTimeCodesAsync(
                Arg.Any<QueryParameters<string>>(), "WG1", 1);
        }

        [Fact]
        public async Task LoadTimeRecordsGrid_ServiceFails_ReturnsPartialWithEmptyItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _workGroupService.GetPagedWorkGroupTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<WorkGroupTimeCodeDto>>.FailureResponse([], new ApiMetaDto()));
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.LoadTimeRecordsGrid(request, "WG1", 1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeCodeItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadTimeRecordsGrid_ServiceReturnsNullData_ReturnsPartialWithEmptyItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _workGroupService.GetPagedWorkGroupTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<WorkGroupTimeCodeDto>>.SuccessResponse(null!));
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.LoadTimeRecordsGrid(request, "WG1", 1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeCodeItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadTimeRecordsGrid_WithNullFilter_UsesEmptyFilterDictionary()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = null };
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.LoadTimeRecordsGrid(request, "WG1", 1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeCodeItem>>(partial.Model);
            Assert.NotNull(grid.CurrentFilters);
            Assert.Empty(grid.CurrentFilters);
        }

        [Fact]
        public async Task LoadTimeRecordsGrid_SuccessWithData_MapsItemsCorrectly()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var timeCodeDtos = new List<WorkGroupTimeCodeDto>
            {
                new() { PACTStaffID = "S1", TimeCode = "TC1", WorkGroup = "WG1" }
            };
            var mappedItems = new List<WorkGroupTimeCodeItem>
            {
                new() { TimeCode = "TC1" }
            };
            _workGroupService.GetPagedWorkGroupTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<WorkGroupTimeCodeDto>>.SuccessResponse(
                    timeCodeDtos, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupTimeCodeItem>>(timeCodeDtos)
                .Returns(mappedItems);

            // Act
            var result = await _controller.LoadTimeRecordsGrid(request, "WG1", 1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeCodeItem>>(partial.Model);
            Assert.Single(grid.Data);
        }

        [Fact]
        public async Task LoadTimeRecordsGrid_SuccessWithPagination_BuildsPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 2, PageSize = 5, Filter = "{}" };
            _workGroupService.GetPagedWorkGroupTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<WorkGroupTimeCodeDto>>.SuccessResponse(
                    [], new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 20 }));
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.LoadTimeRecordsGrid(request, "WG1", 1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeCodeItem>>(partial.Model);
            Assert.Equal(20, grid.Pagination.TotalRecords);
            Assert.Equal(2, grid.Pagination.PageNumber);
            Assert.Equal(5, grid.Pagination.PageSize);
        }

        [Fact]
        public async Task LoadTimeRecordsGrid_NullPagination_BuildsDefaultPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}", SortBy = "Name", Descending = true };
            _workGroupService.GetPagedWorkGroupTimeCodesAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<int>())
                .Returns(new ApiResponseDto<List<WorkGroupTimeCodeDto>> { Success = true, Data = [], Pagination = null });
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.LoadTimeRecordsGrid(request, "WG1", 1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeCodeItem>>(partial.Model);
            Assert.Equal("Name", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadTimeRecordsGrid_GridId_IsCorrect()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupDefaultTimeRecordsResponse();
            SetupDefaultTimeRecordsMapper();

            // Act
            var result = await _controller.LoadTimeRecordsGrid(request, "WG1", 1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeCodeItem>>(partial.Model);
            Assert.Equal("timeRecordsGrid", grid.GridId);
        }

        #endregion
    }
}
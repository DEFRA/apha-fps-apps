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
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.WorkGroupTimeByJobCodeControllerTest
{
    public class WorkGroupTimeByJobCodeControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;
        private readonly WorkGroupTimeByJobCodeController _controller;

        public WorkGroupTimeByJobCodeControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _controller = new WorkGroupTimeByJobCodeController(_mapper, _workGroupService);
        }

        // ── Shared helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Wires _mapper.Map&lt;QueryParameters&lt;string&gt;&gt; for any PaginationFilter input.
        /// </summary>
        private void SetupMapperQueryParameters()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                   .Returns(new QueryParameters<string>());
        }

        /// <summary>
        /// Wires _mapper.Map&lt;List&lt;WorkGroupTimeByJobCodeRow&gt;&gt; for any row-list input.
        /// </summary>
        private void SetupMapperRows(List<WorkGroupTimeByJobCodeRow>? mapped = null)
        {
            _mapper.Map<List<WorkGroupTimeByJobCodeRow>>(
                       Arg.Any<IEnumerable<WorkGroupTimeByJobCodeRowDto>>())
                   .Returns(mapped ?? []);
        }

        /// <summary>
        /// Wires _mapper.Map&lt;WorkGroupTimeByJobCodeSummary&gt; for any summary-dto input.
        /// </summary>
        private void SetupMapperSummary(WorkGroupTimeByJobCodeSummary? mapped = null)
        {
            _mapper.Map<WorkGroupTimeByJobCodeSummary>(
                       Arg.Any<WorkGroupTimeByJobCodeSummaryDto>())
                   .Returns(mapped ?? new WorkGroupTimeByJobCodeSummary());
        }

        /// <summary>Builds a success response with the supplied dto (or an empty one).</summary>
        private static ApiResponseDto<WorkGroupTimeByJobCodeDto> SuccessResponse(
            WorkGroupTimeByJobCodeDto? dto = null)
            => ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(
                   dto ?? new WorkGroupTimeByJobCodeDto());

        /// <summary>Builds a failure response.</summary>
        private static ApiResponseDto<WorkGroupTimeByJobCodeDto> FailureResponse()
            => ApiResponseDto<WorkGroupTimeByJobCodeDto>.FailureResponse(
                   [new ApiErrorDto { Message = "Error", Code = "ERR" }], new ApiMetaDto());

        // ══════════════════════════════════════════════════════════════════════
        // Index
        // ══════════════════════════════════════════════════════════════════════

        #region Index — return type and view model structure

        [Fact]
        public async Task Index_Always_ReturnsViewResult()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_Always_ViewModelIsWorkGroupTimeByJobCodeViewModel()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<WorkGroupTimeByJobCodeViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_WithWorkGroupAndPersonName_SetsSelectedWorkGroupAndPersonName()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal("WG1",   model.SelectedWorkGroup);
            Assert.Equal("Alice", model.SelectedPersonName);
        }

        [Fact]
        public async Task Index_WithWorkGroup_SetsWorkGroupName()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal("WG1", model.WorkGroupName);
        }

        [Fact]
        public async Task Index_ServiceSuccess_HrsPaidTakenFromResponseData()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var dto = new WorkGroupTimeByJobCodeDto { HrsPaid = 120.0 };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal(120.0, model.HrsPaid);
        }

        [Fact]
        public async Task Index_ServiceSuccessWithNullData_HrsPaidDefaultsToZero()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var response = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(null!);
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(response);

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal(0, model.HrsPaid);
        }

        [Fact]
        public async Task Index_ServiceFailure_HrsPaidDefaultsToZero()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(FailureResponse());

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal(0, model.HrsPaid);
        }

        [Fact]
        public async Task Index_ServiceCallsGetWgSummarisedStaffTimeUsageAsyncOnce()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            await _controller.Index("WG1", "Alice");

            // Assert
            await _workGroupService.Received(1)
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<QueryParameters<string>>(), "WG1");
        }

        [Fact]
        public async Task Index_ServiceThrows_PropagatesException()
        {
            // Arrange
            SetupMapperQueryParameters();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Index("WG1", "Alice"));
        }

        #endregion

        #region Index — grid config (MapToGridConfig via Index)

        [Fact]
        public async Task Index_ServiceSuccess_GridContainsMappedRows()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperSummary();
            var rows = new List<WorkGroupTimeByJobCodeRow>
            {
                new() { ParentProject = "PP1", JobCode = "JC1" },
                new() { ParentProject = "PP1", JobCode = "JC2" }
            };
            var dto = new WorkGroupTimeByJobCodeDto
            {
                Rows    = [new() { ParentProject = "PP1", JobCode = "JC1" }],
                Summary = new WorkGroupTimeByJobCodeSummaryDto()
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));
            _mapper.Map<List<WorkGroupTimeByJobCodeRow>>(Arg.Any<IEnumerable<WorkGroupTimeByJobCodeRowDto>>())
                   .Returns(rows);

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal(2, model.Grid.Data.Count);
        }

        [Fact]
        public async Task Index_ServiceFailure_GridDataIsEmpty()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(FailureResponse());

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_ServiceSuccessWithNullData_GridDataIsEmpty()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var response = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(null!);
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(response);

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_GridConfig_HasCorrectStaticConfiguration()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal("timeUsageGrid",                                      model.Grid.GridId);
            Assert.Equal("/PACT/WorkGroupTimeByJobCode/LoadGrid",              model.Grid.BindGridUrl);
            Assert.Equal("getWorkGroupTimeByJobCodeExtraFilters",              model.Grid.ExtraFilterMethod);
            Assert.False(model.Grid.ShowCheckboxColumn);
            Assert.False(model.Grid.AllowAdd);
            Assert.False(model.Grid.AllowEdit);
            Assert.False(model.Grid.AllowDelete);
            Assert.False(model.Grid.AllowRowSelection);
            Assert.True(model.Grid.ShowPagination);
        }

        [Fact]
        public async Task Index_ServiceSuccess_GridPaginationSetFromResponseData()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var dto = new WorkGroupTimeByJobCodeDto
            {
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 25 }
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal(25, model.Grid.Pagination.TotalRecords);
            Assert.Equal(1,  model.Grid.Pagination.PageNumber);
            Assert.Equal(10, model.Grid.Pagination.PageSize);
        }

        [Fact]
        public async Task Index_GridPagination_SortByIsNullAndDescendingIsFalse()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Null(model.Grid.Pagination.SortColumn);
            Assert.False(model.Grid.Pagination.SortDirection);
        }

        #endregion

        #region Index — summary (MapToSummary via Index)

        [Fact]
        public async Task Index_ServiceSuccess_SummaryMappedFromResponseData()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            var expectedSummary = new WorkGroupTimeByJobCodeSummary { GrandTotalTime = 200.0 };
            _mapper.Map<WorkGroupTimeByJobCodeSummary>(Arg.Any<WorkGroupTimeByJobCodeSummaryDto>())
                   .Returns(expectedSummary);
            var dto = new WorkGroupTimeByJobCodeDto
            {
                Summary = new WorkGroupTimeByJobCodeSummaryDto { GrandTotalTime = 200.0 }
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal(expectedSummary, model.Summary);
        }

        [Fact]
        public async Task Index_ServiceFailure_SummaryIsDefault()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(FailureResponse());

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal(0, model.Summary.GrandTotalTime);
        }

        [Fact]
        public async Task Index_ServiceSuccessWithNullData_SummaryIsDefault()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var response = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(null!);
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(response);

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WorkGroupTimeByJobCodeViewModel)((ViewResult)result).Model!;
            Assert.Equal(0, model.Summary.GrandTotalTime);
        }

        #endregion

        // ══════════════════════════════════════════════════════════════════════
        // LoadGrid
        // ══════════════════════════════════════════════════════════════════════

        #region LoadGrid — return type and partial view

        [Fact]
        public async Task LoadGrid_ValidRequest_ReturnsPartialViewResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadGrid_ValidRequest_PartialViewNameIsDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_ValidRequest_ModelIsDataGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<WorkGroupTimeByJobCodeRow>>(partial.Model);
        }

        #endregion

        #region LoadGrid — workGroup validation (ArgumentException.ThrowIfNullOrWhiteSpace)

        [Fact]
        public async Task LoadGrid_NullWorkGroup_ThrowsArgumentException()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _controller.LoadGrid(request, null!));
        }

        [Fact]
        public async Task LoadGrid_EmptyWorkGroup_ThrowsArgumentException()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.LoadGrid(request, ""));
        }

        [Fact]
        public async Task LoadGrid_WhitespaceWorkGroup_ThrowsArgumentException()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.LoadGrid(request, "   "));
        }

        [Fact]
        public async Task LoadGrid_InvalidWorkGroup_ServiceNeverCalled()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            try { await _controller.LoadGrid(request, ""); } catch { /* expected */ }

            // Assert
            await _workGroupService.DidNotReceiveWithAnyArgs()
                .GetWgSummarisedStaffTimeUsageAsync(default!, default!);
        }

        #endregion

        #region LoadGrid — service interaction

        [Fact]
        public async Task LoadGrid_ValidRequest_CallsServiceOnceWithCorrectWorkGroup()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            await _controller.LoadGrid(request, "WG1");

            // Assert
            await _workGroupService.Received(1)
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<QueryParameters<string>>(), "WG1");
        }

        [Fact]
        public async Task LoadGrid_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadGrid(request, "WG1"));
        }

        [Fact]
        public async Task LoadGrid_MapperMapsRequestToQueryParameters()
        {
            // Arrange
            var request   = new PaginationFilter<string> { Page = 2, PageSize = 5, Filter = "{}" };
            var mappedQuery = new QueryParameters<string> { Page = 2, PageSize = 5 };
            _mapper.Map<QueryParameters<string>>(request).Returns(mappedQuery);
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(mappedQuery, "WG1")
                             .Returns(SuccessResponse());

            // Act
            await _controller.LoadGrid(request, "WG1");

            // Assert
            _mapper.Received(1).Map<QueryParameters<string>>(request);
            await _workGroupService.Received(1)
                .GetWgSummarisedStaffTimeUsageAsync(mappedQuery, "WG1");
        }

        #endregion

        #region LoadGrid — MapToGridConfig branches

        [Fact]
        public async Task LoadGrid_ServiceSuccess_GridContainsMappedRows()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            var rows = new List<WorkGroupTimeByJobCodeRow>
            {
                new() { ParentProject = "PP1", JobCode = "JC1" },
                new() { ParentProject = "PP2", JobCode = "JC2" }
            };
            var dto = new WorkGroupTimeByJobCodeDto
            {
                Rows       = [new(), new()],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse(dto));
            _mapper.Map<List<WorkGroupTimeByJobCodeRow>>(
                       Arg.Any<IEnumerable<WorkGroupTimeByJobCodeRowDto>>())
                   .Returns(rows);

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeByJobCodeRow>>(partial.Model);
            Assert.Equal(2, grid.Data.Count);
        }

        [Fact]
        public async Task LoadGrid_ServiceFailure_GridDataIsEmpty()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(FailureResponse());

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeByJobCodeRow>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_ServiceSuccessWithNullData_GridDataIsEmpty()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            var response = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(null!);
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(response);

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeByJobCodeRow>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_ServiceSuccess_GridPaginationSetFromResponseData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 2, PageSize = 5, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            var dto = new WorkGroupTimeByJobCodeDto
            {
                Pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 50 }
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeByJobCodeRow>>(partial.Model);
            Assert.Equal(50, grid.Pagination.TotalRecords);
            Assert.Equal(2,  grid.Pagination.PageNumber);
            Assert.Equal(5,  grid.Pagination.PageSize);
        }

        [Fact]
        public async Task LoadGrid_SortByAndDescending_ArePropagatedIntoGridPagination()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 10, Filter = "{}",
                SortBy = "JobCode", Descending = true
            };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeByJobCodeRow>>(partial.Model);
            Assert.Equal("JobCode", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadGrid_NoSortBy_SortColumnIsNullAndDescendingIsFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeByJobCodeRow>>(partial.Model);
            Assert.Null(grid.Pagination.SortColumn);
            Assert.False(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadGrid_GridConfig_HasCorrectStaticConfiguration()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTimeByJobCodeRow>>(partial.Model);
            Assert.Equal("timeUsageGrid",                                      grid.GridId);
            Assert.Equal("/PACT/WorkGroupTimeByJobCode/LoadGrid",              grid.BindGridUrl);
            Assert.Equal("getWorkGroupTimeByJobCodeExtraFilters",              grid.ExtraFilterMethod);
            Assert.False(grid.ShowCheckboxColumn);
            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.False(grid.AllowRowSelection);
            Assert.True(grid.ShowPagination);
        }

        #endregion
    }
}

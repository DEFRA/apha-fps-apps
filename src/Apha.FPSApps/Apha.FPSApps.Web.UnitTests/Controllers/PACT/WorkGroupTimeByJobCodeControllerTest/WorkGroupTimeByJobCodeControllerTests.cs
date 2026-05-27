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

        // â”€â”€ Shared helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// Wires _mapper.Map&lt;QueryParameters&lt;string&gt;&gt; for any PaginationFilter input.
        /// </summary>
        private void SetupMapperQueryParameters()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                   .Returns(new QueryParameters<string>());
        }

        /// <summary>
        /// Wires _mapper.Map&lt;List&lt;WgSummarisedStaffTimeUsageRow&gt;&gt; for any row-list input.
        /// </summary>
        private void SetupMapperRows(List<WgSummarisedStaffTimeUsageRow>? mapped = null)
        {
            _mapper.Map<List<WgSummarisedStaffTimeUsageRow>>(
                       Arg.Any<IEnumerable<WgSummarisedStaffTimeUsageRowDto>>())
                   .Returns(mapped ?? []);
        }

        /// <summary>
        /// Wires _mapper.Map&lt;WgSummarisedStaffTimeUsageSummary&gt; for any summary-dto input.
        /// </summary>
        private void SetupMapperSummary(WgSummarisedStaffTimeUsageSummary? mapped = null)
        {
            _mapper.Map<WgSummarisedStaffTimeUsageSummary>(
                       Arg.Any<WgSummarisedStaffTimeUsageSummaryDto>())
                   .Returns(mapped ?? new WgSummarisedStaffTimeUsageSummary());
        }

        /// <summary>Builds a success response with the supplied dto (or an empty one).</summary>
        private static ApiResponseDto<WgSummarisedStaffTimeUsageDto> SuccessResponse(
            WgSummarisedStaffTimeUsageDto? dto = null)
            => ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(
                   dto ?? new WgSummarisedStaffTimeUsageDto());

        /// <summary>Builds a failure response.</summary>
        private static ApiResponseDto<WgSummarisedStaffTimeUsageDto> FailureResponse()
            => ApiResponseDto<WgSummarisedStaffTimeUsageDto>.FailureResponse(
                   [new ApiErrorDto { Message = "Error", Code = "ERR" }], new ApiMetaDto());

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // Index
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

        #region Index â€” return type and view model structure

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
        public async Task Index_Always_ViewModelIsWgSummarisedStaffTimeUsageViewModel()
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
            Assert.IsType<WgSummarisedStaffTimeUsageViewModel>(viewResult.Model);
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
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
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
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
            Assert.Equal("WG1", model.WorkGroupName);
        }

        [Fact]
        public async Task Index_ServiceSuccess_HrsPaidTakenFromResponseData()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var dto = new WgSummarisedStaffTimeUsageDto { HrsPaid = 120.0 };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
            Assert.Equal(120.0, model.HrsPaid);
        }

        [Fact]
        public async Task Index_ServiceSuccessWithNullData_HrsPaidDefaultsToZero()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var response = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(null!);
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(response);

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
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
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
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

        #region Index â€” grid config (MapToGridConfig via Index)

        [Fact]
        public async Task Index_ServiceSuccess_GridContainsMappedRows()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperSummary();
            var rows = new List<WgSummarisedStaffTimeUsageRow>
            {
                new() { ParentProject = "PP1", JobCode = "JC1" },
                new() { ParentProject = "PP1", JobCode = "JC2" }
            };
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                Rows    = [new() { ParentProject = "PP1", JobCode = "JC1" }],
                Summary = new WgSummarisedStaffTimeUsageSummaryDto()
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));
            _mapper.Map<List<WgSummarisedStaffTimeUsageRow>>(Arg.Any<IEnumerable<WgSummarisedStaffTimeUsageRowDto>>())
                   .Returns(rows);

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
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
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_ServiceSuccessWithNullData_GridDataIsEmpty()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var response = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(null!);
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(response);

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
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
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
            Assert.Equal("timeUsageGrid",                                      model.Grid.GridId);
            Assert.Equal("/PACT/WorkGroupTimeByJobCode/LoadSummarisedStaffTimeGrid",              model.Grid.BindGridUrl);
            Assert.Equal("getWorkGroupTimeByJobCodeExtraFilters",              model.Grid.ExtraFilterMethod);
            Assert.False(model.Grid.ShowCheckboxColumn);
            Assert.False(model.Grid.AllowAdd);
            Assert.False(model.Grid.AllowEdit);
            Assert.False(model.Grid.AllowDelete);
            Assert.True(model.Grid.AllowRowSelection);
            Assert.True(model.Grid.ShowPagination);
        }

        [Fact]
        public async Task Index_ServiceSuccess_GridPaginationSetFromResponseData()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 25 }
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
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
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
            Assert.Null(model.Grid.Pagination.SortColumn);
            Assert.False(model.Grid.Pagination.SortDirection);
        }

        #endregion

        #region Index â€” summary (MapToSummary via Index)

        [Fact]
        public async Task Index_ServiceSuccess_SummaryMappedFromResponseData()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            var expectedSummary = new WgSummarisedStaffTimeUsageSummary { GrandTotalTime = 200.0 };
            _mapper.Map<WgSummarisedStaffTimeUsageSummary>(Arg.Any<WgSummarisedStaffTimeUsageSummaryDto>())
                   .Returns(expectedSummary);
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                Summary = new WgSummarisedStaffTimeUsageSummaryDto { GrandTotalTime = 200.0 }
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
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
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
            Assert.Equal(0, model.Summary.GrandTotalTime);
        }

        [Fact]
        public async Task Index_ServiceSuccessWithNullData_SummaryIsDefault()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var response = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(null!);
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(response);

            // Act
            var result = await _controller.Index("WG1", "Alice");

            // Assert
            var model = (WgSummarisedStaffTimeUsageViewModel)((ViewResult)result).Model!;
            Assert.Equal(0, model.Summary.GrandTotalTime);
        }

        #endregion
        
        #region LoadSummarisedStaffTimeGrid â€” return type and partial view

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_ValidRequest_ReturnsPartialViewResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_ValidRequest_PartialViewNameIsDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_ValidRequest_ModelIsDataGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<WgSummarisedStaffTimeUsageRow>>(partial.Model);
        }

        #endregion

        #region LoadSummarisedStaffTimeGrid â€” workGroup validation (ArgumentException.ThrowIfNullOrWhiteSpace)

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_NullWorkGroup_ThrowsArgumentException()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            // The controller does not guard workGroup; NullReferenceException is thrown when the
            // service returns null and the response is accessed without a null-check.
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _controller.LoadSummarisedStaffTimeGrid(request, null!));
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_EmptyWorkGroup_ThrowsArgumentException()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            // The controller does not guard workGroup; NullReferenceException is thrown when the
            // service returns null and the response is accessed without a null-check.
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _controller.LoadSummarisedStaffTimeGrid(request, ""));
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_WhitespaceWorkGroup_ThrowsArgumentException()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            // The controller does not guard workGroup; NullReferenceException is thrown when the
            // service returns null and the response is accessed without a null-check.
            await Assert.ThrowsAsync<NullReferenceException>(
                () => _controller.LoadSummarisedStaffTimeGrid(request, "   "));
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_InvalidWorkGroup_ServiceNeverCalled()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            try { await _controller.LoadSummarisedStaffTimeGrid(request, ""); } catch { /* expected */ }

            // Assert
            // The controller does not guard workGroup; the service is called even for empty strings.
            await _workGroupService.Received(1)
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<QueryParameters<string>>(), "");
        }

        #endregion

        #region LoadSummarisedStaffTimeGrid â€” service interaction

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_ValidRequest_CallsServiceOnceWithCorrectWorkGroup()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse());

            // Act
            await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            await _workGroupService.Received(1)
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<QueryParameters<string>>(), "WG1");
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadSummarisedStaffTimeGrid(request, "WG1"));
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_MapperMapsRequestToQueryParameters()
        {
            // Arrange
            var request   = new PaginationFilter<string> { Page = 2, PageSize = 5, Filter = "{}" };
            var mappedQuery = new QueryParameters<string> { Page = 2, PageSize = 5 };
            _mapper.Map<QueryParameters<string>>(request).Returns(mappedQuery);
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(mappedQuery, "WG1")
                             .Returns(SuccessResponse());

            // Act
            await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            _mapper.Received(1).Map<QueryParameters<string>>(request);
            await _workGroupService.Received(1)
                .GetWgSummarisedStaffTimeUsageAsync(mappedQuery, "WG1");
        }

        #endregion

        #region LoadSummarisedStaffTimeGrid â€” MapToGridConfig branches

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_ServiceSuccess_GridContainsMappedRows()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            var rows = new List<WgSummarisedStaffTimeUsageRow>
            {
                new() { ParentProject = "PP1", JobCode = "JC1" },
                new() { ParentProject = "PP2", JobCode = "JC2" }
            };
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                Rows       = [new(), new()],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(SuccessResponse(dto));
            _mapper.Map<List<WgSummarisedStaffTimeUsageRow>>(
                       Arg.Any<IEnumerable<WgSummarisedStaffTimeUsageRowDto>>())
                   .Returns(rows);

            // Act
            var result = await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WgSummarisedStaffTimeUsageRow>>(partial.Model);
            Assert.Equal(2, grid.Data.Count);
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_ServiceFailure_GridDataIsEmpty()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(FailureResponse());

            // Act
            var result = await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WgSummarisedStaffTimeUsageRow>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_ServiceSuccessWithNullData_GridDataIsEmpty()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            var response = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(null!);
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(response);

            // Act
            var result = await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WgSummarisedStaffTimeUsageRow>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_ServiceSuccess_GridPaginationSetFromResponseData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 2, PageSize = 5, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                Pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 50 }
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WgSummarisedStaffTimeUsageRow>>(partial.Model);
            Assert.Equal(50, grid.Pagination.TotalRecords);
            Assert.Equal(2,  grid.Pagination.PageNumber);
            Assert.Equal(5,  grid.Pagination.PageSize);
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_SortByAndDescending_ArePropagatedIntoGridPagination()
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
            var result = await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WgSummarisedStaffTimeUsageRow>>(partial.Model);
            Assert.Equal("JobCode", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_NoSortBy_SortColumnIsNullAndDescendingIsFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WgSummarisedStaffTimeUsageRow>>(partial.Model);
            Assert.Null(grid.Pagination.SortColumn);
            Assert.False(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadSummarisedStaffTimeGrid_GridConfig_HasCorrectStaticConfiguration()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupMapperQueryParameters();
            SetupMapperRows();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse());

            // Act
            var result = await _controller.LoadSummarisedStaffTimeGrid(request, "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WgSummarisedStaffTimeUsageRow>>(partial.Model);
            Assert.Equal("timeUsageGrid",                                      grid.GridId);
            Assert.Equal("/PACT/WorkGroupTimeByJobCode/LoadSummarisedStaffTimeGrid",              grid.BindGridUrl);
            Assert.Equal("getWorkGroupTimeByJobCodeExtraFilters",              grid.ExtraFilterMethod);
            Assert.False(grid.ShowCheckboxColumn);
            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.True(grid.AllowRowSelection);
            Assert.True(grid.ShowPagination);
        }

        #endregion

        // ══════════════════════════════════════════════════════════════════════════════
        // Index — ViewBag.JobTitleLookup
        // ══════════════════════════════════════════════════════════════════════════════

        #region Index — ViewBag.JobTitleLookup

        [Fact]
        public async Task Index_ServiceSuccess_ViewBagJobTitleLookupIsDictionary()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                JobTitleLookup =
                [
                    new JobTitleLookupItemDto { JobCode = "JC1", JobTitle = "Analyst" }
                ]
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));

            // Act
            await _controller.Index("WG1", "Alice");

            // Assert
            var lookup = Assert.IsType<Dictionary<string, string>>(_controller.ViewBag.JobTitleLookup);
            Assert.NotNull(lookup);
        }

        [Fact]
        public async Task Index_ServiceSuccess_ViewBagJobTitleLookupContainsExpectedEntries()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                JobTitleLookup =
                [
                    new JobTitleLookupItemDto { JobCode = "JC1", JobTitle = "Analyst" },
                    new JobTitleLookupItemDto { JobCode = "JC2", JobTitle = "Developer" }
                ]
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));

            // Act
            await _controller.Index("WG1", "Alice");

            // Assert
            var lookup = (Dictionary<string, string>)_controller.ViewBag.JobTitleLookup;
            Assert.Equal(2, lookup.Count);
            Assert.Equal("Analyst",   lookup["JC1"]);
            Assert.Equal("Developer", lookup["JC2"]);
        }

        [Fact]
        public async Task Index_ServiceSuccess_ViewBagJobTitleLookupMapsJobCodeToJobTitle()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                JobTitleLookup =
                [
                    new JobTitleLookupItemDto { JobCode = "ABC", JobTitle = "Senior Analyst" }
                ]
            };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));

            // Act
            await _controller.Index("WG1", "Alice");

            // Assert
            var lookup = (Dictionary<string, string>)_controller.ViewBag.JobTitleLookup;
            Assert.True(lookup.ContainsKey("ABC"));
            Assert.Equal("Senior Analyst", lookup["ABC"]);
        }

        [Fact]
        public async Task Index_ServiceSuccessWithNullData_ViewBagJobTitleLookupIsEmptyDictionary()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var response = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(null!);
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(response);

            // Act
            await _controller.Index("WG1", "Alice");

            // Assert
            var lookup = Assert.IsType<Dictionary<string, string>>(_controller.ViewBag.JobTitleLookup);
            Assert.Empty(lookup);
        }

        [Fact]
        public async Task Index_ServiceFailure_ViewBagJobTitleLookupIsEmptyDictionary()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(FailureResponse());

            // Act
            await _controller.Index("WG1", "Alice");

            // Assert
            var lookup = Assert.IsType<Dictionary<string, string>>(_controller.ViewBag.JobTitleLookup);
            Assert.Empty(lookup);
        }

        [Fact]
        public async Task Index_ServiceSuccess_EmptyJobTitleLookup_ViewBagJobTitleLookupIsEmptyDictionary()
        {
            // Arrange
            SetupMapperQueryParameters();
            SetupMapperRows();
            SetupMapperSummary();
            var dto = new WgSummarisedStaffTimeUsageDto { JobTitleLookup = [] };
            _workGroupService.GetWgSummarisedStaffTimeUsageAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(SuccessResponse(dto));

            // Act
            await _controller.Index("WG1", "Alice");

            // Assert
            var lookup = Assert.IsType<Dictionary<string, string>>(_controller.ViewBag.JobTitleLookup);
            Assert.Empty(lookup);
        }

        #endregion
    }
}

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

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.SummarisedWgTimeControllerTest
{
    public class SummarisedWgTimeControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ISummarisedWorkgroupTimeService _service;
        private readonly SummarisedWgTimeController _controller;

        public SummarisedWgTimeControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _service = Substitute.For<ISummarisedWorkgroupTimeService>();
            _controller = new SummarisedWgTimeController(_mapper, _service);
        }

        private void SetupGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
            _mapper.Map<List<SummarisedWgTimePivotRow>>(Arg.Any<List<SummarisedWgTimeDto>>())
                .Returns([]);
            _mapper.Map<SummarisedWgTimeSummary>(Arg.Any<SummarisedWgTimeSummaryDto>())
                .Returns(new SummarisedWgTimeSummary());
        }

        /// <summary>
        /// Returns a minimal success response with no rows and no lookup.
        /// </summary>
        private static ApiResponseDto<SummarisedWgTimeViewDto> SuccessResponseEmpty() =>
            ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(new SummarisedWgTimeViewDto
            {
                Rows = [],
                Pagination = new PaginationDto()
            });

        /// <summary>
        /// Returns a success response with one row whose Budget and SumOfCost can be customised.
        /// </summary>
        private static ApiResponseDto<SummarisedWgTimeViewDto> SuccessResponseWithRow(
            string parentProject = "PRJ1",
            decimal sumOfCost = 1000m) =>
            ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(new SummarisedWgTimeViewDto
            {
                Rows =
                [
                    new SummarisedWgTimeDto
                    {
                        WorkGroup    = "WG1",
                        ParentProject = parentProject,
                        ProjectTitle  = "Project 1",
                        SumOfTime     = 100,
                        SumOfCost     = sumOfCost
                    }
                ],
                Pagination = new PaginationDto()
            });

        /// <summary>
        /// Sets up the mapper so that mapping a list of DTOs produces the given pivot rows.
        /// </summary>
        private void SetupRowMapper(IEnumerable<SummarisedWgTimePivotRow> rows)
        {
            _mapper.Map<List<SummarisedWgTimePivotRow>>(Arg.Any<List<SummarisedWgTimeDto>>())
                .Returns([.. rows]);
        }

        private static ApiResponseDto<SummarisedWgTimeViewDto> FailureResponse() =>
            ApiResponseDto<SummarisedWgTimeViewDto>.FailureResponse(
                [new ApiErrorDto { Message = "error" }], new ApiMetaDto());

        #region Index

        [Fact]
        public async Task Index_WithValidWorkGroup_ReturnsViewResult()
        {
            // Arrange
            const string workGroup = "WG1";
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(workGroup);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_WithValidWorkGroup_SetsSelectedWorkgroupOnViewModel()
        {
            // Arrange
            const string workGroup = "WG1";
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(workGroup);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Equal(workGroup, model.SelectedWorkgroup);
        }

        [Fact]
        public async Task Index_WithNullWorkGroup_SetsSelectedWorkgroupToNull()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Null(model.SelectedWorkgroup);
        }

        [Fact]
        public async Task Index_CallsServiceWithMappedQueryParameters()
        {
            // Arrange
            const string workGroup = "WG1";
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            await _controller.Index(workGroup);

            // Assert
            await _service.Received(1).GetSummarisedWorkgroupTimeSummaryAsync(
                Arg.Any<QueryParameters<string>>(), workGroup);
        }

        [Fact]
        public async Task Index_PopulatesViewBagProjectTitleLookup_FromServiceResponse()
        {
            // Arrange
            var dto = new SummarisedWgTimeViewDto
            {
                Rows = [],
                Pagination = new PaginationDto(),
                ProjectTitleLookup =
                [
                    new SummarisedWgTimeProjectTitleLookupItem { ParentProject = "PRJ1", ProjectTitle = "Title One" },
                    new SummarisedWgTimeProjectTitleLookupItem { ParentProject = "PRJ2", ProjectTitle = "Title Two" }
                ]
            };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(dto));
            SetupGridMapper();

            // Act
            await _controller.Index(null);

            // Assert
            var lookup = Assert.IsType<Dictionary<string, string>>(_controller.ViewBag.ProjectTitleLookup);
            Assert.Equal(2, lookup.Count);
            Assert.Equal("Title One", lookup["PRJ1"]);
            Assert.Equal("Title Two", lookup["PRJ2"]);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsFailure_SetsViewBagLookupToEmptyDictionary()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(FailureResponse());
            SetupGridMapper();

            // Act
            await _controller.Index(null);

            // Assert
            var lookup = Assert.IsType<Dictionary<string, string>>(_controller.ViewBag.ProjectTitleLookup);
            Assert.Empty(lookup);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsFailure_ReturnsViewWithEmptyGridData()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(FailureResponse());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsNullData_ReturnsViewWithEmptyGridData()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(null!));
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_SetsGridId_Correctly()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Equal("summarisedWorkgroupTimeGrid", model.Grid.GridId);
        }

        [Fact]
        public async Task Index_SetsKeyProperty_Correctly()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Equal("ParentProject", model.Grid.KeyProperty);
        }

        [Fact]
        public async Task Index_DisablesAddEditDelete_OnGrid()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.False(model.Grid.AllowAdd);
            Assert.False(model.Grid.AllowEdit);
            Assert.False(model.Grid.AllowDelete);
        }

        [Fact]
        public async Task Index_EnablesPagination_OnGrid()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.True(model.Grid.ShowPagination);
        }

        [Fact]
        public async Task Index_SetsExtraFilterMethod_OnGrid()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Equal("getSummarisedWgTimeExtraFilters", model.Grid.ExtraFilterMethod);
        }

        [Fact]
        public async Task Index_SetsBindGridUrl_ContainingWorkGroup()
        {
            // Arrange
            const string workGroup = "WG1";
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(workGroup);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Contains("workGroup=WG1", model.Grid.BindGridUrl);
        }

        [Fact]
        public async Task Index_SetsGridColumns_FromGridDataProvider()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.NotNull(model.Grid.Columns);
            Assert.NotEmpty(model.Grid.Columns);
        }

        [Fact]
        public async Task Index_GridColumns_ContainParentProjectColumn()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Contains(model.Grid.Columns, c => c.PropertyName == "ParentProject");
        }

        [Fact]
        public async Task Index_GridColumns_ContainAllTwelveMonthColumns()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            var expectedMonthColumns = new[]
                { "April", "May", "June", "July", "August", "September",
                  "October", "November", "December", "January", "February", "March" };
            foreach (var col in expectedMonthColumns)
                Assert.Contains(model.Grid.Columns, c => c.PropertyName == col);
        }

        [Fact]
        public async Task Index_MapsSummaryFromResponse()
        {
            // Arrange
            var dto = new SummarisedWgTimeViewDto
            {
                Rows = [],
                Pagination = new PaginationDto(),
                Summary = new SummarisedWgTimeSummaryDto { GrandTotalCost = 9999.99, GrandTotalTime = 42.5 }
            };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(dto));
            SetupGridMapper();
            var expectedSummary = new SummarisedWgTimeSummary { GrandTotalCost = 9999.99, GrandTotalTime = 42.5 };
            _mapper.Map<SummarisedWgTimeSummary>(Arg.Any<SummarisedWgTimeSummaryDto>())
                .Returns(expectedSummary);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Equal(9999.99, model.Summary.GrandTotalCost);
            Assert.Equal(42.5, model.Summary.GrandTotalTime);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsFailure_ReturnsEmptySummary()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(FailureResponse());
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Equal(0, model.Summary.GrandTotalCost);
            Assert.Equal(0, model.Summary.GrandTotalTime);
        }

        [Fact]
        public async Task Index_InitialisesGrid_WithYrPlanAmountZero_SoPercentSpentIsZeroWhenBudgetAbsent()
        {
            // Arrange
            var dto = new SummarisedWgTimeViewDto
            {
                Rows = [new SummarisedWgTimeDto { ParentProject = "PRJ1", SumOfCost = 500 }],
                Pagination = new PaginationDto()
            };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(dto));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 500, Budget = null }]);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            var row = Assert.Single(model.Grid.Data);
            Assert.Equal(0, row.PercentSpent);
        }

        #endregion

        #region LoadGrid

        [Fact]
        public async Task LoadGrid_ValidRequest_ReturnsPartialViewResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadGrid_ValidRequest_ReturnsDataGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_ValidRequest_ReturnsDataGridConfigAsModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
        }

        [Fact]
        public async Task LoadGrid_CallsServiceWithMappedQueryParameters()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string workGroup = "WG1";
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            await _controller.LoadGrid(request, workGroup);

            // Assert
            await _service.Received(1).GetSummarisedWorkgroupTimeSummaryAsync(
                Arg.Any<QueryParameters<string>>(), workGroup);
        }

        [Fact]
        public async Task LoadGrid_WithNullWorkGroup_PassesNullToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            await _controller.LoadGrid(request, null);

            // Assert
            await _service.Received(1).GetSummarisedWorkgroupTimeSummaryAsync(
                Arg.Any<QueryParameters<string>>(), null);
        }

        [Fact]
        public async Task LoadGrid_WhenServiceReturnsFailure_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(FailureResponse());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_WhenServiceReturnsNullData_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(null!));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_SetsPaginationFromResponse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 2, PageSize = 5, Filter = "{}" };
            var dto = new SummarisedWgTimeViewDto
            {
                Rows = [],
                Pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 15 }
            };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(dto));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { PageNumber = 2, PageSize = 5, TotalRecords = 15 });
            _mapper.Map<List<SummarisedWgTimePivotRow>>(Arg.Any<List<SummarisedWgTimeDto>>())
                .Returns([]);
            _mapper.Map<SummarisedWgTimeSummary>(Arg.Any<SummarisedWgTimeSummaryDto>())
                .Returns(new SummarisedWgTimeSummary());

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Equal(2, grid.Pagination.PageNumber);
            Assert.Equal(5, grid.Pagination.PageSize);
            Assert.Equal(15, grid.Pagination.TotalRecords);
        }

        [Fact]
        public async Task LoadGrid_SetsSortColumnAndDirectionFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 10, SortBy = "SumOfCost", Descending = true, Filter = "{}"
            };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { SortColumn = "SumOfCost", SortDirection = true });
            _mapper.Map<List<SummarisedWgTimePivotRow>>(Arg.Any<List<SummarisedWgTimeDto>>())
                .Returns([]);
            _mapper.Map<SummarisedWgTimeSummary>(Arg.Any<SummarisedWgTimeSummaryDto>())
                .Returns(new SummarisedWgTimeSummary());

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Equal("SumOfCost", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadGrid_SetsBindGridUrl_ContainingWorkGroup()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string workGroup = "WG1";
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, workGroup);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Contains("workGroup=WG1", grid.BindGridUrl);
        }

        [Fact]
        public async Task LoadGrid_SetsExtraFilterMethod_OnGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Equal("getSummarisedWgTimeExtraFilters", grid.ExtraFilterMethod);
        }

        #endregion

        #region LoadGrid – yrPlanAmount Budget / PercentSpent logic

        [Fact]
        public async Task LoadGrid_WhenYrPlanAmountGreaterThanZero_OverridesRowBudget()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseWithRow(sumOfCost: 500));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 500, Budget = 200m }]);

            // Act
            var result = await _controller.LoadGrid(request, null, yrPlanAmount: 1000m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(1000m, row.Budget);
        }

        [Fact]
        public async Task LoadGrid_WhenYrPlanAmountIsZero_DoesNotOverrideRowBudget()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseWithRow(sumOfCost: 500));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 500, Budget = 800m }]);

            // Act
            var result = await _controller.LoadGrid(request, null, yrPlanAmount: 0m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(800m, row.Budget);
        }

        [Fact]
        public async Task LoadGrid_WhenBudgetIsPositive_CalculatesPercentSpentCorrectly()
        {
            // Arrange – SumOfCost=250, yrPlanAmount=1000 → Budget=1000; PercentSpent=(250/1000)*100=25
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseWithRow(sumOfCost: 250));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 250, Budget = null }]);

            // Act
            var result = await _controller.LoadGrid(request, null, yrPlanAmount: 1000m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(25m, row.PercentSpent);
        }

        [Fact]
        public async Task LoadGrid_WhenBudgetIsZero_SetsPercentSpentToZero()
        {
            // Arrange – yrPlanAmount=0 so Budget stays 0; PercentSpent must be 0
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseWithRow(sumOfCost: 500));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 500, Budget = 0m }]);

            // Act
            var result = await _controller.LoadGrid(request, null, yrPlanAmount: 0m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(0m, row.PercentSpent);
        }

        [Fact]
        public async Task LoadGrid_WhenBudgetIsNull_SetsPercentSpentToZero()
        {
            // Arrange – yrPlanAmount=0 so Budget stays null; PercentSpent must be 0
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseWithRow(sumOfCost: 500));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 500, Budget = null }]);

            // Act
            var result = await _controller.LoadGrid(request, null, yrPlanAmount: 0m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(0m, row.PercentSpent);
        }

        [Fact]
        public async Task LoadGrid_WhenYrPlanAmountGreaterThanZero_PercentSpentIsRoundedToTwoDecimalPlaces()
        {
            // Arrange – SumOfCost=1, Budget=3 → (1/3)*100=33.333… → rounds to 33.33
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseWithRow(sumOfCost: 1m));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 1m, Budget = null }]);

            // Act
            var result = await _controller.LoadGrid(request, null, yrPlanAmount: 3m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(33.33m, row.PercentSpent);
        }

        [Fact]
        public async Task LoadGrid_WhenYrPlanAmountDefaulted_BudgetAndPercentSpentDependOnRowBudget()
        {
            // Arrange – omitting yrPlanAmount uses default=0; existing Budget=1000 is preserved
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(SuccessResponseWithRow(sumOfCost: 400));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 400, Budget = 1000m }]);

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(1000m, row.Budget);
            Assert.Equal(40m, row.PercentSpent);
        }

        [Fact]
        public async Task LoadGrid_WithMultipleRows_AppliesYrPlanAndPercentSpent_ToEachRow()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var dto = new SummarisedWgTimeViewDto
            {
                Rows =
                [
                    new SummarisedWgTimeDto { ParentProject = "PRJ1", SumOfCost = 200 },
                    new SummarisedWgTimeDto { ParentProject = "PRJ2", SumOfCost = 400 }
                ],
                Pagination = new PaginationDto()
            };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(dto));
            SetupGridMapper();
            SetupRowMapper(
            [
                new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 200, Budget = null },
                new SummarisedWgTimePivotRow { ParentProject = "PRJ2", SumOfCost = 400, Budget = null }
            ]);

            // Act – PRJ1: (200/1000)*100=20%, PRJ2: (400/1000)*100=40%
            var result = await _controller.LoadGrid(request, null, yrPlanAmount: 1000m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Equal(2, grid.Data.Count);
            Assert.All(grid.Data, r => Assert.Equal(1000m, r.Budget));
            Assert.Equal(20m, grid.Data.First(r => r.ParentProject == "PRJ1").PercentSpent);
            Assert.Equal(40m, grid.Data.First(r => r.ParentProject == "PRJ2").PercentSpent);
        }

        #endregion
    }
}

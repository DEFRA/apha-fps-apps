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
    public class WorkGroupSummarisedTimeUsageControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ISummarisedWorkgroupTimeService _service;
        private readonly WorkGroupSummarisedTimeUsageController _controller;

        public WorkGroupSummarisedTimeUsageControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _service = Substitute.For<ISummarisedWorkgroupTimeService>();
            _controller = new WorkGroupSummarisedTimeUsageController(_mapper, _service);
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
        public async Task Index_WithEmptyWorkGroup_SetsSelectedWorkgroupToEmptyString()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Equal(string.Empty, model.SelectedWorkgroup);
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
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(dto));
            SetupGridMapper();

            // Act
            await _controller.Index("");

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
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(FailureResponse());
            SetupGridMapper();

            // Act
            await _controller.Index("");

            // Assert
            var lookup = Assert.IsType<Dictionary<string, string>>(_controller.ViewBag.ProjectTitleLookup);
            Assert.Empty(lookup);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsFailure_ReturnsViewWithEmptyGridData()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(FailureResponse());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsNullData_ReturnsViewWithEmptyGridData()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(null!));
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_SetsGridId_Correctly()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Equal("summarisedWorkgroupTimeGrid", model.Grid.GridId);
        }

        [Fact]
        public async Task Index_SetsKeyProperty_Correctly()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Equal("ParentProject", model.Grid.KeyProperty);
        }

        [Fact]
        public async Task Index_DisablesAddEditDelete_OnGrid()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

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
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.True(model.Grid.ShowPagination);
        }

        [Fact]
        public async Task Index_SetsExtraFilterMethod_OnGrid()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

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
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.NotNull(model.Grid.Columns);
            Assert.NotEmpty(model.Grid.Columns);
        }

        [Fact]
        public async Task Index_GridColumns_ContainParentProjectColumn()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Contains(model.Grid.Columns, c => c.PropertyName == "ParentProject");
        }

        [Fact]
        public async Task Index_GridColumns_ContainAllTwelveMonthColumns()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

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
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(dto));
            SetupGridMapper();
            var expectedSummary = new SummarisedWgTimeSummary { GrandTotalCost = 9999.99, GrandTotalTime = 42.5 };
            _mapper.Map<SummarisedWgTimeSummary>(Arg.Any<SummarisedWgTimeSummaryDto>())
                .Returns(expectedSummary);

            // Act
            var result = await _controller.Index("");

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            Assert.Equal(9999.99, model.Summary.GrandTotalCost);
            Assert.Equal(42.5, model.Summary.GrandTotalTime);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsFailure_ReturnsEmptySummary()
        {
            // Arrange
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(FailureResponse());
            SetupGridMapper();

            // Act
            var result = await _controller.Index("");

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
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(dto));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 500, Budget = null }]);

            // Act
            var result = await _controller.Index("");

            // Assert
            var model = Assert.IsType<SummarisedWgTimeViewModel>(((ViewResult)result).Model);
            var row = Assert.Single(model.Grid.Data);
            Assert.Equal(0, row.PercentSpent);
        }

        #endregion

        #region LoadLoadSummarisedWgTimeGrid

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_ValidRequest_ReturnsPartialViewResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "");

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_ValidRequest_ReturnsDataGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_ValidRequest_ReturnsDataGridConfigAsModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_CallsServiceWithMappedQueryParameters()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string workGroup = "WG1";
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            await _controller.LoadSummarisedWgTimeGrid(request, workGroup);

            // Assert
            await _service.Received(1).GetSummarisedWorkgroupTimeSummaryAsync(
                Arg.Any<QueryParameters<string>>(), workGroup);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WithEmptyWorkGroup_PassesEmptyStringToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            await _controller.LoadSummarisedWgTimeGrid(request, "");

            // Assert
            await _service.Received(1).GetSummarisedWorkgroupTimeSummaryAsync(
                Arg.Any<QueryParameters<string>>(), "");
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WhenServiceReturnsFailure_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(FailureResponse());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WhenServiceReturnsNullData_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(null!));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_SetsPaginationFromResponse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 2, PageSize = 5, Filter = "{}" };
            var dto = new SummarisedWgTimeViewDto
            {
                Rows = [],
                Pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 15 }
            };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
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
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Equal(2, grid.Pagination.PageNumber);
            Assert.Equal(5, grid.Pagination.PageSize);
            Assert.Equal(15, grid.Pagination.TotalRecords);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_SetsSortColumnAndDirectionFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 10, SortBy = "SumOfCost", Descending = true, Filter = "{}"
            };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
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
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Equal("SumOfCost", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_SetsBindGridUrl_ContainingWorkGroup()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string workGroup = "WG1";
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, workGroup);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Contains("workGroup=WG1", grid.BindGridUrl);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_SetsExtraFilterMethod_OnGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseEmpty());
            SetupGridMapper();

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Equal("getSummarisedWgTimeExtraFilters", grid.ExtraFilterMethod);
        }

        #endregion

        #region LoadLoadSummarisedWgTimeGrid – yrPlanAmount Budget / PercentSpent logic

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WhenYrPlanAmountGreaterThanZero_OverridesRowBudget()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseWithRow(sumOfCost: 500));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 500, Budget = 200m }]);

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "", yrPlanAmount: 1000m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(1000m, row.Budget);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WhenYrPlanAmountIsZero_DoesNotOverrideRowBudget()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseWithRow(sumOfCost: 500));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 500, Budget = 800m }]);

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "", yrPlanAmount: 0m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(800m, row.Budget);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WhenBudgetIsPositive_CalculatesPercentSpentCorrectly()
        {
            // Arrange – SumOfCost=250, yrPlanAmount=1000 → Budget=1000; PercentSpent=(250/1000)*100=25
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseWithRow(sumOfCost: 250));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 250, Budget = null }]);

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "", yrPlanAmount: 1000m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(25m, row.PercentSpent);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WhenBudgetIsZero_SetsPercentSpentToZero()
        {
            // Arrange – yrPlanAmount=0 so Budget stays 0; PercentSpent must be 0
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseWithRow(sumOfCost: 500));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 500, Budget = 0m }]);

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "", yrPlanAmount: 0m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(0m, row.PercentSpent);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WhenBudgetIsNull_SetsPercentSpentToZero()
        {
            // Arrange – yrPlanAmount=0 so Budget stays null; PercentSpent must be 0
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseWithRow(sumOfCost: 500));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 500, Budget = null }]);

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "", yrPlanAmount: 0m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(0m, row.PercentSpent);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WhenYrPlanAmountGreaterThanZero_PercentSpentIsRoundedToTwoDecimalPlaces()
        {
            // Arrange – SumOfCost=1, Budget=3 → (1/3)*100=33.333… → rounds to 33.33
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseWithRow(sumOfCost: 1m));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 1m, Budget = null }]);

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "", yrPlanAmount: 3m);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(33.33m, row.PercentSpent);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WhenYrPlanAmountDefaulted_BudgetAndPercentSpentDependOnRowBudget()
        {
            // Arrange – omitting yrPlanAmount uses default=0; existing Budget=1000 is preserved
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(SuccessResponseWithRow(sumOfCost: 400));
            SetupGridMapper();
            SetupRowMapper([new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 400, Budget = 1000m }]);

            // Act
            var result = await _controller.LoadSummarisedWgTimeGrid(request, "");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal(1000m, row.Budget);
            Assert.Equal(40m, row.PercentSpent);
        }

        [Fact]
        public async Task LoadLoadSummarisedWgTimeGrid_WithMultipleRows_AppliesYrPlanAndPercentSpent_ToEachRow()
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
            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), "")
                .Returns(ApiResponseDto<SummarisedWgTimeViewDto>.SuccessResponse(dto));
            SetupGridMapper();
            SetupRowMapper(
            [
                new SummarisedWgTimePivotRow { ParentProject = "PRJ1", SumOfCost = 200, Budget = null },
                new SummarisedWgTimePivotRow { ParentProject = "PRJ2", SumOfCost = 400, Budget = null }
            ]);

            // Act – PRJ1: (200/1000)*100=20%, PRJ2: (400/1000)*100=40%
            var result = await _controller.LoadSummarisedWgTimeGrid(request,"", yrPlanAmount: 1000m);

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

    // ─────────────────────────────────────────────────────────────────────────
    // DTO unit tests
    // ─────────────────────────────────────────────────────────────────────────

    public class SummarisedWgTimeViewDtoTests
    {
        // ── SummarisedWgTimeViewDto default initialisation ───────────────────

        [Fact]
        public void SummarisedWgTimeViewDto_DefaultConstructor_InitialisesMonthsToEmptyList()
        {
            var dto = new SummarisedWgTimeViewDto();
            Assert.NotNull(dto.Months);
            Assert.Empty(dto.Months);
        }

        [Fact]
        public void SummarisedWgTimeViewDto_DefaultConstructor_InitialisesRowsToEmptyList()
        {
            var dto = new SummarisedWgTimeViewDto();
            Assert.NotNull(dto.Rows);
            Assert.Empty(dto.Rows);
        }

        [Fact]
        public void SummarisedWgTimeViewDto_DefaultConstructor_InitialisesSummaryToNewInstance()
        {
            var dto = new SummarisedWgTimeViewDto();
            Assert.NotNull(dto.Summary);
            Assert.IsType<SummarisedWgTimeSummaryDto>(dto.Summary);
        }

        [Fact]
        public void SummarisedWgTimeViewDto_DefaultConstructor_InitialisesPaginationToNewInstance()
        {
            var dto = new SummarisedWgTimeViewDto();
            Assert.NotNull(dto.Pagination);
            Assert.IsType<PaginationDto>(dto.Pagination);
        }

        [Fact]
        public void SummarisedWgTimeViewDto_DefaultConstructor_InitialisesProjectTitleLookupToEmptyList()
        {
            var dto = new SummarisedWgTimeViewDto();
            Assert.NotNull(dto.ProjectTitleLookup);
            Assert.Empty(dto.ProjectTitleLookup);
        }

        [Fact]
        public void SummarisedWgTimeViewDto_CanSetAndGetMonths()
        {
            var dto = new SummarisedWgTimeViewDto { Months = [4, 5, 6] };
            Assert.Equal([4, 5, 6], dto.Months);
        }

        [Fact]
        public void SummarisedWgTimeViewDto_CanSetAndGetRows()
        {
            var rows = new List<SummarisedWgTimeDto>
            {
                new() { ParentProject = "PRJ1" },
                new() { ParentProject = "PRJ2" }
            };
            var dto = new SummarisedWgTimeViewDto { Rows = rows };
            Assert.Equal(2, dto.Rows.Count);
            Assert.Equal("PRJ1", dto.Rows[0].ParentProject);
            Assert.Equal("PRJ2", dto.Rows[1].ParentProject);
        }

        [Fact]
        public void SummarisedWgTimeViewDto_CanSetAndGetSummary()
        {
            var summary = new SummarisedWgTimeSummaryDto { GrandTotalCost = 1234.56, GrandTotalTime = 99.9 };
            var dto = new SummarisedWgTimeViewDto { Summary = summary };
            Assert.Equal(1234.56, dto.Summary.GrandTotalCost);
            Assert.Equal(99.9, dto.Summary.GrandTotalTime);
        }

        [Fact]
        public void SummarisedWgTimeViewDto_CanSetAndGetProjectTitleLookup()
        {
            var lookup = new List<SummarisedWgTimeProjectTitleLookupItem>
            {
                new() { ParentProject = "PRJ1", ProjectTitle = "Alpha" },
                new() { ParentProject = "PRJ2", ProjectTitle = "Beta" }
            };
            var dto = new SummarisedWgTimeViewDto { ProjectTitleLookup = lookup };
            Assert.Equal(2, dto.ProjectTitleLookup.Count);
            Assert.Equal("Alpha", dto.ProjectTitleLookup[0].ProjectTitle);
            Assert.Equal("Beta",  dto.ProjectTitleLookup[1].ProjectTitle);
        }

        [Fact]
        public void SummarisedWgTimeViewDto_ProjectTitleLookup_CanBeMappedToDictionary()
        {
            var dto = new SummarisedWgTimeViewDto
            {
                ProjectTitleLookup =
                [
                    new() { ParentProject = "PRJ1", ProjectTitle = "Title One" },
                    new() { ParentProject = "PRJ2", ProjectTitle = "Title Two" }
                ]
            };

            var dict = dto.ProjectTitleLookup.ToDictionary(x => x.ParentProject, x => x.ProjectTitle);

            Assert.Equal(2, dict.Count);
            Assert.Equal("Title One", dict["PRJ1"]);
            Assert.Equal("Title Two", dict["PRJ2"]);
        }

        [Fact]
        public void SummarisedWgTimeViewDto_ProjectTitleLookup_EmptyList_ProducesEmptyDictionary()
        {
            var dto = new SummarisedWgTimeViewDto();
            var dict = dto.ProjectTitleLookup.ToDictionary(x => x.ParentProject, x => x.ProjectTitle);
            Assert.Empty(dict);
        }

        // ── SummarisedWgTimeProjectTitleLookupItem ───────────────────────────

        [Fact]
        public void SummarisedWgTimeProjectTitleLookupItem_DefaultConstructor_SetsParentProjectToEmptyString()
        {
            var item = new SummarisedWgTimeProjectTitleLookupItem();
            Assert.Equal(string.Empty, item.ParentProject);
        }

        [Fact]
        public void SummarisedWgTimeProjectTitleLookupItem_DefaultConstructor_SetsProjectTitleToEmptyString()
        {
            var item = new SummarisedWgTimeProjectTitleLookupItem();
            Assert.Equal(string.Empty, item.ProjectTitle);
        }

        [Fact]
        public void SummarisedWgTimeProjectTitleLookupItem_CanSetAndGetParentProject()
        {
            var item = new SummarisedWgTimeProjectTitleLookupItem { ParentProject = "ABC123" };
            Assert.Equal("ABC123", item.ParentProject);
        }

        [Fact]
        public void SummarisedWgTimeProjectTitleLookupItem_CanSetAndGetProjectTitle()
        {
            var item = new SummarisedWgTimeProjectTitleLookupItem { ProjectTitle = "My Project Title" };
            Assert.Equal("My Project Title", item.ProjectTitle);
        }

        [Fact]
        public void SummarisedWgTimeProjectTitleLookupItem_ParentProjectAndProjectTitle_AreIndependent()
        {
            var item = new SummarisedWgTimeProjectTitleLookupItem
            {
                ParentProject = "PRJ-X",
                ProjectTitle  = "Project X Description"
            };
            Assert.Equal("PRJ-X",                item.ParentProject);
            Assert.Equal("Project X Description", item.ProjectTitle);
        }

        // ── SummarisedWgTimeSummaryDto default values ────────────────────────

        [Fact]
        public void SummarisedWgTimeSummaryDto_DefaultConstructor_AllMonthTotalsAreZero()
        {
            var dto = new SummarisedWgTimeSummaryDto();
            Assert.Equal(0, dto.TotalApril);
            Assert.Equal(0, dto.TotalMay);
            Assert.Equal(0, dto.TotalJune);
            Assert.Equal(0, dto.TotalJuly);
            Assert.Equal(0, dto.TotalAugust);
            Assert.Equal(0, dto.TotalSeptember);
            Assert.Equal(0, dto.TotalOctober);
            Assert.Equal(0, dto.TotalNovember);
            Assert.Equal(0, dto.TotalDecember);
            Assert.Equal(0, dto.TotalJanuary);
            Assert.Equal(0, dto.TotalFebruary);
            Assert.Equal(0, dto.TotalMarch);
        }

        [Fact]
        public void SummarisedWgTimeSummaryDto_DefaultConstructor_GrandTotalsAreZero()
        {
            var dto = new SummarisedWgTimeSummaryDto();
            Assert.Equal(0, dto.GrandTotalTime);
            Assert.Equal(0, dto.GrandTotalCost);
        }

        // ── SummarisedWgTimeSummaryDto – each property can be set and read ───

        [Theory]
        [InlineData(1.1)]
        [InlineData(0.0)]
        [InlineData(999.99)]
        public void SummarisedWgTimeSummaryDto_TotalApril_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalApril = value };
            Assert.Equal(value, dto.TotalApril);
        }

        [Theory]
        [InlineData(2.2)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalMay_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalMay = value };
            Assert.Equal(value, dto.TotalMay);
        }

        [Theory]
        [InlineData(3.3)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalJune_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalJune = value };
            Assert.Equal(value, dto.TotalJune);
        }

        [Theory]
        [InlineData(4.4)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalJuly_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalJuly = value };
            Assert.Equal(value, dto.TotalJuly);
        }

        [Theory]
        [InlineData(5.5)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalAugust_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalAugust = value };
            Assert.Equal(value, dto.TotalAugust);
        }

        [Theory]
        [InlineData(6.6)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalSeptember_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalSeptember = value };
            Assert.Equal(value, dto.TotalSeptember);
        }

        [Theory]
        [InlineData(7.7)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalOctober_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalOctober = value };
            Assert.Equal(value, dto.TotalOctober);
        }

        [Theory]
        [InlineData(8.8)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalNovember_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalNovember = value };
            Assert.Equal(value, dto.TotalNovember);
        }

        [Theory]
        [InlineData(9.9)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalDecember_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalDecember = value };
            Assert.Equal(value, dto.TotalDecember);
        }

        [Theory]
        [InlineData(10.1)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalJanuary_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalJanuary = value };
            Assert.Equal(value, dto.TotalJanuary);
        }

        [Theory]
        [InlineData(11.11)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalFebruary_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalFebruary = value };
            Assert.Equal(value, dto.TotalFebruary);
        }

        [Theory]
        [InlineData(12.12)]
        [InlineData(0.0)]
        public void SummarisedWgTimeSummaryDto_TotalMarch_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalMarch = value };
            Assert.Equal(value, dto.TotalMarch);
        }

        [Theory]
        [InlineData(100.5)]
        [InlineData(0.0)]
        [InlineData(9999999.99)]
        public void SummarisedWgTimeSummaryDto_GrandTotalTime_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { GrandTotalTime = value };
            Assert.Equal(value, dto.GrandTotalTime);
        }

        [Theory]
        [InlineData(50000.00)]
        [InlineData(0.0)]
        [InlineData(1234567.89)]
        public void SummarisedWgTimeSummaryDto_GrandTotalCost_CanBeSetAndRead(double value)
        {
            var dto = new SummarisedWgTimeSummaryDto { GrandTotalCost = value };
            Assert.Equal(value, dto.GrandTotalCost);
        }

        [Fact]
        public void SummarisedWgTimeSummaryDto_AllProperties_SetAndReadIndependently()
        {
            var dto = new SummarisedWgTimeSummaryDto
            {
                TotalApril     = 1.1,
                TotalMay       = 2.2,
                TotalJune      = 3.3,
                TotalJuly      = 4.4,
                TotalAugust    = 5.5,
                TotalSeptember = 6.6,
                TotalOctober   = 7.7,
                TotalNovember  = 8.8,
                TotalDecember  = 9.9,
                TotalJanuary   = 10.1,
                TotalFebruary  = 11.11,
                TotalMarch     = 12.12,
                GrandTotalTime = 82.73,
                GrandTotalCost = 99999.99
            };

            Assert.Equal(1.1,      dto.TotalApril);
            Assert.Equal(2.2,      dto.TotalMay);
            Assert.Equal(3.3,      dto.TotalJune);
            Assert.Equal(4.4,      dto.TotalJuly);
            Assert.Equal(5.5,      dto.TotalAugust);
            Assert.Equal(6.6,      dto.TotalSeptember);
            Assert.Equal(7.7,      dto.TotalOctober);
            Assert.Equal(8.8,      dto.TotalNovember);
            Assert.Equal(9.9,      dto.TotalDecember);
            Assert.Equal(10.1,     dto.TotalJanuary);
            Assert.Equal(11.11,    dto.TotalFebruary);
            Assert.Equal(12.12,    dto.TotalMarch);
            Assert.Equal(82.73,    dto.GrandTotalTime);
            Assert.Equal(99999.99, dto.GrandTotalCost);
        }

        [Fact]
        public void SummarisedWgTimeSummaryDto_MutatingOneMonthProperty_DoesNotAffectOthers()
        {
            var dto = new SummarisedWgTimeSummaryDto { TotalApril = 10.0, TotalMay = 20.0 };
            dto.TotalApril = 99.0;

            Assert.Equal(99.0, dto.TotalApril);
            Assert.Equal(20.0, dto.TotalMay);
        }
    }
}

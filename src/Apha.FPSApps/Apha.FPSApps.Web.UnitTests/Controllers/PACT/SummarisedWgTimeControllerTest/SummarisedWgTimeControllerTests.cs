using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
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
        private readonly IProjectService _projectService;
        private readonly SummarisedWgTimeController _controller;

        public SummarisedWgTimeControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _service = Substitute.For<ISummarisedWorkgroupTimeService>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new SummarisedWgTimeController(_mapper, _service, _projectService);
        }

        private void SetupGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        #region Index

        [Fact]
        public async Task Index_WithValidWorkGroup_ReturnsViewWithViewModel()
        {
            // Arrange
            const string workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        April = 100,
                        May = 150,
                        SumOfTime = 250,
                        SumOfCost = 2500
                    }
                },
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.Index(workGroup);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SummarisedWgTimeViewModel>(viewResult.Model);
            Assert.Equal(workGroup, model.SelectedWorkgroup);
            Assert.NotNull(model.Grid);
            await _service.Received(1).GetSummarisedWorkgroupTimeSummaryAsync(
                Arg.Any<QueryParameters<string>>(), workGroup);
        }

        [Fact]
        public async Task Index_WithNullWorkGroup_ReturnsViewWithAllWorkGroups()
        {
            // Arrange
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    },
                    new()
                    {
                        WorkGroup = "WG2",
                        ParentProject = "PRJ2",
                        ProjectTitle = "Project 2",
                        SumOfTime = 200,
                        SumOfCost = 2000
                    }
                },
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SummarisedWgTimeViewModel>(viewResult.Model);
            Assert.Null(model.SelectedWorkgroup);
            Assert.NotNull(model.Grid);
            Assert.Equal(2, model.Grid.Data.Count);
        }

        [Fact]
        public async Task Index_WithEmptyResult_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            const string workGroup = "WG_NONEXISTENT";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [],
                Rows = [],
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.Index(workGroup);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SummarisedWgTimeViewModel>(viewResult.Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_MapsMonthlyDataToM1ToM12Properties_Correctly()
        {
            // Arrange
            const string workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        April = 10, May = 20, June = 30, July = 40,
                        August = 50, September = 60, October = 70, November = 80,
                        December = 90, January = 100, February = 110, March = 120,
                        SumOfTime = 780,
                        SumOfCost = 7800
                    }
                },
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.Index(workGroup);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SummarisedWgTimeViewModel>(viewResult.Model);
            var row = model.Grid.Data.First();
            Assert.Equal(10, row.M1);  // April
            Assert.Equal(20, row.M2);  // May
            Assert.Equal(30, row.M3);  // June
            Assert.Equal(40, row.M4);  // July
            Assert.Equal(50, row.M5);  // August
            Assert.Equal(60, row.M6);  // September
            Assert.Equal(70, row.M7);  // October
            Assert.Equal(80, row.M8);  // November
            Assert.Equal(90, row.M9);  // December
            Assert.Equal(100, row.M10); // January
            Assert.Equal(110, row.M11); // February
            Assert.Equal(120, row.M12); // March
        }

        [Fact]
        public async Task Index_SetsGridIdAndKeyProperty_Correctly()
        {
            // Arrange
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [],
                Rows = [],
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SummarisedWgTimeViewModel>(viewResult.Model);
            Assert.Equal("summarisedWorkgroupTimeGrid", model.Grid.GridId);
            Assert.Equal("ParentProject", model.Grid.KeyProperty);
        }

        [Fact]
        public async Task Index_DisablesAddEditDeleteButtons_OnGrid()
        {
            // Arrange
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [],
                Rows = [],
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SummarisedWgTimeViewModel>(viewResult.Model);
            Assert.False(model.Grid.AllowAdd);
            Assert.False(model.Grid.AllowEdit);
            Assert.False(model.Grid.AllowDelete);
        }

        [Fact]
        public async Task Index_CreatesColumnsForAllTwelveMonths_WithCorrectNames()
        {
            // Arrange
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                Rows = [],
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SummarisedWgTimeViewModel>(viewResult.Model);
            var monthColumns = model.Grid.Columns.Where(c => c.PropertyName.StartsWith("M")).ToList();
            Assert.Equal(12, monthColumns.Count);

            var expectedMonthNames = new[] { "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar" };
            for (int i = 0; i < 12; i++)
            {
                var col = monthColumns.FirstOrDefault(c => c.PropertyName == $"M{i + 1}");
                Assert.NotNull(col);
                Assert.Equal(expectedMonthNames[i], col.DisplayName);
            }
        }

        [Fact]
        public async Task Index_ServiceReturnsFailure_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            const string workGroup = "WG1";
            var failureResponse = ApiResponseDto<SummarisedWgTimePivotDto>.FailureResponse(
                [new ApiErrorDto { Message = "Service error" }],
                new ApiMetaDto());

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(failureResponse);
            SetupGridMapper();

            // Act
            var result = await _controller.Index(workGroup);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SummarisedWgTimeViewModel>(viewResult.Model);
            Assert.Empty(model.Grid.Data);
        }

        #endregion

        #region LoadGrid

        [Fact]
        public async Task LoadGrid_ValidRequest_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            const string workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, workGroup);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
        }

        [Fact]
        public async Task LoadGrid_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            _controller.ModelState.AddModelError("Page", "Page is required");

            // Act
            var result = await _controller.LoadGrid(request, "WG1");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadGrid_WithNullWorkGroup_ReturnsPartialViewWithAllData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    },
                    new()
                    {
                        WorkGroup = "WG2",
                        ParentProject = "PRJ2",
                        ProjectTitle = "Project 2",
                        SumOfTime = 200,
                        SumOfCost = 2000
                    }
                },
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Equal(2, gridConfig.Data.Count);
        }

        [Fact]
        public async Task LoadGrid_WithPaginationParameters_AppliesPagination()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Page = 2, 
                PageSize = 5, 
                Filter = "{}" 
            };
            const string workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1],
                Rows = [],
                Pagination = new PaginationDto
                {
                    PageNumber = 2,
                    PageSize = 5,
                    TotalRecords = 15,
                    TotalPages = 3
                }
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            _mapper.Map<QueryParameters<string>>(request)
                .Returns(new QueryParameters<string> { Page = 2, PageSize = 5 });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { PageNumber = 2, PageSize = 5, TotalRecords = 15 });

            // Act
            var result = await _controller.LoadGrid(request, workGroup);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Equal(2, gridConfig.Pagination.PageNumber);
            Assert.Equal(5, gridConfig.Pagination.PageSize);
        }

        [Fact]
        public async Task LoadGrid_WithSortParameters_AppliesSorting()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "SumOfTime",
                Descending = true,
                Filter = "{}"
            };
            const string workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1],
                Rows = [],
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            _mapper.Map<QueryParameters<string>>(request)
                .Returns(new QueryParameters<string> { SortBy = "SumOfTime", Descending = true });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { SortColumn = "SumOfTime", SortDirection = true });

            // Act
            var result = await _controller.LoadGrid(request, workGroup);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Equal("SumOfTime", gridConfig.Pagination.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadGrid_WithFilterParameters_AppliesFilters()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"ParentProject\":\"PRJ1\"}"
            };
            const string workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, workGroup);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Contains("ParentProject", gridConfig.CurrentFilters.Keys);
        }

        [Fact]
        public async Task LoadGrid_WithEmptyFilter_ParsesEmptyDictionary()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "" };
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [],
                Rows = [],
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.NotNull(gridConfig.CurrentFilters);
        }

        [Fact]
        public async Task LoadGrid_ServiceReturnsFailure_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            const string workGroup = "WG1";
            var failureResponse = ApiResponseDto<SummarisedWgTimePivotDto>.FailureResponse(
                [new ApiErrorDto { Message = "Database error" }],
                new ApiMetaDto());

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(failureResponse);
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, workGroup);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadGrid_MapsAllMonthColumnsWithCorrectWidthAndType_ForDecimalNumbers()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2, 3],
                Rows = [],
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var monthColumns = gridConfig.Columns.Where(c => c.PropertyName.StartsWith("M")).ToList();

            foreach (var col in monthColumns)
            {
                Assert.Equal(GridColumnType.DecimalNumber, col.ColumnType);
                Assert.Equal(90, col.Width);
                Assert.False(col.IsFilterable);
            }
        }

        [Fact]
        public async Task LoadGrid_MapsTotalColumns_WithCorrectTypes()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [],
                Rows = [],
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);

            var timeColumn = gridConfig.Columns.FirstOrDefault(c => c.PropertyName == "SumOfTime");
            Assert.NotNull(timeColumn);
            Assert.Equal(GridColumnType.DecimalNumber, timeColumn.ColumnType);

            var costColumn = gridConfig.Columns.FirstOrDefault(c => c.PropertyName == "SumOfCost");
            Assert.NotNull(costColumn);
            Assert.Equal(GridColumnType.GbpValue, costColumn.ColumnType);

            var budgetColumn = gridConfig.Columns.FirstOrDefault(c => c.PropertyName == "Budget");
            Assert.NotNull(budgetColumn);
            Assert.Equal(GridColumnType.GbpValue, budgetColumn.ColumnType);
        }

        [Fact]
        public async Task LoadGrid_SetsBindGridUrl_WithWorkGroupParameter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            const string workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [],
                Rows = [],
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, workGroup);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            Assert.Contains("workGroup=WG1", gridConfig.BindGridUrl);
        }

        [Fact]
        public async Task LoadGrid_MapsNullMonthValues_AsNullInPivotRow()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            const string workGroup = "WG1";
            var pivotDto = new SummarisedWgTimePivotDto
            {
                Months = [1, 2],
                Rows = new List<SummarisedWgTimeDto>
                {
                    new()
                    {
                        WorkGroup = "WG1",
                        ParentProject = "PRJ1",
                        ProjectTitle = "Project 1",
                        April = 100,
                        May = null, // Null value
                        June = null,
                        SumOfTime = 100,
                        SumOfCost = 1000
                    }
                },
                Pagination = new PaginationDto()
            };

            _service.GetSummarisedWorkgroupTimeSummaryAsync(Arg.Any<QueryParameters<string>>(), workGroup)
                .Returns(ApiResponseDto<SummarisedWgTimePivotDto>.SuccessResponse(pivotDto));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadGrid(request, workGroup);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<SummarisedWgTimePivotRow>>(partial.Model);
            var row = gridConfig.Data.First();
            Assert.Equal(100, row.M1); // April
            Assert.Null(row.M2);        // May
            Assert.Null(row.M3);        // June
        }

        #endregion

        #region GetProjectDescription

        [Fact]
        public async Task GetProjectDescription_WithValidProjectId_ReturnsOkWithProjectTitle()
        {
            // Arrange
            const string projectId = "PRJ001";
            var projectDto = new Application.Dtos.FPS.ProjectDto
            {
                ParentProject = projectId,
                ProjectTitle = "Test Project Title"
            };
            var response = ApiResponseDto<Application.Dtos.FPS.ProjectDto>.SuccessResponse(projectDto);
            _projectService.GetProjectByIdAsync(projectId).Returns(response);

            // Act
            var result = await _controller.GetProjectDescription(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var value = okResult.Value;
            var successProperty = value.GetType().GetProperty("success")?.GetValue(value);
            var projectTitleProperty = value.GetType().GetProperty("projectTitle")?.GetValue(value);

            Assert.NotNull(successProperty);
            Assert.True((bool)successProperty);
            Assert.Equal("Test Project Title", projectTitleProperty);
            await _projectService.Received(1).GetProjectByIdAsync(projectId);
        }

        [Fact]
        public async Task GetProjectDescription_WithNonExistentProject_ReturnsNotFound()
        {
            // Arrange
            const string projectId = "NONEXISTENT";
            var errors = new List<ApiErrorDto> { new() { Message = "Project not found", Code = "NOT_FOUND" } };
            var response = ApiResponseDto<Application.Dtos.FPS.ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _projectService.GetProjectByIdAsync(projectId).Returns(response);

            // Act
            var result = await _controller.GetProjectDescription(projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);

            var value = notFoundResult.Value;
            var successProperty = value.GetType().GetProperty("success")?.GetValue(value);
            var messageProperty = value.GetType().GetProperty("message")?.GetValue(value);

            Assert.NotNull(successProperty);
            Assert.False((bool)successProperty);
            Assert.Equal("Project not found", messageProperty);
        }

        [Fact]
        public async Task GetProjectDescription_WithNullProjectTitle_ReturnsOkWithEmptyString()
        {
            // Arrange
            const string projectId = "PRJ001";
            var projectDto = new Application.Dtos.FPS.ProjectDto
            {
                ParentProject = projectId,
                ProjectTitle = null!
            };
            var response = ApiResponseDto<Application.Dtos.FPS.ProjectDto>.SuccessResponse(projectDto);
            _projectService.GetProjectByIdAsync(projectId).Returns(response);

            // Act
            var result = await _controller.GetProjectDescription(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var value = okResult.Value;
            var projectTitleProperty = value.GetType().GetProperty("projectTitle")?.GetValue(value);

            Assert.Equal(string.Empty, projectTitleProperty);
        }

        [Fact]
        public async Task GetProjectDescription_WithSuccessButNullData_ReturnsNotFound()
        {
            // Arrange
            const string projectId = "PRJ001";
            var response = ApiResponseDto<Application.Dtos.FPS.ProjectDto>.SuccessResponse(null!);
            _projectService.GetProjectByIdAsync(projectId).Returns(response);

            // Act
            var result = await _controller.GetProjectDescription(projectId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);

            var value = notFoundResult.Value;
            var successProperty = value.GetType().GetProperty("success")?.GetValue(value);

            Assert.NotNull(successProperty);
            Assert.False((bool)successProperty);
        }

        #endregion
    }
}

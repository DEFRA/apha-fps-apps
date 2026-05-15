using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Costbook.CostBookProjectSummaryServiceTest
{
    public class CostBookProjectSummaryServiceTests
    {
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookProjectSummaryApiClient _costBookProjectSummaryApiClient;
        private readonly CostBookProjectSummaryService _costBookProjectSummaryService;

        public CostBookProjectSummaryServiceTests()
        {
            _costBookClient = Substitute.For<ICostBookApiClient>();
            _costBookProjectSummaryApiClient = Substitute.For<ICostBookProjectSummaryApiClient>();
            _costBookClient.ProjectSummary.Returns(_costBookProjectSummaryApiClient);
            _costBookProjectSummaryService = new CostBookProjectSummaryService(_costBookClient);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            // Arrange & Act
            var service = new CostBookProjectSummaryService(_costBookClient);

            // Assert
            Assert.NotNull(service);
        }

        #endregion

        #region GetProfitIncludedTotalAsync Tests

        [Fact]
        public async Task GetProfitIncludedTotalAsync_WithValidParams_ReturnsTotal()
        {
            // Arrange
            var projectId = "P001";
            var year = 2024;
            var expectedTotal = 12345.67;
            var expectedResponse = ApiResponseDto<double>.SuccessResponse(expectedTotal);

            _costBookProjectSummaryApiClient.GetProfitIncludedTotalAsync(projectId, year).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetProfitIncludedTotalAsync(projectId, year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedTotal, result.Data);
            await _costBookProjectSummaryApiClient.Received(1).GetProfitIncludedTotalAsync(projectId, year);
        }

        [Fact]
        public async Task GetProfitIncludedTotalAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "INVALID";
            var year = 2024;
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Project not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<double>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectSummaryApiClient.GetProfitIncludedTotalAsync(projectId, year).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetProfitIncludedTotalAsync(projectId, year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProfitIncludedTotalAsync_PassesCorrectProjectIdAndYear()
        {
            // Arrange
            var projectId = "P123";
            var year = 2025;
            var expectedResponse = ApiResponseDto<double>.SuccessResponse(0.0);

            _costBookProjectSummaryApiClient.GetProfitIncludedTotalAsync(projectId, year).Returns(expectedResponse);

            // Act
            await _costBookProjectSummaryService.GetProfitIncludedTotalAsync(projectId, year);

            // Assert
            await _costBookProjectSummaryApiClient.Received(1).GetProfitIncludedTotalAsync(projectId, year);
        }

        #endregion

        #region GetStaffYearsPivotAsync Tests

        [Fact]
        public async Task GetStaffYearsPivotAsync_WithValidParams_ReturnsStaffYearsPivot()
        {
            // Arrange
            var projectId = "P001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotDto = new StaffYearsPivotDto
            {
                Years = [2023, 2024],
                TotalCount = 2
            };
            var expectedResponse = ApiResponseDto<StaffYearsPivotDto>.SuccessResponse(pivotDto);

            _costBookProjectSummaryApiClient.GetStaffYearsPivotAsync(projectId, query).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetStaffYearsPivotAsync(projectId, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Years.Count);
            await _costBookProjectSummaryApiClient.Received(1).GetStaffYearsPivotAsync(projectId, query);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_WithNullQuery_PassesNullToApiClient()
        {
            // Arrange
            var projectId = "P001";
            var expectedResponse = ApiResponseDto<StaffYearsPivotDto>.SuccessResponse(new StaffYearsPivotDto());

            _costBookProjectSummaryApiClient.GetStaffYearsPivotAsync(projectId, null).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetStaffYearsPivotAsync(projectId, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _costBookProjectSummaryApiClient.Received(1).GetStaffYearsPivotAsync(projectId, null);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<StaffYearsPivotDto>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectSummaryApiClient.GetStaffYearsPivotAsync(projectId, null).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetStaffYearsPivotAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_PassesCorrectProjectIdAndQuery()
        {
            // Arrange
            var projectId = "P123";
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                Search = "Staff",
                SortBy = "Year",
                Descending = true
            };
            var expectedResponse = ApiResponseDto<StaffYearsPivotDto>.SuccessResponse(new StaffYearsPivotDto());

            _costBookProjectSummaryApiClient.GetStaffYearsPivotAsync(projectId, query).Returns(expectedResponse);

            // Act
            await _costBookProjectSummaryService.GetStaffYearsPivotAsync(projectId, query);

            // Assert
            await _costBookProjectSummaryApiClient.Received(1).GetStaffYearsPivotAsync(
                projectId,
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 &&
                    q.PageSize == 20 &&
                    q.Search == "Staff" &&
                    q.SortBy == "Year" &&
                    q.Descending == true
                )
            );
        }

        #endregion

        #region GetStaffEffortAsync Tests

        [Fact]
        public async Task GetStaffEffortAsync_WithValidParams_ReturnsStaffEffortPivot()
        {
            // Arrange
            var projectId = "P001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotDto = new StaffEffortPivotDto
            {
                Years = [2023, 2024],
                TotalCount = 5
            };
            var expectedResponse = ApiResponseDto<StaffEffortPivotDto>.SuccessResponse(pivotDto);

            _costBookProjectSummaryApiClient.GetStaffEffortAsync(projectId, query).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetStaffEffortAsync(projectId, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Years.Count);
            await _costBookProjectSummaryApiClient.Received(1).GetStaffEffortAsync(projectId, query);
        }

        [Fact]
        public async Task GetStaffEffortAsync_WithNullQuery_PassesNullToApiClient()
        {
            // Arrange
            var projectId = "P001";
            var expectedResponse = ApiResponseDto<StaffEffortPivotDto>.SuccessResponse(new StaffEffortPivotDto());

            _costBookProjectSummaryApiClient.GetStaffEffortAsync(projectId, null).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetStaffEffortAsync(projectId, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _costBookProjectSummaryApiClient.Received(1).GetStaffEffortAsync(projectId, null);
        }

        [Fact]
        public async Task GetStaffEffortAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<StaffEffortPivotDto>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectSummaryApiClient.GetStaffEffortAsync(projectId, null).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetStaffEffortAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetStaffEffortAsync_PassesCorrectProjectIdAndQuery()
        {
            // Arrange
            var projectId = "P123";
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 50,
                SortBy = "Effort",
                Descending = false
            };
            var expectedResponse = ApiResponseDto<StaffEffortPivotDto>.SuccessResponse(new StaffEffortPivotDto());

            _costBookProjectSummaryApiClient.GetStaffEffortAsync(projectId, query).Returns(expectedResponse);

            // Act
            await _costBookProjectSummaryService.GetStaffEffortAsync(projectId, query);

            // Assert
            await _costBookProjectSummaryApiClient.Received(1).GetStaffEffortAsync(
                projectId,
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 50 &&
                    q.SortBy == "Effort" &&
                    q.Descending == false
                )
            );
        }

        #endregion

        #region GetProjectCostsPivotAsync Tests

        [Fact]
        public async Task GetProjectCostsPivotAsync_WithValidParams_ReturnsProjectCostsPivot()
        {
            // Arrange
            var projectId = "P001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotDto = new ProjectCostsPivotDto
            {
                Years = [2023, 2024, 2025],
                TotalCount = 3
            };
            var expectedResponse = ApiResponseDto<ProjectCostsPivotDto>.SuccessResponse(pivotDto);

            _costBookProjectSummaryApiClient.GetProjectCostsPivotAsync(projectId, query).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetProjectCostsPivotAsync(projectId, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Years.Count);
            await _costBookProjectSummaryApiClient.Received(1).GetProjectCostsPivotAsync(projectId, query);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_WithNullQuery_PassesNullToApiClient()
        {
            // Arrange
            var projectId = "P001";
            var expectedResponse = ApiResponseDto<ProjectCostsPivotDto>.SuccessResponse(new ProjectCostsPivotDto());

            _costBookProjectSummaryApiClient.GetProjectCostsPivotAsync(projectId, null).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetProjectCostsPivotAsync(projectId, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _costBookProjectSummaryApiClient.Received(1).GetProjectCostsPivotAsync(projectId, null);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<ProjectCostsPivotDto>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectSummaryApiClient.GetProjectCostsPivotAsync(projectId, null).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetProjectCostsPivotAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_PassesCorrectProjectIdAndQuery()
        {
            // Arrange
            var projectId = "P123";
            var query = new QueryParameters<string>
            {
                Page = 3,
                PageSize = 15,
                Search = "Costs",
                SortBy = "Year",
                Descending = true
            };
            var expectedResponse = ApiResponseDto<ProjectCostsPivotDto>.SuccessResponse(new ProjectCostsPivotDto());

            _costBookProjectSummaryApiClient.GetProjectCostsPivotAsync(projectId, query).Returns(expectedResponse);

            // Act
            await _costBookProjectSummaryService.GetProjectCostsPivotAsync(projectId, query);

            // Assert
            await _costBookProjectSummaryApiClient.Received(1).GetProjectCostsPivotAsync(
                projectId,
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 3 &&
                    q.PageSize == 15 &&
                    q.Search == "Costs" &&
                    q.SortBy == "Year" &&
                    q.Descending == true
                )
            );
        }

        #endregion
    }
}
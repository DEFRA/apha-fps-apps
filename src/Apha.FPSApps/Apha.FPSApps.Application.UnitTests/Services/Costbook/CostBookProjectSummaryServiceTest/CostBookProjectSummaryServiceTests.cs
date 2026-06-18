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

        #region GetProjectYearCostSummaryAsync Tests

        [Fact]
        public async Task GetProjectYearCostSummaryAsync_WithValidParams_ReturnsCostSummary()
        {
            // Arrange
            var projectId = "P001";
            var year = 2024;
            var summaryDto = new ProjectYearCostSummaryDto
            {
                Project             = projectId,
                Year                = year,
                StaffCostTotal      = 1000.0,
                TestCostTotal       = 200.0,
                AnimalCostTotal     = 300.0,
                AdditionalCostTotal = 50.0,
                GrandTotal          = 1550.0
            };
            var expectedResponse = ApiResponseDto<ProjectYearCostSummaryDto>.SuccessResponse(summaryDto);

            _costBookProjectSummaryApiClient.GetProjectYearCostSummaryAsync(projectId, year).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetProjectYearCostSummaryAsync(projectId, year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(projectId, result.Data.Project);
            Assert.Equal(year,      result.Data.Year);
            Assert.Equal(1000.0,    result.Data.StaffCostTotal);
            Assert.Equal(200.0,     result.Data.TestCostTotal);
            Assert.Equal(300.0,     result.Data.AnimalCostTotal);
            Assert.Equal(50.0,      result.Data.AdditionalCostTotal);
            Assert.Equal(1550.0,    result.Data.GrandTotal);
            await _costBookProjectSummaryApiClient.Received(1).GetProjectYearCostSummaryAsync(projectId, year);
        }

        [Fact]
        public async Task GetProjectYearCostSummaryAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var projectId = "INVALID";
            var year = 2024;
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Project not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProjectYearCostSummaryDto>.FailureResponse(errors, new ApiMetaDto());

            _costBookProjectSummaryApiClient.GetProjectYearCostSummaryAsync(projectId, year).Returns(expectedResponse);

            // Act
            var result = await _costBookProjectSummaryService.GetProjectYearCostSummaryAsync(projectId, year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetProjectYearCostSummaryAsync_PassesCorrectProjectIdAndYear()
        {
            // Arrange
            var projectId = "P123";
            var year = 2025;
            var expectedResponse = ApiResponseDto<ProjectYearCostSummaryDto>.SuccessResponse(
                new ProjectYearCostSummaryDto { Project = projectId, Year = year });

            _costBookProjectSummaryApiClient.GetProjectYearCostSummaryAsync(projectId, year).Returns(expectedResponse);

            // Act
            await _costBookProjectSummaryService.GetProjectYearCostSummaryAsync(projectId, year);

            // Assert
            await _costBookProjectSummaryApiClient.Received(1).GetProjectYearCostSummaryAsync(projectId, year);
        }

        #endregion

        #region ExportProjectSummaryToExcelAsync Tests

        [Fact]
        public async Task ExportProjectSummaryToExcelAsync_WithValidProjectId_ReturnsExcelData()
        {
            // Arrange
            var projectId = "P001";
            var expectedExcelData = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // Simulated Excel file bytes

            _costBookProjectSummaryApiClient.ExportProjectSummaryToExcelAsync(projectId).Returns(expectedExcelData);

            // Act
            var result = await _costBookProjectSummaryService.ExportProjectSummaryToExcelAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(expectedExcelData, result);
            await _costBookProjectSummaryApiClient.Received(1).ExportProjectSummaryToExcelAsync(projectId);
        }

        [Fact]
        public async Task ExportProjectSummaryToExcelAsync_WithEmptyProjectId_ReturnsEmptyArray()
        {
            // Arrange
            var projectId = string.Empty;
            var expectedExcelData = Array.Empty<byte>();

            _costBookProjectSummaryApiClient.ExportProjectSummaryToExcelAsync(projectId).Returns(expectedExcelData);

            // Act
            var result = await _costBookProjectSummaryService.ExportProjectSummaryToExcelAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _costBookProjectSummaryApiClient.Received(1).ExportProjectSummaryToExcelAsync(projectId);
        }

        [Fact]
        public async Task ExportProjectSummaryToExcelAsync_PassesCorrectProjectId()
        {
            // Arrange
            var projectId = "P123";
            var expectedExcelData = new byte[] { 0x01, 0x02, 0x03 };

            _costBookProjectSummaryApiClient.ExportProjectSummaryToExcelAsync(projectId).Returns(expectedExcelData);

            // Act
            await _costBookProjectSummaryService.ExportProjectSummaryToExcelAsync(projectId);

            // Assert
            await _costBookProjectSummaryApiClient.Received(1).ExportProjectSummaryToExcelAsync(
                Arg.Is<string>(id => id == projectId)
            );
        }

        [Fact]
        public async Task ExportProjectSummaryToExcelAsync_WithDifferentProjectIds_CallsApiClientMultipleTimes()
        {
            // Arrange
            var projectId1 = "P001";
            var projectId2 = "P002";
            var excelData1 = new byte[] { 0x01 };
            var excelData2 = new byte[] { 0x02 };

            _costBookProjectSummaryApiClient.ExportProjectSummaryToExcelAsync(projectId1).Returns(excelData1);
            _costBookProjectSummaryApiClient.ExportProjectSummaryToExcelAsync(projectId2).Returns(excelData2);

            // Act
            var result1 = await _costBookProjectSummaryService.ExportProjectSummaryToExcelAsync(projectId1);
            var result2 = await _costBookProjectSummaryService.ExportProjectSummaryToExcelAsync(projectId2);

            // Assert
            Assert.Equal(excelData1, result1);
            Assert.Equal(excelData2, result2);
            await _costBookProjectSummaryApiClient.Received(1).ExportProjectSummaryToExcelAsync(projectId1);
            await _costBookProjectSummaryApiClient.Received(1).ExportProjectSummaryToExcelAsync(projectId2);
        }

        [Fact]
        public async Task ExportProjectSummaryToExcelAsync_WithLargeExcelFile_ReturnsCompleteData()
        {
            // Arrange
            var projectId = "P001";
            var largeExcelData = new byte[10000]; // Simulating a large Excel file
            for (int i = 0; i < largeExcelData.Length; i++)
            {
                largeExcelData[i] = (byte)(i % 256);
            }

            _costBookProjectSummaryApiClient.ExportProjectSummaryToExcelAsync(projectId).Returns(largeExcelData);

            // Act
            var result = await _costBookProjectSummaryService.ExportProjectSummaryToExcelAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10000, result.Length);
            Assert.Equal(largeExcelData, result);
        }

        #endregion
    }
}
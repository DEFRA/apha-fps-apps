using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.ProjectSubContractServiceTest
{
    public class ProjectSubContractServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactProjectSubContractApiClient _pactProjectSubContractApiClient;
        private readonly ProjectSubContractService _service;

        public ProjectSubContractServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactProjectSubContractApiClient = Substitute.For<IPactProjectSubContractApiClient>();
            _pactClient.PactProjectSubContract.Returns(_pactProjectSubContractApiClient);
            _service = new ProjectSubContractService(_pactClient);
        }

        #region GetPagedProjectSubContractsAsync Tests

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WithValidQuery_ReturnsPaginatedSubContracts()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var project = "PP001";
            var subContracts = new List<ProjectSubContractDto>
            {
                new ProjectSubContractDto { SubContCounter = 1, Project = project, Amount = 300.00m },
                new ProjectSubContractDto { SubContCounter = 2, Project = project, Amount = 600.00m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(
                subContracts,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );
            _pactProjectSubContractApiClient.GetPagedProjectSubContractsAsync(query, project).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectSubContractsAsync(query, project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactProjectSubContractApiClient.Received(1).GetPagedProjectSubContractsAsync(query, project);
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WithNullProject_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(new List<ProjectSubContractDto>());
            _pactProjectSubContractApiClient.GetPagedProjectSubContractsAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectSubContractsAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectSubContractDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectSubContractApiClient.GetPagedProjectSubContractsAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectSubContractsAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetTotalAmountAsync Tests

        [Fact]
        public async Task GetTotalAmountAsync_WithValidProject_ReturnsTotalAmount()
        {
            // Arrange
            var project = "PP001";
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(2000.00m);
            _pactProjectSubContractApiClient.GetTotalAmountAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetTotalAmountAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2000.00m, result.Data);
            await _pactProjectSubContractApiClient.Received(1).GetTotalAmountAsync(project);
        }

        [Fact]
        public async Task GetTotalAmountAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectSubContractApiClient.GetTotalAmountAsync(null).Returns(expectedResponse);

            // Act
            var result = await _service.GetTotalAmountAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsSubContract()
        {
            // Arrange
            var subContCounter = 1;
            var subContract = new ProjectSubContractDto { SubContCounter = subContCounter, Project = "PP001", Amount = 400.00m };
            var expectedResponse = ApiResponseDto<ProjectSubContractDto>.SuccessResponse(subContract);
            _pactProjectSubContractApiClient.GetByIdAsync(subContCounter).Returns(expectedResponse);

            // Act
            var result = await _service.GetByIdAsync(subContCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(subContCounter, result.Data?.SubContCounter);
            await _pactProjectSubContractApiClient.Received(1).GetByIdAsync(subContCounter);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var subContCounter = 9999;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Sub-contract not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectSubContractDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectSubContractApiClient.GetByIdAsync(subContCounter).Returns(expectedResponse);

            // Act
            var result = await _service.GetByIdAsync(subContCounter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidSubContract_ReturnsSuccessResponse()
        {
            // Arrange
            var newSubContract = new ProjectSubContractDto
            {
                SubContCounter = 1,
                Project = "PP001",
                Supplier = "Supplier A",
                Amount = 500.00m
            };
            var expectedResponse = ApiResponseDto<ProjectSubContractDto>.SuccessResponse(newSubContract);
            _pactProjectSubContractApiClient.CreateAsync(newSubContract).Returns(expectedResponse);

            // Act
            var result = await _service.CreateAsync(newSubContract);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(newSubContract.SubContCounter, result.Data?.SubContCounter);
            await _pactProjectSubContractApiClient.Received(1).CreateAsync(newSubContract);
        }

        [Fact]
        public async Task CreateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newSubContract = new ProjectSubContractDto { Project = "PP001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate sub-contract", Code = "DUPLICATE" } };
            var expectedResponse = ApiResponseDto<ProjectSubContractDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectSubContractApiClient.CreateAsync(newSubContract).Returns(expectedResponse);

            // Act
            var result = await _service.CreateAsync(newSubContract);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidSubContract_ReturnsSuccessResponse()
        {
            // Arrange
            var subContCounter = 1;
            var updatedSubContract = new ProjectSubContractDto
            {
                SubContCounter = subContCounter,
                Project = "PP001",
                Supplier = "Updated Supplier",
                Amount = 800.00m
            };
            var expectedResponse = ApiResponseDto<ProjectSubContractDto>.SuccessResponse(updatedSubContract);
            _pactProjectSubContractApiClient.UpdateAsync(subContCounter, updatedSubContract).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateAsync(subContCounter, updatedSubContract);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(800.00m, result.Data?.Amount);
            await _pactProjectSubContractApiClient.Received(1).UpdateAsync(subContCounter, updatedSubContract);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var subContCounter = 9999;
            var subContract = new ProjectSubContractDto { SubContCounter = subContCounter, Project = "PP001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Sub-contract not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectSubContractDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectSubContractApiClient.UpdateAsync(subContCounter, subContract).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateAsync(subContCounter, subContract);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetFpsProjectSubContractsAsync Tests

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_WithValidQuery_ReturnsPaginatedSubContracts()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var project = "PP001";
            var subContracts = new List<ProjectSubContractDto>
            {
                new ProjectSubContractDto { SubContCounter = 1, Project = project, AcctCode = "LargeAnimals", Amount = 300.00m },
                new ProjectSubContractDto { SubContCounter = 2, Project = project, AcctCode = "SmallAnimals", Amount = 150.00m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(
                subContracts,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );
            _pactProjectSubContractApiClient.GetFpsProjectSubContractsAsync(query, project).Returns(expectedResponse);

            // Act
            var result = await _service.GetFpsProjectSubContractsAsync(query, project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactProjectSubContractApiClient.Received(1).GetFpsProjectSubContractsAsync(query, project);
        }

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_WithNullProject_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(new List<ProjectSubContractDto>());
            _pactProjectSubContractApiClient.GetFpsProjectSubContractsAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetFpsProjectSubContractsAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectSubContractDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectSubContractApiClient.GetFpsProjectSubContractsAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetFpsProjectSubContractsAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetFpsProjectSubContractTotalAmountAsync Tests

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_WithValidProject_ReturnsTotalAmount()
        {
            // Arrange
            var project = "PP001";
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(1500.00m);
            _pactProjectSubContractApiClient.GetFpsProjectSubContractTotalAmountAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetFpsProjectSubContractTotalAmountAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1500.00m, result.Data);
            await _pactProjectSubContractApiClient.Received(1).GetFpsProjectSubContractTotalAmountAsync(project);
        }

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectSubContractApiClient.GetFpsProjectSubContractTotalAmountAsync(null).Returns(expectedResponse);

            // Act
            var result = await _service.GetFpsProjectSubContractTotalAmountAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetMonthlySubContractsSummaryAsync Tests

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WithValidQuery_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivot = new MonthlySubContractsPivotDto
            {
                Months = [1, 2, 3],
                Rows =
                [
                    new MonthlySubContractsSummaryItemDto
                    {
                        Program = "ADMIN",
                        ParentProject = "AH",
                        MonthlyAmounts = new Dictionary<int, decimal> { { 1, 100m }, { 2, 200m } }
                    }
                ],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedResponse = ApiResponseDto<MonthlySubContractsPivotDto>.SuccessResponse(pivot);
            _pactProjectSubContractApiClient.GetMonthlySubContractsSummaryAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Months.Count);
            Assert.Single(result.Data.Rows);
            await _pactProjectSubContractApiClient.Received(1).GetMonthlySubContractsSummaryAsync(query);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WithEmptyData_ReturnsSuccessWithEmptyPivot()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivot = new MonthlySubContractsPivotDto();
            var expectedResponse = ApiResponseDto<MonthlySubContractsPivotDto>.SuccessResponse(pivot);
            _pactProjectSubContractApiClient.GetMonthlySubContractsSummaryAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!.Months);
            Assert.Empty(result.Data.Rows);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<MonthlySubContractsPivotDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectSubContractApiClient.GetMonthlySubContractsSummaryAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_DelegatesToPactClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "program", Descending = true };
            var expectedResponse = ApiResponseDto<MonthlySubContractsPivotDto>.SuccessResponse(new MonthlySubContractsPivotDto());
            _pactProjectSubContractApiClient.GetMonthlySubContractsSummaryAsync(query).Returns(expectedResponse);

            // Act
            await _service.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            await _pactProjectSubContractApiClient.Received(1).GetMonthlySubContractsSummaryAsync(query);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var subContCounter = 1;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactProjectSubContractApiClient.DeleteAsync(subContCounter).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteAsync(subContCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactProjectSubContractApiClient.Received(1).DeleteAsync(subContCounter);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var subContCounter = 9999;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Sub-contract not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectSubContractApiClient.DeleteAsync(subContCounter).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteAsync(subContCounter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}

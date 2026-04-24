using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.AdditionalCostServiceTest
{
    public class AdditionalCostServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsAdditionalCostApiClient _fpsAdditionalCostApiClient;
        private readonly AdditionalCostService _sut;

        public AdditionalCostServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsAdditionalCostApiClient = Substitute.For<IFpsAdditionalCostApiClient>();
            _fpsClient.FpsAdditionalCost.Returns(_fpsAdditionalCostApiClient);
            _sut = new AdditionalCostService(_fpsClient);
        }

        private static AdditionalCostDto BuildDto(string jobCode = "JOB001") =>
            new() { JobCode = jobCode, Account = "ACC001", Description = "Test Cost", ItemCost = 100m };

        private static List<AdditionalCostDto> BuildDtoList(string jobCode = "JOB001") =>
        [
            new() { JobCode = jobCode, Account = "ACC001", Description = "Cost A", ItemCost = 50m },
            new() { JobCode = jobCode, Account = "ACC002", Description = "Cost B", ItemCost = 75m }
        ];

        #region GetAdditionalCostsAsync Tests

        [Fact]
        public async Task GetAdditionalCostsAsync_WithSuccessResponse_ReturnsAdditionalCostList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var costs = BuildDtoList(jobCode);
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(
                costs, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _fpsAdditionalCostApiClient.GetAdditionalCostsAsync(query, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetAdditionalCostsAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsAdditionalCostApiClient.Received(1).GetAdditionalCostsAsync(query, jobCode);
        }

        [Fact]
        public async Task GetAdditionalCostsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(
                new List<AdditionalCostDto>(), new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _fpsAdditionalCostApiClient.GetAdditionalCostsAsync(query, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetAdditionalCostsAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAdditionalCostsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsAdditionalCostApiClient.GetAdditionalCostsAsync(query, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetAdditionalCostsAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetTotalItemCostAsync Tests

        [Fact]
        public async Task GetTotalItemCostAsync_WithSuccessResponse_ReturnsTotalCost()
        {
            // Arrange
            var jobCode = "JOB001";
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(250m);
            _fpsAdditionalCostApiClient.GetTotalItemCostAsync(jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetTotalItemCostAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(250m, result.Data);
            await _fpsAdditionalCostApiClient.Received(1).GetTotalItemCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalItemCostAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var jobCode = "JOB001";
            var errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());
            _fpsAdditionalCostApiClient.GetTotalItemCostAsync(jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetTotalItemCostAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetAccountCategoriesAsync Tests

        [Fact]
        public async Task GetAccountCategoriesAsync_WithSuccessResponse_ReturnsCategoryList()
        {
            // Arrange
            var categories = new List<AccountCategoryDto>
            {
                new() { AccShortName = "ACC001", AccountDescription = "Travel" },
                new() { AccShortName = "ACC002", AccountDescription = "Equipment" }
            };
            var expectedResponse = ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(categories);
            _fpsAdditionalCostApiClient.GetAccountCategoriesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsAdditionalCostApiClient.Received(1).GetAccountCategoriesAsync();
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var expectedResponse = ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsAdditionalCostApiClient.GetAccountCategoriesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidKeys_ReturnsAdditionalCost()
        {
            // Arrange
            var jobCode = "JOB001";
            var account = "ACC001";
            var description = "Test Cost";
            var dto = BuildDto(jobCode);
            var expectedResponse = ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto);
            _fpsAdditionalCostApiClient.GetByIdAsync(jobCode, account, description).Returns(expectedResponse);

            // Act
            var result = await _sut.GetByIdAsync(jobCode, account, description);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(jobCode, result.Data?.JobCode);
            await _fpsAdditionalCostApiClient.Received(1).GetByIdAsync(jobCode, account, description);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<AdditionalCostDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsAdditionalCostApiClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(expectedResponse);

            // Act
            var result = await _sut.GetByIdAsync("JOB001", "ACC001", "Missing");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAdditionalCostAsync Tests

        [Fact]
        public async Task CreateAdditionalCostAsync_WithValidDto_ReturnsCreatedRecord()
        {
            // Arrange
            var dto = BuildDto();
            var expectedResponse = ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto);
            _fpsAdditionalCostApiClient.CreateAdditionalCostAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreateAdditionalCostAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(dto.JobCode, result.Data?.JobCode);
            await _fpsAdditionalCostApiClient.Received(1).CreateAdditionalCostAsync(dto);
        }

        [Fact]
        public async Task CreateAdditionalCostAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Create failed", Code = "CREATE_ERROR" } };
            var expectedResponse = ApiResponseDto<AdditionalCostDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsAdditionalCostApiClient.CreateAdditionalCostAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreateAdditionalCostAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region UpdateAdditionalCostAsync Tests

        [Fact]
        public async Task UpdateAdditionalCostAsync_WithValidDto_ReturnsUpdatedRecord()
        {
            // Arrange
            var jobCode = "JOB001";
            var account = "ACC001";
            var dto = BuildDto(jobCode);
            var expectedResponse = ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto);
            _fpsAdditionalCostApiClient.UpdateAdditionalCostAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateAdditionalCostAsync(jobCode, account, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsAdditionalCostApiClient.Received(1).UpdateAdditionalCostAsync(dto);
        }

        [Fact]
        public async Task UpdateAdditionalCostAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<AdditionalCostDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsAdditionalCostApiClient.UpdateAdditionalCostAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateAdditionalCostAsync("JOB001", "ACC001", dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAdditionalCostAsync Tests

        [Fact]
        public async Task DeleteAdditionalCostAsync_WithValidKeys_ReturnsSuccess()
        {
            // Arrange
            var jobCode = "JOB001";
            var account = "ACC001";
            var description = "Test Cost";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsAdditionalCostApiClient.DeleteAdditionalCostAsync(jobCode, account, description).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteAdditionalCostAsync(jobCode, account, description);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsAdditionalCostApiClient.Received(1).DeleteAdditionalCostAsync(jobCode, account, description);
        }

        [Fact]
        public async Task DeleteAdditionalCostAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsAdditionalCostApiClient.DeleteAdditionalCostAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteAdditionalCostAsync("JOB001", "ACC001", "Test Cost");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion
    }
}

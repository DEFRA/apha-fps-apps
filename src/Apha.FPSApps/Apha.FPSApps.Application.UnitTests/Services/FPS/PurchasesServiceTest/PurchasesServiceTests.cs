using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.PurchasesServiceTest
{
    public class PurchasesServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsPurchasesApiClient _fpsPurchasesApiClient;
        private readonly PurchasesService _sut;

        public PurchasesServiceTests()
        {
            _fpsClient            = Substitute.For<IFpsApiClient>();
            _fpsPurchasesApiClient = Substitute.For<IFpsPurchasesApiClient>();
            _fpsClient.FpsPurchases.Returns(_fpsPurchasesApiClient);
            _sut = new PurchasesService(_fpsClient);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullFpsClient_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new PurchasesService(null!));
            Assert.Equal("fpsClient", ex.ParamName);
        }

        #endregion

        #region GetPurchasesAsync Tests

        [Fact]
        public async Task GetPurchasesAsync_WithSuccessResponse_ReturnsPurchases()
        {
            // Arrange
            var list = new List<PurchaseDto> { new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m } };
            var expectedResponse = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(list);
            _fpsPurchasesApiClient.GetPurchasesAsync("WG01", "ACC1").Returns(expectedResponse);

            // Act
            var result = await _sut.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsPurchasesApiClient.Received(1).GetPurchasesAsync("WG01", "ACC1");
        }

        [Fact]
        public async Task GetPurchasesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>());
            _fpsPurchasesApiClient.GetPurchasesAsync("WG01", "ACC1").Returns(expectedResponse);

            // Act
            var result = await _sut.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPurchasesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<PurchaseDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.GetPurchasesAsync("WG01", "ACC1").Returns(expectedResponse);

            // Act
            var result = await _sut.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetPurchaseByIdAsync Tests

        [Fact]
        public async Task GetPurchaseByIdAsync_WithSuccessResponse_ReturnsPurchase()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var expectedResponse = ApiResponseDto<PurchaseDto>.SuccessResponse(dto);
            _fpsPurchasesApiClient.GetPurchaseByIdAsync("WG01", "ACC1", "Item A").Returns(expectedResponse);

            // Act
            var result = await _sut.GetPurchaseByIdAsync("WG01", "ACC1", "Item A");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Item A", result.Data?.ItemDescription);
        }

        [Fact]
        public async Task GetPurchaseByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<PurchaseDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST").Returns(expectedResponse);

            // Act
            var result = await _sut.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreatePurchaseAsync Tests

        [Fact]
        public async Task CreatePurchaseAsync_WithSuccessResponse_ReturnsPurchase()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var expectedResponse = ApiResponseDto<PurchaseDto>.SuccessResponse(dto);
            _fpsPurchasesApiClient.CreatePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsPurchasesApiClient.Received(1).CreatePurchaseAsync(dto);
        }

        [Fact]
        public async Task CreatePurchaseAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var errors = new List<ApiErrorDto> { new() { Message = "Already exists", Code = "CONFLICT" } };
            var expectedResponse = ApiResponseDto<PurchaseDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.CreatePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdatePurchaseAsync Tests

        [Fact]
        public async Task UpdatePurchaseAsync_WithSuccessResponse_ReturnsPurchase()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, OldItemDescription = "Item A" };
            var expectedResponse = ApiResponseDto<PurchaseDto>.SuccessResponse(dto);
            _fpsPurchasesApiClient.UpdatePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsPurchasesApiClient.Received(1).UpdatePurchaseAsync(dto);
        }

        [Fact]
        public async Task UpdatePurchaseAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<PurchaseDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.UpdatePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeletePurchaseAsync Tests

        [Fact]
        public async Task DeletePurchaseAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A" };
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsPurchasesApiClient.DeletePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.DeletePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsPurchasesApiClient.Received(1).DeletePurchaseAsync(dto);
        }

        [Fact]
        public async Task DeletePurchaseAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A" };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.DeletePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.DeletePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetPurchasesPagedAsync Tests

        [Fact]
        public async Task GetPurchasesPagedAsync_WithData_ReturnsPagedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var list = new List<PurchaseDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m },
                new() { WorkGroupName = "WG01", Account = "ACC2", ItemDescription = "Item B", Amount = 200m }
            };
            var allResponse = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(list, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 });
            _fpsPurchasesApiClient.GetPurchasesPagedAsync(query, "WG01", "ACC1").Returns(allResponse);

            // Act
            var result = await _sut.GetPurchasesPagedAsync(query, "WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.NotNull(result.Pagination);
            Assert.Equal(2, result.Pagination!.TotalRecords);
            Assert.Equal(1, result.Pagination.PageNumber);
            Assert.Equal(10, result.Pagination.PageSize);
            await _fpsPurchasesApiClient.Received(1).GetPurchasesPagedAsync(query, "WG01", "ACC1");
        }

        [Fact]
        public async Task GetPurchasesPagedAsync_EmptyList_ReturnsPagedSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var allResponse = ApiResponseDto<List<PurchaseDto>>.SuccessResponse([], new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 });
            _fpsPurchasesApiClient.GetPurchasesPagedAsync(query, "WG01", "ACC1").Returns(allResponse);

            // Act
            var result = await _sut.GetPurchasesPagedAsync(query, "WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            Assert.NotNull(result.Pagination);
            Assert.Equal(0, result.Pagination!.TotalRecords);
        }

        [Fact]
        public async Task GetPurchasesPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var failResponse = ApiResponseDto<List<PurchaseDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.GetPurchasesPagedAsync(query, "WG01", "ACC1").Returns(failResponse);

            // Act
            var result = await _sut.GetPurchasesPagedAsync(query, "WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsPurchasesApiClient.Received(1).GetPurchasesPagedAsync(query, "WG01", "ACC1");
        }

        [Fact]
        public async Task GetPurchasesPagedAsync_AppliesPaging_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 1 };
            var pagedList = new List<PurchaseDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m }
            };
            var pagedResponse = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(pagedList, new PaginationDto { PageNumber = 2, PageSize = 1, TotalRecords = 2, TotalPages = 2 });
            _fpsPurchasesApiClient.GetPurchasesPagedAsync(query, "WG01", "ACC1").Returns(pagedResponse);

            // Act
            var result = await _sut.GetPurchasesPagedAsync(query, "WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal("Item B", result.Data![0].ItemDescription);
            Assert.NotNull(result.Pagination);
            Assert.Equal(2, result.Pagination!.TotalRecords);
            Assert.Equal(2, result.Pagination.PageNumber);
            Assert.Equal(1, result.Pagination.PageSize);
        }

        [Fact]
        public async Task GetPurchasesPagedAsync_AppliesFilter_ReturnsFilteredResults()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = """{"ItemDescription":"Item A"}"""
            };
            var filteredList = new List<PurchaseDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m }
            };
            var filteredResponse = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(filteredList, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });
            _fpsPurchasesApiClient.GetPurchasesPagedAsync(query, "WG01", "ACC1").Returns(filteredResponse);

            // Act
            var result = await _sut.GetPurchasesPagedAsync(query, "WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal("Item A", result.Data![0].ItemDescription);
            Assert.Equal(1, result.Pagination!.TotalRecords);
        }

        [Fact]
        public async Task GetPurchasesPagedAsync_AppliesSortAscending_ReturnsSortedResults()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "ItemDescription", Descending = false };
            var sortedList = new List<PurchaseDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m },
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m }
            };
            var sortedResponse = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(sortedList, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 });
            _fpsPurchasesApiClient.GetPurchasesPagedAsync(query, "WG01", "ACC1").Returns(sortedResponse);

            // Act
            var result = await _sut.GetPurchasesPagedAsync(query, "WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Equal("Item A", result.Data![0].ItemDescription);
            Assert.Equal("Item B", result.Data![1].ItemDescription);
        }

        [Fact]
        public async Task GetPurchasesPagedAsync_AppliesSortDescending_ReturnsSortedResults()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "ItemDescription", Descending = true };
            var sortedList = new List<PurchaseDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m },
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m }
            };
            var sortedResponse = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(sortedList, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 });
            _fpsPurchasesApiClient.GetPurchasesPagedAsync(query, "WG01", "ACC1").Returns(sortedResponse);

            // Act
            var result = await _sut.GetPurchasesPagedAsync(query, "WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Equal("Item B", result.Data![0].ItemDescription);
            Assert.Equal("Item A", result.Data![1].ItemDescription);
        }

        #endregion
    }
}

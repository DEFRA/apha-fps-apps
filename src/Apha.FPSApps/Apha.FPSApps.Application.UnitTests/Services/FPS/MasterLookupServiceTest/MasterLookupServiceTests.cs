using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.MasterLookupServiceTest
{
    public class MasterLookupServiceTests
    {
        private const string TableName = "directorate";

        private readonly IFpsApiClient _fpsApiClient;
        private readonly IFpsMasterLookupApiClient _masterLookupApiClient;
        private readonly MasterLookupService _sut;

        public MasterLookupServiceTests()
        {
            _fpsApiClient = Substitute.For<IFpsApiClient>();
            _masterLookupApiClient = Substitute.For<IFpsMasterLookupApiClient>();
            _fpsApiClient.FpsMasterLookup.Returns(_masterLookupApiClient);
            _sut = new MasterLookupService(_fpsApiClient);
        }

        private static ApiResponseDto<T> SuccessResponse<T>(T data) =>
            ApiResponseDto<T>.SuccessResponse(data);

        private static ApiResponseDto<T> FailureResponse<T>() =>
            ApiResponseDto<T>.FailureResponse(
                new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Error" } },
                new ApiMetaDto());

        private static QueryParameters<string> DefaultQuery() => new() { Page = 1, PageSize = 10 };

        #region Constructor

        [Fact]
        public void Constructor_WithNullClient_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new MasterLookupService(null!));

            Assert.Equal("fpsClient", exception.ParamName);
        }

        #endregion

        #region GetAllMasterLookupsAsync

        [Fact]
        public async Task GetAllMasterLookupsAsync_DelegatesToClient_ReturnsResponse()
        {
            // Arrange
            var expected = SuccessResponse<IEnumerable<MasterLookupDto>>(
                new List<MasterLookupDto> { new MasterLookupDto { MasterTableName = TableName } });
            _masterLookupApiClient.GetAllMasterLookupsAsync().Returns(expected);

            // Act
            var result = await _sut.GetAllMasterLookupsAsync();

            // Assert
            Assert.Same(expected, result);
            await _masterLookupApiClient.Received(1).GetAllMasterLookupsAsync();
        }

        [Fact]
        public async Task GetAllMasterLookupsAsync_ClientFailure_ReturnsFailure()
        {
            // Arrange
            _masterLookupApiClient.GetAllMasterLookupsAsync()
                .Returns(FailureResponse<IEnumerable<MasterLookupDto>>());

            // Act
            var result = await _sut.GetAllMasterLookupsAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetLookupItemsAsync

        [Fact]
        public async Task GetLookupItemsAsync_DelegatesToClient_ReturnsResponse()
        {
            // Arrange
            var expected = SuccessResponse<IEnumerable<LookupItemDto>>(
                new List<LookupItemDto> { new LookupItemDto { Value = "A" } });
            _masterLookupApiClient.GetLookupItemsAsync(TableName).Returns(expected);

            // Act
            var result = await _sut.GetLookupItemsAsync(TableName);

            // Assert
            Assert.Same(expected, result);
            await _masterLookupApiClient.Received(1).GetLookupItemsAsync(TableName);
        }

        #endregion

        #region GetLookupItemsPagedAsync

        [Fact]
        public async Task GetLookupItemsPagedAsync_DelegatesToClient_ReturnsResponse()
        {
            // Arrange
            var query = DefaultQuery();
            var expected = SuccessResponse(
                new PaginatedResult<LookupItemDto>(new List<LookupItemDto> { new LookupItemDto { Value = "A" } }, 1, 1, 10));
            _masterLookupApiClient.GetLookupItemsPagedAsync(TableName, query).Returns(expected);

            // Act
            var result = await _sut.GetLookupItemsPagedAsync(TableName, query);

            // Assert
            Assert.Same(expected, result);
            await _masterLookupApiClient.Received(1).GetLookupItemsPagedAsync(TableName, query);
        }

        #endregion

        #region CreateLookupItemAsync

        [Fact]
        public async Task CreateLookupItemAsync_DelegatesToClient_ReturnsResponse()
        {
            // Arrange
            _masterLookupApiClient.CreateLookupItemAsync(TableName, "A").Returns(SuccessResponse(true));

            // Act
            var result = await _sut.CreateLookupItemAsync(TableName, "A");

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _masterLookupApiClient.Received(1).CreateLookupItemAsync(TableName, "A");
        }

        #endregion

        #region UpdateLookupItemAsync

        [Fact]
        public async Task UpdateLookupItemAsync_DelegatesToClient_ReturnsResponse()
        {
            // Arrange
            _masterLookupApiClient.UpdateLookupItemAsync(TableName, "Old", "New").Returns(SuccessResponse(true));

            // Act
            var result = await _sut.UpdateLookupItemAsync(TableName, "Old", "New");

            // Assert
            Assert.True(result.Success);
            await _masterLookupApiClient.Received(1).UpdateLookupItemAsync(TableName, "Old", "New");
        }

        #endregion

        #region DeleteLookupItemAsync

        [Fact]
        public async Task DeleteLookupItemAsync_DelegatesToClient_ReturnsResponse()
        {
            // Arrange
            _masterLookupApiClient.DeleteLookupItemAsync(TableName, "A").Returns(SuccessResponse(true));

            // Act
            var result = await _sut.DeleteLookupItemAsync(TableName, "A");

            // Assert
            Assert.True(result.Success);
            await _masterLookupApiClient.Received(1).DeleteLookupItemAsync(TableName, "A");
        }

        [Fact]
        public async Task DeleteLookupItemAsync_ClientFailure_ReturnsFailure()
        {
            // Arrange
            _masterLookupApiClient.DeleteLookupItemAsync(TableName, "A").Returns(FailureResponse<bool>());

            // Act
            var result = await _sut.DeleteLookupItemAsync(TableName, "A");

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}

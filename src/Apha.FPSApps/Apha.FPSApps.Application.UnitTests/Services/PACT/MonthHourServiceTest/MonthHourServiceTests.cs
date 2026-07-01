using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.MonthHourServiceTest
{
    public class MonthHourServiceTests
    {
        private readonly IPactApiClient _pactApiClient;
        private readonly IPactMonthHourApiClient _pactMonthHourApiClient;
        private readonly MonthHourService _service;

        public MonthHourServiceTests()
        {
            _pactApiClient = Substitute.For<IPactApiClient>();
            _pactMonthHourApiClient = Substitute.For<IPactMonthHourApiClient>();
            _pactApiClient.PactMonthHour.Returns(_pactMonthHourApiClient);
            _service = new MonthHourService(_pactApiClient);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var monthHours = new List<MonthHourDto>
            {
                new() { Year = 2024, Month = 1, CvlHours = 160 },
                new() { Year = 2024, Month = 2, CvlHours = 152 }
            };
            var expectedResponse = ApiResponseDto<List<MonthHourDto>>.SuccessResponse(monthHours);
            _pactMonthHourApiClient.GetAllAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _pactMonthHourApiClient.Received(1).GetAllAsync(query);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyList_ReturnsSuccessResponseWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<MonthHourDto>>.SuccessResponse([]);
            _pactMonthHourApiClient.GetAllAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _pactMonthHourApiClient.GetAllAsync(query).ThrowsAsync(new Exception("API failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetAllAsync(query));
        }

        #endregion

        #region GetByYearAsync

        [Fact]
        public async Task GetByYearAsync_WithValidYear_ReturnsSuccessResponse()
        {
            // Arrange
            const short year = 2024;
            var monthHours = new List<MonthHourDto>
            {
                new() { Year = year, Month = 1, CvlHours = 160 }
            };
            var expectedResponse = ApiResponseDto<List<MonthHourDto>>.SuccessResponse(monthHours);
            _pactMonthHourApiClient.GetByYearAsync(year).Returns(expectedResponse);

            // Act
            var result = await _service.GetByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactMonthHourApiClient.Received(1).GetByYearAsync(year);
        }

        [Fact]
        public async Task GetByYearAsync_WithNoDataForYear_ReturnsSuccessResponseWithEmptyData()
        {
            // Arrange
            const short year = 1900;
            var expectedResponse = ApiResponseDto<List<MonthHourDto>>.SuccessResponse([]);
            _pactMonthHourApiClient.GetByYearAsync(year).Returns(expectedResponse);

            // Act
            var result = await _service.GetByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetByYearAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            const short year = 2024;
            _pactMonthHourApiClient.GetByYearAsync(year).ThrowsAsync(new Exception("API failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetByYearAsync(year));
        }

        #endregion

        #region GetDistinctYearsAsync

        [Fact]
        public async Task GetDistinctYearsAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var years = new List<short> { 2022, 2023, 2024 };
            var expectedResponse = ApiResponseDto<List<short>>.SuccessResponse(years);
            _pactMonthHourApiClient.GetDistinctYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
            await _pactMonthHourApiClient.Received(1).GetDistinctYearsAsync();
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithEmptyList_ReturnsSuccessResponseWithEmptyData()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<short>>.SuccessResponse([]);
            _pactMonthHourApiClient.GetDistinctYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetDistinctYearsAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            _pactMonthHourApiClient.GetDistinctYearsAsync().ThrowsAsync(new Exception("API failure"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetDistinctYearsAsync());
        }

        #endregion
    }
}

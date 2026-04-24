using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.DivisionServiceTest
{
    public class DivisionServiceTests
    {
        private readonly IFpsApiClient _mockFpsClient;
        private readonly IFpsDivisionApiClient _mockDivisionApiClient;
        private readonly DivisionService _sut;

        public DivisionServiceTests()
        {
            _mockFpsClient = Substitute.For<IFpsApiClient>();
            _mockDivisionApiClient = Substitute.For<IFpsDivisionApiClient>();
            _mockFpsClient.FpsDivision.Returns(_mockDivisionApiClient);
            _sut = new DivisionService(_mockFpsClient);
        }

        #region GetAllDivisionsAsync Tests

        [Fact]
        public async Task GetAllDivisionsAsync_ReturnsApiResponse()
        {
            // Arrange
            var divisions = new List<DivisionDto>
            {
                new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
            };
            var apiResponse = ApiResponseDto<IEnumerable<DivisionDto>>.SuccessResponse(divisions);

            _mockDivisionApiClient.GetAllDivisionsAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAllDivisionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _mockDivisionApiClient.Received(1).GetAllDivisionsAsync();
        }

        [Fact]
        public async Task GetAllDivisionsAsync_PropagatesApiErrors()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var apiResponse = ApiResponseDto<IEnumerable<DivisionDto>>.FailureResponse(errors, new ApiMetaDto());

            _mockDivisionApiClient.GetAllDivisionsAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAllDivisionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetAllDivisionsPagedAsync Tests

        [Fact]
        public async Task GetAllDivisionsPagedAsync_ReturnsPagedApiResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var divisions = new List<DivisionDto>
            {
                new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var apiResponse = ApiResponseDto<List<DivisionDto>>.SuccessResponse(divisions, pagination);

            _mockDivisionApiClient.GetAllDivisionsPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllDivisionsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.NotNull(result.Pagination);
            await _mockDivisionApiClient.Received(1).GetAllDivisionsPagedAsync(query);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_PassesFilterAndSortParameters()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "DivName",
                Descending = false,
                Filter = "{\"DivName\":\"VSD\"}"
            };
            var apiResponse = ApiResponseDto<List<DivisionDto>>.SuccessResponse(new List<DivisionDto>());

            _mockDivisionApiClient.GetAllDivisionsPagedAsync(query).Returns(apiResponse);

            // Act
            await _sut.GetAllDivisionsPagedAsync(query);

            // Assert
            await _mockDivisionApiClient.Received(1).GetAllDivisionsPagedAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 10 &&
                    q.SortBy == "DivName" &&
                    q.Descending == false &&
                    q.Filter == "{\"DivName\":\"VSD\"}"
                ));
        }

        #endregion

        #region GetDivisionByNameAsync Tests

        [Fact]
        public async Task GetDivisionByNameAsync_ReturnsApiResponse()
        {
            // Arrange
            var divName = "VSD";
            var division = new DivisionDto { DivName = divName, DivisionId = 1, AgencyId = 1 };
            var apiResponse = ApiResponseDto<DivisionDto>.SuccessResponse(division);

            _mockDivisionApiClient.GetDivisionByNameAsync(divName).Returns(apiResponse);

            // Act
            var result = await _sut.GetDivisionByNameAsync(divName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(divName, result.Data.DivName);
            await _mockDivisionApiClient.Received(1).GetDivisionByNameAsync(divName);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_ReturnsFailure_WhenNotFound()
        {
            // Arrange
            var divName = "NONEXISTENT";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }
            };
            var apiResponse = ApiResponseDto<DivisionDto>.FailureResponse(errors, new ApiMetaDto());

            _mockDivisionApiClient.GetDivisionByNameAsync(divName).Returns(apiResponse);

            // Act
            var result = await _sut.GetDivisionByNameAsync(divName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateDivisionAsync Tests

        [Fact]
        public async Task CreateDivisionAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var divisionDto = new DivisionDto { DivName = "NEW", DivisionId = 99, AgencyId = 1 };
            var apiResponse = ApiResponseDto<DivisionDto>.SuccessResponse(divisionDto);

            _mockDivisionApiClient.CreateDivisionAsync(divisionDto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateDivisionAsync(divisionDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(divisionDto.DivName, result.Data.DivName);
            await _mockDivisionApiClient.Received(1).CreateDivisionAsync(divisionDto);
        }

        [Fact]
        public async Task CreateDivisionAsync_PropagatesValidationErrors()
        {
            // Arrange
            var divisionDto = new DivisionDto { DivName = "NEW", DivisionId = 99, AgencyId = 1 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Unable to add the division name as it is already in use.", Code = "BUSINESS_LOGIC_ERROR" }
            };
            var apiResponse = ApiResponseDto<DivisionDto>.FailureResponse(errors, new ApiMetaDto());

            _mockDivisionApiClient.CreateDivisionAsync(divisionDto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateDivisionAsync(divisionDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateDivisionAsync Tests

        [Fact]
        public async Task UpdateDivisionAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var divName = "VSD";
            var divisionDto = new DivisionDto { DivName = "VSD", DivisionId = 2, AgencyId = 2 };
            var apiResponse = ApiResponseDto<DivisionDto>.SuccessResponse(divisionDto);

            _mockDivisionApiClient.UpdateDivisionAsync(divName, divisionDto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateDivisionAsync(divName, divisionDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(divisionDto.DivName, result.Data.DivName);
            await _mockDivisionApiClient.Received(1).UpdateDivisionAsync(divName, divisionDto);
        }

        [Fact]
        public async Task UpdateDivisionAsync_PropagatesFKConstraintErrors()
        {
            // Arrange
            var divName = "VSD";
            var divisionDto = new DivisionDto { DivName = "NEWNAME", DivisionId = 1, AgencyId = 1 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Unable to edit the division name as it is already in use.", Code = "BUSINESS_LOGIC_ERROR" }
            };
            var apiResponse = ApiResponseDto<DivisionDto>.FailureResponse(errors, new ApiMetaDto());

            _mockDivisionApiClient.UpdateDivisionAsync(divName, divisionDto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateDivisionAsync(divName, divisionDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteDivisionAsync Tests

        [Fact]
        public async Task DeleteDivisionAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var divName = "VSD";
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _mockDivisionApiClient.DeleteDivisionAsync(divName).Returns(apiResponse);

            // Act
            var result = await _sut.DeleteDivisionAsync(divName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _mockDivisionApiClient.Received(1).DeleteDivisionAsync(divName);
        }

        [Fact]
        public async Task DeleteDivisionAsync_PropagatesFKConstraintErrors()
        {
            // Arrange
            var divName = "VSD";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Unable to delete the division name as it is already in use.", Code = "BUSINESS_LOGIC_ERROR" }
            };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _mockDivisionApiClient.DeleteDivisionAsync(divName).Returns(apiResponse);

            // Act
            var result = await _sut.DeleteDivisionAsync(divName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllAgenciesAsync Tests

        [Fact]
        public async Task GetAllAgenciesAsync_ReturnsApiResponse()
        {
            // Arrange
            var mockAgencyClient = Substitute.For<IFpsAgencyApiClient>();
            _mockFpsClient.FpsAgency.Returns(mockAgencyClient);

            var agencies = new List<AgencyDto>
            {
                new AgencyDto { AgencyId = 1 },
                new AgencyDto { AgencyId = 2 }
            };
            var apiResponse = ApiResponseDto<IEnumerable<AgencyDto>>.SuccessResponse(agencies);

            mockAgencyClient.GetAllAgenciesAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAllAgenciesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await mockAgencyClient.Received(1).GetAllAgenciesAsync();
        }

        #endregion
    }
}

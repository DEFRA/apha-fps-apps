using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.Costbook.CostBookStaffServiceTest
{
    public class CostBookStaffServiceTests
    {
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookStaffApiClient _costBookStaffApiClient;
        private readonly CostBookStaffService _staffService;

        public CostBookStaffServiceTests()
        {
            _costBookClient = Substitute.For<ICostBookApiClient>();
            _costBookStaffApiClient = Substitute.For<ICostBookStaffApiClient>();
            _costBookClient.Staff.Returns(_costBookStaffApiClient);
            _staffService = new CostBookStaffService(_costBookClient);
        }

        #region GetAllStaffAsync Tests

        [Fact]
        public async Task GetAllStaffAsync_WithSuccessResponse_ReturnsStaffList()
        {
            // Arrange
            var staff = new List<StaffDto>
            {
                new StaffDto { Mnumber = "S001", Name = "Staff 1" },
                new StaffDto { Mnumber = "S002", Name = "Staff 2" }
            };
            var expectedResponse = ApiResponseDto<List<StaffDto>>.SuccessResponse(staff);

            _costBookStaffApiClient.GetAllStaffAsync().Returns(expectedResponse);

            // Act
            var result = await _staffService.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _costBookStaffApiClient.Received(1).GetAllStaffAsync();
        }

        [Fact]
        public async Task GetAllStaffAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<StaffDto>>.SuccessResponse(new List<StaffDto>());

            _costBookStaffApiClient.GetAllStaffAsync().Returns(expectedResponse);

            // Act
            var result = await _staffService.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllStaffAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<StaffDto>>.FailureResponse(errors, new ApiMetaDto());

            _costBookStaffApiClient.GetAllStaffAsync().Returns(expectedResponse);

            // Act
            var result = await _staffService.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            // Arrange & Act
            var service = new CostBookStaffService(_costBookClient);

            // Assert
            Assert.NotNull(service);
        }

        #endregion
    }
}

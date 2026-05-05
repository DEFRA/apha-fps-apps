using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.Costbook.CostBookCustomerServiceTest
{
    public class CostBookCustomerServiceTests
    {
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookCustomerApiClient _costBookCustomerApiClient;
        private readonly CostBookCustomerService _customerService;

        public CostBookCustomerServiceTests()
        {
            _costBookClient = Substitute.For<ICostBookApiClient>();
            _costBookCustomerApiClient = Substitute.For<ICostBookCustomerApiClient>();
            _costBookClient.Customers.Returns(_costBookCustomerApiClient);
            _customerService = new CostBookCustomerService(_costBookClient);
        }

        #region GetAllCustomersAsync Tests

        [Fact]
        public async Task GetAllCustomersAsync_WithSuccessResponse_ReturnsCustomerList()
        {
            // Arrange
            var customers = new List<CustomerDto>
            {
                new CustomerDto {CustomerName = "Customer 1" },
                new CustomerDto { CustomerName = "Customer 2" }
            };
            var expectedResponse = ApiResponseDto<List<CustomerDto>>.SuccessResponse(customers);

            _costBookCustomerApiClient.GetAllCustomersAsync().Returns(expectedResponse);

            // Act
            var result = await _customerService.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _costBookCustomerApiClient.Received(1).GetAllCustomersAsync();
        }

        [Fact]
        public async Task GetAllCustomersAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>());

            _costBookCustomerApiClient.GetAllCustomersAsync().Returns(expectedResponse);

            // Act
            var result = await _customerService.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllCustomersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<CustomerDto>>.FailureResponse(errors, new ApiMetaDto());

            _costBookCustomerApiClient.GetAllCustomersAsync().Returns(expectedResponse);

            // Act
            var result = await _customerService.GetAllCustomersAsync();

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
            var service = new CostBookCustomerService(_costBookClient);

            // Assert
            Assert.NotNull(service);
        }

        #endregion
    }
}

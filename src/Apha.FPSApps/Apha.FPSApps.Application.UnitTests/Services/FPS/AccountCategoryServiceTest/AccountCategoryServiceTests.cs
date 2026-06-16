using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.AccountCategoryServiceTest
{
    public class AccountCategoryServiceTests
    {
        private const string TestAccShortName = "TEST001";
        private const string TestAccountDescription = "Test Description";
        private const string TestFilterType = "all";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsAccountCategoryApiClient _accountCategoryClient;
        private readonly AccountCategoryService _service;

        public AccountCategoryServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _accountCategoryClient = Substitute.For<IFpsAccountCategoryApiClient>();
            _fpsClient.FpsAccountCategory.Returns(_accountCategoryClient);
            _service = new AccountCategoryService(_fpsClient);
        }

        #region GetFilteredAccountCategoriesAsync

        [Fact]
        public async Task GetFilteredAccountCategoriesAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var criteria = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = new ApiResponseDto<List<AccountCategoryDto>>
            {
                Success = true,
                Data = new List<AccountCategoryDto>
                {
                    new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription }
                }
            };

            _accountCategoryClient.GetFilteredAccountCategoriesAsync(criteria, TestFilterType)
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetFilteredAccountCategoriesAsync(criteria, TestFilterType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _accountCategoryClient.Received(1).GetFilteredAccountCategoriesAsync(criteria, TestFilterType);
        }

        [Fact]
        public async Task GetFilteredAccountCategoriesAsync_NullFilterType_PassesNullToClient()
        {
            // Arrange
            var criteria = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = new ApiResponseDto<List<AccountCategoryDto>>
            {
                Success = true,
                Data = new List<AccountCategoryDto>()
            };

            _accountCategoryClient.GetFilteredAccountCategoriesAsync(criteria, null)
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetFilteredAccountCategoriesAsync(criteria, null);

            // Assert
            Assert.NotNull(result);
            await _accountCategoryClient.Received(1).GetFilteredAccountCategoriesAsync(criteria, null);
        }

        [Fact]
        public async Task GetFilteredAccountCategoriesAsync_ApiFailure_ReturnsFailure()
        {
            // Arrange
            var criteria = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = new ApiResponseDto<List<AccountCategoryDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error" } }
            };

            _accountCategoryClient.GetFilteredAccountCategoriesAsync(criteria, TestFilterType)
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetFilteredAccountCategoriesAsync(criteria, TestFilterType);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        #endregion

        #region GetAccountCategoryByIdAsync

        [Fact]
        public async Task GetAccountCategoryByIdAsync_ExistingId_ReturnsSuccess()
        {
            // Arrange
            var expectedResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = true,
                Data = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription }
            };

            _accountCategoryClient.GetAccountCategoryByIdAsync(TestAccShortName)
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetAccountCategoryByIdAsync(TestAccShortName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(TestAccShortName, result.Data!.AccShortName);
            await _accountCategoryClient.Received(1).GetAccountCategoryByIdAsync(TestAccShortName);
        }

        [Fact]
        public async Task GetAccountCategoryByIdAsync_NonExistingId_ReturnsFailure()
        {
            // Arrange
            var expectedResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found" } }
            };

            _accountCategoryClient.GetAccountCategoryByIdAsync("NONEXISTENT")
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetAccountCategoryByIdAsync("NONEXISTENT");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAccountCategoryAsync

        [Fact]
        public async Task CreateAccountCategoryAsync_ValidDto_ReturnsSuccess()
        {
            // Arrange
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = TestAccountDescription };
            var expectedResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = true,
                Data = dto
            };

            _accountCategoryClient.CreateAccountCategoryAsync(dto)
                .Returns(expectedResponse);

            // Act
            var result = await _service.CreateAccountCategoryAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(TestAccShortName, result.Data!.AccShortName);
            await _accountCategoryClient.Received(1).CreateAccountCategoryAsync(dto);
        }

        [Fact]
        public async Task CreateAccountCategoryAsync_ApiFailure_ReturnsFailure()
        {
            // Arrange
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName };
            var expectedResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Validation error" } }
            };

            _accountCategoryClient.CreateAccountCategoryAsync(dto)
                .Returns(expectedResponse);

            // Act
            var result = await _service.CreateAccountCategoryAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        #endregion

        #region UpdateAccountCategoryAsync

        [Fact]
        public async Task UpdateAccountCategoryAsync_ValidDto_ReturnsSuccess()
        {
            // Arrange
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName, AccountDescription = "Updated" };
            var expectedResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = true,
                Data = dto
            };

            _accountCategoryClient.UpdateAccountCategoryAsync(TestAccShortName, dto)
                .Returns(expectedResponse);

            // Act
            var result = await _service.UpdateAccountCategoryAsync(TestAccShortName, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Updated", result.Data!.AccountDescription);
            await _accountCategoryClient.Received(1).UpdateAccountCategoryAsync(TestAccShortName, dto);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_UsesOriginalAccShortName()
        {
            // Arrange
            var originalId = "ORIGINAL";
            var dto = new AccountCategoryDto { AccShortName = "CHANGED", AccountDescription = "Updated" };
            var expectedResponse = new ApiResponseDto<AccountCategoryDto> { Success = true, Data = dto };

            _accountCategoryClient.UpdateAccountCategoryAsync(originalId, dto)
                .Returns(expectedResponse);

            // Act
            await _service.UpdateAccountCategoryAsync(originalId, dto);

            // Assert
            await _accountCategoryClient.Received(1).UpdateAccountCategoryAsync(originalId, dto);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_ApiFailure_ReturnsFailure()
        {
            // Arrange
            var dto = new AccountCategoryDto { AccShortName = TestAccShortName };
            var expectedResponse = new ApiResponseDto<AccountCategoryDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Update failed" } }
            };

            _accountCategoryClient.UpdateAccountCategoryAsync(TestAccShortName, dto)
                .Returns(expectedResponse);

            // Act
            var result = await _service.UpdateAccountCategoryAsync(TestAccShortName, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAccountCategoryAsync

        [Fact]
        public async Task DeleteAccountCategoryAsync_ExistingId_ReturnsSuccess()
        {
            // Arrange
            var expectedResponse = new ApiResponseDto<bool>
            {
                Success = true,
                Data = true
            };

            _accountCategoryClient.DeleteAccountCategoryAsync(TestAccShortName)
                .Returns(expectedResponse);

            // Act
            var result = await _service.DeleteAccountCategoryAsync(TestAccShortName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _accountCategoryClient.Received(1).DeleteAccountCategoryAsync(TestAccShortName);
        }

        [Fact]
        public async Task DeleteAccountCategoryAsync_ApiFailure_ReturnsFailure()
        {
            // Arrange
            var expectedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Delete failed" } }
            };

            _accountCategoryClient.DeleteAccountCategoryAsync(TestAccShortName)
                .Returns(expectedResponse);

            // Act
            var result = await _service.DeleteAccountCategoryAsync(TestAccShortName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}

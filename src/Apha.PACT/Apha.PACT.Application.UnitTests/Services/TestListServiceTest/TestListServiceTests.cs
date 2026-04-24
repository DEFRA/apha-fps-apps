using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.TestListServiceTest
{
    /// <summary>
    /// Unit tests for TestListService (Application Layer).
    /// Tests business logic, validation, and repository interaction.
    /// </summary>
    public class TestListServiceTests
    {
        private readonly ITestorProductRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly TestListService _sut;

        public TestListServiceTests()
        {
            _mockRepository = Substitute.For<ITestorProductRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new TestListService(_mockRepository, _mockMapper);
        }

        #region Constructor

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new TestListService(null!, _mockMapper));
            Assert.Equal("repository", exception.ParamName);
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new TestListService(_mockRepository, null!));
            Assert.Equal("mapper", exception.ParamName);
        }

        #endregion

        #region GetPagedTestOrProductsAsync

        [Fact]
        public async Task GetPagedTestOrProductsAsync_ValidQuery_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TestorProduct>(new List<TestorProduct>(), new PaginationData());
            var pagedResult = new PaginatedResult<TestorProductDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedTestOrProductsAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TestorProductDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetPagedTestOrProductsAsync(query);

            // Assert
            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedTestOrProductsAsync(mappedParams);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_NullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetPagedTestOrProductsAsync(null!));
            Assert.Equal("query", exception.ParamName);
            Assert.Contains("Query parameters cannot be null", exception.Message);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_RepositoryReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetPagedTestOrProductsAsync(Arg.Any<PaginationParameters<string>>()).Returns(Task.FromResult<PagedData<TestorProduct>?>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetPagedTestOrProductsAsync(query));
            Assert.Contains("Failed to retrieve paged test/product data", exception.Message);
        }

        #endregion

        #region GetTestorProductByIdAsync

        [Fact]
        public async Task GetTestorProductByIdAsync_ValidItemCode_ReturnsMappedDto()
        {
            // Arrange
            var entity = new TestorProduct { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m };

            _mockRepository.GetTestOrProductByIdAsync("TEST001").Returns(entity);
            _mockMapper.Map<TestorProductDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetTestorProductByIdAsync("TEST001");

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).GetTestOrProductByIdAsync("TEST001");
        }

        [Fact]
        public async Task GetTestorProductByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetTestOrProductByIdAsync("MISSING").Returns(Task.FromResult<TestorProduct?>(null));

            // Act
            var result = await _sut.GetTestorProductByIdAsync("MISSING");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTestorProductByIdAsync_NullItemCode_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetTestorProductByIdAsync(null!));
            Assert.Equal("itemCode", exception.ParamName);
            Assert.Contains("Item Code cannot be null or empty", exception.Message);
        }

        [Fact]
        public async Task GetTestorProductByIdAsync_EmptyItemCode_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetTestorProductByIdAsync(""));
            Assert.Equal("itemCode", exception.ParamName);
        }

        [Fact]
        public async Task GetTestorProductByIdAsync_WhitespaceItemCode_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetTestorProductByIdAsync("   "));
            Assert.Equal("itemCode", exception.ParamName);
        }

        #endregion

        #region CreateTestorProductAsync

        [Fact]
        public async Task CreateTestorProductAsync_ValidDto_ReturnsCreatedDto()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 };
            var entity = new TestorProduct { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var createdEntity = new TestorProduct { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 };
            var createdDto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 };

            _mockMapper.Map<TestorProduct>(dto).Returns(entity);
            _mockRepository.CreateTestOrProductAsync(entity).Returns(createdEntity);
            _mockMapper.Map<TestorProductDto>(createdEntity).Returns(createdDto);

            // Act
            var result = await _sut.CreateTestorProductAsync(dto);

            // Assert
            result.Should().Be(createdDto);
            await _mockRepository.Received(1).CreateTestOrProductAsync(entity);
        }

        [Fact]
        public async Task CreateTestorProductAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.CreateTestorProductAsync(null!));
            Assert.Equal("dto", exception.ParamName);
            Assert.Contains("Test/Product DTO cannot be null", exception.Message);
        }

        [Fact]
        public async Task CreateTestorProductAsync_NullItemCode_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = null!, DefraUnitPrice = 100m };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestorProductAsync(dto));
            Assert.Equal("ItemCode", exception.ParamName);
            Assert.Contains("Item Code is required", exception.Message);
        }

        [Fact]
        public async Task CreateTestorProductAsync_EmptyItemCode_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "", DefraUnitPrice = 100m };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestorProductAsync(dto));
            Assert.Equal("ItemCode", exception.ParamName);
        }

        [Fact]
        public async Task CreateTestorProductAsync_ItemCodeTooLong_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = new string('A', 21), DefraUnitPrice = 100m, FpsYear = 2024 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestorProductAsync(dto));
            Assert.Contains("Item Code cannot exceed 20 characters", exception.Message);
        }

        [Fact]
        public async Task CreateTestorProductAsync_NegativeDefraUnitPrice_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = -1m, FpsYear = 2024 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestorProductAsync(dto));
            Assert.Contains("DEFRA Unit Price cannot be negative", exception.Message);
        }

        [Fact]
        public async Task CreateTestorProductAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 1999 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestorProductAsync(dto));
            Assert.Contains("FPS Year must be between 2000 and 2100", exception.Message);
        }

        [Fact]
        public async Task CreateTestorProductAsync_RepositoryReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 };
            var entity = new TestorProduct { ItemCode = "TEST001" };
            _mockMapper.Map<TestorProduct>(dto).Returns(entity);
            _mockRepository.CreateTestOrProductAsync(entity).Returns(Task.FromResult<TestorProduct?>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.CreateTestorProductAsync(dto));
            Assert.Contains("Failed to create test/product", exception.Message);
        }

        [Fact]
        public async Task CreateTestorProductAsync_AllFieldLengthsValid_Succeeds()
        {
            // Arrange
            var dto = new TestorProductDto
            {
                ItemCode = new string('A', 20), // Max 20
                ShortDescription = new string('B', 18), // Max 18
                ItemDescription = new string('C', 200), // Max 200
                TestManager = new string('D', 50), // Max 50
                Owner = "OW", // Max 2
                JobStatus = "AC", // Max 2
                ChargeMethod = "CM", // Max 2
                DefraUnitPrice = 100m,
                FpsYear = 2024
            };
            var entity = new TestorProduct();
            var createdEntity = new TestorProduct();
            var createdDto = new TestorProductDto();

            _mockMapper.Map<TestorProduct>(dto).Returns(entity);
            _mockRepository.CreateTestOrProductAsync(entity).Returns(createdEntity);
            _mockMapper.Map<TestorProductDto>(createdEntity).Returns(createdDto);

            // Act
            var result = await _sut.CreateTestorProductAsync(dto);

            // Assert
            result.Should().Be(createdDto);
        }

        #endregion

        #region UpdateTestorProductAsync

        [Fact]
        public async Task UpdateTestorProductAsync_ValidDto_ReturnsUpdatedDto()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2024 };
            var existingEntity = new TestorProduct { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var entity = new TestorProduct { ItemCode = "TEST001", DefraUnitPrice = 150m };
            var updatedEntity = new TestorProduct { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2024 };
            var updatedDto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2024 };

            _mockRepository.GetTestOrProductByIdAsync("TEST001").Returns(existingEntity);
            _mockMapper.Map<TestorProduct>(dto).Returns(entity);
            _mockRepository.UpdateTestOrProductAsync(entity).Returns(updatedEntity);
            _mockMapper.Map<TestorProductDto>(updatedEntity).Returns(updatedDto);

            // Act
            var result = await _sut.UpdateTestorProductAsync(dto);

            // Assert
            result.Should().Be(updatedDto);
            await _mockRepository.Received(1).GetTestOrProductByIdAsync("TEST001");
            await _mockRepository.Received(1).UpdateTestOrProductAsync(entity);
        }

        [Fact]
        public async Task UpdateTestorProductAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.UpdateTestorProductAsync(null!));
            Assert.Equal("dto", exception.ParamName);
        }

        [Fact]
        public async Task UpdateTestorProductAsync_NullItemCode_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = null!, DefraUnitPrice = 100m };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.UpdateTestorProductAsync(dto));
            Assert.Equal("ItemCode", exception.ParamName);
            Assert.Contains("Item Code is required for update", exception.Message);
        }

        [Fact]
        public async Task UpdateTestorProductAsync_NonExistentItem_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "MISSING", DefraUnitPrice = 100m, FpsYear = 2024 };
            _mockRepository.GetTestOrProductByIdAsync("MISSING").Returns(Task.FromResult<TestorProduct?>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.UpdateTestorProductAsync(dto));
            Assert.Contains("MISSING", exception.Message);
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task UpdateTestorProductAsync_RepositoryReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2024 };
            var existingEntity = new TestorProduct { ItemCode = "TEST001" };
            var entity = new TestorProduct { ItemCode = "TEST001" };

            _mockRepository.GetTestOrProductByIdAsync("TEST001").Returns(existingEntity);
            _mockMapper.Map<TestorProduct>(dto).Returns(entity);
            _mockRepository.UpdateTestOrProductAsync(entity).Returns(Task.FromResult<TestorProduct?>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.UpdateTestorProductAsync(dto));
            Assert.Contains("Failed to update test/product", exception.Message);
        }

        [Fact]
        public async Task UpdateTestorProductAsync_InvalidValidation_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestorProductDto
            {
                ItemCode = "TEST001",
                DefraUnitPrice = -1m,
                FpsYear = 2024
            };
            var existingEntity = new TestorProduct { ItemCode = "TEST001" };
            _mockRepository.GetTestOrProductByIdAsync("TEST001").Returns(existingEntity);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.UpdateTestorProductAsync(dto));
            Assert.Contains("DEFRA Unit Price cannot be negative", exception.Message);
        }

        #endregion

        #region DeleteTestorProductAsync

        [Fact]
        public async Task DeleteTestorProductAsync_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var existingEntity = new TestorProduct { ItemCode = "TEST001" };
            _mockRepository.GetTestOrProductByIdAsync("TEST001").Returns(existingEntity);
            _mockRepository.DeleteTestOrProductAsync("TEST001").Returns(true);

            // Act
            var result = await _sut.DeleteTestorProductAsync("TEST001");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).GetTestOrProductByIdAsync("TEST001");
            await _mockRepository.Received(1).DeleteTestOrProductAsync("TEST001");
        }

        [Fact]
        public async Task DeleteTestorProductAsync_NullItemCode_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.DeleteTestorProductAsync(null!));
            Assert.Equal("itemCode", exception.ParamName);
        }

        [Fact]
        public async Task DeleteTestorProductAsync_EmptyItemCode_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.DeleteTestorProductAsync(""));
            Assert.Equal("itemCode", exception.ParamName);
        }

        [Fact]
        public async Task DeleteTestorProductAsync_NonExistentItem_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockRepository.GetTestOrProductByIdAsync("MISSING").Returns(Task.FromResult<TestorProduct?>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.DeleteTestorProductAsync("MISSING"));
            Assert.Contains("MISSING", exception.Message);
            Assert.Contains("not found for deletion", exception.Message);
        }

        #endregion

        #region GetOwnersAsync

        [Fact]
        public async Task GetOwnersAsync_ReturnsOwnersFromRepository()
        {
            // Arrange
            var owners = new List<string> { "OW1", "OW2", "OW3" };
            _mockRepository.GetOwnersAsync().Returns(owners);

            // Act
            var result = await _sut.GetOwnersAsync();

            // Assert
            result.Should().BeEquivalentTo(owners);
            await _mockRepository.Received(1).GetOwnersAsync();
        }

        [Fact]
        public async Task GetOwnersAsync_EmptyResult_ReturnsEmptyCollection()
        {
            // Arrange
            _mockRepository.GetOwnersAsync().Returns(new List<string>());

            // Act
            var result = await _sut.GetOwnersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetOwnersAsync_RepositoryReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockRepository.GetOwnersAsync().Returns(Task.FromResult<IEnumerable<string>?>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetOwnersAsync());
            Assert.Contains("Failed to retrieve owners", exception.Message);
        }

        #endregion

        #region Validation Tests

        [Theory]
        [InlineData("ShortDescription", 19)]
        [InlineData("ItemDescription", 201)]
        [InlineData("TestManager", 51)]
        [InlineData("Owner", 3)]
        [InlineData("JobStatus", 3)]
        [InlineData("ChargeMethod", 3)]
        public async Task CreateTestorProductAsync_FieldExceedsMaxLength_ThrowsArgumentException(string fieldName, int length)
        {
            // Arrange
            var dto = new TestorProductDto
            {
                ItemCode = "TEST001",
                DefraUnitPrice = 100m,
                FpsYear = 2024
            };

            // Set the specific field to exceed max length
            switch (fieldName)
            {
                case "ShortDescription":
                    dto.ShortDescription = new string('A', length);
                    break;
                case "ItemDescription":
                    dto.ItemDescription = new string('A', length);
                    break;
                case "TestManager":
                    dto.TestManager = new string('A', length);
                    break;
                case "Owner":
                    dto.Owner = new string('A', length);
                    break;
                case "JobStatus":
                    dto.JobStatus = new string('A', length);
                    break;
                case "ChargeMethod":
                    dto.ChargeMethod = new string('A', length);
                    break;
            }

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestorProductAsync(dto));
            Assert.Contains("cannot exceed", exception.Message);
        }

        [Fact]
        public async Task CreateTestorProductAsync_NegativeUnitPriceVla_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestorProductDto
            {
                ItemCode = "TEST001",
                DefraUnitPrice = 100m,
                UnitPriceVla = -1m,
                FpsYear = 2024
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestorProductAsync(dto));
            Assert.Contains("Unit Price VLA cannot be negative", exception.Message);
        }

        [Fact]
        public async Task CreateTestorProductAsync_NegativePriceAhvg_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestorProductDto
            {
                ItemCode = "TEST001",
                DefraUnitPrice = 100m,
                PriceAhvg = -1m,
                FpsYear = 2024
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestorProductAsync(dto));
            Assert.Contains("Price AHVG cannot be negative", exception.Message);
        }

        [Theory]
        [InlineData(1999)]
        [InlineData(2101)]
        public async Task CreateTestorProductAsync_FpsYearOutOfRange_ThrowsArgumentException(int year)
        {
            // Arrange
            var dto = new TestorProductDto
            {
                ItemCode = "TEST001",
                DefraUnitPrice = 100m,
                FpsYear = year
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestorProductAsync(dto));
            Assert.Contains("FPS Year must be between 2000 and 2100", exception.Message);
        }

        #endregion
    }
}

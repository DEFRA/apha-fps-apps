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
        private readonly ITestOrProductRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly TestListService _sut;

        public TestListServiceTests()
        {
            _mockRepository = Substitute.For<ITestOrProductRepository>();
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
            var pagedData = new PagedData<TestOrProduct>(new List<TestOrProduct>(), new PaginationData());
            var pagedResult = new PaginatedResult<TestOrProductDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedTestOrProductsAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TestOrProductDto>>(pagedData).Returns(pagedResult);

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
            _mockRepository.GetPagedTestOrProductsAsync(Arg.Any<PaginationParameters<string>>()).Returns((PagedData<TestOrProduct>?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetPagedTestOrProductsAsync(query));
            Assert.Contains("Failed to retrieve paged test/product data", exception.Message);
        }

        #endregion

        #region GetTestOrProductByIdAsync

        [Fact]
        public async Task GetTestOrProductByIdAsync_ValidItemCode_ReturnsMappedDto()
        {
            // Arrange
            var entity = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m };

            _mockRepository.GetTestOrProductByIdAsync("TEST001").Returns(entity);
            _mockMapper.Map<TestOrProductDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetTestOrProductByIdAsync("TEST001");

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).GetTestOrProductByIdAsync("TEST001");
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetTestOrProductByIdAsync("MISSING").Returns((TestOrProduct?)null);

            // Act
            var result = await _sut.GetTestOrProductByIdAsync("MISSING");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_NullItemCode_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetTestOrProductByIdAsync(null!));
            Assert.Equal("itemCode", exception.ParamName);
            Assert.Contains("Item Code cannot be null or empty", exception.Message);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_EmptyItemCode_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetTestOrProductByIdAsync(""));
            Assert.Equal("itemCode", exception.ParamName);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_WhitespaceItemCode_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetTestOrProductByIdAsync("   "));
            Assert.Equal("itemCode", exception.ParamName);
        }

        #endregion

        #region CreateTestOrProductAsync

        [Fact]
        public async Task CreateTestOrProductAsync_ValidDto_ReturnsCreatedDto()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 };
            var entity = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var createdEntity = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 };
            var createdDto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 };

            _mockMapper.Map<TestOrProduct>(dto).Returns(entity);
            _mockRepository.CreateTestOrProductAsync(entity).Returns(createdEntity);
            _mockMapper.Map<TestOrProductDto>(createdEntity).Returns(createdDto);

            // Act
            var result = await _sut.CreateTestOrProductAsync(dto);

            // Assert
            result.Should().Be(createdDto);
            await _mockRepository.Received(1).CreateTestOrProductAsync(entity);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.CreateTestOrProductAsync(null!));
            Assert.Equal("dto", exception.ParamName);
            Assert.Contains("Test/Product DTO cannot be null", exception.Message);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_NullItemCode_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = null!, DefraUnitPrice = 100m };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestOrProductAsync(dto));
            Assert.Equal("ItemCode", exception.ParamName);
            Assert.Contains("Item Code is required", exception.Message);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_EmptyItemCode_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = "", DefraUnitPrice = 100m };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestOrProductAsync(dto));
            Assert.Equal("ItemCode", exception.ParamName);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_ItemCodeTooLong_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = new string('A', 21), DefraUnitPrice = 100m, FpsYear = 2024 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestOrProductAsync(dto));
            Assert.Contains("Item Code cannot exceed 20 characters", exception.Message);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_NegativeDefraUnitPrice_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = -1m, FpsYear = 2024 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestOrProductAsync(dto));
            Assert.Contains("DEFRA Unit Price cannot be negative", exception.Message);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 1999 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestOrProductAsync(dto));
            Assert.Contains("FPS Year must be between 2000 and 2100", exception.Message);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_RepositoryReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 };
            var entity = new TestOrProduct { ItemCode = "TEST001" };

            _mockMapper.Map<TestOrProduct>(dto).Returns(entity);
            _mockRepository.CreateTestOrProductAsync(entity).Returns((TestOrProduct?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.CreateTestOrProductAsync(dto));
            Assert.Contains("Failed to create test/product", exception.Message);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_AllFieldLengthsValid_Succeeds()
        {
            // Arrange
            var dto = new TestOrProductDto
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
            var entity = new TestOrProduct();
            var createdEntity = new TestOrProduct();
            var createdDto = new TestOrProductDto();

            _mockMapper.Map<TestOrProduct>(dto).Returns(entity);
            _mockRepository.CreateTestOrProductAsync(entity).Returns(createdEntity);
            _mockMapper.Map<TestOrProductDto>(createdEntity).Returns(createdDto);

            // Act
            var result = await _sut.CreateTestOrProductAsync(dto);

            // Assert
            result.Should().Be(createdDto);
        }

        #endregion

        #region UpdateTestOrProductAsync

        [Fact]
        public async Task UpdateTestOrProductAsync_ValidDto_ReturnsUpdatedDto()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2024 };
            var existingEntity = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var entity = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 150m };
            var updatedEntity = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2024 };
            var updatedDto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2024 };

            _mockRepository.GetTestOrProductByIdAsync("TEST001").Returns(existingEntity);
            _mockMapper.Map<TestOrProduct>(dto).Returns(entity);
            _mockRepository.UpdateTestOrProductAsync(entity).Returns(updatedEntity);
            _mockMapper.Map<TestOrProductDto>(updatedEntity).Returns(updatedDto);

            // Act
            var result = await _sut.UpdateTestOrProductAsync(dto);

            // Assert
            result.Should().Be(updatedDto);
            await _mockRepository.Received(1).GetTestOrProductByIdAsync("TEST001");
            await _mockRepository.Received(1).UpdateTestOrProductAsync(entity);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.UpdateTestOrProductAsync(null!));
            Assert.Equal("dto", exception.ParamName);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_NullItemCode_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = null!, DefraUnitPrice = 100m };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.UpdateTestOrProductAsync(dto));
            Assert.Equal("ItemCode", exception.ParamName);
            Assert.Contains("Item Code is required for update", exception.Message);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_NonExistentItem_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = "MISSING", DefraUnitPrice = 100m, FpsYear = 2024 };
            _mockRepository.GetTestOrProductByIdAsync("MISSING").Returns((TestOrProduct?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.UpdateTestOrProductAsync(dto));
            Assert.Contains("MISSING", exception.Message);
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_RepositoryReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new TestOrProductDto { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2024 };
            var existingEntity = new TestOrProduct { ItemCode = "TEST001" };
            var entity = new TestOrProduct { ItemCode = "TEST001" };

            _mockRepository.GetTestOrProductByIdAsync("TEST001").Returns(existingEntity);
            _mockMapper.Map<TestOrProduct>(dto).Returns(entity);
            _mockRepository.UpdateTestOrProductAsync(entity).Returns((TestOrProduct?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.UpdateTestOrProductAsync(dto));
            Assert.Contains("Failed to update test/product", exception.Message);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_InvalidValidation_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestOrProductDto
            {
                ItemCode = "TEST001",
                DefraUnitPrice = -1m,
                FpsYear = 2024
            };
            var existingEntity = new TestOrProduct { ItemCode = "TEST001" };
            _mockRepository.GetTestOrProductByIdAsync("TEST001").Returns(existingEntity);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.UpdateTestOrProductAsync(dto));
            Assert.Contains("DEFRA Unit Price cannot be negative", exception.Message);
        }

        #endregion

        #region DeleteTestOrProductAsync

        [Fact]
        public async Task DeleteTestOrProductAsync_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var existingEntity = new TestOrProduct { ItemCode = "TEST001" };
            _mockRepository.GetTestOrProductByIdAsync("TEST001").Returns(existingEntity);
            _mockRepository.DeleteTestOrProductAsync("TEST001").Returns(true);

            // Act
            var result = await _sut.DeleteTestOrProductAsync("TEST001");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).GetTestOrProductByIdAsync("TEST001");
            await _mockRepository.Received(1).DeleteTestOrProductAsync("TEST001");
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_NullItemCode_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.DeleteTestOrProductAsync(null!));
            Assert.Equal("itemCode", exception.ParamName);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_EmptyItemCode_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.DeleteTestOrProductAsync(""));
            Assert.Equal("itemCode", exception.ParamName);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_NonExistentItem_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockRepository.GetTestOrProductByIdAsync("MISSING").Returns((TestOrProduct?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.DeleteTestOrProductAsync("MISSING"));
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
            _mockRepository.GetOwnersAsync().Returns((IEnumerable<string>?)null);

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
        public async Task CreateTestOrProductAsync_FieldExceedsMaxLength_ThrowsArgumentException(string fieldName, int length)
        {
            // Arrange
            var dto = new TestOrProductDto
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
                _sut.CreateTestOrProductAsync(dto));
            Assert.Contains("cannot exceed", exception.Message);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_NegativeUnitPriceVla_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestOrProductDto
            {
                ItemCode = "TEST001",
                DefraUnitPrice = 100m,
                UnitPriceVla = -1m,
                FpsYear = 2024
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestOrProductAsync(dto));
            Assert.Contains("Unit Price VLA cannot be negative", exception.Message);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_NegativePriceAhvg_ThrowsArgumentException()
        {
            // Arrange
            var dto = new TestOrProductDto
            {
                ItemCode = "TEST001",
                DefraUnitPrice = 100m,
                PriceAhvg = -1m,
                FpsYear = 2024
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestOrProductAsync(dto));
            Assert.Contains("Price AHVG cannot be negative", exception.Message);
        }

        [Theory]
        [InlineData(1999)]
        [InlineData(2101)]
        public async Task CreateTestOrProductAsync_FpsYearOutOfRange_ThrowsArgumentException(int year)
        {
            // Arrange
            var dto = new TestOrProductDto
            {
                ItemCode = "TEST001",
                DefraUnitPrice = 100m,
                FpsYear = year
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateTestOrProductAsync(dto));
            Assert.Contains("FPS Year must be between 2000 and 2100", exception.Message);
        }

        #endregion
    }
}

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.TestListVlaServiceTest
{
    public class TestListVlaServiceTests
    {
        private const string DefaultItemCode = "TEST001";
        private const int DefaultFpsYear = 2025;

        private readonly ITestListVlaRepository _repository;
        private readonly IMapper _mapper;
        private readonly TestListVlaService _service;

        public TestListVlaServiceTests()
        {
            _repository = Substitute.For<ITestListVlaRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new TestListVlaService(_repository, _mapper);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ValidRequest_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<TestOrProduct>
            {
                Data = new List<TestOrProduct> { CreateTestEntity() },
                PaginationData = new PaginationData { TotalRecords = 1 }
            };
            var expectedResult = new PaginatedResult<TestListVlaDto>
            {
                Data = new List<TestListVlaDto> { CreateTestDto() }
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetPagedAsync(paginationParams, DefaultFpsYear).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestListVlaDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _service.GetAllAsync(query, DefaultFpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            await _repository.Received(1).GetPagedAsync(paginationParams, DefaultFpsYear);
        }

        [Fact]
        public async Task GetAllAsync_NullQuery_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetAllAsync(null!, DefaultFpsYear));
        }

        [Fact]
        public async Task GetAllAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            var query = new QueryParameters<string>();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetAllAsync(query, 0));
        }

        #endregion

        #region GetAllByYearAsync

        [Fact]
        public async Task GetAllByYearAsync_ValidYear_ReturnsEnumerable()
        {
            // Arrange
            var entities = new List<TestOrProduct> { CreateTestEntity() };
            var dtos = new List<TestListVlaDto> { CreateTestDto() };

            _repository.GetAllByYearAsync(DefaultFpsYear).Returns(entities);
            _mapper.Map<IEnumerable<TestListVlaDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllByYearAsync(DefaultFpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllByYearAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetAllByYearAsync(-1));
        }

        [Fact]
        public async Task GetAllByYearAsync_ZeroFpsYear_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetAllByYearAsync(0));
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_ExistingRecord_ReturnsDto()
        {
            // Arrange
            var entity = CreateTestEntity();
            var dto = CreateTestDto();

            _repository.GetByKeyAsync(DefaultItemCode, DefaultFpsYear).Returns(entity);
            _mapper.Map<TestListVlaDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByKeyAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultItemCode, result!.ItemCode);
        }

        [Fact]
        public async Task GetByKeyAsync_RecordNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetByKeyAsync("NOTEXIST", DefaultFpsYear).Returns((TestOrProduct?)null);

            // Act
            var result = await _service.GetByKeyAsync("NOTEXIST", DefaultFpsYear);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WhitespaceItemCode_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByKeyAsync("   ", DefaultFpsYear));
        }

        [Fact]
        public async Task GetByKeyAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByKeyAsync(DefaultItemCode, 0));
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_CreatesSuccessfully()
        {
            // Arrange
            var dto = CreateTestDto();
            var entity = CreateTestEntity();

            _repository.ExistsAsync(DefaultItemCode, DefaultFpsYear).Returns(false);
            _mapper.Map<TestOrProduct>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(entity);
            _mapper.Map<TestListVlaDto>(entity).Returns(dto);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultItemCode, result.ItemCode);
            await _repository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_EmptyItemCode_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.ItemCode = string.Empty;
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.FpsYear = 0;
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_InvalidOwner_ThrowsArgumentException()
        {
            // Arrange
            var dto = CreateTestDto();
            dto.Owner = "XX"; // Not in PT/PA/SD/LT

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
            Assert.Contains("XX", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_DuplicatePrimaryKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = CreateTestDto();
            _repository.ExistsAsync(DefaultItemCode, DefaultFpsYear).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
            Assert.Contains("already exists", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_NullOwner_Succeeds()
        {
            // Arrange — null owner is allowed (nullable column)
            var dto = CreateTestDto();
            dto.Owner = null;
            var entity = CreateTestEntity();
            entity.Owner = null;

            _repository.ExistsAsync(DefaultItemCode, DefaultFpsYear).Returns(false);
            _mapper.Map<TestOrProduct>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(entity);
            _mapper.Map<TestListVlaDto>(entity).Returns(dto);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidDto_UpdatesSuccessfully()
        {
            // Arrange
            var dto = CreateTestDto();
            var entity = CreateTestEntity();

            _repository.GetByKeyAsync(DefaultItemCode, DefaultFpsYear).Returns(entity);
            _mapper.Map<TestOrProduct>(dto).Returns(entity);
            _repository.UpdateAsync(entity).Returns(entity);
            _mapper.Map<TestListVlaDto>(entity).Returns(dto);

            // Act
            var result = await _service.UpdateAsync(DefaultItemCode, DefaultFpsYear, dto);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateAsync(DefaultItemCode, DefaultFpsYear, null!));
        }

        [Fact]
        public async Task UpdateAsync_RouteKeyMismatch_ThrowsArgumentException()
        {
            // Arrange — dto.ItemCode differs from route itemCode
            var dto = CreateTestDto();
            dto.ItemCode = "DIFFERENT";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(DefaultItemCode, DefaultFpsYear, dto));
        }

        [Fact]
        public async Task UpdateAsync_FpsYearMismatch_ThrowsArgumentException()
        {
            // Arrange — dto.FpsYear differs from route fpsYear
            var dto = CreateTestDto();

            // Act & Assert — route year 9999 != dto year DefaultFpsYear
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(DefaultItemCode, 9999, dto));
        }

        [Fact]
        public async Task UpdateAsync_InvalidOwner_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.Owner = "ZZ";
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(DefaultItemCode, DefaultFpsYear, dto));
            Assert.Contains("ZZ", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = CreateTestDto();
            _repository.GetByKeyAsync(DefaultItemCode, DefaultFpsYear).Returns((TestOrProduct?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(DefaultItemCode, DefaultFpsYear, dto));
            Assert.Contains("not found", ex.Message);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingRecord_ReturnsTrue()
        {
            // Arrange
            _repository.DeleteAsync(DefaultItemCode, DefaultFpsYear).Returns(true);

            // Act
            var result = await _service.DeleteAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.True(result);
            await _repository.Received(1).DeleteAsync(DefaultItemCode, DefaultFpsYear);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.DeleteAsync("NOTEXIST", DefaultFpsYear).Returns(false);

            // Act
            var result = await _service.DeleteAsync("NOTEXIST", DefaultFpsYear);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhitespaceItemCode_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteAsync("  ", DefaultFpsYear));
        }

        [Fact]
        public async Task DeleteAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteAsync(DefaultItemCode, 0));
        }

        #endregion

        #region Helper Methods

        private static TestOrProduct CreateTestEntity() =>
            new()
            {
                ItemCode = DefaultItemCode,
                FpsYear = DefaultFpsYear,
                ItemDescription = "Test Description",
                Owner = "PT",
                DefraUnitPrice = 100m
            };

        private static TestListVlaDto CreateTestDto() =>
            new()
            {
                ItemCode = DefaultItemCode,
                FpsYear = DefaultFpsYear,
                ItemDescription = "Test Description",
                Owner = "PT",
                DefraUnitPrice = 100m
            };

        #endregion
    }
}

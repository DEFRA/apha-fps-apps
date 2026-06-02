using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.ProfitCentreServiceTest
{
    public class ProfitCentreServiceTests
    {
        private readonly IProfitCentreRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProfitCentreService _sut;

        public ProfitCentreServiceTests()
        {
            _mockRepository = Substitute.For<IProfitCentreRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new ProfitCentreService(_mockRepository, _mockMapper);
        }

        private static ProfitCentreDto BuildDto(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        private static ProfitCentre BuildEntity(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_WithValidData_ReturnsMappedList()
        {
            // Arrange
            var entities = new List<ProfitCentreView>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One", Division = "DIV1" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two", Division = "DIV1" }
            };
            var expected = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };

            _mockRepository.GetProfitCentresAsync().Returns(entities);
            _mockMapper.Map<List<ProfitCentreDto>>(entities).Returns(expected);

            // Act
            var result = await _sut.GetProfitCentresAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetProfitCentresAsync();
            _mockMapper.Received(1).Map<List<ProfitCentreDto>>(entities);
        }

        [Fact]
        public async Task GetProfitCentresAsync_WithEmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            var entities = new List<ProfitCentreView>();
            var expected = new List<ProfitCentreDto>();

            _mockRepository.GetProfitCentresAsync().Returns(entities);
            _mockMapper.Map<List<ProfitCentreDto>>(entities).Returns(expected);

            // Act
            var result = await _sut.GetProfitCentresAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProfitCentresAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetProfitCentresAsync()
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetProfitCentresAsync());
        }

        #endregion

        #region GetAllProfitCentresAsync Tests

        [Fact]
        public async Task GetAllProfitCentresAsync_WithValidData_ReturnsMappedEnumerable()
        {
            // Arrange
            var entities = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two", Division = "DIV1" }
            };
            var expected = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two" }
            };

            _mockRepository.GetAllProfitCentresAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<ProfitCentreDto>>(entities).Returns(expected);

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WithEmptyRepository_ReturnsEmptyEnumerable()
        {
            // Arrange
            var entities = new List<ProfitCentre>();
            var expected = new List<ProfitCentreDto>();

            _mockRepository.GetAllProfitCentresAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<ProfitCentreDto>>(entities).Returns(expected);

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllProfitCentresAsync()
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetAllProfitCentresAsync());
        }

        #endregion

        #region GetAllProfitCentresPagedAsync Tests

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetAllProfitCentresPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<ProfitCentre>
            {
                Data = new List<ProfitCentre> { BuildEntity() },
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var pagedResult = new PaginatedResult<ProfitCentreDto>
            {
                Data = new List<ProfitCentreDto> { BuildDto() },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllProfitCentresPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProfitCentreDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetAllProfitCentresPagedAsync(query);

            // Assert
            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllProfitCentresPagedAsync(mappedParams);
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ReturnsEmptyResult_WhenNoData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProfitCentre>
            {
                Data = [],
                PaginationData = new PaginationData { TotalRecords = 0 }
            };
            var emptyResult = new PaginatedResult<ProfitCentreDto>
            {
                Data = [],
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllProfitCentresPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProfitCentreDto>>(pagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetAllProfitCentresPagedAsync(query);

            // Assert
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetProfitCentreByIdAsync(""));
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_ThrowsArgumentException_WhenIdIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetProfitCentreByIdAsync("   "));
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepository.GetProfitCentreByIdAsync("NOTEXIST").Returns((ProfitCentre?)null);
            var result = await _sut.GetProfitCentreByIdAsync("NOTEXIST");
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_ReturnsMappedDto_WhenFound()
        {
            // Arrange
            var entity = BuildEntity("PC01");
            var dto    = BuildDto("PC01");

            _mockRepository.GetProfitCentreByIdAsync("PC01").Returns(entity);
            _mockMapper.Map<ProfitCentreDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetProfitCentreByIdAsync("PC01");

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).GetProfitCentreByIdAsync("PC01");
        }

        #endregion

        #region CreateProfitCentreAsync Tests

        [Fact]
        public async Task CreateProfitCentreAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateProfitCentreAsync(null!));
        }

        [Fact]
        public async Task CreateProfitCentreAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            var dto = new ProfitCentreDto { ProfitCentreId = "", ProfitCentreName = "Centre" };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateProfitCentreAsync(dto));
        }

        [Fact]
        public async Task CreateProfitCentreAsync_ThrowsArgumentException_WhenIdIsWhiteSpace()
        {
            var dto = new ProfitCentreDto { ProfitCentreId = "   ", ProfitCentreName = "Centre" };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateProfitCentreAsync(dto));
        }

        [Fact]
        public async Task CreateProfitCentreAsync_ThrowsArgumentException_WhenNameIsEmpty()
        {
            var dto = new ProfitCentreDto { ProfitCentreId = "PC01", ProfitCentreName = "" };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateProfitCentreAsync(dto));
        }

        [Fact]
        public async Task CreateProfitCentreAsync_ThrowsArgumentException_WhenNameIsWhiteSpace()
        {
            var dto = new ProfitCentreDto { ProfitCentreId = "PC01", ProfitCentreName = "   " };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateProfitCentreAsync(dto));
        }

        [Fact]
        public async Task CreateProfitCentreAsync_ThrowsInvalidOperationException_WhenAlreadyExists()
        {
            // Arrange
            var dto = BuildDto("PC01");
            _mockRepository.ProfitCentreExistsAsync("PC01").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateProfitCentreAsync(dto));
        }

        [Fact]
        public async Task CreateProfitCentreAsync_ReturnsMappedDto_WhenSuccessful()
        {
            // Arrange
            var dto     = BuildDto("PC01");
            var entity  = BuildEntity("PC01");
            var created = BuildEntity("PC01");

            _mockRepository.ProfitCentreExistsAsync("PC01").Returns(false);
            _mockMapper.Map<ProfitCentre>(dto).Returns(entity);
            _mockRepository.CreateProfitCentreAsync(entity).Returns(created);
            _mockMapper.Map<ProfitCentreDto>(created).Returns(dto);

            // Act
            var result = await _sut.CreateProfitCentreAsync(dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).CreateProfitCentreAsync(entity);
        }

        #endregion

        #region UpdateProfitCentreAsync Tests

        [Fact]
        public async Task UpdateProfitCentreAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateProfitCentreAsync("PC01", null!));
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_ThrowsArgumentException_WhenOriginalIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateProfitCentreAsync("", BuildDto()));
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_ThrowsArgumentException_WhenOriginalIdIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateProfitCentreAsync("   ", BuildDto()));
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_ReturnsMappedDto_WhenSuccessful()
        {
            // Arrange
            var dto     = BuildDto("PC01");
            var entity  = BuildEntity("PC01");
            var updated = BuildEntity("PC01");

            _mockRepository.ProfitCentreExistsAsync("PC01").Returns(true);
            _mockMapper.Map<ProfitCentre>(dto).Returns(entity);
            _mockRepository.UpdateProfitCentreAsync("PC01", entity).Returns(updated);
            _mockMapper.Map<ProfitCentreDto>(updated).Returns(dto);

            // Act
            var result = await _sut.UpdateProfitCentreAsync("PC01", dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).UpdateProfitCentreAsync("PC01", entity);
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_ThrowsInvalidOperationException_WhenNotFound()
        {
            // Arrange
            var dto = BuildDto("PC01");

            _mockRepository.ProfitCentreExistsAsync("NOTEXIST").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateProfitCentreAsync("NOTEXIST", dto));
            await _mockRepository.DidNotReceive().UpdateProfitCentreAsync(Arg.Any<string>(), Arg.Any<ProfitCentre>());
        }

        #endregion

        #region DeleteProfitCentreAsync Tests

        [Fact]
        public async Task DeleteProfitCentreAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteProfitCentreAsync(""));
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_ThrowsArgumentException_WhenIdIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteProfitCentreAsync("   "));
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_ThrowsInvalidOperationException_WhenGradeExists()
        {
            _mockRepository.HasLinkedGradesAsync("PC01").Returns(true);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteProfitCentreAsync("PC01"));
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_ThrowsInvalidOperationException_WhenWorkgroupExists()
        {
            _mockRepository.HasLinkedGradesAsync("PC01").Returns(false);
            _mockRepository.HasLinkedWorkgroupsAsync("PC01").Returns(true);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteProfitCentreAsync("PC01"));
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_ReturnsTrue_WhenDeleted()
        {
            _mockRepository.HasLinkedGradesAsync("PC01").Returns(false);
            _mockRepository.HasLinkedWorkgroupsAsync("PC01").Returns(false);
            _mockRepository.DeleteProfitCentreAsync("PC01").Returns(true);
            var result = await _sut.DeleteProfitCentreAsync("PC01");
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteProfitCentreAsync("PC01");
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_ReturnsFalse_WhenNotFound()
        {
            _mockRepository.HasLinkedGradesAsync("NOTEXIST").Returns(false);
            _mockRepository.HasLinkedWorkgroupsAsync("NOTEXIST").Returns(false);
            _mockRepository.DeleteProfitCentreAsync("NOTEXIST").Returns(false);
            var result = await _sut.DeleteProfitCentreAsync("NOTEXIST");
            result.Should().BeFalse();
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync Tests

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithValidData_ReturnsTrue()
        {
            // Arrange
            _mockRepository.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1).Returns(true);

            // Act
            var result = await _sut.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1));
        }

        #endregion

    }
}

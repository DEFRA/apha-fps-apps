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

        #region GetProfitCenterCostSummaryAsync Tests

        [Fact]
        public async Task GetProfitCenterCostSummaryAsync_WithoutMonthNumber_ReturnsAllCostData()
        {
            // Arrange
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>
            {
                ("PC01", 1000.50m),
                ("PC02", 2500.75m)
            };

            _mockRepository.GetProfitCenterCostSummaryAsync(null).Returns(repositoryData);

            // Act
            var result = await _sut.GetProfitCenterCostSummaryAsync(null);

            // Assert
            var resultList = result.ToList();
            resultList.Should().HaveCount(2);
            resultList[0].ProfitCentre.Should().Be("PC01");
            resultList[0].Cost.Should().Be(1000.50m);
            resultList[1].ProfitCentre.Should().Be("PC02");
            resultList[1].Cost.Should().Be(2500.75m);
            await _mockRepository.Received(1).GetProfitCenterCostSummaryAsync(null);
        }

        [Fact]
        public async Task GetProfitCenterCostSummaryAsync_WithMonthNumber_ReturnsFilteredCostData()
        {
            // Arrange
            const short monthNumber = 3;
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>
            {
                ("PC01", 1500.00m)
            };

            _mockRepository.GetProfitCenterCostSummaryAsync(monthNumber).Returns(repositoryData);

            // Act
            var result = await _sut.GetProfitCenterCostSummaryAsync(monthNumber);

            // Assert
            var resultList = result.ToList();
            resultList.Should().HaveCount(1);
            resultList[0].ProfitCentre.Should().Be("PC01");
            resultList[0].Cost.Should().Be(1500.00m);
            await _mockRepository.Received(1).GetProfitCenterCostSummaryAsync(monthNumber);
        }

        [Fact]
        public async Task GetProfitCenterCostSummaryAsync_WithEmptyResult_ReturnsEmptyEnumerable()
        {
            // Arrange
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>();

            _mockRepository.GetProfitCenterCostSummaryAsync(null).Returns(repositoryData);

            // Act
            var result = await _sut.GetProfitCenterCostSummaryAsync(null);

            // Assert
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetProfitCenterCostSummaryAsync(null);
        }

        [Fact]
        public async Task GetProfitCenterCostSummaryAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetProfitCenterCostSummaryAsync(Arg.Any<short?>())
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetProfitCenterCostSummaryAsync(null));
        }

        [Fact]
        public async Task GetProfitCenterCostSummaryAsync_WithZeroMonthNumber_ReturnsCostData()
        {
            // Arrange
            const short monthNumber = 0;
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>
            {
                ("PC01", 500.00m)
            };

            _mockRepository.GetProfitCenterCostSummaryAsync(monthNumber).Returns(repositoryData);

            // Act
            var result = await _sut.GetProfitCenterCostSummaryAsync(monthNumber);

            // Assert
            var resultList = result.ToList();
            resultList.Should().HaveCount(1);
            resultList[0].Cost.Should().Be(500.00m);
        }

        [Fact]
        public async Task GetProfitCenterCostSummaryAsync_WithMaxMonthNumber_ReturnsCostData()
        {
            // Arrange
            const short monthNumber = 12;
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>
            {
                ("PC01", 3000.00m),
                ("PC02", 1000.00m)
            };

            _mockRepository.GetProfitCenterCostSummaryAsync(monthNumber).Returns(repositoryData);

            // Act
            var result = await _sut.GetProfitCenterCostSummaryAsync(monthNumber);

            // Assert
            var resultList = result.ToList();
            resultList.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetProfitCenterCostSummaryAsync_MapsDataCorrectly_WithMultipleRecords()
        {
            // Arrange
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>
            {
                ("PC01", 100.00m),
                ("PC02", 200.00m),
                ("PC03", 300.00m)
            };

            _mockRepository.GetProfitCenterCostSummaryAsync(null).Returns(repositoryData);

            // Act
            var result = await _sut.GetProfitCenterCostSummaryAsync(null);

            // Assert
            var resultList = result.ToList();
            resultList.Should().HaveCount(3);
            resultList.Select(r => r.ProfitCentre).Should().Equal("PC01", "PC02", "PC03");
            resultList.Select(r => r.Cost).Should().Equal(100.00m, 200.00m, 300.00m);
        }

        #endregion

        #region GetPagedProfitCenterCostSummaryAsync Tests

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetPagedProfitCenterCostSummaryAsync(null!, null));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithoutMonthNumber_ReturnsPagedData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>
            {
                ("PC01", 1000.00m),
                ("PC02", 2000.00m)
            };
            var pagedData = new PagedData<(string ProfitCentre, decimal Cost)>
            {
                Data = repositoryData,
                PaginationData = new PaginationData
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 2,
                    TotalPages = 1
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProfitCenterCostSummaryAsync(mappedParams, null).Returns(pagedData);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, null);

            // Assert
            result.Data.Should().HaveCount(2);
            result.Data.ElementAt(0).ProfitCentre.Should().Be("PC01");
            result.Data.ElementAt(0).Cost.Should().Be(1000.00m);
            result.PaginationData.PageNumber.Should().Be(1);
            result.PaginationData.PageSize.Should().Be(10);
            result.PaginationData.TotalRecords.Should().Be(2);
            result.PaginationData.TotalPages.Should().Be(1);
            await _mockRepository.Received(1).GetPagedProfitCenterCostSummaryAsync(mappedParams, null);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMonthNumber_ReturnsFilteredPagedData()
        {
            // Arrange
            const short monthNumber = 3;
            var query = new QueryParameters<string> { Page = 1, PageSize = 5 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 5 };
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>
            {
                ("PC01", 1500.00m)
            };
            var pagedData = new PagedData<(string ProfitCentre, decimal Cost)>
            {
                Data = repositoryData,
                PaginationData = new PaginationData
                {
                    PageNumber = 1,
                    PageSize = 5,
                    TotalRecords = 1,
                    TotalPages = 1
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProfitCenterCostSummaryAsync(mappedParams, monthNumber).Returns(pagedData);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

            // Assert
            result.Data.Should().HaveCount(1);
            result.Data.First().ProfitCentre.Should().Be("PC01");
            result.Data.First().Cost.Should().Be(1500.00m);
            result.PaginationData.TotalRecords.Should().Be(1);
            await _mockRepository.Received(1).GetPagedProfitCenterCostSummaryAsync(mappedParams, monthNumber);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithEmptyResult_ReturnsEmptyPagedData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<(string ProfitCentre, decimal Cost)>
            {
                Data = [],
                PaginationData = new PaginationData
                {
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0,
                    TotalPages = 0
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProfitCenterCostSummaryAsync(mappedParams, null).Returns(pagedData);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, null);

            // Assert
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProfitCenterCostSummaryAsync(mappedParams, Arg.Any<short?>())
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetPagedProfitCenterCostSummaryAsync(query, null));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithSortingAndPaging_ReturnsSortedPagedData()
        {
            // Arrange
            const short monthNumber = 6;
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 5,
                SortBy = "ProfitCentre",
                Descending = true
            };
            var mappedParams = new PaginationParameters<string>
            {
                Page = 2,
                PageSize = 5,
                SortBy = "ProfitCentre",
                Descending = true
            };
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>
            {
                ("PC05", 500.00m),
                ("PC04", 400.00m)
            };
            var pagedData = new PagedData<(string ProfitCentre, decimal Cost)>
            {
                Data = repositoryData,
                PaginationData = new PaginationData
                {
                    PageNumber = 2,
                    PageSize = 5,
                    TotalRecords = 10,
                    TotalPages = 2
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProfitCenterCostSummaryAsync(mappedParams, monthNumber).Returns(pagedData);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

            // Assert
            result.Data.Should().HaveCount(2);
            result.PaginationData.PageNumber.Should().Be(2);
            result.PaginationData.PageSize.Should().Be(5);
            result.PaginationData.TotalRecords.Should().Be(10);
            result.PaginationData.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithLargePageNumber_ReturnsEmptyPage()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 999, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 999, PageSize = 10 };
            var pagedData = new PagedData<(string ProfitCentre, decimal Cost)>
            {
                Data = [],
                PaginationData = new PaginationData
                {
                    PageNumber = 999,
                    PageSize = 10,
                    TotalRecords = 50,
                    TotalPages = 5
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProfitCenterCostSummaryAsync(mappedParams, null).Returns(pagedData);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, null);

            // Assert
            result.Data.Should().BeEmpty();
            result.PaginationData.PageNumber.Should().Be(999);
            result.PaginationData.TotalRecords.Should().Be(50);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMinimumPageSize_ReturnsSingleItem()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 1 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 1 };
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>
            {
                ("PC01", 1000.00m)
            };
            var pagedData = new PagedData<(string ProfitCentre, decimal Cost)>
            {
                Data = repositoryData,
                PaginationData = new PaginationData
                {
                    PageNumber = 1,
                    PageSize = 1,
                    TotalRecords = 10,
                    TotalPages = 10
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProfitCenterCostSummaryAsync(mappedParams, null).Returns(pagedData);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, null);

            // Assert
            result.Data.Should().HaveCount(1);
            result.PaginationData.PageSize.Should().Be(1);
            result.PaginationData.TotalPages.Should().Be(10);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_MapsPaginationDataCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 3, PageSize = 20 };
            var mappedParams = new PaginationParameters<string> { Page = 3, PageSize = 20 };
            var repositoryData = new List<(string ProfitCentre, decimal Cost)>
            {
                ("PC01", 100.00m)
            };
            var pagedData = new PagedData<(string ProfitCentre, decimal Cost)>
            {
                Data = repositoryData,
                PaginationData = new PaginationData
                {
                    PageNumber = 3,
                    PageSize = 20,
                    TotalRecords = 55,
                    TotalPages = 3
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProfitCenterCostSummaryAsync(mappedParams, null).Returns(pagedData);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, null);

            // Assert
            result.PaginationData.PageNumber.Should().Be(3);
            result.PaginationData.PageSize.Should().Be(20);
            result.PaginationData.TotalRecords.Should().Be(55);
            result.PaginationData.TotalPages.Should().Be(3);
        }

        #endregion

    }
}

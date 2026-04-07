using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Enities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.YearMasterServiceTest
{
    public class YearMasterServiceTests
    {
        private readonly IYearMasterRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly YearMasterService _sut;

        public YearMasterServiceTests()
        {
            _mockRepository = Substitute.For<IYearMasterRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new YearMasterService(_mockRepository, _mockMapper);
        }

        #region GetAllYearMastersAsync (Non-Paginated)

        [Fact]
        public async Task GetAllYearMastersAsync_WithValidData_ReturnsYearMasterDtos()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new YearMaster { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new YearMaster { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };

            var expectedDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new YearMasterDto { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true }
            };

            _mockRepository.GetAllYearMastersAsync().Returns(yearMasters);
            _mockMapper.Map<IEnumerable<YearMasterDto>>(yearMasters).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllYearMastersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().FpsYear.Should().Be(2024);
            result.First().YearStatus.Should().Be("Open");
            result.Last().FpsYear.Should().Be(2025);

            await _mockRepository.Received(1).GetAllYearMastersAsync();
            _mockMapper.Received(1).Map<IEnumerable<YearMasterDto>>(yearMasters);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var emptyYearMasters = new List<YearMaster>();
            var emptyDtos = new List<YearMasterDto>();

            _mockRepository.GetAllYearMastersAsync().Returns(emptyYearMasters);
            _mockMapper.Map<IEnumerable<YearMasterDto>>(emptyYearMasters).Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllYearMastersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllYearMastersAsync();
            _mockMapper.Received(1).Map<IEnumerable<YearMasterDto>>(emptyYearMasters);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithSingleYear_ReturnsSingleYearMasterDto()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new YearMaster { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };

            var expectedDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };

            _mockRepository.GetAllYearMastersAsync().Returns(yearMasters);
            _mockMapper.Map<IEnumerable<YearMasterDto>>(yearMasters).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllYearMastersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainSingle();
            result.First().FpsYear.Should().Be(2024);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllYearMastersAsync()
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.GetAllYearMastersAsync()
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetAllYearMastersAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<YearMasterDto>>(Arg.Any<IEnumerable<YearMaster>>());
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithMultipleStatuses_ReturnsAllYears()
        {
            // Arrange
            var yearMasters = new List<YearMaster>
            {
                new YearMaster { FpsYear = 2023, YearStatus = "Closed", Active = true },
                new YearMaster { FpsYear = 2024, YearStatus = "Open", Active = true },
                new YearMaster { FpsYear = 2025, YearStatus = "Planned", Active = true }
            };

            var expectedDtos = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2023, YearStatus = "Closed" },
                new YearMasterDto { FpsYear = 2024, YearStatus = "Open" },
                new YearMasterDto { FpsYear = 2025, YearStatus = "Planned" }
            };

            _mockRepository.GetAllYearMastersAsync().Returns(yearMasters);
            _mockMapper.Map<IEnumerable<YearMasterDto>>(yearMasters).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllYearMastersAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(y => y.YearStatus == "Open");
            result.Should().Contain(y => y.YearStatus == "Planned");
            result.Should().Contain(y => y.YearStatus == "Closed");
        }

        #endregion

        #region GetAllYearMastersAsync (Paginated)

        [Fact]
        public async Task GetAllYearMastersAsync_Paginated_WithValidQuery_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024
            };

            var mappedPaginationParams = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024
            };

            var repositoryResult = new PagedData<YearMaster>
            {
                Data = new List<YearMaster>
                {
                    new YearMaster { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
                },
                PaginationData = new PaginationData
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 1
                }
            };

            var expectedResult = new PaginatedResult<YearMasterDto>
            {
                Data = new List<YearMasterDto>
                {
                    new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
                },
                PaginationData = new PaginationDto
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 1
                }
            };

            _mockMapper.Map<PaginationParameters<int>>(query).Returns(mappedPaginationParams);
            _mockRepository.GetAllYearMastersAsync(mappedPaginationParams).Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<YearMasterDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetAllYearMastersAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().ContainSingle();
            result.Data.First().FpsYear.Should().Be(2024);
            result.PaginationData.TotalRecords.Should().Be(1);
            result.PaginationData.PageNumber.Should().Be(1);

            _mockMapper.Received(1).Map<PaginationParameters<int>>(query);
            await _mockRepository.Received(1).GetAllYearMastersAsync(mappedPaginationParams);
            _mockMapper.Received(1).Map<PaginatedResult<YearMasterDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_Paginated_WithMultiplePages_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<int>
            {
                Page = 2,
                PageSize = 2
            };

            var mappedPaginationParams = new PaginationParameters<int>
            {
                Page = 2,
                PageSize = 2
            };

            var repositoryResult = new PagedData<YearMaster>
            {
                Data = new List<YearMaster>
                {
                    new YearMaster { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed" },
                    new YearMaster { FpsYear = 2021, FpsYearCode = "2021", YearStatus = "Closed" }
                },
                PaginationData = new PaginationData
                {
                    TotalPages = 3,
                    PageNumber = 2,
                    PageSize = 2,
                    TotalRecords = 5
                }
            };

            var expectedResult = new PaginatedResult<YearMasterDto>
            {
                Data = new List<YearMasterDto>
                {
                    new YearMasterDto { FpsYear = 2022, FpsYearCode = "2022", YearStatus = "Closed" },
                    new YearMasterDto { FpsYear = 2021, FpsYearCode = "2021", YearStatus = "Closed" }
                },
                PaginationData = new PaginationDto
                {
                    TotalPages = 3,
                    PageNumber = 2,
                    PageSize = 2,
                    TotalRecords = 5
                }
            };

            _mockMapper.Map<PaginationParameters<int>>(query).Returns(mappedPaginationParams);
            _mockRepository.GetAllYearMastersAsync(mappedPaginationParams).Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<YearMasterDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetAllYearMastersAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.PageNumber.Should().Be(2);
            result.PaginationData.TotalPages.Should().Be(3);
            result.PaginationData.TotalRecords.Should().Be(5);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_Paginated_WithFilter_ReturnsFilteredResults()
        {
            // Arrange
            var query = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024
            };

            var mappedPaginationParams = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024
            };

            var repositoryResult = new PagedData<YearMaster>
            {
                Data = new List<YearMaster>
                {
                    new YearMaster { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open" }
                },
                PaginationData = new PaginationData
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 1
                }
            };

            var expectedResult = new PaginatedResult<YearMasterDto>
            {
                Data = new List<YearMasterDto>
                {
                    new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open" }
                },
                PaginationData = new PaginationDto
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 1
                }
            };

            _mockMapper.Map<PaginationParameters<int>>(query).Returns(mappedPaginationParams);
            _mockRepository.GetAllYearMastersAsync(mappedPaginationParams).Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<YearMasterDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetAllYearMastersAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().ContainSingle();
            result.Data.First().FpsYear.Should().Be(2024);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_Paginated_WithEmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 9999
            };

            var mappedPaginationParams = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 9999
            };

            var emptyRepositoryResult = new PagedData<YearMaster>
            {
                Data = new List<YearMaster>(),
                PaginationData = new PaginationData
                {
                    TotalPages = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };

            var emptyExpectedResult = new PaginatedResult<YearMasterDto>
            {
                Data = new List<YearMasterDto>(),
                PaginationData = new PaginationDto
                {
                    TotalPages = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };

            _mockMapper.Map<PaginationParameters<int>>(query).Returns(mappedPaginationParams);
            _mockRepository.GetAllYearMastersAsync(mappedPaginationParams).Returns(emptyRepositoryResult);
            _mockMapper.Map<PaginatedResult<YearMasterDto>>(emptyRepositoryResult).Returns(emptyExpectedResult);

            // Act
            var result = await _sut.GetAllYearMastersAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            result.PaginationData.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_Paginated_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<int> { Page = 1, PageSize = 10 };
            var mappedPaginationParams = new PaginationParameters<int> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<int>>(query).Returns(mappedPaginationParams);
            _mockRepository.GetAllYearMastersAsync(mappedPaginationParams)
                .Throws(new InvalidOperationException("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.GetAllYearMastersAsync(query)
            );

            exception.Message.Should().Be("Database error");
            _mockMapper.DidNotReceive().Map<PaginatedResult<YearMasterDto>>(Arg.Any<PagedData<YearMaster>>());
        }

        [Fact]
        public async Task GetAllYearMastersAsync_Paginated_WithSorting_ReturnsSortedResults()
        {
            // Arrange
            var query = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "FpsYear",
                Descending = true
            };

            var mappedPaginationParams = new PaginationParameters<int>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "FpsYear",
                Descending = true
            };

            var repositoryResult = new PagedData<YearMaster>
            {
                Data = new List<YearMaster>
                {
                    new YearMaster { FpsYear = 2025, FpsYearCode = "2025" },
                    new YearMaster { FpsYear = 2024, FpsYearCode = "2024" },
                    new YearMaster { FpsYear = 2023, FpsYearCode = "2023" }
                },
                PaginationData = new PaginationData
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 3
                }
            };

            var expectedResult = new PaginatedResult<YearMasterDto>
            {
                Data = new List<YearMasterDto>
                {
                    new YearMasterDto { FpsYear = 2025, FpsYearCode = "2025" },
                    new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024" },
                    new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023" }
                },
                PaginationData = new PaginationDto
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 3
                }
            };

            _mockMapper.Map<PaginationParameters<int>>(query).Returns(mappedPaginationParams);
            _mockRepository.GetAllYearMastersAsync(mappedPaginationParams).Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<YearMasterDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetAllYearMastersAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(3);
            result.Data.First().FpsYear.Should().Be(2025);
            result.Data.Last().FpsYear.Should().Be(2023);
        }

        #endregion

        #region GetYearMasterByIdAsync

        [Fact]
        public async Task GetYearMasterByIdAsync_WithValidYear_ReturnsYearMasterDto()
        {
            // Arrange
            var fpsYear = 2024;
            var yearMaster = new YearMaster
            {
                FpsYear = 2024,
                FpsYearCode = "2024",
                YearStatus = "Open",
                Active = true
            };

            var expectedDto = new YearMasterDto
            {
                FpsYear = 2024,
                FpsYearCode = "2024",
                YearStatus = "Open",
                Active = true
            };

            _mockRepository.GetYearMasterByIdAsync(fpsYear).Returns(yearMaster);
            _mockMapper.Map<YearMasterDto?>(yearMaster).Returns(expectedDto);

            // Act
            var result = await _sut.GetYearMasterByIdAsync(fpsYear);

            // Assert
            result.Should().NotBeNull();
            result.FpsYear.Should().Be(2024);
            result.FpsYearCode.Should().Be("2024");
            result.YearStatus.Should().Be("Open");
            result.Active.Should().BeTrue();

            await _mockRepository.Received(1).GetYearMasterByIdAsync(fpsYear);
            _mockMapper.Received(1).Map<YearMasterDto?>(yearMaster);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WhenYearNotFound_ReturnsNull()
        {
            // Arrange
            var fpsYear = 9999;

            _mockRepository.GetYearMasterByIdAsync(fpsYear).Returns((YearMaster?)null);
            _mockMapper.Map<YearMasterDto?>((YearMaster?)null).Returns((YearMasterDto?)null);

            // Act
            var result = await _sut.GetYearMasterByIdAsync(fpsYear);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetYearMasterByIdAsync(fpsYear);
            _mockMapper.Received(1).Map<YearMasterDto?>((YearMaster?)null);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2024)]
        public async Task GetYearMasterByIdAsync_WithInvalidYear_ReturnsNull(int invalidYear)
        {
            // Arrange
            _mockRepository.GetYearMasterByIdAsync(invalidYear).Returns((YearMaster?)null);
            _mockMapper.Map<YearMasterDto?>((YearMaster?)null).Returns((YearMasterDto?)null);

            // Act
            var result = await _sut.GetYearMasterByIdAsync(invalidYear);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetYearMasterByIdAsync(invalidYear);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WithClosedYear_ReturnsYearMasterDto()
        {
            // Arrange
            var fpsYear = 2023;
            var yearMaster = new YearMaster
            {
                FpsYear = 2023,
                FpsYearCode = "2023",
                YearStatus = "Closed",
                Active = true
            };

            var expectedDto = new YearMasterDto
            {
                FpsYear = 2023,
                FpsYearCode = "2023",
                YearStatus = "Closed",
                Active = true
            };

            _mockRepository.GetYearMasterByIdAsync(fpsYear).Returns(yearMaster);
            _mockMapper.Map<YearMasterDto?>(yearMaster).Returns(expectedDto);

            // Act
            var result = await _sut.GetYearMasterByIdAsync(fpsYear);

            // Assert
            result.Should().NotBeNull();
            result.YearStatus.Should().Be("Closed");
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WithPlannedYear_ReturnsYearMasterDto()
        {
            // Arrange
            var fpsYear = 2025;
            var yearMaster = new YearMaster
            {
                FpsYear = 2025,
                FpsYearCode = "2025",
                YearStatus = "Planned",
                Active = true
            };

            var expectedDto = new YearMasterDto
            {
                FpsYear = 2025,
                FpsYearCode = "2025",
                YearStatus = "Planned",
                Active = true
            };

            _mockRepository.GetYearMasterByIdAsync(fpsYear).Returns(yearMaster);
            _mockMapper.Map<YearMasterDto?>(yearMaster).Returns(expectedDto);

            // Act
            var result = await _sut.GetYearMasterByIdAsync(fpsYear);

            // Assert
            result.Should().NotBeNull();
            result.YearStatus.Should().Be("Planned");
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WithInactiveYear_ReturnsYearMasterDto()
        {
            // Arrange
            var fpsYear = 2020;
            var yearMaster = new YearMaster
            {
                FpsYear = 2020,
                FpsYearCode = "2020",
                YearStatus = "Closed",
                Active = false
            };

            var expectedDto = new YearMasterDto
            {
                FpsYear = 2020,
                FpsYearCode = "2020",
                YearStatus = "Closed",
                Active = false
            };

            _mockRepository.GetYearMasterByIdAsync(fpsYear).Returns(yearMaster);
            _mockMapper.Map<YearMasterDto?>(yearMaster).Returns(expectedDto);

            // Act
            var result = await _sut.GetYearMasterByIdAsync(fpsYear);

            // Assert
            result.Should().NotBeNull();
            result.Active.Should().BeFalse();
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var fpsYear = 2024;
            _mockRepository.GetYearMasterByIdAsync(fpsYear)
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.GetYearMasterByIdAsync(fpsYear)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetYearMasterByIdAsync(fpsYear);
            _mockMapper.DidNotReceive().Map<YearMasterDto?>(Arg.Any<YearMaster?>());
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            var fpsYear = 2024;
            var yearMaster = new YearMaster { FpsYear = 2024 };

            _mockRepository.GetYearMasterByIdAsync(fpsYear).Returns(yearMaster);
            _mockMapper.Map<YearMasterDto?>(yearMaster)
                .Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                async () => await _sut.GetYearMasterByIdAsync(fpsYear)
            );

            exception.Message.Should().Be("Mapping failed");
            await _mockRepository.Received(1).GetYearMasterByIdAsync(fpsYear);
        }

        #endregion
    }
}

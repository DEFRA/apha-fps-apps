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

namespace Apha.PACT.Application.UnitTests.Services.MonthlyTimeServiceTest
{
    public class MonthlyTimeServiceTests
    {
        private readonly IMonthlyTimeRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly MonthlyTimeService _sut;

        public MonthlyTimeServiceTests()
        {
            _mockRepository = Substitute.For<IMonthlyTimeRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new MonthlyTimeService(_mockRepository, _mockMapper);
        }

        // ── helpers ────────────────────────────────────────────────────────────

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new() { Page = page, PageSize = pageSize };

        private static PaginationParameters<string> DefaultPaginationParameters(int page = 1, int pageSize = 10)
            => new(page: page, pageSize: pageSize);

        private static PagedData<MonthlyTimeLog> BuildPagedData(
            IEnumerable<MonthlyTimeLog> items,
            int page = 1, int pageSize = 10, int totalRecords = 0)
        {
            var list = items.ToList();
            var total = totalRecords > 0 ? totalRecords : list.Count;
            return new PagedData<MonthlyTimeLog>(
                list.AsReadOnly(),
                new PaginationData
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                });
        }

        private static PaginatedResult<MonthlyTimeLogDto> BuildPaginatedResult(
            IEnumerable<MonthlyTimeLogDto> dtos,
            int page = 1, int pageSize = 10, int totalRecords = 0)
        {
            var list = dtos.ToList();
            var total = totalRecords > 0 ? totalRecords : list.Count;
            return new PaginatedResult<MonthlyTimeLogDto>(
                list,
                new PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                });
        }

        // ── SearchAsync — happy path ────────────────────────────────────────────

        #region SearchAsync — happy path

        [Fact]
        public async Task SearchAsync_WithNoFilters_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var entities = new List<MonthlyTimeLog>
            {
                new() { SequenceNo = 1, PactStaffId = "S001", TimeCode = "TC1", WorkGroup = "WG1" },
                new() { SequenceNo = 2, PactStaffId = "S002", TimeCode = "TC2", WorkGroup = "WG2" }
            };
            var dtos = new List<MonthlyTimeLogDto>
            {
                new() { SequenceNo = 1, PactStaffId = "S001", TimeCode = "TC1", WorkGroup = "WG1" },
                new() { SequenceNo = 2, PactStaffId = "S002", TimeCode = "TC2", WorkGroup = "WG2" }
            };
            var pagedData = BuildPagedData(entities);
            var expectedResult = BuildPaginatedResult(dtos);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, null, null, null, null, null, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1)
                .SearchAsync(paginationParams, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_PassesFiltersToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var dateImported = new DateTime(2024, 6, 1);
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, "WG1", "TC1", "S001", "PP1", dateImported, 6, "USER1", "I")
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, "WG1", "TC1", "S001", "PP1", dateImported, 6, "USER1", "I");

            // Assert
            await _mockRepository.Received(1)
                .SearchAsync(paginationParams, "WG1", "TC1", "S001", "PP1", dateImported, 6, "USER1", "I");
        }

        #endregion

        #region SearchAsync — mapper verification

        [Fact]
        public async Task SearchAsync_MapsQueryParametersToPaginationParameters()
        {
            // Arrange
            var query = DefaultQuery(page: 2, pageSize: 5);
            var paginationParams = DefaultPaginationParameters(page: 2, pageSize: 5);
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.SearchAsync(query, null, null, null, null, null, null, null, null);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
        }

        [Fact]
        public async Task SearchAsync_MapsPagedDataToPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.SearchAsync(query, null, null, null, null, null, null, null, null);

            // Assert
            _mockMapper.Received(1).Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData);
        }

        #endregion

        #region SearchAsync — empty results

        [Fact]
        public async Task SearchAsync_WhenRepositoryReturnsEmpty_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, null, null, null, null, null, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region SearchAsync — pagination metadata

        [Fact]
        public async Task SearchAsync_ReturnsPaginationMetadataFromMappedResult()
        {
            // Arrange
            var query = DefaultQuery(page: 2, pageSize: 3);
            var paginationParams = DefaultPaginationParameters(page: 2, pageSize: 3);
            var entities = new List<MonthlyTimeLog>
            {
                new() { SequenceNo = 4 },
                new() { SequenceNo = 5 },
                new() { SequenceNo = 6 }
            };
            var dtos = entities.Select(e => new MonthlyTimeLogDto { SequenceNo = e.SequenceNo }).ToList();
            var pagedData = BuildPagedData(entities, page: 2, pageSize: 3, totalRecords: 10);
            var expectedResult = BuildPaginatedResult(dtos, page: 2, pageSize: 3, totalRecords: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, null, null, null, null, null, null, null, null);

            // Assert
            result.PaginationData.PageNumber.Should().Be(2);
            result.PaginationData.PageSize.Should().Be(3);
            result.PaginationData.TotalRecords.Should().Be(10);
            result.PaginationData.TotalPages.Should().Be(4);
        }

        #endregion

        #region SearchAsync — individual filter delegation

        [Fact]
        public async Task SearchAsync_WorkGroupFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, "WG1", null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, "WG1", null, null, null, null, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .SearchAsync(paginationParams, "WG1", null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task SearchAsync_TimeCodeFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, "TC1", null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, null, "TC1", null, null, null, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .SearchAsync(paginationParams, null, "TC1", null, null, null, null, null, null);
        }

        [Fact]
        public async Task SearchAsync_PactStaffIdFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, "S001", null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, null, null, "S001", null, null, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .SearchAsync(paginationParams, null, null, "S001", null, null, null, null, null);
        }

        [Fact]
        public async Task SearchAsync_ParentProjectFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, null, "PP1", null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, null, null, null, "PP1", null, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .SearchAsync(paginationParams, null, null, null, "PP1", null, null, null, null);
        }

        [Fact]
        public async Task SearchAsync_DateImportedFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var dateImported = new DateTime(2024, 3, 15);
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, null, null, dateImported, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, null, null, null, null, dateImported, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .SearchAsync(paginationParams, null, null, null, null, dateImported, null, null, null);
        }

        [Fact]
        public async Task SearchAsync_MonthFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, null, null, null, 6, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, null, null, null, null, null, 6, null, null);

            // Assert
            await _mockRepository.Received(1)
                .SearchAsync(paginationParams, null, null, null, null, null, 6, null, null);
        }

        [Fact]
        public async Task SearchAsync_UserIdFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, null, null, null, null, "USER1", null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, null, null, null, null, null, null, "USER1", null);

            // Assert
            await _mockRepository.Received(1)
                .SearchAsync(paginationParams, null, null, null, null, null, null, "USER1", null);
        }

        [Fact]
        public async Task SearchAsync_InsertDeleteFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(paginationParams, null, null, null, null, null, null, null, "D")
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, null, null, null, null, null, null, null, "D");

            // Assert
            await _mockRepository.Received(1)
                .SearchAsync(paginationParams, null, null, null, null, null, null, null, "D");
        }

        #endregion

        #region SearchAsync — exception propagation

        [Fact]
        public async Task SearchAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchAsync(Arg.Any<PaginationParameters<string>>(),
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                    Arg.Any<DateTime?>(), Arg.Any<double?>(), Arg.Any<string?>(), Arg.Any<string?>())
                .ThrowsAsync(new InvalidOperationException("Repository failure"));

            // Act
            var act = async () => await _sut.SearchAsync(query, null, null, null, null, null, null, null, null);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Repository failure");
        }

        #endregion
    }
}

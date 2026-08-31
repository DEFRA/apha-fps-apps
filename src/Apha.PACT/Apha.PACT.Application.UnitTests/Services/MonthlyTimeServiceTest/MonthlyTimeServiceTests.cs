using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Application.Validation;
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
        private readonly ICalenderMonthRepository _mockCalenderMonthRepository;
        private readonly IWorkGroupRepository _mockWorkGroupRepository;
        private readonly ITimeCodeValidRepository _mockTimeCodeValidRepository;
        private readonly MonthlyTimeService _sut;

        public MonthlyTimeServiceTests()
        {
            _mockRepository = Substitute.For<IMonthlyTimeRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockCalenderMonthRepository = Substitute.For<ICalenderMonthRepository>();
            _mockWorkGroupRepository = Substitute.For<IWorkGroupRepository>();
            _mockTimeCodeValidRepository = Substitute.For<ITimeCodeValidRepository>();
            _sut = new MonthlyTimeService(
                _mockRepository,
                _mockMapper,
                _mockCalenderMonthRepository,
                _mockWorkGroupRepository,
                _mockTimeCodeValidRepository);
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
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();
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
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_PassesFiltersToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var dateImported = new DateTime(2024, 6, 1);
            var filterDto = new MonthlyTimeLogFilterDto
            {
                WorkGroup = "WG1", TimeCode = "TC1", PactStaffId = "S001",
                ParentProject = "PP1", DateImported = dateImported, Month = 6,
                UserId = "USER1", InsertDelete = "I"
            };
            var coreFilter = new MonthlyTimeLogFilter
            {
                WorkGroup = "WG1", TimeCode = "TC1", PactStaffId = "S001",
                ParentProject = "PP1", DateImported = dateImported, Month = 6,
                UserId = "USER1", InsertDelete = "I"
            };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        #endregion

        #region SearchAsync — mapper verification

        [Fact]
        public async Task SearchAsync_MapsQueryParametersToPaginationParameters()
        {
            // Arrange
            var query = DefaultQuery(page: 2, pageSize: 5);
            var paginationParams = DefaultPaginationParameters(page: 2, pageSize: 5);
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.SearchAsync(query, filterDto);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
        }

        [Fact]
        public async Task SearchAsync_MapsFilterDtoToCoreFilter()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var coreFilter = new MonthlyTimeLogFilter { WorkGroup = "WG1" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.SearchAsync(query, filterDto);

            // Assert
            _mockMapper.Received(1).Map<MonthlyTimeLogFilter>(filterDto);
        }

        [Fact]
        public async Task SearchAsync_MapsPagedDataToPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.SearchAsync(query, filterDto);

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
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

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
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();
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
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

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
            var filterDto = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var coreFilter = new MonthlyTimeLogFilter { WorkGroup = "WG1" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_TimeCodeFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { TimeCode = "TC1" };
            var coreFilter = new MonthlyTimeLogFilter { TimeCode = "TC1" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_PactStaffIdFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { PactStaffId = "S001" };
            var coreFilter = new MonthlyTimeLogFilter { PactStaffId = "S001" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_ParentProjectFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { ParentProject = "PP1" };
            var coreFilter = new MonthlyTimeLogFilter { ParentProject = "PP1" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_DateImportedFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var dateImported = new DateTime(2024, 3, 15);
            var filterDto = new MonthlyTimeLogFilterDto { DateImported = dateImported };
            var coreFilter = new MonthlyTimeLogFilter { DateImported = dateImported };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_MonthFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { Month = 6 };
            var coreFilter = new MonthlyTimeLogFilter { Month = 6 };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_UserIdFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { UserId = "USER1" };
            var coreFilter = new MonthlyTimeLogFilter { UserId = "USER1" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_InsertDeleteFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { InsertDelete = "D" };
            var coreFilter = new MonthlyTimeLogFilter { InsertDelete = "D" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        #endregion

        #region SearchAsync — exception propagation

        [Fact]
        public async Task SearchAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<MonthlyTimeLogFilter>())
                .ThrowsAsync(new InvalidOperationException("Repository failure"));

            // Act
            var act = async () => await _sut.SearchAsync(query, filterDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Repository failure");
        }

        #endregion

        #region Live and Staging Operations

        [Fact]
        public async Task SearchLiveAsync_WithFilters_MapsAndDelegatesToRepository()
        {
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var paged = new PagedData<MonthlyTimeStaff>([], new PaginationData());
            var expected = new PaginatedResult<MonthlyTimeDto>([], new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchLiveAsync(paginationParams, "WG1", "TC1", "S1", "PP1", 6).Returns(paged);
            _mockMapper.Map<PaginatedResult<MonthlyTimeDto>>(paged).Returns(expected);

            var result = await _sut.SearchLiveAsync(query, "WG1", "TC1", "S1", "PP1", 6);

            result.Should().BeSameAs(expected);
            await _mockRepository.Received(1).SearchLiveAsync(paginationParams, "WG1", "TC1", "S1", "PP1", 6);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenEntityExists_ReturnsMappedDto()
        {
            var entity = new MonthlyTime { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1" };
            var dto = new MonthlyTimeDto { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1" };

            _mockRepository.GetLiveByKeyAsync("S1", "TC1", 6, "PP1").Returns(entity);
            _mockMapper.Map<MonthlyTimeDto>(entity).Returns(dto);

            var result = await _sut.GetLiveByKeyAsync("S1", "TC1", 6, "PP1");

            result.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenEntityMissing_ReturnsNull()
        {
            _mockRepository.GetLiveByKeyAsync("S1", "TC1", 6, "PP1").Returns((MonthlyTime?)null);

            var result = await _sut.GetLiveByKeyAsync("S1", "TC1", 6, "PP1");

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteLiveAsync_DelegatesToRepository()
        {
            _mockRepository.DeleteLiveAsync("S1", "TC1", 6, "PP1").Returns(true);

            var result = await _sut.DeleteLiveAsync("S1", "TC1", 6, "PP1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteLiveAsync("S1", "TC1", 6, "PP1");
        }

        [Fact]
        public async Task SearchStagingAsync_WithFilters_MapsAndDelegatesToRepository()
        {
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var paged = new PagedData<StagingMonthlyTime>([], new PaginationData());
            var expected = new PaginatedResult<StagingMonthlyTimeDto>([], new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchStagingAsync(paginationParams, "user1", false).Returns(paged);
            _mockMapper.Map<PaginatedResult<StagingMonthlyTimeDto>>(paged).Returns(expected);

            var result = await _sut.SearchStagingAsync(query, "user1", false);

            result.Should().BeSameAs(expected);
            await _mockRepository.Received(1).SearchStagingAsync(paginationParams, "user1", false);
        }

        #endregion

        #region Constructor — null guard tests

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyTimeService(null!, _mockMapper, _mockCalenderMonthRepository, _mockWorkGroupRepository, _mockTimeCodeValidRepository);
            act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyTimeService(_mockRepository, null!, _mockCalenderMonthRepository, _mockWorkGroupRepository, _mockTimeCodeValidRepository);
            act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
        }

        [Fact]
        public void Constructor_NullCalenderMonthRepository_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyTimeService(_mockRepository, _mockMapper, null!, _mockWorkGroupRepository, _mockTimeCodeValidRepository);
            act.Should().Throw<ArgumentNullException>().WithParameterName("calenderMonthRepository");
        }

        [Fact]
        public void Constructor_NullWorkGroupRepository_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyTimeService(_mockRepository, _mockMapper, _mockCalenderMonthRepository, null!, _mockTimeCodeValidRepository);
            act.Should().Throw<ArgumentNullException>().WithParameterName("workGroupRepository");
        }

        [Fact]
        public void Constructor_NullTimeCodeValidRepository_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyTimeService(_mockRepository, _mockMapper, _mockCalenderMonthRepository, _mockWorkGroupRepository, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("timeCodeValidRepository");
        }

        #endregion

        #region UpdateLiveAsync

        [Fact]
        public async Task UpdateLiveAsync_WithOriginalPactStaffId_UsesOriginal()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S1", OriginalPactStaffId = "S_ORIG", TimeCode = "TC1", Month = 6, ParentProject = "PP1" };
            var entity = new MonthlyTime { PactStaffId = "S1" };
            var updatedEntity = new MonthlyTime { PactStaffId = "S1" };
            var resultDto = new MonthlyTimeDto { PactStaffId = "S1" };

            _mockMapper.Map<MonthlyTime>(dto).Returns(entity);
            _mockRepository.UpdateLiveAsync(entity, "S_ORIG").Returns(updatedEntity);
            _mockMapper.Map<MonthlyTimeDto>(updatedEntity).Returns(resultDto);

            var result = await _sut.UpdateLiveAsync(dto);

            result.Should().BeSameAs(resultDto);
            await _mockRepository.Received(1).UpdateLiveAsync(entity, "S_ORIG");
        }

        [Fact]
        public async Task UpdateLiveAsync_WithEmptyOriginalPactStaffId_UsesPactStaffId()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S1", OriginalPactStaffId = "", TimeCode = "TC1", Month = 6, ParentProject = "PP1" };
            var entity = new MonthlyTime { PactStaffId = "S1" };
            var updatedEntity = new MonthlyTime { PactStaffId = "S1" };
            var resultDto = new MonthlyTimeDto { PactStaffId = "S1" };

            _mockMapper.Map<MonthlyTime>(dto).Returns(entity);
            _mockRepository.UpdateLiveAsync(entity, "S1").Returns(updatedEntity);
            _mockMapper.Map<MonthlyTimeDto>(updatedEntity).Returns(resultDto);

            var result = await _sut.UpdateLiveAsync(dto);

            result.Should().BeSameAs(resultDto);
            await _mockRepository.Received(1).UpdateLiveAsync(entity, "S1");
        }

        [Fact]
        public async Task UpdateLiveAsync_WithNullOriginalPactStaffId_UsesPactStaffId()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S1", OriginalPactStaffId = null, TimeCode = "TC1", Month = 6, ParentProject = "PP1" };
            var entity = new MonthlyTime { PactStaffId = "S1" };
            var updatedEntity = new MonthlyTime { PactStaffId = "S1" };
            var resultDto = new MonthlyTimeDto { PactStaffId = "S1" };

            _mockMapper.Map<MonthlyTime>(dto).Returns(entity);
            _mockRepository.UpdateLiveAsync(entity, "S1").Returns(updatedEntity);
            _mockMapper.Map<MonthlyTimeDto>(updatedEntity).Returns(resultDto);

            var result = await _sut.UpdateLiveAsync(dto);

            await _mockRepository.Received(1).UpdateLiveAsync(entity, "S1");
        }

        #endregion

        #region GetStagingByIdAsync

        [Fact]
        public async Task GetStagingByIdAsync_WhenEntityExists_ReturnsMappedDto()
        {
            var entity = new StagingMonthlyTime { Id = 1, PactStaffId = "S1" };
            var dto = new StagingMonthlyTimeDto { Id = 1, PactStaffId = "S1" };

            _mockRepository.GetStagingByIdAsync(1, "user1").Returns(entity);
            _mockMapper.Map<StagingMonthlyTimeDto>(entity).Returns(dto);

            var result = await _sut.GetStagingByIdAsync(1, "user1");

            result.Should().BeSameAs(dto);
        }

        [Fact]
        public async Task GetStagingByIdAsync_WhenEntityMissing_ReturnsNull()
        {
            _mockRepository.GetStagingByIdAsync(1, "user1").Returns((StagingMonthlyTime?)null);

            var result = await _sut.GetStagingByIdAsync(1, "user1");

            result.Should().BeNull();
        }

        #endregion

        #region CreateStagingAsync

        [Fact]
        public async Task CreateStagingAsync_SetsImportedByAndDate_DelegatesToRepository()
        {
            var dto = new StagingMonthlyTimeDto { PactStaffId = "S1" };
            var entity = new StagingMonthlyTime { PactStaffId = "S1" };
            var createdEntity = new StagingMonthlyTime { Id = 1, PactStaffId = "S1", ImportedBy = "user1" };
            var resultDto = new StagingMonthlyTimeDto { Id = 1, PactStaffId = "S1" };

            _mockMapper.Map<StagingMonthlyTime>(dto).Returns(entity);
            _mockRepository.CreateStagingAsync(entity).Returns(createdEntity);
            _mockMapper.Map<StagingMonthlyTimeDto>(createdEntity).Returns(resultDto);

            var result = await _sut.CreateStagingAsync(dto, "user1");

            result.Should().BeSameAs(resultDto);
            entity.ImportedBy.Should().Be("user1");
            entity.ImportedDate.Value.Kind.Should().Be(DateTimeKind.Unspecified);
        }

        #endregion

        #region UpdateStagingAsync

        [Fact]
        public async Task UpdateStagingAsync_SetsImportedDate_DelegatesToRepository()
        {
            var dto = new StagingMonthlyTimeDto { Id = 1, PactStaffId = "S1" };
            var entity = new StagingMonthlyTime { Id = 1, PactStaffId = "S1" };
            var updatedEntity = new StagingMonthlyTime { Id = 1, PactStaffId = "S1" };
            var resultDto = new StagingMonthlyTimeDto { Id = 1, PactStaffId = "S1" };

            _mockMapper.Map<StagingMonthlyTime>(dto).Returns(entity);
            _mockRepository.UpdateStagingAsync(entity, "user1").Returns(updatedEntity);
            _mockMapper.Map<StagingMonthlyTimeDto>(updatedEntity).Returns(resultDto);

            var result = await _sut.UpdateStagingAsync(dto, "user1");

            result.Should().BeSameAs(resultDto);
            entity.ImportedDate.Value.Kind.Should().Be(DateTimeKind.Unspecified);
        }

        #endregion

        #region BulkUpdateStagingNamesAsync

        [Fact]
        public async Task BulkUpdateStagingNamesAsync_WhenOriginalWorkGroupEmpty_ReturnsZero()
        {
            var request = new BulkUpdateStagingMonthlyTimeNamesDto { OriginalWorkGroup = "", OriginalPactStaffId = "S1" };

            var result = await _sut.BulkUpdateStagingNamesAsync(request, "user1");

            result.UpdatedCount.Should().Be(0);
            await _mockRepository.DidNotReceiveWithAnyArgs().BulkUpdateStagingNamesAsync(default!, default!, default!, default, default, default, default);
        }

        [Fact]
        public async Task BulkUpdateStagingNamesAsync_WhenOriginalPactStaffIdEmpty_ReturnsZero()
        {
            var request = new BulkUpdateStagingMonthlyTimeNamesDto { OriginalWorkGroup = "WG1", OriginalPactStaffId = "" };

            var result = await _sut.BulkUpdateStagingNamesAsync(request, "user1");

            result.UpdatedCount.Should().Be(0);
        }

        [Fact]
        public async Task BulkUpdateStagingNamesAsync_WithValidRequest_DelegatesToRepository()
        {
            var request = new BulkUpdateStagingMonthlyTimeNamesDto
            {
                OriginalWorkGroup = "WG1",
                OriginalPactStaffId = "S1",
                NewName = "New Name",
                NewPactStaffId = "S2",
                NewPactId = "P2",
                ExcludeId = 5
            };

            _mockRepository.BulkUpdateStagingNamesAsync("user1", "WG1", "S1", "New Name", "S2", "P2", 5).Returns(3);

            var result = await _sut.BulkUpdateStagingNamesAsync(request, "user1");

            result.UpdatedCount.Should().Be(3);
        }

        #endregion

        #region DeleteStagingAsync

        [Fact]
        public async Task DeleteStagingAsync_DelegatesToRepository()
        {
            _mockRepository.DeleteStagingAsync(1, "user1").Returns(true);

            var result = await _sut.DeleteStagingAsync(1, "user1");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteStagingAsync_WhenNotFound_ReturnsFalse()
        {
            _mockRepository.DeleteStagingAsync(99, "user1").Returns(false);

            var result = await _sut.DeleteStagingAsync(99, "user1");

            result.Should().BeFalse();
        }

        #endregion

        #region DeleteAllStagingByUserAsync

        [Fact]
        public async Task DeleteAllStagingByUserAsync_DelegatesToRepository()
        {
            _mockRepository.DeleteAllStagingByUserAsync("user1").Returns(5);

            var result = await _sut.DeleteAllStagingByUserAsync("user1");

            result.Should().Be(5);
        }

        #endregion

        #region DeleteFailedStagingByUserAsync

        [Fact]
        public async Task DeleteFailedStagingByUserAsync_DelegatesToRepository()
        {
            _mockRepository.DeleteFailedStagingByUserAsync("user1").Returns(3);

            var result = await _sut.DeleteFailedStagingByUserAsync("user1");

            result.Should().Be(3);
        }

        #endregion

        #region ImportStagingAsync — ImportType 4

        [Fact]
        public async Task ImportStagingAsync_ImportType4_WithExistingRows_UpdatesAndInserts()
        {
            var existingEntity = new StagingMonthlyTime { Id = 1, PactStaffId = "OLD" };
            _mockRepository.GetStagingByIdAsync(1, "user1").Returns(existingEntity);
            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyTime>>()).Returns(1);

            var request = new MonthlyTimeImportDto
            {
                ImportType = 4,
                FileName = "file.xlsx",
                Rows =
                [
                    new MonthlyTimeImportRowDto { Id = 1, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = "6", WorkGroup = "WG1", Hours = "10", Name = "Name1" },
                    new MonthlyTimeImportRowDto { Id = 0, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = "7", WorkGroup = "WG2", Hours = "20", Name = "Name2" }
                ]
            };

            var result = await _sut.ImportStagingAsync(request, "user1");

            result.ImportedCount.Should().Be(2);
            result.Message.Should().Contain("2 rows processed");
            await _mockRepository.Received(1).UpdateStagingRecordsAsync(Arg.Is<List<StagingMonthlyTime>>(l => l.Count == 1));
            await _mockRepository.Received(1).ImportStagingAsync(Arg.Is<List<StagingMonthlyTime>>(l => l.Count == 1));
        }

        [Fact]
        public async Task ImportStagingAsync_ImportType4_WithIdGreaterThanZero_ButNotFound_InsertsInstead()
        {
            _mockRepository.GetStagingByIdAsync(99, "user1").Returns((StagingMonthlyTime?)null);
            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyTime>>()).Returns(1);

            var request = new MonthlyTimeImportDto
            {
                ImportType = 4,
                FileName = "file.xlsx",
                Rows = [new MonthlyTimeImportRowDto { Id = 99, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = "6", WorkGroup = "WG1", Hours = "10", Name = "N" }]
            };

            var result = await _sut.ImportStagingAsync(request, "user1");

            result.ImportedCount.Should().Be(1);
            await _mockRepository.DidNotReceive().UpdateStagingRecordsAsync(Arg.Any<List<StagingMonthlyTime>>());
        }

        [Fact]
        public async Task ImportStagingAsync_ImportType4_NoRowsToUpdate_SkipsUpdateCall()
        {
            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyTime>>()).Returns(2);

            var request = new MonthlyTimeImportDto
            {
                ImportType = 4,
                FileName = "file.xlsx",
                Rows =
                [
                    new MonthlyTimeImportRowDto { Id = 0, PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = "6", WorkGroup = "WG1", Hours = "10", Name = "N1" },
                    new MonthlyTimeImportRowDto { Id = 0, PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = "7", WorkGroup = "WG2", Hours = "20", Name = "N2" }
                ]
            };

            var result = await _sut.ImportStagingAsync(request, "user1");

            result.ImportedCount.Should().Be(2);
            await _mockRepository.DidNotReceive().UpdateStagingRecordsAsync(Arg.Any<List<StagingMonthlyTime>>());
        }

        #endregion

        #region ImportStagingAsync — non ImportType 4

        [Fact]
        public async Task ImportStagingAsync_NonType4_MapsRowsAndImports()
        {
            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyTime>>()).Returns(3);

            var request = new MonthlyTimeImportDto
            {
                ImportType = 1,
                FileName = "file.xlsx",
                Rows =
                [
                    new MonthlyTimeImportRowDto { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = "6", WorkGroup = "WG1", Hours = "10", Name = "N1", PactId = "P1" },
                    new MonthlyTimeImportRowDto { PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = "7", WorkGroup = "WG2", Hours = "20", Name = "N2", PactId = "P2" },
                    new MonthlyTimeImportRowDto { PactStaffId = "S3", TimeCode = "TC3", ParentProject = "PP3", Month = "8", WorkGroup = "WG3", Hours = "30", Name = "N3", PactId = "P3" }
                ]
            };

            var result = await _sut.ImportStagingAsync(request, "user1");

            result.ImportedCount.Should().Be(3);
            result.Message.Should().Contain("3 rows added to staging");
            await _mockRepository.Received(1).ImportStagingAsync(Arg.Is<List<StagingMonthlyTime>>(l => l.Count == 3));
        }

        #endregion

        #region ValidateStagingAsync

        [Fact]
        public async Task ValidateStagingAsync_NoRecords_ReturnsNoRecordsMessage()
        {
            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(new List<StagingMonthlyTime>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.PassedCount.Should().Be(0);
            result.FailedCount.Should().Be(0);
            result.Message.Should().Contain("No records to validate");
            await _mockRepository.Received(1).RemoveZeroAndNullHourRecordsAsync("user1");
        }

        [Fact]
        public async Task ValidateStagingAsync_AllRecordsPass_ReturnsCorrectCounts()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.PassedCount.Should().Be(1);
            result.FailedCount.Should().Be(0);
            await _mockRepository.Received(1).UpdateStagingRecordsAsync(records);
        }

        [Fact]
        public async Task ValidateStagingAsync_RecordFailsValidation_ReturnsFailedCount()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "INVALID_WG", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>());
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>());
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            result.PassedCount.Should().Be(0);
            records[0].Passed.Should().BeFalse();
            records[0].FailureComments.Should().Contain("work group name is invalid");
        }

        [Fact]
        public async Task ValidateStagingAsync_BlankHours_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = null, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("hours field is not a number");
        }

        [Fact]
        public async Task ValidateStagingAsync_BlankWorkGroup_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>());
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>());
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("work group name is blank");
        }

        [Fact]
        public async Task ValidateStagingAsync_BlankStaffIdAndName_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>());
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("Staff ID/Name blank");
        }

        [Fact]
        public async Task ValidateStagingAsync_NumericStaffNotInWorkGroup_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "999", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Other" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("staff ID not in this WG");
        }

        [Fact]
        public async Task ValidateStagingAsync_NamedStaffSingleMatch_PopulatesRecord()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "John Smith", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "John Smith" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "SP1", PactId = "P1", Name = "John Smith" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.PassedCount.Should().Be(1);
            records[0].PactStaffId.Should().Be("SP1");
            records[0].PactId.Should().Be("P1");
        }

        [Fact]
        public async Task ValidateStagingAsync_MultipleStaffMatchesNoPactId_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "John Smith", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "John Smith", PactId = null }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "SP1", PactId = "P1", Name = "John Smith" },
                new() { WorkGroup = "WG1", SpNumber = "SP2", PactId = "P2", Name = "John Smith" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("more than one person");
        }

        [Fact]
        public async Task ValidateStagingAsync_MultipleStaffMatchesWithPactId_UsesFirstMatch()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "John Smith", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "John Smith", PactId = "P1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "SP1", PactId = "P1", Name = "John Smith" },
                new() { WorkGroup = "WG1", SpNumber = "SP2", PactId = "P2", Name = "John Smith" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.PassedCount.Should().Be(1);
            records[0].PactStaffId.Should().Be("SP1");
        }

        [Fact]
        public async Task ValidateStagingAsync_BlankTimeCode_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "", ParentProject = "PP1", Month = 6, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>());
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("Timecode is blank");
        }

        [Fact]
        public async Task ValidateStagingAsync_InvalidTimeCodeForWorkGroup_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "BADTC", ParentProject = "PP1", Month = 6, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("Timecode not valid for this WG or invalid timecode");
        }

        [Fact]
        public async Task ValidateStagingAsync_InactiveTimeCode_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = false }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("Timecode not valid for this WG");
        }

        [Fact]
        public async Task ValidateStagingAsync_BlankParentProject_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "", Month = 6, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("Project is blank");
        }

        [Fact]
        public async Task ValidateStagingAsync_InvalidParentProjectCombination_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "BAD_PP", Month = 6, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("Not valid timecode/Project/WG combination");
        }

        [Fact]
        public async Task ValidateStagingAsync_BlankMonth_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = null, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("month No. is blank");
        }

        [Fact]
        public async Task ValidateStagingAsync_InvalidMonth_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 99, Hours = 10, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("month No. invalid");
        }

        [Fact]
        public async Task ValidateStagingAsync_DuplicateInStaging_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "Name1", PactId = "P1" },
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 5, Name = "Name1", PactId = "P1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.PassedCount.Should().Be(1);
            result.FailedCount.Should().Be(1);
            records[1].FailureComments.Should().Contain("Similar record in sheet being imported");
        }

        [Fact]
        public async Task ValidateStagingAsync_DuplicateInLive_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "Name1", PactId = "P1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string> { "P1|TC1|PP1|WG1|6" });
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("Similar record already imported");
        }

        [Fact]
        public async Task ValidateStagingAsync_ZeroHours_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "123", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 0, Name = "Name1" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "123", PactId = "P1", Name = "Name1" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("hours field is not a number");
        }

        [Fact]
        public async Task ValidateStagingAsync_MultipleStaffMatchesPactIdZero_FailsValidation()
        {
            var records = new List<StagingMonthlyTime>
            {
                new() { PactStaffId = "John Smith", WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 10, Name = "John Smith", PactId = "0" }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockWorkGroupRepository.GetStaffByWorkGroupAsync().Returns(new List<WorkGroupStaffItem>
            {
                new() { WorkGroup = "WG1", SpNumber = "SP1", PactId = "P1", Name = "John Smith" },
                new() { WorkGroup = "WG1", SpNumber = "SP2", PactId = "P2", Name = "John Smith" }
            });
            _mockTimeCodeValidRepository.GetTimeCodeValidsAsync().Returns(new List<TimeCodeValid>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", Active = true }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("more than one person");
        }

        #endregion

        #region MakeLiveAsync

        [Fact]
        public async Task MakeLiveAsync_WhenHasFailedStaging_ThrowsBusinessValidationException()
        {
            _mockRepository.HasFailedStagingAsync("user1").Returns(true);

            var act = async () => await _sut.MakeLiveAsync("user1");

            await act.Should().ThrowAsync<Validation.BusinessValidationErrorException>();
        }

        [Fact]
        public async Task MakeLiveAsync_WhenNoStagingRecords_ThrowsBusinessValidationException()
        {
            _mockRepository.HasFailedStagingAsync("user1").Returns(false);
            _mockRepository.MakeLiveAsync("user1").Returns((ProcessedCount: 0, ImportedCount: 0, FailedCount: 0));

            var act = async () => await _sut.MakeLiveAsync("user1");

            await act.Should().ThrowAsync<Validation.BusinessValidationErrorException>();
        }

        [Fact]
        public async Task MakeLiveAsync_WhenAllSucceed_ReturnsSuccessResult()
        {
            _mockRepository.HasFailedStagingAsync("user1").Returns(false);
            _mockRepository.MakeLiveAsync("user1").Returns((ProcessedCount: 5, ImportedCount: 5, FailedCount: 0));

            var result = await _sut.MakeLiveAsync("user1");

            result.ProcessedCount.Should().Be(5);
            result.ImportedCount.Should().Be(5);
            result.FailedCount.Should().Be(0);
            result.Message.Should().Contain("5 of 5 records have been successfully made live");
        }

        [Fact]
        public async Task MakeLiveAsync_WhenSomeFail_ReturnsResultWithRevalidationMessage()
        {
            _mockRepository.HasFailedStagingAsync("user1").Returns(false);
            _mockRepository.MakeLiveAsync("user1").Returns((ProcessedCount: 5, ImportedCount: 3, FailedCount: 2));

            var result = await _sut.MakeLiveAsync("user1");

            result.ProcessedCount.Should().Be(5);
            result.ImportedCount.Should().Be(3);
            result.FailedCount.Should().Be(2);
            result.Message.Should().Contain("3 of 5 records have been successfully made live");
            result.Message.Should().Contain("2 records require revalidation");
        }

        #endregion
    }
}

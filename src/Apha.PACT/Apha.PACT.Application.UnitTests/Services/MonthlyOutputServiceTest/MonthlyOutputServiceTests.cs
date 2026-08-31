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

namespace Apha.PACT.Application.UnitTests.Services.MonthlyOutputServiceTest
{
    public class MonthlyOutputServiceTests
    {
        private readonly IMonthlyOutputRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ICalenderMonthRepository _mockCalenderMonthRepository;
        private readonly IWorkGroupRepository _mockWorkGroupRepository;
        private readonly ITestCapabilityRepository _mockTestCapabilityRepository;
        private readonly ITestRequirementRepository _mockTestRequirementRepository;
        private readonly MonthlyOutputService _sut;

        public MonthlyOutputServiceTests()
        {
            _mockRepository = Substitute.For<IMonthlyOutputRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockCalenderMonthRepository = Substitute.For<ICalenderMonthRepository>();
            _mockWorkGroupRepository = Substitute.For<IWorkGroupRepository>();
            _mockTestCapabilityRepository = Substitute.For<ITestCapabilityRepository>();
            _mockTestRequirementRepository = Substitute.For<ITestRequirementRepository>();
            _sut = new MonthlyOutputService(
                _mockRepository,
                _mockMapper,
                _mockCalenderMonthRepository,
                _mockWorkGroupRepository,
                _mockTestCapabilityRepository,
                _mockTestRequirementRepository);
        }

        // ── helpers ────────────────────────────────────────────────────────────

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new() { Page = page, PageSize = pageSize };

        private static PaginationParameters<string> DefaultPaginationParameters(int page = 1, int pageSize = 10)
            => new(page: page, pageSize: pageSize);

        private static PagedData<MonthlyOutputLog> BuildPagedData(
            IEnumerable<MonthlyOutputLog> items,
            int page = 1, int pageSize = 10, int totalRecords = 0)
        {
            var list = items.ToList();
            var total = totalRecords > 0 ? totalRecords : list.Count;
            return new PagedData<MonthlyOutputLog>(
                list.AsReadOnly(),
                new PaginationData
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                });
        }

        private static PaginatedResult<MonthlyOutputLogDto> BuildPaginatedResult(
            IEnumerable<MonthlyOutputLogDto> dtos,
            int page = 1, int pageSize = 10, int totalRecords = 0)
        {
            var list = dtos.ToList();
            var total = totalRecords > 0 ? totalRecords : list.Count;
            return new PaginatedResult<MonthlyOutputLogDto>(
                list,
                new PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                });
        }

        // ── GetMonthlyOutputLogAsync — happy path ───────────────────────────────

        #region GetMonthlyOutputLogAsync — happy path

        [Fact]
        public async Task GetMonthlyOutputLogAsync_WithNoFilters_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var entities = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 1, WorkGroup = "WG1", TestCode = "TC1", Buyer = "BUYER_A" },
                new() { SequenceNo = 2, WorkGroup = "WG2", TestCode = "TC2", Buyer = "BUYER_B" }
            };
            var dtos = new List<MonthlyOutputLogDto>
            {
                new() { SequenceNo = 1, WorkGroup = "WG1", TestCode = "TC1", Buyer = "BUYER_A" },
                new() { SequenceNo = 2, WorkGroup = "WG2", TestCode = "TC2", Buyer = "BUYER_B" }
            };
            var pagedData = BuildPagedData(entities);
            var expectedResult = BuildPaginatedResult(dtos);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_WithAllFilters_PassesFiltersToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var dateImported = new DateTime(2024, 6, 1);
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, "WG1", "TC1", "BUYER_A", dateImported, 6, "SP001", "I")
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetMonthlyOutputLogAsync(query, "WG1", "TC1", "BUYER_A", dateImported, 6, "SP001", "I");

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, "WG1", "TC1", "BUYER_A", dateImported, 6, "SP001", "I");
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_MapsQueryParametersToPaginationParameters()
        {
            // Arrange
            var query = DefaultQuery(page: 2, pageSize: 5);
            var paginationParams = DefaultPaginationParameters(page: 2, pageSize: 5);
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_MapsPagedDataToPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            _mockMapper.Received(1).Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — empty results

        [Fact]
        public async Task GetMonthlyOutputLogAsync_WhenRepositoryReturnsEmpty_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetMonthlyOutputLogAsync — pagination metadata

        [Fact]
        public async Task GetMonthlyOutputLogAsync_ReturnsPaginationMetadataFromMappedResult()
        {
            // Arrange
            var query = DefaultQuery(page: 2, pageSize: 3);
            var paginationParams = DefaultPaginationParameters(page: 2, pageSize: 3);
            var entities = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 4 },
                new() { SequenceNo = 5 },
                new() { SequenceNo = 6 }
            };
            var dtos = entities.Select(e => new MonthlyOutputLogDto { SequenceNo = e.SequenceNo }).ToList();
            var pagedData = BuildPagedData(entities, page: 2, pageSize: 3, totalRecords: 10);
            var expectedResult = BuildPaginatedResult(dtos, page: 2, pageSize: 3, totalRecords: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            result.PaginationData.PageNumber.Should().Be(2);
            result.PaginationData.PageSize.Should().Be(3);
            result.PaginationData.TotalRecords.Should().Be(10);
            result.PaginationData.TotalPages.Should().Be(4);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — individual filter delegation

        [Fact]
        public async Task GetMonthlyOutputLogAsync_WorkGroupFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, "WG1", null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, "WG1", null, null, null, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, "WG1", null, null, null, null, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_TestCodeFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, "TC1", null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, "TC1", null, null, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, "TC1", null, null, null, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_BuyerFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, "BUYER_A", null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, "BUYER_A", null, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, "BUYER_A", null, null, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_DateImportedFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var dateImported = new DateTime(2024, 6, 15);
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, dateImported, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, dateImported, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, dateImported, null, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_MonthFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, 6, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, 6, null, null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, 6, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_UserIdFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, "SP001", null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, "SP001", null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, "SP001", null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_InsertDeleteFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, "I")
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, "I");

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, "I");
        }

        #endregion

        #region GetMonthlyOutputLogAsync — exception handling

        [Fact]
        public async Task GetMonthlyOutputLogAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = () => _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
        }

        #endregion

        #region Live and Staging Operations

        [Fact]
        public async Task SearchLiveAsync_WithFilters_MapsAndDelegatesToRepository()
        {
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var paged = new PagedData<MonthlyOutput>([], new PaginationData());
            var expected = new PaginatedResult<MonthlyOutputDto>([], new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchLiveAsync(paginationParams, "WG1", "TC1", "B1", 6).Returns(paged);
            _mockMapper.Map<PaginatedResult<MonthlyOutputDto>>(paged).Returns(expected);

            var result = await _sut.SearchLiveAsync(query, "WG1", "TC1", "B1", 6);

            result.Should().BeSameAs(expected);
            await _mockRepository.Received(1).SearchLiveAsync(paginationParams, "WG1", "TC1", "B1", 6);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenEntityExists_ReturnsMappedDto()
        {
            var entity = new MonthlyOutput { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6 };
            var dto = new MonthlyOutputDto { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6 };

            _mockRepository.GetLiveByKeyAsync("TC1", "B1", 6, "WG1").Returns(entity);
            _mockMapper.Map<MonthlyOutputDto>(entity).Returns(dto);

            var result = await _sut.GetLiveByKeyAsync("TC1", "B1", 6, "WG1");

            result.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenEntityMissing_ReturnsNull()
        {
            _mockRepository.GetLiveByKeyAsync("TC1", "B1", 6, "WG1").Returns((MonthlyOutput?)null);

            var result = await _sut.GetLiveByKeyAsync("TC1", "B1", 6, "WG1");

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteLiveAsync_DelegatesToRepository()
        {
            _mockRepository.DeleteLiveAsync("TC1", "B1", 6, "WG1").Returns(true);

            var result = await _sut.DeleteLiveAsync("TC1", "B1", 6, "WG1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteLiveAsync("TC1", "B1", 6, "WG1");
        }

        [Fact]
        public async Task SearchStagingAsync_WithFilters_MapsAndDelegatesToRepository()
        {
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var paged = new PagedData<StagingMonthlyOutput>([], new PaginationData());
            var expected = new PaginatedResult<StagingMonthlyOutputDto>([], new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchStagingAsync(paginationParams, "user1", true).Returns(paged);
            _mockMapper.Map<PaginatedResult<StagingMonthlyOutputDto>>(paged).Returns(expected);

            var result = await _sut.SearchStagingAsync(query, "user1", true);

            result.Should().BeSameAs(expected);
            await _mockRepository.Received(1).SearchStagingAsync(paginationParams, "user1", true);
        }

        #endregion

        #region Constructor — null guard tests

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyOutputService(null!, _mockMapper, _mockCalenderMonthRepository, _mockWorkGroupRepository, _mockTestCapabilityRepository, _mockTestRequirementRepository);
            act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyOutputService(_mockRepository, null!, _mockCalenderMonthRepository, _mockWorkGroupRepository, _mockTestCapabilityRepository, _mockTestRequirementRepository);
            act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
        }

        [Fact]
        public void Constructor_NullCalenderMonthRepository_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyOutputService(_mockRepository, _mockMapper, null!, _mockWorkGroupRepository, _mockTestCapabilityRepository, _mockTestRequirementRepository);
            act.Should().Throw<ArgumentNullException>().WithParameterName("calenderMonthRepository");
        }

        [Fact]
        public void Constructor_NullWorkGroupRepository_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyOutputService(_mockRepository, _mockMapper, _mockCalenderMonthRepository, null!, _mockTestCapabilityRepository, _mockTestRequirementRepository);
            act.Should().Throw<ArgumentNullException>().WithParameterName("workGroupRepository");
        }

        [Fact]
        public void Constructor_NullTestCapabilityRepository_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyOutputService(_mockRepository, _mockMapper, _mockCalenderMonthRepository, _mockWorkGroupRepository, null!, _mockTestRequirementRepository);
            act.Should().Throw<ArgumentNullException>().WithParameterName("testCapabilityRepository");
        }

        [Fact]
        public void Constructor_NullTestRequirementRepository_ThrowsArgumentNullException()
        {
            var act = () => new MonthlyOutputService(_mockRepository, _mockMapper, _mockCalenderMonthRepository, _mockWorkGroupRepository, _mockTestCapabilityRepository, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("testRequirementRepository");
        }

        #endregion

        #region UpdateLiveAsync

        [Fact]
        public async Task UpdateLiveAsync_WithOriginalFields_UsesOriginalValues()
        {
            var dto = new MonthlyOutputDto
            {
                TestCode = "TC2", Buyer = "B2", Month = 7, WorkGroup = "WG2",
                OriginalTestCode = "TC1", OriginalBuyer = "B1", OriginalMonth = 6, OriginalWorkGroup = "WG1"
            };
            var entity = new MonthlyOutput { TestCode = "TC2", Buyer = "B2", Month = 7, WorkGroup = "WG2" };
            var updated = new MonthlyOutput { TestCode = "TC2", Buyer = "B2", Month = 7, WorkGroup = "WG2" };
            var resultDto = new MonthlyOutputDto { TestCode = "TC2", Buyer = "B2", Month = 7, WorkGroup = "WG2" };

            _mockMapper.Map<MonthlyOutput>(dto).Returns(entity);
            _mockRepository.UpdateLiveAsync(entity, "TC1", "B1", 6, "WG1").Returns(updated);
            _mockMapper.Map<MonthlyOutputDto>(updated).Returns(resultDto);

            var result = await _sut.UpdateLiveAsync(dto);

            result.Should().BeSameAs(resultDto);
            await _mockRepository.Received(1).UpdateLiveAsync(entity, "TC1", "B1", 6, "WG1");
        }

        [Fact]
        public async Task UpdateLiveAsync_WithoutOriginalFields_FallsBackToCurrentValues()
        {
            var dto = new MonthlyOutputDto
            {
                TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1",
                OriginalTestCode = null, OriginalBuyer = null, OriginalMonth = null, OriginalWorkGroup = null
            };
            var entity = new MonthlyOutput();
            var updated = new MonthlyOutput();
            var resultDto = new MonthlyOutputDto();

            _mockMapper.Map<MonthlyOutput>(dto).Returns(entity);
            _mockRepository.UpdateLiveAsync(entity, "TC1", "B1", 6, "WG1").Returns(updated);
            _mockMapper.Map<MonthlyOutputDto>(updated).Returns(resultDto);

            var result = await _sut.UpdateLiveAsync(dto);

            result.Should().BeSameAs(resultDto);
            await _mockRepository.Received(1).UpdateLiveAsync(entity, "TC1", "B1", 6, "WG1");
        }

        [Fact]
        public async Task UpdateLiveAsync_WithEmptyOriginalFields_FallsBackToCurrentValues()
        {
            var dto = new MonthlyOutputDto
            {
                TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1",
                OriginalTestCode = "", OriginalBuyer = "  ", OriginalMonth = null, OriginalWorkGroup = ""
            };
            var entity = new MonthlyOutput();
            var updated = new MonthlyOutput();
            var resultDto = new MonthlyOutputDto();

            _mockMapper.Map<MonthlyOutput>(dto).Returns(entity);
            _mockRepository.UpdateLiveAsync(entity, "TC1", "B1", 6, "WG1").Returns(updated);
            _mockMapper.Map<MonthlyOutputDto>(updated).Returns(resultDto);

            var result = await _sut.UpdateLiveAsync(dto);

            await _mockRepository.Received(1).UpdateLiveAsync(entity, "TC1", "B1", 6, "WG1");
        }

        #endregion

        #region GetStagingByIdAsync

        [Fact]
        public async Task GetStagingByIdAsync_WhenEntityExists_ReturnsMappedDto()
        {
            var entity = new StagingMonthlyOutput { Id = 5, TestCode = "TC1" };
            var dto = new StagingMonthlyOutputDto { Id = 5, TestCode = "TC1" };

            _mockRepository.GetStagingByIdAsync(5, "user1").Returns(entity);
            _mockMapper.Map<StagingMonthlyOutputDto>(entity).Returns(dto);

            var result = await _sut.GetStagingByIdAsync(5, "user1");

            result.Should().BeSameAs(dto);
        }

        [Fact]
        public async Task GetStagingByIdAsync_WhenEntityMissing_ReturnsNull()
        {
            _mockRepository.GetStagingByIdAsync(5, "user1").Returns((StagingMonthlyOutput?)null);

            var result = await _sut.GetStagingByIdAsync(5, "user1");

            result.Should().BeNull();
        }

        #endregion

        #region CreateStagingAsync

        [Fact]
        public async Task CreateStagingAsync_SetsImportedByAndDate_ReturnsMappedDto()
        {
            var dto = new StagingMonthlyOutputDto { TestCode = "TC1", Buyer = "B1" };
            var entity = new StagingMonthlyOutput { TestCode = "TC1", Buyer = "B1" };
            var created = new StagingMonthlyOutput { Id = 10, TestCode = "TC1", Buyer = "B1", ImportedBy = "user1" };
            var resultDto = new StagingMonthlyOutputDto { Id = 10, TestCode = "TC1", Buyer = "B1" };

            _mockMapper.Map<StagingMonthlyOutput>(dto).Returns(entity);
            _mockRepository.CreateStagingAsync(entity).Returns(created);
            _mockMapper.Map<StagingMonthlyOutputDto>(created).Returns(resultDto);

            var result = await _sut.CreateStagingAsync(dto, "user1");

            result.Should().BeSameAs(resultDto);
            entity.ImportedBy.Should().Be("user1");
            entity.ImportedDate!.Value.Kind.Should().Be(DateTimeKind.Unspecified);
        }

        #endregion

        #region UpdateStagingAsync

        [Fact]
        public async Task UpdateStagingAsync_SetsImportedDate_ReturnsMappedDto()
        {
            var dto = new StagingMonthlyOutputDto { Id = 5, TestCode = "TC1" };
            var entity = new StagingMonthlyOutput { Id = 5, TestCode = "TC1" };
            var updated = new StagingMonthlyOutput { Id = 5, TestCode = "TC1" };
            var resultDto = new StagingMonthlyOutputDto { Id = 5, TestCode = "TC1" };

            _mockMapper.Map<StagingMonthlyOutput>(dto).Returns(entity);
            _mockRepository.UpdateStagingAsync(entity, "user1").Returns(updated);
            _mockMapper.Map<StagingMonthlyOutputDto>(updated).Returns(resultDto);

            var result = await _sut.UpdateStagingAsync(dto, "user1");

            result.Should().BeSameAs(resultDto);
            entity.ImportedDate!.Value.Kind.Should().Be(DateTimeKind.Unspecified);
        }

        #endregion

        #region DeleteStagingAsync

        [Fact]
        public async Task DeleteStagingAsync_DelegatesToRepository()
        {
            _mockRepository.DeleteStagingAsync(5, "user1").Returns(true);

            var result = await _sut.DeleteStagingAsync(5, "user1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteStagingAsync(5, "user1");
        }

        [Fact]
        public async Task DeleteStagingAsync_WhenNotFound_ReturnsFalse()
        {
            _mockRepository.DeleteStagingAsync(5, "user1").Returns(false);

            var result = await _sut.DeleteStagingAsync(5, "user1");

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
            await _mockRepository.Received(1).DeleteAllStagingByUserAsync("user1");
        }

        #endregion

        #region DeleteFailedStagingByUserAsync

        [Fact]
        public async Task DeleteFailedStagingByUserAsync_DelegatesToRepository()
        {
            _mockRepository.DeleteFailedStagingByUserAsync("user1").Returns(3);

            var result = await _sut.DeleteFailedStagingByUserAsync("user1");

            result.Should().Be(3);
            await _mockRepository.Received(1).DeleteFailedStagingByUserAsync("user1");
        }

        #endregion

        #region ImportStagingAsync — non-type-4

        [Fact]
        public async Task ImportStagingAsync_NonType4_InsertsAllRowsToStaging()
        {
            var request = new MonthlyOutputImportDto
            {
                ImportType = 1,
                FileName = "test.xlsx",
                Rows = new List<MonthlyOutputImportRowDto>
                {
                    new() { TestCode = "TC1", Buyer = "B1", Month = "6", WorkGroup = "WG1", Volume = "10" },
                    new() { TestCode = "TC2", Buyer = "B2", Month = "7", WorkGroup = "WG2", Volume = "20" }
                }
            };

            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyOutput>>()).Returns(2);

            var result = await _sut.ImportStagingAsync(request, "user1");

            result.ImportedCount.Should().Be(2);
            result.PassedCount.Should().Be(0);
            result.FailedCount.Should().Be(0);
            result.Message.Should().Contain("2 rows added to staging");
            await _mockRepository.Received(1).ImportStagingAsync(Arg.Is<List<StagingMonthlyOutput>>(l =>
                l.Count == 2 &&
                l[0].TestCode == "TC1" && l[0].ImportedBy == "user1" && l[0].Filename == "test.xlsx" &&
                l[1].TestCode == "TC2"));
        }

        [Fact]
        public async Task ImportStagingAsync_NonType4_WithNullFields_DefaultsToEmptyStrings()
        {
            var request = new MonthlyOutputImportDto
            {
                ImportType = 1,
                FileName = "test.xlsx",
                Rows = new List<MonthlyOutputImportRowDto>
                {
                    new() { TestCode = null, Buyer = null, Month = null, WorkGroup = null, Volume = null }
                }
            };

            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyOutput>>()).Returns(1);

            var result = await _sut.ImportStagingAsync(request, "user1");

            result.ImportedCount.Should().Be(1);
            await _mockRepository.Received(1).ImportStagingAsync(Arg.Is<List<StagingMonthlyOutput>>(l =>
                l[0].TestCode == string.Empty &&
                l[0].Buyer == string.Empty &&
                l[0].WorkGroup == string.Empty &&
                l[0].Month == 0 &&
                l[0].Volume == null));
        }

        #endregion

        #region ImportStagingAsync — type 4

        [Fact]
        public async Task ImportStagingAsync_Type4_ExistingRow_UpdatesExisting()
        {
            var existingEntity = new StagingMonthlyOutput { Id = 10, TestCode = "OLD", Buyer = "OLD", Month = 1, WorkGroup = "OLD" };
            var request = new MonthlyOutputImportDto
            {
                ImportType = 4,
                FileName = "test.xlsx",
                Rows = new List<MonthlyOutputImportRowDto>
                {
                    new() { Id = 10, TestCode = "TC1", Buyer = "B1", Month = "6", WorkGroup = "WG1", Volume = "10" }
                }
            };

            _mockRepository.GetStagingByIdAsync(10, "user1").Returns(existingEntity);
            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyOutput>>()).Returns(0);

            var result = await _sut.ImportStagingAsync(request, "user1");

            result.ImportedCount.Should().Be(1);
            result.Message.Should().Contain("1 rows processed in staging");
            await _mockRepository.Received(1).UpdateStagingRecordsAsync(Arg.Is<List<StagingMonthlyOutput>>(l =>
                l.Count == 1 && l[0].TestCode == "TC1" && l[0].Buyer == "B1" && l[0].Passed == false));
            await _mockRepository.Received(1).ImportStagingAsync(Arg.Is<List<StagingMonthlyOutput>>(l => l.Count == 0));
        }

        [Fact]
        public async Task ImportStagingAsync_Type4_NonExistingRow_InsertsNew()
        {
            var request = new MonthlyOutputImportDto
            {
                ImportType = 4,
                FileName = "test.xlsx",
                Rows = new List<MonthlyOutputImportRowDto>
                {
                    new() { Id = 10, TestCode = "TC1", Buyer = "B1", Month = "6", WorkGroup = "WG1", Volume = "10" }
                }
            };

            _mockRepository.GetStagingByIdAsync(10, "user1").Returns((StagingMonthlyOutput?)null);
            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyOutput>>()).Returns(1);

            var result = await _sut.ImportStagingAsync(request, "user1");

            result.ImportedCount.Should().Be(1);
            await _mockRepository.DidNotReceive().UpdateStagingRecordsAsync(Arg.Any<List<StagingMonthlyOutput>>());
            await _mockRepository.Received(1).ImportStagingAsync(Arg.Is<List<StagingMonthlyOutput>>(l =>
                l.Count == 1 && l[0].TestCode == "TC1" && l[0].ImportedBy == "user1"));
        }

        [Fact]
        public async Task ImportStagingAsync_Type4_ZeroIdRow_InsertsNew()
        {
            var request = new MonthlyOutputImportDto
            {
                ImportType = 4,
                FileName = "test.xlsx",
                Rows = new List<MonthlyOutputImportRowDto>
                {
                    new() { Id = 0, TestCode = "TC1", Buyer = "B1", Month = "6", WorkGroup = "WG1", Volume = "10" }
                }
            };

            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyOutput>>()).Returns(1);

            var result = await _sut.ImportStagingAsync(request, "user1");

            result.ImportedCount.Should().Be(1);
            await _mockRepository.DidNotReceive().GetStagingByIdAsync(Arg.Any<int>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportStagingAsync_Type4_MixedRows_UpdatesAndInserts()
        {
            var existingEntity = new StagingMonthlyOutput { Id = 10, TestCode = "OLD" };
            var request = new MonthlyOutputImportDto
            {
                ImportType = 4,
                FileName = "test.xlsx",
                Rows = new List<MonthlyOutputImportRowDto>
                {
                    new() { Id = 10, TestCode = "TC1", Buyer = "B1", Month = "6", WorkGroup = "WG1", Volume = "10" },
                    new() { Id = 0, TestCode = "TC2", Buyer = "B2", Month = "7", WorkGroup = "WG2", Volume = "20" }
                }
            };

            _mockRepository.GetStagingByIdAsync(10, "user1").Returns(existingEntity);
            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyOutput>>()).Returns(1);

            var result = await _sut.ImportStagingAsync(request, "user1");

            result.ImportedCount.Should().Be(2);
            await _mockRepository.Received(1).UpdateStagingRecordsAsync(Arg.Is<List<StagingMonthlyOutput>>(l => l.Count == 1));
            await _mockRepository.Received(1).ImportStagingAsync(Arg.Is<List<StagingMonthlyOutput>>(l => l.Count == 1));
        }

        [Fact]
        public async Task ImportStagingAsync_Type4_NoRowsToUpdate_SkipsUpdate()
        {
            var request = new MonthlyOutputImportDto
            {
                ImportType = 4,
                FileName = "test.xlsx",
                Rows = new List<MonthlyOutputImportRowDto>
                {
                    new() { Id = 0, TestCode = "TC1", Buyer = "B1", Month = "6", WorkGroup = "WG1", Volume = "10" }
                }
            };

            _mockRepository.ImportStagingAsync(Arg.Any<List<StagingMonthlyOutput>>()).Returns(1);

            await _sut.ImportStagingAsync(request, "user1");

            await _mockRepository.DidNotReceive().UpdateStagingRecordsAsync(Arg.Any<List<StagingMonthlyOutput>>());
        }

        #endregion

        #region ValidateStagingAsync

        [Fact]
        public async Task ValidateStagingAsync_NoRecords_ReturnsZeroCounts()
        {
            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(new List<StagingMonthlyOutput>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.PassedCount.Should().Be(0);
            result.FailedCount.Should().Be(0);
            result.Message.Should().Contain("No records to validate");
            await _mockRepository.Received(1).RemoveZeroAndNullVolumeRecordsAsync("user1");
            await _mockRepository.DidNotReceive().UpdateStagingRecordsAsync(Arg.Any<List<StagingMonthlyOutput>>());
        }

        [Fact]
        public async Task ValidateStagingAsync_WithRecords_ValidatesAndUpdates()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth>
            {
                new() { MonthNumber = 6 }
            });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockTestCapabilityRepository.GetAllAsync().Returns(new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1" }
            });
            _mockTestRequirementRepository.GetAllActiveAsync().Returns(new List<TestRequirement>
            {
                new() { TestCode = "TC1", Buyer = "B1" }
            });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.PassedCount.Should().Be(1);
            result.FailedCount.Should().Be(0);
            records[0].Passed.Should().BeTrue();
            await _mockRepository.Received(1).UpdateStagingRecordsAsync(records);
        }

        [Fact]
        public async Task ValidateStagingAsync_InvalidWorkGroup_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "INVALID", Volume = 10 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth>());
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockTestCapabilityRepository.GetAllAsync().Returns(new List<TestCapability>());
            _mockTestRequirementRepository.GetAllActiveAsync().Returns(new List<TestRequirement>());
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            result.PassedCount.Should().Be(0);
            records[0].Passed.Should().BeFalse();
            records[0].FailureComments.Should().Contain("not an actual WG");
        }

        [Fact]
        public async Task ValidateStagingAsync_NullVolume_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = null }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            SetupValidationContext();

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("not a number");
        }

        [Fact]
        public async Task ValidateStagingAsync_ZeroVolume_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 0 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            SetupValidationContext();

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("not a number");
        }

        [Fact]
        public async Task ValidateStagingAsync_BlankWorkGroup_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "  ", Volume = 10 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            SetupValidationContext();

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("work group name is blank");
        }

        [Fact]
        public async Task ValidateStagingAsync_BlankTestCodeOrBuyer_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockTestCapabilityRepository.GetAllAsync().Returns(new List<TestCapability>());
            _mockTestRequirementRepository.GetAllActiveAsync().Returns(new List<TestRequirement>());
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("No Testcode");
        }

        [Fact]
        public async Task ValidateStagingAsync_InvalidTestCapability_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "BADTC", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockTestCapabilityRepository.GetAllAsync().Returns(new List<TestCapability>());
            _mockTestRequirementRepository.GetAllActiveAsync().Returns(new List<TestRequirement>());
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("WG not set up to do this test");
        }

        [Fact]
        public async Task ValidateStagingAsync_InvalidBuyerRequirement_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "BADB", Month = 6, WorkGroup = "WG1", Volume = 10 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockTestCapabilityRepository.GetAllAsync().Returns(new List<TestCapability> { new() { TestCode = "TC1", WorkGroup = "WG1" } });
            _mockTestRequirementRepository.GetAllActiveAsync().Returns(new List<TestRequirement>());
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("project not buying this test");
        }

        [Fact]
        public async Task ValidateStagingAsync_ZeroMonth_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "B1", Month = 0, WorkGroup = "WG1", Volume = 10 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockTestCapabilityRepository.GetAllAsync().Returns(new List<TestCapability> { new() { TestCode = "TC1", WorkGroup = "WG1" } });
            _mockTestRequirementRepository.GetAllActiveAsync().Returns(new List<TestRequirement> { new() { TestCode = "TC1", Buyer = "B1" } });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("month No. is blank");
        }

        [Fact]
        public async Task ValidateStagingAsync_InvalidMonth_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "B1", Month = 99, WorkGroup = "WG1", Volume = 10 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockTestCapabilityRepository.GetAllAsync().Returns(new List<TestCapability> { new() { TestCode = "TC1", WorkGroup = "WG1" } });
            _mockTestRequirementRepository.GetAllActiveAsync().Returns(new List<TestRequirement> { new() { TestCode = "TC1", Buyer = "B1" } });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("month No. is invalid");
        }

        [Fact]
        public async Task ValidateStagingAsync_DuplicateInStaging_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 },
                new() { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 20 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            SetupFullValidationContext();

            var result = await _sut.ValidateStagingAsync("user1");

            result.PassedCount.Should().Be(1);
            result.FailedCount.Should().Be(1);
            records[1].FailureComments.Should().Contain("Similar record in sheet being imported");
        }

        [Fact]
        public async Task ValidateStagingAsync_DuplicateInLive_FailsRecord()
        {
            var records = new List<StagingMonthlyOutput>
            {
                new() { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 }
            };

            _mockRepository.GetStagingRecordsForValidationAsync("user1").Returns(records);
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockTestCapabilityRepository.GetAllAsync().Returns(new List<TestCapability> { new() { TestCode = "TC1", WorkGroup = "WG1" } });
            _mockTestRequirementRepository.GetAllActiveAsync().Returns(new List<TestRequirement> { new() { TestCode = "TC1", Buyer = "B1" } });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TC1|B1|6|WG1" });
            _mockRepository.GetPassedStagingKeysAsync("user1").Returns(new HashSet<string>());

            var result = await _sut.ValidateStagingAsync("user1");

            result.FailedCount.Should().Be(1);
            records[0].FailureComments.Should().Contain("similar record already imported");
        }

        private void SetupValidationContext()
        {
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth>());
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string>());
            _mockTestCapabilityRepository.GetAllAsync().Returns(new List<TestCapability>());
            _mockTestRequirementRepository.GetAllActiveAsync().Returns(new List<TestRequirement>());
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync(Arg.Any<string>()).Returns(new HashSet<string>());
        }

        private void SetupFullValidationContext()
        {
            _mockCalenderMonthRepository.GetCalenderMonthsAsync().Returns(new List<CalenderMonth> { new() { MonthNumber = 6 } });
            _mockWorkGroupRepository.GetAllWorkGroupNamesAsync().Returns(new List<string> { "WG1" });
            _mockTestCapabilityRepository.GetAllAsync().Returns(new List<TestCapability> { new() { TestCode = "TC1", WorkGroup = "WG1" } });
            _mockTestRequirementRepository.GetAllActiveAsync().Returns(new List<TestRequirement> { new() { TestCode = "TC1", Buyer = "B1" } });
            _mockRepository.GetExistingLiveKeysAsync().Returns(new HashSet<string>());
            _mockRepository.GetPassedStagingKeysAsync(Arg.Any<string>()).Returns(new HashSet<string>());
        }

        #endregion

        #region MakeLiveAsync

        [Fact]
        public async Task MakeLiveAsync_HasFailedStaging_ThrowsBusinessValidationError()
        {
            _mockRepository.HasFailedStagingAsync("user1").Returns(true);

            Func<Task> act = () => _sut.MakeLiveAsync("user1");

            await act.Should().ThrowAsync<BusinessValidationErrorException>();
            await _mockRepository.DidNotReceive().MakeLiveAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task MakeLiveAsync_ZeroProcessedCount_ThrowsBusinessValidationError()
        {
            _mockRepository.HasFailedStagingAsync("user1").Returns(false);
            _mockRepository.MakeLiveAsync("user1").Returns((0, 0, 0));

            Func<Task> act = () => _sut.MakeLiveAsync("user1");

            await act.Should().ThrowAsync<BusinessValidationErrorException>();
        }

        [Fact]
        public async Task MakeLiveAsync_AllImported_ReturnsSuccessResult()
        {
            _mockRepository.HasFailedStagingAsync("user1").Returns(false);
            _mockRepository.MakeLiveAsync("user1").Returns((5, 5, 0));

            var result = await _sut.MakeLiveAsync("user1");

            result.ProcessedCount.Should().Be(5);
            result.ImportedCount.Should().Be(5);
            result.FailedCount.Should().Be(0);
            result.Message.Should().Contain("5 of 5 records have been successfully made live");
            result.Message.Should().NotContain("revalidation");
        }

        [Fact]
        public async Task MakeLiveAsync_SomeFailed_IncludesRevalidationMessage()
        {
            _mockRepository.HasFailedStagingAsync("user1").Returns(false);
            _mockRepository.MakeLiveAsync("user1").Returns((10, 7, 3));

            var result = await _sut.MakeLiveAsync("user1");

            result.ProcessedCount.Should().Be(10);
            result.ImportedCount.Should().Be(7);
            result.FailedCount.Should().Be(3);
            result.Message.Should().Contain("7 of 10 records have been successfully made live");
            result.Message.Should().Contain("3 records require revalidation");
        }

        #endregion
    }
}

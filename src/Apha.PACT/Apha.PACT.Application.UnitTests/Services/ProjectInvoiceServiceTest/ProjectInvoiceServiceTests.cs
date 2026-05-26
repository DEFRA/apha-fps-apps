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
using Xunit;

namespace Apha.PACT.Application.UnitTests.Services.ProjectInvoiceServiceTest
{
    public class ProjectInvoiceServiceTests
    {
        private readonly IProjectInvoiceRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectInvoiceService _sut;

        public ProjectInvoiceServiceTests()
        {
            _mockRepository = Substitute.For<IProjectInvoiceRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectInvoiceService(_mockRepository, _mockMapper);
        }

        #region GetPagedProjectInvoicesAsync

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectInvoice>(new List<ProjectInvoice>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectInvoiceDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectInvoicesAsync(mappedParams, "PRJ1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedProjectInvoicesAsync(query, "PRJ1");

            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedProjectInvoicesAsync(mappedParams, "PRJ1");
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_NullParentProject_PassesNullToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectInvoice>(Array.Empty<ProjectInvoice>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectInvoiceDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectInvoicesAsync(mappedParams, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedProjectInvoicesAsync(query, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedProjectInvoicesAsync(mappedParams, null);
        }

        #endregion

        #region GetPagedProjectInvoicesByMonthAsync

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_ValidMonthAndQuery_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var month = 3;
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectInvoice>(
                new ProjectInvoice[] {
                    new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3 }
                },
                new PaginationData { TotalRecords = 1 });
            var pagedResult = new PaginatedResult<ProjectInvoiceDto>
            {
                Data = new List<ProjectInvoiceDto>
                {
                    new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 3 }
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectInvoicesByMonthAsync(mappedParams, month).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            result.Should().Be(pagedResult);
            result.Data.Should().HaveCount(1);
            result.Data.First().Month.Should().Be(3);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedProjectInvoicesByMonthAsync(mappedParams, month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_NullMonth_ReturnsAllInvoices()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            int? month = null;
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectInvoice>(
                new ProjectInvoice[]
                {
                    new ProjectInvoice { InvoiceCounter = 1, Month = 3 },
                    new ProjectInvoice { InvoiceCounter = 2, Month = 6 }
                },
                new PaginationData { TotalRecords = 2 });
            var pagedResult = new PaginatedResult<ProjectInvoiceDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectInvoicesByMonthAsync(mappedParams, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedProjectInvoicesByMonthAsync(mappedParams, null);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_MonthLessThan1_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var month = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetPagedProjectInvoicesByMonthAsync(query, month));

            exception.Message.Should().Contain("Month must be between 1 and 12");
            exception.ParamName.Should().Be("month");
            await _mockRepository.DidNotReceive().GetPagedProjectInvoicesByMonthAsync(
                Arg.Any<PaginationParameters<string>>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_MonthGreaterThan12_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var month = 13;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetPagedProjectInvoicesByMonthAsync(query, month));

            exception.Message.Should().Contain("Month must be between 1 and 12");
            exception.ParamName.Should().Be("month");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(6)]
        [InlineData(12)]
        public async Task GetPagedProjectInvoicesByMonthAsync_ValidMonthRange_CallsRepository(int month)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectInvoice>(new List<ProjectInvoice>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectInvoiceDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectInvoicesByMonthAsync(mappedParams, month).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).GetPagedProjectInvoicesByMonthAsync(mappedParams, month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_WithFilters_PassesFiltersToRepository()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"ProjectParent\":\"CORE\"}"
            };
            var month = 3;
            var mappedParams = new PaginationParameters<string> { Filter = "{\"ProjectParent\":\"CORE\"}" };
            var pagedData = new PagedData<ProjectInvoice>(Array.Empty<ProjectInvoice>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectInvoiceDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectInvoicesByMonthAsync(mappedParams, month).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).GetPagedProjectInvoicesByMonthAsync(
                Arg.Is<PaginationParameters<string>>(p => p.Filter == "{\"ProjectParent\":\"CORE\"}"),
                month);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_EmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var month = 12;
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectInvoice>(
                Array.Empty<ProjectInvoice>(),
                new PaginationData { TotalRecords = 0 });
            var pagedResult = new PaginatedResult<ProjectInvoiceDto>
            {
                Data = new List<ProjectInvoiceDto>(),
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectInvoicesByMonthAsync(mappedParams, month).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetPagedProjectInvoicesByMonthAsync(query, month);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesByMonthAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var month = 3;
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectInvoicesByMonthAsync(mappedParams, month)
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _sut.GetPagedProjectInvoicesByMonthAsync(query, month));
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_ValidParentProject_ReturnsTotalAmount()
        {
            _mockRepository.GetTotalAmountAsync("PRJ1").Returns(1500.00m);

            var result = await _sut.GetTotalAmountAsync("PRJ1");

            result.Should().Be(1500.00m);
            await _mockRepository.Received(1).GetTotalAmountAsync("PRJ1");
        }

        [Fact]
        public async Task GetTotalAmountAsync_NullParentProject_ReturnsTotalAmount()
        {
            _mockRepository.GetTotalAmountAsync(null).Returns(0m);

            var result = await _sut.GetTotalAmountAsync(null);

            result.Should().Be(0m);
            await _mockRepository.Received(1).GetTotalAmountAsync(null);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsMappedDto()
        {
            var entity = new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ1" };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1" };

            _mockRepository.GetByIdAsync(1).Returns(entity);
            _mockMapper.Map<ProjectInvoiceDto>(entity).Returns(dto);

            var result = await _sut.GetByIdAsync(1);

            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByIdAsync(1);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetByIdAsync(99).Returns((ProjectInvoice?)null);

            var result = await _sut.GetByIdAsync(99);

            result.Should().BeNull();
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ1", Month = 1, Amount = 1000m };
            var entity = new ProjectInvoice { ProjectParent = "PRJ1", Month = 1, Amount = 1000m };
            var created = new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 1, Amount = 1000m };
            var expected = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 1, Amount = 1000m };

            _mockMapper.Map<ProjectInvoice>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<ProjectInvoiceDto>(created).Returns(expected);

            var result = await _sut.CreateAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<ProjectInvoice>(dto);
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_MissingProjectParent_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectInvoiceDto { ProjectParent = "", Month = 1, Amount = 1000m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_MissingMonth_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ1", Month = null, Amount = 1000m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTH_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_MissingAmount_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ1", Month = 1, Amount = null };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "AMOUNT_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ1", Month = 1, Amount = 1000m };
            var entity = new ProjectInvoice { ProjectParent = "PRJ1", Month = 1, Amount = 1000m };

            _mockMapper.Map<ProjectInvoice>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(dto));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 1, Amount = 2000m };
            var entity = new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 1, Amount = 2000m };
            var updated = new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 1, Amount = 2000m };
            var expected = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 1, Amount = 2000m };

            _mockMapper.Map<ProjectInvoice>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).Returns(updated);
            _mockMapper.Map<ProjectInvoiceDto>(updated).Returns(expected);

            var result = await _sut.UpdateAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<ProjectInvoice>(dto);
            await _mockRepository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_MissingProjectParent_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "", Month = 1, Amount = 2000m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_MissingMonth_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = null, Amount = 2000m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTH_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_MissingAmount_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 1, Amount = null };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "AMOUNT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 1, Amount = 2000m };
            var entity = new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ1", Month = 1, Amount = 2000m };

            _mockMapper.Map<ProjectInvoice>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(dto));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingId_ReturnsTrue()
        {
            _mockRepository.DeleteAsync(1).Returns(true);

            var result = await _sut.DeleteAsync(1);

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ReturnsFalse()
        {
            _mockRepository.DeleteAsync(99).Returns(false);

            var result = await _sut.DeleteAsync(99);

            result.Should().BeFalse();
        }

        #endregion

        #region GetMonthlyInvoicesSummaryAsync

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_ValidQuery_ReturnsPivotedData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var summaryData = new List<MonthlyInvoicesSummary>
            {
                new MonthlyInvoicesSummary
                {
                    FpsYear = 2024,
                    Program = "ADMIN",
                    ParentProject = "PRJ1",
                    Month = 1,
                    MonthlyAmount = 1000m
                },
                new MonthlyInvoicesSummary
                {
                    FpsYear = 2024,
                    Program = "ADMIN",
                    ParentProject = "PRJ1",
                    Month = 2,
                    MonthlyAmount = 1500m
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlyInvoicesSummaryAsync(mappedParams).Returns(summaryData);

            // Act
            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Months.Should().ContainInOrder(1, 2);
            result.Rows.Should().HaveCount(1);
            result.Rows.First().Program.Should().Be("ADMIN");
            result.Rows.First().ParentProject.Should().Be("PRJ1");
            result.Rows.First().MonthlyAmounts.Should().ContainKeys(1, 2);
            await _mockRepository.Received(1).GetMonthlyInvoicesSummaryAsync(mappedParams);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_MultipleProjects_GroupsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var summaryData = new List<MonthlyInvoicesSummary>
            {
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ1", Month = 1, MonthlyAmount = 1000m },
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ2", Month = 1, MonthlyAmount = 2000m },
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "CORE", ParentProject = "PRJ3", Month = 1, MonthlyAmount = 3000m }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlyInvoicesSummaryAsync(mappedParams).Returns(summaryData);

            // Act
            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            result.Rows.Should().HaveCount(3);
            result.Rows.Should().Contain(r => r.Program == "ADMIN" && r.ParentProject == "PRJ1");
            result.Rows.Should().Contain(r => r.Program == "ADMIN" && r.ParentProject == "PRJ2");
            result.Rows.Should().Contain(r => r.Program == "CORE" && r.ParentProject == "PRJ3");
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithSortByProgram_SortsCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Program",
                Descending = false
            };
            var mappedParams = new PaginationParameters<string>();
            var summaryData = new List<MonthlyInvoicesSummary>
            {
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "CORE", ParentProject = "PRJ1", Month = 1, MonthlyAmount = 1000m },
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ2", Month = 1, MonthlyAmount = 2000m }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlyInvoicesSummaryAsync(mappedParams).Returns(summaryData);

            // Act
            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            result.Rows.First().Program.Should().Be("ADMIN");
            result.Rows.Last().Program.Should().Be("CORE");
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithSortByMonthColumn_SortsByMonthAmount()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "M3", // Month 3
                Descending = true
            };
            var mappedParams = new PaginationParameters<string>();
            var summaryData = new List<MonthlyInvoicesSummary>
            {
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ1", Month = 3, MonthlyAmount = 1000m },
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ2", Month = 3, MonthlyAmount = 3000m },
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ3", Month = 3, MonthlyAmount = 2000m }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlyInvoicesSummaryAsync(mappedParams).Returns(summaryData);

            // Act
            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            var firstRow = result.Rows.First();
            firstRow.MonthlyAmounts[3].Should().Be(3000m);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 2
            };
            var mappedParams = new PaginationParameters<string>();
            var summaryData = new List<MonthlyInvoicesSummary>
            {
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ1", Month = 1, MonthlyAmount = 1000m },
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ2", Month = 1, MonthlyAmount = 2000m },
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ3", Month = 1, MonthlyAmount = 3000m },
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ4", Month = 1, MonthlyAmount = 4000m },
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ5", Month = 1, MonthlyAmount = 5000m }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlyInvoicesSummaryAsync(mappedParams).Returns(summaryData);

            // Act
            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            result.Rows.Should().HaveCount(2);
            result.Pagination.PageNumber.Should().Be(2);
            result.Pagination.TotalRecords.Should().Be(5);
            result.Pagination.TotalPages.Should().Be(3);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_EmptyData_ReturnsEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var summaryData = new List<MonthlyInvoicesSummary>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlyInvoicesSummaryAsync(mappedParams).Returns(summaryData);

            // Act
            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            result.Months.Should().BeEmpty();
            result.Rows.Should().BeEmpty();
            result.Pagination.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_InvalidPageSize_UsesDefault()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 0 // Invalid
            };
            var mappedParams = new PaginationParameters<string>();
            var summaryData = new List<MonthlyInvoicesSummary>
            {
                new MonthlyInvoicesSummary { FpsYear = 2024, Program = "ADMIN", ParentProject = "PRJ1", Month = 1, MonthlyAmount = 1000m }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlyInvoicesSummaryAsync(mappedParams).Returns(summaryData);

            // Act
            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            result.Pagination.PageSize.Should().Be(10); // Default value
        }

        #endregion

        #region CopyInvoicesAsync

        [Fact]
        public async Task CopyInvoicesAsync_WithInvoiceRecords_CopiesProvidedInvoices()
        {
            // Arrange
            var sourceInvoiceDto = new ProjectInvoiceDto
            {
                InvoiceCounter = 1,
                ProjectParent = "PRJ001",
                Month = 5,
                Amount = 1000m,
                Detail = "Test Invoice"
            };

            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceRecords = new List<ProjectInvoiceDto> { sourceInvoiceDto }
            };

            var mappedEntity = new ProjectInvoice
            {
                InvoiceCounter = 1,
                ProjectParent = "PRJ001",
                Month = 5,
                Amount = 1000m,
                Detail = "Test Invoice"
            };

            _mockMapper.Map<ProjectInvoice>(sourceInvoiceDto).Returns(mappedEntity);
            _mockRepository.CreateBulkAsync(Arg.Any<IEnumerable<ProjectInvoice>>()).Returns(1);

            // Act
            var result = await _sut.CopyInvoicesAsync(copyDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.CopiedCount.Should().Be(1);
            result.Errors.Should().BeEmpty();
            await _mockRepository.Received(1).CreateBulkAsync(Arg.Is<IEnumerable<ProjectInvoice>>(
                invoices => invoices.Count() == 1 && invoices.First().Month == 6));
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithInvoiceIds_FetchesAndCopiesInvoices()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceIds = new List<int> { 1, 2 }
            };

            var invoicesToCopy = new List<ProjectInvoice>
            {
                new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ001", Month = 5, Amount = 1000m },
                new ProjectInvoice { InvoiceCounter = 2, ProjectParent = "PRJ002", Month = 5, Amount = 2000m }
            };

            _mockRepository.GetInvoicesByIdsAsync(copyDto.InvoiceIds).Returns(invoicesToCopy);
            _mockRepository.CreateBulkAsync(Arg.Any<IEnumerable<ProjectInvoice>>()).Returns(2);

            // Act
            var result = await _sut.CopyInvoicesAsync(copyDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.CopiedCount.Should().Be(2);
            await _mockRepository.Received(1).GetInvoicesByIdsAsync(copyDto.InvoiceIds);
            await _mockRepository.Received(1).CreateBulkAsync(Arg.Is<IEnumerable<ProjectInvoice>>(
                invoices => invoices.Count() == 2 && invoices.All(i => i.Month == 6)));
        }

        [Fact]
        public async Task CopyInvoicesAsync_BulkCopy_FetchesAllInvoicesFromSourceMonth()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6
            };

            var invoicesToCopy = new List<ProjectInvoice>
            {
                new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ001", Month = 5, Amount = 1000m },
                new ProjectInvoice { InvoiceCounter = 2, ProjectParent = "PRJ002", Month = 5, Amount = 2000m },
                new ProjectInvoice { InvoiceCounter = 3, ProjectParent = "PRJ003", Month = 5, Amount = 3000m }
            };

            _mockRepository.GetInvoicesByMonthAsync(5).Returns(invoicesToCopy);
            _mockRepository.CreateBulkAsync(Arg.Any<IEnumerable<ProjectInvoice>>()).Returns(3);

            // Act
            var result = await _sut.CopyInvoicesAsync(copyDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.CopiedCount.Should().Be(3);
            await _mockRepository.Received(1).GetInvoicesByMonthAsync(5);
            await _mockRepository.Received(1).CreateBulkAsync(Arg.Is<IEnumerable<ProjectInvoice>>(
                invoices => invoices.Count() == 3 && invoices.All(i => i.Month == 6)));
        }

        [Fact]
        public async Task CopyInvoicesAsync_InvalidSourceMonth_ReturnsErrorResult()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 13, // Invalid
                TargetMonth = 6
            };

            // Act
            var result = await _sut.CopyInvoicesAsync(copyDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("Source month must be between 1 and 12");
            await _mockRepository.DidNotReceive().GetInvoicesByMonthAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task CopyInvoicesAsync_InvalidTargetMonth_ReturnsErrorResult()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 0 // Invalid
            };

            // Act
            var result = await _sut.CopyInvoicesAsync(copyDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("Target month must be between 1 and 12");
        }

        [Fact]
        public async Task CopyInvoicesAsync_NoInvoicesFound_ReturnsErrorResult()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6
            };

            _mockRepository.GetInvoicesByMonthAsync(5).Returns(new List<ProjectInvoice>());

            // Act
            var result = await _sut.CopyInvoicesAsync(copyDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain($"No invoices found for source month {copyDto.SourceMonth}");
        }

        [Fact]
        public async Task CopyInvoicesAsync_WithInvoiceIds_NoInvoicesFound_ReturnsErrorResult()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceIds = new List<int> { 999, 1000 }
            };

            _mockRepository.GetInvoicesByIdsAsync(copyDto.InvoiceIds).Returns(new List<ProjectInvoice>());

            // Act
            var result = await _sut.CopyInvoicesAsync(copyDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("No invoices found with the provided IDs");
        }

        #endregion

        #region Additional Edge Case Tests

        [Fact]
        public async Task CreateAsync_MultipleValidationErrors_ThrowsWithAllErrors()
        {
            // Arrange
            var dto = new ProjectInvoiceDto
            {
                ProjectParent = "",   // Missing
                Month = null,         // Missing
                Amount = null         // Missing
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().HaveCount(3);
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "MONTH_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "AMOUNT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_MultipleValidationErrors_ThrowsWithAllErrors()
        {
            // Arrange
            var dto = new ProjectInvoiceDto
            {
                InvoiceCounter = 1,
                ProjectParent = string.Empty,  // Missing
                Month = null,          // Missing
                Amount = null          // Missing
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().HaveCount(3);
        }

        [Fact]
        public async Task CreateAsync_WhitespaceProjectParent_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new ProjectInvoiceDto
            {
                ProjectParent = "   ", // Whitespace only
                Month = 1,
                Amount = 1000m
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_WhitespaceProjectParent_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new ProjectInvoiceDto
            {
                InvoiceCounter = 1,
                ProjectParent = "   ",
                Month = 1,
                Amount = 2000m
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_EmptyParentProject_PassesEmptyStringToRepository()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectInvoice>(Array.Empty<ProjectInvoice>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectInvoiceDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectInvoicesAsync(mappedParams, "").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetPagedProjectInvoicesAsync(query, "");

            // Assert
            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedProjectInvoicesAsync(mappedParams, "");
        }

        [Fact]
        public async Task CopyInvoicesAsync_CopiesAllProperties_ExceptInvoiceCounter()
        {
            // Arrange
            var sourceInvoiceDto = new ProjectInvoiceDto
            {
                InvoiceCounter = 100,
                ProjectParent = "PRJ001",
                Month = 5,
                Amount = 1000m,
                CostOfWork = 600m,
                Wip = 200m,
                ProfitLoss = 200m,
                Detail = "Test Detail"
            };

            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6,
                InvoiceRecords = new List<ProjectInvoiceDto> { sourceInvoiceDto }
            };

            var mappedEntity = new ProjectInvoice
            {
                InvoiceCounter = 100,
                ProjectParent = "PRJ001",
                Month = 5,
                Amount = 1000m,
                CostOfWork = 600m,
                Wip = 200m,
                ProfitLoss = 200m,
                Detail = "Test Detail"
            };

            _mockMapper.Map<ProjectInvoice>(sourceInvoiceDto).Returns(mappedEntity);
            _mockRepository.CreateBulkAsync(Arg.Any<IEnumerable<ProjectInvoice>>()).Returns(1);

            // Act
            var result = await _sut.CopyInvoicesAsync(copyDto);

            // Assert
            await _mockRepository.Received(1).CreateBulkAsync(Arg.Is<IEnumerable<ProjectInvoice>>(invoices =>
                invoices.First().Month == 6 &&
                invoices.First().ProjectParent == "PRJ001" &&
                invoices.First().Amount == 1000m &&
                invoices.First().CostOfWork == 600m &&
                invoices.First().Wip == 200m &&
                invoices.First().ProfitLoss == 200m &&
                invoices.First().Detail == "Test Detail"));
        }

        [Fact]
        public async Task CopyInvoicesAsync_BulkCopy_SetsCorrectSuccessMessage()
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = 5,
                TargetMonth = 6
            };

            var invoicesToCopy = new List<ProjectInvoice>
            {
                new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ001", Month = 5 }
            };

            _mockRepository.GetInvoicesByMonthAsync(5).Returns(invoicesToCopy);
            _mockRepository.CreateBulkAsync(Arg.Any<IEnumerable<ProjectInvoice>>()).Returns(1);

            // Act
            var result = await _sut.CopyInvoicesAsync(copyDto);

            // Assert
            result.Message.Should().Contain("Successfully copied 1 invoice(s) from month 5 to month 6");
        }

        [Theory]
        [InlineData(-1, 6)]
        [InlineData(5, -1)]
        [InlineData(0, 6)]
        [InlineData(5, 0)]
        public async Task CopyInvoicesAsync_InvalidMonthValues_ReturnsErrorResult(int sourceMonth, int targetMonth)
        {
            // Arrange
            var copyDto = new CopyInvoicesDto
            {
                SourceMonth = sourceMonth,
                TargetMonth = targetMonth
            };

            // Act
            var result = await _sut.CopyInvoicesAsync(copyDto);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetByIdAsync(1).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetByIdAsync(1));
        }

        [Fact]
        public async Task DeleteAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.DeleteAsync(1).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(1));
        }

        [Fact]
        public async Task GetTotalAmountAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetTotalAmountAsync("PRJ1").ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetTotalAmountAsync("PRJ1"));
        }

        #endregion
    }
}

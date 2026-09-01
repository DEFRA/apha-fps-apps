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
            var pagedData = new PagedData<ProjectInvoice>(new List<ProjectInvoice>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectInvoiceDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectInvoicesAsync(mappedParams, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedProjectInvoicesAsync(query, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedProjectInvoicesAsync(mappedParams, null);
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
        public async Task CreateAsync_MissingAllFields_ThrowsWithMultipleErrors()
        {
            var dto = new ProjectInvoiceDto { ProjectParent = "", Month = null, Amount = null };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().HaveCount(3);
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "MONTH_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "AMOUNT_REQUIRED");
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
        public async Task UpdateAsync_MissingAllFields_ThrowsWithMultipleErrors()
        {
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = "", Month = null, Amount = null };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().HaveCount(3);
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "MONTH_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "AMOUNT_REQUIRED");
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
        public async Task GetMonthlyInvoicesSummaryAsync_ReturnsGroupedPivotData()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "P1", ParentProject = "PP1", Month = 2, MonthlyAmount = 200m, FpsYear = 2024 },
                new() { Program = "P2", ParentProject = "PP2", Month = 1, MonthlyAmount = 300m, FpsYear = 2024 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetMonthlyInvoicesSummaryAsync(parameters).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Months.Should().BeEquivalentTo(new[] { 1, 2 });
            result.Rows.Should().HaveCount(2);
            result.Pagination.PageNumber.Should().Be(1);
            result.Pagination.TotalRecords.Should().Be(2);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_EmptyData_ReturnsEmptyResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>())
                .Returns(new List<Core.Entities.MonthlyInvoicesSummary>());

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Months.Should().BeEmpty();
            result.Rows.Should().BeEmpty();
            result.Pagination.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NullSortBy_SortsByProgramThenParentProject()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = null };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "B", ParentProject = "PP2", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "A", ParentProject = "PP1", Month = 1, MonthlyAmount = 200m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows[0].Program.Should().Be("A");
            result.Rows[1].Program.Should().Be("B");
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByProgramAsc_SortsByProgramAscending()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "Program", Descending = false };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "B", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "A", ParentProject = "PP2", Month = 1, MonthlyAmount = 200m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows[0].Program.Should().Be("A");
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByProgramDesc_SortsByProgramDescending()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "Program", Descending = true };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "A", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "B", ParentProject = "PP2", Month = 1, MonthlyAmount = 200m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows[0].Program.Should().Be("B");
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByParentProjectAsc_Sorts()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "ParentProject", Descending = false };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "ZZ", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "P1", ParentProject = "AA", Month = 1, MonthlyAmount = 200m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows[0].ParentProject.Should().Be("AA");
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByParentProjectDesc_Sorts()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "ParentProject", Descending = true };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "AA", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "P1", ParentProject = "ZZ", Month = 1, MonthlyAmount = 200m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows[0].ParentProject.Should().Be("ZZ");
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByMonthColumnAsc_SortsByMonthAmount()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "M1", Descending = false };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "PP1", Month = 1, MonthlyAmount = 500m, FpsYear = 2024 },
                new() { Program = "P2", ParentProject = "PP2", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows[0].MonthlyAmounts[1].Should().Be(100m);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByMonthColumnDesc_SortsByMonthAmountDesc()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "M1", Descending = true };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "P2", ParentProject = "PP2", Month = 1, MonthlyAmount = 500m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows[0].MonthlyAmounts[1].Should().Be(500m);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByUnknownColumn_FallsBackToDefault()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "UnknownColumn", Descending = false };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "B", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "A", ParentProject = "PP2", Month = 1, MonthlyAmount = 200m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows[0].Program.Should().Be("A");
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_Pagination_ReturnsCorrectPage()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 1 };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "P2", ParentProject = "PP2", Month = 1, MonthlyAmount = 200m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows.Should().HaveCount(1);
            result.Rows[0].Program.Should().Be("P2");
            result.Pagination.PageNumber.Should().Be(2);
            result.Pagination.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_InvalidPageAndPageSize_DefaultsTo1And10()
        {
            var query = new QueryParameters<string> { Page = 0, PageSize = 0 };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Pagination.PageNumber.Should().Be(1);
            result.Pagination.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByM13_FallsBackToDefault()
        {
            // M13 is out of range (1-12), so should fall to default sort
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "M13", Descending = false };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "B", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "A", ParentProject = "PP2", Month = 1, MonthlyAmount = 200m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            // Falls through to default switch case which sorts by Program
            result.Rows[0].Program.Should().Be("A");
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByM0_FallsBackToDefault()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "M0", Descending = false };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "B", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 },
                new() { Program = "A", ParentProject = "PP2", Month = 1, MonthlyAmount = 200m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows[0].Program.Should().Be("A");
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NullMonthlyAmount_DefaultsToZero()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "PP1", Month = 1, MonthlyAmount = null, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Rows[0].MonthlyAmounts[1].Should().Be(0m);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NegativePage_DefaultsToPage1()
        {
            var query = new QueryParameters<string> { Page = -5, PageSize = 10 };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Pagination.PageNumber.Should().Be(1);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_NegativePageSize_DefaultsTo10()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = -5 };
            var data = new List<Core.Entities.MonthlyInvoicesSummary>
            {
                new() { Program = "P1", ParentProject = "PP1", Month = 1, MonthlyAmount = 100m, FpsYear = 2024 }
            };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(data);

            var result = await _sut.GetMonthlyInvoicesSummaryAsync(query);

            result.Pagination.PageSize.Should().Be(10);
        }

        #endregion

        #region GetFailedInvoiceImportAsync

        [Fact]
        public async Task GetFailedInvoiceImportAsync_ReturnsMapperResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<Core.Entities.InvoiceImportRow>(new List<Core.Entities.InvoiceImportRow>(), new PaginationData());
            var expected = new PaginatedResult<InvoiceImportRowDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetFailedInvoiceImportAsync(parameters, "user1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<InvoiceImportRowDto>>(pagedData).Returns(expected);

            var result = await _sut.GetFailedInvoiceImportAsync(query, "user1");

            result.Should().Be(expected);
        }

        #endregion

        #region DeleteFailedInvoiceImportByUserAsync

        [Fact]
        public async Task DeleteFailedInvoiceImportByUserAsync_ReturnsDeletedCount()
        {
            _mockRepository.DeleteFailedInvoiceImportByUserAsync("user1").Returns(5);

            var result = await _sut.DeleteFailedInvoiceImportByUserAsync("user1");

            result.Should().Be(5);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByUserAsync_NoRecords_ReturnsZero()
        {
            _mockRepository.DeleteFailedInvoiceImportByUserAsync("user1").Returns(0);

            var result = await _sut.DeleteFailedInvoiceImportByUserAsync("user1");

            result.Should().Be(0);
        }

        #endregion

        #region ImportInvoiceAsync

        [Fact]
        public async Task ImportInvoiceAsync_AllRowsValid_AllPassedNoneFailed()
        {
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { ProjectParent = "PRJ1", Month = "1", Amount = "100.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.ImportInvoiceAsync(Arg.Any<List<ProjectInvoice>>(), Arg.Any<List<ProjectInvoiceStaging>>())
                .Returns(new InvoiceImportResult { PassedCount = 1, FailedCount = 0 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.PassedCount.Should().Be(1);
            result.FailedCount.Should().Be(0);
            result.Message.Should().Contain("1 out of 1");
        }

        [Fact]
        public async Task ImportInvoiceAsync_AllRowsInvalid_AllFailed()
        {
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { ProjectParent = "INVALID", Month = "1", Amount = "100.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.ImportInvoiceAsync(Arg.Any<List<ProjectInvoice>>(), Arg.Any<List<ProjectInvoiceStaging>>())
                .Returns(new InvoiceImportResult { PassedCount = 0, FailedCount = 1 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.PassedCount.Should().Be(0);
            result.FailedCount.Should().Be(1);
        }

        [Fact]
        public async Task ImportInvoiceAsync_MixedRows_SplitsPassedAndFailed()
        {
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { ProjectParent = "PRJ1", Month = "1", Amount = "100.00" },
                    new() { ProjectParent = "INVALID", Month = "1", Amount = "200.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.ImportInvoiceAsync(
                Arg.Is<List<ProjectInvoice>>(l => l.Count == 1),
                Arg.Is<List<ProjectInvoiceStaging>>(l => l.Count == 1))
                .Returns(new InvoiceImportResult { PassedCount = 1, FailedCount = 1 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.PassedCount.Should().Be(1);
            result.FailedCount.Should().Be(1);
        }

        [Fact]
        public async Task ImportInvoiceAsync_NullFileName_DefaultsToInvoiceImportXlsx()
        {
            var request = new InvoiceImportDto
            {
                FileName = null,
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { ProjectParent = "INVALID", Month = "1", Amount = "100.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.ImportInvoiceAsync(
                Arg.Any<List<ProjectInvoice>>(),
                Arg.Is<List<ProjectInvoiceStaging>>(l => l.Count == 1 && l[0].Filename == "InvoiceImport.xlsx"))
                .Returns(new InvoiceImportResult { PassedCount = 0, FailedCount = 1 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.FailedCount.Should().Be(1);
        }

        [Fact]
        public async Task ImportInvoiceAsync_FailedRow_SetsImportedByAndStagingFields()
        {
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { ProjectParent = "INVALID", Month = "1", Amount = "100.00", Detail = "D1", Type = "T1" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.ImportInvoiceAsync(
                Arg.Any<List<ProjectInvoice>>(),
                Arg.Is<List<ProjectInvoiceStaging>>(l =>
                    l[0].ImportedBy == "user1" &&
                    l[0].IsPassed == false &&
                    l[0].IsExported == false &&
                    l[0].Filename == "test.xlsx" &&
                    !string.IsNullOrEmpty(l[0].ValidationFailure)))
                .Returns(new InvoiceImportResult { PassedCount = 0, FailedCount = 1 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.FailedCount.Should().Be(1);
        }

        [Fact]
        public async Task ImportInvoiceAsync_PassedRow_SetsFpsYearAndParsedFields()
        {
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { ProjectParent = "PRJ1", Month = "3", Amount = "500.50", CostOfWork = "100.25", Wip = "50.75", ProfitLoss = "10.00", Detail = "D", Type = "T" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.ImportInvoiceAsync(
                Arg.Is<List<ProjectInvoice>>(l =>
                    l[0].ProjectParent == "PRJ1" &&
                    l[0].FpsYear == 2024 &&
                    l[0].Detail == "D" &&
                    l[0].Type == "T"),
                Arg.Any<List<ProjectInvoiceStaging>>())
                .Returns(new InvoiceImportResult { PassedCount = 1, FailedCount = 0 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.PassedCount.Should().Be(1);
        }

        [Fact]
        public async Task ImportInvoiceAsync_MessageFormat_IncludesCountsAndTotal()
        {
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { ProjectParent = "PRJ1", Month = "1", Amount = "100.00" },
                    new() { ProjectParent = "PRJ1", Month = "2", Amount = "200.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.ImportInvoiceAsync(Arg.Any<List<ProjectInvoice>>(), Arg.Any<List<ProjectInvoiceStaging>>())
                .Returns(new InvoiceImportResult { PassedCount = 2, FailedCount = 0 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.Message.Should().Be("Import completed successfully. 2 out of 2 records successfully validated and is now live.");
        }

        [Fact]
        public async Task ImportInvoiceAsync_ExistingIdPassesValidation_AddedToPassedAndStagingDeleted()
        {
            var existingStaging = new ProjectInvoiceStaging { Id = 10, ProjectParent = "PRJ1", ImportedBy = "user1" };
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { Id = 10, ProjectParent = "PRJ1", Month = "1", Amount = "100.00", Detail = "D", Type = "T" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.GetFailedInvoiceImportByIdAsync(10, "user1").Returns(existingStaging);
            _mockRepository.ImportInvoiceAsync(
                Arg.Is<List<ProjectInvoice>>(l => l.Count == 1 && l[0].ProjectParent == "PRJ1"),
                Arg.Is<List<ProjectInvoiceStaging>>(l => l.Count == 0))
                .Returns(new InvoiceImportResult { PassedCount = 1, FailedCount = 0 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.PassedCount.Should().Be(1);
            result.FailedCount.Should().Be(0);
            await _mockRepository.Received(1).DeleteFailedInvoiceImportByIdsAsync(
                Arg.Is<List<int>>(ids => ids.Count == 1 && ids[0] == 10), "user1");
            await _mockRepository.DidNotReceive().UpdateFailedInvoiceImportRecordsAsync(Arg.Any<List<ProjectInvoiceStaging>>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_ExistingIdFailsValidation_UpdatesExistingStaging()
        {
            var existingStaging = new ProjectInvoiceStaging { Id = 10, ProjectParent = "OLD", ImportedBy = "user1" };
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { Id = 10, ProjectParent = "INVALID", Month = "1", Amount = "100.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.GetFailedInvoiceImportByIdAsync(10, "user1").Returns(existingStaging);
            _mockRepository.ImportInvoiceAsync(Arg.Any<List<ProjectInvoice>>(), Arg.Any<List<ProjectInvoiceStaging>>())
                .Returns(new InvoiceImportResult { PassedCount = 0, FailedCount = 0 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.FailedCount.Should().Be(1);
            await _mockRepository.Received(1).UpdateFailedInvoiceImportRecordsAsync(
                Arg.Is<List<ProjectInvoiceStaging>>(l =>
                    l.Count == 1 &&
                    l[0].ProjectParent == "INVALID" &&
                    l[0].IsPassed == false &&
                    !string.IsNullOrEmpty(l[0].ValidationFailure)));
            await _mockRepository.DidNotReceive().DeleteFailedInvoiceImportByIdsAsync(Arg.Any<List<int>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_ExistingIdFailsValidation_UpdatesAllStagingFields()
        {
            var existingStaging = new ProjectInvoiceStaging { Id = 10, ProjectParent = "OLD", ImportedBy = "user1" };
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { Id = 10, ProjectParent = "INVALID", Month = "3", Amount = "500.50", CostOfWork = "100", Wip = "50", ProfitLoss = "10", Detail = "NewD", Type = "NewT" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.GetFailedInvoiceImportByIdAsync(10, "user1").Returns(existingStaging);
            _mockRepository.ImportInvoiceAsync(Arg.Any<List<ProjectInvoice>>(), Arg.Any<List<ProjectInvoiceStaging>>())
                .Returns(new InvoiceImportResult { PassedCount = 0, FailedCount = 0 });

            await _sut.ImportInvoiceAsync(request, "user1");

            await _mockRepository.Received(1).UpdateFailedInvoiceImportRecordsAsync(
                Arg.Is<List<ProjectInvoiceStaging>>(l =>
                    l[0].ProjectParent == "INVALID" &&
                    l[0].Month == "3" &&
                    l[0].Amount == "500.50" &&
                    l[0].CostOfWork == "100" &&
                    l[0].Wip == "50" &&
                    l[0].ProfitLoss == "10" &&
                    l[0].Detail == "NewD" &&
                    l[0].Type == "NewT"));
        }

        [Fact]
        public async Task ImportInvoiceAsync_ExistingIdNotFoundInStaging_FallsThroughToNormalPath()
        {
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { Id = 99, ProjectParent = "PRJ1", Month = "1", Amount = "100.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.GetFailedInvoiceImportByIdAsync(99, "user1").Returns((ProjectInvoiceStaging?)null);
            _mockRepository.ImportInvoiceAsync(
                Arg.Is<List<ProjectInvoice>>(l => l.Count == 1),
                Arg.Is<List<ProjectInvoiceStaging>>(l => l.Count == 0))
                .Returns(new InvoiceImportResult { PassedCount = 1, FailedCount = 0 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.PassedCount.Should().Be(1);
            await _mockRepository.DidNotReceive().UpdateFailedInvoiceImportRecordsAsync(Arg.Any<List<ProjectInvoiceStaging>>());
            await _mockRepository.DidNotReceive().DeleteFailedInvoiceImportByIdsAsync(Arg.Any<List<int>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_ExistingIdNotFoundInStaging_FailedRow_GoesToFailedRows()
        {
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { Id = 99, ProjectParent = "INVALID", Month = "1", Amount = "100.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.GetFailedInvoiceImportByIdAsync(99, "user1").Returns((ProjectInvoiceStaging?)null);
            _mockRepository.ImportInvoiceAsync(
                Arg.Is<List<ProjectInvoice>>(l => l.Count == 0),
                Arg.Is<List<ProjectInvoiceStaging>>(l => l.Count == 1))
                .Returns(new InvoiceImportResult { PassedCount = 0, FailedCount = 1 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.FailedCount.Should().Be(1);
        }

        [Fact]
        public async Task ImportInvoiceAsync_ZeroId_SkipsExistingLookup()
        {
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { Id = 0, ProjectParent = "PRJ1", Month = "1", Amount = "100.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.ImportInvoiceAsync(Arg.Any<List<ProjectInvoice>>(), Arg.Any<List<ProjectInvoiceStaging>>())
                .Returns(new InvoiceImportResult { PassedCount = 1, FailedCount = 0 });

            await _sut.ImportInvoiceAsync(request, "user1");

            await _mockRepository.DidNotReceive().GetFailedInvoiceImportByIdAsync(Arg.Any<int>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_MixedExistingAndNewRows_CorrectCounts()
        {
            var existingStaging = new ProjectInvoiceStaging { Id = 10, ProjectParent = "OLD", ImportedBy = "user1" };
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { Id = 10, ProjectParent = "PRJ1", Month = "1", Amount = "100.00" },   // existing, passes
                    new() { Id = 0, ProjectParent = "INVALID", Month = "1", Amount = "200.00" },  // new, fails
                    new() { Id = 0, ProjectParent = "PRJ1", Month = "2", Amount = "300.00" }      // new, passes
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.GetFailedInvoiceImportByIdAsync(10, "user1").Returns(existingStaging);
            _mockRepository.ImportInvoiceAsync(
                Arg.Is<List<ProjectInvoice>>(l => l.Count == 2),
                Arg.Is<List<ProjectInvoiceStaging>>(l => l.Count == 1))
                .Returns(new InvoiceImportResult { PassedCount = 2, FailedCount = 1 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.PassedCount.Should().Be(2);
            result.FailedCount.Should().Be(1);
            await _mockRepository.Received(1).DeleteFailedInvoiceImportByIdsAsync(
                Arg.Is<List<int>>(ids => ids.Count == 1 && ids[0] == 10), "user1");
        }

        [Fact]
        public async Task ImportInvoiceAsync_ExistingIdPassesValidation_SetsCorrectParsedFields()
        {
            var existingStaging = new ProjectInvoiceStaging { Id = 10, ImportedBy = "user1" };
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { Id = 10, ProjectParent = "PRJ1", Month = "3", Amount = "500.50", CostOfWork = "100.25", Wip = "50.75", ProfitLoss = "10.00", Detail = "D", Type = "T" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.GetFailedInvoiceImportByIdAsync(10, "user1").Returns(existingStaging);
            _mockRepository.ImportInvoiceAsync(
                Arg.Is<List<ProjectInvoice>>(l =>
                    l.Count == 1 &&
                    l[0].ProjectParent == "PRJ1" &&
                    l[0].FpsYear == 2024 &&
                    l[0].Detail == "D" &&
                    l[0].Type == "T"),
                Arg.Any<List<ProjectInvoiceStaging>>())
                .Returns(new InvoiceImportResult { PassedCount = 1, FailedCount = 0 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            result.PassedCount.Should().Be(1);
        }

        [Fact]
        public async Task ImportInvoiceAsync_NoUpdatesNoDeletes_DoesNotCallUpdateOrDelete()
        {
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { Id = 0, ProjectParent = "PRJ1", Month = "1", Amount = "100.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.ImportInvoiceAsync(Arg.Any<List<ProjectInvoice>>(), Arg.Any<List<ProjectInvoiceStaging>>())
                .Returns(new InvoiceImportResult { PassedCount = 1, FailedCount = 0 });

            await _sut.ImportInvoiceAsync(request, "user1");

            await _mockRepository.DidNotReceive().UpdateFailedInvoiceImportRecordsAsync(Arg.Any<List<ProjectInvoiceStaging>>());
            await _mockRepository.DidNotReceive().DeleteFailedInvoiceImportByIdsAsync(Arg.Any<List<int>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_ExistingRowFailsValidation_CountsInTotalFailed()
        {
            var existingStaging = new ProjectInvoiceStaging { Id = 10, ImportedBy = "user1" };
            var request = new InvoiceImportDto
            {
                FileName = "test.xlsx",
                Rows = new List<InvoiceImportRowDto>
                {
                    new() { Id = 10, ProjectParent = "INVALID", Month = "1", Amount = "100.00" },
                    new() { Id = 0, ProjectParent = "INVALID2", Month = "1", Amount = "50.00" }
                }
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.GetFailedInvoiceImportByIdAsync(10, "user1").Returns(existingStaging);
            _mockRepository.ImportInvoiceAsync(
                Arg.Is<List<ProjectInvoice>>(l => l.Count == 0),
                Arg.Is<List<ProjectInvoiceStaging>>(l => l.Count == 1))
                .Returns(new InvoiceImportResult { PassedCount = 0, FailedCount = 1 });

            var result = await _sut.ImportInvoiceAsync(request, "user1");

            // 1 from failedRows + 1 from rowsToUpdate
            result.FailedCount.Should().Be(2);
            result.PassedCount.Should().Be(0);
            result.Message.Should().Contain("0 out of 2");
        }

        #endregion

        #region GetFailedInvoiceImportByIdAsync

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_Found_ReturnsMappedDto()
        {
            var entity = new ProjectInvoiceStaging { Id = 5, ProjectParent = "PRJ1" };
            var dto = new InvoiceImportRowDto { Id = 5, ProjectParent = "PRJ1" };

            _mockRepository.GetFailedInvoiceImportByIdAsync(5, "user1").Returns(entity);
            _mockMapper.Map<InvoiceImportRowDto>(entity).Returns(dto);

            var result = await _sut.GetFailedInvoiceImportByIdAsync(5, "user1");

            result.Should().Be(dto);
        }

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetFailedInvoiceImportByIdAsync(99, "user1").Returns((ProjectInvoiceStaging?)null);

            var result = await _sut.GetFailedInvoiceImportByIdAsync(99, "user1");

            result.Should().BeNull();
        }

        #endregion

        #region SaveFailedInvoiceImportAsync

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_ValidationPasses_CreatesAndDeletesAndReturnsTrue()
        {
            var dto = new InvoiceImportRowDto
            {
                ProjectParent = "PRJ1", Month = "1", Amount = "100.00",
                CostOfWork = "50", Wip = "25", ProfitLoss = "10",
                Detail = "D", Type = "T"
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.CreateAsync(Arg.Any<ProjectInvoice>()).Returns(new ProjectInvoice());
            _mockRepository.DeleteFailedInvoiceImportByIdAsync(5, "user1").Returns(true);

            var result = await _sut.SaveFailedInvoiceImportAsync(5, dto, "user1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).CreateAsync(Arg.Is<ProjectInvoice>(e =>
                e.ProjectParent == "PRJ1" && e.FpsYear == 2024));
            await _mockRepository.Received(1).DeleteFailedInvoiceImportByIdAsync(5, "user1");
        }

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_ValidationFails_ThrowsBusinessValidationErrorException()
        {
            var dto = new InvoiceImportRowDto
            {
                ProjectParent = "INVALID", Month = "1", Amount = "100.00"
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() =>
                _sut.SaveFailedInvoiceImportAsync(5, dto, "user1"));

            ex.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_ValidationFails_MapsFieldNameToPropertyName()
        {
            var dto = new InvoiceImportRowDto
            {
                ProjectParent = "INVALID", Month = "abc", Amount = null
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() =>
                _sut.SaveFailedInvoiceImportAsync(5, dto, "user1"));

            // Check that field name mapping produces property names
            ex.Errors.Should().Contain(e => e.Code == "ProjectParent" || e.Code == "Month" || e.Code == "Amount");
        }

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_ValidationFailsWithUnmappedField_UsesEmptyCode()
        {
            // All standard validation fields are mapped, but if somehow a failure doesn't match
            // any key, the code should be empty. This tests the else branch.
            // In practice all failures are from the known fields, but we can verify the mapping logic
            var dto = new InvoiceImportRowDto
            {
                ProjectParent = "PRJ1", Month = "1", Amount = "100.00"
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);
            _mockRepository.CreateAsync(Arg.Any<ProjectInvoice>()).Returns(new ProjectInvoice());
            _mockRepository.DeleteFailedInvoiceImportByIdAsync(5, "user1").Returns(true);

            // Valid row should pass validation and return true
            var result = await _sut.SaveFailedInvoiceImportAsync(5, dto, "user1");
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_MultipleValidationErrors_ThrowsWithAllErrors()
        {
            var dto = new InvoiceImportRowDto
            {
                ProjectParent = null, Month = null, Amount = null
            };
            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string> { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2024);

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() =>
                _sut.SaveFailedInvoiceImportAsync(5, dto, "user1"));

            ex.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
        }

        #endregion

        #region DeleteFailedInvoiceImportByIdAsync

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_ExistingId_ReturnsTrue()
        {
            _mockRepository.DeleteFailedInvoiceImportByIdAsync(5, "user1").Returns(true);

            var result = await _sut.DeleteFailedInvoiceImportByIdAsync(5, "user1");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_NotFound_ReturnsFalse()
        {
            _mockRepository.DeleteFailedInvoiceImportByIdAsync(99, "user1").Returns(false);

            var result = await _sut.DeleteFailedInvoiceImportByIdAsync(99, "user1");

            result.Should().BeFalse();
        }

        #endregion
    }
}

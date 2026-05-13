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

namespace Apha.PACT.Application.UnitTests.Services.ProjectSubContractServiceTest
{
    public class ProjectSubContractServiceTests
    {
        private readonly IProjectSubContractRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectSubContractService _sut;

        public ProjectSubContractServiceTests()
        {
            _mockRepository = Substitute.For<IProjectSubContractRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectSubContractService(_mockRepository, _mockMapper);
        }

        #region GetPagedProjectSubContractsAsync

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectSubContract>(new List<ProjectSubContract>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectSubContractsAsync(mappedParams, "PRJ1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedProjectSubContractsAsync(query, "PRJ1");

            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedProjectSubContractsAsync(mappedParams, "PRJ1");
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_NullProject_PassesNullToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectSubContract>(new List<ProjectSubContract>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectSubContractsAsync(mappedParams, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedProjectSubContractsAsync(query, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedProjectSubContractsAsync(mappedParams, null);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_ValidProject_ReturnsTotalAmount()
        {
            _mockRepository.GetTotalAmountAsync("PRJ1").Returns(2500.00m);

            var result = await _sut.GetTotalAmountAsync("PRJ1");

            result.Should().Be(2500.00m);
            await _mockRepository.Received(1).GetTotalAmountAsync("PRJ1");
        }

        [Fact]
        public async Task GetTotalAmountAsync_NullProject_ReturnsTotalAmount()
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
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1" };
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1" };

            _mockRepository.GetByIdAsync(1).Returns(entity);
            _mockMapper.Map<ProjectSubContractDto>(entity).Returns(dto);

            var result = await _sut.GetByIdAsync(1);

            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByIdAsync(1);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetByIdAsync(99).Returns((ProjectSubContract?)null);

            var result = await _sut.GetByIdAsync(99);

            result.Should().BeNull();
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = 1.0, Amount = 500m };
            var entity = new ProjectSubContract { Project = "PRJ1", Month = 1.0, Amount = 500m };
            var created = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 500m };
            var expected = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 500m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<ProjectSubContractDto>(created).Returns(expected);

            var result = await _sut.CreateAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<ProjectSubContract>(dto);
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { Project = "", Month = 1.0, Amount = 500m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_MissingMonth_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = null, Amount = 500m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTH_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_MissingAmount_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = 1.0, Amount = null };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "AMOUNT_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = 1.0, Amount = 500m };
            var entity = new ProjectSubContract { Project = "PRJ1", Month = 1.0, Amount = 500m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(dto));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var updated = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var expected = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).Returns(updated);
            _mockMapper.Map<ProjectSubContractDto>(updated).Returns(expected);

            var result = await _sut.UpdateAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<ProjectSubContract>(dto);
            await _mockRepository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "", Month = 1.0, Amount = 750m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_MissingMonth_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = null, Amount = 750m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTH_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_MissingAmount_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = null };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "AMOUNT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(dto));
        }

        #endregion

        #region GetFpsProjectSubContractsAsync

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<ProjectSubContract> { new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals" } };
            var pagedData = new PagedData<ProjectSubContract>(entities, new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetFpsProjectSubContractsAsync(mappedParams, "PRJ1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetFpsProjectSubContractsAsync(query, "PRJ1");

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetFpsProjectSubContractsAsync(mappedParams, "PRJ1");
        }

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_NullProject_PassesNullToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectSubContract>(new List<ProjectSubContract>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetFpsProjectSubContractsAsync(mappedParams, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetFpsProjectSubContractsAsync(query, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetFpsProjectSubContractsAsync(mappedParams, null);
        }

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_RepositoryThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetFpsProjectSubContractsAsync(mappedParams, "PRJ1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetFpsProjectSubContractsAsync(query, "PRJ1"));
        }

        #endregion

        #region GetFpsProjectSubContractTotalAmountAsync

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_ValidProject_ReturnsTotalAmount()
        {
            _mockRepository.GetFpsProjectSubContractTotalAmountAsync("PRJ1").Returns(1500.00m);

            var result = await _sut.GetFpsProjectSubContractTotalAmountAsync("PRJ1");

            result.Should().Be(1500.00m);
            await _mockRepository.Received(1).GetFpsProjectSubContractTotalAmountAsync("PRJ1");
        }

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_NullProject_ReturnsTotalAmount()
        {
            _mockRepository.GetFpsProjectSubContractTotalAmountAsync(null).Returns(0m);

            var result = await _sut.GetFpsProjectSubContractTotalAmountAsync(null);

            result.Should().Be(0m);
            await _mockRepository.Received(1).GetFpsProjectSubContractTotalAmountAsync(null);
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

        #region GetMonthlySubContractsSummaryAsync

        private static MonthlySubContractsSummary MakeSummary(string program, string parentProject, double month, decimal? amount = null)
            => new() { FpsYear = 2024, Program = program, ParentProject = parentProject, Month = month, MonthlyAmount = amount };

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_ValidQuery_CallsRepositoryWithMappedParameters()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns([]);

            await _sut.GetMonthlySubContractsSummaryAsync(query);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetMonthlySubContractsSummaryAsync(mappedParams);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_EmptyData_ReturnsEmptyPivot()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns([]);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows.Should().BeEmpty();
            result.Months.Should().BeEmpty();
            result.Pagination.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_SingleRow_ReturnsCorrectPivotRow()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH", 1, 100m),
                MakeSummary("ADMIN", "AH", 2, 200m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows.Should().HaveCount(1);
            result.Rows[0].Program.Should().Be("ADMIN");
            result.Rows[0].ParentProject.Should().Be("AH");
            result.Rows[0].MonthlyAmounts[1].Should().Be(100m);
            result.Rows[0].MonthlyAmounts[2].Should().Be(200m);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_MultipleGroups_GroupsByProgramAndParentProject()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH",  1, 100m),
                MakeSummary("ADMIN", "AH",  2, 200m),
                MakeSummary("BETA",  "ZZ",  1, 300m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows.Should().HaveCount(2);
            result.Pagination.TotalRecords.Should().Be(2);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_DiscoverMonths_ReturnsDistinctOrderedMonths()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH", 3, 300m),
                MakeSummary("ADMIN", "AH", 1, 100m),
                MakeSummary("BETA",  "ZZ", 1, 200m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Months.Should().BeInAscendingOrder();
            result.Months.Should().Equal(1, 3);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_NullMonthlyAmount_TreatedAsZero()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH", 1, null)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows[0].MonthlyAmounts[1].Should().Be(0m);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_Pagination_ReturnsCorrectPage()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 1 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH", 1, 100m),
                MakeSummary("BETA",  "ZZ", 1, 200m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows.Should().HaveCount(1);
            result.Pagination.PageNumber.Should().Be(2);
            result.Pagination.PageSize.Should().Be(1);
            result.Pagination.TotalRecords.Should().Be(2);
            result.Pagination.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_PageLessThanOne_DefaultsToPageOne()
        {
            var query = new QueryParameters<string> { Page = 0, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns([]);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Pagination.PageNumber.Should().Be(1);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_PageSizeLessThanOne_DefaultsToTen()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 0 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns([]);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Pagination.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_NoSortBy_SortsByProgramThenParentProject()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ZETA",  "BB", 1, 300m),
                MakeSummary("ADMIN", "CC", 1, 100m),
                MakeSummary("ADMIN", "AA", 1, 200m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows[0].Program.Should().Be("ADMIN");
            result.Rows[0].ParentProject.Should().Be("AA");
            result.Rows[1].Program.Should().Be("ADMIN");
            result.Rows[1].ParentProject.Should().Be("CC");
            result.Rows[2].Program.Should().Be("ZETA");
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_SortByProgramDescending_SortsCorrectly()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "program", Descending = true };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH", 1, 100m),
                MakeSummary("ZETA",  "ZZ", 1, 200m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows[0].Program.Should().Be("ZETA");
            result.Rows[1].Program.Should().Be("ADMIN");
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_SortByMonthColumn_SortsByMonthlyAmount()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "M1", Descending = false };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ZETA",  "ZZ", 1, 500m),
                MakeSummary("ADMIN", "AH", 1, 100m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows[0].MonthlyAmounts[1].Should().Be(100m);
            result.Rows[1].MonthlyAmounts[1].Should().Be(500m);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_RepositoryThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams)
                .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetMonthlySubContractsSummaryAsync(query));
        }

        #endregion
    }
}

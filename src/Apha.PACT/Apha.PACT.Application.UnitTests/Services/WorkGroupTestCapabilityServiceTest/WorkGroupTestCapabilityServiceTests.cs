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

namespace Apha.PACT.Application.UnitTests.Services.WorkGroupTestCapabilityServiceTest
{
    public class WorkGroupTestCapabilityServiceTests
    {
        private readonly ITestCapabilityRepository _testCapabilityRepo;
        private readonly ITestRequirementRepository _testReqmtRepo;
        private readonly ITestorProductRepository _testorProductRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IMapper _mapper;
        private readonly WorkGroupTestCapabilityService _sut;

        public WorkGroupTestCapabilityServiceTests()
        {
            _testCapabilityRepo = Substitute.For<ITestCapabilityRepository>();
            _testReqmtRepo = Substitute.For<ITestRequirementRepository>();
            _testorProductRepo = Substitute.For<ITestorProductRepository>();
            _projectRepo = Substitute.For<IProjectRepository>();
            _mapper = Substitute.For<IMapper>();
            _sut = new WorkGroupTestCapabilityService(
                _testCapabilityRepo, _testReqmtRepo, _testorProductRepo, _projectRepo, _mapper);
        }

        #region GetPagedByWorkGroupAsync

        [Fact]
        public async Task GetPagedByWorkGroupAsync_ValidQuery_ReturnsMappedPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TestCapability>([], new PaginationData());
            var expected = new PaginatedResult<TestCapabilityDto>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedByWorkGroupAsync(mappedParams, "WG1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedByWorkGroupAsync(query, "WG1");

            result.Should().Be(expected);
            await _testCapabilityRepo.Received(1).GetPagedByWorkGroupAsync(mappedParams, "WG1");
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_NullWorkGroup_PassesNullToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TestCapability>([], new PaginationData());
            var expected = new PaginatedResult<TestCapabilityDto>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedByWorkGroupAsync(mappedParams, null).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedByWorkGroupAsync(query, null);

            result.Should().Be(expected);
            await _testCapabilityRepo.Received(1).GetPagedByWorkGroupAsync(mappedParams, null);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_RepositoryThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            var mappedParams = new PaginationParameters<string>();
            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedByWorkGroupAsync(mappedParams, null).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetPagedByWorkGroupAsync(query, null));
        }

        #endregion

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_ValidQuery_ReturnsMappedPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TestCapability>([], new PaginationData());
            var expected = new PaginatedResult<TestCapabilityDto>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedByTestCodeAsync(mappedParams, "TC1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedByTestCodeAsync(query, "TC1");

            result.Should().Be(expected);
            await _testCapabilityRepo.Received(1).GetPagedByTestCodeAsync(mappedParams, "TC1");
        }

        #endregion

        #region GetTestCapabilityByIdAsync

        [Fact]
        public async Task GetTestCapabilityByIdAsync_RecordFound_ReturnsMappedDto()
        {
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(dto);

            var result = await _sut.GetTestCapabilityByIdAsync("TC1", "WG1");

            result.Should().Be(dto);
            await _testCapabilityRepo.Received(1).GetByIdAsync("TC1", "WG1");
        }

        [Fact]
        public async Task GetTestCapabilityByIdAsync_RecordNotFound_ReturnsNull()
        {
            _testCapabilityRepo.GetByIdAsync("MISSING", "WG1").Returns((TestCapability?)null);

            var result = await _sut.GetTestCapabilityByIdAsync("MISSING", "WG1");

            result.Should().BeNull();
        }

        #endregion

        #region AddTestCapabilityAsync

        [Fact]
        public async Task AddTestCapabilityAsync_NoDuplicate_CreatesAndReturnsMappedDto()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var created = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns((TestCapability?)null);
            _mapper.Map<TestCapability>(dto).Returns(entity);
            _testCapabilityRepo.AddAsync(entity).Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(created);

            var result = await _sut.AddTestCapabilityAsync(dto);

            result.Should().Be(created);
            await _testCapabilityRepo.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task AddTestCapabilityAsync_DuplicateExists_ThrowsInvalidOperationException()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var existing = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(existing);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddTestCapabilityAsync(dto));
            await _testCapabilityRepo.DidNotReceive().AddAsync(Arg.Any<TestCapability>());
        }

        #endregion

        #region UpdateTestCapabilityAsync

        [Fact]
        public async Task UpdateTestCapabilityAsync_ValidUpdate_ReturnsUpdatedDto()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var existing = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };
            var updated = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(existing);
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(false);
            _mapper.Map<TestCapability>(dto).Returns(entity);
            _testCapabilityRepo.UpdateAsync(entity).Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(updated);

            var result = await _sut.UpdateTestCapabilityAsync(dto);

            result.Should().Be(updated);
            await _testCapabilityRepo.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateTestCapabilityAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            var dto = new TestCapabilityDto { TestCode = "MISSING", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            _testCapabilityRepo.GetByIdAsync("MISSING", "WG1").Returns((TestCapability?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateTestCapabilityAsync(dto));
        }

        [Fact]
        public async Task UpdateTestCapabilityAsync_HasDependentReqmts_ThrowsInvalidOperationException()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var existing = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(existing);
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTestCapabilityAsync(dto));
            await _testCapabilityRepo.DidNotReceive().UpdateAsync(Arg.Any<TestCapability>());
        }

        #endregion

        #region DeleteTestCapabilityAsync

        [Fact]
        public async Task DeleteTestCapabilityAsync_NoReqmtsDependency_DeletesAndReturnsTrue()
        {
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(false);
            _testCapabilityRepo.DeleteAsync("TC1", "WG1").Returns(true);

            var result = await _sut.DeleteTestCapabilityAsync("TC1", "WG1");

            result.Should().BeTrue();
            await _testCapabilityRepo.Received(1).DeleteAsync("TC1", "WG1");
        }

        [Fact]
        public async Task DeleteTestCapabilityAsync_HasReqmtsDependency_ThrowsInvalidOperationException()
        {
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteTestCapabilityAsync("TC1", "WG1"));
            await _testCapabilityRepo.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        #endregion

        #region GetPagedTestReqmtAsync

        [Fact]
        public async Task GetPagedTestReqmtAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TestRequirementDetail>([], new PaginationData());
            var dtos = new List<TestRequirementtDto>();
            var paginationDto = new PaginationDto();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testReqmtRepo.GetPagedWithDetailsAsync(mappedParams, "BLOOD").Returns(pagedData);
            _mapper.Map<List<TestRequirementtDto>>(pagedData.Data).Returns(dtos);
            _mapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            var result = await _sut.GetPagedTestReqmtAsync(query, "BLOOD");

            result.Should().NotBeNull();
            await _testReqmtRepo.Received(1).GetPagedWithDetailsAsync(mappedParams, "BLOOD");
        }

        #endregion

        #region GetAllTestReqmtForExportAsync

        [Fact]
        public async Task GetAllTestReqmtForExportAsync_WithFilter_ReturnsAllMappedItems()
        {
            var details = new List<TestRequirementDetail>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1" },
                new() { TestCode = "BLOOD", Buyer = "PRJ2" }
            };
            var dtos = new List<TestRequirementtDto>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1" },
                new() { TestCode = "BLOOD", Buyer = "PRJ2" }
            };

            _testReqmtRepo.GetAllForExportAsync("BLOOD", "{}").Returns(details);
            _mapper.Map<IEnumerable<TestRequirementtDto>>(details).Returns(dtos);

            var result = await _sut.GetAllTestReqmtForExportAsync("BLOOD", "{}");

            result.Should().HaveCount(2);
            await _testReqmtRepo.Received(1).GetAllForExportAsync("BLOOD", "{}");
        }

        [Fact]
        public async Task GetAllTestReqmtForExportAsync_NullFilter_PassesNullToRepository()
        {
            var details = new List<TestRequirementDetail>();
            var dtos = new List<TestRequirementtDto>();

            _testReqmtRepo.GetAllForExportAsync("BLOOD", null).Returns(details);
            _mapper.Map<IEnumerable<TestRequirementtDto>>(details).Returns(dtos);

            var result = await _sut.GetAllTestReqmtForExportAsync("BLOOD", null);

            result.Should().BeEmpty();
            await _testReqmtRepo.Received(1).GetAllForExportAsync("BLOOD", null);
        }

        #endregion

        #region GetTestReqmtByIdAsync

        [Fact]
        public async Task GetTestReqmtByIdAsync_RecordFound_ReturnsMappedDto()
        {
            var detail = new TestRequirementDetail { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1" };

            _testReqmtRepo.GetDetailByIdAsync("BLOOD", "PRJ1").Returns(detail);
            _mapper.Map<TestRequirementtDto>(detail).Returns(dto);

            var result = await _sut.GetTestReqmtByIdAsync("BLOOD", "PRJ1");

            result.Should().Be(dto);
        }

        [Fact]
        public async Task GetTestReqmtByIdAsync_RecordNotFound_ReturnsNull()
        {
            _testReqmtRepo.GetDetailByIdAsync("MISSING", "PRJ1").Returns((TestRequirementDetail?)null);

            var result = await _sut.GetTestReqmtByIdAsync("MISSING", "PRJ1");

            result.Should().BeNull();
        }

        #endregion

        #region GetTestReqmtPricingAsync

        [Fact]
        public async Task GetTestReqmtPricingAsync_RecordFound_ReturnsMappedDto()
        {
            var detail = new TestRequirementDetail { TestCode = "BLOOD", RecUnitPrice = 10.5m };
            var dto = new TestRequirementtDto { TestCode = "BLOOD", RecUnitPrice = 10.5m };

            _testReqmtRepo.GetPricingAsync("BLOOD", null).Returns(detail);
            _mapper.Map<TestRequirementtDto>(detail).Returns(dto);

            var result = await _sut.GetTestReqmtPricingAsync("BLOOD", null);

            result.Should().Be(dto);
        }

        [Fact]
        public async Task GetTestReqmtPricingAsync_RecordNotFound_ReturnsNull()
        {
            _testReqmtRepo.GetPricingAsync("MISSING", null).Returns((TestRequirementDetail?)null);

            var result = await _sut.GetTestReqmtPricingAsync("MISSING", null);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTestReqmtPricingAsync_WithProjectCode_PassesProjectCodeToRepository()
        {
            var detail = new TestRequirementDetail { TestCode = "BLOOD", RecUnitPrice = 5.0m, IsDefraProject = 1 };
            var dto = new TestRequirementtDto { TestCode = "BLOOD", RecUnitPrice = 5.0m };

            _testReqmtRepo.GetPricingAsync("BLOOD", "PRJ1").Returns(detail);
            _mapper.Map<TestRequirementtDto>(detail).Returns(dto);

            var result = await _sut.GetTestReqmtPricingAsync("BLOOD", "PRJ1");

            result.Should().Be(dto);
            await _testReqmtRepo.Received(1).GetPricingAsync("BLOOD", "PRJ1");
        }

        #endregion

        #region AddTestReqmtAsync

        [Fact]
        public async Task AddTestReqmtAsync_BothFieldsNull_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto
            {
                TestCode = "BLOOD", Buyer = "PRJ1",
                ProjectBuyerCode = null, TestBuyerCode = null
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddTestReqmtAsync(dto));
            await _testReqmtRepo.DidNotReceive().AddAsync(Arg.Any<TestRequirement>());
        }

        [Fact]
        public async Task AddTestReqmtAsync_ProjectCodeProvidedButNotFound_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PRJ_X" };
            _projectRepo.ExistsAsync("PRJ_X").Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddTestReqmtAsync(dto));
        }

        [Fact]
        public async Task AddTestReqmtAsync_TestBuyerCodeCapabilityNotFound_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1", TestBuyerCode = "BLOOD-WG1" };
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("BLOOD-WG1").Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddTestReqmtAsync(dto));
        }

        [Fact]
        public async Task AddTestReqmtAsync_ValidWithProjectCode_CreatesAndReturnsDto()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PRJ_X" };
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1" };
            var created = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1" };

            _projectRepo.ExistsAsync("PRJ_X").Returns(true);
            _mapper.Map<TestRequirement>(dto).Returns(entity);
            _testReqmtRepo.AddAsync(entity).Returns(entity);
            _mapper.Map<TestRequirementtDto>(entity).Returns(created);

            var result = await _sut.AddTestReqmtAsync(dto);

            result.Should().Be(created);
            await _testReqmtRepo.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task AddTestReqmtAsync_ValidWithTestBuyerCode_CreatesAndReturnsDto()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1", TestBuyerCode = "BLOOD-WG1" };
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1" };
            var created = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1" };

            _testReqmtRepo.ExistsByTestBuyerCodeAsync("BLOOD-WG1").Returns(true);
            _mapper.Map<TestRequirement>(dto).Returns(entity);
            _testReqmtRepo.AddAsync(entity).Returns(entity);
            _mapper.Map<TestRequirementtDto>(entity).Returns(created);

            var result = await _sut.AddTestReqmtAsync(dto);

            result.Should().Be(created);
        }

        #endregion

        #region UpdateTestReqmtAsync

        [Fact]
        public async Task UpdateTestReqmtAsync_BothFieldsNull_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto
            {
                TestCode = "BLOOD", Buyer = "PRJ1",
                ProjectBuyerCode = null, TestBuyerCode = null
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTestReqmtAsync(dto));
        }

        [Fact]
        public async Task UpdateTestReqmtAsync_TestBuyerCodeCapabilityNotFound_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1", TestBuyerCode = "BLOOD-WG1" };
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("BLOOD-WG1").Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTestReqmtAsync(dto));
        }

        [Fact]
        public async Task UpdateTestReqmtAsync_MonthlyOutputExists_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1", TestBuyerCode = "BLOOD-WG1" };
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("BLOOD-WG1").Returns(true);
            _testReqmtRepo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTestReqmtAsync(dto));
        }

        [Fact]
        public async Task UpdateTestReqmtAsync_ProjectCodeNotFound_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PRJ_X" };
            _testReqmtRepo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1").Returns(false);
            _projectRepo.ExistsAsync("PRJ_X").Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTestReqmtAsync(dto));
        }

        [Fact]
        public async Task UpdateTestReqmtAsync_ValidWithProjectCode_ReturnsUpdatedDto()
        {
            var dto = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PRJ_X" };
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1" };
            var updated = new TestRequirementtDto { TestCode = "BLOOD", Buyer = "PRJ1" };

            _testReqmtRepo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1").Returns(false);
            _projectRepo.ExistsAsync("PRJ_X").Returns(true);
            _mapper.Map<TestRequirement>(dto).Returns(entity);
            _testReqmtRepo.UpdateAsync(entity).Returns(entity);
            _mapper.Map<TestRequirementtDto>(entity).Returns(updated);

            var result = await _sut.UpdateTestReqmtAsync(dto);

            result.Should().Be(updated);
            await _testReqmtRepo.Received(1).UpdateAsync(entity);
        }

        #endregion

        #region DeleteTestReqmtAsync

        [Fact]
        public async Task DeleteTestReqmtAsync_MonthlyOutputExists_ThrowsInvalidOperationException()
        {
            _testReqmtRepo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteTestReqmtAsync("BLOOD", "PRJ1"));
            await _testReqmtRepo.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteTestReqmtAsync_NoMonthlyOutput_DeletesAndReturnsTrue()
        {
            _testReqmtRepo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1").Returns(false);
            _testReqmtRepo.DeleteAsync("BLOOD", "PRJ1").Returns(true);

            var result = await _sut.DeleteTestReqmtAsync("BLOOD", "PRJ1");

            result.Should().BeTrue();
            await _testReqmtRepo.Received(1).DeleteAsync("BLOOD", "PRJ1");
        }

        #endregion

        #region GetAllTestorProductsAsync

        [Fact]
        public async Task GetAllTestorProductsAsync_WithData_ReturnsMappedDtos()
        {
            var entities = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test" },
                new() { ItemCode = "URINE", ItemDescription = "Urine Test" }
            };
            var dtos = new List<TestorProductDto>
            {
                new() { ItemCode = "BLOOD" },
                new() { ItemCode = "URINE" }
            };

            _testorProductRepo.GetAllAsync().Returns(entities);
            _mapper.Map<IEnumerable<TestorProductDto>>(entities).Returns(dtos);

            var result = await _sut.GetAllTestorProductsAsync();

            result.Should().BeEquivalentTo(dtos);
            await _testorProductRepo.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllTestorProductsAsync_EmptyRepository_ReturnsEmptyCollection()
        {
            var entities = new List<TestorProduct>();
            var dtos = new List<TestorProductDto>();

            _testorProductRepo.GetAllAsync().Returns(entities);
            _mapper.Map<IEnumerable<TestorProductDto>>(entities).Returns(dtos);

            var result = await _sut.GetAllTestorProductsAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllTestorProductsAsync_RepositoryThrows_PropagatesException()
        {
            _testorProductRepo.GetAllAsync().ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetAllTestorProductsAsync());
        }

        #endregion
    }
}

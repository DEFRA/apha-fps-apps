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

namespace Apha.PACT.Application.UnitTests.Services.TestCapabilityServiceTest
{
    public class TestCapabilityServiceTests
    {
        private readonly ITestCapabilityRepository _testCapabilityRepo;
        private readonly ITestRequirementRepository _testReqmtRepo;
        private readonly ITestorProductRepository _testorProductRepo;
        private readonly IMapper _mapper;
        private readonly TestCapabilityService _sut;

        public TestCapabilityServiceTests()
        {
            _testCapabilityRepo = Substitute.For<ITestCapabilityRepository>();
            _testReqmtRepo = Substitute.For<ITestRequirementRepository>();
            _testorProductRepo = Substitute.For<ITestorProductRepository>();
            _mapper = Substitute.For<IMapper>();
            _sut = new TestCapabilityService(
                _testCapabilityRepo, _testReqmtRepo, _testorProductRepo, _mapper);
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

        
    }
}

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.TestSupplierServiceTest
{
    public class TestSupplierServiceTests
    {
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER001";

        private readonly ITestSupplierRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly TestSupplierService _sut;

        public TestSupplierServiceTests()
        {
            _mockRepository = Substitute.For<ITestSupplierRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new TestSupplierService(_mockRepository, _mockMapper);
        }

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<TestSupplierView> { Data = new List<TestSupplierView>(), PaginationData = new PaginationData() };
            var expectedResult = new PaginatedResult<TestSupplierViewDto>();
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedByTestCodeAsync(mappedParams, DefaultTestCode, false).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TestSupplierViewDto>>(pagedData).Returns(expectedResult);

            var result = await _sut.GetPagedByTestCodeAsync(query, DefaultTestCode, false);

            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);
            await _mockRepository.Received(1).GetPagedByTestCodeAsync(mappedParams, DefaultTestCode, false);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WhenRepositoryThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _mockRepository.GetPagedByTestCodeAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Throws(new InvalidOperationException("DB error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetPagedByTestCodeAsync(query, DefaultTestCode, false));
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithExistingRecord_ReturnsMappedDto()
        {
            var entity = new TestRequirement { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var expectedDto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };

            _mockRepository.GetByIdAsync(DefaultTestCode, DefaultBuyer).Returns(entity);
            _mockMapper.Map<TestRequirementDto>(entity).Returns(expectedDto);

            var result = await _sut.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.NotNull(result);
            Assert.Equal(DefaultTestCode, result!.TestCode);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingRecord_ReturnsNull()
        {
            _mockRepository.GetByIdAsync(DefaultTestCode, DefaultBuyer).Returns((TestRequirement?)null);
            _mockMapper.Map<TestRequirementDto>((TestRequirement?)null).Returns((TestRequirementDto?)null);

            var result = await _sut.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.Null(result);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_WithValidDto_ReturnsCreatedDto()
        {
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer, ProjectBuyerCode = "PROJ001" };
            var entity = new TestRequirement { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var savedEntity = new TestRequirement { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var expectedDto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };

            _mockRepository.ProjectExistsAsync("PROJ001").Returns(true);
            _mockMapper.Map<TestRequirement>(dto).Returns(entity);
            _mockRepository.AddAsync(entity).Returns(savedEntity);
            _mockMapper.Map<TestRequirementDto>(savedEntity).Returns(expectedDto);

            var result = await _sut.AddAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(DefaultTestCode, result.TestCode);
            await _mockRepository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task AddAsync_WithNullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddAsync(null!));
        }

        [Fact]
        public async Task AddAsync_WithBothBuyerCodesNull_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer, ProjectBuyerCode = null, TestBuyerCode = null };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddAsync(dto));
        }

        [Fact]
        public async Task AddAsync_WithInvalidProjectBuyerCode_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer, ProjectBuyerCode = "MISSING" };

            _mockRepository.ProjectExistsAsync("MISSING").Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddAsync(dto));
        }

        [Fact]
        public async Task AddAsync_WithInvalidTestBuyerCode_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer, TestBuyerCode = "WG_MISSING" };

            _mockRepository.TestBuyerCodeExistsAsync(DefaultTestCode, "WG_MISSING").Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddAsync(dto));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithValidDto_ReturnsUpdatedDto()
        {
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer, ProjectBuyerCode = "PROJ001" };
            var existing = new TestRequirement { TestCode = DefaultTestCode, Buyer = DefaultBuyer, FpsYear = 2024 };
            var entity = new TestRequirement { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var updatedEntity = new TestRequirement { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var expectedDto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };

            _mockRepository.GetByIdAsync(DefaultTestCode, DefaultBuyer).Returns(existing);
            _mockRepository.ProjectExistsAsync("PROJ001").Returns(true);
            _mockMapper.Map<TestRequirement>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).Returns(updatedEntity);
            _mockMapper.Map<TestRequirementDto>(updatedEntity).Returns(expectedDto);

            var result = await _sut.UpdateAsync(dto);

            Assert.NotNull(result);
            await _mockRepository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_WithNullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingRecord_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer, ProjectBuyerCode = "PROJ001" };

            _mockRepository.ProjectExistsAsync("PROJ001").Returns(true);
            _mockRepository.GetByIdAsync(DefaultTestCode, DefaultBuyer).Returns((TestRequirement?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(dto));
        }

        [Fact]
        public async Task UpdateAsync_WhenTestCodeChangedAndMonthlyOutputExists_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementDto { TestCode = "NEW_TEST", Buyer = DefaultBuyer, ProjectBuyerCode = "PROJ001" };
            var existing = new TestRequirement { TestCode = DefaultTestCode, Buyer = DefaultBuyer, FpsYear = 2024 };

            _mockRepository.ProjectExistsAsync("PROJ001").Returns(true);
            _mockRepository.GetByIdAsync("NEW_TEST", DefaultBuyer).Returns((TestRequirement?)null);
            // Service calls GetByIdAsync with dto.TestCode and dto.Buyer
            _mockRepository.GetByIdAsync(dto.TestCode, dto.Buyer).Returns((TestRequirement?)null);

            // Simulate the scenario where existing record has different TestCode
            // but we need to test the MonthlyOutput check path
            var existingOld = new TestRequirement { TestCode = DefaultTestCode, Buyer = DefaultBuyer, FpsYear = 2024 };
            _mockRepository.GetByIdAsync("NEW_TEST", DefaultBuyer).Returns(existingOld);
            _mockRepository.MonthlyOutputExistsAsync(DefaultTestCode, DefaultBuyer).Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(dto));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenNoMonthlyOutput_CallsRepositoryDelete()
        {
            _mockRepository.MonthlyOutputExistsAsync(DefaultTestCode, DefaultBuyer).Returns(false);
            _mockRepository.DeleteAsync(DefaultTestCode, DefaultBuyer).Returns(true);

            var result = await _sut.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.True(result);
            await _mockRepository.Received(1).DeleteAsync(DefaultTestCode, DefaultBuyer);
        }

        [Fact]
        public async Task DeleteAsync_WhenMonthlyOutputExists_ThrowsInvalidOperationException()
        {
            _mockRepository.MonthlyOutputExistsAsync(DefaultTestCode, DefaultBuyer).Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync(DefaultTestCode, DefaultBuyer));
            await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordNotFound_ReturnsFalse()
        {
            _mockRepository.MonthlyOutputExistsAsync(DefaultTestCode, DefaultBuyer).Returns(false);
            _mockRepository.DeleteAsync(DefaultTestCode, DefaultBuyer).Returns(false);

            var result = await _sut.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result);
        }

        #endregion

        #region GetTestOrProductsAsync

        [Fact]
        public async Task GetTestOrProductsAsync_WithItems_ReturnsMappedList()
        {
            var entities = new List<TestOrProduct> { new() { ItemCode = "T001" } };
            var expectedDtos = new List<TestOrProductDto> { new() { ItemCode = "T001" } };

            _mockRepository.GetTestOrProductsAsync().Returns(entities);
            _mockMapper.Map<List<TestOrProductDto>>(entities).Returns(expectedDtos);

            var result = await _sut.GetTestOrProductsAsync();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("T001", result[0].ItemCode);
        }

        [Fact]
        public async Task GetTestOrProductsAsync_WithNoItems_ReturnsEmptyList()
        {
            var entities = new List<TestOrProduct>();
            var expectedDtos = new List<TestOrProductDto>();

            _mockRepository.GetTestOrProductsAsync().Returns(entities);
            _mockMapper.Map<List<TestOrProductDto>>(entities).Returns(expectedDtos);

            var result = await _sut.GetTestOrProductsAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion
    }
}

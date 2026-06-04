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

namespace Apha.FPS.Application.UnitTests.Services.TestSupplierService
{
    public class TestSupplierServiceTests
    {
        private const string DefaultTestCode = "TST001";
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 10;

        private readonly ITestSupplierRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly global::Apha.FPS.Application.Services.TestSupplierService _sut;

        public TestSupplierServiceTests()
        {
            _mockRepository = Substitute.For<ITestSupplierRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new global::Apha.FPS.Application.Services.TestSupplierService(_mockRepository, _mockMapper);
        }

        private static QueryParameters<string> DefaultQuery() =>
            new() { Page = DefaultPageNumber, PageSize = DefaultPageSize };

        private static PaginationParameters<string> DefaultParams() =>
            new(page: DefaultPageNumber, pageSize: DefaultPageSize);

        private static PagedData<TestSupplierView> BuildPagedData(int count = 2) =>
            new(
                Enumerable.Range(1, count).Select(i => new TestSupplierView
                {
                    TestCode = DefaultTestCode,
                    Buyer = $"B{i:D3}"
                }),
                new PaginationData { PageNumber = 1, PageSize = DefaultPageSize, TotalRecords = count });

        private static PaginatedResult<TestSupplierViewDto> BuildPaginatedResult(int count = 2) =>
            new(
                Enumerable.Range(1, count).Select(i => new TestSupplierViewDto
                {
                    TestCode = DefaultTestCode,
                    Buyer = $"B{i:D3}"
                }),
                new PaginationDto { PageNumber = 1, PageSize = DefaultPageSize, TotalRecords = count });

        #region GetPagedAsync Tests

        [Fact]
        public async Task GetPagedAsync_WithValidQuery_ReturnsMappedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var parameters = DefaultParams();
            var pagedData = BuildPagedData();
            var expected = BuildPaginatedResult();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedByTestCodeAsync(parameters, DefaultTestCode, false).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TestSupplierViewDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetPagedAsync(query, DefaultTestCode, showRejected: false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            await _mockRepository.Received(1).GetPagedByTestCodeAsync(parameters, DefaultTestCode, false);
        }

        [Fact]
        public async Task GetPagedAsync_ShowRejectedTrue_PassesTrueToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var parameters = DefaultParams();
            var pagedData = BuildPagedData(1);
            var expected = BuildPaginatedResult(1);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedByTestCodeAsync(parameters, DefaultTestCode, true).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TestSupplierViewDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetPagedAsync(query, DefaultTestCode, showRejected: true);

            // Assert
            Assert.NotNull(result);
            await _mockRepository.Received(1).GetPagedByTestCodeAsync(parameters, DefaultTestCode, true);
        }

        [Fact]
        public async Task GetPagedAsync_NullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sut.GetPagedAsync(null!, DefaultTestCode, false));
        }

        [Fact]
        public async Task GetPagedAsync_EmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var parameters = DefaultParams();
            var pagedData = BuildPagedData(0);
            var expected = BuildPaginatedResult(0);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedByTestCodeAsync(parameters, DefaultTestCode, false).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TestSupplierViewDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetPagedAsync(query, DefaultTestCode, showRejected: false);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();
            var parameters = DefaultParams();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedByTestCodeAsync(parameters, DefaultTestCode, false)
                .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetPagedAsync(query, DefaultTestCode, false));
        }

        [Fact]
        public async Task GetPagedAsync_CallsMapper_ForQueryParameters()
        {
            // Arrange
            var query = DefaultQuery();
            var parameters = DefaultParams();
            var pagedData = BuildPagedData();
            var expected = BuildPaginatedResult();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedByTestCodeAsync(parameters, DefaultTestCode, false).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TestSupplierViewDto>>(pagedData).Returns(expected);

            // Act
            await _sut.GetPagedAsync(query, DefaultTestCode, false);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            _mockMapper.Received(1).Map<PaginatedResult<TestSupplierViewDto>>(pagedData);
        }

        #endregion
    }
}

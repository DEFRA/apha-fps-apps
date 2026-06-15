// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — ProjectProfitabilityVlaServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - New file: xUnit tests for ProjectService.GetProjectProfitabilityVlaAsync()
 *     (backend Application layer, Apha.FPS.Application).
 *   - Covers: success path with paged data, empty result, null-guard (ArgumentNullException),
 *     repository delegation with correct PaginationParameters, and mapper invocation.
 *   - Uses NSubstitute for IProjectRepository and IMapper mocks.
 *   - Uses FluentAssertions (already referenced by project).
 *
 * PRESERVED:
 *   - Test naming convention [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - Constructor-based mock initialisation pattern matching ProjectProfitabilityServiceTests.cs.
 *   - Arrange-Act-Assert layout.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm PagedData<ProjectProfitabilityVlaView> constructor signature
 *     matches what RepositoryTestHelper / mock returns (Data + PaginationData).
 *   - Build/test status: NOT RUN — requires dotnet restore && dotnet build.
 */

using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.ProjectServiceTest
{
    public class ProjectProfitabilityVlaServiceTests
    {
        private readonly IProjectRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectService _sut;

        public ProjectProfitabilityVlaServiceTests()
        {
            _mockRepository = Substitute.For<IProjectRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectService(_mockRepository, _mockMapper);
        }

        #region GetProjectProfitabilityVlaAsync

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<ProjectProfitabilityVlaReq>
            {
                Page = 1,
                PageSize = 15,
                Filter = new ProjectProfitabilityVlaReq { ProjectStatus = "Approved", ProgramNo = "P001" }
            };
            var paginationParams = new PaginationParameters<ProjectProfitabilityVlaReq>
            {
                Page = 1,
                PageSize = 15,
                Filter = query.Filter
            };

            var views = new List<ProjectProfitabilityVlaView>
            {
                new() { JobCode = "PP001", StaffCosts = 1000m, Budget = 5000m, Profit = 4000m, TargetProfit = 3500m, OffTarget = 500m },
                new() { JobCode = "PP002", StaffCosts = 2000m, Budget = 6000m, Profit = 4000m, TargetProfit = 3000m, OffTarget = 1000m }
            };
            var pagedData = new PagedData<ProjectProfitabilityVlaView>(
                views, new PaginationData { PageNumber = 1, PageSize = 15, TotalRecords = 2 });

            var expectedResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>
                {
                    new() { JobCode = "PP001", Profit = 4000m },
                    new() { JobCode = "PP002", Profit = 4000m }
                },
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 2 });

            _mockMapper.Map<PaginationParameters<ProjectProfitabilityVlaReq>>(query)
                .Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityVlaAsync(paginationParams)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityVlaDto>>(pagedData)
                .Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectProfitabilityVlaAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedResult);
            await _mockRepository.Received(1).GetProjectProfitabilityVlaAsync(paginationParams);
            _mockMapper.Received(1).Map<PaginationParameters<ProjectProfitabilityVlaReq>>(query);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectProfitabilityVlaDto>>(pagedData);

        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithEmptyRepositoryResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<ProjectProfitabilityVlaReq> { Page = 1, PageSize = 15 };
            var paginationParams = new PaginationParameters<ProjectProfitabilityVlaReq> { Page = 1, PageSize = 15 };
            var emptyPagedData = new PagedData<ProjectProfitabilityVlaView>(
                new List<ProjectProfitabilityVlaView>(),
                new PaginationData { PageNumber = 1, PageSize = 15, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<ProjectProfitabilityVlaReq>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityVlaAsync(paginationParams).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityVlaDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetProjectProfitabilityVlaAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithNullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert — ArgumentNullException.ThrowIfNull(query) guard in service
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetProjectProfitabilityVlaAsync(null!));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithNoFilters_DelegatesToRepositoryWithMappedParams()
        {
            // Arrange — no filter fields set; verifies delegation path with null filter
            var query = new QueryParameters<ProjectProfitabilityVlaReq>
            {
                Page = 2,
                PageSize = 25,
                Filter = new ProjectProfitabilityVlaReq()
            };
            var paginationParams = new PaginationParameters<ProjectProfitabilityVlaReq>
            {
                Page = 2,
                PageSize = 25,
                Filter = query.Filter
            };
            var pagedData = new PagedData<ProjectProfitabilityVlaView>(
                new List<ProjectProfitabilityVlaView>(),
                new PaginationData { PageNumber = 2, PageSize = 25, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { PageNumber = 2, PageSize = 25, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<ProjectProfitabilityVlaReq>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityVlaAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectProfitabilityVlaDto>>(pagedData).Returns(emptyResult);

            // Act
            await _sut.GetProjectProfitabilityVlaAsync(query);

            // Assert
            await _mockRepository.Received(1).GetProjectProfitabilityVlaAsync(paginationParams);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<ProjectProfitabilityVlaReq> { Page = 1, PageSize = 15 };
            var paginationParams = new PaginationParameters<ProjectProfitabilityVlaReq> { Page = 1, PageSize = 15 };

            _mockMapper.Map<PaginationParameters<ProjectProfitabilityVlaReq>>(query).Returns(paginationParams);
            _mockRepository.GetProjectProfitabilityVlaAsync(paginationParams)
                .ThrowsAsync(new InvalidOperationException("DB connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetProjectProfitabilityVlaAsync(query));
        }

        #endregion
    }
}

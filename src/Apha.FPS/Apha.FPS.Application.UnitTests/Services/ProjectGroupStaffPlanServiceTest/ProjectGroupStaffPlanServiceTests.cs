using Apha.FPS.Application.Dtos;
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

namespace Apha.FPS.Application.UnitTests.Services.ProjectGroupStaffPlanServiceTest
{
    public class ProjectGroupStaffPlanServiceTests
    {
        private readonly IProjectGroupStaffPlanRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectGroupStaffPlanService _sut;

        public ProjectGroupStaffPlanServiceTests()
        {
            _mockRepository = Substitute.For<IProjectGroupStaffPlanRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectGroupStaffPlanService(_mockRepository, _mockMapper);
        }

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_HappyPath_ReturnsMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectGroupStaffPlanView>(
                new List<ProjectGroupStaffPlanView>
                {
                    new() { ProjectGroup = "GROUP_1", Manager = "Manager 1", ResourceCentre = "RC1" }
                },
                new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            var expected = new PaginatedResult<ProjectGroupStaffPlanViewDto>(
                new List<ProjectGroupStaffPlanViewDto>
                {
                    new() { ProjectGroup = "GROUP_1", Manager = "Manager 1", ResourceCentre = "RC1" }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectGroupStaffPlanViewDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetPagedAsync(query);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public async Task GetPagedAsync_EmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectGroupStaffPlanView>();
            var expected = new PaginatedResult<ProjectGroupStaffPlanViewDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectGroupStaffPlanViewDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetPagedAsync(query);

            // Assert
            result.Should().Be(expected);
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPagedAsync_MapsQueryToParametersFirst()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "Manager" };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectGroupStaffPlanView>();
            var expected = new PaginatedResult<ProjectGroupStaffPlanViewDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectGroupStaffPlanViewDto>>(pagedData).Returns(expected);

            // Act
            await _sut.GetPagedAsync(query);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
        }

        [Fact]
        public async Task GetPagedAsync_CallsRepositoryWithMappedParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectGroupStaffPlanView>();
            var expected = new PaginatedResult<ProjectGroupStaffPlanViewDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectGroupStaffPlanViewDto>>(pagedData).Returns(expected);

            // Act
            await _sut.GetPagedAsync(query);

            // Assert
            await _mockRepository.Received(1).GetPagedAsync(parameters);
        }

        [Fact]
        public async Task GetPagedAsync_MapsRepositoryResultToDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectGroupStaffPlanView>();
            var expected = new PaginatedResult<ProjectGroupStaffPlanViewDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectGroupStaffPlanViewDto>>(pagedData).Returns(expected);

            // Act
            await _sut.GetPagedAsync(query);

            // Assert
            _mockMapper.Received(1).Map<PaginatedResult<ProjectGroupStaffPlanViewDto>>(pagedData);
        }

        [Fact]
        public async Task GetPagedAsync_WithMultipleRows_ReturnsMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectGroupStaffPlanView>(
                Enumerable.Range(1, 5)
                    .Select(i => new ProjectGroupStaffPlanView
                    {
                        ProjectGroup   = $"GROUP_{i}",
                        Manager        = $"Manager {i}",
                        ResourceCentre = $"RC{i}",
                        WorkGroup      = $"WG{i}",
                        GradeCode      = $"G{i}",
                        Name           = $"Staff {i}",
                        JobCode        = $"JC{i}",
                        ProjectStatus  = "Active",
                        Hrs            = i * 10.0,
                        ChargeRate     = i * 100m,
                        Fee            = i * 50m
                    })
                    .ToList(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 5 });

            var expected = new PaginatedResult<ProjectGroupStaffPlanViewDto>(
                Enumerable.Range(1, 5)
                    .Select(i => new ProjectGroupStaffPlanViewDto { ProjectGroup = $"GROUP_{i}" })
                    .ToList(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 5 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectGroupStaffPlanViewDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetPagedAsync(query);

            // Assert
            result.Should().Be(expected);
            result.Data.Should().HaveCount(5);
        }

        [Fact]
        public async Task GetPagedAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedAsync(parameters).Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetPagedAsync(query));
        }

        [Fact]
        public async Task GetPagedAsync_MapperThrowsOnQueryMapping_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<string>>(query)
                .Throws(new Exception("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetPagedAsync(query));
        }

        [Fact]
        public async Task GetPagedAsync_MapperThrowsOnResultMapping_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectGroupStaffPlanView>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetPagedAsync(parameters).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectGroupStaffPlanViewDto>>(pagedData)
                .Throws(new Exception("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetPagedAsync(query));
        }

        #endregion
    }
}

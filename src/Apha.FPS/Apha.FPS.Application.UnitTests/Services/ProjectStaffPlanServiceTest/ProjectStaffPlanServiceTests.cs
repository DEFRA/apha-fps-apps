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

namespace Apha.FPS.Application.UnitTests.Services.ProjectStaffPlanServiceTest
{
    public class ProjectStaffPlanServiceTests
    {
        private readonly IProjectStaffPlanRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectStaffPlanService _sut;

        public ProjectStaffPlanServiceTests()
        {
            _mockRepository = Substitute.For<IProjectStaffPlanRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new ProjectStaffPlanService(_mockRepository, _mockMapper);
        }

        private static QueryParameters<string> DefaultQuery() => new() { Page = 1, PageSize = 10 };
        private static PaginationParameters<string> DefaultFilter() => new() { Page = 1, PageSize = 10 };

        private static PagedData<ProjectStaffPlanView> MakePagedData(IEnumerable<ProjectStaffPlanView> items)
        {
            var list = items.ToList();
            return new PagedData<ProjectStaffPlanView>(list,
                new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = list.Count });
        }

        private static PaginatedResult<ProjectStaffPlanViewDto> MakePaginatedResult(
            IEnumerable<ProjectStaffPlanViewDto> items)
        {
            var list = items.ToList();
            return new PaginatedResult<ProjectStaffPlanViewDto>(list,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = list.Count });
        }

        #region GetPagedAsync — Happy path

        [Fact]
        public async Task GetPagedAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var query  = DefaultQuery();
            var filter = DefaultFilter();
            var entities = new List<ProjectStaffPlanView>
            {
                new() { ParentProject = "P001", Name = "Alice", StaffId = "S001" },
                new() { ParentProject = "P002", Name = "Bob",   StaffId = "S002" }
            };
            var pagedData = MakePagedData(entities);
            var expectedDtos = new List<ProjectStaffPlanViewDto>
            {
                new() { ParentProject = "P001", Name = "Alice", StaffId = "S001" },
                new() { ParentProject = "P002", Name = "Bob",   StaffId = "S002" }
            };
            var expectedResult = MakePaginatedResult(expectedDtos);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffPlanViewDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetPagedAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            await _mockRepository.Received(1).GetPagedAsync(filter);
        }

        [Fact]
        public async Task GetPagedAsync_WithEmptyData_ReturnsMappedEmptyList()
        {
            // Arrange
            var query  = DefaultQuery();
            var filter = DefaultFilter();
            var pagedData    = MakePagedData(Enumerable.Empty<ProjectStaffPlanView>());
            var emptyResult  = MakePaginatedResult(Enumerable.Empty<ProjectStaffPlanViewDto>());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffPlanViewDto>>(pagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetPagedAsync(query);

            // Assert
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPagedAsync_MapsQueryParametersToPaginationParameters()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 2, PageSize = 5, Filter = "P001" };
            var filter = new PaginationParameters<string> { Page = 2, PageSize = 5, Filter = "P001" };
            var pagedData   = MakePagedData(Enumerable.Empty<ProjectStaffPlanView>());
            var emptyResult = MakePaginatedResult(Enumerable.Empty<ProjectStaffPlanViewDto>());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffPlanViewDto>>(pagedData).Returns(emptyResult);

            // Act
            await _sut.GetPagedAsync(query);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedAsync(filter);
        }

        #endregion

        #region GetPagedAsync — Error cases

        [Fact]
        public async Task GetPagedAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query  = DefaultQuery();
            var filter = DefaultFilter();
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).ThrowsAsync(new Exception("DB error"));

            // Act
            var act = async () => await _sut.GetPagedAsync(query);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
        }

        [Fact]
        public async Task GetPagedAsync_WhenMapperThrowsOnQuery_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();
            _mockMapper.Map<PaginationParameters<string>>(query).Throws(new Exception("Mapping error"));

            // Act
            var act = async () => await _sut.GetPagedAsync(query);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Mapping error");
        }

        [Fact]
        public async Task GetPagedAsync_WhenMapperThrowsOnResult_PropagatesException()
        {
            // Arrange
            var query    = DefaultQuery();
            var filter   = DefaultFilter();
            var pagedData = MakePagedData(Enumerable.Empty<ProjectStaffPlanView>());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffPlanViewDto>>(pagedData)
                .Throws(new Exception("Result mapping error"));

            // Act
            var act = async () => await _sut.GetPagedAsync(query);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Result mapping error");
        }

        #endregion

        #region GetPagedAsync — Verify interactions

        [Fact]
        public async Task GetPagedAsync_RepositoryCalledOnce()
        {
            // Arrange
            var query    = DefaultQuery();
            var filter   = DefaultFilter();
            var pagedData = MakePagedData(Enumerable.Empty<ProjectStaffPlanView>());
            var result   = MakePaginatedResult(Enumerable.Empty<ProjectStaffPlanViewDto>());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffPlanViewDto>>(pagedData).Returns(result);

            // Act
            await _sut.GetPagedAsync(query);

            // Assert
            await _mockRepository.Received(1).GetPagedAsync(filter);
        }

        [Fact]
        public async Task GetPagedAsync_MapperCalledTwice_ForQueryAndResult()
        {
            // Arrange
            var query    = DefaultQuery();
            var filter   = DefaultFilter();
            var pagedData = MakePagedData(Enumerable.Empty<ProjectStaffPlanView>());
            var result   = MakePaginatedResult(Enumerable.Empty<ProjectStaffPlanViewDto>());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffPlanViewDto>>(pagedData).Returns(result);

            // Act
            await _sut.GetPagedAsync(query);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectStaffPlanViewDto>>(pagedData);
        }

        #endregion
    }
}

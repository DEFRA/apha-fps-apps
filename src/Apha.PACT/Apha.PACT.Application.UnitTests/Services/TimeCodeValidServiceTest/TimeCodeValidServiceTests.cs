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

namespace Apha.PACT.Application.UnitTests.Services.TimeCodeValidServiceTest
{
    public class TimeCodeValidServiceTests
    {
        private readonly ITimeCodeValidRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly TimeCodeValidService _sut;

        public TimeCodeValidServiceTests()
        {
            _mockRepository = Substitute.For<ITimeCodeValidRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new TimeCodeValidService(_mockRepository, _mockMapper);
        }

        #region GetByJobCodeAsync

        [Fact]
        public async Task GetByJobCodeAsync_ValidInput_ReturnsMappedDtos()
        {
            var entities = new List<TimeCodeValid> { new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" } };
            var dtos = new List<TimeCodeValidDto> { new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" } };

            _mockRepository.GetByJobCodeAsync("JC1", "PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            var result = await _sut.GetByJobCodeAsync("JC1", "PRJ1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetByJobCodeAsync("JC1", "PRJ1");
        }

        [Fact]
        public async Task GetByJobCodeAsync_EmptyResult_ReturnsEmptyCollection()
        {
            var entities = new List<TimeCodeValid>();
            var dtos = new List<TimeCodeValidDto>();

            _mockRepository.GetByJobCodeAsync("JC1", "PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            var result = await _sut.GetByJobCodeAsync("JC1", "PRJ1");

            result.Should().BeEmpty();
        }

        #endregion

        #region GetPagedTimeCodesAsync

        [Fact]
        public async Task GetPagedTimeCodesAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TimeCodeValid>(new List<TimeCodeValid>(), new PaginationData());
            var pagedResult = new PaginatedResult<TimeCodeValidDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedTimeCodesAsync(mappedParams, "JC1", "PRJ1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TimeCodeValidDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedTimeCodesAsync(query, "JC1", "PRJ1");

            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedTimeCodesAsync(mappedParams, "JC1", "PRJ1");
        }

        [Fact]
        public async Task GetPagedTimeCodesAsync_NullFilters_PassesNullsToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TimeCodeValid>(new List<TimeCodeValid>(), new PaginationData());
            var pagedResult = new PaginatedResult<TimeCodeValidDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedTimeCodesAsync(mappedParams, null, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TimeCodeValidDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedTimeCodesAsync(query, null, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedTimeCodesAsync(mappedParams, null, null);
        }

        #endregion

        #region GetTimeCodeValidAsync

        [Fact]
        public async Task GetTimeCodeValidAsync_ValidKey_ReturnsMappedDto()
        {
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(entity);
            _mockMapper.Map<TimeCodeValidDto>(entity).Returns(dto);

            var result = await _sut.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1");

            result.Should().Be(dto);
            await _mockRepository.Received(1).GetTimeCodeValidAsync("WG1", "TC1", "PRJ1");
        }

        [Fact]
        public async Task GetTimeCodeValidAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1").Returns((TimeCodeValid?)null);

            var result = await _sut.GetTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1");

            result.Should().BeNull();
        }

        #endregion

        #region CreateTimeCodeValidAsync

        [Fact]
        public async Task CreateTimeCodeValidAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var created = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var expected = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.CreateTimeCodeValidAsync(entity).Returns(created);
            _mockMapper.Map<TimeCodeValidDto>(created).Returns(expected);

            var result = await _sut.CreateTimeCodeValidAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<TimeCodeValid>(dto);
            await _mockRepository.Received(1).CreateTimeCodeValidAsync(entity);
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.CreateTimeCodeValidAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.CreateTimeCodeValidAsync(dto));
        }

        #endregion

        #region UpdateTimeCodeValidAsync

        [Fact]
        public async Task UpdateTimeCodeValidAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Active = true };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Active = true };
            var updated = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Active = true };
            var expected = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Active = true };

            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.UpdateTimeCodeValidAsync(entity).Returns(updated);
            _mockMapper.Map<TimeCodeValidDto>(updated).Returns(expected);

            var result = await _sut.UpdateTimeCodeValidAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<TimeCodeValid>(dto);
            await _mockRepository.Received(1).UpdateTimeCodeValidAsync(entity);
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.UpdateTimeCodeValidAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateTimeCodeValidAsync(dto));
        }

        #endregion

        #region DeleteTimeCodeValidAsync

        [Fact]
        public async Task DeleteTimeCodeValidAsync_ValidKey_ReturnsTrue()
        {
            _mockRepository.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(true);

            var result = await _sut.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1");
        }

        [Fact]
        public async Task DeleteTimeCodeValidAsync_NotFound_ReturnsFalse()
        {
            _mockRepository.DeleteTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1").Returns(false);

            var result = await _sut.DeleteTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1");

            result.Should().BeFalse();
        }

        #endregion

        #region DeleteAllByJobCodeAsync

        [Fact]
        public async Task DeleteAllByJobCodeAsync_ValidJobCode_ReturnsTrue()
        {
            _mockRepository.DeleteAllByJobCodeAsync("JC1", "PRJ1").Returns(true);

            var result = await _sut.DeleteAllByJobCodeAsync("JC1", "PRJ1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAllByJobCodeAsync("JC1", "PRJ1");
        }

        [Fact]
        public async Task DeleteAllByJobCodeAsync_NotFound_ReturnsFalse()
        {
            _mockRepository.DeleteAllByJobCodeAsync("JC_MISSING", "PRJ1").Returns(false);

            var result = await _sut.DeleteAllByJobCodeAsync("JC_MISSING", "PRJ1");

            result.Should().BeFalse();
        }

        #endregion

        #region CopyWorkGroupAsync

        [Fact]
        public async Task CopyWorkGroupAsync_ValidInput_ReturnsMappedDtos()
        {
            var entities = new List<TimeCodeValid> { new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG2", ParentProject = "PRJ1" } };
            var dtos = new List<TimeCodeValidDto> { new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG2", ParentProject = "PRJ1" } };

            _mockRepository.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            var result = await _sut.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1");
        }

        [Fact]
        public async Task CopyWorkGroupAsync_EmptyResult_ReturnsEmptyCollection()
        {
            var entities = new List<TimeCodeValid>();
            var dtos = new List<TimeCodeValidDto>();

            _mockRepository.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            var result = await _sut.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1");

            result.Should().BeEmpty();
        }

        #endregion

        #region DeleteBulkAsync

        [Fact]
        public async Task DeleteBulkAsync_WithValidItems_DelegatesToRepositoryAndReturnsTrue()
        {
            // Arrange
            var items = new List<(string WorkGroup, string TimeCode)> { ("WG1", "TC1"), ("WG2", "TC2") };

            _mockRepository
                .DeleteBulkAsync(items, "PRJ1")
                .Returns(true);

            // Act
            var result = await _sut.DeleteBulkAsync(items, "PRJ1");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteBulkAsync(items, "PRJ1");
        }

        [Fact]
        public async Task DeleteBulkAsync_WithEmptyItems_ReturnsTrue()
        {
            // Arrange — repository always returns true even for an empty list
            var items = Enumerable.Empty<(string WorkGroup, string TimeCode)>();

            _mockRepository
                .DeleteBulkAsync(
                    Arg.Any<IEnumerable<(string WorkGroup, string TimeCode)>>(),
                    "PRJ1")
                .Returns(true);

            // Act
            var result = await _sut.DeleteBulkAsync(items, "PRJ1");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteBulkAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var items = new List<(string WorkGroup, string TimeCode)> { ("WG1", "TC1") };

            _mockRepository
                .DeleteBulkAsync(
                    Arg.Any<IEnumerable<(string WorkGroup, string TimeCode)>>(),
                    Arg.Any<string>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.DeleteBulkAsync(items, "PRJ1"));
        }

        #endregion

        #region CopySelectedWorkGroupsAsync

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithValidWorkGroups_ReturnsMappedDtos()
        {
            // Arrange
            var workGroups = new List<string> { "WG1", "WG2" };
            var entities = new List<TimeCodeValid>
            {
                new() { TimeCode = "JC_TGT", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_TGT" },
                new() { TimeCode = "JC_TGT", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC_TGT" }
            };
            var dtos = new List<TimeCodeValidDto>
            {
                new() { TimeCode = "JC_TGT", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_TGT" },
                new() { TimeCode = "JC_TGT", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC_TGT" }
            };

            _mockRepository
                .CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1")
                .Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1");

            // Assert
            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1");
            _mockMapper.Received(1).Map<IEnumerable<TimeCodeValidDto>>(entities);
        }

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithEmptyWorkGroups_ReturnsEmptyCollection()
        {
            // Arrange — no work groups selected; repository returns empty, mapper returns empty
            var workGroups = new List<string>();
            var entities = new List<TimeCodeValid>();
            var dtos = new List<TimeCodeValidDto>();

            _mockRepository
                .CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1")
                .Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var workGroups = new List<string> { "WG1" };

            _mockRepository
                .CopySelectedWorkGroupsAsync(
                    Arg.Any<IEnumerable<string>>(),
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _sut.CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1"));
        }

        #endregion
    }
}

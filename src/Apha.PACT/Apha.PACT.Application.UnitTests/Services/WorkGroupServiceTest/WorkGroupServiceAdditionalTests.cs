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

namespace Apha.PACT.Application.UnitTests.Services.WorkGroupServiceTest
{
    public class WorkGroupServiceAdditionalTests
    {
        private readonly IWorkGroupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly WorkGroupService _sut;

        public WorkGroupServiceAdditionalTests()
        {
            _mockRepository = Substitute.For<IWorkGroupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new WorkGroupService(_mockRepository, _mockMapper);
        }

        #region GetWorkGroupsByProfitCentreAsync

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WithData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query      = new QueryParameters<string>();
            var parameters = new PaginationParameters<string>();
            var pagedData  = new PagedData<WorkGroup>(
                new List<WorkGroup> { new() { WorkGroupName = "WG1", ProfitCentre = "PC001" } },
                new PaginationData());
            var expected = new PaginatedResult<WorkGroupDto>
            {
                Data = new List<WorkGroupDto> { new() { WorkGroupName = "WG1" } }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetWorkGroupsByProfitCentreAsync(parameters, "PC001").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetWorkGroupsByProfitCentreAsync(query, "PC001");

            // Assert
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetWorkGroupsByProfitCentreAsync(parameters, "PC001");
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_EmptyPage_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query      = new QueryParameters<string>();
            var parameters = new PaginationParameters<string>();
            var pagedData  = new PagedData<WorkGroup>(new List<WorkGroup>(), new PaginationData());
            var expected   = new PaginatedResult<WorkGroupDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetWorkGroupsByProfitCentreAsync(parameters, Arg.Any<string>()).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetWorkGroupsByProfitCentreAsync(query, "PC001");

            // Assert
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var query      = new QueryParameters<string>();
            var parameters = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetWorkGroupsByProfitCentreAsync(parameters, Arg.Any<string>())
                .ThrowsAsync(new Exception("DB error"));
            _mockMapper.Map<PaginatedResult<WorkGroupDto>>(Arg.Any<PagedData<WorkGroup>>())
                .Returns(new PaginatedResult<WorkGroupDto>());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetWorkGroupsByProfitCentreAsync(query, "PC001"));
        }

        #endregion

        #region SetSendEmailForProfitCentreWorkGroupsAsync

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WithValidArgs_DelegatesAndReturnsTrue()
        {
            // Arrange
            _mockRepository.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1).Returns(true);

            // Act
            var result = await _sut.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1);
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_RepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            _mockRepository.SetSendEmailForProfitCentreWorkGroupsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(false);

            // Act
            var result = await _sut.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 0);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.SetSendEmailForProfitCentreWorkGroupsAsync(Arg.Any<string>(), Arg.Any<short>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _sut.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1));
        }

        #endregion

        #region SetSendEmailForAllWorkGroupsAsync

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_WithFlagOne_DelegatesAndReturnsTrue()
        {
            // Arrange
            _mockRepository.SetSendEmailForAllWorkGroupsAsync(1).Returns(true);

            // Act
            var result = await _sut.SetSendEmailForAllWorkGroupsAsync(1);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).SetSendEmailForAllWorkGroupsAsync(1);
        }

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_WithFlagZero_DelegatesAndReturnsTrue()
        {
            // Arrange
            _mockRepository.SetSendEmailForAllWorkGroupsAsync(0).Returns(true);

            // Act
            var result = await _sut.SetSendEmailForAllWorkGroupsAsync(0);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).SetSendEmailForAllWorkGroupsAsync(0);
        }

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.SetSendEmailForAllWorkGroupsAsync(Arg.Any<short>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.SetSendEmailForAllWorkGroupsAsync(0));
        }

        #endregion

        #region UpdateWorkGroupEmailAsync

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WithValidArgs_DelegatesAndReturnsTrue()
        {
            // Arrange
            _mockRepository.UpdateWorkGroupEmailAsync("WG1", 1, "test@test.com").Returns(true);

            // Act
            var result = await _sut.UpdateWorkGroupEmailAsync("WG1", 1, "test@test.com");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).UpdateWorkGroupEmailAsync("WG1", 1, "test@test.com");
        }

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WithNullEmailRecipient_DelegatesNullAndReturnsTrue()
        {
            // Arrange
            _mockRepository.UpdateWorkGroupEmailAsync("WG1", 0, null).Returns(true);

            // Act
            var result = await _sut.UpdateWorkGroupEmailAsync("WG1", 0, null);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).UpdateWorkGroupEmailAsync("WG1", 0, null);
        }

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_RepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            _mockRepository.UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>())
                .Returns(false);

            // Act
            var result = await _sut.UpdateWorkGroupEmailAsync("WG1", 1, "x@y.com");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _sut.UpdateWorkGroupEmailAsync("WG1", 1, "test@test.com"));
        }

        #endregion
    }
}

using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.WorkGroupServiceTest
{
    public class WorkGroupServiceTests
    {
        private readonly IWorkGroupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly WorkGroupService _sut;

        public WorkGroupServiceTests()
        {
            _mockRepository = Substitute.For<IWorkGroupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new WorkGroupService(_mockRepository, _mockMapper);
        }

        #region GetAllWorkGroupsAsync

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithData_ReturnsMappedDtos()
        {
            var entities = new List<WorkGroup>
            {
                new WorkGroup { WorkGroupName = "WG1", ProfitCentre = "PC1" },
                new WorkGroup { WorkGroupName = "WG2", ProfitCentre = "PC2" }
            };
            var dtos = new List<WorkGroupDto>
            {
                new WorkGroupDto { WorkGroupName = "WG1" },
                new WorkGroupDto { WorkGroupName = "WG2" }
            };

            _mockRepository.GetAllWorkGroupsAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<WorkGroupDto>>(entities).Returns(dtos);

            var result = await _sut.GetAllWorkGroupsAsync();

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetAllWorkGroupsAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_EmptyResult_ReturnsEmptyCollection()
        {
            var entities = new List<WorkGroup>();
            var dtos = new List<WorkGroupDto>();

            _mockRepository.GetAllWorkGroupsAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<WorkGroupDto>>(entities).Returns(dtos);

            var result = await _sut.GetAllWorkGroupsAsync();

            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetAllWorkGroupsAsync();
        }
    
        [Fact]
        public async Task GetAllWorkGroupsAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetAllWorkGroupsAsync().ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetAllWorkGroupsAsync());
        }

        #endregion
    }
}

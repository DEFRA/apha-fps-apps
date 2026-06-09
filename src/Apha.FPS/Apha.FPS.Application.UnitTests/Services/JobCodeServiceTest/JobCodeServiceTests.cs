using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.JobCodeServiceTest
{
    public class JobCodeServiceTests
    {
        private readonly IJobCodeRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly JobCodeService _sut;

        public JobCodeServiceTests()
        {
            _mockRepository = Substitute.For<IJobCodeRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new JobCodeService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetJobCodeListAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var jobCodeEntities = new List<JobCode>
            {
                new JobCode { JobCodeId = "JC001", JobCodeName = "Field Operations", Type = "A", FpsYear = 2024 },
                new JobCode { JobCodeId = "JC002", JobCodeName = "Lab Analysis",     Type = "B", FpsYear = 2024 }
            };

            var expectedDtos = new List<JobCodeDto>
            {
                new JobCodeDto { JobCodeId = "JC001", Jobcodename = "Field Operations", Type = "A", Fpscalyear = 2024 },
                new JobCodeDto { JobCodeId = "JC002", Jobcodename = "Lab Analysis",     Type = "B", Fpscalyear = 2024 }
            };

            _mockRepository.GetAllJobCodesAsync()
                .Returns(Task.FromResult<IEnumerable<JobCode>>(jobCodeEntities));

            _mockMapper.Map<IEnumerable<JobCodeDto>>(jobCodeEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetJobCodeListAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().JobCodeId.Should().Be("JC001");
            result.First().Jobcodename.Should().Be("Field Operations");

            await _mockRepository.Received(1).GetAllJobCodesAsync();
            _mockMapper.Received(1).Map<IEnumerable<JobCodeDto>>(jobCodeEntities);
        }

        [Fact]
        public async Task GetJobCodeListAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<JobCode>();
            var emptyDtos = new List<JobCodeDto>();

            _mockRepository.GetAllJobCodesAsync()
                .Returns(Task.FromResult<IEnumerable<JobCode>>(emptyEntities));

            _mockMapper.Map<IEnumerable<JobCodeDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetJobCodeListAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllJobCodesAsync();
            _mockMapper.Received(1).Map<IEnumerable<JobCodeDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetJobCodeListAsync_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetAllJobCodesAsync()
                .Returns(Task.FromResult<IEnumerable<JobCode>>(null!));

            _mockMapper.Map<IEnumerable<JobCodeDto>>(null)
                .Returns((IEnumerable<JobCodeDto>?)null);

            // Act
            var result = await _sut.GetJobCodeListAsync();

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetAllJobCodesAsync();
            _mockMapper.Received(1).Map<IEnumerable<JobCodeDto>>(null);
        }

        [Fact]
        public async Task GetJobCodeListAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllJobCodesAsync()
                .Returns(Task.FromException<IEnumerable<JobCode>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetJobCodeListAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllJobCodesAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<JobCodeDto>>(Arg.Any<IEnumerable<JobCode>>());
        }

        #region GetZtCodeLookupAsync

        [Fact]
        public async Task GetZtCodeLookupAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var ztEntities = new List<ZtJobCodeLookup>
            {
                new() { JobCode = "ZT001", Description = "ZT Project 1" },
                new() { JobCode = "ZT002", Description = "ZT Project 2" }
            };

            var expectedDtos = new List<ZtJobCodeDto>
            {
                new() { JobCode = "ZT001", Description = "ZT Project 1" },
                new() { JobCode = "ZT002", Description = "ZT Project 2" }
            };

            _mockRepository.GetZtJobCodesAsync()
                .Returns(Task.FromResult<IEnumerable<ZtJobCodeLookup>>(ztEntities));

            _mockMapper.Map<IEnumerable<ZtJobCodeDto>>(ztEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetZtCodeLookupAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().JobCode.Should().Be("ZT001");
            result.First().Description.Should().Be("ZT Project 1");

            await _mockRepository.Received(1).GetZtJobCodesAsync();
            _mockMapper.Received(1).Map<IEnumerable<ZtJobCodeDto>>(ztEntities);
        }

        [Fact]
        public async Task GetZtCodeLookupAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<ZtJobCodeLookup>();
            var emptyDtos = new List<ZtJobCodeDto>();

            _mockRepository.GetZtJobCodesAsync()
                .Returns(Task.FromResult<IEnumerable<ZtJobCodeLookup>>(emptyEntities));

            _mockMapper.Map<IEnumerable<ZtJobCodeDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetZtCodeLookupAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetZtJobCodesAsync();
            _mockMapper.Received(1).Map<IEnumerable<ZtJobCodeDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetZtCodeLookupAsync_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetZtJobCodesAsync()
                .Returns(Task.FromResult<IEnumerable<ZtJobCodeLookup>>(null!));

            _mockMapper.Map<IEnumerable<ZtJobCodeDto>>(null)
                .Returns((IEnumerable<ZtJobCodeDto>?)null);

            // Act
            var result = await _sut.GetZtCodeLookupAsync();

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetZtJobCodesAsync();
            _mockMapper.Received(1).Map<IEnumerable<ZtJobCodeDto>>(null);
        }

        [Fact]
        public async Task GetZtCodeLookupAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetZtJobCodesAsync()
                .Returns(Task.FromException<IEnumerable<ZtJobCodeLookup>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetZtCodeLookupAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetZtJobCodesAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<ZtJobCodeDto>>(Arg.Any<IEnumerable<ZtJobCodeLookup>>());
        }

        #endregion
    }
}
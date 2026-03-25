using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Enities;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using AccountCodeServiceUnderTest = Apha.FPS.Application.Services.AccountCodeService;

namespace Apha.FPS.Application.UnitTests.Services.AccountCodeService
{
    public class AccountCodeServiceTests
    {
        private readonly IAccountCodeRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly AccountCodeServiceUnderTest _sut;

        public AccountCodeServiceTests()
        {
            _mockRepository = Substitute.For<IAccountCodeRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new AccountCodeServiceUnderTest(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllAccountCodeAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var accountCodeEntities = new List<AccountCode>
            {
                new AccountCode { Code = "AC001", Description = "Travel Expenses" },
                new AccountCode { Code = "AC002", Description = "Office Supplies" }
            };

            var expectedDtos = new List<AccountCodeDto>
            {
                new AccountCodeDto { Code = "AC001", Description = "Travel Expenses" },
                new AccountCodeDto { Code = "AC002", Description = "Office Supplies" }
            };

            _mockRepository.GetAllAccountCodeAsync()
                .Returns(Task.FromResult<IEnumerable<AccountCode>>(accountCodeEntities));

            _mockMapper.Map<IEnumerable<AccountCodeDto>>(accountCodeEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllAccountCodeAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Code.Should().Be("AC001");
            result.First().Description.Should().Be("Travel Expenses");

            await _mockRepository.Received(1).GetAllAccountCodeAsync();
            _mockMapper.Received(1).Map<IEnumerable<AccountCodeDto>>(accountCodeEntities);
        }

        [Fact]
        public async Task GetAllAccountCodeAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<AccountCode>();
            var emptyDtos = new List<AccountCodeDto>();

            _mockRepository.GetAllAccountCodeAsync()
                .Returns(Task.FromResult<IEnumerable<AccountCode>>(emptyEntities));

            _mockMapper.Map<IEnumerable<AccountCodeDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllAccountCodeAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllAccountCodeAsync();
            _mockMapper.Received(1).Map<IEnumerable<AccountCodeDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetAllAccountCodeAsync_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetAllAccountCodeAsync()
                .Returns(Task.FromResult<IEnumerable<AccountCode>>(null!));

            _mockMapper.Map<IEnumerable<AccountCodeDto>>(null)
                .Returns((IEnumerable<AccountCodeDto>?)null);

            // Act
            var result = await _sut.GetAllAccountCodeAsync();

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetAllAccountCodeAsync();
            _mockMapper.Received(1).Map<IEnumerable<AccountCodeDto>>(null);
        }

        [Fact]
        public async Task GetAllAccountCodeAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllAccountCodeAsync()
                .Returns(Task.FromException<IEnumerable<AccountCode>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllAccountCodeAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllAccountCodeAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<AccountCodeDto>>(Arg.Any<IEnumerable<AccountCode>>());
        }
    }
}
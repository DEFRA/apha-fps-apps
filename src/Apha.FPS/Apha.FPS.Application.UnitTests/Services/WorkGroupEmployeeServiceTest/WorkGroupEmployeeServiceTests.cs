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

namespace Apha.FPS.Application.UnitTests.Services.WorkGroupEmployeeServiceTest
{
    public class WorkGroupEmployeeServiceTests
    {
        private const string DefaultWgGrade = "WG01";
        private const string DefaultPactId  = "PACT001";

        private readonly IWorkGroupEmployeeRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly WorkGroupEmployeeService _sut;

        public WorkGroupEmployeeServiceTests()
        {
            _mockRepository = Substitute.For<IWorkGroupEmployeeRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new WorkGroupEmployeeService(_mockRepository, _mockMapper);
        }

        #region GetWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData    = new PagedData<WorkGroupEmployeeView>();
            var expected     = new PaginatedResult<WorkGroupEmployeeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupEmployeeAsync(mappedParams, DefaultWgGrade).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupEmployeeDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            // Assert
            result.Should().Be(expected);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetWorkGroupEmployeeAsync(mappedParams, DefaultWgGrade);
            _mockMapper.Received(1).Map<PaginatedResult<WorkGroupEmployeeDto>>(pagedData);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetWorkGroupEmployeeAsync_WithNullOrWhitespaceWgGrade_PassesValueToRepository(string wgGrade)
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData    = new PagedData<WorkGroupEmployeeView>();
            var expected     = new PaginatedResult<WorkGroupEmployeeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupEmployeeAsync(mappedParams, wgGrade).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupEmployeeDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetWorkGroupEmployeeAsync(query, wgGrade);

            // Assert
            result.Should().Be(expected);
            await _mockRepository.Received(1).GetWorkGroupEmployeeAsync(mappedParams, wgGrade);
        }

        #endregion

        #region GetWorkGroupEmployeeByIdAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithValidPactId_ReturnsMappedDto()
        {
            // Arrange
            var entity   = new WorkGroupEmployeeView { PactId = DefaultPactId };
            var expected = new WorkGroupEmployeeDto { PactId = DefaultPactId };

            _mockRepository.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns(entity);
            _mockMapper.Map<WorkGroupEmployeeDto>(entity).Returns(expected);

            // Act
            var result = await _sut.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            // Assert
            result.Should().Be(expected);
            await _mockRepository.Received(1).GetWorkGroupEmployeeByIdAsync(DefaultPactId);
            _mockMapper.Received(1).Map<WorkGroupEmployeeDto>(entity);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetWorkGroupEmployeeByIdAsync_WithNullOrWhitespacePactId_ThrowsArgumentException(string pactId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetWorkGroupEmployeeByIdAsync(pactId));

            await _mockRepository.DidNotReceive().GetWorkGroupEmployeeByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns((WorkGroupEmployeeView?)null);
            _mockMapper.Map<WorkGroupEmployeeDto>(null).Returns((WorkGroupEmployeeDto?)null);

            // Act
            var result = await _sut.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task CreateWorkGroupEmployeeAsync_WithValidDto_ReturnsMappedResult()
        {
            // Arrange
            var dto     = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade, HrsPaid = 40.0, Leave = 5.0, SickSpecial = 2.0 };
            var entity  = new WorkGroupEmployee    { PactId = DefaultPactId };
            var created = new WorkGroupEmployee    { PactId = DefaultPactId, HrsPaid = 40.0 };
            var expected = new WorkGroupEmployeeDto { PactId = DefaultPactId };

            _mockMapper.Map<WorkGroupEmployee>(dto).Returns(entity);
            _mockRepository.CreateWorkGroupEmployeeAsync(entity).Returns(created);
            _mockMapper.Map<WorkGroupEmployeeDto>(created).Returns(expected);

            // Act
            var result = await _sut.CreateWorkGroupEmployeeAsync(dto);

            // Assert
            result.Should().Be(expected);
            await _mockRepository.Received(1).CreateWorkGroupEmployeeAsync(entity);
            _mockMapper.Received(1).Map<WorkGroupEmployee>(dto);
            _mockMapper.Received(1).Map<WorkGroupEmployeeDto>(created);
        }

        [Fact]
        public async Task CreateWorkGroupEmployeeAsync_WithNullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.CreateWorkGroupEmployeeAsync(null!));

            await _mockRepository.DidNotReceive().CreateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployee>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateWorkGroupEmployeeAsync_WithNullOrWhitespacePactId_ThrowsArgumentException(string pactId)
        {
            // Arrange
            var dto = new WorkGroupEmployeeDto { PactId = pactId, WorkGroupGrade = DefaultWgGrade };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateWorkGroupEmployeeAsync(dto));

            await _mockRepository.DidNotReceive().CreateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployee>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateWorkGroupEmployeeAsync_WithNullOrWhitespaceWorkGroupGrade_ThrowsArgumentException(string wgGrade)
        {
            // Arrange
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = wgGrade };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateWorkGroupEmployeeAsync(dto));

            await _mockRepository.DidNotReceive().CreateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployee>());
        }

        #endregion

        #region UpdateWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithValidDto_ReturnsMappedResult()
        {
            // Arrange
            var dto      = new WorkGroupEmployeeDto { PactId = DefaultPactId, HrsPaid = 40.0, Leave = 3.0, SickSpecial = 4.0 };
            var existing = new WorkGroupEmployeeView { PactId = DefaultPactId };
            var entity   = new WorkGroupEmployee    { PactId = DefaultPactId };
            var updated  = new WorkGroupEmployee    { PactId = DefaultPactId, HrsPaid = 40.0 };
            var expected = new WorkGroupEmployeeDto { PactId = DefaultPactId };

            _mockRepository.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns(existing);
            _mockMapper.Map<WorkGroupEmployee>(dto).Returns(entity);
            _mockRepository.UpdateWorkGroupEmployeeAsync(entity).Returns(updated);
            _mockMapper.Map<WorkGroupEmployeeDto>(updated).Returns(expected);

            // Act
            var result = await _sut.UpdateWorkGroupEmployeeAsync(dto);

            // Assert
            result.Should().Be(expected);
            await _mockRepository.Received(1).UpdateWorkGroupEmployeeAsync(entity);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithNullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.UpdateWorkGroupEmployeeAsync(null!));

            await _mockRepository.DidNotReceive().UpdateWorkGroupEmployeeAsync(Arg.Any<WorkGroupEmployee>());
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var dto    = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var entity = new WorkGroupEmployee    { PactId = DefaultPactId };

            _mockMapper.Map<WorkGroupEmployee>(dto).Returns(entity);
            _mockRepository.UpdateWorkGroupEmployeeAsync(entity)
                .ThrowsAsync(new KeyNotFoundException("Employee not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sut.UpdateWorkGroupEmployeeAsync(dto));
        }

        #endregion

        #region DeleteWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithValidPactId_ReturnsTrue()
        {
            // Arrange
            var entity = new WorkGroupEmployeeView { PactId = DefaultPactId };
            _mockRepository.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns(entity);
            _mockRepository.DeleteWorkGroupEmployeeAsync(DefaultPactId).Returns(true);

            // Act
            var result = await _sut.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).DeleteWorkGroupEmployeeAsync(DefaultPactId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteWorkGroupEmployeeAsync_WithNullOrWhitespacePactId_ThrowsArgumentException(string pactId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.DeleteWorkGroupEmployeeAsync(pactId));

            await _mockRepository.DidNotReceive().DeleteWorkGroupEmployeeAsync(Arg.Any<string>());
        }

        #endregion
    }
}
    
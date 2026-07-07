using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.TotalBusinessOverheadsServiceTest
{
    public class TotalBusinessOverheadsServiceTests
    {
        private readonly ITotalBusinessOverheadsRepository _mockRepository;
        private readonly IFpsRequestContext _mockRequestContext;
        private readonly IMapper _mockMapper;
        private readonly TotalBusinessOverheadsService _sut;

        public TotalBusinessOverheadsServiceTests()
        {
            _mockRepository = Substitute.For<ITotalBusinessOverheadsRepository>();
            _mockRequestContext = Substitute.For<IFpsRequestContext>();
            _mockMapper = Substitute.For<IMapper>();
            _mockRequestContext.FpsYear.Returns(2025);
            _sut = new TotalBusinessOverheadsService(_mockRepository, _mockRequestContext, _mockMapper);
        }

        private static TotalBusinessOverheadsDto BuildDto(decimal? overheads = 1000000m, int fpsYear = 2025) =>
            new() { TotalBusinessOverheads = overheads, FpsYear = fpsYear };

        private static TotalBusinessOverheads BuildEntity(decimal? overheads = 1000000m, int fpsYear = 2025) =>
            new() { BusinessOverheads = overheads, FpsYear = fpsYear };

        #region GetAsync Tests

        [Fact]
        public async Task GetAsync_ReturnsNull_WhenRecordNotFound()
        {
            // Arrange
            _mockRepository.GetByYearAsync(2025).Returns((TotalBusinessOverheads?)null);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            result.Should().BeNull();
            await _mockRepository.Received(1).GetByYearAsync(2025);
        }

        [Fact]
        public async Task GetAsync_ReturnsMappedDto_WhenRecordFound()
        {
            // Arrange
            var entity = BuildEntity();
            var dto = BuildDto();

            _mockRepository.GetByYearAsync(2025).Returns(entity);
            _mockMapper.Map<TotalBusinessOverheadsDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByYearAsync(2025);
            _mockMapper.Received(1).Map<TotalBusinessOverheadsDto>(entity);
        }

        [Fact]
        public async Task GetAsync_UsesCurrentFpsYearFromContext()
        {
            // Arrange
            _mockRequestContext.FpsYear.Returns(2026);
            var entity = BuildEntity(fpsYear: 2026);
            _mockRepository.GetByYearAsync(2026).Returns(entity);
            _mockMapper.Map<TotalBusinessOverheadsDto>(entity).Returns(BuildDto(fpsYear: 2026));

            // Act
            await _sut.GetAsync();

            // Assert
            await _mockRepository.Received(1).GetByYearAsync(2026);
        }

        [Fact]
        public async Task GetAsync_WithNullOverheads_ReturnsDtoWithNullOverheads()
        {
            // Arrange
            var entity = BuildEntity(null);
            var dto = BuildDto(null);

            _mockRepository.GetByYearAsync(2025).Returns(entity);
            _mockMapper.Map<TotalBusinessOverheadsDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            result.Should().NotBeNull();
            result!.TotalBusinessOverheads.Should().BeNull();
        }

        [Fact]
        public async Task GetAsync_WithZeroOverheads_ReturnsDtoWithZeroOverheads()
        {
            // Arrange
            var entity = BuildEntity(0m);
            var dto = BuildDto(0m);

            _mockRepository.GetByYearAsync(2025).Returns(entity);
            _mockMapper.Map<TotalBusinessOverheadsDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            result.Should().NotBeNull();
            result!.TotalBusinessOverheads.Should().Be(0m);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenRecordNotFound()
        {
            // Arrange
            var dto = BuildDto();
            _mockRepository.GetByYearAsync(2025).Returns((TotalBusinessOverheads?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(dto));
            exception.Message.Should().Contain("Total Business Overheads record for year '2025' was not found.");
        }

        [Fact]
        public async Task UpdateAsync_UpdatesEntityAndReturnsMappedDto()
        {
            // Arrange
            var dto = BuildDto(1500000m);
            var existingEntity = BuildEntity(1000000m);
            var updatedEntity = BuildEntity(1500000m);
            var resultDto = BuildDto(1500000m);

            _mockRepository.GetByYearAsync(2025).Returns(existingEntity);
            _mockRepository.UpdateAsync(existingEntity).Returns(updatedEntity);
            _mockMapper.Map<TotalBusinessOverheadsDto>(updatedEntity).Returns(resultDto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            result.Should().Be(resultDto);
            existingEntity.BusinessOverheads.Should().Be(1500000m);
            await _mockRepository.Received(1).GetByYearAsync(2025);
            await _mockRepository.Received(1).UpdateAsync(existingEntity);
            _mockMapper.Received(1).Map<TotalBusinessOverheadsDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdateAsync_UsesCurrentFpsYearFromContext()
        {
            // Arrange
            _mockRequestContext.FpsYear.Returns(2026);
            var dto = BuildDto(fpsYear: 2026);
            var existingEntity = BuildEntity(fpsYear: 2026);
            var updatedEntity = BuildEntity(fpsYear: 2026);

            _mockRepository.GetByYearAsync(2026).Returns(existingEntity);
            _mockRepository.UpdateAsync(existingEntity).Returns(updatedEntity);
            _mockMapper.Map<TotalBusinessOverheadsDto>(updatedEntity).Returns(dto);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert
            await _mockRepository.Received(1).GetByYearAsync(2026);
        }

        [Fact]
        public async Task UpdateAsync_WithNullOverheads_UpdatesToNull()
        {
            // Arrange
            var dto = BuildDto(null);
            var existingEntity = BuildEntity(1000000m);
            var updatedEntity = BuildEntity(null);
            var resultDto = BuildDto(null);

            _mockRepository.GetByYearAsync(2025).Returns(existingEntity);
            _mockRepository.UpdateAsync(existingEntity).Returns(updatedEntity);
            _mockMapper.Map<TotalBusinessOverheadsDto>(updatedEntity).Returns(resultDto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            existingEntity.BusinessOverheads.Should().BeNull();
            result!.TotalBusinessOverheads.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_WithZeroOverheads_UpdatesToZero()
        {
            // Arrange
            var dto = BuildDto(0m);
            var existingEntity = BuildEntity(1000000m);
            var updatedEntity = BuildEntity(0m);
            var resultDto = BuildDto(0m);

            _mockRepository.GetByYearAsync(2025).Returns(existingEntity);
            _mockRepository.UpdateAsync(existingEntity).Returns(updatedEntity);
            _mockMapper.Map<TotalBusinessOverheadsDto>(updatedEntity).Returns(resultDto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            existingEntity.BusinessOverheads.Should().Be(0m);
            result.TotalBusinessOverheads.Should().Be(0m);
        }

        [Fact]
        public async Task UpdateAsync_WithLargeValue_UpdatesSuccessfully()
        {
            // Arrange
            var dto = BuildDto(999999999.99m);
            var existingEntity = BuildEntity(1000000m);
            var updatedEntity = BuildEntity(999999999.99m);
            var resultDto = BuildDto(999999999.99m);

            _mockRepository.GetByYearAsync(2025).Returns(existingEntity);
            _mockRepository.UpdateAsync(existingEntity).Returns(updatedEntity);
            _mockMapper.Map<TotalBusinessOverheadsDto>(updatedEntity).Returns(resultDto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            existingEntity.BusinessOverheads.Should().Be(999999999.99m);
            result.TotalBusinessOverheads.Should().Be(999999999.99m);
        }

        [Fact]
        public async Task UpdateAsync_OnlyUpdatesBusinessOverheadsField()
        {
            // Arrange
            var dto = BuildDto(1500000m);
            var existingEntity = BuildEntity(1000000m);
            var originalFpsYear = existingEntity.FpsYear;

            _mockRepository.GetByYearAsync(2025).Returns(existingEntity);
            _mockRepository.UpdateAsync(existingEntity).Returns(existingEntity);
            _mockMapper.Map<TotalBusinessOverheadsDto>(existingEntity).Returns(dto);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert
            existingEntity.FpsYear.Should().Be(originalFpsYear);
            existingEntity.BusinessOverheads.Should().Be(1500000m);
        }

        [Fact]
        public async Task UpdateAsync_WhenRepositoryThrowsException_ExceptionPropagates()
        {
            // Arrange
            var dto = BuildDto();
            var existingEntity = BuildEntity();

            _mockRepository.GetByYearAsync(2025).Returns(existingEntity);
            _mockRepository.UpdateAsync(existingEntity).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(dto));
        }

        #endregion
    }
}

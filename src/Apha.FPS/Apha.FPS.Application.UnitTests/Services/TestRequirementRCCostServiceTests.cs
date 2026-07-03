using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.TestRequirementRCCostServiceTest
{
    public class TestRequirementRCCostServiceTests
    {
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER01";
        private const string DefaultProfitCentre = "PC001";
        private const int DefaultFpsYear = 2025;

        private readonly ITestRequirementRCCostRepository _repository;
        private readonly IMapper _mapper;
        private readonly TestRequirementRCCostService _service;

        public TestRequirementRCCostServiceTests()
        {
            _repository = Substitute.For<ITestRequirementRCCostRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new TestRequirementRCCostService(_repository, _mapper);
        }

        #region GetByTestCodeAsync

        [Fact]
        public async Task GetByTestCodeAsync_ValidInput_ReturnsDtoList()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost> { CreateTestEntity() };
            var dtos = new List<TestRequirementRCCostDto> { CreateTestDto() };

            _repository.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear).Returns(entities);
            _mapper.Map<IEnumerable<TestRequirementRCCostDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            await _repository.Received(1).GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);
        }

        [Fact]
        public async Task GetByTestCodeAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>();
            var dtos = new List<TestRequirementRCCostDto>();

            _repository.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear).Returns(entities);
            _mapper.Map<IEnumerable<TestRequirementRCCostDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByTestCodeAsync_WhitespaceTestCode_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByTestCodeAsync("  ", DefaultFpsYear));
        }

        [Fact]
        public async Task GetByTestCodeAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByTestCodeAsync(DefaultTestCode, 0));
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_ExistingRecord_ReturnsDto()
        {
            // Arrange
            var entity = CreateTestEntity();
            var dto = CreateTestDto();

            _repository.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear)
                .Returns(entity);
            _mapper.Map<TestRequirementRCCostDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByKeyAsync_RecordNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetByKeyAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear)
                .Returns((TestRequirementRCCost?)null);

            // Act
            var result = await _service.GetByKeyAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WhitespaceBuyer_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByKeyAsync(DefaultTestCode, "  ", DefaultProfitCentre, DefaultFpsYear));
        }

        [Fact]
        public async Task GetByKeyAsync_WhitespaceProfitCentre_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByKeyAsync(DefaultTestCode, DefaultBuyer, "  ", DefaultFpsYear));
        }

        [Fact]
        public async Task GetByKeyAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, -1));
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_CreatesSuccessfully()
        {
            // Arrange
            var dto = CreateTestDto();
            var entity = CreateTestEntity();

            _repository.ExistsAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear)
                .Returns(false);
            _mapper.Map<TestRequirementRCCost>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(entity);
            _mapper.Map<TestRequirementRCCostDto>(entity).Returns(dto);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_EmptyTestCode_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.TestCode = string.Empty;
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_EmptyBuyer_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.Buyer = string.Empty;
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_EmptyProfitCentre_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.ProfitCentre = string.Empty;
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.FpsYear = 0;
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_DuplicatePrimaryKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = CreateTestDto();
            _repository.ExistsAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear)
                .Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
            Assert.Contains("already exists", ex.Message);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidDto_UpdatesSuccessfully()
        {
            // Arrange
            var dto = CreateTestDto();
            var entity = CreateTestEntity();

            _repository.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear)
                .Returns(entity);
            _mapper.Map<TestRequirementRCCost>(dto).Returns(entity);
            _repository.UpdateAsync(entity).Returns(entity);
            _mapper.Map<TestRequirementRCCostDto>(entity).Returns(dto);

            // Act
            var result = await _service.UpdateAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, dto);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, null!));
        }

        [Fact]
        public async Task UpdateAsync_TestCodeMismatch_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.TestCode = "DIFFERENT";
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, dto));
        }

        [Fact]
        public async Task UpdateAsync_BuyerMismatch_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.Buyer = "DIFFERENT";
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, dto));
        }

        [Fact]
        public async Task UpdateAsync_ProfitCentreMismatch_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.ProfitCentre = "DIFFERENT";
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, dto));
        }

        [Fact]
        public async Task UpdateAsync_FpsYearMismatch_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, 9999, dto));
        }

        [Fact]
        public async Task UpdateAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = CreateTestDto();
            _repository.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear)
                .Returns((TestRequirementRCCost?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, dto));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingRecord_ReturnsTrue()
        {
            // Arrange
            _repository.DeleteAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear).Returns(true);

            // Act
            var result = await _service.DeleteAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.True(result);
            await _repository.Received(1).DeleteAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.DeleteAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear).Returns(false);

            // Act
            var result = await _service.DeleteAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhitespaceTestCode_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteAsync("  ", DefaultBuyer, DefaultProfitCentre, DefaultFpsYear));
        }

        [Fact]
        public async Task DeleteAsync_WhitespaceBuyer_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteAsync(DefaultTestCode, "  ", DefaultProfitCentre, DefaultFpsYear));
        }

        [Fact]
        public async Task DeleteAsync_WhitespaceProfitCentre_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteAsync(DefaultTestCode, DefaultBuyer, "  ", DefaultFpsYear));
        }

        [Fact]
        public async Task DeleteAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, 0));
        }

        #endregion

        #region Helper Methods

        private static TestRequirementRCCost CreateTestEntity() =>
            new()
            {
                TestCode = DefaultTestCode,
                Buyer = DefaultBuyer,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 200m
            };

        private static TestRequirementRCCostDto CreateTestDto() =>
            new()
            {
                TestCode = DefaultTestCode,
                Buyer = DefaultBuyer,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 200m
            };

        #endregion
    }
}

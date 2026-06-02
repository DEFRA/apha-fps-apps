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

namespace Apha.FPS.Application.UnitTests.Services.ProfitCentreGradeServiceTest
{
    public class ProfitCentreGradeServiceTests
    {
        private const string DefaultProfitCentre = "PC01";

        private readonly IProfitCentreGradeRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProfitCentreGradeService _sut;

        public ProfitCentreGradeServiceTests()
        {
            _mockRepository = Substitute.For<IProfitCentreGradeRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new ProfitCentreGradeService(_mockRepository, _mockMapper);
        }

        #region GetProfitCentreGradesAsync Tests

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData    = new PagedData<ProfitCentreGrade>();
            var expected     = new PaginatedResult<ProfitCentreGradeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetProfitCentreGradesAsync(mappedParams, DefaultProfitCentre).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProfitCentreGradeDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            result.Should().Be(expected);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetProfitCentreGradesAsync(mappedParams, DefaultProfitCentre);
            _mockMapper.Received(1).Map<PaginatedResult<ProfitCentreGradeDto>>(pagedData);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProfitCentreGradesAsync_WithNullOrWhitespaceProfitCentre_ThrowsArgumentException(string profitCentre)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetProfitCentreGradesAsync(query, profitCentre));

            await _mockRepository.DidNotReceive()
                .GetProfitCentreGradesAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetProfitCentreGradesAsync(mappedParams, DefaultProfitCentre)
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetProfitCentreGradesAsync(query, DefaultProfitCentre));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetAllPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData    = new PagedData<ProfitCentreGrade>();
            var expected     = new PaginatedResult<ProfitCentreGradeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProfitCentreGradeDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            result.Should().Be(expected);
            await _mockRepository.Received(1).GetAllPagedAsync(mappedParams);
        }

        #endregion

        #region GetByIdAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetByIdAsync_WithNullOrWhitespacePcGrade_ThrowsArgumentException(string pcGrade)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByIdAsync(pcGrade));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenEntityNotFound()
        {
            _mockRepository.GetByIdAsync("NOTEXIST").Returns((ProfitCentreGrade?)null);

            var result = await _sut.GetByIdAsync("NOTEXIST");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsMappedDto_WhenFound()
        {
            // Arrange
            var entity = new ProfitCentreGrade { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var dto    = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };

            _mockRepository.GetByIdAsync("G001").Returns(entity);
            _mockMapper.Map<ProfitCentreGradeDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetByIdAsync("G001");

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByIdAsync("G001");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenProfitCentreDoesNotExist()
        {
            // Arrange
            var dto = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = "INVALID" };

            _mockRepository.ProfitCentreExistsAsync("INVALID").Returns(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto));
            ex.Message.Should().Contain("INVALID");
            await _mockRepository.DidNotReceive().CreateAsync(Arg.Any<ProfitCentreGrade>());
        }

        [Fact]
        public async Task CreateAsync_ReturnsMappedDto_WhenProfitCentreExists()
        {
            // Arrange
            var dto     = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var entity  = new ProfitCentreGrade { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var created = new ProfitCentreGrade { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };

            _mockRepository.ProfitCentreExistsAsync(DefaultProfitCentre).Returns(true);
            _mockMapper.Map<ProfitCentreGrade>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<ProfitCentreGradeDto>(created).Returns(dto);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        #endregion

        #region UpdateAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateAsync_ThrowsArgumentException_WhenOriginalPcGradeIsNullOrWhitespace(string originalPcGrade)
        {
            var dto = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(originalPcGrade, dto));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateAsync("G001", null!));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenProfitCentreDoesNotExist()
        {
            // Arrange
            var dto = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = "INVALID" };

            _mockRepository.ProfitCentreExistsAsync("INVALID").Returns(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync("G001", dto));
            ex.Message.Should().Contain("INVALID");
            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<string>(), Arg.Any<ProfitCentreGrade>());
        }

        [Fact]
        public async Task UpdateAsync_ReturnsMappedDto_WhenProfitCentreExists()
        {
            // Arrange
            var dto     = new ProfitCentreGradeDto { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var entity  = new ProfitCentreGrade { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };
            var updated = new ProfitCentreGrade { PcGrade = "G001", ProfitCentre = DefaultProfitCentre };

            _mockRepository.ProfitCentreExistsAsync(DefaultProfitCentre).Returns(true);
            _mockMapper.Map<ProfitCentreGrade>(dto).Returns(entity);
            _mockRepository.UpdateAsync("G001", entity).Returns(updated);
            _mockMapper.Map<ProfitCentreGradeDto>(updated).Returns(dto);

            // Act
            var result = await _sut.UpdateAsync("G001", dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).UpdateAsync("G001", entity);
        }

        #endregion

        #region DeleteAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteAsync_ThrowsArgumentException_WhenPcGradeIsNullOrWhitespace(string pcGrade)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteAsync(pcGrade));
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
        {
            _mockRepository.DeleteAsync("G001").Returns(true);

            var result = await _sut.DeleteAsync("G001");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync("G001");
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            _mockRepository.DeleteAsync("NOTEXIST").Returns(false);

            var result = await _sut.DeleteAsync("NOTEXIST");

            result.Should().BeFalse();
        }

        #endregion

        #region GetAllProfitCentreCodesAsync Tests

        [Fact]
        public async Task GetAllProfitCentreCodesAsync_ReturnsCodes()
        {
            // Arrange
            var codes = new List<string> { "PC01", "PC02", "PC03" };
            _mockRepository.GetAllProfitCentreCodesAsync().Returns(codes);

            // Act
            var result = await _sut.GetAllProfitCentreCodesAsync();

            // Assert
            result.Should().BeEquivalentTo(codes);
            await _mockRepository.Received(1).GetAllProfitCentreCodesAsync();
        }

        [Fact]
        public async Task GetAllProfitCentreCodesAsync_ReturnsEmpty_WhenNoCodes()
        {
            _mockRepository.GetAllProfitCentreCodesAsync().Returns([]);

            var result = await _sut.GetAllProfitCentreCodesAsync();

            result.Should().BeEmpty();
        }

        #endregion
    }
}

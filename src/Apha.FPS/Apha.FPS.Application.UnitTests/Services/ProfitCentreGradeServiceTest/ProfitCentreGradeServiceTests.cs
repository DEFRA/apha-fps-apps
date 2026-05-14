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
    }
}

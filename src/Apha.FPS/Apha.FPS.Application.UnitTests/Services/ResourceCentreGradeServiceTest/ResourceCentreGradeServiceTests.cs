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

namespace Apha.FPS.Application.UnitTests.Services.ResourceCentreGradeServiceTest
{
    public class ResourceCentreGradeServiceTests
    {
        private const string DefaultProfitCentre = "PC01";

        private readonly IResourceCentreGradeRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ResourceCentreGradeService _sut;

        public ResourceCentreGradeServiceTests()
        {
            _mockRepository = Substitute.For<IResourceCentreGradeRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new ResourceCentreGradeService(_mockRepository, _mockMapper);
        }

        #region GetResourceCentreGradesAsync Tests

        [Fact]
        public async Task GetResourceCentreGradesAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData    = new PagedData<ProfitCentreGrade>();
            var expected     = new PaginatedResult<ProfitCentreGradeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetResourceCentreGradesAsync(mappedParams, DefaultProfitCentre).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProfitCentreGradeDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetResourceCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            result.Should().Be(expected);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetResourceCentreGradesAsync(mappedParams, DefaultProfitCentre);
            _mockMapper.Received(1).Map<PaginatedResult<ProfitCentreGradeDto>>(pagedData);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetResourceCentreGradesAsync_WithNullOrWhitespaceProfitCentre_ThrowsArgumentException(string profitCentre)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetResourceCentreGradesAsync(query, profitCentre));

            await _mockRepository.DidNotReceive()
                .GetResourceCentreGradesAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetResourceCentreGradesAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetResourceCentreGradesAsync(mappedParams, DefaultProfitCentre)
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetResourceCentreGradesAsync(query, DefaultProfitCentre));
        }

        #endregion
    }
}

using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.CalenderMonthServiceTest
{
    public class CalenderMonthServiceTests
    {
        private readonly ICalenderMonthRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly CalenderMonthService _sut;

        public CalenderMonthServiceTests()
        {
            _mockRepository = Substitute.For<ICalenderMonthRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new CalenderMonthService(_mockRepository, _mockMapper);
        }

        #region GetAllCalenderMonthsAsync

        [Fact]
        public async Task GetAllCalenderMonthsAsync_WithData_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<CalenderMonth>
            {
                new() { MonthNumber = 1, MonthName = "January",  AccntsPeriod = 1, Fquarter = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2, Fquarter = 1 }
            };
            var dtos = new List<CalenderMonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January",  AccntsPeriod = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2 }
            };

            _mockRepository.GetAllCalenderMonthsAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<CalenderMonthDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetAllCalenderMonthsAsync();

            // Assert
            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetAllCalenderMonthsAsync();
        }

        [Fact]
        public async Task GetAllCalenderMonthsAsync_EmptyResult_ReturnsEmptyCollection()
        {
            // Arrange
            var entities = new List<CalenderMonth>();
            var dtos = new List<CalenderMonthDto>();

            _mockRepository.GetAllCalenderMonthsAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<CalenderMonthDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetAllCalenderMonthsAsync();

            // Assert
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetAllCalenderMonthsAsync();
        }

        [Fact]
        public async Task GetAllCalenderMonthsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllCalenderMonthsAsync().ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetAllCalenderMonthsAsync());
        }

        #endregion
    }
}

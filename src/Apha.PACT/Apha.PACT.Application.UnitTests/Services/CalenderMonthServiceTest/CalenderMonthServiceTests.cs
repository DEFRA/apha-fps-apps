using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.CalenderMonthServiceTest
{
    public class CalenderMonthServiceTests
    {
        private readonly ICalenderMonthRepository _repository;
        private readonly IMapper _mapper;
        private readonly CalenderMonthService _sut;

        public CalenderMonthServiceTests()
        {
            _repository = Substitute.For<ICalenderMonthRepository>();
            _mapper = Substitute.For<IMapper>();
            _sut = new CalenderMonthService(_repository, _mapper);
        }

        #region GetCalenderMonthsAsync

        [Fact]
        public async Task GetCalenderMonthsAsync_WithData_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<CalenderMonth>
            {
                new() { MonthNumber = 1, MonthName = "January", AccntsPeriod = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2 }
            };
            var dtos = new List<CalenderMonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January", AccntsPeriod = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2 }
            };

            _repository.GetCalenderMonthsAsync().Returns(entities);
            _mapper.Map<IEnumerable<CalenderMonthDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            await _repository.Received(1).GetCalenderMonthsAsync();
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_EmptyResult_ReturnsEmptyCollection()
        {
            // Arrange
            var emptyEntities = new List<CalenderMonth>();
            var emptyDtos = new List<CalenderMonthDto>();

            _repository.GetCalenderMonthsAsync().Returns(emptyEntities);
            _mapper.Map<IEnumerable<CalenderMonthDto>>(emptyEntities).Returns(emptyDtos);

            // Act
            var result = await _sut.GetCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _repository.Received(1).GetCalenderMonthsAsync();
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetCalenderMonthsAsync().ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetCalenderMonthsAsync());
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_ReturnsMappedResultFromMapper()
        {
            // Arrange
            var entities = new List<CalenderMonth>
            {
                new() { MonthNumber = 3, MonthName = "March", AccntsPeriod = 3 }
            };
            var dtos = new List<CalenderMonthDto>
            {
                new() { MonthNumber = 3, MonthName = "March", AccntsPeriod = 3 }
            };

            _repository.GetCalenderMonthsAsync().Returns(entities);
            _mapper.Map<IEnumerable<CalenderMonthDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetCalenderMonthsAsync();

            // Assert
            Assert.Same(dtos, result);
            _mapper.Received(1).Map<IEnumerable<CalenderMonthDto>>(entities);
        }

        #endregion
    }
}

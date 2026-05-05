using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.PACT.Application.UnitTests.Services.MonthServiceTest
{
    public class MonthServiceTests
    {
        private readonly IMapper _mapper;
        private readonly IMonthRepository _repository;
        private readonly MonthService _service;

        public MonthServiceTests()
        {
            _mapper = Substitute.For<IMapper>();
            _repository = Substitute.For<IMonthRepository>();
            _service = new MonthService(_repository, _mapper);
        }

        [Fact]
        public async Task GetAllMonthsAsync_ReturnsListOfMonths()
        {
            // Arrange
            var entities = new List<Month>
            {
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };
            var dtos = new List<MonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };

            _repository.GetAllMonthsAsync()
                .Returns(entities);
            _mapper.Map<IEnumerable<MonthDto>>(entities)
                .Returns(dtos);

            // Act
            var result = await _service.GetAllMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            await _repository.Received(1).GetAllMonthsAsync();
        }

        [Fact]
        public async Task GetAllMonthsAsync_ReturnsEmptyList_WhenNoMonthsExist()
        {
            // Arrange
            var emptyEntities = new List<Month>();
            var emptyDtos = new List<MonthDto>();

            _repository.GetAllMonthsAsync()
                .Returns(emptyEntities);
            _mapper.Map<IEnumerable<MonthDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _service.GetAllMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}

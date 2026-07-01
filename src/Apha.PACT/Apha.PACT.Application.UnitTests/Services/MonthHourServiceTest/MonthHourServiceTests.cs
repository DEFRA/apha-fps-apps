using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.MonthHourServiceTest
{
    public class MonthHourServiceTests
    {
        private readonly IMonthHourRepository _repository;
        private readonly IMapper _mapper;
        private readonly MonthHourService _service;

        public MonthHourServiceTests()
        {
            _repository = Substitute.For<IMonthHourRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new MonthHourService(_repository, _mapper);
        }

        [Fact]
        public async Task GetAllAsync_WithData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pagedData = new PagedData<MonthHour>(
                [new MonthHour { Year = 2025, Month = 1, CvlHours = 160 }],
                new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });
            var mappedResult = new PaginatedResult<MonthHourDto>
            {
                Data = [new MonthHourDto { Year = 2025, Month = 1, CvlHours = 160 }],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetAllAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<MonthHourDto>>(pagedData).Returns(mappedResult);

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _repository.Received(1).GetAllAsync(mappedParams);
            _mapper.Received(1).Map<PaginatedResult<MonthHourDto>>(pagedData);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyData_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pagedData = new PagedData<MonthHour>([], new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 });
            var mappedResult = new PaginatedResult<MonthHourDto> { Data = [] };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetAllAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<MonthHourDto>>(pagedData).Returns(mappedResult);

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetAllAsync(mappedParams).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetAllAsync(query));
        }

        [Fact]
        public async Task GetByYearAsync_WithData_ReturnsMappedItems()
        {
            // Arrange
            const short year = 2025;
            var entities = new List<MonthHour> { new() { Year = year, Month = 1, CvlHours = 160 } };
            var dtos = new List<MonthHourDto> { new() { Year = year, Month = 1, CvlHours = 160 } };

            _repository.GetByYearAsync(year).Returns(entities);
            _mapper.Map<IEnumerable<MonthHourDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            await _repository.Received(1).GetByYearAsync(year);
            _mapper.Received(1).Map<IEnumerable<MonthHourDto>>(entities);
        }

        [Fact]
        public async Task GetByYearAsync_WithEmptyData_ReturnsEmptyCollection()
        {
            // Arrange
            const short year = 1900;
            var entities = new List<MonthHour>();
            var dtos = new List<MonthHourDto>();

            _repository.GetByYearAsync(year).Returns(entities);
            _mapper.Map<IEnumerable<MonthHourDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByYearAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetByYearAsync(Arg.Any<short>()).ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetByYearAsync(2025));
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithData_ReturnsYears()
        {
            // Arrange
            var years = new List<short> { 2023, 2024, 2025 };
            _repository.GetDistinctYearsAsync().Returns(years);

            // Act
            var result = await _service.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            await _repository.Received(1).GetDistinctYearsAsync();
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithEmptyData_ReturnsEmptyCollection()
        {
            // Arrange
            _repository.GetDistinctYearsAsync().Returns(new List<short>());

            // Act
            var result = await _service.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDistinctYearsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetDistinctYearsAsync().ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetDistinctYearsAsync());
        }
    }
}

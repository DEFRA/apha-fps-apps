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

namespace Apha.FPS.Application.UnitTests.Services.MonthlyOutputCalcsServiceTest
{
    public class MonthlyOutputCalcsServiceTests
    {
        private readonly IMonthlyOutputCalcsRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly MonthlyOutputCalcsService _sut;

        public MonthlyOutputCalcsServiceTests()
        {
            _mockRepository = Substitute.For<IMonthlyOutputCalcsRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new MonthlyOutputCalcsService(_mockRepository, _mockMapper);
        }

        private static QueryParameters<string> DefaultQuery() => new() { Page = 1, PageSize = 10 };
        private static PaginationParameters<string> DefaultFilter() => new() { Page = 1, PageSize = 10 };

        private static PagedData<MonthlyOutputCalcsView> MakePagedData(IEnumerable<MonthlyOutputCalcsView> items)
        {
            var list = items.ToList();
            return new PagedData<MonthlyOutputCalcsView>(list, new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = list.Count });
        }

        private static PaginatedResult<MonthlyOutputCalcsViewDto> MakePaginatedResult(IEnumerable<MonthlyOutputCalcsViewDto> items)
        {
            var list = items.ToList();
            return new PaginatedResult<MonthlyOutputCalcsViewDto>(list, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = list.Count });
        }

        [Fact]
        public async Task GetByProjectAsync_WithValidData_ReturnsMappedDtoList()
        {
            var query = DefaultQuery(); var filter = DefaultFilter(); var projectCode = "AH0033";
            var entities = new List<MonthlyOutputCalcsView> { new() { Buyer = projectCode, TestCode = "TC01" }, new() { Buyer = projectCode, TestCode = "TC02" } };
            var pagedData = MakePagedData(entities);
            var expectedDtos = new List<MonthlyOutputCalcsViewDto> { new() { Buyer = projectCode, TestCode = "TC01" }, new() { Buyer = projectCode, TestCode = "TC02" } };
            var expectedResult = MakePaginatedResult(expectedDtos);
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetByProjectAsync(filter, projectCode).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputCalcsViewDto>>(pagedData).Returns(expectedResult);
            var result = await _sut.GetByProjectAsync(query, projectCode);
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            await _mockRepository.Received(1).GetByProjectAsync(filter, projectCode);
        }

        [Fact]
        public async Task GetByProjectAsync_WithEmptyData_ReturnsMappedEmptyList()
        {
            var query = DefaultQuery(); var filter = DefaultFilter(); var projectCode = "AH0033";
            var pagedData = MakePagedData(Enumerable.Empty<MonthlyOutputCalcsView>());
            var emptyResult = MakePaginatedResult(Enumerable.Empty<MonthlyOutputCalcsViewDto>());
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetByProjectAsync(filter, projectCode).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputCalcsViewDto>>(pagedData).Returns(emptyResult);
            var result = await _sut.GetByProjectAsync(query, projectCode);
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByProjectAsync_WhenRepositoryThrows_PropagatesException()
        {
            var query = DefaultQuery(); var filter = DefaultFilter();
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetByProjectAsync(filter, "X").ThrowsAsync(new Exception("DB error"));
            var act = async () => await _sut.GetByProjectAsync(query, "X");
            await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithValidData_ReturnsMappedTotalsDto()
        {
            _mockRepository.GetTotalActualByProjectAsync("AH0033").Returns((TotalVolume: 10.0, TotalCost: 1200.0));
            var result = await _sut.GetTotalActualByProjectAsync("AH0033");
            result.TotalVolume.Should().Be(10);
            result.TotalCost.Should().Be(1200);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenRepositoryThrows_PropagatesException()
        {
            _mockRepository.GetTotalActualByProjectAsync("X").ThrowsAsync(new Exception("DB error"));
            var act = async () => await _sut.GetTotalActualByProjectAsync("X");
            await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
        }

        [Fact]
        public async Task DeleteAsync_WithValidKey_ReturnsTrue()
        {
            _mockRepository.DeleteAsync("AH0033", "TC01", 1.0, "WG1").Returns(true);
            var result = await _sut.DeleteAsync("AH0033", "TC01", 1.0, "WG1");
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordNotFound_ReturnsFalse()
        {
            _mockRepository.DeleteAsync("XX", "XX", 99.0, "XX").Returns(false);
            (await _sut.DeleteAsync("XX", "XX", 99.0, "XX")).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrows_PropagatesException()
        {
            _mockRepository.DeleteAsync("X", "X", 1.0, "X").ThrowsAsync(new Exception("DB error"));
            var act = async () => await _sut.DeleteAsync("X", "X", 1.0, "X");
            await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
        }
    }
}
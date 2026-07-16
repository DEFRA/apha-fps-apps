using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.TestActualBreakdownServiceTest
{
    public class TestActualBreakdownServiceTests
    {
        private readonly ITestActualBreakdownRepository _repository;
        private readonly IMapper _mapper;
        private readonly TestActualBreakdownService _sut;

        public TestActualBreakdownServiceTests()
        {
            _repository = Substitute.For<ITestActualBreakdownRepository>();
            _mapper     = Substitute.For<IMapper>();
            _sut        = new TestActualBreakdownService(_repository, _mapper);
        }

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_ValidQuery_MapsQueryAndCallsRepository()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData());
            var expected     = new PaginatedResult<TestActualBreakdownDto>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetPagedAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedAsync(query);

            result.Should().Be(expected);
            await _repository.Received(1).GetPagedAsync(mappedParams);
        }

        [Fact]
        public async Task GetPagedAsync_WithItems_ReturnsMappedDtos()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300", Program = "Viro",  Month = 4, PCPrice = 159.00m, PCCost = 319.00m, FpsYear = 2025 },
                new() { TestCode = "PT0049", Buyer = "SB4600", Program = "Bact",  Month = 4, PCPrice = 313.00m, PCCost = 313.00m, FpsYear = 2025 }
            };
            var pagedData = new PagedData<TestActualBreakdownView>(views, new PaginationData());
            var dtos = new List<TestActualBreakdownDto>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300" },
                new() { TestCode = "PT0049", Buyer = "SB4600" }
            };
            var expected = new PaginatedResult<TestActualBreakdownDto>(dtos, new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetPagedAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedAsync(query);

            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPagedAsync_EmptyRepository_ReturnsMappedEmptyResult()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData());
            var expected     = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetPagedAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedAsync(query);

            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPagedAsync_RepositoryThrows_PropagatesException()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetPagedAsync(mappedParams).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetPagedAsync(query));
        }

        [Fact]
        public async Task GetPagedAsync_MapsQueryToParametersExactlyOnce()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData());
            var expected     = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetPagedAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            await _sut.GetPagedAsync(query);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
        }

        [Fact]
        public async Task GetPagedAsync_MapsPagedDataToResultExactlyOnce()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData());
            var expected     = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetPagedAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            await _sut.GetPagedAsync(query);

            _mapper.Received(1).Map<PaginatedResult<TestActualBreakdownDto>>(pagedData);
        }

        [Fact]
        public async Task GetPagedAsync_WithMultiplePages_PropagatesPaginationToResult()
        {
            var query        = new QueryParameters<string> { Page = 3, PageSize = 5 };
            var mappedParams = new PaginationParameters<string> { Page = 3, PageSize = 5 };
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData { TotalRecords = 30, PageNumber = 3, PageSize = 5 });
            var expected     = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto { TotalRecords = 30, PageNumber = 3, PageSize = 5 });

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetPagedAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedAsync(query);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Fact]
        public async Task GetPagedAsync_WithSortingParameters_PassesMappedParamsToRepository()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "buyer", Descending = true };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "buyer", Descending = true };
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData());
            var expected     = new PaginatedResult<TestActualBreakdownDto>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _repository.GetPagedAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedAsync(query);

            result.Should().Be(expected);
            await _repository.Received(1).GetPagedAsync(mappedParams);
        }

        #endregion
    }
}

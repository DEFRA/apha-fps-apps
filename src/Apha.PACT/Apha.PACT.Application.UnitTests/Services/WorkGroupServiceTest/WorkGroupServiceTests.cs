using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.WorkGroupServiceTest
{
    public class WorkGroupServiceTests
    {
        private readonly IWorkGroupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly WorkGroupService _sut;

        public WorkGroupServiceTests()
        {
            _mockRepository = Substitute.For<IWorkGroupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new WorkGroupService(_mockRepository, _mockMapper);
        }

        #region GetAllWorkGroupsAsync

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithData_ReturnsMappedDtos()
        {
            var entities = new List<WorkGroup>
            {
                new WorkGroup { WorkGroupName = "WG1", ProfitCentre = "PC1" },
                new WorkGroup { WorkGroupName = "WG2", ProfitCentre = "PC2" }
            };
            var dtos = new List<WorkGroupDto>
            {
                new WorkGroupDto { WorkGroupName = "WG1" },
                new WorkGroupDto { WorkGroupName = "WG2" }
            };

            _mockRepository.GetAllWorkGroupsAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<WorkGroupDto>>(entities).Returns(dtos);

            var result = await _sut.GetAllWorkGroupsAsync();

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetAllWorkGroupsAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_EmptyResult_ReturnsEmptyCollection()
        {
            var entities = new List<WorkGroup>();
            var dtos = new List<WorkGroupDto>();

            _mockRepository.GetAllWorkGroupsAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<WorkGroupDto>>(entities).Returns(dtos);

            var result = await _sut.GetAllWorkGroupsAsync();

            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetAllWorkGroupsAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetAllWorkGroupsAsync().ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetAllWorkGroupsAsync());
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_MapperThrows_PropagatesException()
        {
            var entities = new List<WorkGroup> { new() { WorkGroupName = "WG1" } };
            _mockRepository.GetAllWorkGroupsAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<WorkGroupDto>>(entities)
                       .Throws(new InvalidOperationException("Mapping error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetAllWorkGroupsAsync());
        }

        #endregion

        #region GetWorkGroupTimeCodeAsync

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_WithData_ReturnsMappedPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<WorkGroupTimeCode>(
                [new WorkGroupTimeCode { PACTStaffID = "S1", TimeCode = "TC1" }],
                new PaginationData { TotalRecords = 1 });
            var dto = new WorkGroupTimeCodeDto { PACTStaffID = "S1", TimeCode = "TC1" };
            var expected = new PaginatedResult<WorkGroupTimeCodeDto> { Data = [dto] };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupTimeCodeAsync(mappedParams, "WG1", 3).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData).Returns(expected);

            var result = await _sut.GetWorkGroupTimeCodeAsync(query, "WG1", 3);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetWorkGroupTimeCodeAsync(mappedParams, "WG1", 3);
            _mockMapper.Received(1).Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_ValidWorkGroupAndMonth_PassesToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<WorkGroupTimeCode>([], new PaginationData());
            var expected = new PaginatedResult<WorkGroupTimeCodeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupTimeCodeAsync(mappedParams, "WG1", 1).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData).Returns(expected);

            var result = await _sut.GetWorkGroupTimeCodeAsync(query, "WG1", 1);

            result.Should().Be(expected);
            await _mockRepository.Received(1).GetWorkGroupTimeCodeAsync(mappedParams, "WG1", 1);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_EmptyData_ReturnsMappedEmptyResult()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<WorkGroupTimeCode>([], new PaginationData { TotalRecords = 0 });
            var expected = new PaginatedResult<WorkGroupTimeCodeDto> { Data = [] };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupTimeCodeAsync(mappedParams, "WG2", 2).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData).Returns(expected);

            var result = await _sut.GetWorkGroupTimeCodeAsync(query, "WG2", 2);

            result.Data.Should().BeEmpty();
            await _mockRepository.Received(1).GetWorkGroupTimeCodeAsync(mappedParams, "WG2", 2);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_RepositoryThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupTimeCodeAsync(mappedParams, Arg.Any<string?>(), Arg.Any<int>())
                           .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetWorkGroupTimeCodeAsync(query, "WG1", 1));
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MapsQueryParametersBeforeCallingRepository()
        {
            var query = new QueryParameters<string> { Page = 3, PageSize = 20, SortBy = "Name" };
            var mappedParams = new PaginationParameters<string> { Page = 3, PageSize = 20, SortBy = "Name" };
            var pagedData = new PagedData<WorkGroupTimeCode>([], new PaginationData());
            var expected = new PaginatedResult<WorkGroupTimeCodeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupTimeCodeAsync(mappedParams, "WG3", 6).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData).Returns(expected);

            await _sut.GetWorkGroupTimeCodeAsync(query, "WG3", 6);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetWorkGroupTimeCodeAsync(mappedParams, "WG3", 6);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_NullWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWorkGroupTimeCodeAsync(query, null!, 3));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _mockRepository.DidNotReceive().GetWorkGroupTimeCodeAsync(
                Arg.Any<PaginationParameters<string>>(), Arg.Any<string?>(), Arg.Any<int>());
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_EmptyWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWorkGroupTimeCodeAsync(query, "   ", 3));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_DefaultMonthNumber_PassesDefaultToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<WorkGroupTimeCode>([], new PaginationData());
            var expected = new PaginatedResult<WorkGroupTimeCodeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupTimeCodeAsync(mappedParams, "WG1", 1).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData).Returns(expected);

            var result = await _sut.GetWorkGroupTimeCodeAsync(query, "WG1", 1);

            result.Should().Be(expected);
            await _mockRepository.Received(1).GetWorkGroupTimeCodeAsync(mappedParams, "WG1", 1);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_NullWorkGroupDefaultMonth_ThrowsOnlyWorkGroupError()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWorkGroupTimeCodeAsync(query, null!, 1));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MapperThrowsOnQueryParameters_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mockMapper.Map<PaginationParameters<string>>(query)
                       .Throws(new InvalidOperationException("Mapping error"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetWorkGroupTimeCodeAsync(query, "WG1", 3));

            await _mockRepository.DidNotReceive().GetWorkGroupTimeCodeAsync(
                Arg.Any<PaginationParameters<string>>(), Arg.Any<string?>(), Arg.Any<int>());
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MapperThrowsOnResult_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<WorkGroupTimeCode>([], new PaginationData());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupTimeCodeAsync(mappedParams, "WG1", 3).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData)
                       .Throws(new InvalidOperationException("Result mapping error"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetWorkGroupTimeCodeAsync(query, "WG1", 3));
        }

        #endregion

        #region GetWorkGroupValidTimeCodeAsync

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_WithData_ReturnsMappedPaginatedResult()
        {
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData  = new PagedData<WorkGroupValidTimeCode>(
                [new WorkGroupValidTimeCode { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1" }],
                new PaginationData { TotalRecords = 1 });
            var dto      = new WorkGroupValidTimeCodeDto { WorkGroup = "WG1", TimeCode = "TC1" };
            var expected = new PaginatedResult<WorkGroupValidTimeCodeDto> { Data = [dto] };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupValidTimeCodeAsync(mappedParams, "WG1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupValidTimeCodeDto>>(pagedData).Returns(expected);

            var result = await _sut.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetWorkGroupValidTimeCodeAsync(mappedParams, "WG1");
            _mockMapper.Received(1).Map<PaginatedResult<WorkGroupValidTimeCodeDto>>(pagedData);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_EmptyData_ReturnsMappedEmptyResult()
        {
            var query        = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<WorkGroupValidTimeCode>([], new PaginationData { TotalRecords = 0 });
            var expected     = new PaginatedResult<WorkGroupValidTimeCodeDto> { Data = [] };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupValidTimeCodeAsync(mappedParams, "WG2").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupValidTimeCodeDto>>(pagedData).Returns(expected);

            var result = await _sut.GetWorkGroupValidTimeCodeAsync(query, "WG2");

            result.Data.Should().BeEmpty();
            await _mockRepository.Received(1).GetWorkGroupValidTimeCodeAsync(mappedParams, "WG2");
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_MapsQueryParametersBeforeCallingRepository()
        {
            var query        = new QueryParameters<string> { Page = 3, PageSize = 20, SortBy = "TimeCode" };
            var mappedParams = new PaginationParameters<string> { Page = 3, PageSize = 20, SortBy = "TimeCode" };
            var pagedData    = new PagedData<WorkGroupValidTimeCode>([], new PaginationData());
            var expected     = new PaginatedResult<WorkGroupValidTimeCodeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupValidTimeCodeAsync(mappedParams, "WG3").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupValidTimeCodeDto>>(pagedData).Returns(expected);

            await _sut.GetWorkGroupValidTimeCodeAsync(query, "WG3");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetWorkGroupValidTimeCodeAsync(mappedParams, "WG3");
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_RepositoryThrows_PropagatesException()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupValidTimeCodeAsync(mappedParams, Arg.Any<string>())
                           .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetWorkGroupValidTimeCodeAsync(query, "WG1"));
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_NullWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWorkGroupValidTimeCodeAsync(query, null!));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            Assert.Equal("WorkGroup is required", ex.Errors[0].Message);
            await _mockRepository.DidNotReceive().GetWorkGroupValidTimeCodeAsync(
                Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_EmptyWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWorkGroupValidTimeCodeAsync(query, ""));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _mockRepository.DidNotReceive().GetWorkGroupValidTimeCodeAsync(
                Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_WhitespaceWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWorkGroupValidTimeCodeAsync(query, "   "));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _mockRepository.DidNotReceive().GetWorkGroupValidTimeCodeAsync(
                Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_MapperThrowsOnQueryParameters_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mockMapper.Map<PaginationParameters<string>>(query)
                       .Throws(new InvalidOperationException("Mapping error"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetWorkGroupValidTimeCodeAsync(query, "WG1"));

            await _mockRepository.DidNotReceive().GetWorkGroupValidTimeCodeAsync(
                Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_MapperThrowsOnResult_PropagatesException()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<WorkGroupValidTimeCode>([], new PaginationData());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupValidTimeCodeAsync(mappedParams, "WG1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkGroupValidTimeCodeDto>>(pagedData)
                       .Throws(new InvalidOperationException("Result mapping error"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetWorkGroupValidTimeCodeAsync(query, "WG1"));
        }

        #endregion
    }
}

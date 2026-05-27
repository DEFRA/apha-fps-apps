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
            _mockMapper     = Substitute.For<IMapper>();

            // GetWgSummarisedStaffTimeUsageAsync calls _mapper.Map<IEnumerable<WgSummarisedStaffTimeUsageEntryDto>>
            // after the repository call. Configure a global pass-through so the mock performs the
            // real property-by-property copy instead of returning an empty default.
            _mockMapper
                .Map<IEnumerable<WgSummarisedStaffTimeUsageEntryDto>>(Arg.Any<object>())
                .Returns(callInfo =>
                {
                    var views = (IEnumerable<WgSummarisedStaffTimeUsageView>)callInfo.Arg<object>();
                    return views.Select(v => new WgSummarisedStaffTimeUsageEntryDto
                    {
                        MonthName     = v.MonthName,
                        Name          = v.Name,
                        HrsPaid       = v.HrsPaid,
                        ParentProject = v.ParentProject,
                        JobCode       = v.JobCode,
                        JobTitle      = v.JobTitle,
                        TotalTime     = v.TotalTime,
                        TotalCost     = v.TotalCost
                    });
                });

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

        // ════════════════════════════════════════════════════════════════════════════
        // Helpers shared by GetWgSummarisedStaffTimeUsageAsync tests
        // ════════════════════════════════════════════════════════════════════════════

        /// <summary>Builds a minimal view entry with sensible defaults.</summary>
        private static WgSummarisedStaffTimeUsageView TimeUsageEntry(
            string  workGroup     = "WG1",
            string  name          = "Alice",
            string  monthName     = "April",
            string  parentProject = "PP1",
            string  jobCode       = "JC1",
            string  jobTitle      = "Job Title 1",
            double? hrsPaid       = 120.0,
            double? totalTime     = 10.0,
            double? totalCost     = 500.0) =>
            new()
            {
                WorkGroup     = workGroup,
                Name          = name,
                MonthName     = monthName,
                ParentProject = parentProject,
                JobCode       = jobCode,
                JobTitle      = jobTitle,
                HrsPaid       = hrsPaid,
                TotalTime     = totalTime,
                TotalCost     = totalCost
            };

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10) =>
            new() { Page = page, PageSize = pageSize };

        #region GetWgSummarisedStaffTimeUsageAsync — validation

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), null!));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            Assert.Equal("WorkGroup is required", ex.Errors[0].Message);
            await _mockRepository.DidNotReceive()
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_EmptyWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), ""));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _mockRepository.DidNotReceive()
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WhitespaceWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "   "));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _mockRepository.DidNotReceive()
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<string>());
        }

        #endregion

        #region GetWgSummarisedStaffTimeUsageAsync — repository interaction

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_ValidWorkGroup_CallsRepositoryOnceWithCorrectWorkGroup()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1")
                           .Returns(new List<WgSummarisedStaffTimeUsageView>());

            await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            await _mockRepository.Received(1).GetWgSummarisedStaffTimeUsageAsync("WG1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_RepositoryThrows_PropagatesException()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1")
                           .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1"));
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_EmptyRepositoryResult_ReturnsEmptyRowsAndZeroSummary()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1")
                           .Returns(new List<WgSummarisedStaffTimeUsageView>());

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Rows.Should().BeEmpty();
            result.HrsPaid.Should().Be(0);
            result.Summary.GrandTotalTime.Should().Be(0);
            result.Summary.TotalStandardHours.Should().Be(0);
            result.Summary.GrandTotalPercentAllocated.Should().Be(0);
            result.Pagination.TotalRecords.Should().Be(0);
        }

        #endregion

        #region GetWgSummarisedStaffTimeUsageAsync — HrsPaid calculation

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SinglePerson_HrsPaidEqualsThatPersonsValue()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, monthName: "April"),
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, monthName: "May")  // duplicate Name
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            // Alice appears twice but is counted only once via GroupBy(Name).First()
            result.HrsPaid.Should().Be(120.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_MultipleDistinctPeople_HrsPaidIsSumOfFirstEntryPerPerson()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, monthName: "April"),
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, monthName: "May"),   // deduplicated
                TimeUsageEntry(name: "Bob",   hrsPaid: 60.0,  monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.HrsPaid.Should().Be(180.0);  // 120 (Alice) + 60 (Bob)
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullHrsPaid_TreatedAsZeroInSum()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: null,  monthName: "April"),
                TimeUsageEntry(name: "Bob",   hrsPaid: 60.0,  monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.HrsPaid.Should().Be(60.0);   // null treated as 0
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_AllHrsPaidNull_HrsPaidIsZero()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: null, monthName: "April"),
                TimeUsageEntry(name: "Bob",   hrsPaid: null, monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.HrsPaid.Should().Be(0);
        }

        #endregion

        #region GetWgSummarisedStaffTimeUsageAsync — BuildRows

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SingleEntry_ProducesOneRowWithCorrectFields()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: "Analyst",
                               monthName: "April", totalTime: 10.0, totalCost: 500.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            var row = result.Rows.Single();
            row.ParentProject.Should().Be("PP1");
            row.JobCode.Should().Be("JC1");
            row.JobTitle.Should().Be("Analyst");
            row.April.Should().Be(10.0);
            row.May.Should().Be(0.0);
            row.TotalTime.Should().Be(10.0);
            row.TotalCost.Should().Be(500.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SameGroupMultipleMonths_PivotsHoursIntoCorrectColumns()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April",    totalTime: 10.0),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "May",      totalTime: 20.0),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "December", totalTime: 5.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            var row = result.Rows.Single();
            row.April.Should().Be(10.0);
            row.May.Should().Be(20.0);
            row.June.Should().Be(0.0);
            row.December.Should().Be(5.0);
            row.TotalTime.Should().Be(35.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_MultipleGroups_ProducesOneRowPerGroup()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April", totalTime: 10.0),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC2", monthName: "April", totalTime: 5.0),
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC1", monthName: "April", totalTime: 8.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Rows.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Rows_OrderedByParentProjectThenJobCode()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC2", monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            var rows = result.Rows.ToList();
            rows[0].ParentProject.Should().Be("PP1");
            rows[0].JobCode.Should().Be("JC1");
            rows[1].ParentProject.Should().Be("PP1");
            rows[1].JobCode.Should().Be("JC2");
            rows[2].ParentProject.Should().Be("PP2");
            rows[2].JobCode.Should().Be("JC1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullTotalTime_TreatedAsZeroInRow()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April",
                               totalTime: null, totalCost: null)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            var row = result.Rows.Single();
            row.April.Should().Be(0.0);
            row.TotalTime.Should().Be(0.0);
            row.TotalCost.Should().Be(0.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_JobTitleFromFirstEntryInGroup()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: "First Title",  monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: "Second Title", monthName: "May")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            // JobTitle taken from .First() of the group
            result.Rows.Single().JobTitle.Should().Be("First Title");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_JobTitle_NullValue_ShowsNoDescriptionAvailable()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: null!, monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Rows.Single().JobTitle.Should().Be("No description available");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_JobTitle_EmptyString_ShowsNoDescriptionAvailable()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: "", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Rows.Single().JobTitle.Should().Be("No description available");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_JobTitle_WhitespaceOnly_ShowsNoDescriptionAvailable()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: "   ", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Rows.Single().JobTitle.Should().Be("No description available");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_AllTwelveMonthsPivotedCorrectly()
        {
            var months = new[]
            {
                ("April", 1.0), ("May", 2.0), ("June", 3.0), ("July", 4.0),
                ("August", 5.0), ("September", 6.0), ("October", 7.0), ("November", 8.0),
                ("December", 9.0), ("January", 10.0), ("February", 11.0), ("March", 12.0)
            };
            var entries = months.Select(m =>
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1",
                               monthName: m.Item1, totalTime: m.Item2)).ToList();

            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(entries);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            var row = result.Rows.Single();
            row.April.Should().Be(1.0);
            row.May.Should().Be(2.0);
            row.June.Should().Be(3.0);
            row.July.Should().Be(4.0);
            row.August.Should().Be(5.0);
            row.September.Should().Be(6.0);
            row.October.Should().Be(7.0);
            row.November.Should().Be(8.0);
            row.December.Should().Be(9.0);
            row.January.Should().Be(10.0);
            row.February.Should().Be(11.0);
            row.March.Should().Be(12.0);
            row.TotalTime.Should().Be(78.0);
        }

        #endregion

        #region GetWgSummarisedStaffTimeUsageAsync — JobTitleLookup

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SingleJobCode_LookupContainsOneItem()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(jobCode: "JC1", jobTitle: "Analyst", monthName: "April"),
                TimeUsageEntry(jobCode: "JC1", jobTitle: "Analyst", monthName: "May")   // same code, different month
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SingleJobCode_LookupItemHasCorrectJobCodeAndTitle()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(jobCode: "JC1", jobTitle: "Analyst", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            var item = result.JobTitleLookup.Single();
            item.JobCode.Should().Be("JC1");
            item.JobTitle.Should().Be("Analyst");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_MultipleDistinctJobCodes_LookupContainsOneItemPerCode()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: "Analyst",   monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC2", jobTitle: "Developer", monthName: "April"),
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC3", jobTitle: "Tester",    monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Should().HaveCount(3);
            result.JobTitleLookup.Should().Contain(x => x.JobCode == "JC1" && x.JobTitle == "Analyst");
            result.JobTitleLookup.Should().Contain(x => x.JobCode == "JC2" && x.JobTitle == "Developer");
            result.JobTitleLookup.Should().Contain(x => x.JobCode == "JC3" && x.JobTitle == "Tester");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_DuplicateJobCodes_LookupDeduplicatesByJobCode()
        {
            // Same JobCode appears in multiple rows (different ParentProject or month)
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: "Analyst", monthName: "April"),
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC1", jobTitle: "Analyst", monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: "Analyst", monthName: "May")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Should().HaveCount(1);
            result.JobTitleLookup.Single().JobCode.Should().Be("JC1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullJobCode_ExcludedFromLookup()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(jobCode: "JC1",  jobTitle: "Analyst", monthName: "April"),
                TimeUsageEntry(jobCode: null!,   jobTitle: "Unknown", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Should().HaveCount(1);
            result.JobTitleLookup.Should().NotContain(x => x.JobCode == null);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WhitespaceJobCode_ExcludedFromLookup()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(jobCode: "JC1", jobTitle: "Analyst", monthName: "April"),
                TimeUsageEntry(jobCode: "   ", jobTitle: "Unknown", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Should().HaveCount(1);
            result.JobTitleLookup.Single().JobCode.Should().Be("JC1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullJobTitle_LookupItemJobTitleIsNoDescriptionAvailable()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(jobCode: "JC1", jobTitle: null!, monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Single().JobTitle.Should().Be("No description available");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_EmptyJobTitle_LookupItemJobTitleIsNoDescriptionAvailable()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(jobCode: "JC1", jobTitle: "", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Single().JobTitle.Should().Be("No description available");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WhitespaceJobTitle_LookupItemJobTitleIsNoDescriptionAvailable()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(jobCode: "JC1", jobTitle: "   ", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Single().JobTitle.Should().Be("No description available");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_EmptyData_LookupIsEmpty()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1")
                           .Returns(new List<WgSummarisedStaffTimeUsageView>());

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Should().BeEmpty();
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_LookupBuiltFromAllRows_NotJustCurrentPage()
        {
            // Seed 15 distinct job codes with a page size of 10 — page 1 only contains 10,
            // but the lookup should reflect all 15 (built pre-pagination).
            var entries = Enumerable.Range(1, 15)
                .Select(i => TimeUsageEntry(
                    jobCode:  $"JC{i:D2}",
                    jobTitle: $"Title {i}",
                    monthName: "April"))
                .ToList();

            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(entries);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10 }, "WG1");

            // Paged rows = 10, but lookup must have all 15
            result.Rows.Should().HaveCount(10);
            result.JobTitleLookup.Should().HaveCount(15);
        }

        #endregion

        #region GetWgSummarisedStaffTimeUsageAsync — BuildSummary

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Summary_TotalsAreCorrect()
        {
            // Two rows contributing to the same month
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April",
                               totalTime: 10.0, totalCost: 200.0),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC2", monthName: "April",
                               totalTime: 5.0,  totalCost: 100.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Summary.TotalApril.Should().Be(15.0);    // 10 + 5
            result.Summary.GrandTotalTime.Should().Be(15.0);
            result.Summary.GrandTotalCost.Should().Be(300.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Summary_StandardHoursPerMonthCorrect()
        {
            // hrsPaid = 120 → standardHoursPerMonth = 120/12 = 10
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, monthName: "April", totalTime: 8.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Summary.StandardHoursPerMonth.Should().Be(10.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Summary_StandardHoursForMonthZeroWhenNoData()
        {
            // Only April has data; May has no data → StandardHoursFor(May) = 0
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, monthName: "April", totalTime: 10.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            // TotalStandardHours = only April contributes = 10
            result.Summary.TotalStandardHours.Should().Be(10.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Summary_TotalStandardHoursSumsOnlyActiveMonths()
        {
            // April and May have data; all other months are empty
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, parentProject: "PP1", jobCode: "JC1",
                               monthName: "April", totalTime: 10.0),
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, parentProject: "PP1", jobCode: "JC1",
                               monthName: "May",   totalTime: 5.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            // standardHoursPerMonth = 10; two active months → TotalStandardHours = 20
            result.Summary.TotalStandardHours.Should().Be(20.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Summary_PercentAllocatedForMonthWithData()
        {
            // standardHoursPerMonth = 10; April = 8 → 8/10*100 = 80.0
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, monthName: "April", totalTime: 8.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Summary.PercentAllocatedApril.Should().Be(80.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Summary_PercentAllocatedForEmptyMonthIsZero()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, monthName: "April", totalTime: 10.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            // May has no data → StandardHoursFor(0) = 0 → PercentAllocated(0, 0) = 0
            result.Summary.PercentAllocatedMay.Should().Be(0.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Summary_GrandTotalPercentAllocatedCorrect()
        {
            // April=10, May=5; standardHoursPerMonth=10; TotalStandardHours=20
            // GrandTotalPercentAllocated = 15/20*100 = 75.0
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, parentProject: "PP1", jobCode: "JC1",
                               monthName: "April", totalTime: 10.0),
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, parentProject: "PP1", jobCode: "JC1",
                               monthName: "May",   totalTime: 5.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Summary.GrandTotalPercentAllocated.Should().Be(75.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Summary_GrandTotalPercentAllocatedIsZeroWhenNoStandardHours()
        {
            // hrsPaid = 0 → standardHoursPerMonth = 0 → TotalStandardHours = 0 → percent = 0
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: 0.0, monthName: "April", totalTime: 10.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Summary.GrandTotalPercentAllocated.Should().Be(0.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Summary_AllPercentAllocatedZeroWhenHrsPaidIsZero()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: null, monthName: "April", totalTime: 10.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Summary.PercentAllocatedApril.Should().Be(0.0);
            result.Summary.PercentAllocatedMay.Should().Be(0.0);
            result.Summary.GrandTotalPercentAllocated.Should().Be(0.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Summary_SummaryComputedFromAllRowsNotJustPage()
        {
            // 15 rows; page 1 has only 10 — but summary totals must cover all 15
            const int totalRows  = 15;
            const double hoursPerEntry = 4.0;
            var entries = Enumerable.Range(1, totalRows)
                .Select(i => TimeUsageEntry(
                    name: "Alice",
                    hrsPaid: 120.0,
                    parentProject: $"PP{i}",
                    jobCode: $"JC{i}",
                    monthName: "April",
                    totalTime: hoursPerEntry))
                .ToList();

            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(entries);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10 }, "WG1");

            // Only 10 rows on page 1 but summary covers all 15 rows
            result.Rows.Should().HaveCount(10);
            result.Summary.TotalApril.Should().Be(totalRows * hoursPerEntry);    // 60.0
            result.Summary.GrandTotalTime.Should().Be(totalRows * hoursPerEntry);
        }

        #endregion

        #region GetWgSummarisedStaffTimeUsageAsync — pagination

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Pagination_TotalRecordsEqualsTotalRows()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP3", jobCode: "JC1", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Pagination.TotalRecords.Should().Be(3);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Pagination_TotalPagesIsCeiling()
        {
            var entries = Enumerable.Range(1, 15)
                .Select(i => TimeUsageEntry(parentProject: $"PP{i}", jobCode: "JC1", monthName: "April"))
                .ToList();
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(entries);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10 }, "WG1");

            result.Pagination.TotalPages.Should().Be(2);   // ceil(15/10)
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Pagination_FirstPageReturnsCorrectSlice()
        {
            var entries = Enumerable.Range(1, 15)
                .Select(i => TimeUsageEntry(parentProject: $"PP{i:D2}", jobCode: "JC1", monthName: "April"))
                .ToList();
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(entries);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10 }, "WG1");

            result.Rows.Should().HaveCount(10);
            result.Rows.First().ParentProject.Should().Be("PP01");
            result.Rows.Last().ParentProject.Should().Be("PP10");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Pagination_SecondPageReturnsRemainder()
        {
            var entries = Enumerable.Range(1, 15)
                .Select(i => TimeUsageEntry(parentProject: $"PP{i:D2}", jobCode: "JC1", monthName: "April"))
                .ToList();
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(entries);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 2, PageSize = 10 }, "WG1");

            result.Rows.Should().HaveCount(5);
            result.Rows.First().ParentProject.Should().Be("PP11");
            result.Rows.Last().ParentProject.Should().Be("PP15");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Pagination_PageNumberAndPageSizeReturnedInResult()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 2, PageSize = 5 }, "WG1");

            result.Pagination.PageNumber.Should().Be(2);
            result.Pagination.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Pagination_PageLessThanOneClampedToOne()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 0, PageSize = 10 }, "WG1");

            result.Pagination.PageNumber.Should().Be(1);
            result.Rows.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Pagination_PageSizeLessThanOneClampedToOne()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC1", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 0 }, "WG1");

            result.Pagination.PageSize.Should().Be(1);
            result.Rows.Should().HaveCount(1);   // pageSize clamped to 1
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Pagination_NegativePageClampedToOne()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = -5, PageSize = 10 }, "WG1");

            result.Pagination.PageNumber.Should().Be(1);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Pagination_EmptyData_TotalPagesIsZero()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1")
                           .Returns(new List<WgSummarisedStaffTimeUsageView>());

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Pagination.TotalRecords.Should().Be(0);
            result.Pagination.TotalPages.Should().Be(0);
        }

        #endregion

        #region GetWgSummarisedStaffTimeUsageAsync — HrsPaid on returned Dto

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_ReturnedDto_ContainsComputedHrsPaid()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(name: "Alice", hrsPaid: 120.0, monthName: "April"),
                TimeUsageEntry(name: "Bob",   hrsPaid: 60.0,  monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.HrsPaid.Should().Be(180.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_MapperCalledOnceForViewToEntryDtoConversion()
        {
            // The service calls the mapper exactly once: to convert IEnumerable<WgSummarisedStaffTimeUsageView>
            // → IEnumerable<WgSummarisedStaffTimeUsageEntryDto>. No other mapper calls are made.
            var entries = new List<WgSummarisedStaffTimeUsageView>
            {
                TimeUsageEntry(monthName: "April")
            };
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(entries);

            await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            _mockMapper.Received(1).Map<IEnumerable<WgSummarisedStaffTimeUsageEntryDto>>(entries);
        }

        #endregion
    }
}

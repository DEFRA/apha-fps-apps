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
            Assert.Equal("STAFFNane_REQUIRED", ex.Errors[0].Code);
            Assert.Equal("Staff Name is required", ex.Errors[0].Message);
            await _mockRepository.DidNotReceive()
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_EmptyWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), ""));

            Assert.Single(ex.Errors);
            Assert.Equal("STAFFNane_REQUIRED", ex.Errors[0].Code);
            await _mockRepository.DidNotReceive()
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WhitespaceWorkGroup_ThrowsBusinessValidationErrorException()
        {
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "   "));

            Assert.Single(ex.Errors);
            Assert.Equal("STAFFNane_REQUIRED", ex.Errors[0].Code);
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

            result.Rows.Single().JobTitle.Should().BeEmpty();
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_JobTitle_EmptyString_ShowsNoDescriptionAvailable()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: "", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Rows.Single().JobTitle.Should().BeEmpty();
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_JobTitle_WhitespaceOnly_ShowsNoDescriptionAvailable()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", jobTitle: "   ", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.Rows.Single().JobTitle.Should().BeEmpty();
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

            result.JobTitleLookup.Single().JobTitle.Should().BeEmpty();
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_EmptyJobTitle_LookupItemJobTitleIsNoDescriptionAvailable()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(jobCode: "JC1", jobTitle: "", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Single().JobTitle.Should().BeEmpty();
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WhitespaceJobTitle_LookupItemJobTitleIsNoDescriptionAvailable()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(jobCode: "JC1", jobTitle: "   ", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(DefaultQuery(), "WG1");

            result.JobTitleLookup.Single().JobTitle.Should().BeEmpty();
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

        // ════════════════════════════════════════════════════════════════════════════
        // Helpers shared by GetSummarisedWorkgroupTimeSummaryAsync tests
        // ════════════════════════════════════════════════════════════════════════════

        /// <summary>Builds a minimal SummarisedWgTimeView entry with sensible defaults.</summary>
        private static SummarisedWgTimeView WgTimeEntry(
            string  workGroup     = "WG1",
            string  parentProject = "PP1",
            string  projectTitle  = "Project Title 1",
            string  monthName     = "April",
            double? totalTime     = 10.0,
            double? totalCost     = 500.0) =>
            new()
            {
                WorkGroup     = workGroup,
                ParentProject = parentProject,
                ProjectTitle  = projectTitle,
                MonthName     = monthName,
                TotalTime     = totalTime,
                TotalCost     = totalCost
            };

        private static QueryParameters<string> DefaultWgQuery(int page = 1, int pageSize = 10) =>
            new() { Page = page, PageSize = pageSize };

        // ════════════════════════════════════════════════════════════════════════════
        // Setup helper: configure the mapper to pass-through SummarisedWgTimeView
        // ════════════════════════════════════════════════════════════════════════════

        private void SetupWgTimeEntryMapper()
        {
            _mockMapper
                .Map<IEnumerable<SummarisedWgTimeEntryDto>>(Arg.Any<object>())
                .Returns(callInfo =>
                {
                    var views = (IEnumerable<SummarisedWgTimeView>)callInfo.Arg<object>();
                    return views.Select(v => new SummarisedWgTimeEntryDto
                    {
                        MonthName     = v.MonthName,
                        ParentProject = v.ParentProject,
                        ProjectTitle  = v.ProjectTitle,
                        TotalTime     = v.TotalTime,
                        TotalCost     = v.TotalCost
                    });
                });
        }

        #region GetSummarisedWorkgroupTimeSummaryAsync — validation

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_NullWorkGroup_ThrowsBusinessValidationErrorException()
        {
            SetupWgTimeEntryMapper();

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), null!));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            Assert.Equal("WorkGroup is required", ex.Errors[0].Message);
            await _mockRepository.DidNotReceive()
                .GetSummarisedWorkgroupTimeAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_EmptyWorkGroup_ThrowsBusinessValidationErrorException()
        {
            SetupWgTimeEntryMapper();

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), ""));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _mockRepository.DidNotReceive()
                .GetSummarisedWorkgroupTimeAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WhitespaceWorkGroup_ThrowsBusinessValidationErrorException()
        {
            SetupWgTimeEntryMapper();

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "   "));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _mockRepository.DidNotReceive()
                .GetSummarisedWorkgroupTimeAsync(Arg.Any<string>());
        }

        #endregion

        #region GetSummarisedWorkgroupTimeSummaryAsync — repository interaction

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_ValidWorkGroup_CallsRepositoryOnceWithCorrectWorkGroup()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1")
                           .Returns(new List<SummarisedWgTimeView>());

            await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            await _mockRepository.Received(1).GetSummarisedWorkgroupTimeAsync("WG1");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_RepositoryThrows_PropagatesException()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1")
                           .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1"));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_EmptyRepositoryResult_ReturnsEmptyRowsAndZeroSummary()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1")
                           .Returns(new List<SummarisedWgTimeView>());

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.Rows.Should().BeEmpty();
            result.Summary.GrandTotalTime.Should().Be(0);
            result.Summary.GrandTotalCost.Should().Be(0);
            result.Pagination.TotalRecords.Should().Be(0);
            result.ProjectTitleLookup.Should().BeEmpty();
        }

        #endregion

        #region GetSummarisedWorkgroupTimeSummaryAsync — BuildWgSummarisedTimeRows

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_SingleEntry_ProducesOneRowWithCorrectFields()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April", totalTime: 10.0, totalCost: 500.0)
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            var row = result.Rows.Single();
            row.ParentProject.Should().Be("PP1");
            row.April.Should().Be(10.0);
            row.May.Should().Be(0.0);
            row.TotalTime.Should().Be(10.0);
            row.TotalCost.Should().Be(500.0);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_SameProjectMultipleMonths_PivotsHoursIntoCorrectColumns()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April",    totalTime: 10.0),
                WgTimeEntry(parentProject: "PP1", monthName: "May",      totalTime: 20.0),
                WgTimeEntry(parentProject: "PP1", monthName: "December", totalTime: 5.0)
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            var row = result.Rows.Single();
            row.April.Should().Be(10.0);
            row.May.Should().Be(20.0);
            row.June.Should().Be(0.0);
            row.December.Should().Be(5.0);
            row.TotalTime.Should().Be(35.0);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_MultipleProjects_ProducesOneRowPerProject()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April", totalTime: 10.0),
                WgTimeEntry(parentProject: "PP2", monthName: "April", totalTime: 5.0),
                WgTimeEntry(parentProject: "PP3", monthName: "April", totalTime: 8.0)
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.Rows.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Rows_OrderedByParentProject()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP3", monthName: "April"),
                WgTimeEntry(parentProject: "PP1", monthName: "April"),
                WgTimeEntry(parentProject: "PP2", monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            var rows = result.Rows.ToList();
            rows[0].ParentProject.Should().Be("PP1");
            rows[1].ParentProject.Should().Be("PP2");
            rows[2].ParentProject.Should().Be("PP3");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_NullTotalTime_TreatedAsZeroInRow()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April", totalTime: null, totalCost: null)
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            var row = result.Rows.Single();
            row.April.Should().Be(0.0);
            row.TotalTime.Should().Be(0.0);
            row.TotalCost.Should().Be(0.0);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_AllTwelveMonthsPivotedCorrectly()
        {
            SetupWgTimeEntryMapper();
            var months = new[]
            {
                ("April", 1.0), ("May", 2.0), ("June", 3.0), ("July", 4.0),
                ("August", 5.0), ("September", 6.0), ("October", 7.0), ("November", 8.0),
                ("December", 9.0), ("January", 10.0), ("February", 11.0), ("March", 12.0)
            };
            var entries = months.Select(m =>
                WgTimeEntry(parentProject: "PP1", monthName: m.Item1, totalTime: m.Item2)).ToList();

            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(entries);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

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

        #region GetSummarisedWorkgroupTimeSummaryAsync — BuildWgSummarisedTimeSummary

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Summary_MonthlyTotalsAreCorrect()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April", totalTime: 10.0, totalCost: 200.0),
                WgTimeEntry(parentProject: "PP2", monthName: "April", totalTime:  5.0, totalCost: 100.0)
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.Summary.TotalApril.Should().Be(15.0);
            result.Summary.GrandTotalTime.Should().Be(15.0);
            result.Summary.GrandTotalCost.Should().Be(300.0);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Summary_AllMonthsTotalled()
        {
            SetupWgTimeEntryMapper();
            var months = new[]
            {
                ("April", 1.0), ("May", 2.0), ("June", 3.0), ("July", 4.0),
                ("August", 5.0), ("September", 6.0), ("October", 7.0), ("November", 8.0),
                ("December", 9.0), ("January", 10.0), ("February", 11.0), ("March", 12.0)
            };
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
                months.Select(m => WgTimeEntry(parentProject: "PP1", monthName: m.Item1, totalTime: m.Item2)).ToList());

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.Summary.TotalApril.Should().Be(1.0);
            result.Summary.TotalMay.Should().Be(2.0);
            result.Summary.TotalJune.Should().Be(3.0);
            result.Summary.TotalJuly.Should().Be(4.0);
            result.Summary.TotalAugust.Should().Be(5.0);
            result.Summary.TotalSeptember.Should().Be(6.0);
            result.Summary.TotalOctober.Should().Be(7.0);
            result.Summary.TotalNovember.Should().Be(8.0);
            result.Summary.TotalDecember.Should().Be(9.0);
            result.Summary.TotalJanuary.Should().Be(10.0);
            result.Summary.TotalFebruary.Should().Be(11.0);
            result.Summary.TotalMarch.Should().Be(12.0);
            result.Summary.GrandTotalTime.Should().Be(78.0);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Summary_ComputedFromAllRowsNotJustCurrentPage()
        {
            SetupWgTimeEntryMapper();
            // 15 distinct projects, each with 4 hours in April; page 1 = 10 rows
            var entries = Enumerable.Range(1, 15)
                .Select(i => WgTimeEntry(parentProject: $"PP{i}", monthName: "April", totalTime: 4.0))
                .ToList();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(entries);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10 }, "WG1");

            result.Rows.Should().HaveCount(10);
            result.Summary.TotalApril.Should().Be(60.0);    // 15 * 4
            result.Summary.GrandTotalTime.Should().Be(60.0);
        }

        #endregion

        #region GetSummarisedWorkgroupTimeSummaryAsync — ProjectTitleLookup

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_SingleProject_LookupContainsOneItem()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", projectTitle: "Alpha", monthName: "April"),
                WgTimeEntry(parentProject: "PP1", projectTitle: "Alpha", monthName: "May")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.ProjectTitleLookup.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_MultipleProjects_LookupContainsOneItemPerProject()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", projectTitle: "Alpha",   monthName: "April"),
                WgTimeEntry(parentProject: "PP2", projectTitle: "Beta",    monthName: "April"),
                WgTimeEntry(parentProject: "PP3", projectTitle: "Gamma",   monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.ProjectTitleLookup.Should().HaveCount(3);
            result.ProjectTitleLookup.Should().Contain(x => x.ParentProject == "PP1" && x.ProjectTitle == "Alpha");
            result.ProjectTitleLookup.Should().Contain(x => x.ParentProject == "PP2" && x.ProjectTitle == "Beta");
            result.ProjectTitleLookup.Should().Contain(x => x.ParentProject == "PP3" && x.ProjectTitle == "Gamma");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_NullParentProject_ExcludedFromLookup()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1",  projectTitle: "Alpha", monthName: "April"),
                WgTimeEntry(parentProject: null!,  projectTitle: "None",  monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.ProjectTitleLookup.Should().HaveCount(1);
            result.ProjectTitleLookup.Should().NotContain(x => x.ParentProject == null);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WhitespaceParentProject_ExcludedFromLookup()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", projectTitle: "Alpha", monthName: "April"),
                WgTimeEntry(parentProject: "   ", projectTitle: "None",  monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.ProjectTitleLookup.Should().HaveCount(1);
            result.ProjectTitleLookup.Single().ParentProject.Should().Be("PP1");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_NullProjectTitle_LookupItemProjectTitleIsEmpty()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", projectTitle: null!, monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.ProjectTitleLookup.Single().ProjectTitle.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_WhitespaceProjectTitle_LookupItemProjectTitleIsEmpty()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", projectTitle: "   ", monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.ProjectTitleLookup.Single().ProjectTitle.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_LookupBuiltFromAllEntries_NotJustCurrentPage()
        {
            SetupWgTimeEntryMapper();
            var entries = Enumerable.Range(1, 15)
                .Select(i => WgTimeEntry(parentProject: $"PP{i:D2}", projectTitle: $"Title {i}", monthName: "April"))
                .ToList();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(entries);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10 }, "WG1");

            result.Rows.Should().HaveCount(10);
            result.ProjectTitleLookup.Should().HaveCount(15);
        }

        #endregion

        #region GetSummarisedWorkgroupTimeSummaryAsync — pagination

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Pagination_TotalRecordsEqualsTotalProjects()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April"),
                WgTimeEntry(parentProject: "PP2", monthName: "April"),
                WgTimeEntry(parentProject: "PP3", monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.Pagination.TotalRecords.Should().Be(3);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Pagination_TotalPagesIsCeiling()
        {
            SetupWgTimeEntryMapper();
            var entries = Enumerable.Range(1, 15)
                .Select(i => WgTimeEntry(parentProject: $"PP{i}", monthName: "April"))
                .ToList();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(entries);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10 }, "WG1");

            result.Pagination.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Pagination_FirstPageReturnsCorrectSlice()
        {
            SetupWgTimeEntryMapper();
            var entries = Enumerable.Range(1, 15)
                .Select(i => WgTimeEntry(parentProject: $"PP{i:D2}", monthName: "April"))
                .ToList();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(entries);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10 }, "WG1");

            result.Rows.Should().HaveCount(10);
            result.Rows.First().ParentProject.Should().Be("PP01");
            result.Rows.Last().ParentProject.Should().Be("PP10");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Pagination_SecondPageReturnsRemainder()
        {
            SetupWgTimeEntryMapper();
            var entries = Enumerable.Range(1, 15)
                .Select(i => WgTimeEntry(parentProject: $"PP{i:D2}", monthName: "April"))
                .ToList();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(entries);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 2, PageSize = 10 }, "WG1");

            result.Rows.Should().HaveCount(5);
            result.Rows.First().ParentProject.Should().Be("PP11");
            result.Rows.Last().ParentProject.Should().Be("PP15");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Pagination_PageNumberAndPageSizeReturnedInResult()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 2, PageSize = 5 }, "WG1");

            result.Pagination.PageNumber.Should().Be(2);
            result.Pagination.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Pagination_PageLessThanOneClampedToOne()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 0, PageSize = 10 }, "WG1");

            result.Pagination.PageNumber.Should().Be(1);
            result.Rows.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Pagination_PageSizeLessThanOneClampedToOne()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April"),
                WgTimeEntry(parentProject: "PP2", monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 0 }, "WG1");

            result.Pagination.PageSize.Should().Be(1);
            result.Rows.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Pagination_EmptyData_TotalPagesIsZero()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1")
                           .Returns(new List<SummarisedWgTimeView>());

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(DefaultWgQuery(), "WG1");

            result.Pagination.TotalRecords.Should().Be(0);
            result.Pagination.TotalPages.Should().Be(0);
        }

        #endregion

        #region GetSummarisedWorkgroupTimeSummaryAsync — ApplySortToWgSummarisedTimeRows

        [Theory]
        [InlineData("ParentProject", false)]
        [InlineData("ParentProject", true)]
        [InlineData("April",         false)]
        [InlineData("April",         true)]
        [InlineData("May",           false)]
        [InlineData("June",          false)]
        [InlineData("July",          false)]
        [InlineData("August",        false)]
        [InlineData("September",     false)]
        [InlineData("October",       false)]
        [InlineData("November",      false)]
        [InlineData("December",      false)]
        [InlineData("January",       false)]
        [InlineData("February",      false)]
        [InlineData("March",         false)]
        [InlineData("TotalTime",     false)]
        [InlineData("TotalTime",     true)]
        [InlineData("TotalCost",     false)]
        [InlineData("TotalCost",     true)]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Sort_KnownColumn_DoesNotThrow(string sortBy, bool descending)
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP2", monthName: "April", totalTime: 5.0,  totalCost: 100.0),
                WgTimeEntry(parentProject: "PP1", monthName: "April", totalTime: 10.0, totalCost: 200.0)
            ]);

            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var act = () => _sut.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1");

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Sort_ByParentProjectAscending_OrdersRowsAZ()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP3", monthName: "April"),
                WgTimeEntry(parentProject: "PP1", monthName: "April"),
                WgTimeEntry(parentProject: "PP2", monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "ParentProject", Descending = false }, "WG1");

            var rows = result.Rows.ToList();
            rows[0].ParentProject.Should().Be("PP1");
            rows[1].ParentProject.Should().Be("PP2");
            rows[2].ParentProject.Should().Be("PP3");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Sort_ByParentProjectDescending_OrdersRowsZA()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April"),
                WgTimeEntry(parentProject: "PP3", monthName: "April"),
                WgTimeEntry(parentProject: "PP2", monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "ParentProject", Descending = true }, "WG1");

            var rows = result.Rows.ToList();
            rows[0].ParentProject.Should().Be("PP3");
            rows[1].ParentProject.Should().Be("PP2");
            rows[2].ParentProject.Should().Be("PP1");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Sort_ByTotalTimeAscending_OrdersRowsLowToHigh()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April", totalTime: 30.0),
                WgTimeEntry(parentProject: "PP2", monthName: "April", totalTime: 10.0),
                WgTimeEntry(parentProject: "PP3", monthName: "April", totalTime: 20.0)
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "TotalTime", Descending = false }, "WG1");

            var rows = result.Rows.ToList();
            rows[0].TotalTime.Should().Be(10.0);
            rows[1].TotalTime.Should().Be(20.0);
            rows[2].TotalTime.Should().Be(30.0);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Sort_ByTotalCostDescending_OrdersRowsHighToLow()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP1", monthName: "April", totalCost: 100.0),
                WgTimeEntry(parentProject: "PP2", monthName: "April", totalCost: 300.0),
                WgTimeEntry(parentProject: "PP3", monthName: "April", totalCost: 200.0)
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "TotalCost", Descending = true }, "WG1");

            var rows = result.Rows.ToList();
            rows[0].TotalCost.Should().Be(300.0);
            rows[1].TotalCost.Should().Be(200.0);
            rows[2].TotalCost.Should().Be(100.0);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Sort_NullSortBy_ReturnDefaultOrder()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP3", monthName: "April"),
                WgTimeEntry(parentProject: "PP1", monthName: "April"),
                WgTimeEntry(parentProject: "PP2", monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = null }, "WG1");

            // With no sort, BuildWgSummarisedTimeRows applies default OrderBy(ParentProject)
            result.Rows.First().ParentProject.Should().Be("PP1");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Sort_UnknownColumn_FallsBackToParentProjectOrder()
        {
            SetupWgTimeEntryMapper();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(
            [
                WgTimeEntry(parentProject: "PP3", monthName: "April"),
                WgTimeEntry(parentProject: "PP1", monthName: "April"),
                WgTimeEntry(parentProject: "PP2", monthName: "April")
            ]);

            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "UnknownColumn", Descending = false }, "WG1");

            // Unknown column falls back to ParentProject in the switch _ case
            result.Rows.First().ParentProject.Should().Be("PP1");
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeSummaryAsync_Sort_AppliedBeforePaging_SortedSliceReturned()
        {
            SetupWgTimeEntryMapper();
            var entries = Enumerable.Range(1, 15)
                .Select(i => WgTimeEntry(parentProject: $"PP{i:D2}", monthName: "April", totalTime: (double)(16 - i)))
                .ToList();
            _mockRepository.GetSummarisedWorkgroupTimeAsync("WG1").Returns(entries);

            // Sort by TotalTime descending: PP01 has highest time (15), PP15 has lowest (1)
            var result = await _sut.GetSummarisedWorkgroupTimeSummaryAsync(
                new QueryParameters<string> { Page = 1, PageSize = 5, SortBy = "TotalTime", Descending = true }, "WG1");

            result.Rows.Should().HaveCount(5);
            result.Rows.First().TotalTime.Should().Be(15.0);
            result.Rows.Last().TotalTime.Should().Be(11.0);
        }

        #endregion

        #region GetWgSummarisedStaffTimeUsageAsync — ApplySortToWgStaffTimeRows

        [Theory]
        [InlineData("ParentProject", false)]
        [InlineData("ParentProject", true)]
        [InlineData("JobCode",       false)]
        [InlineData("JobCode",       true)]
        [InlineData("April",         false)]
        [InlineData("April",         true)]
        [InlineData("May",           false)]
        [InlineData("June",          false)]
        [InlineData("July",          false)]
        [InlineData("August",        false)]
        [InlineData("September",     false)]
        [InlineData("October",       false)]
        [InlineData("November",      false)]
        [InlineData("December",      false)]
        [InlineData("January",       false)]
        [InlineData("February",      false)]
        [InlineData("March",         false)]
        [InlineData("TotalTime",     false)]
        [InlineData("TotalTime",     true)]
        [InlineData("TotalCost",     false)]
        [InlineData("TotalCost",     true)]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Sort_KnownColumn_DoesNotThrow(string sortBy, bool descending)
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC2", monthName: "April", totalTime: 5.0,  totalCost: 100.0),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April", totalTime: 10.0, totalCost: 200.0)
            ]);

            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var act = () => _sut.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Sort_ByParentProjectAscending_OrdersRowsAZ()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP3", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC1", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "ParentProject", Descending = false }, "WG1");

            var rows = result.Rows.ToList();
            rows[0].ParentProject.Should().Be("PP1");
            rows[1].ParentProject.Should().Be("PP2");
            rows[2].ParentProject.Should().Be("PP3");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Sort_ByParentProjectDescending_OrdersRowsZA()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP3", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC1", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "ParentProject", Descending = true }, "WG1");

            var rows = result.Rows.ToList();
            rows[0].ParentProject.Should().Be("PP3");
            rows[1].ParentProject.Should().Be("PP2");
            rows[2].ParentProject.Should().Be("PP1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Sort_ByJobCodeAscending_OrdersRowsAZ()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC3", monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC2", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "JobCode", Descending = false }, "WG1");

            var rows = result.Rows.ToList();
            rows[0].JobCode.Should().Be("JC1");
            rows[1].JobCode.Should().Be("JC2");
            rows[2].JobCode.Should().Be("JC3");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Sort_ByJobCodeDescending_OrdersRowsZA()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC3", monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC2", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "JobCode", Descending = true }, "WG1");

            var rows = result.Rows.ToList();
            rows[0].JobCode.Should().Be("JC3");
            rows[1].JobCode.Should().Be("JC2");
            rows[2].JobCode.Should().Be("JC1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Sort_ByTotalTimeAscending_OrdersRowsLowToHigh()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April", totalTime: 30.0),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC2", monthName: "April", totalTime: 10.0),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC3", monthName: "April", totalTime: 20.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "TotalTime", Descending = false }, "WG1");

            var rows = result.Rows.ToList();
            rows[0].TotalTime.Should().Be(10.0);
            rows[1].TotalTime.Should().Be(20.0);
            rows[2].TotalTime.Should().Be(30.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Sort_ByTotalCostDescending_OrdersRowsHighToLow()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April", totalCost: 100.0),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC2", monthName: "April", totalCost: 300.0),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC3", monthName: "April", totalCost: 200.0)
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "TotalCost", Descending = true }, "WG1");

            var rows = result.Rows.ToList();
            rows[0].TotalCost.Should().Be(300.0);
            rows[1].TotalCost.Should().Be(200.0);
            rows[2].TotalCost.Should().Be(100.0);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Sort_NullSortBy_ReturnDefaultOrder()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP3", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC1", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = null }, "WG1");

            // BuildRows applies default OrderBy(ParentProject).ThenBy(JobCode)
            result.Rows.First().ParentProject.Should().Be("PP1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Sort_UnknownColumn_FallsBackToParentProjectOrder()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
            [
                TimeUsageEntry(parentProject: "PP3", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP1", jobCode: "JC1", monthName: "April"),
                TimeUsageEntry(parentProject: "PP2", jobCode: "JC1", monthName: "April")
            ]);

            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "UnknownColumn", Descending = false }, "WG1");

            // Unknown column falls back to ParentProject in the switch _ case
            result.Rows.First().ParentProject.Should().Be("PP1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_Sort_AppliedBeforePaging_SortedSliceReturned()
        {
            _mockRepository.GetWgSummarisedStaffTimeUsageAsync("WG1").Returns(
                Enumerable.Range(1, 15)
                    .Select(i => TimeUsageEntry(
                        parentProject: $"PP{i:D2}",
                        jobCode: $"JC{i:D2}",
                        monthName: "April",
                        totalTime: (double)(16 - i)))
                    .ToList());

            // Sort by TotalTime descending: PP01 has time=15, PP15 has time=1
            var result = await _sut.GetWgSummarisedStaffTimeUsageAsync(
                new QueryParameters<string> { Page = 1, PageSize = 5, SortBy = "TotalTime", Descending = true }, "WG1");

                    result.Rows.Should().HaveCount(5);
                        result.Rows.First().TotalTime.Should().Be(15.0);
                        result.Rows.Last().TotalTime.Should().Be(11.0);
                    }

                    #endregion

                    #region GetAllWorkGroupNamesAsync

                    [Fact]
                    public async Task GetAllWorkGroupNamesAsync_WithData_ReturnsDelegatedList()
                    {
                        // Arrange
                        var names = new List<string> { "WG1", "WG2", "WG3" };
                        _mockRepository.GetAllWorkGroupNamesAsync().Returns(names);

                        // Act
                        var result = await _sut.GetAllWorkGroupNamesAsync();

                        // Assert
                        result.Should().BeEquivalentTo(names);
                        await _mockRepository.Received(1).GetAllWorkGroupNamesAsync();
                    }

                    [Fact]
                    public async Task GetAllWorkGroupNamesAsync_EmptyList_ReturnsEmptyCollection()
                    {
                        // Arrange
                        _mockRepository.GetAllWorkGroupNamesAsync().Returns(new List<string>());

                        // Act
                        var result = await _sut.GetAllWorkGroupNamesAsync();

                        // Assert
                        result.Should().BeEmpty();
                        await _mockRepository.Received(1).GetAllWorkGroupNamesAsync();
                    }

                    [Fact]
                    public async Task GetAllWorkGroupNamesAsync_RepositoryThrows_PropagatesException()
                    {
                        // Arrange
                        _mockRepository.GetAllWorkGroupNamesAsync().ThrowsAsync(new Exception("DB error"));

                        // Act & Assert
                        await Assert.ThrowsAsync<Exception>(() => _sut.GetAllWorkGroupNamesAsync());
                    }

                    #endregion

                    #region GetWorkGroupsByProfitCentreAsync

                    [Fact]
                    public async Task GetWorkGroupsByProfitCentreAsync_WithData_ReturnsMappedPaginatedResult()
                    {
                        // Arrange
                        var query      = new QueryParameters<string>();
                        var parameters = new PaginationParameters<string>();
                        var pagedData  = new PagedData<WorkGroup>(
                            new List<WorkGroup> { new() { WorkGroupName = "WG1", ProfitCentre = "PC001" } },
                            new PaginationData());
                        var expected = new PaginatedResult<WorkGroupDto>
                        {
                            Data = new List<WorkGroupDto> { new() { WorkGroupName = "WG1" } }
                        };

                        _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
                        _mockRepository.GetWorkGroupsByProfitCentreAsync(parameters, "PC001").Returns(pagedData);
                        _mockMapper.Map<PaginatedResult<WorkGroupDto>>(pagedData).Returns(expected);

                        // Act
                        var result = await _sut.GetWorkGroupsByProfitCentreAsync(query, "PC001");

                        // Assert
                        result.Should().BeEquivalentTo(expected);
                        await _mockRepository.Received(1).GetWorkGroupsByProfitCentreAsync(parameters, "PC001");
                    }

                    [Fact]
                    public async Task GetWorkGroupsByProfitCentreAsync_EmptyPage_ReturnsEmptyPaginatedResult()
                    {
                        // Arrange
                        var query      = new QueryParameters<string>();
                        var parameters = new PaginationParameters<string>();
                        var pagedData  = new PagedData<WorkGroup>(new List<WorkGroup>(), new PaginationData());
                        var expected   = new PaginatedResult<WorkGroupDto>();

                        _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
                        _mockRepository.GetWorkGroupsByProfitCentreAsync(parameters, Arg.Any<string>()).Returns(pagedData);
                        _mockMapper.Map<PaginatedResult<WorkGroupDto>>(pagedData).Returns(expected);

                        // Act
                        var result = await _sut.GetWorkGroupsByProfitCentreAsync(query, "PC001");

                        // Assert
                        result.Data.Should().BeEmpty();
                    }

                    [Fact]
                    public async Task GetWorkGroupsByProfitCentreAsync_RepositoryThrows_PropagatesException()
                    {
                        // Arrange
                        var query      = new QueryParameters<string>();
                        var parameters = new PaginationParameters<string>();

                        _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
                        _mockRepository.GetWorkGroupsByProfitCentreAsync(parameters, Arg.Any<string>())
                            .ThrowsAsync(new Exception("DB error"));
                        _mockMapper.Map<PaginatedResult<WorkGroupDto>>(Arg.Any<PagedData<WorkGroup>>())
                            .Returns(new PaginatedResult<WorkGroupDto>());

                        // Act & Assert
                        await Assert.ThrowsAsync<Exception>(() => _sut.GetWorkGroupsByProfitCentreAsync(query, "PC001"));
                    }

                    #endregion

                    #region SetSendEmailForProfitCentreWorkGroupsAsync

                    [Fact]
                    public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WithValidArgs_DelegatesAndReturnsTrue()
                    {
                        // Arrange
                        _mockRepository.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1).Returns(true);

                        // Act
                        var result = await _sut.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1);

                        // Assert
                        result.Should().BeTrue();
                        await _mockRepository.Received(1).SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1);
                    }

                    [Fact]
                    public async Task SetSendEmailForProfitCentreWorkGroupsAsync_RepositoryReturnsFalse_ReturnsFalse()
                    {
                        // Arrange
                        _mockRepository.SetSendEmailForProfitCentreWorkGroupsAsync(Arg.Any<string>(), Arg.Any<short>())
                            .Returns(false);

                        // Act
                        var result = await _sut.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 0);

                        // Assert
                        result.Should().BeFalse();
                    }

                    [Fact]
                    public async Task SetSendEmailForProfitCentreWorkGroupsAsync_RepositoryThrows_PropagatesException()
                    {
                        // Arrange
                        _mockRepository.SetSendEmailForProfitCentreWorkGroupsAsync(Arg.Any<string>(), Arg.Any<short>())
                            .ThrowsAsync(new Exception("DB error"));

                        // Act & Assert
                        await Assert.ThrowsAsync<Exception>(() =>
                            _sut.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1));
                    }

                    #endregion

                    #region SetSendEmailForAllWorkGroupsAsync

                    [Fact]
                    public async Task SetSendEmailForAllWorkGroupsAsync_WithFlagOne_DelegatesAndReturnsTrue()
                    {
                        // Arrange
                        _mockRepository.SetSendEmailForAllWorkGroupsAsync(1).Returns(true);

                        // Act
                        var result = await _sut.SetSendEmailForAllWorkGroupsAsync(1);

                        // Assert
                        result.Should().BeTrue();
                        await _mockRepository.Received(1).SetSendEmailForAllWorkGroupsAsync(1);
                    }

                    [Fact]
                    public async Task SetSendEmailForAllWorkGroupsAsync_WithFlagZero_DelegatesAndReturnsTrue()
                    {
                        // Arrange
                        _mockRepository.SetSendEmailForAllWorkGroupsAsync(0).Returns(true);

                        // Act
                        var result = await _sut.SetSendEmailForAllWorkGroupsAsync(0);

                        // Assert
                        result.Should().BeTrue();
                        await _mockRepository.Received(1).SetSendEmailForAllWorkGroupsAsync(0);
                    }

                    [Fact]
                    public async Task SetSendEmailForAllWorkGroupsAsync_RepositoryThrows_PropagatesException()
                    {
                        // Arrange
                        _mockRepository.SetSendEmailForAllWorkGroupsAsync(Arg.Any<short>())
                            .ThrowsAsync(new Exception("DB error"));

                        // Act & Assert
                        await Assert.ThrowsAsync<Exception>(() => _sut.SetSendEmailForAllWorkGroupsAsync(0));
                    }

                    #endregion

                    #region UpdateWorkGroupEmailAsync

                    [Fact]
                    public async Task UpdateWorkGroupEmailAsync_WithValidArgs_DelegatesAndReturnsTrue()
                    {
                        // Arrange
                        _mockRepository.UpdateWorkGroupEmailAsync("WG1", 1, "test@test.com").Returns(true);

                        // Act
                        var result = await _sut.UpdateWorkGroupEmailAsync("WG1", 1, "test@test.com");

                        // Assert
                        result.Should().BeTrue();
                        await _mockRepository.Received(1).UpdateWorkGroupEmailAsync("WG1", 1, "test@test.com");
                    }

                    [Fact]
                    public async Task UpdateWorkGroupEmailAsync_WithNullEmailRecipient_DelegatesNullAndReturnsTrue()
                    {
                        // Arrange
                        _mockRepository.UpdateWorkGroupEmailAsync("WG1", 0, null).Returns(true);

                        // Act
                        var result = await _sut.UpdateWorkGroupEmailAsync("WG1", 0, null);

                        // Assert
                        result.Should().BeTrue();
                        await _mockRepository.Received(1).UpdateWorkGroupEmailAsync("WG1", 0, null);
                    }

                    [Fact]
                    public async Task UpdateWorkGroupEmailAsync_RepositoryReturnsFalse_ReturnsFalse()
                    {
                        // Arrange
                        _mockRepository.UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>())
                            .Returns(false);

                        // Act
                        var result = await _sut.UpdateWorkGroupEmailAsync("WG1", 1, "x@y.com");

                        // Assert
                        result.Should().BeFalse();
                    }

                    [Fact]
                    public async Task UpdateWorkGroupEmailAsync_RepositoryThrows_PropagatesException()
                    {
                        // Arrange
                        _mockRepository.UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>())
                            .ThrowsAsync(new Exception("DB error"));

                        // Act & Assert
                        await Assert.ThrowsAsync<Exception>(() =>
                            _sut.UpdateWorkGroupEmailAsync("WG1", 1, "test@test.com"));
                    }

                    #endregion
                }
            }

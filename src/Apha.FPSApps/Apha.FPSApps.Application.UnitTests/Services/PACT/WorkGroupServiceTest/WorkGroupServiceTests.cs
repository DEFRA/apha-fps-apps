using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using Apha.FPSApps.Application.Validation;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.WorkGroupServiceTest
{
    public class WorkGroupServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactWorkGroupApiClient _pactWorkGroupApiClient;
        private readonly WorkGroupService _service;

        public WorkGroupServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactWorkGroupApiClient = Substitute.For<IPactWorkGroupApiClient>();
            _pactClient.PactWorkGroup.Returns(_pactWorkGroupApiClient);
            _service = new WorkGroupService(_pactClient);
        }

        #region GetAllWorkGroupsAsync

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1" },
                new() { WorkGroupName = "WG2", ProfitCentre = "PC2" }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(workGroups);
            _pactWorkGroupApiClient.GetAllWorkGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _pactWorkGroupApiClient.Received(1).GetAllWorkGroupsAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_EmptyList_ReturnsSuccessResponseWithEmptyData()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]);
            _pactWorkGroupApiClient.GetAllWorkGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pactWorkGroupApiClient.Received(1).GetAllWorkGroupsAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.GetAllWorkGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _pactWorkGroupApiClient.Received(1).GetAllWorkGroupsAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            _pactWorkGroupApiClient.GetAllWorkGroupsAsync()
                .ThrowsAsync(new Exception("Connection error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetAllWorkGroupsAsync());
        }

        #endregion

        #region GetPagedWorkGroupTimeCodesAsync

        [Fact]
        public async Task GetPagedWorkGroupTimeCodesAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var timeCodes = new List<WorkGroupTimeCodeDto>
            {
                new()
                {
                    PACTStaffID = "S1",
                    ParentProject = "PP1",
                    WorkGroup = "WG1",
                    Name = "John Smith",
                    TimeCode = "TC1",
                    Month = 3,
                    Hours = 7.5
                }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupTimeCodeDto>>.SuccessResponse(
                timeCodes, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            _pactWorkGroupApiClient.GetPagedWorkGroupTimeCodesAsync(query, "WG1", 3).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedWorkGroupTimeCodesAsync(query, "WG1", 3);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            var item = result.Data!.First();
            Assert.Equal("S1", item.PACTStaffID);
            Assert.Equal("PP1", item.ParentProject);
            Assert.Equal("WG1", item.WorkGroup);
            Assert.Equal("John Smith", item.Name);
            Assert.Equal("TC1", item.TimeCode);
            Assert.Equal(3, item.Month);
            Assert.Equal(7.5, item.Hours);
            await _pactWorkGroupApiClient.Received(1).GetPagedWorkGroupTimeCodesAsync(query, "WG1", 3);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodesAsync_ValidWorkGroupAndMonth_PassesToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<WorkGroupTimeCodeDto>>.SuccessResponse([]);
            _pactWorkGroupApiClient.GetPagedWorkGroupTimeCodesAsync(query, "WG1", 1).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedWorkGroupTimeCodesAsync(query, "WG1", 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pactWorkGroupApiClient.Received(1).GetPagedWorkGroupTimeCodesAsync(query, "WG1", 1);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodesAsync_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var expectedResponse = ApiResponseDto<List<WorkGroupTimeCodeDto>>.SuccessResponse(
                [], new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 0 });
            _pactWorkGroupApiClient.GetPagedWorkGroupTimeCodesAsync(query, "WG2", 2).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedWorkGroupTimeCodesAsync(query, "WG2", 2);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkGroupTimeCodeDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.GetPagedWorkGroupTimeCodesAsync(query, "WG1", 1).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedWorkGroupTimeCodesAsync(query, "WG1", 1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _pactWorkGroupApiClient.Received(1).GetPagedWorkGroupTimeCodesAsync(query, "WG1", 1);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodesAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _pactWorkGroupApiClient
                .GetPagedWorkGroupTimeCodesAsync(query, Arg.Any<string?>(), Arg.Any<int>())
                .ThrowsAsync(new Exception("Connection error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetPagedWorkGroupTimeCodesAsync(query, "WG1", 1));
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodesAsync_NullWorkGroup_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.GetPagedWorkGroupTimeCodesAsync(query, null!, 3));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _pactWorkGroupApiClient.DidNotReceive().GetPagedWorkGroupTimeCodesAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string?>(), Arg.Any<int>());
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodesAsync_EmptyWorkGroup_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.GetPagedWorkGroupTimeCodesAsync(query, "   ", 3));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodesAsync_DefaultMonthNumber_PassesDefaultToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<WorkGroupTimeCodeDto>>.SuccessResponse([]);
            _pactWorkGroupApiClient.GetPagedWorkGroupTimeCodesAsync(query, "WG1", 1).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedWorkGroupTimeCodesAsync(query, "WG1", 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pactWorkGroupApiClient.Received(1).GetPagedWorkGroupTimeCodesAsync(query, "WG1", 1);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodesAsync_NullWorkGroupDefaultMonth_ThrowsOnlyWorkGroupError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.GetPagedWorkGroupTimeCodesAsync(query, null!, 1));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
        }

        #endregion

        #region GetPagedWorkGroupValidTimeCodesAsync

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodesAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var validTimeCodes = new List<WorkGroupValidTimeCodeDto>
            {
                new()
                {
                    WorkGroup = "WG1",
                    TimeCode = "TC1",
                    ParentProject = "PP1",
                    Manager = "John Smith",
                    Active = true
                }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.SuccessResponse(
                validTimeCodes, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            _pactWorkGroupApiClient.GetPagedWorkGroupValidTimeCodesAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedWorkGroupValidTimeCodesAsync(query, "WG1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            var item = result.Data!.First();
            Assert.Equal("WG1", item.WorkGroup);
            Assert.Equal("TC1", item.TimeCode);
            Assert.Equal("PP1", item.ParentProject);
            Assert.Equal("John Smith", item.Manager);
            Assert.True(item.Active);
            await _pactWorkGroupApiClient.Received(1).GetPagedWorkGroupValidTimeCodesAsync(query, "WG1");
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodesAsync_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var expectedResponse = ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.SuccessResponse(
                [], new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 0 });
            _pactWorkGroupApiClient.GetPagedWorkGroupValidTimeCodesAsync(query, "WG2").Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedWorkGroupValidTimeCodesAsync(query, "WG2");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pactWorkGroupApiClient.Received(1).GetPagedWorkGroupValidTimeCodesAsync(query, "WG2");
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.GetPagedWorkGroupValidTimeCodesAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedWorkGroupValidTimeCodesAsync(query, "WG1");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _pactWorkGroupApiClient.Received(1).GetPagedWorkGroupValidTimeCodesAsync(query, "WG1");
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodesAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _pactWorkGroupApiClient
                .GetPagedWorkGroupValidTimeCodesAsync(query, Arg.Any<string>())
                .ThrowsAsync(new Exception("Connection error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetPagedWorkGroupValidTimeCodesAsync(query, "WG1"));
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodesAsync_NullWorkGroup_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.GetPagedWorkGroupValidTimeCodesAsync(query, null!));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _pactWorkGroupApiClient.DidNotReceive().GetPagedWorkGroupValidTimeCodesAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodesAsync_EmptyWorkGroup_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.GetPagedWorkGroupValidTimeCodesAsync(query, ""));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _pactWorkGroupApiClient.DidNotReceive().GetPagedWorkGroupValidTimeCodesAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodesAsync_WhitespaceWorkGroup_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.GetPagedWorkGroupValidTimeCodesAsync(query, "   "));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _pactWorkGroupApiClient.DidNotReceive().GetPagedWorkGroupValidTimeCodesAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        #endregion

        #region GetWgSummarisedStaffTimeUsageAsync

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WorkGroupTimeByJobCodeDto
            {
                Rows    = [new() { ParentProject = "PP1", JobCode = "JC1", April = 10.0 }],
                Summary = new WorkGroupTimeByJobCodeSummaryDto { GrandTotalTime = 10.0, StandardHoursPerMonth = 10.0 },
                HrsPaid = 120.0
            };
            var expectedResponse = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(dto);
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data!.Rows);
            Assert.Equal("PP1",  result.Data.Rows.First().ParentProject);
            Assert.Equal("JC1",  result.Data.Rows.First().JobCode);
            Assert.Equal(10.0,   result.Data.Rows.First().April);
            Assert.Equal(10.0,   result.Data.Summary.GrandTotalTime);
            Assert.Equal(120.0,  result.Data.HrsPaid);
            await _pactWorkGroupApiClient.Received(1).GetWgSummarisedStaffTimeUsageAsync(query, "WG1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_ValidWorkGroup_PassesQueryAndWorkGroupToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "JobCode" };
            var expectedResponse = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(new WorkGroupTimeByJobCodeDto());
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG_ALPHA").Returns(expectedResponse);

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG_ALPHA");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pactWorkGroupApiClient.Received(1).GetWgSummarisedStaffTimeUsageAsync(query, "WG_ALPHA");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_EmptyRows_ReturnsSuccessWithEmptyRowsCollection()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WorkGroupTimeByJobCodeDto { Rows = [], HrsPaid = 0 };
            var expectedResponse = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(dto);
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!.Rows);
            await _pactWorkGroupApiClient.Received(1).GetWgSummarisedStaffTimeUsageAsync(query, "WG1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<WorkGroupTimeByJobCodeDto>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _pactWorkGroupApiClient.Received(1).GetWgSummarisedStaffTimeUsageAsync(query, "WG1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _pactWorkGroupApiClient
                .GetWgSummarisedStaffTimeUsageAsync(query, Arg.Any<string>())
                .ThrowsAsync(new Exception("Connection error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1"));
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullWorkGroup_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.GetWgSummarisedStaffTimeUsageAsync(query, null!));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            Assert.Equal("WorkGroup is required", ex.Errors[0].Message);
            await _pactWorkGroupApiClient.DidNotReceive()
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_EmptyWorkGroup_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.GetWgSummarisedStaffTimeUsageAsync(query, ""));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _pactWorkGroupApiClient.DidNotReceive()
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WhitespaceWorkGroup_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.GetWgSummarisedStaffTimeUsageAsync(query, "   "));

            Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_REQUIRED", ex.Errors[0].Code);
            await _pactWorkGroupApiClient.DidNotReceive()
                .GetWgSummarisedStaffTimeUsageAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_ValidationFails_ApiClientNeverCalled()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.GetWgSummarisedStaffTimeUsageAsync(query, null!));

            await _pactWorkGroupApiClient.DidNotReceiveWithAnyArgs()
                .GetWgSummarisedStaffTimeUsageAsync(default!, default!);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_MultipleRows_AllRowsReturnedInResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WorkGroupTimeByJobCodeDto
            {
                Rows =
                [
                    new() { ParentProject = "PP1", JobCode = "JC1", April = 10.0 },
                    new() { ParentProject = "PP1", JobCode = "JC2", April = 5.0  },
                    new() { ParentProject = "PP2", JobCode = "JC1", April = 8.0  }
                ],
                HrsPaid = 120.0
            };
            var expectedResponse = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(dto);
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Rows.Count());
            await _pactWorkGroupApiClient.Received(1).GetWgSummarisedStaffTimeUsageAsync(query, "WG1");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_ReturnedDtoContainsHrsPaid()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WorkGroupTimeByJobCodeDto { HrsPaid = 180.0 };
            var expectedResponse = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(dto);
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(180.0, result.Data!.HrsPaid);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_ReturnedDtoContainsSummary()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WorkGroupTimeByJobCodeDto
            {
                Summary = new WorkGroupTimeByJobCodeSummaryDto
                {
                    GrandTotalTime             = 200.0,
                    StandardHoursPerMonth      = 10.0,
                    GrandTotalPercentAllocated = 75.0
                }
            };
            var expectedResponse = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(dto);
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(200.0, result.Data!.Summary.GrandTotalTime);
            Assert.Equal(10.0,  result.Data.Summary.StandardHoursPerMonth);
            Assert.Equal(75.0,  result.Data.Summary.GrandTotalPercentAllocated);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_ReturnsApiClientResponseDirectly()
        {
            // Arrange — verify the service forwards the exact ApiResponseDto returned by the client
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WorkGroupTimeByJobCodeDto { HrsPaid = 60.0 };
            var expectedResponse = ApiResponseDto<WorkGroupTimeByJobCodeDto>.SuccessResponse(dto);
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert — same reference, no additional transformation
            Assert.Same(expectedResponse, result);
        }

        #endregion
    }
}
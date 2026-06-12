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
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                Rows    = [new() { ParentProject = "PP1", JobCode = "JC1", April = 10.0 }],
                Summary = new WgSummarisedStaffTimeUsageSummaryDto { GrandTotalTime = 10.0, StandardHoursPerMonth = 10.0 },
                HrsPaid = 120.0
            };
            var expectedResponse = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto);
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
            var expectedResponse = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(new WgSummarisedStaffTimeUsageDto());
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
            var dto = new WgSummarisedStaffTimeUsageDto { Rows = [], HrsPaid = 0 };
            var expectedResponse = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto);
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
            var expectedResponse = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.FailureResponse(errors, new ApiMetaDto());
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
            Assert.Equal("STAFFNAME_REQUIRED", ex.Errors[0].Code);
            Assert.Equal("Staff Name is required", ex.Errors[0].Message);
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
            Assert.Equal("STAFFNAME_REQUIRED", ex.Errors[0].Code);
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
            Assert.Equal("STAFFNAME_REQUIRED", ex.Errors[0].Code);
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
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                Rows =
                [
                    new() { ParentProject = "PP1", JobCode = "JC1", April = 10.0 },
                    new() { ParentProject = "PP1", JobCode = "JC2", April = 5.0  },
                    new() { ParentProject = "PP2", JobCode = "JC1", April = 8.0  }
                ],
                HrsPaid = 120.0
            };
            var expectedResponse = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto);
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
            var dto = new WgSummarisedStaffTimeUsageDto { HrsPaid = 180.0 };
            var expectedResponse = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto);
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
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                Summary = new WgSummarisedStaffTimeUsageSummaryDto
                {
                    GrandTotalTime             = 200.0,
                    StandardHoursPerMonth      = 10.0,
                    GrandTotalPercentAllocated = 75.0
                }
            };
            var expectedResponse = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto);
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
            var dto = new WgSummarisedStaffTimeUsageDto { HrsPaid = 60.0 };
            var expectedResponse = ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto);
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert — same reference, no additional transformation
            Assert.Same(expectedResponse, result);
        }

        // ── WgSummarisedStaffTimeUsageRowDto coverage ─────────────────────

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_RowDto_AllMonthPropertiesAreMapped()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var row = new WgSummarisedStaffTimeUsageRowDto
            {
                ParentProject = "PP1",
                JobCode       = "JC1",
                JobTitle      = "Senior Scientist",
                April         = 1.0,
                May           = 2.0,
                June          = 3.0,
                July          = 4.0,
                August        = 5.0,
                September     = 6.0,
                October       = 7.0,
                November      = 8.0,
                December      = 9.0,
                January       = 10.0,
                February      = 11.0,
                March         = 12.0,
                TotalTime     = 78.0,
                TotalCost     = 3900.0
            };
            var dto = new WgSummarisedStaffTimeUsageDto { Rows = [row], HrsPaid = 120.0 };
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            var returned = result.Data!.Rows.Single();
            Assert.Equal("PP1",              returned.ParentProject);
            Assert.Equal("JC1",              returned.JobCode);
            Assert.Equal("Senior Scientist", returned.JobTitle);
            Assert.Equal(1.0,  returned.April);
            Assert.Equal(2.0,  returned.May);
            Assert.Equal(3.0,  returned.June);
            Assert.Equal(4.0,  returned.July);
            Assert.Equal(5.0,  returned.August);
            Assert.Equal(6.0,  returned.September);
            Assert.Equal(7.0,  returned.October);
            Assert.Equal(8.0,  returned.November);
            Assert.Equal(9.0,  returned.December);
            Assert.Equal(10.0, returned.January);
            Assert.Equal(11.0, returned.February);
            Assert.Equal(12.0, returned.March);
            Assert.Equal(78.0,   returned.TotalTime);
            Assert.Equal(3900.0, returned.TotalCost);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_RowDto_DefaultValues_MonthsAndTotalsAreZero()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WgSummarisedStaffTimeUsageDto { Rows = [new WgSummarisedStaffTimeUsageRowDto()] };
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            var returned = result.Data!.Rows.Single();
            Assert.Null(returned.ParentProject);
            Assert.Null(returned.JobCode);
            Assert.Null(returned.JobTitle);
            Assert.Equal(0, returned.April);
            Assert.Equal(0, returned.May);
            Assert.Equal(0, returned.June);
            Assert.Equal(0, returned.July);
            Assert.Equal(0, returned.August);
            Assert.Equal(0, returned.September);
            Assert.Equal(0, returned.October);
            Assert.Equal(0, returned.November);
            Assert.Equal(0, returned.December);
            Assert.Equal(0, returned.January);
            Assert.Equal(0, returned.February);
            Assert.Equal(0, returned.March);
            Assert.Equal(0, returned.TotalTime);
            Assert.Equal(0, returned.TotalCost);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_RowDto_FractionalHours_ArePreserved()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WgSummarisedStaffTimeUsageDto
            {
                Rows = [new WgSummarisedStaffTimeUsageRowDto { April = 7.5, May = 3.25, TotalTime = 10.75 }]
            };
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            var returned = result.Data!.Rows.Single();
            Assert.Equal(7.5,  returned.April);
            Assert.Equal(3.25, returned.May);
            Assert.Equal(10.75, returned.TotalTime);
        }

        // ── WgSummarisedStaffTimeUsageSummaryDto coverage ─────────────────

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SummaryDto_AllTotalMonthsAreMapped()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var summary = new WgSummarisedStaffTimeUsageSummaryDto
            {
                TotalApril     = 10.0, TotalMay       = 11.0, TotalJune      = 12.0,
                TotalJuly      = 13.0, TotalAugust    = 14.0, TotalSeptember = 15.0,
                TotalOctober   = 16.0, TotalNovember  = 17.0, TotalDecember  = 18.0,
                TotalJanuary   = 19.0, TotalFebruary  = 20.0, TotalMarch     = 21.0
            };
            var dto = new WgSummarisedStaffTimeUsageDto { Summary = summary };
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            var s = result.Data!.Summary;
            Assert.Equal(10.0, s.TotalApril);
            Assert.Equal(11.0, s.TotalMay);
            Assert.Equal(12.0, s.TotalJune);
            Assert.Equal(13.0, s.TotalJuly);
            Assert.Equal(14.0, s.TotalAugust);
            Assert.Equal(15.0, s.TotalSeptember);
            Assert.Equal(16.0, s.TotalOctober);
            Assert.Equal(17.0, s.TotalNovember);
            Assert.Equal(18.0, s.TotalDecember);
            Assert.Equal(19.0, s.TotalJanuary);
            Assert.Equal(20.0, s.TotalFebruary);
            Assert.Equal(21.0, s.TotalMarch);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SummaryDto_AllPercentAllocatedFieldsAreMapped()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var summary = new WgSummarisedStaffTimeUsageSummaryDto
            {
                PercentAllocatedApril     = 80.0, PercentAllocatedMay       = 81.0,
                PercentAllocatedJune      = 82.0, PercentAllocatedJuly      = 83.0,
                PercentAllocatedAugust    = 84.0, PercentAllocatedSeptember = 85.0,
                PercentAllocatedOctober   = 86.0, PercentAllocatedNovember  = 87.0,
                PercentAllocatedDecember  = 88.0, PercentAllocatedJanuary   = 89.0,
                PercentAllocatedFebruary  = 90.0, PercentAllocatedMarch     = 91.0
            };
            var dto = new WgSummarisedStaffTimeUsageDto { Summary = summary };
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            var s = result.Data!.Summary;
            Assert.Equal(80.0, s.PercentAllocatedApril);
            Assert.Equal(81.0, s.PercentAllocatedMay);
            Assert.Equal(82.0, s.PercentAllocatedJune);
            Assert.Equal(83.0, s.PercentAllocatedJuly);
            Assert.Equal(84.0, s.PercentAllocatedAugust);
            Assert.Equal(85.0, s.PercentAllocatedSeptember);
            Assert.Equal(86.0, s.PercentAllocatedOctober);
            Assert.Equal(87.0, s.PercentAllocatedNovember);
            Assert.Equal(88.0, s.PercentAllocatedDecember);
            Assert.Equal(89.0, s.PercentAllocatedJanuary);
            Assert.Equal(90.0, s.PercentAllocatedFebruary);
            Assert.Equal(91.0, s.PercentAllocatedMarch);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SummaryDto_StandardHoursFields_AreMapped()
        {
            // Arrange — 120 HrsPaid → StandardHoursPerMonth = 10, TotalStandardHours = 120
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var summary = new WgSummarisedStaffTimeUsageSummaryDto
            {
                StandardHoursPerMonth = 10.0,
                TotalStandardHours    = 120.0
            };
            var dto = new WgSummarisedStaffTimeUsageDto { Summary = summary, HrsPaid = 120.0 };
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.Equal(10.0,  result.Data!.Summary.StandardHoursPerMonth);
            Assert.Equal(120.0, result.Data.Summary.TotalStandardHours);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SummaryDto_ZeroHrsPaid_StandardHoursAreZero()
        {
            // Arrange — mirrors service rule: hrsPaid == 0 → standardHoursPerMonth = 0
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var summary = new WgSummarisedStaffTimeUsageSummaryDto
            {
                StandardHoursPerMonth = 0,
                TotalStandardHours    = 0
            };
            var dto = new WgSummarisedStaffTimeUsageDto { Summary = summary, HrsPaid = 0 };
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.Equal(0, result.Data!.Summary.StandardHoursPerMonth);
            Assert.Equal(0, result.Data.Summary.TotalStandardHours);
            Assert.Equal(0, result.Data.HrsPaid);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SummaryDto_DefaultValues_AllFieldsAreZero()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WgSummarisedStaffTimeUsageDto { Summary = new WgSummarisedStaffTimeUsageSummaryDto() };
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            var s = result.Data!.Summary;
            Assert.Equal(0, s.TotalApril);     Assert.Equal(0, s.TotalMay);
            Assert.Equal(0, s.TotalJune);      Assert.Equal(0, s.TotalJuly);
            Assert.Equal(0, s.TotalAugust);    Assert.Equal(0, s.TotalSeptember);
            Assert.Equal(0, s.TotalOctober);   Assert.Equal(0, s.TotalNovember);
            Assert.Equal(0, s.TotalDecember);  Assert.Equal(0, s.TotalJanuary);
            Assert.Equal(0, s.TotalFebruary);  Assert.Equal(0, s.TotalMarch);
            Assert.Equal(0, s.GrandTotalTime); Assert.Equal(0, s.GrandTotalCost);
            Assert.Equal(0, s.GrandTotalPercentAllocated);
            Assert.Equal(0, s.StandardHoursPerMonth);
            Assert.Equal(0, s.TotalStandardHours);
            Assert.Equal(0, s.PercentAllocatedApril);
            Assert.Equal(0, s.PercentAllocatedMarch);
        }

        // ── WgSummarisedStaffTimeUsageDto (wrapper) coverage ──────────────

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WrapperDto_DefaultRows_IsEmptyCollection()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WgSummarisedStaffTimeUsageDto();
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.NotNull(result.Data!.Rows);
            Assert.Empty(result.Data.Rows);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WrapperDto_DefaultSummary_IsNotNull()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WgSummarisedStaffTimeUsageDto();
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.NotNull(result.Data!.Summary);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_WrapperDto_DefaultPagination_IsNotNull()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new WgSummarisedStaffTimeUsageDto();
            _pactWorkGroupApiClient.GetWgSummarisedStaffTimeUsageAsync(query, "WG1")
                .Returns(ApiResponseDto<WgSummarisedStaffTimeUsageDto>.SuccessResponse(dto));

            // Act
            var result = await _service.GetWgSummarisedStaffTimeUsageAsync(query, "WG1");

            // Assert
            Assert.NotNull(result.Data!.Pagination);
        }

        #endregion

        #region GetAllWorkGroupNamesAsync

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var names = new List<string> { "WG01", "WG02" };
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(names);
            _pactWorkGroupApiClient.GetAllWorkGroupNamesAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactWorkGroupApiClient.Received(1).GetAllWorkGroupNamesAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_EmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse([]);
            _pactWorkGroupApiClient.GetAllWorkGroupNamesAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pactWorkGroupApiClient.Received(1).GetAllWorkGroupNamesAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.GetAllWorkGroupNamesAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _pactWorkGroupApiClient.Received(1).GetAllWorkGroupNamesAsync();
        }

        #endregion

        #region GetWorkGroupsByProfitCentreAsync Tests

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WithValidInput_ReturnsPagedWorkGroups()
        {
            // Arrange
            const string profitCentre = "PC001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "WG001", ProfitCentre = profitCentre },
                new() { WorkGroupName = "WG002", ProfitCentre = profitCentre }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(workGroups);
            _pactWorkGroupApiClient.GetWorkGroupsByProfitCentreAsync(query, profitCentre).Returns(expectedResponse);

            // Act
            var result = await _service.GetWorkGroupsByProfitCentreAsync(query, profitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactWorkGroupApiClient.Received(1).GetWorkGroupsByProfitCentreAsync(query, profitCentre);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WithNoMatchingWorkGroups_ReturnsEmptyList()
        {
            // Arrange
            const string profitCentre = "PC_NONE";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(new List<WorkGroupDto>());
            _pactWorkGroupApiClient.GetWorkGroupsByProfitCentreAsync(query, profitCentre).Returns(expectedResponse);

            // Act
            var result = await _service.GetWorkGroupsByProfitCentreAsync(query, profitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.GetWorkGroupsByProfitCentreAsync(query, Arg.Any<string>()).Returns(expectedResponse);

            // Act
            var result = await _service.GetWorkGroupsByProfitCentreAsync(query, "PC001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region SetSendEmailForProfitCentreWorkGroupsAsync Tests

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WithValidInput_ReturnsSuccessTrue()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short flag = 1;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactWorkGroupApiClient.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag).Returns(expectedResponse);

            // Act
            var result = await _service.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactWorkGroupApiClient.Received(1).SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag);
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient
                .SetSendEmailForProfitCentreWorkGroupsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(expectedResponse);

            // Act
            var result = await _service.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region SetSendEmailForAllWorkGroupsAsync Tests

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_WithClearFlag_ReturnsSuccessTrue()
        {
            // Arrange
            const short flag = 0;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactWorkGroupApiClient.SetSendEmailForAllWorkGroupsAsync(flag).Returns(expectedResponse);

            // Act
            var result = await _service.SetSendEmailForAllWorkGroupsAsync(flag);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactWorkGroupApiClient.Received(1).SetSendEmailForAllWorkGroupsAsync(flag);
        }

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.SetSendEmailForAllWorkGroupsAsync(Arg.Any<short>()).Returns(expectedResponse);

            // Act
            var result = await _service.SetSendEmailForAllWorkGroupsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region UpdateWorkGroupEmailAsync Tests

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WithValidInput_ReturnsSuccessTrue()
        {
            // Arrange
            const string workGroupName = "WG001";
            const short sendEmail = 1;
            const string emailRecipient = "test@example.com";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactWorkGroupApiClient.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactWorkGroupApiClient.Received(1).UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient);
        }

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WithNullEmailRecipient_ReturnsSuccessTrue()
        {
            // Arrange
            const string workGroupName = "WG001";
            const short sendEmail = 0;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactWorkGroupApiClient.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, null).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pactWorkGroupApiClient.Received(1).UpdateWorkGroupEmailAsync(workGroupName, sendEmail, null);
        }

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient
                .UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>())
                .Returns(expectedResponse);

            // Act
            var result = await _service.UpdateWorkGroupEmailAsync("WG001", 1, "test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion
    }
}
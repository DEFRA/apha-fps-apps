using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.PlanStaffZTCodeServiceTest
{
    public class PlanStaffZTCodeServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IPactApiClient _pactClient;
        private readonly IFpsStaffJobApiClient _fpsStaffJobApiClient;
        private readonly IPactJobCodeApiClient _pactJobCodeApiClient;
        private readonly PlanStaffZTCodeService _sut;

        public PlanStaffZTCodeServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _pactClient = Substitute.For<IPactApiClient>();
            _fpsStaffJobApiClient = Substitute.For<IFpsStaffJobApiClient>();
            _pactJobCodeApiClient = Substitute.For<IPactJobCodeApiClient>();
            _fpsClient.FpsStaffJob.Returns(_fpsStaffJobApiClient);
            _pactClient.PactJobCode.Returns(_pactJobCodeApiClient);
            _sut = new PlanStaffZTCodeService(_fpsClient, _pactClient);
        }

        #region GetZtJobCodesAsync Tests

        [Fact]
        public async Task GetZtJobCodesAsync_WithSuccessResponse_ReturnsZtJobCodes()
        {
            // Arrange
            var ztCodes = new List<FpsJobCodeZtDto>
            {
                new FpsJobCodeZtDto { JobCode = "ZT001", Description = "Admin Work" },
                new FpsJobCodeZtDto { JobCode = "ZT002", Description = "Training" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<FpsJobCodeZtDto>>.SuccessResponse(ztCodes);

            _pactJobCodeApiClient.GetZtJobCodesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            await _pactJobCodeApiClient.Received(1).GetZtJobCodesAsync();
        }

        [Fact]
        public async Task GetZtJobCodesAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IEnumerable<FpsJobCodeZtDto>>.SuccessResponse(
                new List<FpsJobCodeZtDto>());

            _pactJobCodeApiClient.GetZtJobCodesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetZtJobCodesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<FpsJobCodeZtDto>>.FailureResponse(errors, new ApiMetaDto());

            _pactJobCodeApiClient.GetZtJobCodesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetStaffSummaryByIdAsync Tests

        [Fact]
        public async Task GetStaffSummaryByIdAsync_WithValidStaffId_ReturnsStaffSummary()
        {
            // Arrange
            var staffId = "S001";
            var staffSummary = new StaffWorkgroupLookupDto
            {
                StaffID = staffId,
                Name = "John Doe",
                WorkGroupGrade = "WG1",
                HrsAvail = 37.5,
                HrsPaid = 40.0,
                Leave = 2.0,
                SickSpecial = 0.5
            };
            var expectedResponse = ApiResponseDto<StaffWorkgroupLookupDto>.SuccessResponse(staffSummary);

            _fpsStaffJobApiClient.GetStaffSummaryByIdAsync(staffId).Returns(expectedResponse);

            // Act
            var result = await _sut.GetStaffSummaryByIdAsync(staffId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(staffId, result.Data.StaffID);
            Assert.Equal("John Doe", result.Data.Name);
            Assert.Equal(37.5, result.Data.HrsAvail);
            await _fpsStaffJobApiClient.Received(1).GetStaffSummaryByIdAsync(staffId);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<StaffWorkgroupLookupDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetStaffSummaryByIdAsync(staffId).Returns(expectedResponse);

            // Act
            var result = await _sut.GetStaffSummaryByIdAsync(staffId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetZtTotalHoursByStaffIdAsync Tests

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_WithValidStaffId_ReturnsTotalHours()
        {
            // Arrange
            var staffId = "S001";
            var totalHours = 120.5;
            var expectedResponse = ApiResponseDto<double>.SuccessResponse(totalHours);

            _fpsStaffJobApiClient.GetZtTotalHoursByStaffIdAsync(staffId).Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtTotalHoursByStaffIdAsync(staffId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(totalHours, result.Data);
            await _fpsStaffJobApiClient.Received(1).GetZtTotalHoursByStaffIdAsync(staffId);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_WithNoZtHours_ReturnsZero()
        {
            // Arrange
            var staffId = "S002";
            var expectedResponse = ApiResponseDto<double>.SuccessResponse(0.0);

            _fpsStaffJobApiClient.GetZtTotalHoursByStaffIdAsync(staffId).Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtTotalHoursByStaffIdAsync(staffId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0.0, result.Data);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "S001";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<double>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetZtTotalHoursByStaffIdAsync(staffId).Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtTotalHoursByStaffIdAsync(staffId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetZtStaffJobsByStaffIdPagedAsync Tests

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_WithValidParams_ReturnsPagedResult()
        {
            // Arrange
            var staffId = "S001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var ztJobs = new List<StaffJobZtViewDto>
            {
                new StaffJobZtViewDto { StaffID = staffId, JobCode = "ZT001", PlannedHours = 40, Name = "Admin Work" },
                new StaffJobZtViewDto { StaffID = staffId, JobCode = "ZT002", PlannedHours = 20, Name = "Training" }
            };
            var expectedResponse = ApiResponseDto<List<StaffJobZtViewDto>>.SuccessResponse(ztJobs,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _fpsStaffJobApiClient.GetZtStaffJobsByStaffIdPagedAsync(query, staffId).Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtStaffJobsByStaffIdPagedAsync(query, staffId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("ZT001", result.Data[0].JobCode);
            await _fpsStaffJobApiClient.Received(1).GetZtStaffJobsByStaffIdPagedAsync(query, staffId);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var staffId = "S999";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<StaffJobZtViewDto>>.SuccessResponse(new List<StaffJobZtViewDto>());

            _fpsStaffJobApiClient.GetZtStaffJobsByStaffIdPagedAsync(query, staffId).Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtStaffJobsByStaffIdPagedAsync(query, staffId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "S001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<StaffJobZtViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetZtStaffJobsByStaffIdPagedAsync(query, staffId).Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtStaffJobsByStaffIdPagedAsync(query, staffId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetZtStaffJobDetailsByIdAsync Tests

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_WithValidIds_ReturnsDetail()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "ZT001";
            var ztDetail = new StaffJobZtViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                PlannedHours = 40,
                Name = "Admin Work",
                ZtDescription = "Administrative Tasks"
            };
            var expectedResponse = ApiResponseDto<StaffJobZtViewDto>.SuccessResponse(ztDetail);

            _fpsStaffJobApiClient.GetZtStaffJobDetailsByIdAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtStaffJobDetailsByIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(staffId, result.Data.StaffID);
            Assert.Equal(jobCode, result.Data.JobCode);
            Assert.Equal(40, result.Data.PlannedHours);
            await _fpsStaffJobApiClient.Received(1).GetZtStaffJobDetailsByIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "S999";
            var jobCode = "ZT999";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<StaffJobZtViewDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetZtStaffJobDetailsByIdAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetZtStaffJobDetailsByIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetStaffJobsByJobCodeAsync Tests

        [Fact]
        public async Task GetStaffJobsByJobCodeAsync_WithValidParams_ReturnsStaffJobs()
        {
            // Arrange
            var jobCode = "JOB001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var staffJobs = new List<StaffJobViewDto>
            {
                new StaffJobViewDto { StaffID = "S001", JobCode = jobCode, PlannedHours = 40 },
                new StaffJobViewDto { StaffID = "S002", JobCode = jobCode, PlannedHours = 20 }
            };
            var expectedResponse = ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(staffJobs);

            _fpsStaffJobApiClient.GetAllStaffJobAsync(query, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetStaffJobsByJobCodeAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _fpsStaffJobApiClient.Received(1).GetAllStaffJobAsync(query, jobCode);
        }

        [Fact]
        public async Task GetStaffJobsByJobCodeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var jobCode = "JOB001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<StaffJobViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetAllStaffJobAsync(query, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetStaffJobsByJobCodeAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetStaffJobAsync Tests

        [Fact]
        public async Task GetStaffJobAsync_WithValidIds_ReturnsStaffJob()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "ZT001";
            var staffJob = new StaffJobDto { StaffId = staffId, JobCode = jobCode, PlannedHours = 40 };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(staffJob);

            _fpsStaffJobApiClient.GetStaffJobByIdAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetStaffJobAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(staffId, result.Data.StaffId);
            Assert.Equal(jobCode, result.Data.JobCode);
            await _fpsStaffJobApiClient.Received(1).GetStaffJobByIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetStaffJobAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "S999";
            var jobCode = "ZT999";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetStaffJobByIdAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.GetStaffJobAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateStaffJobAsync Tests

        [Fact]
        public async Task CreateStaffJobAsync_WithValidDto_ReturnsCreatedStaffJob()
        {
            // Arrange
            var staffJob = new StaffJobDto { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(staffJob);

            _fpsStaffJobApiClient.CreateStaffJobAsync(staffJob).Returns(expectedResponse);

            // Act
            var result = await _sut.CreateStaffJobAsync(staffJob);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("S001", result.Data.StaffId);
            Assert.Equal("ZT001", result.Data.JobCode);
            Assert.Equal(40, result.Data.PlannedHours);
            await _fpsStaffJobApiClient.Received(1).CreateStaffJobAsync(staffJob);
        }

        [Fact]
        public async Task CreateStaffJobAsync_WhenDuplicate_ReturnsFailureResponse()
        {
            // Arrange
            var staffJob = new StaffJobDto { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Duplicate entry", Code = "DUPLICATE" }
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.CreateStaffJobAsync(staffJob).Returns(expectedResponse);

            // Act
            var result = await _sut.CreateStaffJobAsync(staffJob);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region UpdateStaffJobAsync Tests

        [Fact]
        public async Task UpdateStaffJobAsync_WithValidDto_ReturnsUpdatedStaffJob()
        {
            // Arrange
            var staffJob = new StaffJobDto { StaffId = "S001", JobCode = "ZT001", PlannedHours = 60 };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(staffJob);

            _fpsStaffJobApiClient.UpdateStaffJobAsync(staffJob).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateStaffJobAsync(staffJob);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(60, result.Data.PlannedHours);
            await _fpsStaffJobApiClient.Received(1).UpdateStaffJobAsync(staffJob);
        }

        [Fact]
        public async Task UpdateStaffJobAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var staffJob = new StaffJobDto { StaffId = "S999", JobCode = "ZT999", PlannedHours = 60 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.UpdateStaffJobAsync(staffJob).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateStaffJobAsync(staffJob);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteStaffJobAsync Tests

        [Fact]
        public async Task DeleteStaffJobAsync_WithValidIds_ReturnsSuccess()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "ZT001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsStaffJobApiClient.DeleteStaffJobAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteStaffJobAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsStaffJobApiClient.Received(1).DeleteStaffJobAsync(staffId, jobCode);
        }

        [Fact]
        public async Task DeleteStaffJobAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "S999";
            var jobCode = "ZT999";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.DeleteStaffJobAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteStaffJobAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("S001", "ZT001")]
        [InlineData("S002", "ZT002")]
        [InlineData("EMP123", "ZT_TEST")]
        public async Task DeleteStaffJobAsync_WithVariousIds_CallsApiClient(string staffId, string jobCode)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsStaffJobApiClient.DeleteStaffJobAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            await _sut.DeleteStaffJobAsync(staffId, jobCode);

            // Assert
            await _fpsStaffJobApiClient.Received(1).DeleteStaffJobAsync(staffId, jobCode);
        }

        #endregion
    }
}

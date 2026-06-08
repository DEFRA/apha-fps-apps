using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.ProjectYearCostsServiceTest
{
    public class ProjectYearCostsServiceTests
    {
        private readonly IPimsApiClient _pimsApiClient;
        private readonly IPimsProjectYearCostsApiClient _pimsProjectYearCostsApiClient;
        private readonly ProjectYearCostsService _projectYearCostsService;

        private const string Project = "PP001";
        private const short Year = 2024;

        public ProjectYearCostsServiceTests()
        {
            _pimsApiClient = Substitute.For<IPimsApiClient>();
            _pimsProjectYearCostsApiClient = Substitute.For<IPimsProjectYearCostsApiClient>();
            _pimsApiClient.PimsProjectYearCosts.Returns(_pimsProjectYearCostsApiClient);
            _projectYearCostsService = new ProjectYearCostsService(_pimsApiClient);
        }

        #region GetAdditionalActualsAsync Tests

        [Fact]
        public async Task GetAdditionalActualsAsync_WithSuccessResponse_ReturnsAdditionalActualsList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "Test" };
            var actuals = new List<AdditionalCostDto>
            {
                new AdditionalCostDto { Project = Project, Year = Year, Description = "Subcontract A", Amount = 500m },
                new AdditionalCostDto { Project = Project, Year = Year, Description = "Subcontract B", Amount = 750m }
            };
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(actuals);

            _pimsProjectYearCostsApiClient.GetAdditionalActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAdditionalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectYearCostsApiClient.Received(1).GetAdditionalActualsAsync(Project, Year, query);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>());

            _pimsProjectYearCostsApiClient.GetAdditionalActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAdditionalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetAdditionalActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAdditionalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_PassesCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 20, Search = "Contract", SortBy = "Description", Descending = true };
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>());

            _pimsProjectYearCostsApiClient.GetAdditionalActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetAdditionalActualsAsync(Project, Year, query);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetAdditionalActualsAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year),
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 &&
                    q.PageSize == 20 &&
                    q.Search == "Contract" &&
                    q.SortBy == "Description" &&
                    q.Descending == true
                )
            );
        }

        #endregion

        #region GetAdditionalPlansAsync Tests

        [Fact]
        public async Task GetAdditionalPlansAsync_WithSuccessResponse_ReturnsAdditionalPlansList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var plans = new List<AdditionalCostDto>
            {
                new AdditionalCostDto { Project = Project, Year = Year, Description = "Plan A", ItemCost = 200m },
                new AdditionalCostDto { Project = Project, Year = Year, Description = "Plan B", ItemCost = 400m }
            };
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(plans);

            _pimsProjectYearCostsApiClient.GetAdditionalPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAdditionalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectYearCostsApiClient.Received(1).GetAdditionalPlansAsync(Project, Year, query);
        }

        [Fact]
        public async Task GetAdditionalPlansAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>());

            _pimsProjectYearCostsApiClient.GetAdditionalPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAdditionalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAdditionalPlansAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetAdditionalPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAdditionalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAdditionalPlansAsync_PassesCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15, Search = "Plan", SortBy = "Account", Descending = false };
            var expectedResponse = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(new List<AdditionalCostDto>());

            _pimsProjectYearCostsApiClient.GetAdditionalPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetAdditionalPlansAsync(Project, Year, query);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetAdditionalPlansAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year),
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 15 &&
                    q.Search == "Plan" &&
                    q.SortBy == "Account" &&
                    q.Descending == false
                )
            );
        }

        #endregion

        #region GetAnimalActualsAsync Tests

        [Fact]
        public async Task GetAnimalActualsAsync_WithSuccessResponse_ReturnsAnimalActualsList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var animalActuals = new List<AnimalCostDto>
            {
                new AnimalCostDto { Project = Project, Year = Year, Description = "Large Animal A", Amount = 1200m },
                new AnimalCostDto { Project = Project, Year = Year, Description = "Small Animal B", Amount = 300m }
            };
            var expectedResponse = ApiResponseDto<List<AnimalCostDto>>.SuccessResponse(animalActuals);

            _pimsProjectYearCostsApiClient.GetAnimalActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAnimalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectYearCostsApiClient.Received(1).GetAnimalActualsAsync(Project, Year, query);
        }

        [Fact]
        public async Task GetAnimalActualsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<AnimalCostDto>>.SuccessResponse(new List<AnimalCostDto>());

            _pimsProjectYearCostsApiClient.GetAnimalActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAnimalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAnimalActualsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Animal actuals not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<List<AnimalCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetAnimalActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAnimalActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAnimalActualsAsync_PassesCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 3, PageSize = 25, Search = "Animal", SortBy = "AcctCode", Descending = false };
            var expectedResponse = ApiResponseDto<List<AnimalCostDto>>.SuccessResponse(new List<AnimalCostDto>());

            _pimsProjectYearCostsApiClient.GetAnimalActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetAnimalActualsAsync(Project, Year, query);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetAnimalActualsAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year),
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 3 &&
                    q.PageSize == 25 &&
                    q.Search == "Animal" &&
                    q.SortBy == "AcctCode" &&
                    q.Descending == false
                )
            );
        }

        #endregion

        #region GetAnimalPlansAsync Tests

        [Fact]
        public async Task GetAnimalPlansAsync_WithSuccessResponse_ReturnsAnimalPlansList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var animalPlans = new List<AnimalCostDto>
            {
                new AnimalCostDto { ParentProject = Project, Year = Year, AnimalType = "Cattle", NumberOfAnimals = 5, Cost = 2000 },
                new AnimalCostDto { ParentProject = Project, Year = Year, AnimalType = "Sheep", NumberOfAnimals = 10, Cost = 1500 }
            };
            var expectedResponse = ApiResponseDto<List<AnimalCostDto>>.SuccessResponse(animalPlans);

            _pimsProjectYearCostsApiClient.GetAnimalPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAnimalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectYearCostsApiClient.Received(1).GetAnimalPlansAsync(Project, Year, query);
        }

        [Fact]
        public async Task GetAnimalPlansAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<AnimalCostDto>>.SuccessResponse(new List<AnimalCostDto>());

            _pimsProjectYearCostsApiClient.GetAnimalPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAnimalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAnimalPlansAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Animal plans not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<List<AnimalCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetAnimalPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetAnimalPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAnimalPlansAsync_PassesCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "Cattle", SortBy = "AnimalType", Descending = true };
            var expectedResponse = ApiResponseDto<List<AnimalCostDto>>.SuccessResponse(new List<AnimalCostDto>());

            _pimsProjectYearCostsApiClient.GetAnimalPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetAnimalPlansAsync(Project, Year, query);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetAnimalPlansAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year),
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 10 &&
                    q.Search == "Cattle" &&
                    q.SortBy == "AnimalType" &&
                    q.Descending == true
                )
            );
        }

        #endregion

        #region GetTestPlansAsync Tests

        [Fact]
        public async Task GetTestPlansAsync_WithSuccessResponse_ReturnsTestPlansList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var testPlans = new List<TestCostDto>
            {
                new TestCostDto { Year = Year, Buyer = Project, TestCode = "TC001", UnitPrice = 100m, Cost = 500m },
                new TestCostDto { Year = Year, Buyer = Project, TestCode = "TC002", UnitPrice = 200m, Cost = 800m }
            };
            var expectedResponse = ApiResponseDto<List<TestCostDto>>.SuccessResponse(testPlans);

            _pimsProjectYearCostsApiClient.GetTestPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetTestPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectYearCostsApiClient.Received(1).GetTestPlansAsync(Project, Year, query);
        }

        [Fact]
        public async Task GetTestPlansAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<TestCostDto>>.SuccessResponse(new List<TestCostDto>());

            _pimsProjectYearCostsApiClient.GetTestPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetTestPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetTestPlansAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Test plans not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<List<TestCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetTestPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetTestPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetTestPlansAsync_PassesCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 10, Search = "TC", SortBy = "TestCode", Descending = false };
            var expectedResponse = ApiResponseDto<List<TestCostDto>>.SuccessResponse(new List<TestCostDto>());

            _pimsProjectYearCostsApiClient.GetTestPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetTestPlansAsync(Project, Year, query);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetTestPlansAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year),
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 &&
                    q.PageSize == 10 &&
                    q.Search == "TC" &&
                    q.SortBy == "TestCode" &&
                    q.Descending == false
                )
            );
        }

        #endregion

        #region GetTestActualsAsync Tests

        [Fact]
        public async Task GetTestActualsAsync_WithSuccessResponse_ReturnsTestActualsList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var testActuals = new List<TestCostDto>
            {
                new TestCostDto { Year = Year, Buyer = Project, TestCode = "TC001", Charge = 450m, Volume = 5 },
                new TestCostDto { Year = Year, Buyer = Project, TestCode = "TC002", Charge = 900m, Volume = 9 }
            };
            var expectedResponse = ApiResponseDto<List<TestCostDto>>.SuccessResponse(testActuals);

            _pimsProjectYearCostsApiClient.GetTestActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetTestActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectYearCostsApiClient.Received(1).GetTestActualsAsync(Project, Year, query);
        }

        [Fact]
        public async Task GetTestActualsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<TestCostDto>>.SuccessResponse(new List<TestCostDto>());

            _pimsProjectYearCostsApiClient.GetTestActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetTestActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetTestActualsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Test actuals not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<List<TestCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetTestActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetTestActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetTestActualsAsync_PassesCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 50, Search = "TC001", SortBy = "WorkGroup", Descending = true };
            var expectedResponse = ApiResponseDto<List<TestCostDto>>.SuccessResponse(new List<TestCostDto>());

            _pimsProjectYearCostsApiClient.GetTestActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetTestActualsAsync(Project, Year, query);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetTestActualsAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year),
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 50 &&
                    q.Search == "TC001" &&
                    q.SortBy == "WorkGroup" &&
                    q.Descending == true
                )
            );
        }

        #endregion

        #region GetStaffPlansAsync Tests

        [Fact]
        public async Task GetStaffPlansAsync_WithSuccessResponse_ReturnsStaffPlansList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var staffPlans = new List<StaffCostDto>
            {
                new StaffCostDto { Year = Year, ParentProject = Project, Name = "Staff A", WgGrade = "G6", PlannedHours = 100, Cost = 5000m },
                new StaffCostDto { Year = Year, ParentProject = Project, Name = "Staff B", WgGrade = "G7", PlannedHours = 80, Cost = 3500m }
            };
            var expectedResponse = ApiResponseDto<List<StaffCostDto>>.SuccessResponse(staffPlans);

            _pimsProjectYearCostsApiClient.GetStaffPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetStaffPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectYearCostsApiClient.Received(1).GetStaffPlansAsync(Project, Year, query);
        }

        [Fact]
        public async Task GetStaffPlansAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<StaffCostDto>>.SuccessResponse(new List<StaffCostDto>());

            _pimsProjectYearCostsApiClient.GetStaffPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetStaffPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetStaffPlansAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Staff plans not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<List<StaffCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetStaffPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetStaffPlansAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetStaffPlansAsync_PassesCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "Staff", SortBy = "Name", Descending = false };
            var expectedResponse = ApiResponseDto<List<StaffCostDto>>.SuccessResponse(new List<StaffCostDto>());

            _pimsProjectYearCostsApiClient.GetStaffPlansAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetStaffPlansAsync(Project, Year, query);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetStaffPlansAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year),
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 10 &&
                    q.Search == "Staff" &&
                    q.SortBy == "Name" &&
                    q.Descending == false
                )
            );
        }

        #endregion

        #region GetStaffActualsAsync Tests

        [Fact]
        public async Task GetStaffActualsAsync_WithSuccessResponse_ReturnsStaffActualsList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var staffActuals = new List<StaffCostDto>
            {
                new StaffCostDto { Year = Year, JobCode = Project, WorkGroup = "WG1", GradeCode = "G6", Time = 80, ActualCost = 4800m },
                new StaffCostDto { Year = Year, JobCode = Project, WorkGroup = "WG2", GradeCode = "G7", Time = 60, ActualCost = 3200m }
            };
            var expectedResponse = ApiResponseDto<List<StaffCostDto>>.SuccessResponse(staffActuals);

            _pimsProjectYearCostsApiClient.GetStaffActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetStaffActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectYearCostsApiClient.Received(1).GetStaffActualsAsync(Project, Year, query);
        }

        [Fact]
        public async Task GetStaffActualsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<StaffCostDto>>.SuccessResponse(new List<StaffCostDto>());

            _pimsProjectYearCostsApiClient.GetStaffActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetStaffActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetStaffActualsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Staff actuals not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<List<StaffCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetStaffActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetStaffActualsAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetStaffActualsAsync_PassesCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 30, Search = "WG1", SortBy = "GradeCode", Descending = true };
            var expectedResponse = ApiResponseDto<List<StaffCostDto>>.SuccessResponse(new List<StaffCostDto>());

            _pimsProjectYearCostsApiClient.GetStaffActualsAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetStaffActualsAsync(Project, Year, query);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetStaffActualsAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year),
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 &&
                    q.PageSize == 30 &&
                    q.Search == "WG1" &&
                    q.SortBy == "GradeCode" &&
                    q.Descending == true
                )
            );
        }

        #endregion

        #region GetProjectYearDetailsAsync Tests

        [Fact]
        public async Task GetProjectYearDetailsAsync_WithValidParameters_ReturnsProjectYearDetails()
        {
            // Arrange
            var projectYearDetails = new ProjectYearDetailsDto
            {
                Year = Year,
                Parentproject = Project,
                Manager = "Manager A",
                Disease = "Disease A",
                Contract = "Contract A"
            };
            var expectedResponse = ApiResponseDto<ProjectYearDetailsDto>.SuccessResponse(projectYearDetails);

            _pimsProjectYearCostsApiClient.GetProjectYearDetailsAsync(Project, Year).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetProjectYearDetailsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(Project, result.Data.Parentproject);
            Assert.Equal(Year, result.Data.Year);
            await _pimsProjectYearCostsApiClient.Received(1).GetProjectYearDetailsAsync(Project, Year);
        }

        [Fact]
        public async Task GetProjectYearDetailsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Project year details not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProjectYearDetailsDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetProjectYearDetailsAsync(Project, Year).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetProjectYearDetailsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProjectYearDetailsAsync_PassesCorrectParameters()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<ProjectYearDetailsDto>.SuccessResponse(new ProjectYearDetailsDto { Parentproject = Project, Year = Year });

            _pimsProjectYearCostsApiClient.GetProjectYearDetailsAsync(Project, Year).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetProjectYearDetailsAsync(Project, Year);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetProjectYearDetailsAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year)
            );
        }

        #endregion

        #region GetPactPayAsync Tests

        [Fact]
        public async Task GetPactPayAsync_WithSuccessResponse_ReturnsPactPayList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pactPay = new List<PactPayDto>
            {
                new PactPayDto { Year = Year, Project = Project, Month = 1, Pay = 5000m, NonPay = 2000m, StaffCosts = 3000m, Overhead = 1000m },
                new PactPayDto { Year = Year, Project = Project, Month = 2, Pay = 5500m, NonPay = 2200m, StaffCosts = 3200m, Overhead = 1100m }
            };
            var expectedResponse = ApiResponseDto<List<PactPayDto>>.SuccessResponse(pactPay);

            _pimsProjectYearCostsApiClient.GetPactPayAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetPactPayAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectYearCostsApiClient.Received(1).GetPactPayAsync(Project, Year, query);
        }

        [Fact]
        public async Task GetPactPayAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<PactPayDto>>.SuccessResponse(new List<PactPayDto>());

            _pimsProjectYearCostsApiClient.GetPactPayAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetPactPayAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPactPayAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Pact pay data not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<List<PactPayDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetPactPayAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetPactPayAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetPactPayAsync_PassesCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 12, Search = "Pay", SortBy = "Month", Descending = false };
            var expectedResponse = ApiResponseDto<List<PactPayDto>>.SuccessResponse(new List<PactPayDto>());

            _pimsProjectYearCostsApiClient.GetPactPayAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetPactPayAsync(Project, Year, query);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetPactPayAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year),
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 12 &&
                    q.Search == "Pay" &&
                    q.SortBy == "Month" &&
                    q.Descending == false
                )
            );
        }

        #endregion

        #region GetMonthlyPactDataAsync Tests

        [Fact]
        public async Task GetMonthlyPactDataAsync_WithSuccessResponse_ReturnsMonthlyPactList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var monthlyPact = new List<MonthlyPactDto>
            {
                new MonthlyPactDto { Year = Year, Project = Project, Monthno = 1, Periodname = "April", Totalcost = 10000m },
                new MonthlyPactDto { Year = Year, Project = Project, Monthno = 2, Periodname = "May", Totalcost = 12000m }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyPactDto>>.SuccessResponse(monthlyPact);

            _pimsProjectYearCostsApiClient.GetMonthlyPactDataAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetMonthlyPactDataAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsProjectYearCostsApiClient.Received(1).GetMonthlyPactDataAsync(Project, Year, query);
        }

        [Fact]
        public async Task GetMonthlyPactDataAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<MonthlyPactDto>>.SuccessResponse(new List<MonthlyPactDto>());

            _pimsProjectYearCostsApiClient.GetMonthlyPactDataAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetMonthlyPactDataAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetMonthlyPactDataAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Monthly PACT data not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyPactDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetMonthlyPactDataAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetMonthlyPactDataAsync(Project, Year, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetMonthlyPactDataAsync_PassesCorrectParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 12, Search = "April", SortBy = "Monthno", Descending = false };
            var expectedResponse = ApiResponseDto<List<MonthlyPactDto>>.SuccessResponse(new List<MonthlyPactDto>());

            _pimsProjectYearCostsApiClient.GetMonthlyPactDataAsync(Project, Year, query).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetMonthlyPactDataAsync(Project, Year, query);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetMonthlyPactDataAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year),
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 12 &&
                    q.Search == "April" &&
                    q.SortBy == "Monthno" &&
                    q.Descending == false
                )
            );
        }

        #endregion

        #region GetFpsYearTotalsAsync Tests

        [Fact]
        public async Task GetFpsYearTotalsAsync_WithValidParameters_ReturnsFpsYearTotals()
        {
            // Arrange
            var fpsYearTotals = new FpsYearTotalsDto
            {
                Year = Year,
                Parentproject = Project,
                Totaladditionalcosts = 5000m,
                Totalanimalcosts = 3000,
                Totalstaffcosts = 15000,
                Totaltestcosts = 2000,
                Totalcosts = 25000,
                Custincome = 30000m,
                Transferincome = 5000m,
                Totalincome = 35000m
            };
            var expectedResponse = ApiResponseDto<FpsYearTotalsDto>.SuccessResponse(fpsYearTotals);

            _pimsProjectYearCostsApiClient.GetFpsYearTotalsAsync(Project, Year).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetFpsYearTotalsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(Project, result.Data.Parentproject);
            Assert.Equal(Year, result.Data.Year);
            await _pimsProjectYearCostsApiClient.Received(1).GetFpsYearTotalsAsync(Project, Year);
        }

        [Fact]
        public async Task GetFpsYearTotalsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "FPS year totals not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<FpsYearTotalsDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectYearCostsApiClient.GetFpsYearTotalsAsync(Project, Year).Returns(expectedResponse);

            // Act
            var result = await _projectYearCostsService.GetFpsYearTotalsAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetFpsYearTotalsAsync_PassesCorrectParameters()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<FpsYearTotalsDto>.SuccessResponse(new FpsYearTotalsDto { Parentproject = Project, Year = Year });

            _pimsProjectYearCostsApiClient.GetFpsYearTotalsAsync(Project, Year).Returns(expectedResponse);

            // Act
            await _projectYearCostsService.GetFpsYearTotalsAsync(Project, Year);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).GetFpsYearTotalsAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year)
            );
        }

        #endregion

        #region ExportProjectYearCostsToExcelAsync Tests

        [Fact]
        public async Task ExportProjectYearCostsToExcelAsync_ReturnsExcelBytes()
        {
            // Arrange
            var expectedBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // ZIP/XLSX magic bytes
            _pimsProjectYearCostsApiClient.ExportProjectYearCostsToExcelAsync(Project, Year).Returns(Task.FromResult(expectedBytes));

            // Act
            var result = await _projectYearCostsService.ExportProjectYearCostsToExcelAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(expectedBytes, result);
            await _pimsProjectYearCostsApiClient.Received(1).ExportProjectYearCostsToExcelAsync(Project, Year);
        }

        [Fact]
        public async Task ExportProjectYearCostsToExcelAsync_WhenNoData_ReturnsEmptyByteArray()
        {
            // Arrange
            var expectedBytes = Array.Empty<byte>();
            _pimsProjectYearCostsApiClient.ExportProjectYearCostsToExcelAsync(Project, Year).Returns(Task.FromResult(expectedBytes));

            // Act
            var result = await _projectYearCostsService.ExportProjectYearCostsToExcelAsync(Project, Year);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExportProjectYearCostsToExcelAsync_PassesCorrectParameters()
        {
            // Arrange
            _pimsProjectYearCostsApiClient.ExportProjectYearCostsToExcelAsync(Project, Year).Returns(Task.FromResult(new byte[] { 1, 2, 3 }));

            // Act
            await _projectYearCostsService.ExportProjectYearCostsToExcelAsync(Project, Year);

            // Assert
            await _pimsProjectYearCostsApiClient.Received(1).ExportProjectYearCostsToExcelAsync(
                Arg.Is<string>(p => p == Project),
                Arg.Is<short>(y => y == Year)
            );
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            // Arrange & Act
            var service = new ProjectYearCostsService(_pimsApiClient);

            // Assert
            Assert.NotNull(service);
        }

        #endregion
    }
}
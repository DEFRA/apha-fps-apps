using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.ProjectYearCostsServiceTest
{
    public class ProjectYearCostsServiceTests
    {
        private readonly IProjectYearCostsRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectYearCostsService _sut;

        private const string TestProject = "PP001";
        private const short TestYear = 2024;

        public ProjectYearCostsServiceTests()
        {
            _mockRepository = Substitute.For<IProjectYearCostsRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectYearCostsService(_mockRepository, _mockMapper);
        }

        private static PaginationData MakePaginationData(int totalRecords = 2) =>
            new() { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = totalRecords };

        private static PaginationData EmptyPaginationData() =>
            new() { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 };

        private static PaginationParameters<string> MakePaging() =>
            new(page: 1, pageSize: 10);

        #region GetAdditionalActualsAsync

        [Fact]
        public async Task GetAdditionalActualsAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var paging = MakePaging();
            var entities = new List<ProjSubContract>
            {
                new() { Year = TestYear, Subcontcounter = 1, Project = TestProject, Month = 1d, Amount = 500m, Description = "Sub1", Supplier = "Sup1" },
                new() { Year = TestYear, Subcontcounter = 2, Project = TestProject, Month = 2d, Amount = 750m, Description = "Sub2", Supplier = "Sup2" }
            };
            var pagedData = new PagedData<ProjSubContract>(entities, MakePaginationData());
            var expectedDtos = new List<AdditionalCostDto>
            {
                new() { Year = TestYear, Amount = 500m, Description = "Sub1" },
                new() { Year = TestYear, Amount = 750m, Description = "Sub2" }
            };

            _mockRepository.GetAdditionalActualsAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<AdditionalCostDto>>(pagedData.Data).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAdditionalActualsAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeEquivalentTo(expectedDtos);
            result.PaginationData.TotalRecords.Should().Be(2);
            result.PaginationData.PageNumber.Should().Be(1);

            await _mockRepository.Received(1).GetAdditionalActualsAsync(TestProject, TestYear, paging);
            _mockMapper.Received(1).Map<List<AdditionalCostDto>>(pagedData.Data);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_WithEmptyList_ReturnsMappedEmptyResult()
        {
            // Arrange
            var paging = MakePaging();
            var pagedData = new PagedData<ProjSubContract>(new List<ProjSubContract>(), EmptyPaginationData());
            var emptyDtos = new List<AdditionalCostDto>();

            _mockRepository.GetAdditionalActualsAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<AdditionalCostDto>>(pagedData.Data).Returns(emptyDtos);

            // Act
            var result = await _sut.GetAdditionalActualsAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetAdditionalActualsAsync(TestProject, TestYear, paging);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var paging = MakePaging();
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAdditionalActualsAsync(TestProject, TestYear, paging)
                .Returns(Task.FromException<PagedData<ProjSubContract>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAdditionalActualsAsync(TestProject, TestYear, paging));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetAdditionalActualsAsync(TestProject, TestYear, paging);
            _mockMapper.DidNotReceive().Map<List<AdditionalCostDto>>(Arg.Any<IReadOnlyCollection<ProjSubContract>>());
        }

        #endregion

        #region GetAdditionalPlansAsync

        [Fact]
        public async Task GetAdditionalPlansAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var paging = MakePaging();
            var entities = new List<AdditionalCosts>
            {
                new() { Year = TestYear, Jobcode = "JC001", Account = "ACC001", Description = "Plan1", Itemcost = 1000m },
                new() { Year = TestYear, Jobcode = "JC002", Account = "ACC002", Description = "Plan2", Itemcost = 2000m }
            };
            var pagedData = new PagedData<AdditionalCosts>(entities, MakePaginationData());
            var expectedDtos = new List<AdditionalCostDto>
            {
                new() { Year = TestYear, JobCode = "JC001", Account = "ACC001", Description = "Plan1", ItemCost = 1000m },
                new() { Year = TestYear, JobCode = "JC002", Account = "ACC002", Description = "Plan2", ItemCost = 2000m }
            };

            _mockRepository.GetAdditionalPlansAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<AdditionalCostDto>>(pagedData.Data).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAdditionalPlansAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().JobCode.Should().Be("JC001");
            result.Data.Should().BeEquivalentTo(expectedDtos);
            result.PaginationData.TotalRecords.Should().Be(2);

            await _mockRepository.Received(1).GetAdditionalPlansAsync(TestProject, TestYear, paging);
            _mockMapper.Received(1).Map<List<AdditionalCostDto>>(pagedData.Data);
        }

        [Fact]
        public async Task GetAdditionalPlansAsync_WithEmptyList_ReturnsMappedEmptyResult()
        {
            // Arrange
            var paging = MakePaging();
            var pagedData = new PagedData<AdditionalCosts>(new List<AdditionalCosts>(), EmptyPaginationData());
            var emptyDtos = new List<AdditionalCostDto>();

            _mockRepository.GetAdditionalPlansAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<AdditionalCostDto>>(pagedData.Data).Returns(emptyDtos);

            // Act
            var result = await _sut.GetAdditionalPlansAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetAdditionalPlansAsync(TestProject, TestYear, paging);
        }

        [Fact]
        public async Task GetAdditionalPlansAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var paging = MakePaging();
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAdditionalPlansAsync(TestProject, TestYear, paging)
                .Returns(Task.FromException<PagedData<AdditionalCosts>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAdditionalPlansAsync(TestProject, TestYear, paging));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetAdditionalPlansAsync(TestProject, TestYear, paging);
            _mockMapper.DidNotReceive().Map<List<AdditionalCostDto>>(Arg.Any<IReadOnlyCollection<AdditionalCosts>>());
        }

        #endregion

        #region GetAnimalActualsAsync

        [Fact]
        public async Task GetAnimalActualsAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var paging = MakePaging();
            var entities = new List<ProjSubContract>
            {
                new() { Year = TestYear, Subcontcounter = 1, Project = TestProject, Acctcode = "LA001", DailyRate = 50m, AnimalDays = 10, Amount = 500m },
                new() { Year = TestYear, Subcontcounter = 2, Project = TestProject, Acctcode = "SA001", DailyRate = 30m, AnimalDays = 5,  Amount = 150m }
            };
            var pagedData = new PagedData<ProjSubContract>(entities, MakePaginationData());
            var expectedDtos = new List<AnimalCostDto>
            {
                new() { Year = TestYear, AcctCode = "LA001", DailyRate = 50m, AnimalDays = 10, Amount = 500m },
                new() { Year = TestYear, AcctCode = "SA001", DailyRate = 30m, AnimalDays = 5,  Amount = 150m }
            };

            _mockRepository.GetAnimalActualsAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<AnimalCostDto>>(pagedData.Data).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAnimalActualsAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeEquivalentTo(expectedDtos);
            result.PaginationData.TotalRecords.Should().Be(2);

            await _mockRepository.Received(1).GetAnimalActualsAsync(TestProject, TestYear, paging);
            _mockMapper.Received(1).Map<List<AnimalCostDto>>(pagedData.Data);
        }

        [Fact]
        public async Task GetAnimalActualsAsync_WithEmptyList_ReturnsMappedEmptyResult()
        {
            // Arrange
            var paging = MakePaging();
            var pagedData = new PagedData<ProjSubContract>(new List<ProjSubContract>(), EmptyPaginationData());
            var emptyDtos = new List<AnimalCostDto>();

            _mockRepository.GetAnimalActualsAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<AnimalCostDto>>(pagedData.Data).Returns(emptyDtos);

            // Act
            var result = await _sut.GetAnimalActualsAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetAnimalActualsAsync(TestProject, TestYear, paging);
        }

        [Fact]
        public async Task GetAnimalActualsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var paging = MakePaging();
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAnimalActualsAsync(TestProject, TestYear, paging)
                .Returns(Task.FromException<PagedData<ProjSubContract>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAnimalActualsAsync(TestProject, TestYear, paging));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetAnimalActualsAsync(TestProject, TestYear, paging);
            _mockMapper.DidNotReceive().Map<List<AnimalCostDto>>(Arg.Any<IReadOnlyCollection<ProjSubContract>>());
        }

        #endregion

        #region GetAnimalPlansAsync

        [Fact]
        public async Task GetAnimalPlansAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var paging = MakePaging();
            var entities = new List<ProjectAnimalPlan>
            {
                new() { Year = TestYear, Parentproject = TestProject, Animaltype = "Cattle",  Numberofdays = 10d, Numberofanimals = 5d,  Rate = 20m, Cost = 1000m },
                new() { Year = TestYear, Parentproject = TestProject, Animaltype = "Rabbits", Numberofdays = 5d,  Numberofanimals = 20d, Rate = 10m, Cost = 1000m }
            };
            var pagedData = new PagedData<ProjectAnimalPlan>(entities, MakePaginationData());
            var expectedDtos = new List<AnimalCostDto>
            {
                new() { AnimalType = "Cattle",  NumberOfDays = 10d, Rate = 20m, Cost = 1000d },
                new() { AnimalType = "Rabbits", NumberOfDays = 5d,  Rate = 10m, Cost = 1000d }
            };

            _mockRepository.GetAnimalPlansAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<AnimalCostDto>>(pagedData.Data).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAnimalPlansAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().AnimalType.Should().Be("Cattle");
            result.Data.Should().BeEquivalentTo(expectedDtos);
            result.PaginationData.TotalRecords.Should().Be(2);

            await _mockRepository.Received(1).GetAnimalPlansAsync(TestProject, TestYear, paging);
            _mockMapper.Received(1).Map<List<AnimalCostDto>>(pagedData.Data);
        }

        [Fact]
        public async Task GetAnimalPlansAsync_WithEmptyList_ReturnsMappedEmptyResult()
        {
            // Arrange
            var paging = MakePaging();
            var pagedData = new PagedData<ProjectAnimalPlan>(new List<ProjectAnimalPlan>(), EmptyPaginationData());
            var emptyDtos = new List<AnimalCostDto>();

            _mockRepository.GetAnimalPlansAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<AnimalCostDto>>(pagedData.Data).Returns(emptyDtos);

            // Act
            var result = await _sut.GetAnimalPlansAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetAnimalPlansAsync(TestProject, TestYear, paging);
        }

        [Fact]
        public async Task GetAnimalPlansAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var paging = MakePaging();
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAnimalPlansAsync(TestProject, TestYear, paging)
                .Returns(Task.FromException<PagedData<ProjectAnimalPlan>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAnimalPlansAsync(TestProject, TestYear, paging));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetAnimalPlansAsync(TestProject, TestYear, paging);
            _mockMapper.DidNotReceive().Map<List<AnimalCostDto>>(Arg.Any<IReadOnlyCollection<ProjectAnimalPlan>>());
        }

        #endregion

        #region GetTestPlansAsync

        [Fact]
        public async Task GetTestPlansAsync_WithValidData_ReturnsManuallymappedPaginatedResult()
        {
            // Arrange
            var paging = MakePaging();
            var entities = new List<TestReqmt>
            {
                new() { Year = TestYear, Testcode = "TC001", Buyer = "BUY1", Unitprice = 100m, Norequired = 2.0d },
                new() { Year = TestYear, Testcode = "TC002", Buyer = "BUY2", Unitprice = null,  Norequired = 3.0d }
            };
            var pagedData = new PagedData<TestReqmt>(entities, MakePaginationData());

            _mockRepository.GetTestPlansAsync(TestProject, TestYear, paging).Returns(pagedData);

            // Act
            var result = await _sut.GetTestPlansAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);

            var items = result.Data.ToList();
            items[0].Year.Should().Be(TestYear);
            items[0].TestCode.Should().Be("TC001");
            items[0].Buyer.Should().Be("BUY1");
            items[0].UnitPrice.Should().Be(100m);
            items[0].NoRequired.Should().Be(2.0d);
            items[0].Cost.Should().Be(200m); // 100m * 2.0d

            items[1].Cost.Should().BeNull(); // Unitprice is null → Cost is null

            await _mockRepository.Received(1).GetTestPlansAsync(TestProject, TestYear, paging);
            _mockMapper.DidNotReceive().Map<List<TestCostDto>>(Arg.Any<object>());
        }

        [Fact]
        public async Task GetTestPlansAsync_WithEmptyList_ReturnsEmptyResult()
        {
            // Arrange
            var paging = MakePaging();
            var pagedData = new PagedData<TestReqmt>(new List<TestReqmt>(), EmptyPaginationData());

            _mockRepository.GetTestPlansAsync(TestProject, TestYear, paging).Returns(pagedData);

            // Act
            var result = await _sut.GetTestPlansAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetTestPlansAsync(TestProject, TestYear, paging);
        }

        [Fact]
        public async Task GetTestPlansAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var paging = MakePaging();
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetTestPlansAsync(TestProject, TestYear, paging)
                .Returns(Task.FromException<PagedData<TestReqmt>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetTestPlansAsync(TestProject, TestYear, paging));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetTestPlansAsync(TestProject, TestYear, paging);
        }

        #endregion

        #region GetTestActualsAsync

        [Fact]
        public async Task GetTestActualsAsync_WithValidData_ReturnsManuallymappedPaginatedResult()
        {
            // Arrange
            var paging = MakePaging();
            var output1 = new MonthlyOutput { Year = TestYear, Testcode = "TC001", Buyer = "BUY1", Month = 1d, Workgroup = "WG1", Volume = 5.0d };
            var reqmt1  = new TestReqmt   { Year = TestYear, Testcode = "TC001", Buyer = "BUY1", Unitprice = 50m };
            var output2 = new MonthlyOutput { Year = TestYear, Testcode = "TC002", Buyer = "BUY2", Month = 2d, Workgroup = "WG2", Volume = null };
            var reqmt2  = new TestReqmt   { Year = TestYear, Testcode = "TC002", Buyer = "BUY2", Unitprice = 80m };

            var entities = new List<(MonthlyOutput Output, TestReqmt Reqmt)>
            {
                (output1, reqmt1),
                (output2, reqmt2)
            };
            var pagedData = new PagedData<(MonthlyOutput Output, TestReqmt Reqmt)>(entities, MakePaginationData());

            _mockRepository.GetTestActualsAsync(TestProject, TestYear, paging).Returns(pagedData);

            // Act
            var result = await _sut.GetTestActualsAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);

            var items = result.Data.ToList();
            items[0].TestCode.Should().Be("TC001");
            items[0].Buyer.Should().Be("BUY1");
            items[0].Month.Should().Be(1d);
            items[0].WorkGroup.Should().Be("WG1");
            items[0].Volume.Should().Be(5.0d);
            items[0].UnitPrice.Should().Be(50m);
            items[0].Charge.Should().Be(250m); // 50m * 5.0d

            items[1].Charge.Should().BeNull(); // Volume is null → Charge is null

            await _mockRepository.Received(1).GetTestActualsAsync(TestProject, TestYear, paging);
            _mockMapper.DidNotReceive().Map<List<TestCostDto>>(Arg.Any<object>());
        }

        [Fact]
        public async Task GetTestActualsAsync_WithEmptyList_ReturnsEmptyResult()
        {
            // Arrange
            var paging = MakePaging();
            var pagedData = new PagedData<(MonthlyOutput Output, TestReqmt Reqmt)>(
                new List<(MonthlyOutput, TestReqmt)>(), EmptyPaginationData());

            _mockRepository.GetTestActualsAsync(TestProject, TestYear, paging).Returns(pagedData);

            // Act
            var result = await _sut.GetTestActualsAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetTestActualsAsync(TestProject, TestYear, paging);
        }

        [Fact]
        public async Task GetTestActualsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var paging = MakePaging();
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetTestActualsAsync(TestProject, TestYear, paging)
                .Returns(Task.FromException<PagedData<(MonthlyOutput Output, TestReqmt Reqmt)>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetTestActualsAsync(TestProject, TestYear, paging));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetTestActualsAsync(TestProject, TestYear, paging);
        }

        #endregion

        #region GetStaffPlansAsync

        [Fact]
        public async Task GetStaffPlansAsync_WithValidData_ReturnsManuallymappedPaginatedResult()
        {
            // Arrange
            var paging = MakePaging();
            var entities = new List<ProjectStaffPlan>
            {
                new() { Year = TestYear, Parentproject = TestProject, Workgroupgrade = "WG1-GR5", Name = "Alice", Plannedhours = 100d, Rate = 45.00m, Cost = 4500m },
                new() { Year = TestYear, Parentproject = TestProject, Workgroupgrade = "WG2-GR3", Name = "Bob",   Plannedhours = 80d,  Rate = 35.00m, Cost = 2800m }
            };
            var pagedData = new PagedData<ProjectStaffPlan>(entities, MakePaginationData());

            _mockRepository.GetStaffPlansAsync(TestProject, TestYear, paging).Returns(pagedData);

            // Act
            var result = await _sut.GetStaffPlansAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);

            var items = result.Data.ToList();
            items[0].Year.Should().Be(TestYear);
            items[0].ParentProject.Should().Be(TestProject);
            items[0].WgGrade.Should().Be("WG1-GR5");
            items[0].Name.Should().Be("Alice");
            items[0].PlannedHours.Should().Be(100d);
            items[0].Rate.Should().Be(45.00m);
            items[0].Cost.Should().Be(4500m);

            items[1].Name.Should().Be("Bob");
            items[1].Cost.Should().Be(2800m);

            await _mockRepository.Received(1).GetStaffPlansAsync(TestProject, TestYear, paging);
            _mockMapper.DidNotReceive().Map<List<StaffCostDto>>(Arg.Any<object>());
        }

        [Fact]
        public async Task GetStaffPlansAsync_WithEmptyList_ReturnsEmptyResult()
        {
            // Arrange
            var paging = MakePaging();
            var pagedData = new PagedData<ProjectStaffPlan>(new List<ProjectStaffPlan>(), EmptyPaginationData());

            _mockRepository.GetStaffPlansAsync(TestProject, TestYear, paging).Returns(pagedData);

            // Act
            var result = await _sut.GetStaffPlansAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetStaffPlansAsync(TestProject, TestYear, paging);
        }

        [Fact]
        public async Task GetStaffPlansAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var paging = MakePaging();
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetStaffPlansAsync(TestProject, TestYear, paging)
                .Returns(Task.FromException<PagedData<ProjectStaffPlan>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetStaffPlansAsync(TestProject, TestYear, paging));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetStaffPlansAsync(TestProject, TestYear, paging);
        }

        #endregion

        #region GetStaffActualsAsync

        [Fact]
        public async Task GetStaffActualsAsync_WithValidData_ReturnsManuallymappedPaginatedResult()
        {
            // Arrange
            var paging = MakePaging();
            var entities = new List<TimeCostCalcs>
            {
                new() { Year = TestYear, Jobcode = "JC001", Name = "Alice", Workgroup = "WG1", Gradecode = "GR5", Month = 1d, Time = 10.0d, Chargerate = 50.25m, Staffid = "S001", Project = TestProject },
                new() { Year = TestYear, Jobcode = "JC002", Name = "Bob",   Workgroup = "WG2", Gradecode = "GR3", Month = 2d, Time = null,  Chargerate = null,    Staffid = "S002", Project = TestProject }
            };
            var pagedData = new PagedData<TimeCostCalcs>(entities, MakePaginationData());

            _mockRepository.GetStaffActualsAsync(TestProject, TestYear, paging).Returns(pagedData);

            // Act
            var result = await _sut.GetStaffActualsAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);

            var items = result.Data.ToList();
            items[0].JobCode.Should().Be("JC001");
            items[0].Name.Should().Be("Alice");
            items[0].WorkGroup.Should().Be("WG1");
            items[0].GradeCode.Should().Be("GR5");
            items[0].Month.Should().Be(1d);
            items[0].Time.Should().Be(10.0d);
            items[0].ChargeRate.Should().Be(50.25m);
            items[0].ActualCost.Should().Be(Math.Round((decimal)10.0d * 50.25m, 2)); // 502.50m

            items[1].ActualCost.Should().BeNull(); // Time and ChargeRate are null

            await _mockRepository.Received(1).GetStaffActualsAsync(TestProject, TestYear, paging);
            _mockMapper.DidNotReceive().Map<List<StaffCostDto>>(Arg.Any<object>());
        }

        [Fact]
        public async Task GetStaffActualsAsync_WithEmptyList_ReturnsEmptyResult()
        {
            // Arrange
            var paging = MakePaging();
            var pagedData = new PagedData<TimeCostCalcs>(new List<TimeCostCalcs>(), EmptyPaginationData());

            _mockRepository.GetStaffActualsAsync(TestProject, TestYear, paging).Returns(pagedData);

            // Act
            var result = await _sut.GetStaffActualsAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetStaffActualsAsync(TestProject, TestYear, paging);
        }

        [Fact]
        public async Task GetStaffActualsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var paging = MakePaging();
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetStaffActualsAsync(TestProject, TestYear, paging)
                .Returns(Task.FromException<PagedData<TimeCostCalcs>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetStaffActualsAsync(TestProject, TestYear, paging));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetStaffActualsAsync(TestProject, TestYear, paging);
        }

        #endregion

        #region GetProjectYearDetailsAsync

        [Fact]
        public async Task GetProjectYearDetailsAsync_WithExistingEntity_ReturnsMappedDto()
        {
            // Arrange
            var entity = new Projects
            {
                Year = TestYear,
                Parentproject = TestProject,
                Manager = "MGR1",
                Customer = "CUST1",
                Program = "PROG1"
            };
            var expectedDto = new ProjectYearDetailsDto
            {
                Year = TestYear,
                Parentproject = TestProject,
                Manager = "MGR1"
            };

            _mockRepository.GetProjectYearDetailsAsync(TestProject, TestYear)
                .Returns(Task.FromResult<Projects?>(entity));
            _mockMapper.Map<ProjectYearDetailsDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetProjectYearDetailsAsync(TestProject, TestYear);

            // Assert
            result.Should().NotBeNull();
            result.Parentproject.Should().Be(TestProject);
            result.Year.Should().Be(TestYear);
            result.Manager.Should().Be("MGR1");

            await _mockRepository.Received(1).GetProjectYearDetailsAsync(TestProject, TestYear);
            _mockMapper.Received(1).Map<ProjectYearDetailsDto>(entity);
        }

        [Fact]
        public async Task GetProjectYearDetailsAsync_WhenEntityIsNull_ReturnsEmptyDto()
        {
            // Arrange
            _mockRepository.GetProjectYearDetailsAsync(TestProject, TestYear)
                .Returns(Task.FromResult<Projects?>(null));

            // Act
            var result = await _sut.GetProjectYearDetailsAsync(TestProject, TestYear);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ProjectYearDetailsDto>();
            result.Parentproject.Should().BeNull();

            await _mockRepository.Received(1).GetProjectYearDetailsAsync(TestProject, TestYear);
            _mockMapper.DidNotReceive().Map<ProjectYearDetailsDto>(Arg.Any<Projects>());
        }

        [Fact]
        public async Task GetProjectYearDetailsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetProjectYearDetailsAsync(TestProject, TestYear)
                .Returns(Task.FromException<Projects?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetProjectYearDetailsAsync(TestProject, TestYear));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetProjectYearDetailsAsync(TestProject, TestYear);
            _mockMapper.DidNotReceive().Map<ProjectYearDetailsDto>(Arg.Any<Projects>());
        }

        #endregion

        #region GetFpsYearTotalsAsync

        [Fact]
        public async Task GetFpsYearTotalsAsync_WithExistingEntity_ReturnsMappedDto()
        {
            // Arrange
            var entity = new FpsYearTotal
            {
                Year = TestYear,
                Parentproject = TestProject,
                Program = "PROG1",
                Totalstaffcosts = 10000d,
                Custincome = 20000m,
                Transferincome = 5000m,
                Totalincome = 25000m,
                Projectstatus = "Active"
            };
            var expectedDto = new FpsYearTotalsDto
            {
                Year = TestYear,
                Parentproject = TestProject,
                Totalstaffcosts = 10000d,
                Custincome = 20000m,
                Totalincome = 25000m
            };

            _mockRepository.GetFpsYearTotalsAsync(TestProject, TestYear)
                .Returns(Task.FromResult<FpsYearTotal?>(entity));
            _mockMapper.Map<FpsYearTotalsDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetFpsYearTotalsAsync(TestProject, TestYear);

            // Assert
            result.Should().NotBeNull();
            result!.Year.Should().Be(TestYear);
            result.Parentproject.Should().Be(TestProject);
            result.Totalstaffcosts.Should().Be(10000d);
            result.Totalincome.Should().Be(25000m);

            await _mockRepository.Received(1).GetFpsYearTotalsAsync(TestProject, TestYear);
            _mockMapper.Received(1).Map<FpsYearTotalsDto>(entity);
        }

        [Fact]
        public async Task GetFpsYearTotalsAsync_WhenEntityIsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetFpsYearTotalsAsync(TestProject, TestYear)
                .Returns(Task.FromResult<FpsYearTotal?>(null));

            // Act
            var result = await _sut.GetFpsYearTotalsAsync(TestProject, TestYear);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetFpsYearTotalsAsync(TestProject, TestYear);
            _mockMapper.DidNotReceive().Map<FpsYearTotalsDto>(Arg.Any<FpsYearTotal>());
        }

        [Fact]
        public async Task GetFpsYearTotalsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetFpsYearTotalsAsync(TestProject, TestYear)
                .Returns(Task.FromException<FpsYearTotal?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetFpsYearTotalsAsync(TestProject, TestYear));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetFpsYearTotalsAsync(TestProject, TestYear);
            _mockMapper.DidNotReceive().Map<FpsYearTotalsDto>(Arg.Any<FpsYearTotal>());
        }

        #endregion

        #region GetPactPayAsync

        [Fact]
        public async Task GetPactPayAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var paging = MakePaging();
            var entities = new List<PactPayCalc>
            {
                new() { Year = TestYear, Project = TestProject, Month = 1d, Pay = 3000m, NonPay = 500m, StaffCosts = 3500m, Overhead = 700m },
                new() { Year = TestYear, Project = TestProject, Month = 2d, Pay = 4000m, NonPay = 600m, StaffCosts = 4600m, Overhead = 920m }
            };
            var pagedData = new PagedData<PactPayCalc>(entities, MakePaginationData());
            var expectedDtos = new List<PactPayDto>
            {
                new() { Year = TestYear, Project = TestProject, Month = 1d, Pay = 3000m, NonPay = 500m, StaffCosts = 3500m, Overhead = 700m },
                new() { Year = TestYear, Project = TestProject, Month = 2d, Pay = 4000m, NonPay = 600m, StaffCosts = 4600m, Overhead = 920m }
            };

            _mockRepository.GetPactPayAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<PactPayDto>>(pagedData.Data).Returns(expectedDtos);

            // Act
            var result = await _sut.GetPactPayAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Pay.Should().Be(3000m);
            result.Data.Should().BeEquivalentTo(expectedDtos);
            result.PaginationData.TotalRecords.Should().Be(2);

            await _mockRepository.Received(1).GetPactPayAsync(TestProject, TestYear, paging);
            _mockMapper.Received(1).Map<List<PactPayDto>>(pagedData.Data);
        }

        [Fact]
        public async Task GetPactPayAsync_WithEmptyList_ReturnsMappedEmptyResult()
        {
            // Arrange
            var paging = MakePaging();
            var pagedData = new PagedData<PactPayCalc>(new List<PactPayCalc>(), EmptyPaginationData());
            var emptyDtos = new List<PactPayDto>();

            _mockRepository.GetPactPayAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<PactPayDto>>(pagedData.Data).Returns(emptyDtos);

            // Act
            var result = await _sut.GetPactPayAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetPactPayAsync(TestProject, TestYear, paging);
        }

        [Fact]
        public async Task GetPactPayAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var paging = MakePaging();
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetPactPayAsync(TestProject, TestYear, paging)
                .Returns(Task.FromException<PagedData<PactPayCalc>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPactPayAsync(TestProject, TestYear, paging));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetPactPayAsync(TestProject, TestYear, paging);
            _mockMapper.DidNotReceive().Map<List<PactPayDto>>(Arg.Any<IReadOnlyCollection<PactPayCalc>>());
        }

        #endregion

        #region GetMonthlyPactDataAsync

        [Fact]
        public async Task GetMonthlyPactDataAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var paging = MakePaging();
            var entities = new List<ProjectMonthFinal>
            {
                new() { Year = TestYear, Project = TestProject, Monthno = 1d, Periodname = "Apr", Totalcost = 5000m, Timecosts = 3500m },
                new() { Year = TestYear, Project = TestProject, Monthno = 2d, Periodname = "May", Totalcost = 6000m, Timecosts = 4000m }
            };
            var pagedData = new PagedData<ProjectMonthFinal>(entities, MakePaginationData());
            var expectedDtos = new List<MonthlyPactDto>
            {
                new() { Year = TestYear, Project = TestProject, Monthno = 1d, Periodname = "Apr", Totalcost = 5000m },
                new() { Year = TestYear, Project = TestProject, Monthno = 2d, Periodname = "May", Totalcost = 6000m }
            };

            _mockRepository.GetMonthlyPactDataAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<MonthlyPactDto>>(pagedData.Data).Returns(expectedDtos);

            // Act
            var result = await _sut.GetMonthlyPactDataAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Periodname.Should().Be("Apr");
            result.Data.Should().BeEquivalentTo(expectedDtos);
            result.PaginationData.TotalRecords.Should().Be(2);

            await _mockRepository.Received(1).GetMonthlyPactDataAsync(TestProject, TestYear, paging);
            _mockMapper.Received(1).Map<List<MonthlyPactDto>>(pagedData.Data);
        }

        [Fact]
        public async Task GetMonthlyPactDataAsync_WithEmptyList_ReturnsMappedEmptyResult()
        {
            // Arrange
            var paging = MakePaging();
            var pagedData = new PagedData<ProjectMonthFinal>(new List<ProjectMonthFinal>(), EmptyPaginationData());
            var emptyDtos = new List<MonthlyPactDto>();

            _mockRepository.GetMonthlyPactDataAsync(TestProject, TestYear, paging).Returns(pagedData);
            _mockMapper.Map<List<MonthlyPactDto>>(pagedData.Data).Returns(emptyDtos);

            // Act
            var result = await _sut.GetMonthlyPactDataAsync(TestProject, TestYear, paging);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetMonthlyPactDataAsync(TestProject, TestYear, paging);
        }

        [Fact]
        public async Task GetMonthlyPactDataAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var paging = MakePaging();
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetMonthlyPactDataAsync(TestProject, TestYear, paging)
                .Returns(Task.FromException<PagedData<ProjectMonthFinal>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetMonthlyPactDataAsync(TestProject, TestYear, paging));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetMonthlyPactDataAsync(TestProject, TestYear, paging);
            _mockMapper.DidNotReceive().Map<List<MonthlyPactDto>>(Arg.Any<IReadOnlyCollection<ProjectMonthFinal>>());
        }

        #endregion

        #region ExportProjectYearCostsToExcelAsync

        [Fact]
        public async Task ExportProjectYearCostsToExcelAsync_WithValidData_ReturnsNonEmptyByteArray()
        {
            // Arrange
            var staffPlans = new List<ProjectStaffPlan>
            {
                new() { Year = TestYear, Parentproject = TestProject, Workgroupgrade = "WG1-GR5", Name = "Alice", Plannedhours = 100d, Rate = 45m, Cost = 4500m }
            };
            var staffActuals = new List<TimeCostCalcs>
            {
                new() { Year = TestYear, Jobcode = "JC001", Name = "Alice", Workgroup = "WG1", Gradecode = "GR5", Month = 1d, Time = 10d, Chargerate = 50m, Staffid = "S001", Project = TestProject }
            };
            var testPlans = new List<TestReqmt>
            {
                new() { Year = TestYear, Testcode = "TC001", Buyer = "BUY1", Unitprice = 100m, Norequired = 2d }
            };
            var testActuals = new List<(MonthlyOutput Output, TestReqmt Reqmt)>
            {
                (new MonthlyOutput { Year = TestYear, Testcode = "TC001", Buyer = "BUY1", Month = 1d, Workgroup = "WG1", Volume = 5d },
                 new TestReqmt    { Year = TestYear, Testcode = "TC001", Buyer = "BUY1", Unitprice = 50m })
            };
            var animalPlans = new List<ProjectAnimalPlan>
            {
                new() { Year = TestYear, Parentproject = TestProject, Animaltype = "Cattle", Numberofdays = 10d, Numberofanimals = 5d, Rate = 20m, Cost = 1000m }
            };
            var animalActuals = new List<ProjSubContract>
            {
                new() { Year = TestYear, Subcontcounter = 1, Project = TestProject, Acctcode = "LA001", Month = 1d, DailyRate = 50m, AnimalDays = 10, Amount = 500m, Description = "Cattle housing" }
            };
            var additionalPlans = new List<AdditionalCosts>
            {
                new() { Year = TestYear, Jobcode = "JC001", Account = "MISC", Description = "Equipment", Itemcost = 300m }
            };
            var additionalActuals = new List<ProjSubContract>
            {
                new() { Year = TestYear, Subcontcounter = 2, Project = TestProject, Acctcode = "MISC", Month = 1d, Amount = 300m, Description = "Equipment", Supplier = "SupplierX" }
            };

            _mockRepository.GetStaffPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<ProjectStaffPlan>(staffPlans, MakePaginationData(1)));
            _mockRepository.GetStaffActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<TimeCostCalcs>(staffActuals, MakePaginationData(1)));
            _mockRepository.GetTestPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<TestReqmt>(testPlans, MakePaginationData(1)));
            _mockRepository.GetTestActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<(MonthlyOutput Output, TestReqmt Reqmt)>(testActuals, MakePaginationData(1)));
            _mockRepository.GetAnimalPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<ProjectAnimalPlan>(animalPlans, MakePaginationData(1)));
            _mockRepository.GetAnimalActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<ProjSubContract>(animalActuals, MakePaginationData(1)));
            _mockRepository.GetAdditionalPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<AdditionalCosts>(additionalPlans, MakePaginationData(1)));
            _mockRepository.GetAdditionalActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<ProjSubContract>(additionalActuals, MakePaginationData(1)));

            // Act
            var result = await _sut.ExportProjectYearCostsToExcelAsync(TestProject, TestYear);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();

            await _mockRepository.Received(1).GetStaffPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetStaffActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetTestPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetTestActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetAnimalPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetAnimalActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetAdditionalPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetAdditionalActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
        }

        [Fact]
        public async Task ExportProjectYearCostsToExcelAsync_WithEmptyData_ReturnsValidByteArray()
        {
            // Arrange
            _mockRepository.GetStaffPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<ProjectStaffPlan>(new List<ProjectStaffPlan>(), EmptyPaginationData()));
            _mockRepository.GetStaffActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<TimeCostCalcs>(new List<TimeCostCalcs>(), EmptyPaginationData()));
            _mockRepository.GetTestPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<TestReqmt>(new List<TestReqmt>(), EmptyPaginationData()));
            _mockRepository.GetTestActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<(MonthlyOutput Output, TestReqmt Reqmt)>(new List<(MonthlyOutput, TestReqmt)>(), EmptyPaginationData()));
            _mockRepository.GetAnimalPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<ProjectAnimalPlan>(new List<ProjectAnimalPlan>(), EmptyPaginationData()));
            _mockRepository.GetAnimalActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<ProjSubContract>(new List<ProjSubContract>(), EmptyPaginationData()));
            _mockRepository.GetAdditionalPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<AdditionalCosts>(new List<AdditionalCosts>(), EmptyPaginationData()));
            _mockRepository.GetAdditionalActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(new PagedData<ProjSubContract>(new List<ProjSubContract>(), EmptyPaginationData()));

            // Act
            var result = await _sut.ExportProjectYearCostsToExcelAsync(TestProject, TestYear);

            // Assert — Even with empty sheets the workbook is still a valid Excel file
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();

            await _mockRepository.Received(1).GetStaffPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetStaffActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetTestPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetTestActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetAnimalPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetAnimalActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetAdditionalPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
            await _mockRepository.Received(1).GetAdditionalActualsAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
        }

        [Fact]
        public async Task ExportProjectYearCostsToExcelAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetStaffPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>())
                .Returns(Task.FromException<PagedData<ProjectStaffPlan>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.ExportProjectYearCostsToExcelAsync(TestProject, TestYear));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetStaffPlansAsync(TestProject, TestYear, Arg.Any<PaginationParameters<string>>());
        }

        #endregion
    }
}
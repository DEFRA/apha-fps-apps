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

namespace Apha.FPS.Application.UnitTests.Services.StaffJobServiceTest
{
    public class StaffJobServiceTests
    {
        private readonly IStaffJobRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly StaffJobService _sut;

        public StaffJobServiceTests()
        {
            _mockRepository = Substitute.For<IStaffJobRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new StaffJobService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_WithValidQueryFilter_ReturnsSuccessfulPaginatedResult()
        {
            string jobCode = "JOB001";
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"JobCode\":\"JOB001\"}"
            };

            var mappedPaginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"JobCode\":\"JOB001\"}"
            };

            var repositoryResult = new PagedData<StaffJobView>
            {

                Data = new List<StaffJobView>
                        {
                        new StaffJobView { StaffID = "S001", JobCode = "JOB001", ChargeRate = 75.50m },
                        new StaffJobView { StaffID = "S002", JobCode = "JOB001", ChargeRate = 80.00m }
                        },
                PaginationData = new PaginationData
                {
                    TotalPages = 2,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 2
                }
            };

            var expectedResult = new PaginatedResult<StaffJobViewDto>
            {
                Data = new List<StaffJobViewDto>
                        {
                        new StaffJobViewDto { StaffID = "S001", JobCode = "JOB001", ChargeRate = 75.50m },
                        new StaffJobViewDto { StaffID = "S002", JobCode = "JOB001", ChargeRate = 80.00m }
                        },
                PaginationData = new PaginationDto
                {
                    TotalPages = 2,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 2
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter)
            .Returns(mappedPaginationParams);

            _mockRepository.GetJobStaffCostAsync(mappedPaginationParams, jobCode)
            .Returns(repositoryResult);

            _mockMapper.Map<PaginatedResult<StaffJobViewDto>>(repositoryResult)
            .Returns(expectedResult);

            // Act
            var result = await _sut.GetJobStaffCostAsync(queryFilter, jobCode);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);
            result.PaginationData.PageNumber.Should().Be(1);
            result.PaginationData.PageSize.Should().Be(10);
            result.Data.First().StaffID.Should().Be("S001");
            result.Data.First().ChargeRate.Should().Be(75.50m);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(queryFilter);
            await _mockRepository.Received(1).GetJobStaffCostAsync(mappedPaginationParams, jobCode);
            _mockMapper.Received(1).Map<PaginatedResult<StaffJobViewDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_WithValidQueryFilter_ReturnsEmptyPaginatedResult()
        {
            string jobCode = "NONEXISTENT";
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,                
                Filter = "{\"JobCode\":\"NONEXISTENT\"}"
            };

            var mappedPaginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"JobCode\":\"NONEXISTENT\"}"
            };

            var emptyRepositoryResult = new PagedData<StaffJobView>
            {

                Data = new List<StaffJobView>(),
                PaginationData = new PaginationData
                {
                    TotalPages = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };

            var emptyExpectedResult = new PaginatedResult<StaffJobViewDto>
            {
                Data = new List<StaffJobViewDto>(),
                PaginationData = new PaginationDto
                {
                    TotalPages = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter)
            .Returns(mappedPaginationParams);

            _mockRepository.GetJobStaffCostAsync(mappedPaginationParams, jobCode)
            .Returns(emptyRepositoryResult);

            _mockMapper.Map<PaginatedResult<StaffJobViewDto>>(emptyRepositoryResult)
            .Returns(emptyExpectedResult);

            // Act
            var result = await _sut.GetJobStaffCostAsync(queryFilter, jobCode);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            result.PaginationData.PageNumber.Should().Be(1);
            result.PaginationData.PageSize.Should().Be(10);

            await _mockRepository.Received(1).GetJobStaffCostAsync(mappedPaginationParams, jobCode);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            string jobCode = "";
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var mappedPaginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter)
            .Returns(mappedPaginationParams);

            _mockRepository.GetJobStaffCostAsync(mappedPaginationParams, jobCode)
            .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.GetJobStaffCostAsync(queryFilter, jobCode)
            );

            exception.Message.Should().Be("Database connection failed");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(queryFilter);
            _mockMapper.DidNotReceive().Map<PaginatedResult<StaffJobViewDto>>(Arg.Any<PaginatedResult<StaffJobView>>());
        }


        [Fact]
        public async Task GetStaffWorkgroupLookup_WithValidData_ReturnsMapperDtoList()
        {
            // Arrange
            var staffWorkgroupEntities = new List<StaffWorkgroupLookup>
            {
                new StaffWorkgroupLookup
                {
                     StaffID = "S001",
                    WorkGroupGrade = "WG001",
                    Name = "Engineering"
                },
                new StaffWorkgroupLookup
                {
                     StaffID = "S002",
                    WorkGroupGrade = "WG002",
                    Name = "Design"
                }
            };

            var expectedDtos = new List<StaffWorkgroupLookupDto>
            {
                new StaffWorkgroupLookupDto
                {
                    StaffID = "S001",
                    WorkGroupGrade = "WG001",
                    Name = "Engineering"
                },
                new StaffWorkgroupLookupDto
                {
                    StaffID = "S002",
                    WorkGroupGrade = "WG002",
                    Name = "Design"
                }
            };

            _mockRepository.GetStaffWorkgroupLookup()
            .Returns(Task.FromResult(staffWorkgroupEntities));

            _mockMapper.Map<List<StaffWorkgroupLookupDto>>(staffWorkgroupEntities)
            .Returns(expectedDtos);

            // Act
            var result = await _sut.GetStaffWorkgroupLookup();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("S001", result[0].StaffID);
            Assert.Equal("Engineering", result[0].Name);
            Assert.Equal("S002", result[1].StaffID);
            Assert.Equal("Design", result[1].Name);

            await _mockRepository.Received(1).GetStaffWorkgroupLookup();
            _mockMapper.Received(1).Map<List<StaffWorkgroupLookupDto>>(staffWorkgroupEntities);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var emptyEntityList = new List<StaffWorkgroupLookup>();
            var emptyDtoList = new List<StaffWorkgroupLookupDto>();

            _mockRepository.GetStaffWorkgroupLookup()
            .Returns(Task.FromResult(emptyEntityList));

            _mockMapper.Map<List<StaffWorkgroupLookupDto>>(emptyEntityList)
            .Returns(emptyDtoList);

            // Act
            var result = await _sut.GetStaffWorkgroupLookup();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            await _mockRepository.Received(1).GetStaffWorkgroupLookup();
            _mockMapper.Received(1).Map<List<StaffWorkgroupLookupDto>>(emptyEntityList);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetStaffWorkgroupLookup()
            .Returns(await Task.FromResult<List<StaffWorkgroupLookup>>(null!));

            _mockMapper.Map<List<StaffWorkgroupLookupDto>?>(null)
            .Returns((List<StaffWorkgroupLookupDto>?)null);

            // Act
            var result = await _sut.GetStaffWorkgroupLookup();

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetStaffWorkgroupLookup();
            _mockMapper.Received(1).Map<List<StaffWorkgroupLookupDto>?>(null); // Explicitly mark the type as nullable
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetStaffWorkgroupLookup()
            .Returns(Task.FromException<List<StaffWorkgroupLookup>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
            async () => await _sut.GetStaffWorkgroupLookup()
            );

            Assert.Equal("Database connection failed", exception.Message);
            await _mockRepository.Received(1).GetStaffWorkgroupLookup();
            _mockMapper.DidNotReceive().Map<List<StaffWorkgroupLookupDto>>(Arg.Any<List<StaffWorkgroupLookup>>());
        }

        [Fact]
        public async Task GetStaffChargeRate_WithValidStaffIdAndJobCode_ReturnsChargeRate()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobcode = "JOB001";
            var expectedChargeRate = 150.50m;

            _mockRepository
            .GetStaffChargeRate(staffId, jobcode)
            .Returns(Task.FromResult<decimal?>(expectedChargeRate));

            // Act
            var result = await _sut.GetStaffChargeRate(staffId, jobcode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedChargeRate, result.Value);
            await _mockRepository.Received(1).GetStaffChargeRate(staffId, jobcode);
        }

        [Fact]
        public async Task GetStaffChargeRate_WhenNoChargeRateExists_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF999";
            var jobcode = "JOBNOTFOUND";

            _mockRepository
            .GetStaffChargeRate(staffId, jobcode)
            .Returns(Task.FromResult<decimal?>(null));

            // Act
            var result = await _sut.GetStaffChargeRate(staffId, jobcode);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetStaffChargeRate(staffId, jobcode);
        }

        [Fact]
        public async Task GetStaffChargeRate_WithEmptyStrings_PassesToRepositoryAndReturnsResult()
        {
            // Arrange
            var staffId = string.Empty;
            var jobcode = string.Empty;

            _mockRepository
            .GetStaffChargeRate(staffId, jobcode)
            .Returns(Task.FromResult<decimal?>(null));

            // Act
            var result = await _sut.GetStaffChargeRate(staffId, jobcode);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetStaffChargeRate(staffId, jobcode);
        }

        [Fact]
        public async Task GetStaffChargeRate_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobcode = "JOB001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository
            .GetStaffChargeRate(staffId, jobcode)
            .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
            () => _sut.GetStaffChargeRate(staffId, jobcode)
            );

            Assert.Equal(expectedException.Message, exception.Message);
            await _mockRepository.Received(1).GetStaffChargeRate(staffId, jobcode);
        }


        [Fact]
        public async Task GetByIdAsync_WhenValidStaffIdAndJobCode_ReturnsStaffJobDto()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var staffJobEntity = new StaffJob
            {
                StaffId = staffId,
                JobCode = jobCode
            };

            var expectedDto = new StaffJobDto
            {
                StaffId = staffId,
                JobCode = jobCode
            };                      

            _mockRepository.GetByIdAsync(staffId, jobCode).Returns(Task.FromResult<StaffJob?>(staffJobEntity));            
            _mockMapper.Map<StaffJobDto>(staffJobEntity).Returns(expectedDto);

            // Act
            var result = await _sut.GetByIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.StaffId, result.StaffId);
            Assert.Equal(expectedDto.JobCode, result.JobCode);

            await _mockRepository.Received(1).GetByIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobDto>(staffJobEntity);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRecordNotFound_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF999";
            var jobCode = "JOB999";

            _mockRepository.GetByIdAsync(staffId, jobCode).Returns(Task.FromResult<StaffJob?>(null)); 
            _mockMapper.Map<StaffJobDto>(Arg.Any<StaffJob?>()).Returns((StaffJobDto?)null);

            // Act
            var result = await _sut.GetByIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetByIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobDto>(null);
        }

        [Theory]
        [InlineData(null, "JOB001")]
        [InlineData("STAFF001", null)]
        [InlineData(null, null)]
        [InlineData("", "JOB001")]
        [InlineData("STAFF001", "")]
        public async Task GetByIdAsync_WhenInvalidInputParameters_ReturnsNull(string staffId, string jobCode)
        {
            // Arrange
            _mockRepository.GetByIdAsync(staffId, jobCode).Returns(Task.FromResult<StaffJob?>(null)); 
            _mockMapper.Map<StaffJobDto>(Arg.Any<StaffJob?>()).Returns((StaffJobDto?)null);

            // Act
            var result = await _sut.GetByIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetByIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenMapperReturnsNull_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var staffJobEntity = new StaffJob
            {
                StaffId = staffId,
                JobCode = jobCode
            };

            _mockRepository.GetByIdAsync(staffId, jobCode).Returns(Task.FromResult<StaffJob?>(staffJobEntity));            
            _mockMapper.Map<StaffJobDto>(staffJobEntity).Returns((StaffJobDto?)null);

            // Act
            var result = await _sut.GetByIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetByIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobDto>(staffJobEntity);
        }

        #region GetViewByStaffIdAsync Tests

        [Fact]
        public async Task GetViewByStaffIdAsync_WhenValidStaffIdAndJobCode_ReturnsStaffJobViewDto()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var staffJobViewEntity = new StaffJobView
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "John Doe",
                PlannedHours = 40,
                ChargeRate = 150.00m,
                StaffCost = 6000.00m
            };

            var expectedDto = new StaffJobViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "John Doe",
                PlannedHours = 40,
                ChargeRate = 150.00m,
                StaffCost = 6000.00m
            };

            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobView?>(staffJobViewEntity));
            _mockMapper.Map<StaffJobViewDto>(staffJobViewEntity)
                .Returns(expectedDto);

            // Act
            var result = await _sut.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            result.Should().NotBeNull();
            result!.StaffID.Should().Be(expectedDto.StaffID);
            result.JobCode.Should().Be(expectedDto.JobCode);
            result.Name.Should().Be(expectedDto.Name);
            result.PlannedHours.Should().Be(expectedDto.PlannedHours);
            result.ChargeRate.Should().Be(expectedDto.ChargeRate);
            result.StaffCost.Should().Be(expectedDto.StaffCost);

            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobViewDto>(staffJobViewEntity);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WhenRecordNotFound_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF999";
            var jobCode = "JOB999";

            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobView?>(null));
            _mockMapper.Map<StaffJobViewDto>(Arg.Any<StaffJobView?>())
                .Returns((StaffJobViewDto?)null);

            // Act
            var result = await _sut.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobViewDto>(null);
        }

        [Theory]
        [InlineData(null, "JOB001")]
        [InlineData("STAFF001", null)]
        [InlineData(null, null)]
        [InlineData("", "JOB001")]
        [InlineData("STAFF001", "")]
        public async Task GetViewByStaffIdAsync_WhenInvalidInputParameters_ReturnsNull(string staffId, string jobCode)
        {
            // Arrange
            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobView?>(null));
            _mockMapper.Map<StaffJobViewDto>(Arg.Any<StaffJobView?>())
                .Returns((StaffJobViewDto?)null);

            // Act
            var result = await _sut.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WhenMapperReturnsNull_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var staffJobViewEntity = new StaffJobView
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "John Doe"
            };

            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobView?>(staffJobViewEntity));
            _mockMapper.Map<StaffJobViewDto>(staffJobViewEntity)
                .Returns((StaffJobViewDto?)null);

            // Act
            var result = await _sut.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobViewDto>(staffJobViewEntity);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetViewByStaffIdAsync(staffId, jobCode)
            );

            Assert.Equal(expectedException.Message, exception.Message);
            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mockMapper.DidNotReceive().Map<StaffJobViewDto>(Arg.Any<StaffJobView>());
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WithCompleteData_MapsAllProperties()
        {
            // Arrange
            var staffId = "STAFF002";
            var jobCode = "JOB002";

            var staffJobViewEntity = new StaffJobView
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "Jane Smith",
                PlannedHours = 80,
                ChargeRate = 200.00m,
                StaffCost = 16000.00m,
                WorkGroupGrade = "WG01",
                GradeCode = "G01",
                WorkGroup = "Engineering",
                SectorName = "charge",
                Days = 10
            };

            var expectedDto = new StaffJobViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "Jane Smith",
                PlannedHours = 80,
                ChargeRate = 200.00m,
                StaffCost = 16000.00m,
                WorkGroupGrade = "WG01",
                GradeCode = "G01",
                WorkGroup = "Engineering",
                SectorName = "charge",
                Days = 10
            };

            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobView?>(staffJobViewEntity));
            _mockMapper.Map<StaffJobViewDto>(staffJobViewEntity)
                .Returns(expectedDto);

            // Act
            var result = await _sut.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            result.Should().NotBeNull();
            result!.StaffID.Should().Be(expectedDto.StaffID);
            result.JobCode.Should().Be(expectedDto.JobCode);
            result.Name.Should().Be(expectedDto.Name);
            result.PlannedHours.Should().Be(expectedDto.PlannedHours);
            result.ChargeRate.Should().Be(expectedDto.ChargeRate);
            result.StaffCost.Should().Be(expectedDto.StaffCost);
            result.WorkGroupGrade.Should().Be(expectedDto.WorkGroupGrade);
            result.GradeCode.Should().Be(expectedDto.GradeCode);
            result.WorkGroup.Should().Be(expectedDto.WorkGroup);
            result.SectorName.Should().Be(expectedDto.SectorName);
            result.Days.Should().Be(expectedDto.Days);

            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobViewDto>(staffJobViewEntity);
        }

        #endregion

        [Fact]
        public async Task AddAsync_WithValidStaffJob_ShouldReturnStaffJobDto()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            var mappedEntity = new StaffJob
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            var repositoryResult = new StaffJob
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            var expectedDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            _mockMapper.Map<StaffJob>(inputDto).Returns(mappedEntity);
            _mockRepository.AddAsync(mappedEntity).Returns(repositoryResult);
            _mockMapper.Map<StaffJobDto>(repositoryResult).Returns(expectedDto);

            // Act
            var result = await _sut.AddAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.StaffId.Should().Be("STAFF001");
            result.JobCode.Should().Be("JOB001");
            result.PlannedHours.Should().Be(75.50);

            _mockMapper.Received(1).Map<StaffJob>(inputDto);
            await _mockRepository.Received(1).AddAsync(mappedEntity);
            _mockMapper.Received(1).Map<StaffJobDto>(repositoryResult);
        }

        [Fact]
        public async Task AddAsync_WithMinimalData_ShouldProcessSuccessfully()
        {
            // Arrange
            var minimalDto = new StaffJobDto
            {
                StaffId = "STAFF002",
                JobCode = "JOB002"
            };

            var mappedEntity = new StaffJob
            {
                StaffId = "STAFF002",
                JobCode = "JOB002"
            };

            var repositoryResult = new StaffJob
            {
                StaffId = "STAFF002",
                JobCode = "JOB002"
            };

            var expectedDto = new StaffJobDto
            {
                StaffId = "STAFF002",
                JobCode = "JOB002"
            };

            _mockMapper.Map<StaffJob>(minimalDto).Returns(mappedEntity);
            _mockRepository.AddAsync(mappedEntity).Returns(repositoryResult);
            _mockMapper.Map<StaffJobDto>(repositoryResult).Returns(expectedDto);

            // Act
            var result = await _sut.AddAsync(minimalDto);

            // Assert
            result.Should().NotBeNull();
            result.StaffId.Should().Be("STAFF002");
            result.JobCode.Should().Be("JOB002");

            await _mockRepository.Received(1).AddAsync(Arg.Any<StaffJob>());
        }

        [Fact]
        public async Task AddAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF003",
                JobCode = "JOB003"
            };

            var mappedEntity = new StaffJob
            {
                StaffId = "STAFF003",
                JobCode = "JOB003"
            };

            _mockMapper.Map<StaffJob>(inputDto).Returns(mappedEntity);
            _mockRepository.AddAsync(mappedEntity)
            .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            Func<Task> act = async () => await _sut.AddAsync(inputDto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");

            await _mockRepository.Received(1).AddAsync(mappedEntity);
        }

        [Fact]
        public async Task UpdateAsync_WithValidStaffJob_ShouldReturnUpdatedStaffJobDto()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            var mappedEntity = new StaffJob
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            var updatedEntity = new StaffJob
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 85.00
            };

            var expectedDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 85.00
            };

            _mockMapper.Map<StaffJob>(inputDto).Returns(mappedEntity);
            _mockRepository.UpdateAsync(mappedEntity).Returns(updatedEntity);
            _mockMapper.Map<StaffJobDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.StaffId, result.StaffId);
            Assert.Equal(expectedDto.JobCode, result.JobCode);
            Assert.Equal(expectedDto.PlannedHours, result.PlannedHours);

            _mockMapper.Received(1).Map<StaffJob>(inputDto);
            await _mockRepository.Received(1).UpdateAsync(mappedEntity);
            _mockMapper.Received(1).Map<StaffJobDto>(updatedEntity);
        }        

        [Fact]
        public async Task UpdateAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF999",
                JobCode = "INVALID"
            };

            var mappedEntity = new StaffJob
            {
                StaffId = "STAFF999",
                JobCode = "INVALID"
            };

            var exceptionMessage = "Database connection failed";

            _mockMapper.Map<StaffJob>(inputDto).Returns(mappedEntity);
            _mockRepository.UpdateAsync(mappedEntity)
            .Throws(new InvalidOperationException(exceptionMessage));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.UpdateAsync(inputDto)
            );

            Assert.Equal(exceptionMessage, exception.Message);

            await _mockRepository.Received(1).UpdateAsync(mappedEntity);
            _mockMapper.DidNotReceive().Map<StaffJobDto>(Arg.Any<StaffJob>());
        }

        [Fact]
        public async Task DeleteAsync_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB123";
            _mockRepository.DeleteAsync(staffId, jobCode).Returns(Task.FromResult(true));

            // Act
            var result = await _sut.DeleteAsync(staffId, jobCode);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).DeleteAsync(staffId, jobCode);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentRecord_ReturnsFalse()
        {
            // Arrange
            var staffId = "STAFF999";
            var jobCode = "NONEXISTENT";
            _mockRepository.DeleteAsync(staffId, jobCode).Returns(Task.FromResult(false));

            // Act
            var result = await _sut.DeleteAsync(staffId, jobCode);

            // Assert
            Assert.False(result);
            await _mockRepository.Received(1).DeleteAsync(staffId, jobCode);
        }

        [Theory]
        [InlineData(null, "JOB123")]
        [InlineData("", "JOB123")]
        [InlineData("STAFF001", null)]
        [InlineData("STAFF001", "")]
        public async Task DeleteAsync_WithNullOrEmptyParameters_CallsRepository(string staffId, string jobCode)
        {
            // Arrange
            _mockRepository.DeleteAsync(staffId, jobCode).Returns(Task.FromResult(false));

            // Act
            var result = await _sut.DeleteAsync(staffId, jobCode);

            // Assert
            Assert.False(result);
            await _mockRepository.Received(1).DeleteAsync(staffId, jobCode);
        }

        [Fact]
        public async Task DeleteAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB123";
            var expectedException = new Exception("Database connection failed");
            _mockRepository.DeleteAsync(staffId, jobCode).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(staffId, jobCode));
            Assert.Equal("Database connection failed", exception.Message);
            await _mockRepository.Received(1).DeleteAsync(staffId, jobCode);
        }

    }
}




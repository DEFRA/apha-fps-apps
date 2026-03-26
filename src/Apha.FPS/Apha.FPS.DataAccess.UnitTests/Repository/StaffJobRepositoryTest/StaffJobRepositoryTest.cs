using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.StaffJobRepositoryTest
{
    public class StaffJobRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;
        private const int DefaultUserId = 42;

        private static Mock<IFpsYearContext> CreateMockFpsYearContext(int year = DefaultTestFpsYear)
        {
            var mockFpsYearContext = new Mock<IFpsYearContext>();
            mockFpsYearContext.Setup(x => x.FPSYear).Returns(year);
            return mockFpsYearContext;
        }

        private static StaffJobRepository CreateRepository(
            IEnumerable<StaffJob>? staffJobs = null,
            IEnumerable<StaffJobTblView>? staffJobTblViews = null,
            IEnumerable<StaffGeneralView>? staffGeneralViews = null,
            IEnumerable<WorkgroupGrade>? workgroupGrades = null,
            IEnumerable<ProfitCentreGrade>? profitCentreGrades = null,
            IEnumerable<ProjectView>? projectViews = null,
            IEnumerable<ProgramView>? programViews = null,
            IEnumerable<StaffView>? staffViews = null,
            IEnumerable<StaffPickView>? staffPickViews = null,
            IEnumerable<FpsSetting>? settings = null,
            IEnumerable<WgEmployee>? wgEmployees = null,
            IEnumerable<Employee>? employees = null,
            IEnumerable<Project>? projects = null,
            int fpsYear = DefaultTestFpsYear)
        {
            var mockFpsYearContext = CreateMockFpsYearContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            if (staffJobs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(staffJobs);
                mockContext.Setup(x => x.StaffJobs).Returns(mockSet.Object);
            }

            if (staffJobTblViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(staffJobTblViews);
                mockContext.Setup(x => x.StaffJobTblViews).Returns(mockSet.Object);
            }

            if (staffGeneralViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(staffGeneralViews);
                mockContext.Setup(x => x.StaffGeneralViews).Returns(mockSet.Object);
            }

            if (workgroupGrades != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(workgroupGrades);
                mockContext.Setup(x => x.WorkgroupGrades).Returns(mockSet.Object);
            }

            if (profitCentreGrades != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(profitCentreGrades);
                mockContext.Setup(x => x.ProfitcentreGrades).Returns(mockSet.Object);
            }

            if (projectViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
                mockContext.Setup(x => x.ProjectViews).Returns(mockSet.Object);
            }

            if (programViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(programViews);
                mockContext.Setup(x => x.ProgramViews).Returns(mockSet.Object);
            }

            if (staffViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(staffViews);
                mockContext.Setup(x => x.StaffViews).Returns(mockSet.Object);
            }

            if (staffPickViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(staffPickViews);
                mockContext.Setup(x => x.StaffPickViews).Returns(mockSet.Object);
            }

            if (settings != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(settings);
                mockContext.Setup(x => x.TblSettings).Returns(mockSet.Object);
            }

            if (wgEmployees != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(wgEmployees);
                mockContext.Setup(x => x.WgEmployees).Returns(mockSet.Object);
            }

            if (employees != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(employees);
                mockContext.Setup(x => x.Employees).Returns(mockSet.Object);
            }

            if (projects != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projects);
                mockContext.Setup(x => x.Projects).Returns(mockSet.Object);
            }

            return new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);
        }

        #region GetJobStaffCostAsync Tests

        [Fact]
        public async Task GetJobStaffCostAsync_ReturnsPagedData_WithValidJobCode()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "John Doe", WorkGroupGrade = "WG01" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ChargeRate = 100, DefraChargeRate = 120 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01" }
            };
            var programViews = new List<ProgramView>
            {
                new() { ProgramNo = "PROG01", UserId = DefaultUserId, SectorName = "charge" }
            };

            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_AppliesFilter_ByName()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 30, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob", WorkGroupGrade = "WG01" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ChargeRate = 100, DefraChargeRate = 120 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01" }
            };
            var programViews = new List<ProgramView>
            {
                new() { ProgramNo = "PROG01", UserId = DefaultUserId, SectorName = "charge" }
            };

            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                settings: settings);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Name\":\"Alice\"}"
            };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            Assert.Single(result.Data);
            Assert.Contains("Alice", result.Data.First().Name);
        }

        [Theory]
        [InlineData("name", false, "Alice")]
        [InlineData("name", true, "Charlie")]
        [InlineData("plannedhours", false, 20.0)]
        [InlineData("plannedhours", true, 40.0)]
        public async Task GetJobStaffCostAsync_AppliesSorting_Correctly(string sortBy, bool descending, object expectedFirstValue)
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 30, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S003", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Bob", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Charlie", WorkGroupGrade = "WG01" },
                new() { StaffId = "S003", Name = "Alice", WorkGroupGrade = "WG01" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ChargeRate = 100, DefraChargeRate = 120 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01" }
            };
            var programViews = new List<ProgramView>
            {
                new() { ProgramNo = "PROG01", UserId = DefaultUserId, SectorName = "charge" }
            };

            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                settings: settings);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var firstItem = result.Data.First();
            var actualValue = sortBy.ToLower() switch
            {
                "name" => (object?)firstItem.Name,
                "plannedhours" => (object)firstItem.PlannedHours,
                _ => (object?)firstItem.Name
            };
            Assert.Equal(expectedFirstValue.ToString(), actualValue?.ToString());
        }

        [Fact]
        public async Task GetJobStaffCostAsync_CalculatesStaffCost_ForChargeableSector()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "John", WorkGroupGrade = "WG01" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ChargeRate = 100, DefraChargeRate = 120 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01" }
            };
            var programViews = new List<ProgramView>
            {
                new() { ProgramNo = "PROG01", UserId = DefaultUserId, SectorName = "charge" }
            };

            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var staffJobView = result.Data.First();
            Assert.Equal(4000m, staffJobView.StaffCost); // 40 hours * 100 rate * 1 (charge)
        }

        [Fact]
        public async Task GetJobStaffCostAsync_ReturnsEmptyList_WhenNoDataFound()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var repo = CreateRepository(
                staffJobTblViews: new List<StaffJobTblView>(),
                staffGeneralViews: new List<StaffGeneralView>(),
                workgroupGrades: new List<WorkgroupGrade>(),
                profitCentreGrades: new List<ProfitCentreGrade>(),
                projectViews: new List<ProjectView>(),
                programViews: new List<ProgramView>(),
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "NONEXISTENT");

            // Assert
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetStaffWorkgroupLookup Tests

        [Fact]
        public async Task GetStaffWorkgroupLookup_ReturnsStaffList_OrderedByName()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S003", Name = "Charlie", WorkgroupGrade = "WG01", HrsAvail = 40, UserId = DefaultUserId },
                new() { StaffId = "S001", Name = "Alice", WorkgroupGrade = "WG02", HrsAvail = 35, UserId = DefaultUserId },
                new() { StaffId = "S002", Name = "Bob", WorkgroupGrade = "WG03", HrsAvail = 38, UserId = DefaultUserId }
            };
            var staffPickViews = new List<StaffPickView>
            {
                new() { StaffId = "S001" },
                new() { StaffId = "S002" },
                new() { StaffId = "S003" }
            };

            var repo = CreateRepository(staffViews: staffViews, staffPickViews: staffPickViews);

            // Act
            var result = await repo.GetStaffWorkgroupLookup();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Alice", result[0].Name);
            Assert.Equal("Bob", result[1].Name);
            Assert.Equal("Charlie", result[2].Name);
        }      

        [Fact]
        public async Task GetStaffWorkgroupLookup_ReturnsEmpty_WhenNoMatches()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S001", Name = "John", UserId = 999 }
            };
            var staffPickViews = new List<StaffPickView>
            {
                new() { StaffId = "S002" }
            };

            var repo = CreateRepository(staffViews: staffViews, staffPickViews: staffPickViews);

            // Act
            var result = await repo.GetStaffWorkgroupLookup();

            // Assert
            Assert.Empty(result);
        }

        #endregion
    

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsStaffJob_WhenFound()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 },
                new() { StaffId = "S002", JobCode = "JOB002", PlannedHours = 35 }
            };
            var repo = CreateRepository(staffJobs: staffJobs);

            // Act
            var result = await repo.GetByIdAsync("S001", "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffId);
            Assert.Equal("JOB001", result.JobCode);
            Assert.Equal(40, result.PlannedHours);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 }
            };
            var repo = CreateRepository(staffJobs: staffJobs);

            // Act
            var result = await repo.GetByIdAsync("S999", "JOB999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenEmptyList()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: new List<StaffJob>());

            // Act
            var result = await repo.GetByIdAsync("S001", "JOB001");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetViewByStaffIdAsync Tests

        [Fact]
        public async Task GetViewByStaffIdAsync_ReturnsView_WhenFound()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "John", WorkGroupGrade = "WG01" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ChargeRate = 100, DefraChargeRate = 120 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01" }
            };
            var programViews = new List<ProgramView>
            {
                new() { ProgramNo = "PROG01", UserId = DefaultUserId, SectorName = "charge" }
            };

            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                settings: settings);

            // Act
            var result = await repo.GetViewByStaffIdAsync("S001", "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffID);
            Assert.Equal("JOB001", result.JobCode);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var repo = CreateRepository(
                staffJobTblViews: new List<StaffJobTblView>(),
                staffGeneralViews: new List<StaffGeneralView>(),
                workgroupGrades: new List<WorkgroupGrade>(),
                profitCentreGrades: new List<ProfitCentreGrade>(),
                projectViews: new List<ProjectView>(),
                programViews: new List<ProgramView>(),
                settings: settings);

            // Act
            var result = await repo.GetViewByStaffIdAsync("S999", "JOB999");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_AddsStaffJob_WithFpsYear()
        {
            // Arrange
            var mockFpsYearContext = CreateMockFpsYearContext(2025);
            var (mockContext, staffJobMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, StaffJob>(
                    new List<StaffJob>(),
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.StaffJobs).Returns(staffJobMockSet.Object);

            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);
            var newStaffJob = new StaffJob
            {
                StaffId = "S001",
                JobCode = "JOB001",
                PlannedHours = 40
            };

            // Act
            var result = await repo.AddAsync(newStaffJob);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffId);
            Assert.Equal("JOB001", result.JobCode);
            Assert.Equal(2025, result.FpsCalYear);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddAsync_ThrowsArgumentNullException_WhenStaffJobIsNull()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: new List<StaffJob>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAsync(null!));
        }

        [Fact]
        public async Task AddAsync_ThrowsArgumentOutOfRangeException_WhenPlannedHoursIsNegative()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: new List<StaffJob>());
            var staffJob = new StaffJob
            {
                StaffId = "S001",
                JobCode = "JOB001",
                PlannedHours = -10
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repo.AddAsync(staffJob));
        }

        [Fact]
        public async Task AddAsync_ThrowsInvalidOperationException_WhenDuplicateExists()
        {
            // Arrange
            var existingStaffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 }
            };
            var repo = CreateRepository(staffJobs: existingStaffJobs);
            var duplicateStaffJob = new StaffJob
            {
                StaffId = "S001",
                JobCode = "JOB001",
                PlannedHours = 50
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.AddAsync(duplicateStaffJob));
            Assert.Contains("already exists", exception.Message);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_UpdatesStaffJob_WithFpsYear()
        {
            // Arrange
            var existingStaffJob = new StaffJob
            {
                StaffId = "S001",
                JobCode = "JOB001",
                PlannedHours = 40,
                FpsCalYear = 2024
            };
            var mockFpsYearContext = CreateMockFpsYearContext(2025);
            var (mockContext, staffJobMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, StaffJob>(
                    new List<StaffJob> { existingStaffJob },
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.StaffJobs).Returns(staffJobMockSet.Object);

            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);
            var updatedStaffJob = new StaffJob
            {
                StaffId = "S001",
                JobCode = "JOB001",
                PlannedHours = 50
            };

            // Act
            var result = await repo.UpdateAsync(updatedStaffJob);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50, result.PlannedHours);
            Assert.Equal(2025, result.FpsCalYear);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenStaffJobIsNull()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: new List<StaffJob>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentOutOfRangeException_WhenPlannedHoursIsNegative()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: new List<StaffJob>());
            var staffJob = new StaffJob
            {
                StaffId = "S001",
                JobCode = "JOB001",
                PlannedHours = -10
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repo.UpdateAsync(staffJob));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenNotFound()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: new List<StaffJob>());
            var staffJob = new StaffJob
            {
                StaffId = "S999",
                JobCode = "JOB999",
                PlannedHours = 50
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync(staffJob));
            Assert.Contains("not found", exception.Message);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_DeletesStaffJob_WhenFound()
        {
            // Arrange
            var existingStaffJob = new StaffJob
            {
                StaffId = "S001",
                JobCode = "JOB001",
                PlannedHours = 40
            };
            var mockFpsYearContext = CreateMockFpsYearContext();
            var (mockContext, staffJobMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, StaffJob>(
                    new List<StaffJob> { existingStaffJob },
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.StaffJobs).Returns(staffJobMockSet.Object);

            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.DeleteAsync("S001", "JOB001");

            // Assert
            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(staffJobMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            var repo = CreateRepository(staffJobs: new List<StaffJob>());

            // Act
            var result = await repo.DeleteAsync("S999", "JOB999");

            // Assert
            Assert.False(result);
        }
        #endregion
    }
}

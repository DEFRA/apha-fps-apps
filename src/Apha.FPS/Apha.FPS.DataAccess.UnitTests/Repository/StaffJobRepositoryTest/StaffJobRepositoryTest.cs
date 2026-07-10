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
        private const string DefaultUserEmail = "test@example.com";

        private static Mock<IFpsRequestContext> CreateMockFpsYearContext(int year = DefaultTestFpsYear)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            mockFpsYearContext.Setup(x => x.FpsYear).Returns(year);
            mockFpsYearContext.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
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
            IEnumerable<WorkGroupEmployee>? wgEmployees = null,
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
                mockContext.Setup(x => x.ProfitCentreGrades).Returns(mockSet.Object);
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
                mockContext.Setup(x => x.WorkGroupEmployees).Returns(mockSet.Object);
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
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01", UserEmail = DefaultUserEmail }
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
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
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
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01", UserEmail = DefaultUserEmail }
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
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
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
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "NONEXISTENT");

            // Assert
            Assert.Empty(result.Data);
        }

        #endregion

        #region ApplyStaffJobFilter Tests

        private static (List<StaffJobTblView>, List<StaffGeneralView>, List<WorkgroupGrade>, List<ProfitCentreGrade>, List<ProjectView>, List<ProgramView>, List<FpsSetting>) BuildTwoStaffFilterTestData()
        {
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice Smith", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob Jones",   WorkGroupGrade = "WG01" }
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
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01", UserEmail = DefaultUserEmail }
            };
            var programViews = new List<ProgramView>
            {
                new() { ProgramNo = "PROG01", UserId = DefaultUserId, SectorName = "charge" }
            };
            return (staffJobTblViews, staffGeneralViews, workgroupGrades, profitCentreGrades, projectViews, programViews, settings);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Filter_Null_ReturnsAllRecords()
        {
            // Arrange
            var (staffJobTblViews, staffGeneralViews, workgroupGrades, profitCentreGrades, projectViews, programViews, settings) = BuildTwoStaffFilterTestData();
            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Filter_EmptyString_ReturnsAllRecords()
        {
            // Arrange
            var (staffJobTblViews, staffGeneralViews, workgroupGrades, profitCentreGrades, projectViews, programViews, settings) = BuildTwoStaffFilterTestData();
            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Filter_InvalidJson_ReturnsAllRecords()
        {
            // Arrange
            var (staffJobTblViews, staffGeneralViews, workgroupGrades, profitCentreGrades, projectViews, programViews, settings) = BuildTwoStaffFilterTestData();
            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "not-valid-json" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Filter_ByNameExactMatch_ReturnsMatchingRecord()
        {
            // Arrange
            var (staffJobTblViews, staffGeneralViews, workgroupGrades, profitCentreGrades, projectViews, programViews, settings) = BuildTwoStaffFilterTestData();
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S001", Name = "Alice Smith", WorkgroupGrade = "WG01", UserEmail = DefaultUserEmail },
                new() { StaffId = "S002", Name = "Bob Jones",   WorkgroupGrade = "WG01", UserEmail = DefaultUserEmail }
            };
            var staffPickViews = new List<StaffPickView>
            {
                new() { StaffId = "S001" },
                new() { StaffId = "S002" }
            };
            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                staffViews: staffViews,
                staffPickViews: staffPickViews,
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"Name\":\"Alice Smith\"}" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("Alice Smith", result.Data.First().Name);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Filter_ByNamePartialMatch_IsCaseInsensitive()
        {
            // Arrange
            var (staffJobTblViews, staffGeneralViews, workgroupGrades, profitCentreGrades, projectViews, programViews, settings) = BuildTwoStaffFilterTestData();
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S001", Name = "Alice Smith", WorkgroupGrade = "WG01", UserEmail = DefaultUserEmail },
                new() { StaffId = "S002", Name = "Bob Jones",   WorkgroupGrade = "WG01", UserEmail = DefaultUserEmail }
            };
            var staffPickViews = new List<StaffPickView>
            {
                new() { StaffId = "S001" },
                new() { StaffId = "S002" }
            };
            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                staffViews: staffViews,
                staffPickViews: staffPickViews,
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"Name\":\"alice\"}" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("Alice Smith", result.Data.First().Name);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Filter_ByNameNoMatch_ReturnsEmptyList()
        {
            // Arrange
            var (staffJobTblViews, staffGeneralViews, workgroupGrades, profitCentreGrades, projectViews, programViews, settings) = BuildTwoStaffFilterTestData();
            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"Name\":\"ZZZ_NoMatch\"}" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_Filter_UnknownKey_ReturnsAllRecords()
        {
            // Arrange
            var (staffJobTblViews, staffGeneralViews, workgroupGrades, profitCentreGrades, projectViews, programViews, settings) = BuildTwoStaffFilterTestData();
            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
                settings: settings);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"UnknownField\":\"value\"}" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetStaffWorkgroupLookup Tests

        [Fact]
        public async Task GetStaffWorkgroupLookup_ReturnsStaffList_OrderedByName()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S003", Name = "Charlie", WorkgroupGrade = "WG01", HrsAvail = 40, UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { StaffId = "S001", Name = "Alice", WorkgroupGrade = "WG02", HrsAvail = 35, UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { StaffId = "S002", Name = "Bob", WorkgroupGrade = "WG03", HrsAvail = 38, UserId = DefaultUserId, UserEmail = DefaultUserEmail }
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
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01", UserEmail = DefaultUserEmail }
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
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
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
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
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

            var staffJobLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<StaffJobLog>());
            mockContext.Setup(x => x.StaffJobLogs).Returns(staffJobLogsMockSet.Object);

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
            Assert.Equal(2025, result.FpsYear);
            staffJobLogsMockSet.Verify(m => m.Add(It.Is<StaffJobLog>(log =>
                log.StaffId == "S001" &&
                log.JobCode == "JOB001" &&
                log.InsertDelete == "I")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
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
                FpsYear = 2024
            };
            var mockFpsYearContext = CreateMockFpsYearContext(2025);
            var (mockContext, staffJobMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, StaffJob>(
                    new List<StaffJob> { existingStaffJob },
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.StaffJobs).Returns(staffJobMockSet.Object);

            var staffJobLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<StaffJobLog>());
            mockContext.Setup(x => x.StaffJobLogs).Returns(staffJobLogsMockSet.Object);

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
            Assert.Equal(2025, result.FpsYear);
            staffJobLogsMockSet.Verify(m => m.Add(It.Is<StaffJobLog>(log =>
                log.StaffId == "S001" &&
                log.JobCode == "JOB001" &&
                log.InsertDelete == "I")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
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

            var staffJobLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<StaffJobLog>());
            mockContext.Setup(x => x.StaffJobLogs).Returns(staffJobLogsMockSet.Object);

            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.DeleteAsync("S001", "JOB001");

            // Assert
            Assert.True(result);
            staffJobLogsMockSet.Verify(m => m.Add(It.Is<StaffJobLog>(log =>
                log.StaffId == "S001" &&
                log.JobCode == "JOB001" &&
                log.InsertDelete == "D")), Times.Once);
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

        #region GetTotalStaffCostAsync Tests

        [Fact]
        public async Task GetTotalStaffCostAsync_ReturnsSumOfStaffCosts_ForSingleRecord()
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
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01", UserEmail = DefaultUserEmail }
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
            var result = await repo.GetTotalStaffCostAsync("JOB001");

            // Assert
            Assert.Equal(4000m, result); // 40 hours * 100 rate
        }

        [Fact]
        public async Task GetTotalStaffCostAsync_ReturnsSumOfStaffCosts_ForMultipleRecords()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
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
                new() { ParentProject = "JOB001", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01", UserEmail = DefaultUserEmail }
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
            var result = await repo.GetTotalStaffCostAsync("JOB001");

            // Assert
            Assert.Equal(6000m, result); // (40 + 20) hours * 100 rate
        }

        [Fact]
        public async Task GetTotalStaffCostAsync_ReturnsZero_WhenNoRecordsFound()
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
            var result = await repo.GetTotalStaffCostAsync("NONEXISTENT");

            // Assert
            Assert.Equal(0m, result);
        }

        #endregion
        
        #region GetZtStaffJobsByStaffIdPagedAsync Tests

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_ReturnsPagedData_WithValidStaffId()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S001", JobCode = "ZT002", PlannedHours = 20, UserId = DefaultUserId }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { ParentProject = "ZT002", ProjectTitle = "Training", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var staffJobTblViewsMockSet = RepositoryTestHelper.CreateMockDbSet(staffJobTblViews);
            mockContext.Setup(x => x.StaffJobTblViews).Returns(staffJobTblViewsMockSet.Object);

            var projectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
            mockContext.Setup(x => x.ProjectViews).Returns(projectViewsMockSet.Object);

            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_ReturnsEmpty_WhenNoMatchingStaff()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var staffJobTblViewsMockSet = RepositoryTestHelper.CreateMockDbSet(staffJobTblViews);
            mockContext.Setup(x => x.StaffJobTblViews).Returns(staffJobTblViewsMockSet.Object);

            var projectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
            mockContext.Setup(x => x.ProjectViews).Returns(projectViewsMockSet.Object);

            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S999");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetZtStaffJobDetailsByIdAsync Tests

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_ReturnsDetail_WhenFound()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var staffJobTblViewsMockSet = RepositoryTestHelper.CreateMockDbSet(staffJobTblViews);
            mockContext.Setup(x => x.StaffJobTblViews).Returns(staffJobTblViewsMockSet.Object);

            var projectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
            mockContext.Setup(x => x.ProjectViews).Returns(projectViewsMockSet.Object);

            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.GetZtStaffJobDetailsByIdAsync("S001", "ZT001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffID);
            Assert.Equal("ZT001", result.JobCode);
            Assert.Equal(40, result.PlannedHours);
            Assert.Equal("Admin Work", result.Name);
        }

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var staffJobTblViewsMockSet = RepositoryTestHelper.CreateMockDbSet(staffJobTblViews);
            mockContext.Setup(x => x.StaffJobTblViews).Returns(staffJobTblViewsMockSet.Object);

            var projectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
            mockContext.Setup(x => x.ProjectViews).Returns(projectViewsMockSet.Object);

            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.GetZtStaffJobDetailsByIdAsync("S999", "ZT999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_ReturnsNull_WhenStaffIdDoesNotMatch()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var staffJobTblViewsMockSet = RepositoryTestHelper.CreateMockDbSet(staffJobTblViews);
            mockContext.Setup(x => x.StaffJobTblViews).Returns(staffJobTblViewsMockSet.Object);

            var projectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
            mockContext.Setup(x => x.ProjectViews).Returns(projectViewsMockSet.Object);

            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.GetZtStaffJobDetailsByIdAsync("S002", "ZT001");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetStaffSummaryByIdAsync Tests

        [Fact]
        public async Task GetStaffSummaryByIdAsync_ReturnsStaffSummary_WhenStaffIdExists()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S001", Name = "Alice Smith", WorkgroupGrade = "WG01", HrsAvail = 37.5, HrsPaid = 40, Leave = 5, SickSpecial = 2, UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(staffViews: staffViews, staffPickViews: new List<StaffPickView>());

            // Act
            var result = await repo.GetStaffSummaryByIdAsync("S001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffID);
            Assert.Equal("Alice Smith", result.Name);
            Assert.Equal("WG01", result.WorkGroupGrade);
            Assert.Equal(37.5, result.HrsAvail);
            Assert.Equal(40, result.HrsPaid);
            Assert.Equal(5, result.Leave);
            Assert.Equal(2, result.SickSpecial);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_ReturnsNull_WhenStaffIdDoesNotExist()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S001", Name = "Alice Smith", WorkgroupGrade = "WG01", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(staffViews: staffViews, staffPickViews: new List<StaffPickView>());

            // Act
            var result = await repo.GetStaffSummaryByIdAsync("S999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_ReturnsNull_WhenStaffViewsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(staffViews: new List<StaffView>(), staffPickViews: new List<StaffPickView>());

            // Act
            var result = await repo.GetStaffSummaryByIdAsync("S001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_MapsNullableFields_ToDefaults_WhenNullInView()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new() { StaffId = "S001", Name = null, WorkgroupGrade = null, HrsAvail = null, HrsPaid = null, Leave = null, SickSpecial = null, UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(staffViews: staffViews, staffPickViews: new List<StaffPickView>());

            // Act
            var result = await repo.GetStaffSummaryByIdAsync("S001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.Name);
            Assert.Equal("", result.WorkGroupGrade);
            Assert.Equal(0, result.HrsAvail);
            Assert.Equal(0, result.HrsPaid);
            Assert.Equal(0, result.Leave);
            Assert.Equal(0, result.SickSpecial);
        }

        #endregion

        #region GetStaffChargeRate Tests

        [Fact]
        public async Task GetStaffChargeRate_ReturnsChargeRate_WhenJobCodeMatchesProject()
        {
            // Arrange
            var workGroupEmployees = new List<WorkGroupEmployee>
            {
                new() { PactId = "S001", SpNumber = "SP001", WorkGroupGrade = "WG01", PersonStatus = "Active", HrsPaid = 40, Leave = 5, SickSpecial = 2, HrsAvail = 37.5, MakeAvailable = 1, TimeRecorder = 1 }
            };
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ChargeRate = 150m, DefraChargeRate = 200m }
            };
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", IsDefraProject = 0 }
            };

            var repo = CreateRepository(
                staffJobs: staffJobs,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                wgEmployees: workGroupEmployees,
                employees: employees,
                projects: projects);

            // Act
            var result = await repo.GetStaffChargeRate("S001", "JOB001");

            // Assert
            Assert.Equal(150m, result);
        }

        [Fact]
        public async Task GetStaffChargeRate_ReturnsDefraChargeRate_WhenProjectIsDefra()
        {
            // Arrange
            var workGroupEmployees = new List<WorkGroupEmployee>
            {
                new() { PactId = "S001", SpNumber = "SP001", WorkGroupGrade = "WG01", PersonStatus = "Active", HrsPaid = 40, Leave = 5, SickSpecial = 2, HrsAvail = 37.5, MakeAvailable = 1, TimeRecorder = 1 }
            };
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ChargeRate = 150m, DefraChargeRate = 200m }
            };
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", IsDefraProject = -1 }
            };

            var repo = CreateRepository(
                staffJobs: staffJobs,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                wgEmployees: workGroupEmployees,
                employees: employees,
                projects: projects);

            // Act
            var result = await repo.GetStaffChargeRate("S001", "JOB001");

            // Assert
            Assert.Equal(200m, result);
        }

        [Fact]
        public async Task GetStaffChargeRate_ReturnsNull_WhenStaffIdDoesNotExist()
        {
            // Arrange
            var repo = CreateRepository(
                staffJobs: new List<StaffJob>(),
                workgroupGrades: new List<WorkgroupGrade>(),
                profitCentreGrades: new List<ProfitCentreGrade>(),
                wgEmployees: new List<WorkGroupEmployee>(),
                employees: new List<Employee>(),
                projects: new List<Project>());

            // Act
            var result = await repo.GetStaffChargeRate("S999", "JOB001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetStaffChargeRate_ReturnsFallbackChargeRate_WhenJobCodeDoesNotMatch()
        {
            // Arrange - staff has a job on JOB002, but we query for JOB001
            var workGroupEmployees = new List<WorkGroupEmployee>
            {
                new() { PactId = "S001", SpNumber = "SP001", WorkGroupGrade = "WG01", PersonStatus = "Active", HrsPaid = 40, Leave = 5, SickSpecial = 2, HrsAvail = 37.5, MakeAvailable = 1, TimeRecorder = 1 }
            };
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ChargeRate = 150m, DefraChargeRate = 200m }
            };
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JOB002", PlannedHours = 40 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB002", IsDefraProject = 0 }
            };

            var repo = CreateRepository(
                staffJobs: staffJobs,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                wgEmployees: workGroupEmployees,
                employees: employees,
                projects: projects);

            // Act — no exact job-code match, falls back to first available rate
            var result = await repo.GetStaffChargeRate("S001", "JOB001");

            // Assert
            Assert.Equal(150m, result);
        }

        #endregion

        #region GetZtTotalHoursByStaffIdAsync Tests

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_ReturnsTotalHours_WhenZtJobsExist()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 30, UserId = DefaultUserId },
                new() { StaffId = "S001", JobCode = "ZT002", PlannedHours = 20, UserId = DefaultUserId }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "ZT001", Type = "ZT" },
                new() { JobCodeId = "ZT002", Type = "ZT" }
            };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);
            mockContext.Setup(x => x.StaffJobTblViews).Returns(RepositoryTestHelper.CreateMockDbSet(staffJobTblViews).Object);
            mockContext.Setup(x => x.JobCodes).Returns(RepositoryTestHelper.CreateMockDbSet(jobCodes).Object);
            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.GetZtTotalHoursByStaffIdAsync("S001");

            // Assert
            Assert.Equal(50, result);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_ReturnsZero_WhenNoZtJobsExist()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JOB001", Type = "STD" }
            };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);
            mockContext.Setup(x => x.StaffJobTblViews).Returns(RepositoryTestHelper.CreateMockDbSet(staffJobTblViews).Object);
            mockContext.Setup(x => x.JobCodes).Returns(RepositoryTestHelper.CreateMockDbSet(jobCodes).Object);
            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.GetZtTotalHoursByStaffIdAsync("S001");

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_ReturnsZero_WhenStaffIdDoesNotExist()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "ZT001", Type = "ZT" }
            };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);
            mockContext.Setup(x => x.StaffJobTblViews).Returns(RepositoryTestHelper.CreateMockDbSet(staffJobTblViews).Object);
            mockContext.Setup(x => x.JobCodes).Returns(RepositoryTestHelper.CreateMockDbSet(jobCodes).Object);
            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.GetZtTotalHoursByStaffIdAsync("S999");

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_IgnoresNonZtTypeJobs()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 20, UserId = DefaultUserId },
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "ZT001", Type = "ZT" },
                new() { JobCodeId = "JOB001", Type = "STD" }
            };

            var mockFpsYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);
            mockContext.Setup(x => x.StaffJobTblViews).Returns(RepositoryTestHelper.CreateMockDbSet(staffJobTblViews).Object);
            mockContext.Setup(x => x.JobCodes).Returns(RepositoryTestHelper.CreateMockDbSet(jobCodes).Object);
            var repo = new StaffJobRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.GetZtTotalHoursByStaffIdAsync("S001");

            // Assert — only ZT001 hours (20) should be summed
            Assert.Equal(20, result);
        }

        #endregion
    }
}

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
            IEnumerable<StaffJobRmView>? staffJobRmViews = null,
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

            if (staffJobRmViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(staffJobRmViews);
                mockContext.Setup(x => x.StaffJobRmViews).Returns(mockSet.Object);
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
        public async Task GetJobStaffCostAsync_CalculatesStaffCost_ForChargeableSector_IsCaseInsensitive()
        {
            // Arrange - SectorName uses mixed case ("Charge") to verify the
            // case-insensitive comparison in ComputeStaffCost.
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
                new() { ProgramNo = "PROG01", UserId = DefaultUserId, SectorName = "Charge" }
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
            Assert.Equal(4000m, staffJobView.StaffCost); // 40 hours * 100 rate * 1 (Charge, case-insensitive)
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

        [Fact]
        public async Task GetJobStaffCostAsync_PopulatesName_FromStaffGeneralView_WhenLookupEmpty()
        {
            // Arrange - staffViews/staffPickViews are empty, so the name can only come
            // from StaffGeneralView (regression test for the blank-name fix).
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
            var staffJobView = result.Data.First();
            Assert.Equal("John Doe", staffJobView.Name);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_SortsByWorkGroupGrade_Ascending()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 30, UserId = DefaultUserId },
                new() { StaffId = "S003", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG03" },
                new() { StaffId = "S002", Name = "Bob", WorkGroupGrade = "WG01" },
                new() { StaffId = "S003", Name = "Charlie", WorkGroupGrade = "WG02" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC01", GradeCode = "G02", Workgroup = "IT" },
                new() { WgGrade = "WG03", ProfitCentreGrade = "PC01", GradeCode = "G03", Workgroup = "IT" }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "WorkGroupGrade", Descending = false };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var data = result.Data.ToList();
            Assert.Equal(3, data.Count);
            Assert.Equal("WG01", data[0].WorkGroupGrade);
            Assert.Equal("WG02", data[1].WorkGroupGrade);
            Assert.Equal("WG03", data[2].WorkGroupGrade);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_SortsByWorkGroupGrade_Descending()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 30, UserId = DefaultUserId },
                new() { StaffId = "S003", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob", WorkGroupGrade = "WG02" },
                new() { StaffId = "S003", Name = "Charlie", WorkGroupGrade = "WG03" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC01", GradeCode = "G02", Workgroup = "IT" },
                new() { WgGrade = "WG03", ProfitCentreGrade = "PC01", GradeCode = "G03", Workgroup = "IT" }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "WorkGroupGrade", Descending = true };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var data = result.Data.ToList();
            Assert.Equal(3, data.Count);
            Assert.Equal("WG03", data[0].WorkGroupGrade);
            Assert.Equal("WG02", data[1].WorkGroupGrade);
            Assert.Equal("WG01", data[2].WorkGroupGrade);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_SortsByName_Ascending()
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
                new() { StaffId = "S001", Name = "Zoe", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Alice", WorkGroupGrade = "WG01" }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "Name", Descending = false };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var data = result.Data.ToList();
            Assert.Equal("Alice", data[0].Name);
            Assert.Equal("Zoe", data[1].Name);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_SortsByPlannedHours_Descending()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 50, UserId = DefaultUserId },
                new() { StaffId = "S003", JobCode = "JOB001", PlannedHours = 30, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob", WorkGroupGrade = "WG01" },
                new() { StaffId = "S003", Name = "Charlie", WorkGroupGrade = "WG01" }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "PlannedHours", Descending = true };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var data = result.Data.ToList();
            Assert.Equal(50, data[0].PlannedHours);
            Assert.Equal(30, data[1].PlannedHours);
            Assert.Equal(20, data[2].PlannedHours);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_SortsByStaffCost_Ascending()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId },
                new() { StaffId = "S003", JobCode = "JOB001", PlannedHours = 30, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob", WorkGroupGrade = "WG01" },
                new() { StaffId = "S003", Name = "Charlie", WorkGroupGrade = "WG01" }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "StaffCost", Descending = false };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var data = result.Data.ToList();
            Assert.Equal(2000m, data[0].StaffCost); // 20 hours * 100 rate
            Assert.Equal(3000m, data[1].StaffCost); // 30 hours * 100 rate
            Assert.Equal(4000m, data[2].StaffCost); // 40 hours * 100 rate
        }

        [Fact]
        public async Task GetJobStaffCostAsync_NoSorting_WhenSortByIsNull()
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
                new() { StaffId = "S001", Name = "Zoe", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Alice", WorkGroupGrade = "WG01" }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = null, Descending = false };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert - Order should remain as original (S002 first, S001 second)
            var data = result.Data.ToList();
            Assert.Equal("S002", data[0].StaffID);
            Assert.Equal("S001", data[1].StaffID);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_NoSorting_WhenSortByIsUnknown()
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
                new() { StaffId = "S001", Name = "Zoe", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Alice", WorkGroupGrade = "WG01" }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "UnknownField", Descending = false };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert - Order should remain as original (default case in switch)
            var data = result.Data.ToList();
            Assert.Equal("S002", data[0].StaffID);
            Assert.Equal("S001", data[1].StaffID);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_SortsByChargeRate_Ascending()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 30, UserId = DefaultUserId },
                new() { StaffId = "S003", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG03" },
                new() { StaffId = "S002", Name = "Bob", WorkGroupGrade = "WG02" },
                new() { StaffId = "S003", Name = "Charlie", WorkGroupGrade = "WG01" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "IT" },
                new() { WgGrade = "WG03", ProfitCentreGrade = "PC03", GradeCode = "G03", Workgroup = "IT" }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ChargeRate = 80, DefraChargeRate = 100 },
                new() { PcGrade = "PC02", ChargeRate = 100, DefraChargeRate = 120 },
                new() { PcGrade = "PC03", ChargeRate = 120, DefraChargeRate = 140 }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "ChargeRate", Descending = false };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var data = result.Data.ToList();
            Assert.Equal(80m, data[0].ChargeRate);
            Assert.Equal(100m, data[1].ChargeRate);
            Assert.Equal(120m, data[2].ChargeRate);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_SortsByChargeRate_Descending()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 30, UserId = DefaultUserId },
                new() { StaffId = "S003", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob", WorkGroupGrade = "WG02" },
                new() { StaffId = "S003", Name = "Charlie", WorkGroupGrade = "WG03" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC02", GradeCode = "G02", Workgroup = "IT" },
                new() { WgGrade = "WG03", ProfitCentreGrade = "PC03", GradeCode = "G03", Workgroup = "IT" }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ChargeRate = 80, DefraChargeRate = 100 },
                new() { PcGrade = "PC02", ChargeRate = 100, DefraChargeRate = 120 },
                new() { PcGrade = "PC03", ChargeRate = 120, DefraChargeRate = 140 }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "ChargeRate", Descending = true };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var data = result.Data.ToList();
            Assert.Equal(120m, data[0].ChargeRate);
            Assert.Equal(100m, data[1].ChargeRate);
            Assert.Equal(80m, data[2].ChargeRate);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_SortsByDays_Ascending()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 16, UserId = DefaultUserId },
                new() { StaffId = "S003", JobCode = "JOB001", PlannedHours = 24, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob", WorkGroupGrade = "WG01" },
                new() { StaffId = "S003", Name = "Charlie", WorkGroupGrade = "WG01" }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "Days", Descending = false };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var data = result.Data.ToList();
            Assert.Equal(3, data.Count);
            Assert.Equal(2, data[0].Days);   // 16 hours / 8 = 2 days
            Assert.Equal(3, data[1].Days);   // 24 hours / 8 = 3 days
            Assert.Equal(5, data[2].Days);   // 40 hours / 8 = 5 days
        }

        [Fact]
        public async Task GetJobStaffCostAsync_SortsByDays_Descending()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 16, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S003", JobCode = "JOB001", PlannedHours = 24, UserId = DefaultUserId }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob", WorkGroupGrade = "WG01" },
                new() { StaffId = "S003", Name = "Charlie", WorkGroupGrade = "WG01" }
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

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "Days", Descending = true };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var data = result.Data.ToList();
            Assert.Equal(3, data.Count);
            Assert.Equal(5, data[0].Days);   // 40 hours / 8 = 5 days
            Assert.Equal(3, data[1].Days);   // 24 hours / 8 = 3 days
            Assert.Equal(2, data[2].Days);   // 16 hours / 8 = 2 days
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


        #region GetStaffSummaryByIdAsync Tests

        [Fact]
        public async Task GetStaffSummaryByIdAsync_ReturnsStaffSummary_WhenStaffExists()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new()
                {
                    StaffId = "S001",
                    Name = "John Doe",
                    WorkgroupGrade = "WG01",
                    HrsAvail = 150,
                    HrsPaid = 140,
                    Leave = 10,
                    SickSpecial = 5,
                    UserEmail = DefaultUserEmail
                }
            };

            var repo = CreateRepository(staffViews: staffViews);

            // Act
            var result = await repo.GetStaffSummaryByIdAsync("S001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffID);
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("WG01", result.WorkGroupGrade);
            Assert.Equal(150, result.HrsAvail);
            Assert.Equal(140, result.HrsPaid);
            Assert.Equal(10, result.Leave);
            Assert.Equal(5, result.SickSpecial);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_ReturnsNull_WhenStaffDoesNotExist()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new()
                {
                    StaffId = "S001",
                    Name = "John Doe",
                    WorkgroupGrade = "WG01",
                    UserEmail = DefaultUserEmail
                }
            };

            var repo = CreateRepository(staffViews: staffViews);

            // Act
            var result = await repo.GetStaffSummaryByIdAsync("S999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_HandlesNullValues_ReturnsDefaults()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new()
                {
                    StaffId = "S001",
                    Name = null,
                    WorkgroupGrade = null,
                    HrsAvail = null,
                    HrsPaid = null,
                    Leave = null,
                    SickSpecial = null,
                    UserEmail = DefaultUserEmail
                }
            };

            var repo = CreateRepository(staffViews: staffViews);

            // Act
            var result = await repo.GetStaffSummaryByIdAsync("S001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffID);
            Assert.Equal("", result.Name);
            Assert.Equal("", result.WorkGroupGrade);
            Assert.Equal(0, result.HrsAvail);
            Assert.Equal(0, result.HrsPaid);
            Assert.Equal(0, result.Leave);
            Assert.Equal(0, result.SickSpecial);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_ReturnsNull_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository(staffViews: new List<StaffView>());

            // Act
            var result = await repo.GetStaffSummaryByIdAsync("S001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_ReturnsFirstMatch_WhenMultipleRecordsExist()
        {
            // Arrange
            var staffViews = new List<StaffView>
            {
                new()
                {
                    StaffId = "S001",
                    Name = "John Doe",
                    WorkgroupGrade = "WG01",
                    HrsAvail = 150,
                    UserEmail = DefaultUserEmail
                },
                new()
                {
                    StaffId = "S001",
                    Name = "John Doe Updated",
                    WorkgroupGrade = "WG02",
                    HrsAvail = 160,
                    UserEmail = DefaultUserEmail
                }
            };

            var repo = CreateRepository(staffViews: staffViews);

            // Act
            var result = await repo.GetStaffSummaryByIdAsync("S001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffID);
            // Should return first match
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("WG01", result.WorkGroupGrade);
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

        #region GetStaffChargeRate Tests

        [Fact]
        public async Task GetStaffChargeRate_ReturnsChargeRate_WhenJobCodeMatches()
        {
            // Arrange
            var wgEmployees = new List<WorkGroupEmployee> { new() { SpNumber = "SP001", PactId = "S001", WorkGroupGrade = "WG01" } };
            var employees = new List<Employee> { new() { SPNumber = "SP001", FirstName = "John Doe" } };
            var workgroupGrades = new List<WorkgroupGrade> { new() { WgGrade = "WG01", ProfitCentreGrade = "PC01" } };
            var profitCentreGrades = new List<ProfitCentreGrade> { new() { PcGrade = "PC01", ChargeRate = 100m, DefraChargeRate = 120m } };
            var staffJobs = new List<StaffJob> { new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 } };
            var projects = new List<Project> { new() { ParentProject = "JOB001", IsDefraProject = 0 } };

            var repo = CreateRepository(staffJobs: staffJobs, workgroupGrades: workgroupGrades, profitCentreGrades: profitCentreGrades, wgEmployees: wgEmployees, employees: employees, projects: projects);

            // Act
            var result = await repo.GetStaffChargeRate("S001", "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100m, result.Value);
        }

        [Fact]
        public async Task GetStaffChargeRate_ReturnsDefraChargeRate_WhenProjectIsDefraProject()
        {
            // Arrange
            var wgEmployees = new List<WorkGroupEmployee> { new() { SpNumber = "SP001", PactId = "S001", WorkGroupGrade = "WG01" } };
            var employees = new List<Employee> { new() { SPNumber = "SP001", FirstName = "John Doe" } };
            var workgroupGrades = new List<WorkgroupGrade> { new() { WgGrade = "WG01", ProfitCentreGrade = "PC01" } };
            var profitCentreGrades = new List<ProfitCentreGrade> { new() { PcGrade = "PC01", ChargeRate = 100m, DefraChargeRate = 120m } };
            var staffJobs = new List<StaffJob> { new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 } };
            var projects = new List<Project> { new() { ParentProject = "JOB001", IsDefraProject = -1 } };

            var repo = CreateRepository(staffJobs: staffJobs, workgroupGrades: workgroupGrades, profitCentreGrades: profitCentreGrades, wgEmployees: wgEmployees, employees: employees, projects: projects);

            // Act
            var result = await repo.GetStaffChargeRate("S001", "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(120m, result.Value);
        }

        [Fact]
        public async Task GetStaffChargeRate_ReturnsFallbackChargeRate_WhenJobCodeDoesNotMatch()
        {
            // Arrange
            var wgEmployees = new List<WorkGroupEmployee> { new() { SpNumber = "SP001", PactId = "S001", WorkGroupGrade = "WG01" } };
            var employees = new List<Employee> { new() { SPNumber = "SP001", FirstName = "John Doe" } };
            var workgroupGrades = new List<WorkgroupGrade> { new() { WgGrade = "WG01", ProfitCentreGrade = "PC01" } };
            var profitCentreGrades = new List<ProfitCentreGrade> { new() { PcGrade = "PC01", ChargeRate = 100m, DefraChargeRate = 120m } };
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40 },
                new() { StaffId = "S001", JobCode = "JOB002", PlannedHours = 20 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", IsDefraProject = 0 },
                new() { ParentProject = "JOB002", IsDefraProject = 0 }
            };

            var repo = CreateRepository(staffJobs: staffJobs, workgroupGrades: workgroupGrades, profitCentreGrades: profitCentreGrades, wgEmployees: wgEmployees, employees: employees, projects: projects);

            // Act
            var result = await repo.GetStaffChargeRate("S001", "JOB999");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100m, result.Value);
        }

        [Fact]
        public async Task GetStaffChargeRate_ReturnsNull_WhenNoStaffJobsFound()
        {
            // Arrange
            var wgEmployees = new List<WorkGroupEmployee> { new() { SpNumber = "SP001", PactId = "S001", WorkGroupGrade = "WG01" } };
            var employees = new List<Employee> { new() { SPNumber = "SP001", FirstName = "John Doe" } };
            var workgroupGrades = new List<WorkgroupGrade> { new() { WgGrade = "WG01", ProfitCentreGrade = "PC01" } };
            var profitCentreGrades = new List<ProfitCentreGrade> { new() { PcGrade = "PC01", ChargeRate = 100m, DefraChargeRate = 120m } };
            var staffJobs = new List<StaffJob> { new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 40 } };
            var projects = new List<Project> { new() { ParentProject = "JOB001", IsDefraProject = 0 } };

            var repo = CreateRepository(staffJobs: staffJobs, workgroupGrades: workgroupGrades, profitCentreGrades: profitCentreGrades, wgEmployees: wgEmployees, employees: employees, projects: projects);

            // Act
            var result = await repo.GetStaffChargeRate("S999", "JOB001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetStaffChargeRate_ReturnsNull_WhenNoDataExists()
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
            var result = await repo.GetStaffChargeRate("S001", "JOB001");

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
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40, UserId = DefaultUserId }
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
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", UserId = DefaultUserId, IsDefraProject = 0, Program = "PROG01", UserEmail = DefaultUserEmail }
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
            var result = await repo.GetViewByStaffIdAsync("S001", "ZT001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("S001", result.StaffID);
            Assert.Equal("ZT001", result.JobCode);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var settings = new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } };
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40, UserId = DefaultUserId }
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
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
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
            var result = await repo.GetViewByStaffIdAsync("S999", "ZT999");

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
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 },
                new() { StaffId = "S001", JobCode = "ZT002", PlannedHours = 20 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { ParentProject = "ZT002", ProjectTitle = "Training", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };

            var repo = CreateRepository(
                staffJobs: staffJobs,
                projectViews: projectViews);

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
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };

            var repo = CreateRepository(
                staffJobs: staffJobs,
                projectViews: projectViews);

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

            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                projectViews: projectViews);

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

            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                projectViews: projectViews);

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

            var repo = CreateRepository(
                staffJobTblViews: staffJobTblViews,
                projectViews: projectViews);

            // Act
            var result = await repo.GetZtStaffJobDetailsByIdAsync("S002", "ZT001");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetStaffResourceUtilisationAsync Tests

        private static (List<WorkgroupGrade> WorkgroupGrades,
                        List<StaffView> StaffViews,
                        List<ProfitCentreGrade> ProfitCentreGrades,
                        List<StaffJobRmView> StaffJobRmViews,
                        List<Project> Projects)
            BuildUtilisationDataset(double? hrsAvail = 100)
        {
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", Workgroup = "IT", FpsYear = DefaultTestFpsYear }
            };
            var staffViews = new List<StaffView>
            {
                new()
                {
                    StaffId = "S001",
                    Name = "John General",
                    WorkgroupGrade = "WG01",
                    FpsYear = DefaultTestFpsYear,
                    UserEmail = DefaultUserEmail,
                    HrsAvail = hrsAvail
                }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PC01", ProfitCentre = "PC-Alpha", FpsYear = DefaultTestFpsYear }
            };
            var staffJobRmViews = new List<StaffJobRmView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, FpsYear = DefaultTestFpsYear },
                new() { StaffId = "S001", JobCode = "JOB002", PlannedHours = 10, FpsYear = DefaultTestFpsYear },
                new() { StaffId = "S001", JobCode = "ZTJOB",  PlannedHours = 5,  FpsYear = DefaultTestFpsYear }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "JOB001", Program = "prog",    ProjectStatus = "Approved",     FpsYear = DefaultTestFpsYear },
                new() { ParentProject = "JOB002", Program = "prog",    ProjectStatus = "Not Approved", FpsYear = DefaultTestFpsYear },
                new() { ParentProject = "ZTJOB",  Program = "zt_prog", ProjectStatus = "Approved",     FpsYear = DefaultTestFpsYear }
            };

            return (workgroupGrades, staffViews, profitCentreGrades, staffJobRmViews, projects);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_ReturnsAggregatedData_WithValidWorkgroup()
        {
            // Arrange
            var data = BuildUtilisationDataset();
            var repo = CreateRepository(
                workgroupGrades: data.WorkgroupGrades,
                staffViews: data.StaffViews,
                profitCentreGrades: data.ProfitCentreGrades,
                staffJobRmViews: data.StaffJobRmViews,
                projects: data.Projects);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffResourceUtilisationAsync(query, "IT");

            // Assert
            Assert.NotNull(result);
            var row = Assert.Single(result.Data);
            Assert.Equal("S001", row.StaffId);
            Assert.Equal("PC-Alpha", row.ProfitCentre);
            Assert.Equal("IT", row.WorkGroup);
            Assert.Equal(100, row.HrsAvail);
            // plannedZt = 5 (zt_prog), approvedRaw = 40+5 = 45, nApproved = 10
            Assert.Equal(5, row.PlannedZt);
            Assert.Equal(40, row.ApprovedSoct);   // 45 - 5
            Assert.Equal(95, row.AvailSoct);      // 100 - 5
            Assert.Equal(10, row.NotApprovedSoct);
            Assert.Equal(45, row.Left);           // 95 - 40 - 10
            Assert.Equal(40, row.ApprovedUtilPct);
            Assert.Equal(10, row.NotApprovedUtilPct);
            Assert.Equal(50, row.TotalUtilPct);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_ReturnsNullPercentages_WhenHrsAvailIsZero()
        {
            // Arrange
            var data = BuildUtilisationDataset(hrsAvail: 0);
            var repo = CreateRepository(
                workgroupGrades: data.WorkgroupGrades,
                staffViews: data.StaffViews,
                profitCentreGrades: data.ProfitCentreGrades,
                staffJobRmViews: data.StaffJobRmViews,
                projects: data.Projects);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffResourceUtilisationAsync(query, "IT");

            // Assert
            var row = Assert.Single(result.Data);
            Assert.Null(row.ApprovedUtilPct);
            Assert.Null(row.NotApprovedUtilPct);
            Assert.Null(row.TotalUtilPct);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_ReturnsEmpty_WhenWorkgroupDoesNotMatch()
        {
            // Arrange
            var data = BuildUtilisationDataset();
            var repo = CreateRepository(
                workgroupGrades: data.WorkgroupGrades,
                staffViews: data.StaffViews,
                profitCentreGrades: data.ProfitCentreGrades,
                staffJobRmViews: data.StaffJobRmViews,
                projects: data.Projects);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffResourceUtilisationAsync(query, "HR");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_AppliesWgGradeAndNameFilter()
        {
            // Arrange
            var data = BuildUtilisationDataset();
            var repo = CreateRepository(
                workgroupGrades: data.WorkgroupGrades,
                staffViews: data.StaffViews,
                profitCentreGrades: data.ProfitCentreGrades,
                staffJobRmViews: data.StaffJobRmViews,
                projects: data.Projects);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"WgGrade\":\"WG01\",\"Name\":\"John\"}" };

            // Act
            var result = await repo.GetStaffResourceUtilisationAsync(query, "IT");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("S001", result.Data.First().StaffId);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_FiltersOut_WhenNameDoesNotMatch()
        {
            // Arrange
            var data = BuildUtilisationDataset();
            var repo = CreateRepository(
                workgroupGrades: data.WorkgroupGrades,
                staffViews: data.StaffViews,
                profitCentreGrades: data.ProfitCentreGrades,
                staffJobRmViews: data.StaffJobRmViews,
                projects: data.Projects);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"Name\":\"Nonexistent\"}" };

            // Act
            var result = await repo.GetStaffResourceUtilisationAsync(query, "IT");

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_IgnoresNullFilterValues()
        {
            // Arrange
            var data = BuildUtilisationDataset();
            var repo = CreateRepository(
                workgroupGrades: data.WorkgroupGrades,
                staffViews: data.StaffViews,
                profitCentreGrades: data.ProfitCentreGrades,
                staffJobRmViews: data.StaffJobRmViews,
                projects: data.Projects);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"WgGrade\":null,\"Name\":null}" };

            // Act
            var result = await repo.GetStaffResourceUtilisationAsync(query, "IT");

            // Assert
            Assert.Single(result.Data);
        }

        [Theory]
        [InlineData("Name", false)]
        [InlineData("Name", true)]
        [InlineData("HrsAvail", true)]
        [InlineData("PlannedZt", false)]
        [InlineData("AvailSoct", true)]
        [InlineData("NotApprovedSoct", false)]
        [InlineData("ApprovedSoct", true)]
        [InlineData("Left", false)]
        [InlineData("ApprovedUtilPct", true)]
        [InlineData("NotApprovedUtilPct", false)]
        [InlineData("TotalUtilPct", true)]
        [InlineData("Unknown", true)]
        [InlineData(null, false)]
        public async Task GetStaffResourceUtilisationAsync_AppliesSorting_ForEachSortKey(string? sortBy, bool descending)
        {
            // Arrange - two staff so ordering is exercised
            var (workgroupGrades, staffViews, profitCentreGrades, staffJobRmViews, projects) = BuildUtilisationDataset();
            workgroupGrades.Add(new WorkgroupGrade { WgGrade = "WG02", ProfitCentreGrade = "PC01", Workgroup = "IT", FpsYear = DefaultTestFpsYear });
            staffViews.Add(new StaffView
            {
                StaffId = "S002",
                Name = "Amy General",
                WorkgroupGrade = "WG02",
                FpsYear = DefaultTestFpsYear,
                UserEmail = DefaultUserEmail,
                HrsAvail = 80
            });
            staffJobRmViews.Add(new StaffJobRmView { StaffId = "S002", JobCode = "JOB001", PlannedHours = 20, FpsYear = DefaultTestFpsYear });

            var repo = CreateRepository(
                workgroupGrades: workgroupGrades,
                staffViews: staffViews,
                profitCentreGrades: profitCentreGrades,
                staffJobRmViews: staffJobRmViews,
                projects: projects);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetStaffResourceUtilisationAsync(query, "IT");

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetJobStaffCostAsync In-Memory Filter Tests

        private static StaffJobRepository CreateJobStaffCostRepository(IEnumerable<StaffJobTblView> staffJobTblViews)
        {
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "John Doe", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Amy Smith", WorkGroupGrade = "WG02" }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "PC01", GradeCode = "G01", Workgroup = "IT" },
                new() { WgGrade = "WG02", ProfitCentreGrade = "PC01", GradeCode = "G02", Workgroup = "IT" }
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

            return CreateRepository(
                staffJobTblViews: staffJobTblViews,
                staffGeneralViews: staffGeneralViews,
                workgroupGrades: workgroupGrades,
                profitCentreGrades: profitCentreGrades,
                projectViews: projectViews,
                programViews: programViews,
                staffViews: new List<StaffView>(),
                staffPickViews: new List<StaffPickView>(),
                settings: new List<FpsSetting> { new() { Id = "HoursInDay", Setting = "8" } });
        }

        [Fact]
        public async Task GetJobStaffCostAsync_FiltersByPlannedHours_MatchesExactValue()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
            };
            var repo = CreateJobStaffCostRepository(staffJobTblViews);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"PlannedHours\":\"40\"}" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var single = Assert.Single(result.Data);
            Assert.Equal(40, single.PlannedHours, 4);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_FiltersByPlannedHours_MatchesWithinTolerance()
        {
            // Arrange - stored value differs from the filter by less than the 0.0001 tolerance
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40.00001, UserId = DefaultUserId }
            };
            var repo = CreateJobStaffCostRepository(staffJobTblViews);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"PlannedHours\":\"40\"}" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert - exact equality would have missed this row
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_FiltersByPlannedHours_ExcludesOutsideTolerance()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40.5, UserId = DefaultUserId }
            };
            var repo = CreateJobStaffCostRepository(staffJobTblViews);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"PlannedHours\":\"40\"}" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_FiltersByPlannedHours_IgnoresNonNumericValue()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
            };
            var repo = CreateJobStaffCostRepository(staffJobTblViews);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"PlannedHours\":\"abc\"}" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert - unparseable filter value leaves the list untouched
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetJobStaffCostAsync_FiltersByPlannedHours_MatchesNegativeValue()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = -10, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
            };
            var repo = CreateJobStaffCostRepository(staffJobTblViews);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"PlannedHours\":\"-10\"}" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var single = Assert.Single(result.Data);
            Assert.Equal(-10, single.PlannedHours, 4);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_FiltersByWorkGroupGrade()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 20, UserId = DefaultUserId }
            };
            var repo = CreateJobStaffCostRepository(staffJobTblViews);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"WorkGroupGrade\":\"wg01\"}" };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var single = Assert.Single(result.Data);
            Assert.Equal("WG01", single.WorkGroupGrade);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_FiltersByPlannedHoursAndWorkGroupGrade_Combined()
        {
            // Arrange
            var staffJobTblViews = new List<StaffJobTblView>
            {
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId },
                new() { StaffId = "S002", JobCode = "JOB001", PlannedHours = 40, UserId = DefaultUserId }
            };
            var repo = CreateJobStaffCostRepository(staffJobTblViews);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"WorkGroupGrade\":\"WG02\",\"PlannedHours\":\"40\"}"
            };

            // Act
            var result = await repo.GetJobStaffCostAsync(query, "JOB001");

            // Assert
            var single = Assert.Single(result.Data);
            Assert.Equal("WG02", single.WorkGroupGrade);
        }

        #endregion

        #region GetZtTotalHoursByStaffIdAsync Tests

        private static (List<StaffJob> StaffJobs, List<ProjectView> ProjectViews) BuildZtDataset()
        {
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 },
                new() { StaffId = "S001", JobCode = "ZT002", PlannedHours = 20.5 },
                new() { StaffId = "S002", JobCode = "ZT001", PlannedHours = 15 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { ParentProject = "ZT002", ProjectTitle = "Training", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            return (staffJobs, projectViews);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_SumsPlannedHours_ForMatchingStaff()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            // Act
            var result = await repo.GetZtTotalHoursByStaffIdAsync("S001");

            // Assert
            Assert.Equal(60.5, result, 4);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_ReturnsZero_WhenStaffHasNoZtJobs()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            // Act
            var result = await repo.GetZtTotalHoursByStaffIdAsync("S999");

            // Assert
            Assert.Equal(0, result, 4);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_ExcludesNonZtPrograms()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 },
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 100 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { ParentProject = "JOB001", ProjectTitle = "Real Work", Program = "PROG01", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            // Act
            var result = await repo.GetZtTotalHoursByStaffIdAsync("S001");

            // Assert
            Assert.Equal(40, result, 4);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_ExcludesRowsWithNullProgram()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 },
                new() { StaffId = "S001", JobCode = "ZT002", PlannedHours = 25 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { ParentProject = "ZT002", ProjectTitle = "Unmapped", Program = null, UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            // Act
            var result = await repo.GetZtTotalHoursByStaffIdAsync("S001");

            // Assert
            Assert.Equal(40, result, 4);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_CountsDuplicateRowsOnce()
        {
            // Arrange - duplicate project rows would otherwise inflate the join result
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work Copy", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            // Act
            var result = await repo.GetZtTotalHoursByStaffIdAsync("S001");

            // Assert
            Assert.Equal(40, result, 4);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_HandlesNegativeHours()
        {
            // Arrange - negative planned hours are permitted after the Range relaxation
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 },
                new() { StaffId = "S001", JobCode = "ZT002", PlannedHours = -10 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { ParentProject = "ZT002", ProjectTitle = "Correction", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            // Act
            var result = await repo.GetZtTotalHoursByStaffIdAsync("S001");

            // Assert
            Assert.Equal(30, result, 4);
        }

        #endregion

        #region GetZtStaffJobsByStaffIdPagedAsync Sorting And Filtering Tests

        [Theory]
        [InlineData("jobcode", false)]
        [InlineData("jobcode", true)]
        [InlineData("plannedhours", false)]
        [InlineData("plannedhours", true)]
        [InlineData("name", false)]
        [InlineData("name", true)]
        [InlineData("ztdescription", false)]
        [InlineData("ztdescription", true)]
        [InlineData("unknownfield", false)]
        [InlineData(null, false)]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_AppliesSorting_ForEachSortKey(string? sortBy, bool descending)
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_SortsByZtDescription_SameAsName()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var byName = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "name", Descending = false };
            var byZtDescription = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "ztdescription", Descending = false };

            // Act
            var nameResult = await repo.GetZtStaffJobsByStaffIdPagedAsync(byName, "S001");
            var ztResult = await repo.GetZtStaffJobsByStaffIdPagedAsync(byZtDescription, "S001");

            // Assert - ztdescription is an alias for the Name column
            Assert.Equal(
                nameResult.Data.Select(x => x.JobCode),
                ztResult.Data.Select(x => x.JobCode));
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_SortsByZtDescriptionDescending_ReversesOrder()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var ascending = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "ztdescription", Descending = false };
            var descending = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "ztdescription", Descending = true };

            // Act
            var ascResult = await repo.GetZtStaffJobsByStaffIdPagedAsync(ascending, "S001");
            var descResult = await repo.GetZtStaffJobsByStaffIdPagedAsync(descending, "S001");

            // Assert
            Assert.Equal(
                ascResult.Data.Select(x => x.Name).Reverse(),
                descResult.Data.Select(x => x.Name));
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_FiltersByName()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Name\":\"Training\"}"
            };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            var single = Assert.Single(result.Data);
            Assert.Equal("ZT002", single.JobCode);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_FiltersByName_IsCaseInsensitive()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Name\":\"tRaInInG\"}"
            };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            var single = Assert.Single(result.Data);
            Assert.Equal("ZT002", single.JobCode);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_FiltersByPlannedHours()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"PlannedHours\":\"40\"}"
            };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            var single = Assert.Single(result.Data);
            Assert.Equal("ZT001", single.JobCode);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_FiltersByNameAndPlannedHours_Combined()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Name\":\"Admin\",\"PlannedHours\":\"40\"}"
            };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            var single = Assert.Single(result.Data);
            Assert.Equal("ZT001", single.JobCode);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_ReturnsEmpty_WhenFilterMatchesNothing()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Name\":\"Nonexistent\"}"
            };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            Assert.Empty(result.Data);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_IgnoresEmptyFilter(string? filter)
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_IgnoresNullFilterValues()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Name\":null,\"PlannedHours\":null}"
            };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_AppliesPaging()
        {
            // Arrange
            var (staffJobs, projectViews) = BuildZtDataset();
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 1, SortBy = "jobcode", Descending = false };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_ExcludesNonZtPrograms()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = 40 },
                new() { StaffId = "S001", JobCode = "JOB001", PlannedHours = 100 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Admin Work", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail },
                new() { ParentProject = "JOB001", ProjectTitle = "Real Work", Program = "PROG01", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            var single = Assert.Single(result.Data);
            Assert.Equal("ZT001", single.JobCode);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_PreservesNegativePlannedHours()
        {
            // Arrange
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "ZT001", PlannedHours = -5 }
            };
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Correction", Program = "zt_prog", UserId = DefaultUserId, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(staffJobs: staffJobs, projectViews: projectViews);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetZtStaffJobsByStaffIdPagedAsync(query, "S001");

            // Assert
            var single = Assert.Single(result.Data);
            Assert.Equal(-5, single.PlannedHours, 4);
        }

        #endregion
    }
}

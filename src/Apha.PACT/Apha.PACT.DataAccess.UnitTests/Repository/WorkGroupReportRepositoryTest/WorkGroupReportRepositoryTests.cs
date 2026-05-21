using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.WorkGroupReportRepositoryTest
{
    public class WorkGroupReportRepositoryTests
    {
        private static WorkGroupReportRepository CreateRepository(
            IEnumerable<PactProfitCentreView>? profitCentreViews = null,
            IEnumerable<WorkGroup>? workGroups = null,
            IEnumerable<TimeCodeValid>? timeCodeValids = null,
            IEnumerable<PactWorkGroupGradeView>? workGroupGradeViews = null,
            IEnumerable<PactStaffView>? staffViews = null,
            IEnumerable<JobCode>? jobCodes = null,
            IEnumerable<TestorProduct>? testorProducts = null,
            IEnumerable<TestCapability>? testCapabilities = null,
            IEnumerable<TestRequirement>? testRequirements = null)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            mockContext.Setup(x => x.PactProfitCentreViews)
                .Returns(RepositoryTestHelper.CreateMockDbSet(profitCentreViews ?? []).Object);
            mockContext.Setup(x => x.WorkGroups)
                .Returns(RepositoryTestHelper.CreateMockDbSet(workGroups ?? []).Object);
            mockContext.Setup(x => x.TimeCodeValids)
                .Returns(RepositoryTestHelper.CreateMockDbSet(timeCodeValids ?? []).Object);
            mockContext.Setup(x => x.PactWorkGroupGradeViews)
                .Returns(RepositoryTestHelper.CreateMockDbSet(workGroupGradeViews ?? []).Object);
            mockContext.Setup(x => x.PactStaffViews)
                .Returns(RepositoryTestHelper.CreateMockDbSet(staffViews ?? []).Object);
            mockContext.Setup(x => x.JobCodes)
                .Returns(RepositoryTestHelper.CreateMockDbSet(jobCodes ?? []).Object);
            mockContext.Setup(x => x.TestorProducts)
                .Returns(RepositoryTestHelper.CreateMockDbSet(testorProducts ?? []).Object);
            mockContext.Setup(x => x.TestCapabilities)
                .Returns(RepositoryTestHelper.CreateMockDbSet(testCapabilities ?? []).Object);
            mockContext.Setup(x => x.TestRequirements)
                .Returns(RepositoryTestHelper.CreateMockDbSet(testRequirements ?? []).Object);

            return new WorkGroupReportRepository(mockContext.Object);
        }

        #region GetProfitCentreAsync

        [Fact]
        public async Task GetProfitCentreAsync_MatchingProfitCentre_ReturnsEntity()
        {
            var views = new List<PactProfitCentreView>
            {
                new() { ProfitCentre = "PC1", ProfitCentreName = "Centre One" },
                new() { ProfitCentre = "PC2", ProfitCentreName = "Centre Two" }
            };
            var repo = CreateRepository(profitCentreViews: views);

            var result = await repo.GetProfitCentreAsync("PC1");

            Assert.NotNull(result);
            Assert.Equal("PC1", result.ProfitCentre);
            Assert.Equal("Centre One", result.ProfitCentreName);
        }

        [Fact]
        public async Task GetProfitCentreAsync_NoMatchingProfitCentre_ReturnsNull()
        {
            var views = new List<PactProfitCentreView>
            {
                new() { ProfitCentre = "PC1" }
            };
            var repo = CreateRepository(profitCentreViews: views);

            var result = await repo.GetProfitCentreAsync("MISSING");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfitCentreAsync_EmptyData_ReturnsNull()
        {
            var repo = CreateRepository(profitCentreViews: []);

            var result = await repo.GetProfitCentreAsync("PC1");

            Assert.Null(result);
        }

        #endregion

        #region GetWorkGroupsForEmailAsync

        [Fact]
        public async Task GetWorkGroupsForEmailAsync_FiltersBySendEmailAndProfitCentre_ReturnsMatchingOrderedList()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "ZWG", ProfitCentre = "PC1", SendEmail = 1 },
                new() { WorkGroupName = "AWG", ProfitCentre = "PC1", SendEmail = 1 },
                new() { WorkGroupName = "MWG", ProfitCentre = "PC1", SendEmail = 0 },
                new() { WorkGroupName = "BWG", ProfitCentre = "PC2", SendEmail = 1 }
            };
            var repo = CreateRepository(workGroups: workGroups);

            var result = (await repo.GetWorkGroupsForEmailAsync("PC1")).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("AWG", result[0].WorkGroupName);
            Assert.Equal("ZWG", result[1].WorkGroupName);
            Assert.All(result, wg => Assert.Equal("PC1", wg.ProfitCentre));
            Assert.All(result, wg => Assert.Equal((short)1, wg.SendEmail));
        }

        [Fact]
        public async Task GetWorkGroupsForEmailAsync_NoSendEmailWorkGroups_ReturnsEmptyList()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", SendEmail = 0 },
                new() { WorkGroupName = "WG2", ProfitCentre = "PC1", SendEmail = 0 }
            };
            var repo = CreateRepository(workGroups: workGroups);

            var result = await repo.GetWorkGroupsForEmailAsync("PC1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWorkGroupsForEmailAsync_NoProfitCentreMatch_ReturnsEmptyList()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC2", SendEmail = 1 }
            };
            var repo = CreateRepository(workGroups: workGroups);

            var result = await repo.GetWorkGroupsForEmailAsync("PC1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWorkGroupsForEmailAsync_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateRepository(workGroups: []);

            var result = await repo.GetWorkGroupsForEmailAsync("PC1");

            Assert.Empty(result);
        }

        #endregion

        #region GetTimeSheetTemplateAsync — Layout 1 (Flat-file)

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout1_ReturnsOneRowPerStaffTimeCodeParentProject()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" }
            };
            var staffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkgroupGrade = "GR1", PersonStatus = "A" }
            };
            var repo = CreateRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                staffViews: staffViews);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 3, layout: 1)).ToList();

            Assert.Single(result);
            Assert.Equal("Alice", result[0].StaffName);
            Assert.Equal("TC1", result[0].TimeCode);
            Assert.Equal("PRJ1", result[0].ParentProject);
            Assert.Equal((short)3, result[0].Month);
            Assert.Null(result[0].Hours);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout1_ExcludesInactiveTimeCodes()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Active = true,  FpsYear = 2024 },
                new() { TimeCode = "TC2", WorkGroup = "WG1", ParentProject = "PRJ1", Active = false, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" }
            };
            var staffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkgroupGrade = "GR1", PersonStatus = "A" }
            };
            var repo = CreateRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                staffViews: staffViews);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 3, layout: 1)).ToList();

            Assert.Single(result);
            Assert.Equal("TC1", result[0].TimeCode);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout1_NoMatchingWorkGroup_ReturnsEmptyList()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG2", ParentProject = "PRJ1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG2", WgGrade = "GR1" }
            };
            var staffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkgroupGrade = "GR1", PersonStatus = "A" }
            };
            var repo = CreateRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                staffViews: staffViews);

            var result = await repo.GetTimeSheetTemplateAsync("WG1", 3, layout: 1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout1_MultipleStaffSameTimeCode_ReturnsRowPerStaff()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" },
                new() { WorkGroup = "WG1", WgGrade = "GR2" }
            };
            var staffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkgroupGrade = "GR1", PersonStatus = "A" },
                new() { Name = "Bob",   WorkgroupGrade = "GR2", PersonStatus = "A" }
            };
            var repo = CreateRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                staffViews: staffViews);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 3, layout: 1)).ToList();

            Assert.Equal(2, result.Count);
        }

        #endregion

        #region GetTimeSheetTemplateAsync — Layout 2 (Cross-tab)

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_ReturnsGroupedRowPerTimeCodeParentProject()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" }
            };
            var staffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkgroupGrade = "GR1", PersonStatus = "A" }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", JobCodeName = "Job One", FpsYear = 2024 }
            };
            var repo = CreateRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                staffViews: staffViews,
                jobCodes: jobCodes);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2)).ToList();

            Assert.Single(result);
            Assert.Equal("TC1", result[0].TimeCode);
            Assert.Equal("PRJ1", result[0].ParentProject);
            Assert.Equal((short)4, result[0].Month);
            Assert.Null(result[0].Hours);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_ExcludesInactiveStaff()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" },
                new() { WorkGroup = "WG1", WgGrade = "GR2" }
            };
            var staffViews = new List<PactStaffView>
            {
                new() { Name = "Alice",    WorkgroupGrade = "GR1", PersonStatus = "A" },
                new() { Name = "Inactive", WorkgroupGrade = "GR2", PersonStatus = "I" }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", JobCodeName = "Job One", FpsYear = 2024 }
            };
            var repo = CreateRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                staffViews: staffViews,
                jobCodes: jobCodes);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2)).ToList();

            Assert.Single(result);
            Assert.Equal("Alice", result[0].StaffName);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_MultipleStaffSameTimeCode_GroupsIntoOneRowWithCommaNames()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" },
                new() { WorkGroup = "WG1", WgGrade = "GR2" }
            };
            var staffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkgroupGrade = "GR1", PersonStatus = "A" },
                new() { Name = "Bob",   WorkgroupGrade = "GR2", PersonStatus = "A" }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", JobCodeName = "Job One", FpsYear = 2024 }
            };
            var repo = CreateRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                staffViews: staffViews,
                jobCodes: jobCodes);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2)).ToList();

            Assert.Single(result);
            Assert.Contains("Alice", result[0].StaffName);
            Assert.Contains("Bob", result[0].StaffName);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_UsesJobCodeNameWhenJobCodePresent()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" }
            };
            var staffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkgroupGrade = "GR1", PersonStatus = "A" }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", JobCodeName = "Job One", FpsYear = 2024 }
            };
            var repo = CreateRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                staffViews: staffViews,
                jobCodes: jobCodes);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2)).ToList();

            Assert.Single(result);
            Assert.Equal("Job One", result[0].Description);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_UsesItemDescriptionWhenNoJobCode()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" }
            };
            var staffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkgroupGrade = "GR1", PersonStatus = "A" }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TST1", ItemDescription = "Blood Test" }
            };
            var repo = CreateRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                staffViews: staffViews,
                testorProducts: testorProducts);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2)).ToList();

            Assert.Single(result);
            Assert.Equal("Blood Test", result[0].Description);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateRepository();

            var result = await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2);

            Assert.Empty(result);
        }

        #endregion

        #region GetOutputSheetTemplateAsync

        [Fact]
        public async Task GetOutputSheetTemplateAsync_MatchingWorkGroup_ReturnsOrderedRows()
        {
            var testCapabilities = new List<TestCapability>
            {
                new() { TestCode = "B", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2024 },
                new() { TestCode = "A", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2024 }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { TestCode = "B", Buyer = "BuyerB", Active = 1, FpsYear = 2024 },
                new() { TestCode = "A", Buyer = "BuyerA", Active = 1, FpsYear = 2024 }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "B", ItemDescription = "Blood Test" },
                new() { ItemCode = "A", ItemDescription = "Anthrax Test" }
            };
            var repo = CreateRepository(
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                testorProducts: testorProducts);

            var result = (await repo.GetOutputSheetTemplateAsync("WG1", month: 5)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("A", result[0].TestCode);
            Assert.Equal("B", result[1].TestCode);
        }

        [Fact]
        public async Task GetOutputSheetTemplateAsync_FiltersOutInactiveTestRequirements()
        {
            var testCapabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2024 },
                new() { TestCode = "TC2", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2024 }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { TestCode = "TC1", Buyer = "B1", Active = 1,  FpsYear = 2024 },
                new() { TestCode = "TC2", Buyer = "B2", Active = 0,  FpsYear = 2024 }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Test 1" },
                new() { ItemCode = "TC2", ItemDescription = "Test 2" }
            };
            var repo = CreateRepository(
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                testorProducts: testorProducts);

            var result = (await repo.GetOutputSheetTemplateAsync("WG1", month: 5)).ToList();

            Assert.Single(result);
            Assert.Equal("TC1", result[0].TestCode);
        }

        [Fact]
        public async Task GetOutputSheetTemplateAsync_SetsMonthAndNullVolume()
        {
            var testCapabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2024 }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { TestCode = "TC1", Buyer = "B1", Active = 1, FpsYear = 2024 }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Test 1" }
            };
            var repo = CreateRepository(
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                testorProducts: testorProducts);

            var result = (await repo.GetOutputSheetTemplateAsync("WG1", month: 7)).ToList();

            Assert.Single(result);
            Assert.Equal((short)7, result[0].Month);
            Assert.Null(result[0].Volume);
        }

        [Fact]
        public async Task GetOutputSheetTemplateAsync_NoMatchingWorkGroup_ReturnsEmptyList()
        {
            var testCapabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG2", PlanPortfolio = "PP1", FpsYear = 2024 }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { TestCode = "TC1", Buyer = "B1", Active = 1, FpsYear = 2024 }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Test 1" }
            };
            var repo = CreateRepository(
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                testorProducts: testorProducts);

            var result = await repo.GetOutputSheetTemplateAsync("WG1", month: 5);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOutputSheetTemplateAsync_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateRepository();

            var result = await repo.GetOutputSheetTemplateAsync("WG1", month: 5);

            Assert.Empty(result);
        }

        #endregion
    }
}

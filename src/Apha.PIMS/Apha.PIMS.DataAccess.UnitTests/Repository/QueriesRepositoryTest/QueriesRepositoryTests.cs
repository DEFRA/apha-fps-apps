using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.QueriesRepositoryTest
{
    public class QueriesRepositoryTests
    {
        private static QueriesRepository CreateRepository(
            IEnumerable<Report>? reports = null,
            IEnumerable<Projects>? myTlkpProjects = null,
            IEnumerable<Project>? projects = null,
            IEnumerable<RadTrackContract>? radTrackContracts = null,
            IEnumerable<FpsYearTotal>? fpsYearTotals = null,
            IEnumerable<ProjectMonthFinal>? projectMonthFinals = null,
            IEnumerable<Comment>? comments = null,
            IEnumerable<RadtrackProg>? radtrackProgs = null,
            IEnumerable<YearlyFinancialData>? yearlyFinancialData = null,
            IEnumerable<ProjectRadTrackData>? projectRadTrackData = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var reportsDbSet = RepositoryTestHelper.CreateMockDbSet(reports ?? Enumerable.Empty<Report>());
            var myTlkpProjectsDbSet = RepositoryTestHelper.CreateMockDbSet(myTlkpProjects ?? Enumerable.Empty<Projects>());
            var projectsDbSet = RepositoryTestHelper.CreateMockDbSet(projects ?? Enumerable.Empty<Project>());
            var radTrackContractsDbSet = RepositoryTestHelper.CreateMockDbSet(radTrackContracts ?? Enumerable.Empty<RadTrackContract>());
            var fpsYearTotalsDbSet = RepositoryTestHelper.CreateMockDbSet(fpsYearTotals ?? Enumerable.Empty<FpsYearTotal>());
            var projectMonthFinalsDbSet = RepositoryTestHelper.CreateMockDbSet(projectMonthFinals ?? Enumerable.Empty<ProjectMonthFinal>());
            var commentsDbSet = RepositoryTestHelper.CreateMockDbSet(comments ?? Enumerable.Empty<Comment>());
            var radtrackProgsDbSet = RepositoryTestHelper.CreateMockDbSet(radtrackProgs ?? Enumerable.Empty<RadtrackProg>());
            var yearlyFinancialDataDbSet = RepositoryTestHelper.CreateMockDbSet(yearlyFinancialData ?? Enumerable.Empty<YearlyFinancialData>());
            var projectRadTrackDataDbSet = RepositoryTestHelper.CreateMockDbSet(projectRadTrackData ?? Enumerable.Empty<ProjectRadTrackData>());

            mockContext.Setup(x => x.Reports).Returns(reportsDbSet.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(myTlkpProjectsDbSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsDbSet.Object);
            mockContext.Setup(x => x.RadTrackContracts).Returns(radTrackContractsDbSet.Object);
            mockContext.Setup(x => x.FpsYearTotals).Returns(fpsYearTotalsDbSet.Object);
            mockContext.Setup(x => x.ProjectMonthFinals).Returns(projectMonthFinalsDbSet.Object);
            mockContext.Setup(x => x.Comments).Returns(commentsDbSet.Object);
            mockContext.Setup(x => x.RadtrackProgs).Returns(radtrackProgsDbSet.Object);
            mockContext.Setup(x => x.YearlyFinancialData).Returns(yearlyFinancialDataDbSet.Object);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(projectRadTrackDataDbSet.Object);

            return new QueriesRepository(mockContext.Object);
        }

        private static PaginationParameters<string> Params(
            int page = 1,
            int pageSize = 10,
            string? filter = "{}",
            string? sortBy = null,
            bool descending = false)
            => new() { Page = page, PageSize = pageSize, Filter = filter, SortBy = sortBy, Descending = descending };

        [Fact]
        public async Task GetQueryReportsAsync_ReturnsOnlyTypeQ_OrderedByDescriptionThenName()
        {
            // Arrange
            var repo = CreateRepository(reports:
            [
                new Report { Id = 1, Type = "R", ReportName = "Z-NonQuery", ReportDescription = "ZZ" },
                new Report { Id = 2, Type = "Q", ReportName = "B-Report", ReportDescription = "Alpha" },
                new Report { Id = 3, Type = "Q", ReportName = "A-Report", ReportDescription = "Alpha" },
                new Report { Id = 4, Type = "Q", ReportName = "C-Report", ReportDescription = "Beta" }
            ]);

            // Act
            var result = await repo.GetQueryReportsAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("A-Report", result[0].ReportName);
            Assert.Equal("B-Report", result[1].ReportName);
            Assert.Equal("C-Report", result[2].ReportName);
            Assert.DoesNotContain(result, r => r.ReportName == "Z-NonQuery");
        }

        [Fact]
        public async Task GetMonitoringReportDataAsync_WithExactContractFilter_ReturnsMatchingRows()
        {
            // Arrange
            var myProjects = new List<Projects>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "TB", Manager = "M1", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Program = "AMR", Manager = "M2", Projectstatus = "Live" }
            };

            var baseProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "Project 1", Contract = "LabTGen" },
                new() { Parentproject = "PP002", Projecttitle = "Project 2", Contract = "Other" }
            };

            var radContracts = new List<RadTrackContract>
            {
                new() { Contract = "LabTGen" },
                new() { Contract = "Other" }
            };

            var yearTotals = new List<FpsYearTotal>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "TB", Totalcosts = 100.0, Customer = "C1", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Program = "AMR", Totalcosts = 200.0, Customer = "C2", Projectstatus = "Live" }
            };

            var monthFinals = new List<ProjectMonthFinal>
            {
                new() { Year = 2025, Project = "PP001", Monthno = 6, Cumcost = 10m },
                new() { Year = 2025, Project = "PP002", Monthno = 6, Cumcost = 20m }
            };

            var comments = new List<Comment>
            {
                new() { Project = "PP001", Year = 2025, Topic = "P&C Monitoring Report", CommentText = "comment-1" },
                new() { Project = "PP002", Year = 2025, Topic = "P&C Monitoring Report", CommentText = "comment-2" }
            };

            var repo = CreateRepository(
                myTlkpProjects: myProjects,
                projects: baseProjects,
                radTrackContracts: radContracts,
                fpsYearTotals: yearTotals,
                projectMonthFinals: monthFinals,
                comments: comments,
                radtrackProgs: new List<RadtrackProg>());

            // Act
            var result = await repo.GetMonitoringReportDataAsync(
                Params(),
                reportYear: 2025,
                fiscalMonth: 6,
                contractFilter: "LabTGen",
                programFilter: null);

            // Assert
            Assert.Equal(1, result.PaginationData.TotalRecords);
            var row = Assert.Single(result.Data);
            Assert.Equal("PP001", row.ParentProject);
            Assert.Equal("LabTGen", row.Contract);
            Assert.Equal("comment-1", row.MonitoringComment);
            Assert.Equal(100m, row.TotalPlanCosts);
            Assert.Equal(10m, row.TotalYtdCosts);
        }

        [Fact]
        public async Task GetAllContractsMonitoringReportDataAsync_AppliesSurveillanceProgramFilter()
        {
            // Arrange
            var myProjects = new List<Projects>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "END_SURV", Manager = "M1", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Program = "ZZ", Manager = "M2", Projectstatus = "Live" }
            };

            var baseProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "Project 1", Contract = "LabTGen" },
                new() { Parentproject = "PP002", Projecttitle = "Project 2", Contract = "LabTGen" }
            };

            var yearTotals = new List<FpsYearTotal>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "TB", Totalcosts = 100.0, Customer = "C1", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Program = "ZZ", Totalcosts = 200.0, Customer = "C2", Projectstatus = "Live" }
            };

            var monthFinals = new List<ProjectMonthFinal>
            {
                new() { Year = 2025, Project = "PP001", Monthno = 6, Cumcost = 10m },
                new() { Year = 2025, Project = "PP002", Monthno = 6, Cumcost = 20m }
            };

            var repo = CreateRepository(
                myTlkpProjects: myProjects,
                projects: baseProjects,
                radTrackContracts: [new RadTrackContract { Contract = "LabTGen" }],
                fpsYearTotals: yearTotals,
                projectMonthFinals: monthFinals,
                comments: new List<Comment>(),
                radtrackProgs:
                [
                    new RadtrackProg { Program = "TB", Radtrackprog = true },
                    new RadtrackProg { Program = "ZZ", Radtrackprog = false }
                ]);

            // Act
            var result = await repo.GetAllContractsMonitoringReportDataAsync(
                Params(),
                reportYear: 2025,
                fiscalMonth: 6,
                contractFilter: "*",
                programFilter: null);

            // Assert
            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("END_SURV", Assert.Single(result.Data).Program);
        }

        [Fact]
        public async Task GetContractsMonitoringReportDataAsync_DoesNotApplySurveillanceProgramFilter()
        {
            // Arrange
            var myProjects = new List<Projects>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "TB", Manager = "M1", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Program = "ZZ", Manager = "M2", Projectstatus = "Live" }
            };

            var baseProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "Project 1", Contract = "LabTGen" },
                new() { Parentproject = "PP002", Projecttitle = "Project 2", Contract = "LabTGen" }
            };

            var yearTotals = new List<FpsYearTotal>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "TB", Totalcosts = 100.0, Customer = "C1", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Program = "ZZ", Totalcosts = 200.0, Customer = "C2", Projectstatus = "Live" }
            };

            var monthFinals = new List<ProjectMonthFinal>
            {
                new() { Year = 2025, Project = "PP001", Monthno = 6, Cumcost = 10m },
                new() { Year = 2025, Project = "PP002", Monthno = 6, Cumcost = 20m }
            };

            var repo = CreateRepository(
                myTlkpProjects: myProjects,
                projects: baseProjects,
                radTrackContracts: [new RadTrackContract { Contract = "LabTGen" }],
                fpsYearTotals: yearTotals,
                projectMonthFinals: monthFinals,
                comments: new List<Comment>(),
                radtrackProgs:
                [
                    new RadtrackProg { Program = "TB", Radtrackprog = true },
                    new RadtrackProg { Program = "ZZ", Radtrackprog = false }
                ]);

            // Act
            var result = await repo.GetContractsMonitoringReportDataAsync(
                Params(),
                reportYear: 2025,
                fiscalMonth: 6,
                contractFilter: "*",
                programFilter: null);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_ReturnsProjectedFields_AndAppliesProgramFilter()
        {
            // Arrange
            var myProjects = new List<Projects>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "TB", Customer = "FromProject", Manager = "M1", Projectstatus = "Live", BudgetCvl = 300m },
                new() { Year = 2025, Parentproject = "PP002", Program = "AMR", Customer = "FromProject2", Manager = "M2", Projectstatus = "Live" }
            };

            var baseProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "Project 1", Contract = "LabTGen" },
                new() { Parentproject = "PP002", Projecttitle = "Project 2", Contract = "Other" }
            };

            var yearTotals = new List<FpsYearTotal>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "TB", Totalcosts = 150.0, BudgetCvl = 300m, Custincome = 55m, Customer = "CustomerA", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Program = "AMR", Totalcosts = 250.0, BudgetCvl = 400m, Custincome = 65m, Customer = "CustomerB", Projectstatus = "Live" }
            };

            var monthFinals = new List<ProjectMonthFinal>
            {
                new() { Year = 2025, Project = "PP001", Monthno = 6, Cumcost = 60m, Sumofcostprofile = 70m, Cumprofile = 50m, Cuminvoices = 45m },
                new() { Year = 2025, Project = "PP002", Monthno = 6, Cumcost = 80m, Sumofcostprofile = 90m, Cumprofile = 75m, Cuminvoices = 40m }
            };

            var yearlyFinancial = new List<YearlyFinancialData>
            {
                new() { Year = 2025, Project = "PP001", BfBudget = 33m },
                new() { Year = 2025, Project = "PP002", BfBudget = 66m }
            };

            var radTrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001", Pcforecastspend = 12.5, Startdate = new DateTime(2025, 4, 1), Enddate = new DateTime(2026, 3, 31) },
                new() { Parentproject = "PP002", Pcforecastspend = 15.5, Startdate = new DateTime(2025, 5, 1), Enddate = new DateTime(2026, 4, 30) }
            };

            var comments = new List<Comment>
            {
                new() { Project = "PP001", Year = 2025, Topic = "P&C Monitoring Report", CommentText = "pc-comment" }
            };

            var repo = CreateRepository(
                myTlkpProjects: myProjects,
                projects: baseProjects,
                fpsYearTotals: yearTotals,
                projectMonthFinals: monthFinals,
                yearlyFinancialData: yearlyFinancial,
                projectRadTrackData: radTrackData,
                comments: comments);

            // Act
            var result = await repo.GetProgramCustomerMonitoringReportDataAsync(
                Params(sortBy: "Program"),
                reportYear: 2025,
                fiscalMonth: 6,
                programFilter: ["TB"]);

            // Assert
            Assert.Equal(1, result.PaginationData.TotalRecords);
            var row = Assert.Single(result.Data);
            Assert.Equal("TB", row.Program);
            Assert.Equal("FromProject", row.Customer);
            Assert.Equal(150m, row.PlannedCosts);
            Assert.Equal(60m, row.ActualCostsYt);
            Assert.Equal(0.2m, row.PercentOfBudget);
            Assert.Equal(12.5, row.PcForecastSpend);
            Assert.Equal(33m, row.BfBudget);
            Assert.Equal("pc-comment", row.MonitoringComment);
        }

        [Fact]
        public async Task GetAllContractsMonitoringReportDataAsync_WithContractFilter_FiltersToMatchingContract()
        {
            // Arrange
            var myProjects = new List<Projects>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "END_SURV", Manager = "M1", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Program = "TB_SURV", Manager = "M2", Projectstatus = "Live" }
            };

            var baseProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "Project 1", Contract = "SurvA" },
                new() { Parentproject = "PP002", Projecttitle = "Project 2", Contract = "SurvG" }
            };

            var radContracts = new List<RadTrackContract>
            {
                new() { Contract = "SurvA" },
                new() { Contract = "SurvG" }
            };

            var yearTotals = new List<FpsYearTotal>
            {
                new() { Year = 2025, Parentproject = "PP001", Totalcosts = 100.0, Customer = "C1", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Totalcosts = 200.0, Customer = "C2", Projectstatus = "Live" }
            };

            var monthFinals = new List<ProjectMonthFinal>
            {
                new() { Year = 2025, Project = "PP001", Monthno = 5, Cumcost = 10m },
                new() { Year = 2025, Project = "PP002", Monthno = 5, Cumcost = 20m }
            };

            var repo = CreateRepository(
                myTlkpProjects: myProjects,
                projects: baseProjects,
                radTrackContracts: radContracts,
                fpsYearTotals: yearTotals,
                projectMonthFinals: monthFinals,
                comments: new List<Comment>());

            // Act — filter by SurvA, should return only PP001
            var result = await repo.GetAllContractsMonitoringReportDataAsync(
                Params(),
                reportYear: 2025,
                fiscalMonth: 5,
                contractFilter: "SurvA",
                programFilter: null);

            // Assert
            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("PP001", Assert.Single(result.Data).ParentProject);
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_DuplicateComments_ProducesDuplicateRowsLikeAccess()
        {
            // Arrange — PP001 has 2 comment rows for the same Year/Project/Topic
            var myProjects = new List<Projects>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "TB", Manager = "M1", Projectstatus = "Live" }
            };

            var baseProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "Project 1", Contract = "LabTGen" }
            };

            var yearTotals = new List<FpsYearTotal>
            {
                new() { Year = 2025, Parentproject = "PP001", Totalcosts = 100.0, BudgetCvl = 200m, Customer = "C1", Projectstatus = "Live" }
            };

            var monthFinals = new List<ProjectMonthFinal>
            {
                new() { Year = 2025, Project = "PP001", Monthno = 5, Cumcost = 50m }
            };

            var comments = new List<Comment>
            {
                new() { CommentNo = 1, Project = "PP001", Year = 2025, Topic = "P&C Monitoring Report", CommentText = "first comment" },
                new() { CommentNo = 2, Project = "PP001", Year = 2025, Topic = "P&C Monitoring Report", CommentText = "second comment" }
            };

            var repo = CreateRepository(
                myTlkpProjects: myProjects,
                projects: baseProjects,
                fpsYearTotals: yearTotals,
                projectMonthFinals: monthFinals,
                comments: comments);

            // Act
            var result = await repo.GetProgramCustomerMonitoringReportDataAsync(
                Params(),
                reportYear: 2025,
                fiscalMonth: 5,
                programFilter: null);

            // Assert — 2 comment rows produce 2 result rows, mirroring Access LEFT JOIN behaviour
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_Paging_WorksAsExpected()
        {
            // Arrange
            var myProjects = new List<Projects>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "TB", Manager = "M1", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Program = "AMR", Manager = "M2", Projectstatus = "Live" }
            };

            var baseProjects = new List<Project>
            {
                new() { Parentproject = "PP001", Projecttitle = "Project 1", Contract = "LabTGen" },
                new() { Parentproject = "PP002", Projecttitle = "Project 2", Contract = "Other" }
            };

            var yearTotals = new List<FpsYearTotal>
            {
                new() { Year = 2025, Parentproject = "PP001", Program = "TB", Totalcosts = 150.0, Customer = "CustomerA", Projectstatus = "Live" },
                new() { Year = 2025, Parentproject = "PP002", Program = "AMR", Totalcosts = 250.0, Customer = "CustomerB", Projectstatus = "Live" }
            };

            var monthFinals = new List<ProjectMonthFinal>
            {
                new() { Year = 2025, Project = "PP001", Monthno = 6 },
                new() { Year = 2025, Project = "PP002", Monthno = 6 }
            };

            var repo = CreateRepository(
                myTlkpProjects: myProjects,
                projects: baseProjects,
                fpsYearTotals: yearTotals,
                projectMonthFinals: monthFinals);

            // Act
            var result = await repo.GetProgramCustomerMonitoringReportDataAsync(
                Params(page: 2, pageSize: 1, sortBy: "project", descending: false),
                reportYear: 2025,
                fiscalMonth: 6,
                programFilter: null);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
            Assert.Equal(2, result.PaginationData.PageNumber);
            var row = Assert.Single(result.Data);
            Assert.Equal("PP002", row.ParentProject);
        }

        // -----------------------------------------------------------------------
        // Sorting tests for All Contracts / Contracts Monitoring (MonitoringReportData)
        // -----------------------------------------------------------------------

        private static (QueriesRepository repo, List<Projects> myProjects) CreateMonitoringRepo()
        {
            var myProjects = new List<Projects>
            {
                new() { Year = 2025, Parentproject = "AA001", Program = "END_SURV", Manager = "Alpha", Projectstatus = "Approved" },
                new() { Year = 2025, Parentproject = "BB002", Program = "TB_SURV",  Manager = "Zeta",  Projectstatus = "Not Approved" }
            };
            var baseProjects = new List<Project>
            {
                new() { Parentproject = "AA001", Projecttitle = "Zebra Project",   Contract = "SurvA" },
                new() { Parentproject = "BB002", Projecttitle = "Alpha Project",   Contract = "SurvG" }
            };
            var radContracts = new List<RadTrackContract>
            {
                new() { Contract = "SurvA" },
                new() { Contract = "SurvG" }
            };
            var yearTotals = new List<FpsYearTotal>
            {
                new() { Year = 2025, Parentproject = "AA001", Totalcosts = 500.0, Projectstatus = "Approved" },
                new() { Year = 2025, Parentproject = "BB002", Totalcosts = 100.0, Projectstatus = "Not Approved" }
            };
            var monthFinals = new List<ProjectMonthFinal>
            {
                new() { Year = 2025, Project = "AA001", Monthno = 6, Cumcost = 200m },
                new() { Year = 2025, Project = "BB002", Monthno = 6, Cumcost = 800m }
            };
            var repo = CreateRepository(
                myTlkpProjects: myProjects,
                projects: baseProjects,
                radTrackContracts: radContracts,
                fpsYearTotals: yearTotals,
                projectMonthFinals: monthFinals,
                comments: new List<Comment>());
            return (repo, myProjects);
        }

        [Theory]
        [InlineData("program",        false, "AA001",    "BB002")]   // END_SURV < TB_SURV asc → AA001 first
        [InlineData("program",        true,  "BB002",    "AA001")]   // desc → BB002 first
        [InlineData("project",        false, "AA001",    "BB002")]
        [InlineData("project",        true,  "BB002",    "AA001")]
        [InlineData("parentproject",  false, "AA001",    "BB002")]
        [InlineData("parentproject",  true,  "BB002",    "AA001")]
        [InlineData("projecttitle",   false, "BB002",    "AA001")]   // Alpha Project < Zebra Project
        [InlineData("projecttitle",   true,  "AA001",    "BB002")]
        [InlineData("manager",        false, "AA001",    "BB002")]   // Alpha < Zeta
        [InlineData("manager",        true,  "BB002",    "AA001")]
        [InlineData("status",         false, "AA001",    "BB002")]   // "Approved" < "Not Approved" asc → AA001 first
        [InlineData("projectstatus",  false, "AA001",    "BB002")]
        [InlineData("contract",       false, "AA001",    "BB002")]   // SurvA < SurvG
        [InlineData("contract",       true,  "BB002",    "AA001")]
        [InlineData("totalplancosts", false, "BB002",    "AA001")]   // 100 < 500
        [InlineData("totalplancosts", true,  "AA001",    "BB002")]
        [InlineData("totalytdcosts",  false, "AA001",    "BB002")]   // 200 < 800
        [InlineData("totalytdcosts",  true,  "BB002",    "AA001")]
        public async Task GetContractsMonitoringReportDataAsync_Sorting_OrdersCorrectly(
            string sortBy, bool descending, string expectedFirst, string expectedSecond)
        {
            var (repo, _) = CreateMonitoringRepo();

            var result = await repo.GetContractsMonitoringReportDataAsync(
                Params(sortBy: sortBy, descending: descending),
                reportYear: 2025,
                fiscalMonth: 6,
                contractFilter: "*",
                programFilter: null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.Equal(expectedFirst,  result.Data.ElementAt(0).ParentProject);
            Assert.Equal(expectedSecond, result.Data.ElementAt(1).ParentProject);
        }

        [Theory]
        [InlineData("program",        false, "AA001",    "BB002")]   // END_SURV < TB_SURV
        [InlineData("program",        true,  "BB002",    "AA001")]
        [InlineData("totalplancosts", false, "BB002",    "AA001")]
        [InlineData("totalplancosts", true,  "AA001",    "BB002")]
        [InlineData("totalytdcosts",  false, "AA001",    "BB002")]
        [InlineData("totalytdcosts",  true,  "BB002",    "AA001")]
        public async Task GetAllContractsMonitoringReportDataAsync_Sorting_OrdersCorrectly(
            string sortBy, bool descending, string expectedFirst, string expectedSecond)
        {
            // All Contracts adds a surveillance-program filter — supply SURV programs only
            var myProjects = new List<Projects>
            {
                new() { Year = 2025, Parentproject = "AA001", Program = "END_SURV", Manager = "Alpha", Projectstatus = "Approved" },
                new() { Year = 2025, Parentproject = "BB002", Program = "TB_SURV",  Manager = "Zeta",  Projectstatus = "Not Approved" }
            };
            var baseProjects = new List<Project>
            {
                new() { Parentproject = "AA001", Projecttitle = "Zebra Project", Contract = "SurvA" },
                new() { Parentproject = "BB002", Projecttitle = "Alpha Project", Contract = "SurvG" }
            };
            var radContracts = new List<RadTrackContract> { new() { Contract = "SurvA" }, new() { Contract = "SurvG" } };
            var yearTotals = new List<FpsYearTotal>
            {
                new() { Year = 2025, Parentproject = "AA001", Totalcosts = 500.0, Projectstatus = "Approved" },
                new() { Year = 2025, Parentproject = "BB002", Totalcosts = 100.0, Projectstatus = "Not Approved" }
            };
            var monthFinals = new List<ProjectMonthFinal>
            {
                new() { Year = 2025, Project = "AA001", Monthno = 6, Cumcost = 200m },
                new() { Year = 2025, Project = "BB002", Monthno = 6, Cumcost = 800m }
            };
            var repo = CreateRepository(
                myTlkpProjects: myProjects,
                projects: baseProjects,
                radTrackContracts: radContracts,
                fpsYearTotals: yearTotals,
                projectMonthFinals: monthFinals,
                comments: new List<Comment>());

            var result = await repo.GetAllContractsMonitoringReportDataAsync(
                Params(sortBy: sortBy, descending: descending),
                reportYear: 2025,
                fiscalMonth: 6,
                contractFilter: "*",
                programFilter: null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.Equal(expectedFirst,  result.Data.ElementAt(0).ParentProject);
            Assert.Equal(expectedSecond, result.Data.ElementAt(1).ParentProject);
        }

        [Fact]
        public async Task GetContractsMonitoringReportDataAsync_UnknownSortBy_DefaultsToParentProjectAsc()
        {
            var (repo, _) = CreateMonitoringRepo();

            var result = await repo.GetContractsMonitoringReportDataAsync(
                Params(sortBy: "nonexistentcolumn", descending: false),
                reportYear: 2025,
                fiscalMonth: 6,
                contractFilter: "*",
                programFilter: null);

            Assert.Equal("AA001", result.Data.ElementAt(0).ParentProject);
            Assert.Equal("BB002", result.Data.ElementAt(1).ParentProject);
        }

        [Fact]
        public async Task GetContractsMonitoringReportDataAsync_NullSortBy_DefaultsToParentProjectAsc()
        {
            var (repo, _) = CreateMonitoringRepo();

            var result = await repo.GetContractsMonitoringReportDataAsync(
                Params(sortBy: null),
                reportYear: 2025,
                fiscalMonth: 6,
                contractFilter: "*",
                programFilter: null);

            Assert.Equal("AA001", result.Data.ElementAt(0).ParentProject);
            Assert.Equal("BB002", result.Data.ElementAt(1).ParentProject);
        }
    }
}

using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProfitCentreGradeRepositoryTest
{
    public class ProfitCentreGradeRepositoryTests
    {
        private const string DefaultProfitCentre = "PC01";
        private const string DefaultUserEmail     = "test@example.com";

        private static ProfitCentreGradeRepository CreateViewRepository(
            IEnumerable<ProfitCentreGradeView> grades)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns(DefaultUserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var gradesMockSet = RepositoryTestHelper.CreateMockDbSet(grades);
            mockContext.Setup(x => x.ProfitCentreGradeViews).Returns(gradesMockSet.Object);

            return new ProfitCentreGradeRepository(mockContext.Object, requestContext);
        }

        private static ProfitCentreGradeRepository CreateRepository(
            IEnumerable<ProfitCentreGrade>? grades = null,
            IEnumerable<ProfitCentre>? profitCentres = null,
            int fpsYear = 2024)
        {
            var requestContext = new Mock<IFpsRequestContext>();
            requestContext.Setup(x => x.FpsYear).Returns(fpsYear);
            requestContext.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext.Object);

            if (grades != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(grades);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.ProfitCentreGrades).Returns(mockSet.Object);
            }

            if (profitCentres != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres);
                mockContext.Setup(x => x.ProfitCentres).Returns(mockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProfitCentreGradeRepository(mockContext.Object, requestContext.Object);
        }

        private static (
            ProfitCentreGradeRepository Repo,
            Mock<DbSet<ProfitCentreGrade>> DbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<ProfitCentreGrade>? grades = null)
        {
            var requestContext = new Mock<IFpsRequestContext>();
            requestContext.Setup(x => x.FpsYear).Returns(2024);
            requestContext.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext.Object);

            var dbSet = RepositoryTestHelper.CreateMockDbSet(grades ?? []);
            RepositoryTestHelper.SetupDbSetOperations(dbSet);
            mockContext.Setup(x => x.ProfitCentreGrades).Returns(dbSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new ProfitCentreGradeRepository(mockContext.Object, requestContext.Object);
            return (repo, dbSet, mockContext);
        }

        private static ProfitCentreGrade BuildGrade(
            string pcGrade = "G001",
            string profitCentre = DefaultProfitCentre,
            string gradeCode = "GC1",
            string divisionGrade = "DG1",
            decimal? chargeRate = 100m,
            decimal? directRate = 80m,
            decimal? payRate = 70m,
            int fpsYear = 2024) =>
            new()
            {
                PcGrade       = pcGrade,
                ProfitCentre  = profitCentre,
                GradeCode     = gradeCode,
                DivisionGrade = divisionGrade,
                ChargeRate    = chargeRate,
                DirectRate    = directRate,
                PayRate       = payRate,
                FpsYear       = fpsYear
            };

        #region GetProfitCentreGradesAsync Tests

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithMatchingProfitCentre_ReturnsPagedData()
        {
            // Arrange
            var grades = new List<ProfitCentreGradeView>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m, UserEmail = DefaultUserEmail },
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m, UserEmail = DefaultUserEmail },
                new() { PcGrade = "G003", ProfitCentre = "OTHER",            ChargeRate = 300m, UserEmail = DefaultUserEmail }
            };
            var repo = CreateViewRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, g => Assert.Equal(DefaultProfitCentre, g.ProfitCentre));
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithNoMatchingProfitCentre_ReturnsEmptyData()
        {
            // Arrange
            var grades = new List<ProfitCentreGradeView>
            {
                new() { PcGrade = "G001", ProfitCentre = "OTHER", ChargeRate = 100m, UserEmail = DefaultUserEmail }
            };
            var repo = CreateViewRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_ReturnsOrderedByChargeRateDescending()
        {
            // Arrange
            var grades = new List<ProfitCentreGradeView>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m, UserEmail = DefaultUserEmail },
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 300m, UserEmail = DefaultUserEmail },
                new() { PcGrade = "G003", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m, UserEmail = DefaultUserEmail }
            };
            var repo = CreateViewRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            var resultList = result.Data.ToList();
            Assert.Equal(300m, resultList[0].ChargeRate);
            Assert.Equal(200m, resultList[1].ChargeRate);
            Assert.Equal(100m, resultList[2].ChargeRate);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var grades = Enumerable.Range(1, 5).Select(i => new ProfitCentreGradeView
            {
                PcGrade      = $"G00{i}",
                ProfitCentre = DefaultProfitCentre,
                ChargeRate   = i * 100m,
                UserEmail    = DefaultUserEmail
            }).ToList();
            var repo = CreateViewRepository(grades);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await repo.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithEmptyRepository_ReturnsEmptyData()
        {
            // Arrange
            var repo = CreateViewRepository([]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_FiltersOutNullUserEmail()
        {
            // Arrange
            var grades = new List<ProfitCentreGradeView>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m, UserEmail = DefaultUserEmail },
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m, UserEmail = null }
            };
            var repo = CreateViewRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("G001", result.Data.First().PcGrade);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_FiltersOutDifferentUserEmail()
        {
            // Arrange
            var grades = new List<ProfitCentreGradeView>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m, UserEmail = DefaultUserEmail },
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m, UserEmail = "other@example.com" }
            };
            var repo = CreateViewRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_MapsAllFields()
        {
            // Arrange
            var grades = new List<ProfitCentreGradeView>
            {
                new()
                {
                    PcGrade         = "G001",
                    ProfitCentre    = DefaultProfitCentre,
                    DivisionGrade   = "DG1",
                    GradeCode       = "GC1",
                    ChargeRate      = 100m,
                    DirectRate      = 80m,
                    PayRate         = 70m,
                    Npr             = 5m,
                    Ohr             = 3m,
                    HrsAvailable    = 40.0,
                    OldChargeRate   = 90m,
                    DefraChargeRate = 95m,
                    FpsYear         = 2024,
                    UserEmail       = DefaultUserEmail
                }
            };
            var repo = CreateViewRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            var item = result.Data.First();
            Assert.Equal("G001",            item.PcGrade);
            Assert.Equal("DG1",             item.DivisionGrade);
            Assert.Equal("GC1",             item.GradeCode);
            Assert.Equal(DefaultProfitCentre, item.ProfitCentre);
            Assert.Equal(100m,              item.ChargeRate);
            Assert.Equal(80m,               item.DirectRate);
            Assert.Equal(70m,               item.PayRate);
            Assert.Equal(5m,                item.NPR);
            Assert.Equal(3m,                item.OHR);
            Assert.Equal(40.0,              item.HrsAvailable);
            Assert.Equal(90m,               item.OldChargeRate);
            Assert.Equal(95m,               item.DefraChargeRate);
            Assert.Equal(2024,              item.FpsYear);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_MapsNullFpsYearToZero()
        {
            // Arrange
            var grades = new List<ProfitCentreGradeView>
            {
                new()
                {
                    PcGrade      = "G001",
                    ProfitCentre = DefaultProfitCentre,
                    ChargeRate   = 100m,
                    UserEmail    = DefaultUserEmail,
                    FpsYear      = null
                }
            };
            var repo = CreateViewRepository(grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.Equal(0, result.Data.First().FpsYear);
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            var repo = CreateRepository(grades: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAllPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsEmptyPagedData_WhenNoRecords()
        {
            var repo = CreateRepository(grades: []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllPagedAsync(query);
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsAllRecords()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("G001"),
                BuildGrade("G002"),
                BuildGrade("G003")
            };
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsCorrectPage()
        {
            var grades = Enumerable.Range(1, 5).Select(i => BuildGrade($"G00{i}")).ToList();
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByPcGrade()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("ALPHA1"),
                BuildGrade("BETA1"),
                BuildGrade("ALPHA2")
            };
            var repo = CreateRepository(grades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "PcGrade", "ALPHA" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByDivisionGrade()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("G001", divisionGrade: "DGA"),
                BuildGrade("G002", divisionGrade: "DGB"),
                BuildGrade("G003", divisionGrade: "DGA")
            };
            var repo = CreateRepository(grades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "DivisionGrade", "DGA" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByGradeCode()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("G001", gradeCode: "GCA"),
                BuildGrade("G002", gradeCode: "GCB"),
                BuildGrade("G003", gradeCode: "GCA")
            };
            var repo = CreateRepository(grades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "GradeCode", "GCA" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByProfitCentre()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("G001", profitCentre: "PC01"),
                BuildGrade("G002", profitCentre: "PC02"),
                BuildGrade("G003", profitCentre: "PC01")
            };
            var repo = CreateRepository(grades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "ProfitCentre", "PC01" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_OrdersByPcGradeAscByDefault()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("C001"),
                BuildGrade("A001"),
                BuildGrade("B001")
            };
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal("A001", list[0].PcGrade);
        }

        [Theory]
        [InlineData("pcgrade",       false, "A001", "B001", "C001")]
        [InlineData("pcgrade",       true,  "C001", "B001", "A001")]
        [InlineData("divisiongrade", false, "DGA",  "DGB",  "DGC")]
        [InlineData("divisiongrade", true,  "DGC",  "DGB",  "DGA")]
        public async Task GetAllPagedAsync_SortsByColumn(string sortBy, bool descending, string first, string second, string third)
        {
            var grades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "B001", ProfitCentre = DefaultProfitCentre, DivisionGrade = "DGB", GradeCode = "GC1" },
                new() { PcGrade = "A001", ProfitCentre = DefaultProfitCentre, DivisionGrade = "DGA", GradeCode = "GC1" },
                new() { PcGrade = "C001", ProfitCentre = DefaultProfitCentre, DivisionGrade = "DGC", GradeCode = "GC1" }
            };
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllPagedAsync(query);
            var list = result.Data.ToList();

            var actualFirst  = sortBy == "pcgrade" ? list[0].PcGrade       : list[0].DivisionGrade;
            var actualSecond = sortBy == "pcgrade" ? list[1].PcGrade       : list[1].DivisionGrade;
            var actualThird  = sortBy == "pcgrade" ? list[2].PcGrade       : list[2].DivisionGrade;

            Assert.Equal(first,  actualFirst);
            Assert.Equal(second, actualSecond);
            Assert.Equal(third,  actualThird);
        }

        [Fact]
        public async Task GetAllPagedAsync_SortsByGradeCodeAscending()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("G003", gradeCode: "GCC"),
                BuildGrade("G001", gradeCode: "GCA"),
                BuildGrade("G002", gradeCode: "GCB")
            };
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "gradecode", Descending = false };
            var result = await repo.GetAllPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal("GCA", list[0].GradeCode);
            Assert.Equal("GCB", list[1].GradeCode);
            Assert.Equal("GCC", list[2].GradeCode);
        }

        [Fact]
        public async Task GetAllPagedAsync_SortsByGradeCodeDescending()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("G001", gradeCode: "GCA"),
                BuildGrade("G002", gradeCode: "GCB"),
                BuildGrade("G003", gradeCode: "GCC")
            };
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "gradecode", Descending = true };
            var result = await repo.GetAllPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal("GCC", list[0].GradeCode);
        }

        [Fact]
        public async Task GetAllPagedAsync_SortsByProfitCentreAscending()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("G003", profitCentre: "PC03"),
                BuildGrade("G001", profitCentre: "PC01"),
                BuildGrade("G002", profitCentre: "PC02")
            };
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "profitcentre", Descending = false };
            var result = await repo.GetAllPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal("PC01", list[0].ProfitCentre);
        }

        [Fact]
        public async Task GetAllPagedAsync_SortsByProfitCentreDescending()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("G001", profitCentre: "PC01"),
                BuildGrade("G002", profitCentre: "PC02"),
                BuildGrade("G003", profitCentre: "PC03")
            };
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "profitcentre", Descending = true };
            var result = await repo.GetAllPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal("PC03", list[0].ProfitCentre);
        }

        [Theory]
        [InlineData("chargerate", false)]
        [InlineData("chargerate", true)]
        [InlineData("directrate", false)]
        [InlineData("directrate", true)]
        [InlineData("payrate",    false)]
        [InlineData("payrate",    true)]
        [InlineData("npr",        false)]
        [InlineData("npr",        true)]
        [InlineData("ohr",        false)]
        [InlineData("ohr",        true)]
        [InlineData("hrsavailable", false)]
        [InlineData("hrsavailable", true)]
        public async Task GetAllPagedAsync_SortsByNumericColumn(string sortBy, bool descending)
        {
            var grades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, DivisionGrade = "DG1", GradeCode = "GC1",
                        ChargeRate = 200m, DirectRate = 200m, PayRate = 200m, NPR = 20m, OHR = 20m, HrsAvailable = 20.0 },
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, DivisionGrade = "DG1", GradeCode = "GC1",
                        ChargeRate = 100m, DirectRate = 100m, PayRate = 100m, NPR = 10m, OHR = 10m, HrsAvailable = 10.0 },
                new() { PcGrade = "G003", ProfitCentre = DefaultProfitCentre, DivisionGrade = "DG1", GradeCode = "GC1",
                        ChargeRate = 300m, DirectRate = 300m, PayRate = 300m, NPR = 30m, OHR = 30m, HrsAvailable = 30.0 }
            };
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllPagedAsync(query);
            var list = result.Data.ToList();

            // Verify ordering direction is correct by checking first vs last
            var firstVal = sortBy switch
            {
                "chargerate"   => list[0].ChargeRate,
                "directrate"   => list[0].DirectRate,
                "payrate"      => list[0].PayRate,
                "npr"          => list[0].NPR,
                "ohr"          => list[0].OHR,
                "hrsavailable" => (decimal?)list[0].HrsAvailable,
                _              => null
            };
            var lastVal = sortBy switch
            {
                "chargerate"   => list[2].ChargeRate,
                "directrate"   => list[2].DirectRate,
                "payrate"      => list[2].PayRate,
                "npr"          => list[2].NPR,
                "ohr"          => list[2].OHR,
                "hrsavailable" => (decimal?)list[2].HrsAvailable,
                _              => null
            };

            if (descending)
                Assert.True(firstVal > lastVal);
            else
                Assert.True(firstVal < lastVal);
        }

        [Fact]
        public async Task GetAllPagedAsync_UnknownSortByDefaultsToPcGradeAscending()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("C001"),
                BuildGrade("A001"),
                BuildGrade("B001")
            };
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknown_column" };
            var result = await repo.GetAllPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal("A001", list[0].PcGrade);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepository(grades: []);
            var result = await repo.GetByIdAsync("NONEXISTENT");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsRecord_WhenFound()
        {
            var grades = new List<ProfitCentreGrade> { BuildGrade("G001") };
            var repo = CreateRepository(grades: grades);
            var result = await repo.GetByIdAsync("G001");
            Assert.NotNull(result);
            Assert.Equal("G001", result.PcGrade);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenDifferentPcGrade()
        {
            var grades = new List<ProfitCentreGrade> { BuildGrade("G001") };
            var repo = CreateRepository(grades: grades);
            var result = await repo.GetByIdAsync("G002");
            Assert.Null(result);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository(grades: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_AddsEntityAndSavesChanges()
        {
            var (repo, dbSet, context) = CreateRepositoryWithMocks([]);
            dbSet.Setup(x => x.Add(It.IsAny<ProfitCentreGrade>()));
            var entity = BuildGrade("NEW001");
            var result = await repo.CreateAsync(entity);
            Assert.NotNull(result);
            Assert.Equal("NEW001", result.PcGrade);
            dbSet.Verify(x => x.Add(It.Is<ProfitCentreGrade>(e => e.PcGrade == "NEW001")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        [Fact]
        public async Task CreateAsync_SetsFpsYearFromContext()
        {
            var (repo, dbSet, _) = CreateRepositoryWithMocks([]);
            dbSet.Setup(x => x.Add(It.IsAny<ProfitCentreGrade>()));
            var entity = BuildGrade("NEW001");
            entity.FpsYear = 0;
            var result = await repo.CreateAsync(entity);
            Assert.Equal(2024, result.FpsYear);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository(grades: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync("G001", null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateAsync_ThrowsArgumentException_WhenOriginalPcGradeIsNullOrWhitespace(string originalPcGrade)
        {
            var repo = CreateRepository(grades: []);
            var entity = BuildGrade("G001");
            await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateAsync(originalPcGrade, entity));
        }

        [Fact]
        public async Task UpdateAsync_UpdatesFieldsAndSavesChanges()
        {
            var existing = BuildGrade("G001", profitCentre: "PC01", gradeCode: "OLD", chargeRate: 50m);
            var (repo, _, context) = CreateRepositoryWithMocks([existing]);

            var updated = new ProfitCentreGrade
            {
                PcGrade       = "G001",
                ProfitCentre  = "PC02",
                DivisionGrade = "DG_NEW",
                GradeCode     = "GC_NEW",
                ChargeRate    = 200m,
                DirectRate    = 150m,
                PayRate       = 100m,
                NPR           = 10m,
                OHR           = 5m,
                HrsAvailable  = 37.5
            };

            var result = await repo.UpdateAsync("G001", updated);

            Assert.Equal("PC02",   result.ProfitCentre);
            Assert.Equal("DG_NEW", result.DivisionGrade);
            Assert.Equal("GC_NEW", result.GradeCode);
            Assert.Equal(200m,     result.ChargeRate);
            Assert.Equal(150m,     result.DirectRate);
            Assert.Equal(100m,     result.PayRate);
            Assert.Equal(10m,      result.NPR);
            Assert.Equal(5m,       result.OHR);
            Assert.Equal(37.5,     result.HrsAvailable);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            var repo = CreateRepository(grades: []);
            var result = await repo.DeleteAsync("NONEXISTENT");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_RemovesEntityAndReturnsTrue_WhenFound()
        {
            var existing = BuildGrade("G001");
            var (repo, dbSet, context) = CreateRepositoryWithMocks([existing]);

            var result = await repo.DeleteAsync("G001");

            Assert.True(result);
            dbSet.Verify(x => x.Remove(It.Is<ProfitCentreGrade>(e => e.PcGrade == "G001")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        #endregion

        #region ProfitCentreExistsAsync Tests

        [Fact]
        public async Task ProfitCentreExistsAsync_ReturnsTrue_WhenProfitCentreExists()
        {
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre 1", Division = "DIV1" }
            };
            var repo = CreateRepository(grades: [], profitCentres: profitCentres);

            var result = await repo.ProfitCentreExistsAsync("PC01");

            Assert.True(result);
        }

        [Fact]
        public async Task ProfitCentreExistsAsync_ReturnsFalse_WhenProfitCentreDoesNotExist()
        {
            var repo = CreateRepository(grades: [], profitCentres: []);

            var result = await repo.ProfitCentreExistsAsync("INVALID");

            Assert.False(result);
        }

        #endregion

        #region GetAllProfitCentreCodesAsync Tests

        [Fact]
        public async Task GetAllProfitCentreCodesAsync_ReturnsOrderedCodes()
        {
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC03", ProfitCentreName = "Centre 3", Division = "D" },
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre 1", Division = "D" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre 2", Division = "D" }
            };
            var repo = CreateRepository(grades: [], profitCentres: profitCentres);

            var result = await repo.GetAllProfitCentreCodesAsync();

            Assert.Equal(["PC01", "PC02", "PC03"], result);
        }

        [Fact]
        public async Task GetAllProfitCentreCodesAsync_ReturnsEmpty_WhenNoProfitCentres()
        {
            var repo = CreateRepository(grades: [], profitCentres: []);

            var result = await repo.GetAllProfitCentreCodesAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProfitCentreCodesAsync_ReturnsOnlyIds()
        {
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "D" }
            };
            var repo = CreateRepository(grades: [], profitCentres: profitCentres);

            var result = await repo.GetAllProfitCentreCodesAsync();

            Assert.Single(result);
            Assert.Equal("PC01", result[0]);
        }

        #endregion

        #region GetAllPcGradesAsync Tests

        [Fact]
        public async Task GetAllPcGradesAsync_ReturnsDistinctOrderedPcGrades()
        {
            var grades = new List<ProfitCentreGrade>
            {
                BuildGrade("GCC"),
                BuildGrade("GCA"),
                BuildGrade("GCB"),
                BuildGrade("GCA") // duplicate
            };
            var repo = CreateRepository(grades: grades);

            var result = await repo.GetAllPcGradesAsync();

            Assert.Equal(["GCA", "GCB", "GCC"], result);
        }

        [Fact]
        public async Task GetAllPcGradesAsync_ReturnsEmpty_WhenNoGrades()
        {
            var repo = CreateRepository(grades: []);

            var result = await repo.GetAllPcGradesAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPcGradesAsync_ReturnsSingleItem_WhenOneGrade()
        {
            var grades = new List<ProfitCentreGrade> { BuildGrade("G001") };
            var repo = CreateRepository(grades: grades);

            var result = await repo.GetAllPcGradesAsync();

            Assert.Single(result);
            Assert.Equal("G001", result[0]);
        }

        #endregion

        #region ExistsForGradeCodeAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExistsForGradeCodeAsync_ReturnsFalse_WhenGradeCodeIsEmptyOrWhiteSpace(string gradeCode)
        {
            var repo = CreateRepository(grades: []);

            var result = await repo.ExistsForGradeCodeAsync(gradeCode);

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsForGradeCodeAsync_ReturnsTrue_WhenGradeCodeExists()
        {
            var grades = new List<ProfitCentreGrade> { BuildGrade("G001", gradeCode: "GCA") };
            var repo = CreateRepository(grades: grades);

            var result = await repo.ExistsForGradeCodeAsync("GCA");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsForGradeCodeAsync_ReturnsFalse_WhenGradeCodeDoesNotExist()
        {
            var grades = new List<ProfitCentreGrade> { BuildGrade("G001", gradeCode: "GCA") };
            var repo = CreateRepository(grades: grades);

            var result = await repo.ExistsForGradeCodeAsync("NONEXISTENT");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsForGradeCodeAsync_ReturnsFalse_WhenNoGrades()
        {
            var repo = CreateRepository(grades: []);

            var result = await repo.ExistsForGradeCodeAsync("GCA");

            Assert.False(result);
        }

        #endregion
    }
}

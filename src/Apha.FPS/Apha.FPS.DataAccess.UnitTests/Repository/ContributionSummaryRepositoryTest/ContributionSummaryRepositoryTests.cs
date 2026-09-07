using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ContributionSummaryRepositoryTest
{
    public class ContributionSummaryRepositoryTests
    {
        private const string DefaultUserEmail = "test@example.com";
        private const string DefaultSellingPc = "ENV";

        private static ContributionSummaryRepository CreateRepository(
            IEnumerable<ContributionSummaryView>? views = null,
            string userEmail = DefaultUserEmail)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            if (views != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(views);
                mockContext.Setup(x => x.VQryFrmTimeSellerPcViews).Returns(mockSet.Object);
            }

            return new ContributionSummaryRepository(mockContext.Object, mockRequestContext.Object);
        }

        private static ContributionSummaryView MakeView(
            string  sellingPc = DefaultSellingPc,
            string  workGroup = "WG1",
            string  wgGrade   = "G1",
            string? userEmail = DefaultUserEmail)
            => new()
            {
                SellingPc = sellingPc,
                WorkGroup = workGroup,
                WgGrade   = wgGrade,
                UserEmail = userEmail
            };

        #region GetBySellingPcAsync — Happy path

        [Fact]
        public async Task GetBySellingPcAsync_ReturnsRowsForMatchingSellingPcAndUser()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeView("ENV", "WG1", "G1"),
                MakeView("ENV", "WG2", "G2"),
                MakeView("ASU", "WG3", "G3")  // different PC — must be excluded
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(DefaultSellingPc, r.SellingPc));
        }

        [Fact]
        public async Task GetBySellingPcAsync_ReturnsEmpty_WhenNoMatchingSellingPc()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeView("ASU", "WG1", "G1"),
                MakeView("DTE", "WG2", "G2")
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBySellingPcAsync_ReturnsEmpty_WhenDataSetIsEmpty()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetBySellingPcAsync — User email filter

        [Fact]
        public async Task GetBySellingPcAsync_ExcludesRowsWithDifferentUserEmail()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeView(userEmail: DefaultUserEmail),
                MakeView(workGroup: "WG2", wgGrade: "G2", userEmail: "other@example.com")
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Single(result);
            Assert.Equal("WG1", result[0].WorkGroup);
        }

        [Fact]
        public async Task GetBySellingPcAsync_ExcludesRowsWithNullUserEmail()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeView(userEmail: DefaultUserEmail),
                MakeView(workGroup: "WG2", wgGrade: "G2", userEmail: null)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Single(result);
            Assert.Equal("WG1", result[0].WorkGroup);
        }

        #endregion

        #region GetBySellingPcAsync — Ordering

        [Fact]
        public async Task GetBySellingPcAsync_IsOrderedByWorkGroupThenWgGrade()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeView(workGroup: "WG2", wgGrade: "G1"),
                MakeView(workGroup: "WG1", wgGrade: "G2"),
                MakeView(workGroup: "WG1", wgGrade: "G1")
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Equal("WG1", result[0].WorkGroup);
            Assert.Equal("G1",  result[0].WgGrade);
            Assert.Equal("WG1", result[1].WorkGroup);
            Assert.Equal("G2",  result[1].WgGrade);
            Assert.Equal("WG2", result[2].WorkGroup);
        }

        #endregion

        #region GetBySellingPcAsync — Sorting

        private static ContributionSummaryView MakeSortableView(
            string   workGroup         = "WG1",
            string   wgGrade           = "G1",
            string?  profitCentreGrade = "PCG1",
            double?  avHrs             = 100d,
            decimal? chargeRate        = 10m,
            double?  hrs               = 50d,
            decimal? fec               = 500m,
            double?  appHours          = 25d,
            decimal? appFec            = 250m,
            decimal? ohr               = 5m,
            decimal? contribution      = 1000m)
            => new()
            {
                SellingPc         = DefaultSellingPc,
                UserEmail         = DefaultUserEmail,
                WorkGroup         = workGroup,
                WgGrade           = wgGrade,
                ProfitCentreGrade = profitCentreGrade,
                AvHrs             = avHrs,
                ChargeRate        = chargeRate,
                Hrs               = hrs,
                Fec               = fec,
                AppHours          = appHours,
                AppFec            = appFec,
                Ohr               = ohr,
                Contribution      = contribution
            };

        [Fact]
        public async Task GetBySellingPcAsync_WhenSortByIsNull_KeepsDefaultOrdering()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "WG2"),
                MakeSortableView(workGroup: "WG1")
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, null);

            // Assert — falls back to WorkGroup then WgGrade
            Assert.Equal("WG1", result[0].WorkGroup);
            Assert.Equal("WG2", result[1].WorkGroup);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetBySellingPcAsync_WhenSortByIsBlank_KeepsDefaultOrdering(string sortBy)
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "WG2"),
                MakeSortableView(workGroup: "WG1")
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, sortBy);

            // Assert
            Assert.Equal("WG1", result[0].WorkGroup);
            Assert.Equal("WG2", result[1].WorkGroup);
        }

        [Fact]
        public async Task GetBySellingPcAsync_WhenSortByIsUnknownColumn_KeepsDefaultOrdering()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "WG2"),
                MakeSortableView(workGroup: "WG1")
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "NotAColumn", descending: true);

            // Assert
            Assert.Equal("WG1", result[0].WorkGroup);
            Assert.Equal("WG2", result[1].WorkGroup);
        }

        [Theory]
        [InlineData("WorkGroup")]
        [InlineData("WgGrade")]
        [InlineData("ProfitCentreGrade")]
        public async Task GetBySellingPcAsync_SortsTextColumnsAscendingAndDescending(string sortBy)
        {
            // Arrange — the same three distinct values on every text column
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", wgGrade: "B", profitCentreGrade: "B"),
                MakeSortableView(workGroup: "C", wgGrade: "C", profitCentreGrade: "C"),
                MakeSortableView(workGroup: "A", wgGrade: "A", profitCentreGrade: "A")
            };
            var repo = CreateRepository(views);

            static string? Value(ContributionSummaryView v, string column) => column switch
            {
                "WorkGroup"         => v.WorkGroup,
                "WgGrade"           => v.WgGrade,
                _                   => v.ProfitCentreGrade
            };

            // Act
            var ascending  = await repo.GetBySellingPcAsync(DefaultSellingPc, sortBy);
            var descending = await repo.GetBySellingPcAsync(DefaultSellingPc, sortBy, descending: true);

            // Assert
            Assert.Equal(["A", "B", "C"], ascending.Select(v => Value(v, sortBy)));
            Assert.Equal(["C", "B", "A"], descending.Select(v => Value(v, sortBy)));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsByAvHrs_TreatingNullAsZero()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", avHrs: 50d),
                MakeSortableView(workGroup: "C", avHrs: null),
                MakeSortableView(workGroup: "A", avHrs: 200d)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "AvHrs");

            // Assert — null sorts as 0, so it comes first
            Assert.Equal(["C", "B", "A"], result.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsByChargeRate_TreatingNullAsZero()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", chargeRate: 20m),
                MakeSortableView(workGroup: "C", chargeRate: null),
                MakeSortableView(workGroup: "A", chargeRate: 90m)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "ChargeRate");

            // Assert
            Assert.Equal(["C", "B", "A"], result.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsByHrs_TreatingNullAsZero()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", hrs: 30d),
                MakeSortableView(workGroup: "C", hrs: null),
                MakeSortableView(workGroup: "A", hrs: 75d)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "Hrs");

            // Assert
            Assert.Equal(["C", "B", "A"], result.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsByFecDescending_TreatingNullAsZero()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", fec: 300m),
                MakeSortableView(workGroup: "C", fec: null),
                MakeSortableView(workGroup: "A", fec: 900m)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "Fec", descending: true);

            // Assert
            Assert.Equal(["A", "B", "C"], result.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsByAppHours_TreatingNullAsZero()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", appHours: 12d),
                MakeSortableView(workGroup: "C", appHours: null),
                MakeSortableView(workGroup: "A", appHours: 44d)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "AppHours");

            // Assert
            Assert.Equal(["C", "B", "A"], result.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsByAppFec_TreatingNullAsZero()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", appFec: 150m),
                MakeSortableView(workGroup: "C", appFec: null),
                MakeSortableView(workGroup: "A", appFec: 480m)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "AppFec");

            // Assert
            Assert.Equal(["C", "B", "A"], result.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsByOhr_TreatingNullAsZero()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", ohr: 3m),
                MakeSortableView(workGroup: "C", ohr: null),
                MakeSortableView(workGroup: "A", ohr: 8m)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "Ohr");

            // Assert
            Assert.Equal(["C", "B", "A"], result.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsByContribution_TreatingNullAsZero_IncludingNegatives()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", contribution: null),
                MakeSortableView(workGroup: "C", contribution: 400m),
                MakeSortableView(workGroup: "A", contribution: -250m)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "Contribution");

            // Assert — negative first, then the null (0), then the positive
            Assert.Equal(["A", "B", "C"], result.Select(r => r.WorkGroup));
        }

        #endregion

        #region GetBySellingPcAsync — Derived percentage sorting

        [Theory]
        [InlineData("PctPlanned")]
        [InlineData("PctPlannedDisplay")]
        public async Task GetBySellingPcAsync_SortsByPctPlanned_UsingHrsOverAvHrs(string sortBy)
        {
            // Arrange — ratios: A = 0.25, B = 0.75, C = 1.5
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", avHrs: 100d, hrs: 75d),
                MakeSortableView(workGroup: "C", avHrs: 100d, hrs: 150d),
                MakeSortableView(workGroup: "A", avHrs: 100d, hrs: 25d)
            };
            var repo = CreateRepository(views);

            // Act
            var ascending  = await repo.GetBySellingPcAsync(DefaultSellingPc, sortBy);
            var descending = await repo.GetBySellingPcAsync(DefaultSellingPc, sortBy, descending: true);

            // Assert
            Assert.Equal(["A", "B", "C"], ascending.Select(r => r.WorkGroup));
            Assert.Equal(["C", "B", "A"], descending.Select(r => r.WorkGroup));
        }

        [Theory]
        [InlineData("PctAssuredPlanned")]
        [InlineData("PctAssuredPlannedDisplay")]
        public async Task GetBySellingPcAsync_SortsByPctAssuredPlanned_UsingAppHoursOverAvHrs(string sortBy)
        {
            // Arrange — ratios: A = 0.1, B = 0.5, C = 2.0
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", avHrs: 100d, appHours: 50d),
                MakeSortableView(workGroup: "C", avHrs: 100d, appHours: 200d),
                MakeSortableView(workGroup: "A", avHrs: 100d, appHours: 10d)
            };
            var repo = CreateRepository(views);

            // Act
            var ascending  = await repo.GetBySellingPcAsync(DefaultSellingPc, sortBy);
            var descending = await repo.GetBySellingPcAsync(DefaultSellingPc, sortBy, descending: true);

            // Assert
            Assert.Equal(["A", "B", "C"], ascending.Select(r => r.WorkGroup));
            Assert.Equal(["C", "B", "A"], descending.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsPctPlanned_GroupsZeroAvHrsRowsFirstAscending()
        {
            // Arrange — rows displayed as "!" (AvHrs 0 or null) must sort as 0
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "D", avHrs: 100d, hrs: 90d),
                MakeSortableView(workGroup: "A", avHrs: 0d,   hrs: 90d),
                MakeSortableView(workGroup: "C", avHrs: 100d, hrs: 40d),
                MakeSortableView(workGroup: "B", avHrs: null, hrs: 90d)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "PctPlannedDisplay");

            // Assert — both "!" rows group together at the start, in stable order
            Assert.Equal(["A", "B", "C", "D"], result.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsPctPlanned_GroupsZeroAvHrsRowsLastDescending()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "C", avHrs: 100d, hrs: 40d),
                MakeSortableView(workGroup: "A", avHrs: 0d,   hrs: 90d),
                MakeSortableView(workGroup: "D", avHrs: 100d, hrs: 90d),
                MakeSortableView(workGroup: "B", avHrs: null, hrs: 90d)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "PctPlannedDisplay", descending: true);

            // Assert — "!" rows sort as 0, so they group together at the end
            Assert.Equal(["D", "C", "A", "B"], result.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsPctPlanned_TreatsNullHoursAsZero()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", avHrs: 100d, hrs: 60d),
                MakeSortableView(workGroup: "A", avHrs: 100d, hrs: null)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "PctPlanned");

            // Assert
            Assert.Equal(["A", "B"], result.Select(r => r.WorkGroup));
        }

        [Fact]
        public async Task GetBySellingPcAsync_SortsPctAssuredPlanned_TreatsZeroAvHrsAsZero()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeSortableView(workGroup: "B", avHrs: 100d, appHours: 60d),
                MakeSortableView(workGroup: "A", avHrs: 0d,   appHours: 999d)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc, "PctAssuredPlanned");

            // Assert
            Assert.Equal(["A", "B"], result.Select(r => r.WorkGroup));
        }

        #endregion

        #region GetBySellingPcAsync — Validation

        [Fact]
        public async Task GetBySellingPcAsync_WhenSellingPcIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetBySellingPcAsync(null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetBySellingPcAsync_WhenSellingPcIsEmptyOrWhitespace_ThrowsArgumentException(string sellingPc)
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetBySellingPcAsync(sellingPc));
        }

        #endregion
    }
}

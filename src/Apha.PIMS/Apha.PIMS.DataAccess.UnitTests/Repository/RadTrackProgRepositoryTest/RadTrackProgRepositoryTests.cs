using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Reflection;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.RadTrackProgRepositoryTest
{
    public class RadTrackProgRepositoryTests
    {
        private static (RadTrackProgRepository Repo, Mock<DbSet<RadtrackProg>> RadTrackProgsDbSet, Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<RadtrackProg>? radTrackProgs = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var radTrackProgsMockSet = RepositoryTestHelper.CreateMockDbSet(radTrackProgs ?? Enumerable.Empty<RadtrackProg>());

            RepositoryTestHelper.SetupDbSetOperations(radTrackProgsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.RadtrackProgs).Returns(radTrackProgsMockSet.Object);

            var repo = new RadTrackProgRepository(mockContext.Object);
            return (repo, radTrackProgsMockSet, mockContext);
        }

        private static RadTrackProgRepository CreateRepository(
            IEnumerable<RadtrackProg>? radTrackProgs = null,
            IEnumerable<Projects>? projects = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var radTrackProgsMockSet = RepositoryTestHelper.CreateMockDbSet(radTrackProgs ?? Enumerable.Empty<RadtrackProg>());
            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? Enumerable.Empty<Projects>());

            RepositoryTestHelper.SetupDbSetOperations(radTrackProgsMockSet);
            RepositoryTestHelper.SetupDbSetOperations(projectsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.RadtrackProgs).Returns(radTrackProgsMockSet.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(projectsMockSet.Object);

            return new RadTrackProgRepository(mockContext.Object);
        }

        [Fact]
        public async Task GetAllRadTrackProgsAsync_ReturnsOrderedByProgram()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "Z_PROG" },
                new() { Program = "A_PROG" },
                new() { Program = "M_PROG" }
            });

            var result = await repo.GetAllRadTrackProgsAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal("A_PROG", result[0].Program);
            Assert.Equal("M_PROG", result[1].Program);
            Assert.Equal("Z_PROG", result[2].Program);
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_DefaultSortByProgramAscending()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "C", Publicationprefix = "CP" },
                new() { Program = "A", Publicationprefix = "AP" },
                new() { Program = "B", Publicationprefix = "BP" }
            });

            var query = new PaginationParameters<string>(page: 1, pageSize: 2)
            {
                Filter = "{}",
                SortBy = null,
                Descending = false
            };

            var result = await repo.GetPagedRadTrackProgsAsync(query);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal("A", result.Data.First().Program);
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_SearchMatchesProgramOrPrefix()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "PVS", Publicationprefix = "AA" },
                new() { Program = "OTHER", Publicationprefix = "PVSX" },
                new() { Program = "NONE", Publicationprefix = "ZZ" }
            });

            var query = new PaginationParameters<string>(search: "PVS", page: 1, pageSize: 10)
            {
                Filter = "{}",
                SortBy = "program",
                Descending = false
            };

            var result = await repo.GetPagedRadTrackProgsAsync(query);

            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, x => x.Program == "PVS");
            Assert.Contains(result.Data, x => x.Program == "OTHER");
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_FilterByProgram_AppliesProgramBranch()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "ALPHA", Publicationprefix = "X1" },
                new() { Program = "BETA", Publicationprefix = "X2" }
            });

            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"program\":\"ALP\"}",
                SortBy = "program",
                Descending = false
            };

            var result = await repo.GetPagedRadTrackProgsAsync(query);

            var row = Assert.Single(result.Data);
            Assert.Equal("ALPHA", row.Program);
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_FilterByPublicationPrefix_AppliesPrefixBranch()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "A", Publicationprefix = "COMM" },
                new() { Program = "B", Publicationprefix = "XYZ" }
            });

            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"publicationprefix\":\"COM\"}",
                SortBy = "program",
                Descending = false
            };

            var result = await repo.GetPagedRadTrackProgsAsync(query);

            var row = Assert.Single(result.Data);
            Assert.Equal("A", row.Program);
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_WhitespaceFilterValue_IsIgnored()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "A", Publicationprefix = "AA" },
                new() { Program = "B", Publicationprefix = "BB" }
            });

            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"program\":\"   \"}",
                SortBy = "program",
                Descending = false
            };

            var result = await repo.GetPagedRadTrackProgsAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_SortsByProgramDescending()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "A", Publicationprefix = "P1" },
                new() { Program = "C", Publicationprefix = "P3" },
                new() { Program = "B", Publicationprefix = "P2" }
            });

            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{}",
                SortBy = "program",
                Descending = true
            };

            var result = await repo.GetPagedRadTrackProgsAsync(query);

            Assert.Equal("C", result.Data.First().Program);
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_SortsByPublicationPrefixAscending()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "X", Publicationprefix = "CC" },
                new() { Program = "Y", Publicationprefix = "AA" },
                new() { Program = "Z", Publicationprefix = "BB" }
            });

            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{}",
                SortBy = "publicationprefix",
                Descending = false
            };

            var result = await repo.GetPagedRadTrackProgsAsync(query);

            Assert.Equal("Y", result.Data.First().Program);
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_SortsByPublicationPrefixDescending()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "X", Publicationprefix = "CC" },
                new() { Program = "Y", Publicationprefix = "AA" },
                new() { Program = "Z", Publicationprefix = "BB" }
            });

            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{}",
                SortBy = "publicationprefix",
                Descending = true
            };

            var result = await repo.GetPagedRadTrackProgsAsync(query);

            Assert.Equal("X", result.Data.First().Program);
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_UnknownSortWithDescending_UsesDefaultDescendingProgram()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "A" },
                new() { Program = "C" },
                new() { Program = "B" }
            });

            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{}",
                SortBy = "unknown",
                Descending = true
            };

            var result = await repo.GetPagedRadTrackProgsAsync(query);

            Assert.Equal("C", result.Data.First().Program);
        }

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_ReturnsMatch_WhenProgramExists()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "PVS", Publicationprefix = "AA" },
                new() { Program = "OTHER", Publicationprefix = "BB" }
            });

            var result = await repo.GetRadTrackProgByProgramAsync("PVS");

            Assert.NotNull(result);
            Assert.Equal("PVS", result!.Program);
        }

        [Fact]
        public async Task GetRadTrackProgByProgramAsync_ReturnsNull_WhenProgramMissing()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "PVS", Publicationprefix = "AA" }
            });

            var result = await repo.GetRadTrackProgByProgramAsync("MISSING");

            Assert.Null(result);
        }

        [Fact]
        public async Task AddRadTrackProgAsync_ForcesRadtrackprogTrue_AndCallsSave()
        {
            var (repo, radTrackProgsDbSet, context) = CreateRepositoryWithMocks();
            var entity = new RadtrackProg
            {
                Program = "PVS",
                Publicationprefix = "PFX",
                Radtrackprog = false
            };

            var result = await repo.AddRadTrackProgAsync(entity);

            Assert.True(result.Radtrackprog);
            radTrackProgsDbSet.Verify(x => x.Add(It.Is<RadtrackProg>(r => r.Program == "PVS" && r.Radtrackprog)), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        [Fact]
        public async Task AddRadTrackProgAsync_WhenIncomingValueTrue_KeepsTrueAndCallsSave()
        {
            var (repo, radTrackProgsDbSet, context) = CreateRepositoryWithMocks();
            var entity = new RadtrackProg
            {
                Program = "ABC",
                Publicationprefix = "PX",
                Radtrackprog = true
            };

            var result = await repo.AddRadTrackProgAsync(entity);

            Assert.True(result.Radtrackprog);
            radTrackProgsDbSet.Verify(x => x.Add(It.Is<RadtrackProg>(r => r.Program == "ABC" && r.Radtrackprog)), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        [Fact]
        public async Task UpdateRadTrackProgAsync_ExistingFalseRadtrackprog_RemainsFalse()
        {
            var existing = new RadtrackProg
            {
                Program = "PVS",
                Publicationprefix = "OLD",
                Radtrackprog = false
            };

            var (repo, radTrackProgsDbSet, context) = CreateRepositoryWithMocks(new List<RadtrackProg> { existing });

            var updateRequest = new RadtrackProg
            {
                Program = "PVS",
                Publicationprefix = "NEW",
                Radtrackprog = true
            };

            var result = await repo.UpdateRadTrackProgAsync(updateRequest);

            Assert.Equal("NEW", result.Publicationprefix);
            Assert.False(result.Radtrackprog);
            Assert.False(existing.Radtrackprog);
            radTrackProgsDbSet.Verify(x => x.Update(It.IsAny<RadtrackProg>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        [Fact]
        public async Task UpdateRadTrackProgAsync_ExistingRecord_DoesNotChangeRadtrackprog()
        {
            var existing = new RadtrackProg
            {
                Program = "PVS",
                Publicationprefix = "OLD",
                Radtrackprog = true
            };

            var (repo, radTrackProgsDbSet, context) = CreateRepositoryWithMocks(new List<RadtrackProg> { existing });

            var updateRequest = new RadtrackProg
            {
                Program = "PVS",
                Publicationprefix = "NEW",
                Radtrackprog = false
            };

            var result = await repo.UpdateRadTrackProgAsync(updateRequest);

            Assert.Equal("NEW", result.Publicationprefix);
            Assert.True(result.Radtrackprog);
            Assert.True(existing.Radtrackprog);
            radTrackProgsDbSet.Verify(x => x.Update(It.IsAny<RadtrackProg>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        [Fact]
        public async Task UpdateRadTrackProgAsync_RecordNotFound_ThrowsInvalidOperationException()
        {
            var (repo, radTrackProgsDbSet, context) = CreateRepositoryWithMocks(new List<RadtrackProg>());
            var updateRequest = new RadtrackProg
            {
                Program = "UNKNOWN",
                Publicationprefix = "NEW",
                Radtrackprog = false
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateRadTrackProgAsync(updateRequest));

            Assert.Contains("UNKNOWN", ex.Message);
            radTrackProgsDbSet.Verify(x => x.Update(It.IsAny<RadtrackProg>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(context, 0);
        }

        [Fact]
        public async Task UpdateRadTrackProgAsync_AllowsNullPublicationPrefix_WithoutChangingRadtrackprog()
        {
            var existing = new RadtrackProg
            {
                Program = "PVS",
                Publicationprefix = "OLD",
                Radtrackprog = true
            };

            var (repo, radTrackProgsDbSet, context) = CreateRepositoryWithMocks(new List<RadtrackProg> { existing });
            var updateRequest = new RadtrackProg
            {
                Program = "PVS",
                Publicationprefix = null,
                Radtrackprog = false
            };

            var result = await repo.UpdateRadTrackProgAsync(updateRequest);

            Assert.Null(result.Publicationprefix);
            Assert.True(result.Radtrackprog);
            Assert.True(existing.Radtrackprog);
            radTrackProgsDbSet.Verify(x => x.Update(It.IsAny<RadtrackProg>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_WithMockQueryable_ThrowsInvalidOperationException()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "PVS", Publicationprefix = "AA", Radtrackprog = true }
            });

            var ex = await Assert.ThrowsAsync<TargetInvocationException>(() => repo.DeleteRadTrackProgAsync("PVS"));
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }

        [Fact]
        public async Task RadTrackProgExistsAsync_ReturnsTrue_WhenExists()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "PVS", Publicationprefix = "AA" }
            });

            var result = await repo.RadTrackProgExistsAsync("PVS");

            Assert.True(result);
        }

        [Fact]
        public async Task RadTrackProgExistsAsync_ReturnsFalse_WhenNotExists()
        {
            var repo = CreateRepository(new List<RadtrackProg>
            {
                new() { Program = "PVS", Publicationprefix = "AA" }
            });

            var result = await repo.RadTrackProgExistsAsync("UNKNOWN");

            Assert.False(result);
        }

        [Fact]
        public async Task GetAllProgramNamesAsync_ReturnsDistinctNonNullNamesOrdered()
        {
            var repo = CreateRepository(
                projects: new List<Projects>
                {
                    new() { Program = "B", Parentproject = "P2", Year = 2025 },
                    new() { Program = "A", Parentproject = "P1", Year = 2025 },
                    new() { Program = "B", Parentproject = "P3", Year = 2026 },
                    new() { Program = null, Parentproject = "P4", Year = 2026 }
                });

            var result = await repo.GetAllProgramNamesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("A", result[0]);
            Assert.Equal("B", result[1]);
        }
    }
}

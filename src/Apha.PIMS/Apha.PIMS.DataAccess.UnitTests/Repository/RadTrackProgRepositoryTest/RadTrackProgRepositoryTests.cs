using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

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
    }
}

using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.PublicationTypeRepositoryTest
{
    public class PublicationTypeRepositoryTests
    {
        private static PublicationTypeRepository CreateRepository(IEnumerable<PublicationType>? types = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var typesMockSet = RepositoryTestHelper.CreateMockDbSet(types ?? Enumerable.Empty<PublicationType>());

            RepositoryTestHelper.SetupDbSetOperations(typesMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.PublicationTypes).Returns(typesMockSet.Object);

            return new PublicationTypeRepository(mockContext.Object);
        }

        private static (PublicationTypeRepository Repo, Mock<DbSet<PublicationType>> TypesDbSet, Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<PublicationType>? types = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var typesMockSet = RepositoryTestHelper.CreateMockDbSet(types ?? Enumerable.Empty<PublicationType>());

            RepositoryTestHelper.SetupDbSetOperations(typesMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.PublicationTypes).Returns(typesMockSet.Object);

            var repo = new PublicationTypeRepository(mockContext.Object);
            return (repo, typesMockSet, mockContext);
        }

        private static PublicationType MakeType(string type = "RPC", string? description = null) =>
            new()
            {
                Type = type,
                Description = description
            };

        [Fact]
        public async Task GetAllPublicationTypesAsync_ReturnsAllTypes()
        {
            var repo = CreateRepository(new List<PublicationType>
            {
                MakeType("RPC", "Peer reviewed"),
                MakeType("NPR", "Non peer reviewed")
            });

            var result = await repo.GetAllPublicationTypesAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetPublicationTypeByCodeAsync_ReturnsType_WhenExists()
        {
            var repo = CreateRepository(new List<PublicationType> { MakeType("RPC", "Peer reviewed") });

            var result = await repo.GetPublicationTypeByCodeAsync("RPC");

            Assert.NotNull(result);
            Assert.Equal("RPC", result!.Type);
        }

        [Fact]
        public async Task GetPublicationTypeByCodeAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepository(new List<PublicationType> { MakeType("RPC") });

            var result = await repo.GetPublicationTypeByCodeAsync("ABC");

            Assert.Null(result);
        }

        [Fact]
        public async Task AddPublicationTypeAsync_ReturnsSameEntity_AndCallsSave()
        {
            var (repo, typesDbSet, context) = CreateRepositoryWithMocks();
            var entity = MakeType("ABC", "Desc");

            var result = await repo.AddPublicationTypeAsync(entity);

            Assert.Same(entity, result);
            typesDbSet.Verify(x => x.Add(It.Is<PublicationType>(p => p.Type == "ABC")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        [Fact]
        public async Task UpdatePublicationTypeAsync_ReturnsUpdatedEntity_AndCallsSave()
        {
            var (repo, _, context) = CreateRepositoryWithMocks(new List<PublicationType> { MakeType("RPC", "Old") });
            var entity = MakeType("RPC", "New");

            var result = await repo.UpdatePublicationTypeAsync(entity);

            Assert.Equal("New", result.Description);
            RepositoryTestHelper.VerifySaveChanges(context, 1);
        }

        [Fact]
        public async Task PublicationTypeExistsAsync_ReturnsTrue_WhenExists()
        {
            var repo = CreateRepository(new List<PublicationType> { MakeType("RPC") });

            var result = await repo.PublicationTypeExistsAsync("RPC");

            Assert.True(result);
        }

        [Fact]
        public async Task PublicationTypeExistsAsync_ReturnsFalse_WhenNotExists()
        {
            var repo = CreateRepository(new List<PublicationType> { MakeType("RPC") });

            var result = await repo.PublicationTypeExistsAsync("ABC");

            Assert.False(result);
        }
    }
}

using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.Helpers
{
    /// <summary>
    /// Common helper class for repository unit testing.
    /// Provides utilities for mocking DbContext, FpsYearContext, and DbSets without database dependencies.
    /// </summary>
    public static class RepositoryTestHelper
    {
        /// <summary>
        /// Default test FPS year used across repository tests.
        /// </summary>
        public const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a mocked FpsDbContext with no database dependencies.
        /// </summary>
        /// <param name="fpsYearContext">The FPS year context to use. If null, a default mock will be created.</param>
        /// <returns>Mocked FpsDbContext instance.</returns>
        public static Mock<FpsDbContext> CreateMockFpsDbContext(IFpsYearContext? fpsYearContext = null)
        {
            var yearContext = fpsYearContext ?? CreateMockFpsYearContext().Object;
            
            // Create DbContextOptions using DbContextOptionsBuilder with a fake provider
            // This doesn't actually connect to any database
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .Options;
            
            var mockContext = new Mock<FpsDbContext>(options, yearContext);
            mockContext.CallBase = false; // Prevent actual DbContext methods from being called
            
            return mockContext;
        }

        /// <summary>
        /// Creates a mocked IFpsYearContext with specified year.
        /// </summary>
        /// <param name="year">The FPS year to use. Defaults to DefaultTestFpsYear.</param>
        /// <returns>Mocked IFpsYearContext instance.</returns>
        public static Mock<IFpsYearContext> CreateMockFpsYearContext(int year = DefaultTestFpsYear)
        {
            var mockFpsYearContext = new Mock<IFpsYearContext>();
            mockFpsYearContext.Setup(x => x.FPSYear).Returns(year);
            return mockFpsYearContext;
        }

        /// <summary>
        /// Creates a mocked DbSet from a collection of entities with full async support.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="entities">The collection of entities to include in the mocked DbSet.</param>
        /// <returns>Mocked DbSet instance.</returns>
        public static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> entities) where T : class
        {
            return new TestAsyncEnumerable<T>(entities).AsDbSetMock();
        }

        /// <summary>
        /// Configures a mock context with SaveChangesAsync behavior.
        /// </summary>
        /// <param name="mockContext">The mock context to configure.</param>
        /// <param name="returnValue">The number of records affected (default: 1).</param>
        public static void SetupSaveChanges(Mock<FpsDbContext> mockContext, int returnValue = 1)
        {
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(returnValue);
        }

        /// <summary>
        /// Configures a mock DbSet with standard setup (Add, Remove, Update operations).
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="mockSet">The mock DbSet to configure.</param>
        public static void SetupDbSetOperations<T>(Mock<DbSet<T>> mockSet) where T : class
        {
            mockSet.Setup(x => x.Add(It.IsAny<T>())).Verifiable();
            mockSet.Setup(x => x.Remove(It.IsAny<T>())).Verifiable();
            mockSet.Setup(x => x.Update(It.IsAny<T>())).Verifiable();
        }

        /// <summary>
        /// Creates a complete repository test context with mocked DbContext, FpsYearContext, and specified entities.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entities">The entities to include in the DbSet.</param>
        /// <param name="fpsYear">The FPS year to use.</param>
        /// <returns>Tuple containing mocked context, mocked year context, and mocked DbSet.</returns>
        public static (Mock<FpsDbContext> Context, Mock<IFpsYearContext> YearContext, Mock<DbSet<TEntity>> DbSet) 
            CreateRepositoryContext<TEntity>(IEnumerable<TEntity> entities, int fpsYear = DefaultTestFpsYear) 
            where TEntity : class
        {
            var mockYearContext = CreateMockFpsYearContext(fpsYear);
            var mockContext = CreateMockFpsDbContext(mockYearContext.Object);
            var mockDbSet = CreateMockDbSet(entities);

            SetupSaveChanges(mockContext);
            SetupDbSetOperations(mockDbSet);

            return (mockContext, mockYearContext, mockDbSet);
        }

        /// <summary>
        /// Verifies that SaveChangesAsync was called the expected number of times.
        /// </summary>
        /// <param name="mockContext">The mock context to verify.</param>
        /// <param name="times">Expected number of calls.</param>
        public static void VerifySaveChanges(Mock<FpsDbContext> mockContext, int times = 1)
        {
            mockContext.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), 
                Times.Exactly(times));
        }

        /// <summary>
        /// Verifies that an entity was added to the DbSet.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="mockSet">The mock DbSet to verify.</param>
        /// <param name="times">Expected number of calls.</param>
        public static void VerifyAdd<T>(Mock<DbSet<T>> mockSet, int times = 1) where T : class
        {
            mockSet.Verify(x => x.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()), Times.Exactly(times));
        }

        /// <summary>
        /// Verifies that an entity was removed from the DbSet.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="mockSet">The mock DbSet to verify.</param>
        /// <param name="times">Expected number of calls.</param>
        public static void VerifyRemove<T>(Mock<DbSet<T>> mockSet, int times = 1) where T : class
        {
            mockSet.Verify(x => x.Remove(It.IsAny<T>()), Times.Exactly(times));
        }

        /// <summary>
        /// Verifies that an entity was updated in the DbSet.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="mockSet">The mock DbSet to verify.</param>
        /// <param name="times">Expected number of calls.</param>
        public static void VerifyUpdate<T>(Mock<DbSet<T>> mockSet, int times = 1) where T : class
        {
            mockSet.Verify(x => x.Update(It.IsAny<T>()), Times.Exactly(times));
        }

        /// <summary>
        /// Sets up the Entry method for a DbContext mock to support Entity Framework state tracking.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="mockContext">The mock context to configure.</param>
        public static void SetupEntityEntry<T>(Mock<FpsDbContext> mockContext) where T : class
        {
            mockContext.Setup(x => x.Entry(It.IsAny<T>()))
                .Returns((T entity) =>
                {
                    var mockEntry = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>>();
                    mockEntry.SetupAllProperties();
                    return mockEntry.Object;
                });
        }
    }
}
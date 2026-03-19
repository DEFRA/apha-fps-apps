using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.Common.Helpers.Repository;

/// <summary>
/// Common helper class for repository unit testing.
/// Provides utilities for mocking DbContext and DbSets without database dependencies.
/// Generic implementation that works with any DbContext (FPS, PACT, Costbook, PIMS).
/// </summary>
public static class RepositoryTestHelper
{
    /// <summary>
    /// Creates a mocked DbContext with no database dependencies.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type (e.g., FpsDbContext, PactDbContext)</typeparam>
    /// <param name="constructorArgs">Optional constructor arguments for the DbContext</param>
    /// <returns>Mocked DbContext instance.</returns>
    public static Mock<TContext> CreateMockDbContext<TContext>(params object[] constructorArgs) 
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>().Options;

        var allArgs = new List<object> { options };
        if (constructorArgs != null && constructorArgs.Length > 0)
        {
            allArgs.AddRange(constructorArgs);
        }

        var mockContext = new Mock<TContext>(allArgs.ToArray());
        mockContext.CallBase = false; // Prevent actual DbContext methods from being called

        return mockContext;
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
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="mockContext">The mock context to configure.</param>
    /// <param name="returnValue">The number of records affected (default: 1).</param>
    public static void SetupSaveChanges<TContext>(Mock<TContext> mockContext, int returnValue = 1) 
        where TContext : DbContext
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
    /// Creates a complete repository test context with mocked DbContext and specified entities.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entities">The entities to include in the DbSet.</param>
    /// <param name="constructorArgs">Optional constructor arguments for the DbContext</param>
    /// <returns>Tuple containing mocked context and mocked DbSet.</returns>
    public static (Mock<TContext> Context, Mock<DbSet<TEntity>> DbSet) 
        CreateRepositoryContext<TContext, TEntity>(
            IEnumerable<TEntity> entities, 
            params object[] constructorArgs) 
        where TContext : DbContext
        where TEntity : class
    {
        var mockContext = CreateMockDbContext<TContext>(constructorArgs);
        var mockDbSet = CreateMockDbSet(entities);

        SetupSaveChanges(mockContext);
        SetupDbSetOperations(mockDbSet);

        return (mockContext, mockDbSet);
    }

    /// <summary>
    /// Verifies that SaveChangesAsync was called the expected number of times.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="mockContext">The mock context to verify.</param>
    /// <param name="times">Expected number of calls.</param>
    public static void VerifySaveChanges<TContext>(Mock<TContext> mockContext, int times = 1) 
        where TContext : DbContext
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
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="mockContext">The mock context to configure.</param>
    public static void SetupEntityEntry<TContext, T>(Mock<TContext> mockContext) 
        where TContext : DbContext
        where T : class
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
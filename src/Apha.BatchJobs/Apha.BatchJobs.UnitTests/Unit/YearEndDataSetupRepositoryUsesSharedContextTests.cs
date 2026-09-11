using System.Reflection;
using Apha.BatchJobs.Infrastructure.YearEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Architectural fitness function for Year End main-port Phase 7A: <see cref="YearEndDataSetupRepository"/>
/// must depend on a single shared, scoped <see cref="Apha.BatchJobs.Infrastructure.Data.BatchJobsDbContext"/>
/// — never an <see cref="IDbContextFactory{TContext}"/> — because
/// <see cref="YearEndDataSetupTransactionManager"/> begins its transaction on that exact scoped
/// instance. A method that independently called <c>IDbContextFactory.CreateDbContext()</c> would open
/// a second, unrelated connection that silently escapes the transaction, breaking the all-or-nothing
/// guarantee without any compiler error to catch it.
/// </summary>
public sealed class YearEndDataSetupRepositoryUsesSharedContextTests
{
    [Fact]
    public void YearEndDataSetupRepository_ShouldNotDependOnIDbContextFactory()
    {
        var offendingMembers = typeof(YearEndDataSetupRepository)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => IsDbContextFactory(f.FieldType))
            .Select(f => f.Name)
            .Concat(typeof(YearEndDataSetupRepository)
                .GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Where(p => IsDbContextFactory(p.ParameterType))
                .Select(p => p.Name ?? "<unnamed>"))
            .ToList();

        Assert.True(
            offendingMembers.Count == 0,
            $"YearEndDataSetupRepository references IDbContextFactory via [{string.Join(", ", offendingMembers)}] " +
            "— this repository must use a single shared, scoped BatchJobsDbContext so it participates in " +
            $"{nameof(YearEndDataSetupTransactionManager)}'s ambient transaction. Any independently-created " +
            "context would silently escape it.");

        static bool IsDbContextFactory(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDbContextFactory<>);
    }
}

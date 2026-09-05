using System.Reflection;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Architectural fitness function: the Year End steps that used to each own their own hardcoded
/// table-name list were rewritten (Year End main-port Phases 2 and 4) to derive everything from
/// <see cref="YearEndTableRuleMatrix"/> instead. This proves that stays true going forward — a
/// static field of a string-collection type on any of these types is exactly the shape a
/// reintroduced hardcoded list would take.
///
/// <c>ProjectFinancialResetStep</c>/<c>ConfiguredPlanningResetStep</c> were added in Phase 4, once
/// rewritten to consume the matrix via the repository-driven <see cref="YearEndMatrixResetApplier"/>
/// (not the source branch's <c>DbConnection</c>/<c>DbTransaction</c>-taking <c>YearEndMatrixResetHelper</c>
/// shape) and to drop their <c>mabarchive.my_*</c> targets — closing the gap this test's doc comment
/// previously flagged.
/// </summary>
public sealed class YearEndNoHardcodedTableListsTests
{
    public static IEnumerable<object[]> MatrixDrivenStepTypes =>
    [
        [typeof(CopyFpsYearScopedTablesStep)],
        [typeof(FinalValidationStep)],
        [typeof(ProjectFinancialResetStep)],
        [typeof(ConfiguredPlanningResetStep)]
    ];

    [Theory]
    [MemberData(nameof(MatrixDrivenStepTypes))]
    public void Type_ShouldNotDeclareAStaticStringCollectionField(Type stepType)
    {
        var suspiciousFields = stepType
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => typeof(IEnumerable<string>).IsAssignableFrom(f.FieldType))
            .Select(f => f.Name)
            .ToList();

        Assert.True(
            suspiciousFields.Count == 0,
            $"{stepType.Name} declares static string-collection field(s) [{string.Join(", ", suspiciousFields)}] " +
            "— that's the shape of a hardcoded table list. Table names should come from " +
            $"{nameof(YearEndTableRuleMatrix)}.{nameof(YearEndTableRuleMatrix.Entries)} instead.");
    }

    [Theory]
    [MemberData(nameof(MatrixDrivenStepTypes))]
    public void Type_ShouldNotDeclareAStaticTupleOrValueTupleCollectionField(Type stepType)
    {
        // Covers a (Schema, Table, YearColumn)-shaped tuple list — a plain IEnumerable<string> check
        // wouldn't catch that shape.
        var suspiciousFields = stepType
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => typeof(System.Collections.IEnumerable).IsAssignableFrom(f.FieldType) && f.FieldType != typeof(string))
            .Where(f => f.FieldType.IsGenericType && IsTupleLike(f.FieldType.GetGenericArguments().FirstOrDefault()))
            .Select(f => f.Name)
            .ToList();

        Assert.True(
            suspiciousFields.Count == 0,
            $"{stepType.Name} declares static tuple-collection field(s) [{string.Join(", ", suspiciousFields)}] " +
            "— that's the shape of a hardcoded (schema, table, ...) list. Table metadata should come from " +
            $"{nameof(YearEndTableRuleMatrix)}.{nameof(YearEndTableRuleMatrix.Entries)} instead.");

        static bool IsTupleLike(Type? elementType) =>
            elementType is not null
            && elementType.IsGenericType
            && (elementType.FullName?.StartsWith("System.ValueTuple", StringComparison.Ordinal) ?? false);
    }
}

using System.Reflection;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Architectural fitness function: the Year End steps that used to each own their own hardcoded
/// table-name list (<c>CopyFpsYearScopedTablesStep</c>, <c>TargetYearEmptyTablesStep</c>,
/// <c>FinalValidationStep</c>, <c>ProjectFinancialResetStep</c>, <c>ConfiguredPlanningResetStep</c>)
/// were rewritten 2026-08-14 to derive everything from <see cref="YearEndTableRuleMatrix"/> instead.
/// This proves that stays true going forward — a static field of a string-collection type on any of
/// these types is exactly the shape a reintroduced hardcoded list would take.
/// </summary>
public sealed class YearEndNoHardcodedTableListsTests
{
    public static IEnumerable<object[]> MatrixDrivenStepTypes =>
    [
        [typeof(CopyFpsYearScopedTablesStep)],
        [typeof(TargetYearEmptyTablesStep)],
        [typeof(FinalValidationStep)],
        [typeof(ProjectFinancialResetStep)],
        [typeof(ConfiguredPlanningResetStep)],
        [typeof(YearEndMatrixResetHelper)]
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
        // Covers the (Schema, Table, YearColumn)-shaped tuple list FinalValidationStep used to own
        // (RequiredTargetYearDataTables) — a plain IEnumerable<string> check wouldn't catch that shape.
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

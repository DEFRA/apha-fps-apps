using Apha.BatchJobs.Application.FailureHandling;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="DiagnosticSummaryBuilder"/> — the bounded, best-effort technical summary
/// written only to the Failed-transition <c>job_queue_log</c> row, never to
/// <c>job_queue.errormessage</c>.
/// </summary>
public sealed class DiagnosticSummaryBuilderTests
{
    [Fact]
    public void Build_WithShortMessage_ReturnsMessageUnchanged()
    {
        var result = DiagnosticSummaryBuilder.Build(new InvalidOperationException("short failure"));

        Assert.Equal("short failure", result);
    }

    [Fact]
    public void Build_WithMessageLongerThanMaxLength_TruncatesToMaxLength()
    {
        var longMessage = new string('x', DiagnosticSummaryBuilder.MaxLength + 100);

        var result = DiagnosticSummaryBuilder.Build(new InvalidOperationException(longMessage));

        Assert.Equal(DiagnosticSummaryBuilder.MaxLength, result.Length);
        Assert.Equal(new string('x', DiagnosticSummaryBuilder.MaxLength), result);
    }

    [Fact]
    public void Build_RespectsCustomMaxLength()
    {
        var result = DiagnosticSummaryBuilder.Build(new InvalidOperationException("0123456789"), maxLength: 5);

        Assert.Equal("01234", result);
    }

    [Fact]
    public void Build_WithMultilineMessage_NormalizesToSingleReadableLine()
    {
        var result = DiagnosticSummaryBuilder.Build(new InvalidOperationException("line one\r\nline two\nline three"));

        Assert.DoesNotContain('\n', result);
        Assert.DoesNotContain('\r', result);
        Assert.Contains("line one", result);
        Assert.Contains("line two", result);
        Assert.Contains("line three", result);
    }

    [Fact]
    public void Build_WithBlankMessage_ReturnsUsefulFallback()
    {
        var result = DiagnosticSummaryBuilder.Build(new BlankMessageException());

        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Equal("Execution failed with no further detail available.", result);
    }

    [Fact]
    public void Build_NeverThrows_EvenWhenTheExceptionsOwnMessageGetterThrows()
    {
        var result = DiagnosticSummaryBuilder.Build(new ThrowingMessageException());

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    /// <summary>
    /// Enforces the separation the whole design exists for: a marker present only in the raw
    /// exception must reach the bounded diagnostic summary, and must never leak into the friendly
    /// text a caller derives separately (via <see cref="BatchFailureMessageProvider"/>) for
    /// <c>job_queue.errormessage</c>.
    /// </summary>
    [Fact]
    public void Build_PreservesRawMarkerText_ThatMustNeverReachTheFriendlyErrorMessage()
    {
        const string marker = "RAW_DATABASE_VALUE_ABC123";
        var exception = new InvalidOperationException($"Constraint violated: {marker}");

        var diagnosticSummary = DiagnosticSummaryBuilder.Build(exception);
        var friendlyErrorMessage = BatchFailureMessageProvider.GetHumanReadableMessage(BatchFailureCategory.Sql);

        Assert.Contains(marker, diagnosticSummary);
        Assert.DoesNotContain(marker, friendlyErrorMessage);
    }

    private sealed class BlankMessageException : Exception
    {
        public override string Message => string.Empty;
    }

    private sealed class ThrowingMessageException : Exception
    {
        public override string Message => throw new InvalidOperationException("Message getter is broken");
    }
}

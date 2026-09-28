namespace Apha.BatchJobs.Domain;

/// <summary>FPS year is April–March, named by its start year (fps.tblyearmaster.fpsyearcode).</summary>
public static class FpsYearResolver
{
    public static int ResolveFpsYear(int? jobFpsYear, DateTime occurredAtUtc) =>
        jobFpsYear ?? (occurredAtUtc.Month >= 4 ? occurredAtUtc.Year : occurredAtUtc.Year - 1);
}

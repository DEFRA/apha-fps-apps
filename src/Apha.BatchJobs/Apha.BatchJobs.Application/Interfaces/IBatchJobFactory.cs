namespace Apha.BatchJobs.Application.Interfaces;

/// <summary>Factory for resolving batch job instances by name.</summary>
public interface IBatchJobFactory
{
    /// <summary>Creates and returns the batch job registered under <paramref name="jobName"/>.</summary>
    /// <param name="jobName">The registered name of the job to create.</param>
    /// <returns>The matching <see cref="IBatchJob"/> instance.</returns>
    IBatchJob Create(string jobName);
}
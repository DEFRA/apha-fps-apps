namespace Apha.BatchJobs.Domain.Entities;

/// <summary>Represents the metadata definition of a registered batch job.</summary>
public sealed class BatchJobDefinition
{
    /// <summary>Gets the unique name that identifies this batch job.</summary>
    public required string Name { get; init; }

    /// <summary>Gets an optional human-readable description of the job's purpose.</summary>
    public string? Description { get; init; }
}
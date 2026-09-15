namespace Apha.BatchJobs.Domain.Enums;

/// <summary>Enumeration of batch job types.</summary>
public enum JobType
{
    /// <summary>Default job type.</summary>
    Unknown = 0,

    /// <summary>Data load/ETL job.</summary>
    DataLoad = 1,

    /// <summary>Data archival job.</summary>
    Archival = 2,

    /// <summary>Data validation job.</summary>
    Validation = 3,

    /// <summary>Cleanup job.</summary>
    Cleanup = 4
}

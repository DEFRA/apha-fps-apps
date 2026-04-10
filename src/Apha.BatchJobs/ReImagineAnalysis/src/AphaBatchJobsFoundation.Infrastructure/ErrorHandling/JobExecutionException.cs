using System;
using System.Runtime.Serialization;

namespace AphaBatchJobsFoundation.Infrastructure.ErrorHandling
{
    /// <summary>
    /// Custom exception class for job execution errors with exit code support for scheduler integration.
    /// Provides scheduler-friendly exit codes to enable proper error handling and retry logic in batch job orchestration.
    /// </summary>
    [Serializable]
    public class JobExecutionException : Exception
    {
        /// <summary>
        /// Gets the scheduler-friendly exit code associated with this exception.
        /// Exit codes can be used by schedulers to determine retry behavior and error severity.
        /// Common convention: 0 = Success, 1 = General Error, 2+ = Specific Error Codes
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Initializes a new instance of the JobExecutionException class with a default message.
        /// </summary>
        public JobExecutionException() 
            : base()
        {
            ExitCode = 1; // Default general error code
        }

        /// <summary>
        /// Initializes a new instance of the JobExecutionException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public JobExecutionException(string message) 
            : base(message)
        {
            ExitCode = 1; // Default general error code
        }

        /// <summary>
        /// Initializes a new instance of the JobExecutionException class with a specified error message 
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public JobExecutionException(string message, Exception innerException) 
            : base(message, innerException)
        {
            ExitCode = 1; // Default general error code
        }

        /// <summary>
        /// Initializes a new instance of the JobExecutionException class with a specified error message and exit code.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="exitCode">The scheduler-friendly exit code for this exception.</param>
        public JobExecutionException(string message, int exitCode) 
            : base(message)
        {
            ExitCode = exitCode;
        }

        /// <summary>
        /// Initializes a new instance of the JobExecutionException class with a specified error message, 
        /// exit code, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="exitCode">The scheduler-friendly exit code for this exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public JobExecutionException(string message, int exitCode, Exception innerException) 
            : base(message, innerException)
        {
            ExitCode = exitCode;
        }

        /// <summary>
        /// Initializes a new instance of the JobExecutionException class with serialized data.
        /// This constructor is required for proper exception serialization across application domains.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected JobExecutionException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            if (info != null)
            {
                ExitCode = info.GetInt32(nameof(ExitCode));
            }
        }

        /// <summary>
        /// Sets the SerializationInfo with information about the exception.
        /// This method is required for proper exception serialization across application domains.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(ExitCode), ExitCode);
            base.GetObjectData(info, context);
        }
    }
}


// Key improvements made to align with .NET best practices:
// 1. Added [Serializable] attribute for proper exception serialization support
// 2. Added parameterless constructor as per exception design guidelines
// 3. Added constructor with only message parameter for standard exception pattern
// 4. Added constructor with message and innerException for standard exception pattern
// 5. Implemented serialization constructor for cross-AppDomain scenarios
// 6. Overridden GetObjectData method to properly serialize the ExitCode property
// 7. Added null check in GetObjectData as per security best practices
// 8. Maintained all existing functionality while following the standard exception pattern
// 9. Set default ExitCode of 1 for constructors without explicit exitCode parameter
namespace Apha.FPSApps.Application.Validation
{
    public class ValidationErrorResponse
    {
        public string Status { get; set; } = "error";
        public string Message { get; set; } = "Validation failed.";
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();

        public ValidationErrorResponse(List<ValidationError> errors)
        {
            Errors = errors;
        }
    }
}

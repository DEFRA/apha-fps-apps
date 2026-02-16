namespace Apha.FPS.Application.Validation
{
    public class BusinessValidationErrorException : Exception
    {
        public string Status { get; set; } = "error";
        public string ExceptionMessage { get; set; } = "Business validation failed.";


        public List<BusinessValidationError> Errors { get; set; } = new List<BusinessValidationError>();

        public BusinessValidationErrorException(List<BusinessValidationError> errors)
        {
            Errors = errors;
        }
    }
}

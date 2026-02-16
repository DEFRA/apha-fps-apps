namespace Apha.FPSApps.Application.Validation
{
    public class ValidationError
    {
        public string Field { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }

        public ValidationError(string field, string message, string code = null)
        {
            Field = field;
            Message = message;
            Code = code;
        }
    }
}

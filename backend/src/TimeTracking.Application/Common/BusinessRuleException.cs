namespace TimeTracking.Application.Common;

public class BusinessRuleException : Exception
{
    public string Code { get; }

    public int StatusCode { get; }

    public BusinessRuleException(string code, string message, int statusCode = HttpStatus.BadRequest)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

namespace TimeTracking.Application.Common;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";

    public const string InvalidHours = "INVALID_HOURS";
    public const string DayHoursLimitExceeded = "DAY_HOURS_LIMIT_EXCEEDED";
    public const string RateNotFoundOnDate = "RATE_NOT_FOUND_ON_DATE";
    public const string ProjectPeriodViolation = "PROJECT_PERIOD_VIOLATION";
    public const string PeriodClosed = "PERIOD_CLOSED";
    public const string EntryNotFound = "ENTRY_NOT_FOUND";
    public const string EntryVersionConflict = "ENTRY_VERSION_CONFLICT";
    public const string EmployeeNotFound = "EMPLOYEE_NOT_FOUND";
    public const string ProjectNotFound = "PROJECT_NOT_FOUND";
    public const string InvalidRate = "INVALID_RATE";
    public const string BadPeriod = "BAD_PERIOD";
}

public static class HttpStatus
{
    public const int BadRequest = 400;
    public const int NotFound = 404;
    public const int Conflict = 409;
}

namespace TimeTracking.Application.Common;

public static class Money
{
    /// <summary>
    /// Округление до копеек
    /// </summary>
    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

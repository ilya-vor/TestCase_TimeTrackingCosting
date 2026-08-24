namespace TimeTracking.Application.Common;

public static class Money
{
    /// <summary>Округление денег до копеек (от нуля — как принято для рублёвых сумм).</summary>
    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

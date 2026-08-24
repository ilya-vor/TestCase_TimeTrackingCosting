using FluentAssertions;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;
using TimeTracking.Domain;

namespace TimeTracking.UnitTests;

/// <summary>
/// Правило 1: стоимость записи считается по ставке, действовавшей на дату записи.
/// Ставки можно менять задним числом — резолв всегда по дате.
/// </summary>
public class EmployeeRatesTests
{
    private static DateTime D(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    private static readonly List<Rate> IvanovRates = new()
    {
        new Rate { From = D(2026, 3, 1), Value = 600m },
        new Rate { From = D(2026, 1, 1), Value = 500m }
    };

    [Fact]
    public void EffectiveOn_date_before_any_rate_returns_null()
    {
        EmployeeRates.EffectiveOn(IvanovRates, D(2025, 12, 15)).Should().BeNull();
    }

    [Fact]
    public void EffectiveOn_february_uses_rate_500()
    {
        var rate = EmployeeRates.EffectiveOn(IvanovRates, D(2026, 2, 20));
        rate.Should().NotBeNull();
        rate!.Value.Should().Be(500m);
    }

    [Fact]
    public void EffectiveOn_march_uses_rate_600()
    {
        var rate = EmployeeRates.EffectiveOn(IvanovRates, D(2026, 3, 5));
        rate.Should().NotBeNull();
        rate!.Value.Should().Be(600m);
    }

    [Fact]
    public void EffectiveOn_rate_from_date_is_inclusive()
    {
        // Ставка действует с указанной даты — на 01.03.2026 уже 600.
        var rate = EmployeeRates.EffectiveOn(IvanovRates, D(2026, 3, 1));
        rate.Should().NotBeNull();
        rate!.Value.Should().Be(600m);
    }

    [Fact]
    public void EffectiveOn_does_not_depend_on_order_of_rates()
    {
        var sorted = IvanovRates.OrderBy(r => r.From).ToList();
        EmployeeRates.EffectiveOn(IvanovRates, D(2026, 2, 20)).Should()
            .BeEquivalentTo(EmployeeRates.EffectiveOn(sorted, D(2026, 2, 20)));
    }

    [Fact]
    public void RequireOn_throws_when_no_rate_on_date()
    {
        var act = () => EmployeeRates.RequireOn(IvanovRates, D(2025, 12, 15), "Иванов");
        act.Should().Throw<BusinessRuleException>().Where(e => e.Code == ErrorCodes.RateNotFoundOnDate);
    }
}

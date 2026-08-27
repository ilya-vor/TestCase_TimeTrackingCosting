using FluentValidation;
using MediatR;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Application.Periods;

public class ClosePeriodCommand : IRequest<Unit>
{
    public int Year { get; set; }
    public int Month { get; set; }
}

public class PeriodValidator : AbstractValidator<ClosePeriodCommand>
{
    public PeriodValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Год вне допустимого диапазона.");
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Месяц должен быть от 1 до 12.");
    }
}

public class ClosePeriodCommandHandler(ITimeTrackingDb _db) : IRequestHandler<ClosePeriodCommand, Unit>
{
    public async Task<Unit> Handle(ClosePeriodCommand command, CancellationToken ct)
    {
        await _db.ClosedPeriods.ReplaceOneAsync(
            p => p.Year == command.Year && p.Month == command.Month,
            new ClosedPeriod { Year = command.Year, Month = command.Month },
            new ReplaceOptions { IsUpsert = true },
            ct);
        return Unit.Value;
    }
}

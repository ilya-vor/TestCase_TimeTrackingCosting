using FluentValidation;
using MediatR;
using MongoDB.Driver;
using TimeTracking.Application.Common;

namespace TimeTracking.Application.Periods;

public class OpenPeriodCommand : IRequest<Unit>
{
    public int Year { get; set; }
    public int Month { get; set; }
}

public class OpenPeriodValidator : AbstractValidator<OpenPeriodCommand>
{
    public OpenPeriodValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Год вне допустимого диапазона.");
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Месяц должен быть от 1 до 12.");
    }
}

public class OpenPeriodCommandHandler(ITimeTrackingDb _db) : IRequestHandler<OpenPeriodCommand, Unit>
{
    public async Task<Unit> Handle(OpenPeriodCommand command, CancellationToken ct)
    {
        await _db.ClosedPeriods.DeleteOneAsync(
            p => p.Year == command.Year && p.Month == command.Month,
            ct);
        return Unit.Value;
    }
}

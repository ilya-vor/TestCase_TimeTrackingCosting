using FluentValidation;
using MediatR;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Application.Employees;

/// <summary>
/// Задание не описывает API для смены ставок, но сценарий приёмочной проверки № 8
/// («изменить ставку задним числом и перестроить отчёт») требует такого изменения.
/// Upsert по дате начала: если ставка на дату уже есть — меняем её значение, иначе добавляем новую.
/// </summary>
public class SetEmployeeRateCommand : IRequest<Unit>
{
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime From { get; set; }
    public decimal Value { get; set; }
}

public class SetEmployeeRateCommandValidator : AbstractValidator<SetEmployeeRateCommand>
{
    public SetEmployeeRateCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Сотрудник обязателен.");
        RuleFor(x => x.Value).GreaterThan(0).WithMessage("Ставка должна быть положительной.");
        RuleFor(x => x.From).Must(d => d != default).WithMessage("Дата начала действия ставки обязательна.");
    }
}

public class SetEmployeeRateCommandHandler : IRequestHandler<SetEmployeeRateCommand, Unit>
{
    private readonly ITimeTrackingDb _db;

    public SetEmployeeRateCommandHandler(ITimeTrackingDb db) => _db = db;

    public async Task<Unit> Handle(SetEmployeeRateCommand command, CancellationToken ct)
    {
        var from = Dates.CalendarDateToUtc(command.From);

        var employee = await _db.Employees.Find(e => e.Id == command.EmployeeId).FirstOrDefaultAsync(ct)
            ?? throw new BusinessRuleException(ErrorCodes.EmployeeNotFound, "Сотрудник не найден.", HttpStatus.NotFound);

        var existing = employee.Rates.FirstOrDefault(r => r.From == from);
        if (existing is null)
        {
            employee.Rates.Add(new Rate { From = from, Value = command.Value });
        }
        else
        {
            existing.Value = command.Value;
        }

        employee.Rates = employee.Rates.OrderBy(r => r.From).ToList();

        await _db.Employees.ReplaceOneAsync(e => e.Id == employee.Id, employee, cancellationToken: ct);
        return Unit.Value;
    }
}

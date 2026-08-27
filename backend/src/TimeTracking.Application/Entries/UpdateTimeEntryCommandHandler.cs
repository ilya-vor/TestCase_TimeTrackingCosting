using MediatR;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;
using TimeTracking.Domain;

namespace TimeTracking.Application.Entries;

public class UpdateTimeEntryCommandHandler(ITimeTrackingDb _db) : IRequestHandler<UpdateTimeEntryCommand, TimeEntryRow>
{
    public async Task<TimeEntryRow> Handle(UpdateTimeEntryCommand command, CancellationToken ct)
    {
        var date = Dates.CalendarDateToUtc(command.Date);
        EntryHoursRule.Validate(command.Hours);

        TimeEntry? updated = null;
        decimal rate = 0m;

        await TransactionRunner.RunAsync(_db, async (session, token) =>
        {
            var entry = await _db.TimeEntries.Find(session, e => e.Id == command.Id).FirstOrDefaultAsync(token)
                ?? throw new BusinessRuleException(ErrorCodes.EntryNotFound, "Запись табеля не найдена.", HttpStatus.NotFound);

            if (entry.Version != command.ExpectedVersion)
                throw new BusinessRuleException(ErrorCodes.EntryVersionConflict,
                    "Запись была изменена другим пользователем. Перезагрузите данные и повторите сохранение.",
                    HttpStatus.Conflict);

            var employee = await _db.Employees.Find(session, e => e.Id == command.EmployeeId).FirstOrDefaultAsync(token)
                ?? throw new BusinessRuleException(ErrorCodes.EmployeeNotFound, "Сотрудник не найден.", HttpStatus.NotFound);
            var project = await _db.Projects.Find(session, p => p.Id == command.ProjectId).FirstOrDefaultAsync(token)
                ?? throw new BusinessRuleException(ErrorCodes.ProjectNotFound, "Проект не найден.", HttpStatus.NotFound);

            // Закрытый период проверяем и по старой, и по новой дате: дату можно менять.
            var oldClosed = await _db.ClosedPeriods
                .Find(session, p => p.Year == entry.Date.Year && p.Month == entry.Date.Month).ToListAsync(token);
            ClosedPeriodRule.ThrowIfClosed(oldClosed, entry.Date);
            var newClosed = await _db.ClosedPeriods
                .Find(session, p => p.Year == date.Year && p.Month == date.Month).ToListAsync(token);
            ClosedPeriodRule.ThrowIfClosed(newClosed, date);

            ProjectPeriodRule.ThrowIfOutside(date, project.Start, project.End);
            rate = EmployeeRates.RequireOn(employee.Rates, date, employee.Name);

            // Счётчик: убираем вклад старой записи, добавляем новую — лимит проверяется при инкременте.
            await DayCounterService.AddHoursAsync(_db, session, entry.EmployeeId, entry.Date, -entry.Hours, token);
            await DayCounterService.AddHoursAsync(_db, session, employee.Id, date, command.Hours, token);

            entry.EmployeeId = employee.Id;
            entry.ProjectId = project.Id;
            entry.Date = date;
            entry.Hours = command.Hours;
            entry.Comment = command.Comment ?? string.Empty;
            entry.Version += 1;
            entry.UpdatedAt = DateTime.UtcNow;
            entry.UpdatedBy = "demo";

            await _db.TimeEntries.ReplaceOneAsync(session, e => e.Id == entry.Id, entry, cancellationToken: token);
            updated = entry;
        }, ct);

        return await TimeEntryRowProjector.BuildWithLookupsAsync(_db, null, updated!, ct, rate);
    }
}


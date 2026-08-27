using MediatR;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;
using TimeTracking.Domain;

namespace TimeTracking.Application.Entries;

public class CreateTimeEntryCommandHandler(ITimeTrackingDb _db) : IRequestHandler<CreateTimeEntryCommand, TimeEntryRow>
{
    public async Task<TimeEntryRow> Handle(CreateTimeEntryCommand command, CancellationToken ct)
    {
        var date = Dates.CalendarDateToUtc(command.Date);
        EntryHoursRule.Validate(command.Hours);

        TimeEntry? created = null;

        await TransactionRunner.RunAsync(_db, async (session, token) =>
        {
            var employee = await _db.Employees.Find(session, e => e.Id == command.EmployeeId).FirstOrDefaultAsync(token)
                ?? throw new BusinessRuleException(ErrorCodes.EmployeeNotFound, "Сотрудник не найден.", HttpStatus.NotFound);
            var project = await _db.Projects.Find(session, p => p.Id == command.ProjectId).FirstOrDefaultAsync(token)
                ?? throw new BusinessRuleException(ErrorCodes.ProjectNotFound, "Проект не найден.", HttpStatus.NotFound);

            var closedPeriods = await _db.ClosedPeriods
                .Find(session, p => p.Year == date.Year && p.Month == date.Month).ToListAsync(token);
            ClosedPeriodRule.ThrowIfClosed(closedPeriods, date);

            ProjectPeriodRule.ThrowIfOutside(date, project.Start, project.End);
            EmployeeRates.RequireOn(employee.Rates, date, employee.Name);

            var dayTotal = await DayHoursAggregator.SumForDayAsync(_db, session, employee.Id, date, token);
            DayHoursLimitRule.ValidateDayTotal(dayTotal, command.Hours);

            var now = DateTime.UtcNow;
            created = new TimeEntry
            {
                EmployeeId = employee.Id,
                ProjectId = project.Id,
                Date = date,
                Hours = command.Hours,
                Comment = command.Comment ?? string.Empty,
                Version = 1,
                CreatedAt = now,
                CreatedBy = "demo",
                UpdatedAt = now,
                UpdatedBy = "demo"
            };

            await _db.TimeEntries.InsertOneAsync(session, created, cancellationToken: token);
        }, ct);

        return await TimeEntryRowProjector.BuildWithLookupsAsync(_db, null, created!, ct);
    }
}


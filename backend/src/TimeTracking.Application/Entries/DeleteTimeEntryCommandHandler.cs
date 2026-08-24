using MediatR;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;
using TimeTracking.Domain;

namespace TimeTracking.Application.Entries;

public class DeleteTimeEntryCommandHandler : IRequestHandler<DeleteTimeEntryCommand, Unit>
{
    private readonly ITimeTrackingDb _db;

    public DeleteTimeEntryCommandHandler(ITimeTrackingDb db) => _db = db;

    public async Task<Unit> Handle(DeleteTimeEntryCommand command, CancellationToken ct)
    {
        await TransactionRunner.RunAsync(_db, async (session, token) =>
        {
            var entry = await _db.TimeEntries.Find(session, e => e.Id == command.Id).FirstOrDefaultAsync(token)
                ?? throw new BusinessRuleException(ErrorCodes.EntryNotFound, "Запись табеля не найдена.", HttpStatus.NotFound);

            var closedPeriods = await _db.ClosedPeriods
                .Find(session, p => p.Year == entry.Date.Year && p.Month == entry.Date.Month).ToListAsync(token);
            ClosedPeriodRule.ThrowIfClosed(closedPeriods, entry.Date);

            await _db.TimeEntries.DeleteOneAsync(session, e => e.Id == command.Id, cancellationToken: token);
        }, ct);

        return Unit.Value;
    }
}


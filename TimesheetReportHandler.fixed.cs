using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;

namespace Demo.Api.Queries.Reports
{
    public class TimesheetReportHandler : IRequestHandler<GetProjectReportQuery, List<ProjectReportRow>>
    {
        private readonly IMongoDatabase _db;

        public TimesheetReportHandler(IMongoDatabase db)
        {
            _db = db;
        }

        public async Task<List<ProjectReportRow>> Handle(GetProjectReportQuery request, CancellationToken token)
        {
            // Фильтр по году и месяцу
            var startDate = new DateTime(request.Year, request.Month, 1);
            var endDate = startDate.AddMonths(1);
            var filter = Builders<TimeEntry>.Filter.And(
                Builders<TimeEntry>.Filter.Gte(e => e.Date, startDate),
                Builders<TimeEntry>.Filter.Lt(e => e.Date, endDate)
            );

            var monthEntries = await _db.GetCollection<TimeEntry>("time_entries")
                .Find(filter, new FindOptions { AllowDiskUse = false })
                .ToListAsync(token);

            if (!monthEntries.Any())
                return new List<ProjectReportRow>();

            // Загружаем всех нужных сотрудников и проекты одним запросом
            var employeeIds = monthEntries.Select(e => e.EmployeeId).Distinct().ToList();
            var projectIds = monthEntries.Select(e => e.ProjectId).Distinct().ToList();

            var employees = await _db.GetCollection<Employee>("employees")
                .Find(Builders<Employee>.Filter.In(e => e.Id, employeeIds))
                .ToListAsync(token);

            var projects = await _db.GetCollection<Project>("projects")
                .Find(Builders<Project>.Filter.In(p => p.Id, projectIds))
                .ToListAsync(token);

            var employeeDict = employees.ToDictionary(e => e.Id);
            var projectDict = projects.ToDictionary(p => p.Id);

            // Агрегация в памяти
            var rows = new Dictionary<string, ProjectReportRow>();

            foreach (var entry in monthEntries)
            {
                if (!employeeDict.TryGetValue(entry.EmployeeId, out var employee))
                    continue;
                if (!projectDict.TryGetValue(entry.ProjectId, out var project))
                    continue;

                // Выбираем ставку, действующую на дату записи
                var rate = employee.Rates
                    .Where(r => r.From <= entry.Date)
                    .OrderByDescending(r => r.From)
                    .FirstOrDefault();

                if (rate == null)
                    continue;

                var amount = Math.Round(entry.Hours * rate.Value, 2, MidpointRounding.AwayFromZero);

                if (!rows.TryGetValue(entry.ProjectId, out var row))
                {
                    row = new ProjectReportRow
                    {
                        ProjectId = project.Id,
                        ProjectName = project.Name,
                        Budget = project.Budget,
                        Hours = 0,
                        Amount = 0
                    };
                    rows[entry.ProjectId] = row;
                }

                row.Hours += entry.Hours;
                row.Amount += amount;
            }

            // Расчёт процентов с защитой от деления на ноль
            foreach (var row in rows.Values)
            {
                row.Percent = row.Budget != 0
                    ? Math.Round(row.Amount / row.Budget * 100, 2, MidpointRounding.AwayFromZero)
                    : 0;
                row.Overspent = row.Percent > 100;
            }

            return rows.Values.OrderBy(r => r.ProjectName).ToList();
        }
    }

    // Остальные классы
}
using MediatR;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Application.Employees;

public class EmployeeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public class GetEmployeesQuery : IRequest<List<EmployeeDto>>
{
}

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, List<EmployeeDto>>
{
    private readonly ITimeTrackingDb _db;

    public GetEmployeesQueryHandler(ITimeTrackingDb db) => _db = db;

    public async Task<List<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken ct)
    {
        var employees = await _db.Employees.Find(Builders<Employee>.Filter.Empty)
            .SortBy(e => e.Name)
            .ToListAsync(ct);

        return employees.Select(e => new EmployeeDto
        {
            Id = e.Id,
            Name = e.Name,
            Department = e.Department
        }).ToList();
    }
}

using FluentValidation;
using MediatR;

namespace TimeTracking.Application.Entries;

public class GetTimeEntriesQuery : IRequest<TimeEntryPageResult>
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string? EmployeeId { get; set; }
    public string? ProjectId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetTimeEntriesQueryValidator : AbstractValidator<GetTimeEntriesQuery>
{
    public GetTimeEntriesQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Год вне допустимого диапазона.");
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Месяц должен быть от 1 до 12.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Номер страницы должен быть больше нуля.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200).WithMessage("Размер страницы должен быть от 1 до 200.");
    }
}

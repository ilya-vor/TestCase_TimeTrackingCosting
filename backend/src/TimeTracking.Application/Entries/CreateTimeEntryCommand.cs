using FluentValidation;
using MediatR;

namespace TimeTracking.Application.Entries;

public class CreateTimeEntryCommand : IRequest<TimeEntryRow>
{
    public string EmployeeId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double Hours { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class CreateTimeEntryCommandValidator : AbstractValidator<CreateTimeEntryCommand>
{
    public CreateTimeEntryCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Сотрудник обязателен.");
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("Проект обязателен.");
        RuleFor(x => x.Hours).GreaterThan(0).WithMessage("Часы должны быть положительным числом.");
        RuleFor(x => x.Date).Must(d => d != default).WithMessage("Дата обязательна.");
    }
}

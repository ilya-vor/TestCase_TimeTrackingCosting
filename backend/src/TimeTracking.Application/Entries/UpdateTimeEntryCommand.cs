using FluentValidation;
using MediatR;

namespace TimeTracking.Application.Entries;

public record UpdateTimeEntryCommand(
    int ExpectedVersion,
    string EmployeeId,
    string ProjectId,
    DateTime Date,
    double Hours,
    string Comment) : IRequest<TimeEntryRow>
{
    public string Id { get; set; } = string.Empty;
}

public class UpdateTimeEntryCommandValidator : AbstractValidator<UpdateTimeEntryCommand>
{
    public UpdateTimeEntryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Идентификатор записи обязателен.");
        RuleFor(x => x.ExpectedVersion).GreaterThanOrEqualTo(1).WithMessage("Версия записи не может быть меньше 1.");
        RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Сотрудник обязателен.");
        RuleFor(x => x.ProjectId).NotEmpty().WithMessage("Проект обязателен.");
        RuleFor(x => x.Date).Must(d => d != default).WithMessage("Дата обязательна.");
    }
}

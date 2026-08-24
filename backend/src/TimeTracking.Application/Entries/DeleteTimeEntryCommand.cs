using FluentValidation;
using MediatR;

namespace TimeTracking.Application.Entries;

public class DeleteTimeEntryCommand : IRequest<Unit>
{
    public string Id { get; set; } = string.Empty;
}

public class DeleteTimeEntryCommandValidator : AbstractValidator<DeleteTimeEntryCommand>
{
    public DeleteTimeEntryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Идентификатор записи обязателен.");
    }
}

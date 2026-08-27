using FluentValidation;
using MediatR;

namespace TimeTracking.Application.Reports;

public class ProjectReportRow
{
    public string ProjectId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Hours { get; set; }
    public decimal Amount { get; set; }
    public decimal Budget { get; set; }

    /// <summary>
    /// Процент освоения бюджета. null, если бюджет равен нулю.
    /// </summary>
    public decimal? Percent { get; set; }
    public bool Overspent { get; set; }
    public bool AtRisk { get; set; }
}

public class GetProjectReportQuery : IRequest<List<ProjectReportRow>>
{
    public int Year { get; set; }
    public int Month { get; set; }
}

public class GetProjectReportQueryValidator : AbstractValidator<GetProjectReportQuery>
{
    public GetProjectReportQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Год вне допустимого диапазона.");
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Месяц должен быть от 1 до 12.");
    }
}

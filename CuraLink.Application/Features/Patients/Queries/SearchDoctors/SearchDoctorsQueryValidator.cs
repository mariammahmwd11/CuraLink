using FluentValidation;

namespace CuraLink.Application.Features.Patients.Queries.SearchDoctors;

public class SearchDoctorsQueryValidator
    : AbstractValidator<SearchDoctorsQuery>
{
    public SearchDoctorsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage(
                "Page size must be between 1 and 50.");

        RuleFor(x => x.DoctorName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.DoctorName));

        RuleFor(x => x.Specialty)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Specialty));

        RuleFor(x => x.Governorate)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Governorate));
    }
}
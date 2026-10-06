using FluentValidation;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.CreatePatient;

public class CreatePatientCommandValidator
    : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^01[0125][0-9]{8}$")
            .WithMessage(
                "Please enter a valid Egyptian phone number.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today)
            .WithMessage(
                "Date of birth must be in the past.");

        RuleFor(x => x.BloodType)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.BloodType));

        RuleFor(x => x.MedicalHistoryNotes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.MedicalHistoryNotes));
    }
}
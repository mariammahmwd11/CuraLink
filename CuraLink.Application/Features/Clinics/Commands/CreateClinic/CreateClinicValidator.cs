using FluentValidation;

namespace CuraLink.Application.Features.Clinics.Commands.CreateClinic
{
    public class CreateClinicCommandValidator
        : AbstractValidator<CreateClinicCommand>
    {
        public CreateClinicCommandValidator()
        {
          
            RuleFor(x => x.ClinicName)
                .NotEmpty()
                .WithMessage("Clinic name is required.")
                .MaximumLength(200)
                .WithMessage("Clinic name cannot exceed 200 characters.");

            RuleFor(x => x.Address)
                .NotEmpty()
                .WithMessage("Address is required.")
                .MaximumLength(500)
                .WithMessage("Address cannot exceed 500 characters.");

            RuleFor(x => x.ConsultationPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Consultation price cannot be negative.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .MaximumLength(20)
                .WithMessage("Phone number cannot exceed 20 characters.");
        }
    }
}
using FluentValidation;

namespace CuraLink.Application.Features.Prescriptions.Commands.CreatePrescription;

public class CreatePrescriptionCommandValidator
    : AbstractValidator<CreatePrescriptionCommand>
{
    public CreatePrescriptionCommandValidator()
    {
        RuleFor(x => x.PatientUserId)
            .NotEmpty()
            .WithMessage("Patient user ID is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage(
                "End date must be greater than or equal to start date.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(
                "Prescription must contain at least one medication.");

        RuleForEach(x => x.Items)
            .SetValidator(new PrescriptionItemValidator());
    }
}

public class PrescriptionItemValidator
    : AbstractValidator<PrescriptionItemRequest>
{
    public PrescriptionItemValidator()
    {
        RuleFor(x => x.MedicationName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Dosage)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Instructions)
            .MaximumLength(500);

        RuleFor(x => x.DosageTimes)
            .NotEmpty()
            .WithMessage("At least one dosage time is required.");

        RuleForEach(x => x.DosageTimes)
            .Must(time =>
                time >= TimeSpan.Zero &&
                time < TimeSpan.FromDays(1))
            .WithMessage(
                "Dosage time must be a valid time of day.");
    }
}
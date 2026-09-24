using FluentValidation;

namespace CuraLink.Application.Features.Doctors.Commands.UpdateDoctorSchedule;

public class UpdateDoctorScheduleCommandValidator
    : AbstractValidator<UpdateDoctorScheduleCommand>
{
    public UpdateDoctorScheduleCommandValidator()
    {
        RuleFor(x => x.Availability)
            .NotNull()
            .WithMessage("Availability is required.");

        RuleForEach(x => x.Availability)
            .SetValidator(new DoctorScheduleItemValidator());
    }

    private class DoctorScheduleItemValidator
        : AbstractValidator<DoctorScheduleItem>
    {
        public DoctorScheduleItemValidator()
        {
            RuleFor(x => x.StartTime)
                .LessThan(x => x.EndTime)
                .WithMessage("Start time must be before end time.");

            RuleFor(x => x.SlotDurationMinutes)
                .GreaterThan(0)
                .WithMessage("Slot duration must be greater than zero.");

            RuleFor(x => x.SlotDurationMinutes)
                .Must((item, duration) =>
                {
                    var shiftDuration =
                        (item.EndTime - item.StartTime).TotalMinutes;

                    return duration <= shiftDuration;
                })
                .WithMessage(
                    "Slot duration cannot be greater than the shift duration.");

            RuleFor(x => x.SlotDurationMinutes)
                .Must((item, duration) =>
                {
                    var shiftDuration =
                        (item.EndTime - item.StartTime).TotalMinutes;

                    return shiftDuration % duration == 0;
                })
                .WithMessage(
                    "Slot duration must divide the shift duration exactly.");
        }
    }
}
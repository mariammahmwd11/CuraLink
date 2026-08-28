using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Admin.Doctors.Commands.VerifyDoctor
{
    public class VerifyDoctorCommandValidator
        : AbstractValidator<VerifyDoctorCommand>
    {
        public VerifyDoctorCommandValidator()
        {
            RuleFor(x => x.DoctorId)
                .NotEmpty()
                .WithMessage("Doctor ID is required.");

            When(x => !x.IsApproved, () =>
            {
                RuleFor(x => x.RejectionReason)
                    .NotEmpty()
                    .WithMessage("Rejection reason is required.")
                    .MaximumLength(500);
            });
        }
    }
}

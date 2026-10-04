using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.InviteClinicAssistant
{
    public class InviteClinicAssistantCommandValidator
     : AbstractValidator<InviteClinicAssistantCommand>
    {
        public InviteClinicAssistantCommandValidator()
        {
            RuleFor(x => x.DoctorUserId)
            .NotEmpty();

            RuleFor(x => x.ClinicId)
                .NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);
        }
    }
}

using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.ClinicAssistants.Commands.AcceptClinicAssistantInvitation
{
    public class AcceptClinicAssistantInvitationCommandValidator
    : AbstractValidator<AcceptClinicAssistantInvitationCommand>
    {
        public AcceptClinicAssistantInvitationCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.Token)
                .NotEmpty();
        }
    }
}

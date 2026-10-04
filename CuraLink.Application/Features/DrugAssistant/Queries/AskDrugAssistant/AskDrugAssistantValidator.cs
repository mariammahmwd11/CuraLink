using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.DrugAssistant.Queries.AskDrugAssistant
{
    public class AskDrugAssistantValidator
    : AbstractValidator<AskDrugAssistantQuery>
    {
        public AskDrugAssistantValidator()
        {
            RuleFor(x => x.Query)
                .NotEmpty()
                .WithMessage("Please enter a medication name or question.")
                .MaximumLength(1000)
                .WithMessage("The query cannot exceed 1000 characters.");
        }
    }
}

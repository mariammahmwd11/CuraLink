using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Authentication.Commands.RegisterDoctor
{
    public class RegisterDoctorCommandValidator
     : AbstractValidator<RegisterDoctorCommand>
    {
        private static readonly string[] AllowedExtensions =
        {
        ".jpg",
        ".jpeg",
        ".png",
        ".pdf"
    };

        private const long MaxFileSize = 5 * 1024 * 1024;//5 mb

        public RegisterDoctorCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty();

            RuleFor(x => x.Specialty)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.SyndicateId)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.LicenseDocument)
                .NotNull()
                .Must(file => file.Length > 0)
                .WithMessage("License document is required.")
                .Must(file =>
                    AllowedExtensions.Contains(
                        Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage("Only JPG, JPEG, PNG, and PDF files are allowed.")
                .Must(file => file.Length <= MaxFileSize)
                .WithMessage("File size must not exceed 5 MB.");
        }
    }
}

using FluentValidation;

namespace CuraLink.Application.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandValidator
    : AbstractValidator<UpdateProfileCommand>
{
    private static readonly string[] AllowedImageTypes =
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private const long MaxFileSize = 5 * 1024 * 1024;

    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));

        When(x => x.ProfilePhoto is not null, () =>
        {
            RuleFor(x => x.ProfilePhoto!.FileName)
                .NotEmpty();

            RuleFor(x => x.ProfilePhoto!.ContentType)
                .Must(type => AllowedImageTypes.Contains(type))
                .WithMessage(
                    "Profile photo must be a JPEG, PNG, or WebP image.");

            RuleFor(x => x.ProfilePhoto!.Content.Length)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage(
                    "Profile photo size must not exceed 5 MB.");
        });
    }
}
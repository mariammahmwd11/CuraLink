using CuraLink.Application.Features.Patients.Commands.UploadMedicalDocument;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace CuraLink.Application.Features.MedicalHistories.Commands.UploadMedicalDocument;

public class UploadMedicalDocumentValidator
    : AbstractValidator<UploadMedicalDocumentCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private static readonly string[] AllowedExtensions =
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png"
    };

    private static readonly string[] AllowedContentTypes =
    {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };

    public UploadMedicalDocumentValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required.");

        RuleFor(x => x.File)
            .Must(file => file == null || file.Length > 0)
            .WithMessage("File cannot be empty.");

        RuleFor(x => x.File)
            .Must(file =>
                file == null ||
                file.Length <= MaxFileSize)
            .WithMessage("File size must not exceed 10 MB.");

        RuleFor(x => x.File)
            .Must(IsAllowedExtension)
            .WithMessage(
                "Only PDF, JPG, JPEG, and PNG files are allowed.");

        RuleFor(x => x.File)
            .Must(IsAllowedContentType)
            .WithMessage("Invalid file type.");
    }

    private static bool IsAllowedExtension(IFormFile? file)
    {
        if (file == null)
            return true;

        var extension = Path.GetExtension(file.FileName);

        return AllowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsAllowedContentType(IFormFile? file)
    {
        if (file == null)
            return true;

        return AllowedContentTypes.Contains(
            file.ContentType,
            StringComparer.OrdinalIgnoreCase);
    }
}
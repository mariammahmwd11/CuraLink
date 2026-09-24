using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;

namespace CuraLink.Application.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandHandler
    : IRequestHandler<UpdateProfileCommand, UpdateProfileResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<UpdateProfileResponse> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        var oldProfilePhoto = user.ProfilePhoto;

        string? newProfilePhoto = null;

        if (request.ProfilePhoto is not null)
        {
            using (request.ProfilePhoto.Content)
            {
                newProfilePhoto = await _fileStorageService.UploadAsync(
                    request.ProfilePhoto.Content,
                    request.ProfilePhoto.FileName,
                    request.ProfilePhoto.ContentType,
                    "profile-images",
                    cancellationToken);
            }
        }

        var updated = await _userRepository.UpdateProfileAsync(
            request.UserId,
            request.PhoneNumber,
            request.Bio,
            newProfilePhoto,
            cancellationToken);

        if (!updated)
            throw new InvalidOperationException(
                "Failed to update profile.");

        if (!string.IsNullOrEmpty(oldProfilePhoto)
            && !string.IsNullOrEmpty(newProfilePhoto))
        {
            await _fileStorageService.DeleteAsync(
                oldProfilePhoto,
                "image",
                cancellationToken);
        }

        var updatedUser = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (updatedUser is null)
            throw new KeyNotFoundException("User not found.");

        string? profilePhotoUrl = null;

        if (!string.IsNullOrEmpty(updatedUser.ProfilePhoto))
        {
            profilePhotoUrl = await _fileStorageService.GetUrlAsync(
                updatedUser.ProfilePhoto,
                "image",
                cancellationToken);
        }

        return new UpdateProfileResponse(
            updatedUser.Id,
            updatedUser.FirstName,
            updatedUser.LastName,
            updatedUser.PhoneNumber,
            updatedUser.Bio,
            profilePhotoUrl);
    }
}
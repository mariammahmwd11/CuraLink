using CuraLink.Application.Common.Interfaces.FileStorage;
using CuraLink.Application.Common.Interfaces.Presistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Features.Profile.Queries.GetProfile
{
    public class GetProfileQueryHandler
    : IRequestHandler<GetProfileQuery, GetProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;

        public GetProfileQueryHandler(
            IUserRepository userRepository,
            IFileStorageService fileStorageService)
        {
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<GetProfileResponse> Handle(
            GetProfileQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(
                request.UserId,
                cancellationToken);

            if (user is null)
                throw new KeyNotFoundException("User not found.");

            string? profilePhotoUrl = null;

            if (!string.IsNullOrEmpty(user.ProfilePhoto))
            {
                profilePhotoUrl = await _fileStorageService.GetUrlAsync(
                    user.ProfilePhoto,
                    "image",
                    cancellationToken);
            }

            return new GetProfileResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.Bio,
                profilePhotoUrl);
        }
    }
}

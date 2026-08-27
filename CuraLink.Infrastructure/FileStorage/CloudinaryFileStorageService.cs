using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CuraLink.Application.Common.Interfaces.FileStorage;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.FileStorage
{
    public class CloudinaryFileStorageService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryFileStorageService(
            IOptions<CloudinarySettings> settings)
        {
            var account = new Account(
                settings.Value.CloudName,
                settings.Value.ApiKey,
                settings.Value.ApiSecret);

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = $"doctors/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception(
                    $"File upload failed: {result.Error.Message}");
            }

            return result.PublicId;
        }
    }
}

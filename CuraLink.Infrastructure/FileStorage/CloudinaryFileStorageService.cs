
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CuraLink.Application.Common.Interfaces.FileStorage;
using Microsoft.Extensions.Options;

namespace CuraLink.Infrastructure.FileStorage;

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
        string folder,
        CancellationToken cancellationToken = default)
    {
        var publicId =
            $"{folder}/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}";

        // Images are uploaded as public assets
        // so they can be displayed directly in <img src="...">
        if (contentType.StartsWith("image/"))
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = publicId,
                Type = "upload"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception(
                    $"Image upload failed: {result.Error.Message}");
            }

            return result.PublicId;
        }

        // Other files remain authenticated
        var rawUploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            PublicId = publicId,
            Type = "authenticated"
        };

        var rawResult = await _cloudinary.UploadAsync(rawUploadParams);

        if (rawResult.Error != null)
        {
            throw new Exception(
                $"File upload failed: {rawResult.Error.Message}");
        }

        return rawResult.PublicId;
    }

    public Task<string> GetUrlAsync(
        string storageKey,
        string resourceType = "raw",
        CancellationToken cancellationToken = default)
    {
        var isImage = resourceType.Equals(
            "image",
            StringComparison.OrdinalIgnoreCase);

        var url = _cloudinary.Api.Url
            .ResourceType(resourceType)
            .Type(isImage ? "upload" : "authenticated")
            .Secure(true)
            .BuildUrl(storageKey);

        url = url.Replace("http://", "https://");

        return Task.FromResult(url);
    }

    public async Task<Stream> DownloadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var url = _cloudinary.Api.Url
            .ResourceType("raw")
            .Type("authenticated")
            .Secure(true)
            .Signed(true)
            .BuildUrl(storageKey);

        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(
            url,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"File download failed. Status code: {response.StatusCode}");
        }

        var memoryStream = new MemoryStream();

        await response.Content.CopyToAsync(
            memoryStream,
            cancellationToken);

        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task DeleteAsync(
        string storageKey,
        string resourceType = "raw",
        CancellationToken cancellationToken = default)
    {
        var isImage = resourceType.Equals(
            "image",
            StringComparison.OrdinalIgnoreCase);

        var resource = isImage
            ? CloudinaryDotNet.Actions.ResourceType.Image
            : CloudinaryDotNet.Actions.ResourceType.Raw;

        var deletionParams = new DeletionParams(storageKey)
        {
            ResourceType = resource,
            Type = isImage ? "upload" : "authenticated"
        };

        var result = await _cloudinary.DestroyAsync(deletionParams);

        if (result.Error != null)
        {
            throw new Exception(
                $"File deletion failed: {result.Error.Message}");
        }
    }
}


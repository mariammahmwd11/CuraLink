using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Application.Common.Interfaces.FileStorage
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(
       Stream fileStream,
       string fileName,
       string contentType,
       CancellationToken cancellationToken = default);

        Task<string> GetUrlAsync(
          string storageKey,
          CancellationToken cancellationToken = default);
        Task<Stream> DownloadAsync(
   string storageKey,
   CancellationToken cancellationToken = default);

    }
}

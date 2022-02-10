using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace dotnet_core_api_storage.Helpers
{
    public static class StorageHelper
    {
        public static async Task<(bool uploadSuccess, string uploadedUri)> UploadFileToStorage(Stream stream, string containerName, string fileName, BlobServiceClient serviceClient)
        {
            BlobClient blobClient = null;

            if (stream is not null)
            {
                try
                {
                    // Get a reference to the blob container
                    BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(containerName);

                    // Create a unique name for the blob
                    fileName = Guid.NewGuid().ToString() + fileName;
                    // Get a reference to a blob
                    blobClient = containerClient.GetBlobClient(fileName);
                    await blobClient.UploadAsync(stream, true);
                }
                catch (Exception)
                {
                    return (false, null);
                }
            }
            return (true, blobClient.Uri.ToString());
        }
    }
}
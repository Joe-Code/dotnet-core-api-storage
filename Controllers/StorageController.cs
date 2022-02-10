using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using dotnet_core_api_storage.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace dotnet_core_api_storage.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private BlobServiceClient _blobServiceClient;
        private readonly ILogger<StorageController> _logger;

        public StorageController(ILogger<StorageController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _blobServiceClient = new BlobServiceClient(_configuration["StorageConnectionString"]);
        }

        [HttpPost("CreateContainer")]
        public async Task<IActionResult> CreateContainer(string containerName)
        {
            if (containerName == string.Empty)
                return BadRequest("Please provide a name for your image container in the azure blob storage");

            // Create the container and return a container client object
            BlobContainerClient containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);

            return Ok($"Container has been successfully created: {containerName}");
        }

        [HttpGet("GetContainerBlobs")]
        public async Task<IActionResult> GetContainerBlobs(string containerName)
        {
            try
            {
                if (containerName == string.Empty)
                    return BadRequest("Please provide a name for your container in Azure blob storage.");

                // Get reference to the container
                BlobContainerClient container = _blobServiceClient.GetBlobContainerClient(containerName);

                List<string> blobUrls = new List<string>();
                if (container.Exists())
                {
                    await foreach (BlobItem blobItem in container.GetBlobsAsync())
                    {
                        blobUrls.Add(container.Uri + "/" + blobItem.Name);
                    }
                }
                return new ObjectResult(blobUrls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UploadFiles")]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files, string containerName)
        {
            bool uploadSuccess = false;
            string uploadedUri = null;

            try
            {
                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        // read directly from stream for blob upload      
                        using (Stream stream = formFile.OpenReadStream())
                        {
                            (uploadSuccess, uploadedUri) = await StorageHelper.UploadFileToStorage(stream, containerName, formFile.FileName, _blobServiceClient);
                        }
                    }
                }

                if (uploadSuccess)
                {
                    return Ok($"{uploadedUri} has been successfully uploaded to storage.");
                }
                else
                {
                    //create an error component to show there was some error while uploading
                    return BadRequest("There was an error uploading the file to storage.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

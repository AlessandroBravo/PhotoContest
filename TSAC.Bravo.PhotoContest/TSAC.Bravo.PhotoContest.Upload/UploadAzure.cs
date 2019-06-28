using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;

namespace TSAC.Bravo.PhotoContest.Upload
{
    public class UploadAzure : IUploadLibrary
    {
        private readonly IConfiguration _configuration;

        public UploadAzure(IConfiguration config)
        {
            _configuration = config;
        }

        public string GetCdn()
        {
            return "";
        }

        public async Task Upload(Stream stream, string fileName)
        {
            var storageCredentials = new StorageCredentials(_configuration["AzureStorage:User"], _configuration["AzureStorage:Key"]);
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, true);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("images");

            //create a container if it is not already exists
            if (await cloudBlobContainer.CreateIfNotExistsAsync())
            {
                await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }

            //get Blob reference

            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

            await cloudBlockBlob.UploadFromStreamAsync(stream);

        }
    }
}

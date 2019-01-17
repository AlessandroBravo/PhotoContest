using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TSAC.Bravo.PhotoContest.Upload
{
    public class UploadHelper : IUploadHelper
    {
        private readonly IAmazonS3 _client;
        private readonly IConfiguration _config;

        public UploadHelper(IConfiguration config)
        {
            _client = new AmazonS3Client(config["Amazon:aws_access_key_id"], config["Amazon:aws_secret_access_key"], Amazon.RegionEndpoint.EUWest1);
            _config = config;
        }

        public async Task UploadToS3(Stream stream, string fileName)
        {
            string cdn = _config["CDN"];
            string bucketName = _config["bucketName"];

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = bucketName,
                CannedACL = S3CannedACL.PublicRead
            };
            var fileTransferUtility = new TransferUtility(_client);
            await fileTransferUtility.UploadAsync(uploadRequest);
        }
    }
}

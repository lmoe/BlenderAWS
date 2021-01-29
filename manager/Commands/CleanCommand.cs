using Amazon.S3;
using Amazon.S3.Transfer;
using BlenderAWS.Interfaces;
using CliFx;
using CliFx.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlenderAWS.Commands
{
    [Command("clean")]
    public class CleanCommand : ICommand
    {
        private readonly ISetupService setupService;
        private readonly IAWSLocatorService awsLocatorService;
        private readonly IJobService jobService;

        public CleanCommand(ISetupService setupService, IAWSLocatorService awsLocatorService, IJobService jobService)
        {
            this.setupService = setupService;
            this.awsLocatorService = awsLocatorService;
            this.jobService = jobService;
        }

        private async Task CleanBucket(AmazonS3Client s3Client, string bucketId)
        {
            var files = await s3Client.ListObjectsAsync(bucketId);

            foreach (var file in files.S3Objects)
            {
                await s3Client.DeleteObjectAsync(new Amazon.S3.Model.DeleteObjectRequest { BucketName = files.Name, Key = file.Key });   
            }
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            var config = await this.setupService.Get();
            var credentials = this.awsLocatorService.GetCredentials(config.ProfileName);

            var s3Client = new AmazonS3Client(credentials);

            await this.CleanBucket(s3Client, config.S3InputId);
            await this.CleanBucket(s3Client, config.S3OutputId);            
        }
    }
}

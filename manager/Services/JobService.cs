using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Batch;
using BlenderAWS.Contracts;
using BlenderAWS.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Batch.Model;

namespace BlenderAWS.Services
{
    public class JobService : IJobService
    {
        private readonly IAWSLocatorService awsLocatorService;
        private readonly ISetupService setupService;

        public JobService(IAWSLocatorService awsLocatorService, ISetupService setupService)
        {
            this.awsLocatorService = awsLocatorService;
            this.setupService = setupService;
        }

        private async Task UploadToS3(AWSCredentials credentials, RemoteCredentials config, string jobName, byte[] jobFile)
        {
            var s3Client = new AmazonS3Client(credentials);
            var s3 = new TransferUtility(s3Client);

            await s3.UploadAsync(new MemoryStream(jobFile), config.S3InputId, jobName + ".zip");
        }

        private async Task<SubmitJobResponse> SubmitRenderingJob(AWSCredentials credentials, RemoteCredentials config, string jobName, string bakeJobId)
        {
            var batchClient = new AmazonBatchClient(credentials);
            var request = new SubmitJobRequest
            {
                JobDefinition = config.RenderJobDefinitionId,
                JobName = jobName,
                JobQueue = config.RenderJobQueueId,
                Parameters = new Dictionary<string, string>
                {
                    { "job", jobName }
                },
                DependsOn = new List<JobDependency>()
            };

            if (!string.IsNullOrEmpty(bakeJobId))
            {
                request.DependsOn.Add(new JobDependency { 
                    JobId = bakeJobId,
                    Type = ArrayJobDependency.N_TO_N
                });
            }

            var response = await batchClient.SubmitJobAsync(request);

            return response;
        }

        private async Task<SubmitJobResponse> SubmitBakeJob(AWSCredentials credentials, RemoteCredentials config, string jobName)
        {
            var batchClient = new AmazonBatchClient(credentials);

            var response = await batchClient.SubmitJobAsync(new SubmitJobRequest
            {
                JobDefinition = config.BakeJobDefinitionId,
                JobName = jobName,
                JobQueue = config.BakeJobQueueId,
                
                Parameters = new Dictionary<string, string>
                {
                    { "job", jobName }
                },
                
                ContainerOverrides = new ContainerOverrides()
                {/*
                    ResourceRequirements = new List<ResourceRequirement> { 
                        new ResourceRequirement()
                        {
                            Type = ResourceType.VCPU,
                            Value = "36"
                        },
                        new ResourceRequirement()
                        {
                            Type = ResourceType.MEMORY,
                            Value = "70000"
                        }
                    },*/

                    Vcpus = 36,
                    Memory = 70000
                }
            });

            

            return response;
        }

        public async Task SubmitJob(string jobName, byte[] jobFile)
        {
            var config = await this.setupService.Get();
            var credentials = this.awsLocatorService.GetCredentials(config.ProfileName);

            await this.UploadToS3(credentials, config, jobName, jobFile);

            var bakeJob = await this.SubmitBakeJob(credentials, config, jobName);
            await this.SubmitRenderingJob(credentials, config, jobName, bakeJob.JobId);
        }

        public async Task GetJobStatus()
        {
            var config = await this.setupService.Get();
            var credentials = this.awsLocatorService.GetCredentials(config.ProfileName);
            var batchClient = new AmazonBatchClient(credentials);

            await batchClient.ListJobsAsync(new ListJobsRequest
            {
                JobQueue = config.RenderJobQueueId
            });
        }
    }
}

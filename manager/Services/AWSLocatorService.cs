using Amazon.CloudFormation.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using BlenderAWS.Contracts;
using BlenderAWS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlenderAWS.Services
{
    public class AWSLocatorService : IAWSLocatorService
    {
        public async Task<RemoteCredentials> Locate(string profile, string stackName)
        {
            var credentials = this.GetCredentials(profile);

            var cloudClient = new Amazon.CloudFormation.AmazonCloudFormationClient(credentials);
            var stacks = await cloudClient.DescribeStacksAsync(new DescribeStacksRequest { StackName = stackName });
            var stack = stacks.Stacks.FirstOrDefault();

            if (stack != default(Stack))
            {
                return new RemoteCredentials
                {
                    ProfileName = profile,
                    StackName = stackName,
                    S3InputId = stack.Outputs.First(x => x.OutputKey == "SourceBucket").OutputValue,
                    S3OutputId = stack.Outputs.First(x => x.OutputKey == "ResultsBucket").OutputValue,
                    RenderJobDefinitionId = stack.Outputs.First(x => x.OutputKey == "RenderJobDefinition").OutputValue,
                    RenderJobQueueId = stack.Outputs.First(x => x.OutputKey == "RenderJobQueue").OutputValue,
                    BakeJobDefinitionId = stack.Outputs.First(x => x.OutputKey == "BakeJobDefinition").OutputValue,
                    BakeJobQueueId = stack.Outputs.First(x => x.OutputKey == "BakeJobQueue").OutputValue,
                };
            }

            throw new Exception("Could not find stack!");
        }

        public AWSCredentials GetCredentials(string profile)
        {
            var chain = new CredentialProfileStoreChain();

            AWSCredentials awsCredentials;
            if (!chain.TryGetAWSCredentials(profile, out awsCredentials))
            {
                throw new Exception($"Could not fetch aws credentials for profile '{profile}'");
            }

            return awsCredentials;
        }
    }
}

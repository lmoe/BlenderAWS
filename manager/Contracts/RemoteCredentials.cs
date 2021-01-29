using System;
using System.Collections.Generic;
using System.Text;

namespace BlenderAWS.Contracts
{
    public class RemoteCredentials
    {
        public string StackName { get; set; }
        public string ProfileName { get; set; }
        public string S3InputId { get; set; }
        public string S3OutputId { get; set; }        
        public string BakeJobQueueId { get; set; }
        public string BakeJobDefinitionId { get; set; }
        public string RenderJobDefinitionId { get; set; }
        public string RenderJobQueueId { get; set; }
    }
}

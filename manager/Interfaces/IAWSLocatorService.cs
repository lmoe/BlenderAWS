using Amazon.Runtime;
using BlenderAWS.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlenderAWS.Interfaces
{
    public interface IAWSLocatorService
    {
        Task<RemoteCredentials> Locate(string profile, string stackName);
        AWSCredentials GetCredentials(string profile);
    }
}

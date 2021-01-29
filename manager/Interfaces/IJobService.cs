using Amazon.Runtime;
using BlenderAWS.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlenderAWS.Interfaces
{
    public interface IJobService
    {
        public Task SubmitJob(string jobName, byte[] jobFile);   
    }
}

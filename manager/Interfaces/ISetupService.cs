using BlenderAWS.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlenderAWS.Interfaces
{
    public interface ISetupService
    {
        public Task<RemoteCredentials> Get();
        public Task Save(RemoteCredentials remoteCredentials);
    }
}

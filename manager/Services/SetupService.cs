using BlenderAWS.Contracts;
using BlenderAWS.Exceptions;
using BlenderAWS.Interfaces;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using static System.Environment;

namespace BlenderAWS.Services
{
    public class SetupService : ISetupService
    {
        public async Task<RemoteCredentials> Get()
        {
            var configPath = Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify), "BlenderAWS");
            var configFilePath = Path.Combine(configPath, "config.json");

            if (!File.Exists(configFilePath))
            {
                throw new RemoteConfigNotFoundException("Can't find config file. Run 'BlenderAWS configure'");
            }

            var config = await File.ReadAllTextAsync(configFilePath);
            var configObject = JsonConvert.DeserializeObject<RemoteCredentials>(config);

            return configObject;
        }

        public async Task Save(RemoteCredentials remoteCredentials)
        {
            var configPath = Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify), "BlenderAWS");
            var configFilePath = Path.Combine(configPath, "config.json");

            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }

            var configString = JsonConvert.SerializeObject(remoteCredentials, Formatting.Indented);

            await File.WriteAllTextAsync(configFilePath, configString);
        }
    }
}

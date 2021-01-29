using BlenderAWS.Interfaces;
using CliFx;
using CliFx.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlenderAWS.Commands
{
    [Command("configure")]
    public class ConfigureCommand : ICommand
    {
        private readonly ISetupService setupService;
        private readonly IAWSLocatorService awsLocatorService;
        
        public ConfigureCommand(ISetupService setupService, IAWSLocatorService awsLocatorService)
        {
            this.setupService = setupService;
            this.awsLocatorService = awsLocatorService;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine("Enter aws profile name. [default]");
            var profileName = console.Input.ReadLine();

            if (string.IsNullOrEmpty(profileName))
            {
                profileName = "default";
            }

            console.Output.WriteLine("Enter stack name. [render-stack]");
            var stackName = console.Input.ReadLine();

            if (string.IsNullOrEmpty(stackName))
            {
                stackName = "render-stack";
            }

            var awsConfig = await this.awsLocatorService.Locate(profileName, stackName);

            await this.setupService.Save(awsConfig);

            console.Output.WriteLine("Configuration done!");
        }
    }
}

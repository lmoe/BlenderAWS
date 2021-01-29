using Amazon.CloudFormation.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using BlenderAWS.Commands;
using BlenderAWS.Interfaces;
using BlenderAWS.Services;
using CliFx;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using static System.Environment;

namespace BlenderAWS
{
    class Program
    {
        public static async Task<int> Main()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IAWSLocatorService, AWSLocatorService>();
            services.AddSingleton<ISetupService, SetupService>();
            services.AddSingleton<IJobService, JobService>();

            services.AddTransient<ConfigureCommand>();
            services.AddTransient<PushCommand>();
            services.AddTransient<CleanCommand>();

            var serviceProvider = services.BuildServiceProvider();

            var result = await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(serviceProvider.GetService)
                .Build()
                .RunAsync();

            return result;
        }
    }
}

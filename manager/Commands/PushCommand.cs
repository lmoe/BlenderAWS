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
    [Command("push")]
    public class PushCommand : ICommand
    {
        [CommandOption("blend", 'b', Description = "Blenderfile", IsRequired = true)]
        public string BlenderFile { get; set; }

        [CommandOption("manifest", 'm', Description = "Manifest file")]
        public string ManifestFile { get; set; }


        private readonly ISetupService setupService;
        private readonly IAWSLocatorService awsLocatorService;
        private readonly IJobService jobService;

        public PushCommand(ISetupService setupService, IAWSLocatorService awsLocatorService, IJobService jobService)
        {
            this.setupService = setupService;
            this.awsLocatorService = awsLocatorService;
            this.jobService = jobService;
        }

        private string FindManifest()
        {
            var realManifestFile = string.Empty;

            if (File.Exists(this.ManifestFile))
            {
                realManifestFile = this.ManifestFile;
            }
            else
            {
                var blenderFileName = Path.GetFileNameWithoutExtension(this.BlenderFile);
                var blenderFileRootPath = Path.GetDirectoryName(this.BlenderFile);

                var constructedManifestFile = Path.Combine(blenderFileRootPath, blenderFileName + ".json");

                if (File.Exists(constructedManifestFile))
                {
                    realManifestFile = constructedManifestFile;
                }
                else
                {
                    var rootPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    var combinedPath = Path.Combine(rootPath, "manifest.stub.json");

                    realManifestFile = combinedPath;
                }
            }

            return realManifestFile;
        }

        private byte[] CreateJobFile(string blendFile, string manifestFile)
        {
            byte[] zipFile;

            using (var packageStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(packageStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntryFromFile(blendFile, "job.blend");
                    archive.CreateEntryFromFile(manifestFile, "manifest.json");
                }
                
                zipFile = packageStream.ToArray();
            }

            return zipFile;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (File.Exists(this.BlenderFile))
            {
                var manifestFile = this.FindManifest();

                if (string.IsNullOrEmpty(manifestFile))
                {
                    throw new Exception("Could not find manifest file");
                }

                var blenderFileName = Path.GetFileNameWithoutExtension(this.BlenderFile);
                var jobName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture) + $"_{ blenderFileName}";
                var jobFile = this.CreateJobFile(this.BlenderFile, manifestFile);

                await this.jobService.SubmitJob(jobName, jobFile);
            }
            else
            {
                // Todo
            }
        }
    }
}

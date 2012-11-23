using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace PAppsManager.Core.PApps.Commands
{
    internal class ExecuteCommand : Command
    {
        public ExecuteCommand()
        {
            FailOnError = true;
        }

        [JsonProperty("file")]
        public string FileName { get; set; }

        public string Arguments { get; set; }

        [JsonProperty("fail_on_error")]
        public bool FailOnError { get; set; }

        public override string Validate()
        {
            return ValidateRelativePath(() => FileName);
        }

        public override void Execute(DirectoryInfo targetDirectory, PortableEnvironment portableEnvironment)
        {
            var psi = new ProcessStartInfo(FileName, Arguments)
                {
                    WorkingDirectory = targetDirectory.FullName,
                };

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
                if (FailOnError && process.ExitCode != 0)
                    throw new CommandException(string.Format("External program {0} returned exit code {1}", FileName,
                                                            process.ExitCode));
            }
        }
    }
}
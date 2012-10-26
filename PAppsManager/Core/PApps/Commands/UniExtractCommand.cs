using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace PAppsManager.Core.PApps.Commands
{
    public class UniExtractCommand : Command
    {
        public UniExtractCommand()
        {
            ToDirectory = ".";
        }

        [JsonProperty("file")]
        public string FileName { get; set; }

        [JsonProperty("to_directory")]
        public string ToDirectory { get; set; }

        private static FileInfo UniExtractExe
        {
            get
            {
                var uniExtract = new FileInfo(Path.Combine(ExeDirectory, @"..\UniExtract\UniExtract.exe"));
                return uniExtract;
            }
        }

        public override string Validate()
        {
            if (!UniExtractExe.Exists)
                return "UniExtract could not be found in: " + UniExtractExe.FullName;

            return ValidateRelativePath(FileName) ??
                   ValidateNotEndingByEscape(FileName) ??
                   ValidateRelativePath(ToDirectory, true) ??
                   ValidateNotEndingByEscape(ToDirectory);
        }

        private static string ValidateNotEndingByEscape(string value)
        {
            if (value.EndsWith(@"\"))
                return @"Value should not end with '\'.";
            return null;
        }

        public override void Execute(DirectoryInfo targetDirectory)
        {
            var file = new FileInfo(Path.Combine(targetDirectory.FullName, FileName));
            var toDirectory = new DirectoryInfo(Path.Combine(targetDirectory.FullName, ToDirectory));

            if (!file.Exists)
                throw new FileNotFoundException("Couldn't file the file to extract.", FileName);

            var psi = new ProcessStartInfo(UniExtractExe.FullName,
                                           string.Format("\"{0}\" \"{1}\"", file.FullName, toDirectory.FullName))
                {
                    WorkingDirectory = targetDirectory.FullName,
                };

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new CommandException(string.Format("7-Zip returned exit code {1}", process.ExitCode));
            }
        }
    }
}
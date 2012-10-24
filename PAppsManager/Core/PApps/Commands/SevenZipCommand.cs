using System.Diagnostics;
using System.IO;

namespace PAppsManager.Core.PApps.Commands
{
    public class SevenZipCommand : Command
    {
        public string Arguments { get; set; }

        public override string Validate()
        {
            if (string.IsNullOrWhiteSpace(Arguments))
                return "Arguments is not defined.";

            return null;
        }

        public override void Execute(DirectoryInfo targetDirectory)
        {
            var sevenZipExe = Path.Combine(ExeDirectory, @"7-Zip\x86\7z.exe");

            var psi = new ProcessStartInfo(sevenZipExe, Arguments)
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
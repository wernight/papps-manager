﻿using System.Diagnostics;

namespace PAppsManager.Core.PApps.Commands
{
    public class ExecuteCommand : Command
    {
        public ExecuteCommand()
        {
            FailOnError = true;
        }

        public string FileName { get; set; }

        public string Arguments { get; set; }

        public bool FailOnError { get; set; }

        public override string Validate()
        {
            if (string.IsNullOrWhiteSpace(FileName))
                return "FileName is not defined.";

            return null;
        }

        public override void Execute()
        {
            using (Process process = Process.Start(FileName, Arguments))
            {
                if (process == null)
                    throw new CommandException("Failed to start a new process.");

                process.WaitForExit();
                if (FailOnError && process.ExitCode != 0)
                    throw new CommandException(string.Format("External program {0} returned exit code {1}", FileName,
                                                            process.ExitCode));
            }
        }
    }
}
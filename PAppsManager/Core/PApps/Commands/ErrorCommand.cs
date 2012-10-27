using System.IO;

namespace PAppsManager.Core.PApps.Commands
{
    /// <summary>
    /// Display an error message (should be the first command) and then does nothing else.
    /// </summary>
    internal class ErrorCommand : Command
    {
        public string Message { get; set; }

        public override string Validate()
        {
            return null;
        }

        public override void Execute(DirectoryInfo targetDirectory, PortableEnvironment portableEnvironment)
        {
            if (string.IsNullOrEmpty(Message))
                throw new CommandException("Undefined error message.");

            throw new CommandException(Message);
        }

        public override string ToString()
        {
            return "Error: " + Message;
        }
    }
}